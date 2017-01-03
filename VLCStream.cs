// AForge Video Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © AForge.NET, 2005-2011
// contacts@aforgenet.com
//

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Timers;
using Declarations;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using iSpyApplication;
using Timer = System.Timers.Timer;

namespace AForge.Video
{
    // for registry access

    /// <summary>
    /// MJPEG video source.
    /// </summary>
    /// 
    /// <remarks><para>The video source downloads JPEG images from the specified URL, which represents
    /// MJPEG stream.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create MJPEG video source
    /// MJPEGStream stream = new MJPEGStream( "some url" );
    /// // set event handlers
    /// stream.NewFrame += new NewFrameEventHandler( video_NewFrame );
    /// // start the video source
    /// stream.Start( );
    /// // ...
    /// </code>
    /// 
    /// <para><note>Some cameras produce HTTP header, which does not conform strictly to
    /// standard, what leads to .NET exception. To avoid this exception the <b>useUnsafeHeaderParsing</b>
    /// configuration option of <b>httpWebRequest</b> should be set, what may be done using application
    /// configuration file.</note></para>
    /// <code>
    /// &lt;configuration&gt;
    /// 	&lt;system.net&gt;
    /// 		&lt;settings&gt;
    /// 			&lt;httpWebRequest useUnsafeHeaderParsing="true" /&gt;
    /// 		&lt;/settings&gt;
    /// 	&lt;/system.net&gt;
    /// &lt;/configuration&gt;
    /// </code>
    /// </remarks>
    /// 
    public class VlcStream : IVideoSource
    {
        public int ResizeHeight = 60;
        public int ResizeWidth = 100;
        private bool _isrunning;
        public bool Restart;
        public Timer RetryTimer;
        public string[] Arguments;
        private int _framesReceived;
        private IMediaPlayerFactory _mFactory;
        private IMedia _mMedia;
        private IVideoPlayer _mPlayer;
        private IMemoryRenderer _memRender;


