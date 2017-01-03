using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace AForge.Video
{
    public class DesktopStream : IVideoSource
    {
        // URL for JPEG files
        // recieved byte count
        public int ResizeHeight = 60;
        public int ResizeWidth = 100;
        private int _bytesReceived;

        private int _frameInterval;
        private int _framesReceived;
        private int _screenindex;
        private ManualResetEvent _stopEvent;
        private Thread _thread;

        public DesktopStream()
        {
            _screenindex = 0;
        }


        public DesktopStream(int screenindex)
        {
            _screenindex = screenindex;
        }

        public int FrameInterval
        {
            get { return _frameInterval; }
            set { _frameInterval = value; }
        }

        #region IVideoSource Members

        public event NewFrameEventHandler NewFrame;


        public event VideoSourceErrorEventHandler VideoSourceError;


        public event PlayingFinishedEventHandler PlayingFinished;


        public int BytesReceived
        {
            get
            {
                int bytes = _bytesReceived;
                _bytesReceived = 0;
                return bytes;
            }
        }


        public virtual string Source
        {
            get { return _screenindex.ToString(); }
            set { _screenindex = Convert.ToInt32(value); }
        }


        public int FramesReceived
        {
            get
            {
                int frames = _framesReceived;
                _framesReceived = 0;
                return frames;
            }
        }


        public bool IsRunning
        {
            get
            {
                if (_thread != null)
                {
                    // check thread status
                    if (_thread.Join(0) == false)
                        return true;

                    // the thread is not running, free resources
                    Free();
                }
                return false;
            }
        }


        public void Start()
        {
            if (IsRunning) return;
            _framesReceived = 0;
            _bytesReceived = 0;

            // create events
            _stopEvent = new ManualResetEvent(false);

            // create and start new thread
            _thread = new Thread(WorkerThread) {Name = "desktop" + _screenindex};
            _thread.Start();
        }


        public void SignalToStop()
        {
            // stop thread
            if (_thread != null)
            {
                // signal to stop
                _stopEvent.Set();
            }
        }


        public void WaitForStop()
        {
            if (_thread != null)
            {
                // wait for thread stop
                _thread.Join();

                Free();
            }
        }


        public void Stop()
        {
            if (IsRunning)
            {
                _stopEvent.Set();
                _thread.Abort();
                WaitForStop();
            }
        }

        #endregion

        /// <summary>
        /// Free resource.
        /// </summary>
        /// 
        private void Free()
        {
            _thread = null;

            // release events
            _stopEvent.Close();
            _stopEvent = null;
        }

        // Worker thread
        private void WorkerThread()
        {
            TimeSpan span;

            while (true)
            {
                try
                {
                    DateTime start = DateTime.Now;
                    if (!_stopEvent.WaitOne(0, true))
                    {
                        // increment frames counter
                        _framesReceived++;

                        // provide new image to clients
                        if (NewFrame != null)
                        {
                            Rectangle screenSize = Screen.AllScreens[_screenindex].Bounds;
                            var target = new Bitmap(screenSize.Width, screenSize.Height, PixelFormat.Format24bppRgb);
                            using (Graphics g = Graphics.FromImage(target))
                            {
                                g.CopyFromScreen(Screen.AllScreens[_screenindex].Bounds.X,
                                                 Screen.AllScreens[_screenindex].Bounds.Y, 0, 0,
                                                 new Size(screenSize.Width, screenSize.Height));
                            }
                            var result = new Bitmap(ResizeWidth, ResizeHeight, PixelFormat.Format24bppRgb);
                            using (Graphics g = Graphics.FromImage(result))
                                g.DrawImage(target, 0, 0, result.Width, result.Height);
                            target.Dispose();

                            // notify client
                            NewFrame(this, new NewFrameEventArgs(result));
                            // release the image
                            result.Dispose();
                        }
                    }

                    // wait for a while ?
                    if (_frameInterval > 0)
                    {
                        // get download duration
                        span = DateTime.Now.Subtract(start);
                        // miliseconds to sleep
                        int msec = _frameInterval - (int) span.TotalMilliseconds;

                        while ((msec > 0) && (_stopEvent.WaitOne(0, true) == false))
                        {
                            // sleeping ...
                            Thread.Sleep((msec < 100) ? msec : 100);
                            msec -= 100;
                        }
                    }
                }
                catch (WebException exception)
                {
                    // provide information to clients
                    if (VideoSourceError != null)
                    {
                        VideoSourceError(this, new VideoSourceErrorEventArgs(exception.Message));
                    }
                    // wait for a while before the next try
                    Thread.Sleep(250);
                }
                catch
                {
                }
                
                // need to stop ?
                if (_stopEvent.WaitOne(0, true))
                    break;
            }

            if (PlayingFinished != null)
            {
                PlayingFinished(this, ReasonToFinishPlaying.StoppedByUser);
            }
        }
    }
}