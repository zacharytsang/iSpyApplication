using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Vision.Motion;

using Image = System.Drawing.Image;
using Point = System.Drawing.Point;

namespace iSpyApplication
{
    /// <summary>
    /// Camera class
    /// </summary>
    public class Camera
    {
        public CameraWindow CW;

        public bool LastFrameNull = true;
        public bool MotionDetected;
        public float MotionLevel;

        public Rectangle[] MotionZoneRectangles;
        public IVideoSource VideoSource;
        public double Framerate;
        public UnmanagedImage LastFrameUnmanaged;

        private HSLFiltering _filter = null;

        public HSLFiltering Filter
        {
            get
            {
                if (CW.Camobject.detector.colourprocessingenabled)
                {
                    if (_filter != null)
                        return _filter;
                    if (!String.IsNullOrEmpty(CW.Camobject.detector.colourprocessing))
                    {
                        string[] config = CW.Camobject.detector.colourprocessing.Split(',');
                        _filter = new HSLFiltering()
                                      {
                                          FillColor =
                                              new HSL(Convert.ToInt32(config[2]), float.Parse(config[5]),
                                                      float.Parse(config[8])),
                                          FillOutsideRange = Convert.ToInt32(config[9])==0,
                                          Hue = new IntRange(Convert.ToInt32(config[0]), Convert.ToInt32(config[1])),
                                          Saturation = new Range(float.Parse(config[3]), float.Parse(config[4])),
                                          Luminance = new Range(float.Parse(config[6]), float.Parse(config[7])),
                                          UpdateHue = Convert.ToBoolean(config[10]),
                                          UpdateSaturation = Convert.ToBoolean(config[11]),
                                          UpdateLuminance = Convert.ToBoolean(config[12])

                        };
                    }
                    
                }

                return null;
            }
        }

        public void FilterChanged()
        {
            lock (this)
            {
                _filter = null;
            }

        }
        
        
        private MotionDetector _motionDetector;
        private int _processFrameCount;

        // alarm level
        private double _alarmLevel = 0.0005;
        private int _height = -1;
        private DateTime _lastframeReceived = DateTime.MinValue;
        private DateTime _lastframeProcessed = DateTime.MinValue;
        public double _frameratetotal, _frameratetotalactual;
        private double _frameratesamplecount;
        private DateTime _lastframeReceivedActual;
        public int _frameratesamplecountactual;
        
        private int _width = -1;

        public Camera() : this(null, null)
        {
        }

        public Camera(IVideoSource source)
        {
            VideoSource = source;
            _motionDetector = null;
            VideoSource.NewFrame += VideoNewFrame;
        }

        public Camera(IVideoSource source, MotionDetector detector)
        {
            VideoSource = source; // new AsyncVideoSource(source);
            _motionDetector = detector;
            VideoSource.NewFrame += VideoNewFrame;
        }

        public Bitmap LastFrame
        {
            get
            {
                Bitmap bm = null;
                lock (this)
                {
                    if (LastFrameUnmanaged != null)
                    {
                        try
                        {
                            bm = LastFrameUnmanaged.ToManagedImage();
                            LastFrameNull = false;
                        }
                        catch
                        {
                            if (bm != null)
                                bm.Dispose();
                            LastFrameNull = true;
                        }
                    }
                }
                return bm;
            }
        }

        // Running propert
        public bool IsRunning
        {
            get { return (VideoSource == null) ? false : VideoSource.IsRunning; }
        }

        //


        // Width property
        public int Width
        {
            get { return _width; }
        }

        // Height property
        public int Height
        {
            get { return _height; }
        }

        // AlarmLevel property
        public double AlarmLevel
        {
            get { return _alarmLevel; }
            set { _alarmLevel = value; }
        }

        // FramesReceived property
        public int FramesReceived
        {
            get { return (VideoSource == null) ? 0 : VideoSource.FramesReceived; }
        }

        // BytesReceived property
        public int BytesReceived
        {
            get { return (VideoSource == null) ? 0 : VideoSource.BytesReceived; }
        }

        // motionDetector property
        public MotionDetector MotionDetector
        {
            get { return _motionDetector; }
            set
            {
                _motionDetector = value;
                if (value != null) _motionDetector.MotionZones = MotionZoneRectangles;
            }
        }

        public Image Mask { get; set; }

        public bool SetMotionZones(objectsCameraDetectorZone[] zones)
        {
            if (zones == null || zones.Length == 0)
            {
                ClearMotionZones();
                return true;
            }
            //rectangles come in as percentages to allow resizing and resolution changes

            if (_width > -1)
            {
                double wmulti = Convert.ToDouble(_width)/Convert.ToDouble(100);
                double hmulti = Convert.ToDouble(_height)/Convert.ToDouble(100);
                var rects = new List<Rectangle>();
                for (int index = 0; index < zones.Length; index++)
                {
                    var r = zones[index];
                    rects.Add(new Rectangle(Convert.ToInt32(r.left*wmulti), Convert.ToInt32(r.top*hmulti),
                                            Convert.ToInt32(r.width*wmulti), Convert.ToInt32(r.height*hmulti)));
                }
                MotionZoneRectangles = rects.ToArray();
                if (_motionDetector != null)
                    _motionDetector.MotionZones = MotionZoneRectangles;
                return true;
            }
            return false;
        }