        // URL for MJPEG stream
        private string _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="MJPEGStream"/> class.
        /// </summary>
        /// 
        public VlcStream()
        {
            throw new Exception("Not Implemented");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MJPEGStream"/> class.
        /// </summary>
        /// 
        /// <param name="source">URL, which provides MJPEG stream.</param>
        /// <param name="arguments"></param>
        public VlcStream(string source, string[] arguments)
        {
            _source = source;
            Arguments = arguments;
        }

        #region IVideoSource Members

        /// <summary>
        /// New frame event.
        /// </summary>
        /// 
        /// <remarks><para>Notifies clients about new available frame from video source.</para>
        /// 
        /// <para><note>Since video source may have multiple clients, each client is responsible for
        /// making a copy (cloning) of the passed video frame, because the video source disposes its
        /// own original copy after notifying of clients.</note></para>
        /// </remarks>
        /// 
        public event NewFrameEventHandler NewFrame;

        /// <summary>
        /// Video source error event.
        /// </summary>
        /// 
        /// <remarks>This event is used to notify clients about any type of errors occurred in
        /// video source object, for example internal exceptions.</remarks>
        /// 
        public event VideoSourceErrorEventHandler VideoSourceError;

        /// <summary>
        /// Video playing finished event.
        /// </summary>
        /// 
        /// <remarks><para>This event is used to notify clients that the video playing has finished.</para>
        /// </remarks>
        /// 
        public event PlayingFinishedEventHandler PlayingFinished;

        /// <summary>
        /// Video source.
        /// </summary>
        /// 
        /// <remarks>URL, which provides MJPEG stream.</remarks>
        /// 
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// Received bytes count.
        /// </summary>
        /// 
        /// <remarks>Number of bytes the video source provided from the moment of the last
        /// access to the property.
        /// </remarks>
        /// 
        public int BytesReceived
        {
            get
            {
                int bytes = 0;
                //bytesReceived = 0;
                return bytes;
            }
        }

        /// <summary>
        /// Received frames count.
        /// </summary>
        /// 
        /// <remarks>Number of frames the video source provided from the moment of the last
        /// access to the property.
        /// </remarks>
        /// 
        public int FramesReceived
        {
            get
            {
                int frames = _framesReceived;
                _framesReceived = 0;
                return frames;
            }
        }

        /// <summary>
        /// State of the video source.
        /// </summary>
        /// 
        /// <remarks>Current state of video source object - running or not.</remarks>
        /// 
        public bool IsRunning
        {
            get { return _isrunning; }
        }

        private bool _starting;
        /// <summary>
        /// Start video source.
        /// </summary>
        /// 
        /// <remarks>Starts video source and return execution to caller. Video source
        /// object creates background thread and notifies about new frames with the
        /// help of <see cref="NewFrame"/> event.</remarks>
        /// 
        /// <exception cref="ArgumentException">Video source is not specified.</exception>
        /// 
        public void Start()
        {
            if (!IsRunning && !_starting)
            {
                _starting = true;
                // check source
                if (string.IsNullOrEmpty(_source))
                    throw new ArgumentException("Video source is not specified.");

                _mFactory = new MediaPlayerFactory();

                _mPlayer = _mFactory.CreatePlayer<IVideoPlayer>();
                _mPlayer.Events.PlayerPlaying += EventsPlayerPlaying;
                _mPlayer.Events.PlayerStopped += EventsPlayerStopped;
                _mPlayer.Events.PlayerEncounteredError += EventsPlayerEncounteredError;

                _mMedia = _mFactory.CreateMedia<IMedia>(_source, Arguments);
                _mPlayer.Open(_mMedia);


                GC.SuppressFinalize(_mFactory);
                GC.SuppressFinalize(_mPlayer);
                GC.SuppressFinalize(_mMedia);

                _memRender = _mPlayer.CustomRenderer;
                _memRender.SetFormat(new BitmapFormat(ResizeWidth, ResizeHeight, ChromaType.RV24));
                _memRender.SetCallback(delegate(Bitmap frame)
                {
                    _framesReceived++;
                    if (NewFrame != null)
                    {
                        NewFrame(this, new NewFrameEventArgs(frame));
                    }
                    frame.Dispose();
                });

                _mMedia.Parse(true);
                _framesReceived = 0;
                Debug.WriteLine("VLCStream Started");
                _mPlayer.Play();
            }
        }


        /// <summary>
        /// Calls Stop
        /// </summary>
        public void SignalToStop()
        {
            Stop();
        }

        /// <summary>
        /// Calls Stop
        /// </summary>
        public void WaitForStop()
        {
            Stop();
        }

        /// <summary>
        /// Stop video source.
        /// </summary>
        /// 
        public void Stop()
        {
            if (RetryTimer != null)
            {
                RetryTimer.Close();
                RetryTimer.Dispose();
                RetryTimer = null;
            }
            
            if (_mPlayer != null)
            {
                _isrunning = false;
                var t = new Thread(DoStopVideo);
                t.Start();
                
            }                
        }

        private void DoStopVideo()
        {
            var t = new Thread(StopVideo);
            t.Start();
            t.Join(2000);
        }

        private void StopVideo()
        {
            try
            {
                _mPlayer.Stop();
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
        }
        #endregion

        private void EventsPlayerEncounteredError(object sender, EventArgs e)
        {
            _starting = false;
            Debug.Print("VLC Error");
            DisposePlayer();

            if (RetryTimer != null)
            {
                RetryTimer.Stop();
                RetryTimer.Close();
                RetryTimer.Dispose();
            }

            VideoSourceError(sender, new VideoSourceErrorEventArgs("Error playing stream"));
            RetryTimer = new Timer(5000);
            RetryTimer.Elapsed += new ElapsedEventHandler(RetryTimerElapsed);
            RetryTimer.Start();
        }

        private void RetryTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RetryTimer.Stop();
            RetryTimer.Close();
            RetryTimer.Dispose();
            Start();
        }

        private void EventsPlayerStopped(object sender, EventArgs e)
        {
            _starting = false;
            Debug.Print("VLC Stopped");

            DisposePlayer();

            _isrunning = false;

            if (Restart)
                return;

            if (PlayingFinished!=null)
                PlayingFinished(sender, ReasonToFinishPlaying.StoppedByUser);
        }

        private void DisposePlayer()
        {
            GC.ReRegisterForFinalize(_mFactory);
            GC.ReRegisterForFinalize(_mPlayer);
            GC.ReRegisterForFinalize(_mMedia);

            _mPlayer.Dispose();
            _mFactory.Dispose();
            _mMedia.Dispose();
        }

        private void EventsPlayerPlaying(object sender, EventArgs e)
        {
            _isrunning = true;
            _starting = false;
        }
    }
}