        public void ClearMotionZones()
        {
            MotionZoneRectangles = null;
            if (_motionDetector != null)
                _motionDetector.MotionZones = null;
        }

        public event EventHandler NewFrame;
        public event EventHandler Alarm;

        // Constructor

        // Start video source
        public void Start()
        {
            if (VideoSource != null)
            {
                VideoSource.Start();
            }
        }

        // Signal video source to stop
        public void SignalToStop()
        {
            Monitor.Enter(this);
            if (VideoSource != null)
            {
                VideoSource.SignalToStop();
            }
            Monitor.Exit(this);
            _frameratesamplecount = 0;
            _frameratetotal = 0;
            _lastframeProcessed = DateTime.MinValue;
            _lastframeReceived = DateTime.MinValue;
        }

        //// Wait video source for stop
        //public void WaitForStop()
        //{
        //    // lock
        //    Monitor.Enter(this);

        //    if (VideoSource != null)
        //    {
        //        VideoSource.WaitForStop();
        //    }
        //    // unlock
        //    Monitor.Exit(this);
        //}

        // Abort camera
        public void Stop()
        {
            // lock
            Monitor.Enter(this);

            if (VideoSource != null)
            {
                VideoSource.Stop();
            }
            // unlock
            Monitor.Exit(this);
            _frameratesamplecount = 0;
            _frameratetotal = 0;
            _lastframeProcessed = DateTime.MinValue;
            _lastframeReceived = DateTime.MinValue;
        }

        // On new frame
        

        public double FramerateAverage
        {
            get
            {
                if (_frameratesamplecount>0)
                    return _frameratetotal/_frameratesamplecount;
                return 0;
            }    
        }

        public double FramerateAverageActual
        {
            get
            {
                if (_frameratesamplecountactual > 0)
                    return _frameratetotalactual / _frameratesamplecountactual;
                return 0;
            }
        }

        private double Mininterval
        {
            get
            {
                var ret = 1000d/CW.Camobject.settings.maxframerate;
                if (CW.Recording)
                    ret = 1000d / CW.Camobject.settings.maxframeraterecord;

                
                if (ret < 1000d/MainForm.ThrottleFramerate)
                    ret = 1000d/MainForm.ThrottleFramerate;
                
                return ret;
            }
        }

        private void VideoNewFrame(object sender, NewFrameEventArgs e)
        {
            if (_lastframeReceivedActual > DateTime.MinValue)
            {
                _frameratesamplecountactual++;
                double fr = 1000d / (DateTime.Now - _lastframeReceivedActual).TotalMilliseconds;
                _frameratetotalactual += fr;
                if (_frameratesamplecountactual == 10)
                {
                    _frameratesamplecountactual = 1;
                    _frameratetotalactual = fr;
                }
            }
            _lastframeReceivedActual = DateTime.Now;
            //discard this frame to limit framerate?
            if (_lastframeProcessed>DateTime.MinValue)
            {

                if ((DateTime.Now - _lastframeProcessed).TotalMilliseconds < Mininterval)
                    return;
            }
            _lastframeProcessed = DateTime.Now;

            if (_lastframeReceived > DateTime.MinValue)
            {
                TimeSpan tsFr = DateTime.Now - _lastframeReceived;
                Framerate = 1000d / tsFr.TotalMilliseconds;

                _frameratetotal += Framerate;
                _frameratesamplecount++;
                if (_frameratesamplecount == 10)
                {
                    _frameratesamplecount = 1;
                    _frameratetotal = Framerate;
                }
            }
            _lastframeReceived = DateTime.Now;

            var tsBrush = new SolidBrush(MainForm.Conf.TimestampColor.ToColor());
            var sbTs = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            var sbPlate = new SolidBrush(Color.Red);
            Bitmap bmOrig = null, bmp = null;
            Graphics g = null, gCam = null;
            try
            {
                // motionLevel = 0;
                if (e.Frame != null)
                {
                    lock (this)
                    {
                        if (LastFrameUnmanaged != null)
                            LastFrameUnmanaged.Dispose();


                        bmOrig = e.Frame;
                        if (CW.Camobject.rotate90)
                            bmOrig.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        if (CW.Camobject.flipx)
                        {
                            bmOrig.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        if (CW.Camobject.flipy)
                        {
                            bmOrig.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        }


                        _width = bmOrig.Width;
                        _height = bmOrig.Height;

                        if (CW.NeedMotionZones)
                            CW.NeedMotionZones = !SetMotionZones(CW.Camobject.detector.motionzones);

                        if (Mask != null)
                        {
                            g = Graphics.FromImage(bmOrig);
                            g.DrawImage(Mask, 0, 0, _width, _height);
                        }

                        LastFrameUnmanaged = UnmanagedImage.FromManagedImage(bmOrig);

                        if (_motionDetector != null)
                        {
                            if (Alarm != null)
                            {
                                _processFrameCount++;
                                if (_processFrameCount >= CW.Camobject.detector.processeveryframe || CW.Calibrating)
                                {
                                    _processFrameCount = 0;
                                    MotionLevel = _motionDetector.ProcessFrame(LastFrameUnmanaged,Filter);
                                    if (MotionLevel >= _alarmLevel)
                                    {
                                        MotionDetected = true;
                                        Alarm(this, new EventArgs());
                                    }
                                    else
                                        MotionDetected = false;
                                }
                            }
                            else
                                MotionDetected = false;
                        }
                        else
                            MotionDetected = false;


                        if (CW.Camobject.settings.timestamplocation != 0 &&
                            CW.Camobject.settings.timestampformatter != "")
                        {
                            bmp = LastFrameUnmanaged.ToManagedImage();
                            gCam = Graphics.FromImage(bmp);

                            var timestamp =
                                String.Format(
                                    CW.Camobject.settings.timestampformatter.Replace("{FPS}",
                                                                                      string.Format("{0:F2}",
                                                                                                    FramerateAverage)),
                                    DateTime.Now);

                            var rs = gCam.MeasureString(timestamp, Drawfont).ToSize();
                            var p = new Point(0, 0);
                            switch (CW.Camobject.settings.timestamplocation)
                            {
                                case 2:
                                    p.X = _width/2 - (rs.Width/2);
                                    break;
                                case 3:
                                    p.X = _width - rs.Width;
                                    break;
                                case 4:
                                    p.Y = _height - rs.Height;
                                    break;
                                case 5:
                                    p.Y = _height - rs.Height;
                                    p.X = _width/2 - (rs.Width/2);
                                    break;
                                case 6:
                                    p.Y = _height - rs.Height;
                                    p.X = _width - rs.Width;
                                    break;
                            }
                            var rect = new Rectangle(p, rs);

                            gCam.FillRectangle(sbTs, rect);
                            gCam.DrawString(timestamp, Drawfont, tsBrush, p);

                            if (CW.Camobject.alerts.overlay && CW.Numberplate != Rectangle.Empty && CW.Plate!="")
                            {
                                Rectangle r = CW.Numberplate;
                                var pPlate = new Point(r.X, r.Y);
                                if (!String.IsNullOrEmpty(CW.Camobject.alerts.numberplatesarea))
                                {
                                    var area = CW.Camobject.alerts.numberplatesarea.Split(',');
                                    pPlate.X += (Convert.ToInt32(area[0])*Width)/100;
                                    pPlate.Y += (Convert.ToInt32(area[1])*Height)/100;
                                    if (pPlate.Y < 20)
                                        pPlate.Y = 20;
                                }
                                var plateFont = new Font(MainForm.Drawfont.Name, 25);
                                gCam.DrawString(CW.Plate, plateFont, sbPlate, pPlate);
                                plateFont.Dispose();
                            }

                            LastFrameUnmanaged.Dispose();
                            LastFrameUnmanaged = UnmanagedImage.FromManagedImage(bmp);
                        }
                    }
                }
            }
            catch (UnsupportedImageFormatException ex)
            {
                CW.VideoSourceErrorState = true;
                CW.VideoSourceErrorMessage = ex.Message;
                if (LastFrameUnmanaged != null)
                {
                    try
                    {
                        lock (this)
                        {
                            LastFrameUnmanaged.Dispose();
                            LastFrameUnmanaged = null;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                if (LastFrameUnmanaged != null)
                {
                    try
                    {
                        lock (this)
                        {
                            LastFrameUnmanaged.Dispose();
                            LastFrameUnmanaged = null;
                        }
                    }
                    catch
                    {
                    }
                }
                MainForm.LogExceptionToFile(ex);
            }
            
            if (gCam != null)
                gCam.Dispose();
            if (bmp != null)
                bmp.Dispose();
            if (g != null)
                g.Dispose();
            if (bmOrig != null)
                bmOrig.Dispose();
            tsBrush.Dispose();
            sbTs.Dispose();
            sbPlate.Dispose();

            if (NewFrame != null && LastFrameUnmanaged != null)
            {
                LastFrameNull = false;
                NewFrame(this, new EventArgs());
            }
            if (NewFrame==null)
                Debug.WriteLine("NewFrame is null!");
        }

        private Font _drawfont;
        public Font Drawfont
        {
            get
            {
                if (_drawfont!=null)
                    return _drawfont;
                _drawfont = new Font(MainForm.Drawfont.FontFamily,CW.Camobject.settings.timestampfontsize);
                return _drawfont;
            }
            set { _drawfont = value; }
        }
    }
}