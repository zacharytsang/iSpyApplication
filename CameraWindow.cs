using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.Kinect;
using AForge.Video.VFW;
using AForge.Video.Ximea;
using AForge.Vision.Motion;
using xiApi.NET;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using PictureBox = AForge.Controls.PictureBox;

namespace iSpyApplication
{
    /// <summary>
    /// Summary description for CameraWindow.
    /// </summary>
    public sealed class CameraWindow : PictureBox
    {
        #region Private
        private readonly HaarCascade _haaRclassifier = new FaceHaarCascade();
        public bool ForcedRecording;
        public bool NeedMotionZones = true;
        public XimeaVideoSource XimeaSource;
        private List<FilesFile> _filelist;

        private AVIWriter _timeLapseWriter;
        private AVIWriter _writer;
        public bool Alerted;
        private Camera _camera;
        //public double Framerate;
        private double _intervalCount;
        private DateTime _lastFrameSent = DateTime.Now;
        private DateTime _lastFrameUploaded = DateTime.Now;
        private DateTime _lastNumberplateScan = DateTime.Now;
        private long _lastRun = DateTime.Now.Ticks;
        private int _milliCount;


        public double MovementCount;
        public double NoMovementCount;
        public Rectangle Numberplate = Rectangle.Empty;
        public string Plate = "";
        
        private bool _pairedRecording;
        private bool _processing;
        private double _recordingTime;
        private bool _stopWrite;
        private double _timeLapse;
        private double _timeLapseFrames;
        private double _timeLapseTotal;
        private Point _mouseLoc;
        private Queue _videoBuffer = new Queue();
        private QueueWithEvents<FrameAction> _writerBuffer;
        private DateTime _errorTime = DateTime.MinValue;
        private HaarObjectDetector _haardetector;
        private readonly AutoResetEvent _newRecordingFrame = new AutoResetEvent(false);
        private string _lastmatched = "";
        private Thread _recordingThread;
        private bool _isTrigger;
        private int _calibrateTarget;
        #endregion

        #region Public

        #region Delegates

        public delegate void NotificationEventHandler(object sender, NotificationType e);

        public delegate void RemoteCommandEventHandler(object sender, ThreadSafeCommand e);

        #endregion

        public string AudioMergeFilename = "";
        public double CalibrateCount, ReconnectCount;
        public Rectangle RestoreRect = Rectangle.Empty;
        public bool Calibrating;

        public Graphics CurrentFrame;
        public PTZController PTZ;
        public int FlashCounter;
        public double InactiveRecord;
        public bool IsEdit;
        public bool MovementDetected;
        public bool PTZNavigate;
        public Point PTZReference;
        public bool NeedSizeUpdate;
        public bool ResizeParent;
        public bool ShuttingDown;
        public string TimeLapseVideoFileName = "";
        public string VideoFileName = "";
        public string VideoSourceErrorMessage = "";
        public bool VideoSourceErrorState;
        public objectsCamera Camobject;
        public ANPR Lpr = null;
        public bool LprInited = false;

        public VolumeLevel VolumeControl
        {
         get
         {
            if (Camobject!=null && Camobject.settings.micpair>-1)
            {
                return ((MainForm) TopLevelControl).GetVolumeLevel(Camobject.settings.micpair);
            }
            return null;
         }   
        }

        public bool Recording
        {
            get
            {
                return _recordingThread!=null && _recordingThread.IsAlive;
            }
        }
        
        public bool SavingTimeLapse
        {
            get { return _timeLapseWriter != null; }
        }

        public List<FilesFile> FileList
        {
            get
            {
                if (_filelist != null)
                    return _filelist;
                string dir = MainForm.Conf.MediaDirectory + "video\\" +
                                                      Camobject.directory + "\\";
                if (File.Exists(dir + "data.xml"))
                {
                    var s = new XmlSerializer(typeof(Files));
                    var fs = new FileStream(dir + "data.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    try
                    {
                        TextReader reader = new StreamReader(fs);
                        fs.Position = 0;
                        _filelist = ((Files)s.Deserialize(reader)).File.ToList();
                    }
                    catch (Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex);
                    }
                    fs.Close();
                    if (_filelist != null)
                        return _filelist;
                }
                
                //else build from directory contents
                //if (_filelist==null)
                    _filelist = new List<FilesFile>();
                var dirinfo = new DirectoryInfo(MainForm.Conf.MediaDirectory + "video\\" +
                                                      Camobject.directory + "\\");

                var lFi = new List<FileInfo>();
                lFi.AddRange(dirinfo.GetFiles());
                lFi = lFi.FindAll(f => f.Extension.ToLower() == ".flv" || f.Extension.ToLower() == ".mp4" || f.Extension.ToLower() == ".webm");
                lFi = lFi.OrderByDescending(f => f.CreationTime).ToList();
                //sanity check existing data
                foreach (FileInfo fi in lFi)
                {
                    if (_filelist.Where(p => p.Filename == fi.Name).Count() == 0)
                    {
                        _filelist.Add(new FilesFile
                                          {
                                              CreatedDateTicks = fi.CreationTime.Ticks,
                                              Filename = fi.Name,
                                              SizeBytes = fi.Length,
                                              MaxAlarm = 0,
                                              AlertData = "0",
                                              DurationSeconds = 0,
                                              IsTimelapse = fi.Name.ToLower().IndexOf("timelapse")!=-1
                                          });
                    }
                }
                for (int index = 0; index < _filelist.Count; index++)
                {
                    FilesFile ff = _filelist[index];
                    if (lFi.Where(p => p.Name == ff.Filename).Count() == 0)
                    {
                        _filelist.Remove(ff);
                        index--;
                    }
                }
                _filelist = _filelist.OrderByDescending(p => p.CreatedDateTicks).ToList();
                return _filelist;
            }
            set { lock (_filelist) {_filelist = value;} }
        }

        public void SaveFileList()
        {
            try
            {
                if (_filelist!=null)
                    lock (_filelist)
                    {
                        var fl = new Files {File = FileList.ToArray()};
                        string fn = MainForm.Conf.MediaDirectory + "video\\" +
                                    Camobject.directory + "\\data.xml";
                        var s = new XmlSerializer(typeof (Files));
                        var fs = new FileStream(fn, FileMode.Create);
                        TextWriter writer = new StreamWriter(fs);
                        fs.Position = 0;

                        s.Serialize(writer, fl);
                        fs.Close();
                    }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
        }


        public event RemoteCommandEventHandler RemoteCommand;
        public event NotificationEventHandler Notification;

        #endregion

        #region SizingControls

        public void UpdatePosition()
        {
            Monitor.Enter(this);

            if (Parent != null && _camera != null)
            {
                if (!_camera.LastFrameNull)
                {
                    int width = _camera.Width;
                    int height = _camera.Height;
                    Camobject.resolution = width + "x" + height;
                    SuspendLayout();
                    Size = new Size(width + 2, height + 26);
                    Camobject.width = width;
                    Camobject.height = height;
                    ResumeLayout();
                    NeedSizeUpdate = false;
                }
                else
                {
                    Monitor.Exit(this);
                    return;
                }               
            }
            Monitor.Exit(this);
        }

        private void CameraWindowResize(object sender, EventArgs e)
        {
        }
        #endregion

        private double _secondCount;

        public CameraWindow(objectsCamera cam)
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
            Margin = new Padding(0, 0, 0, 0);
            Padding = new Padding(0, 0, 5, 5);
            BorderStyle = BorderStyle.None;
            BackColor = MainForm.Conf.BackColor.ToColor();
            Camobject = cam;
            PTZ = new PTZController(this);
        }

        public Camera Camera
        {
            get { return _camera; }
            set
            {
                Monitor.Enter(this);
                bool newCamera = (value != _camera && value != null);

                if (value == null && _camera != null && _camera.IsRunning)
                {
                    Disable();
                }

                _camera = value;
                if (_camera != null)
                {
                    _camera.CW = this;
                    lock (_videoBuffer.SyncRoot)
                    {
                        while (_videoBuffer.Count > 0)
                            ((FrameAction) _videoBuffer.Dequeue()).Frame.Dispose();

                        _videoBuffer = null;

                        if (value != null)
                        {
                            if (newCamera)
                            {
                                _camera.NewFrame += CameraNewFrame;
                                _camera.Alarm += CameraAlarm;
                            }
                            _videoBuffer = new Queue(Camobject.recorder.bufferframes);
                        }
                    }
                }
                Monitor.Exit(this);
            }
        }

        private HaarObjectDetector HaarDetector
        {
            get
            {
                return _haardetector ?? (_haardetector = new HaarObjectDetector(
                                                             _haaRclassifier, 50, ObjectDetectorSearchMode.NoOverlap,
                                                             1.2f, ObjectDetectorScalingMode.SmallerToGreater));
            }
        }

        public string[] ScheduleDetails
        {
            get
            {
                var entries = new List<string>();
                foreach (objectsCameraScheduleEntry sched in Camobject.schedule.entries)
                {
                    string daysofweek = sched.daysofweek;
                    daysofweek = daysofweek.Replace("0", LocRm.GetString("Sun"));
                    daysofweek = daysofweek.Replace("1", LocRm.GetString("Mon"));
                    daysofweek = daysofweek.Replace("2", LocRm.GetString("Tue"));
                    daysofweek = daysofweek.Replace("3", LocRm.GetString("Wed"));
                    daysofweek = daysofweek.Replace("4", LocRm.GetString("Thu"));
                    daysofweek = daysofweek.Replace("5", LocRm.GetString("Fri"));
                    daysofweek = daysofweek.Replace("6", LocRm.GetString("Sat"));

                    string s = sched.start + " -> " + sched.stop + " (" + daysofweek + ")";
                    if (sched.recordonstart)
                        s += " " + LocRm.GetString("RECORD_UC");
                    if (sched.alerts)
                        s += " " + LocRm.GetString("ALERT_UC");
                    if (sched.recordondetect)
                        s += " " + LocRm.GetString("DETECT_UC");
                    if (sched.timelapseenabled)
                        s += " " + LocRm.GetString("TIMELAPSE_UC");
                    if (!sched.active)
                        s += " (" + LocRm.GetString("INACTIVE_UC") + ")";

                    entries.Add(s);
                }
                return entries.ToArray();
            }
        }

        #region MouseEvents

        private MousePos GetMousePos(Point location)
        {
            MousePos result = MousePos.NoWhere;
            int rightSize = Padding.Right;
            int bottomSize = Padding.Bottom;
            var testRect = new Rectangle(Width - rightSize, 0, Width - rightSize, Height - bottomSize);
            if (testRect.Contains(location)) result = MousePos.Right;
            testRect = new Rectangle(0, Height - bottomSize, Width - rightSize, Height);
            if (testRect.Contains(location)) result = MousePos.Bottom;
            testRect = new Rectangle(Width - rightSize, Height - bottomSize, Width, Height);
            if (testRect.Contains(location)) result = MousePos.BottomRight;
            return result;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            IntPtr hwnd = Handle;
            if (ResizeParent && Parent != null && Parent.IsHandleCreated)
            {
                hwnd = Parent.Handle;
            }
            MousePos mousePos = GetMousePos(e.Location);
            switch (mousePos)
            {
                case MousePos.Right:
                    {
                        NativeCalls.ReleaseCapture(hwnd);
                        NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeE, IntPtr.Zero);
                    }
                    break;
                case MousePos.Bottom:
                    {
                        NativeCalls.ReleaseCapture(hwnd);
                        NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeS, IntPtr.Zero);
                    }
                    break;
                case MousePos.BottomRight:
                    {
                        NativeCalls.ReleaseCapture(hwnd);
                        NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeSe, IntPtr.Zero);
                    }
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mouseLoc.X = e.X;
            _mouseLoc.Y = e.Y;
            MousePos mousePos = GetMousePos(e.Location);
            switch (mousePos)
            {
                case MousePos.Right:
                    Cursor = Cursors.SizeWE;
                    break;
                case MousePos.Bottom:
                    Cursor = Cursors.SizeNS;
                    break;
                case MousePos.BottomRight:
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    Cursor = Cursors.Hand;
                    break;
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (_camera != null && !_camera.LastFrameNull)
                {
                    double arW = Convert.ToDouble(_camera.Width)/Convert.ToDouble(_camera.Height);
                    Width = Convert.ToInt32(arW*Height);
                }
            }
            base.OnResize(eventargs);
            if (Width < MinimumSize.Width) Width = MinimumSize.Width;
            if (Height < MinimumSize.Height) Height = MinimumSize.Height;
            if (VolumeControl!=null)
                MainForm.NeedsRedraw = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
            
        }

        private enum MousePos
        {
            NoWhere,
            Right,
            Bottom,
            BottomRight
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Invalidate();
            }
            if (_timeLapseWriter != null)
                _timeLapseWriter.Dispose();
            if (_writer != null)
                _writer.Dispose();
            base.Dispose(disposing);
        }

        public void Tick()
        {
            if (_processing)
                return;
            _processing = true;
            try
            {
                //time since last tick
                var ts = new TimeSpan(DateTime.Now.Ticks - _lastRun);

                _milliCount += Convert.ToInt32(ts.TotalMilliseconds);
                _lastRun = DateTime.Now.Ticks;

                _secondCount += ts.TotalMilliseconds/1000.0;
                
                if (FlashCounter == 5)
                {
                    MovementDetected = false;
                }
                if (FlashCounter > 5)
                {
                    if (Camobject.ftp.enabled && Camobject.ftp.mode == 0 && Camobject.ftp.ready)
                    {
                        //ftp frame on motion detection
                        FtpFrame();
                    }
                }
                if (FlashCounter > 5)
                    InactiveRecord = 0;

                if (FlashCounter == 1)
                {
                    UpdateFloorplans(false);
                }

                if (FlashCounter > 0)
                    FlashCounter--;

                if (Recording)
                    _recordingTime += Convert.ToDouble(ts.TotalMilliseconds)/1000.0;

                if (Camobject.alerts.active && Camobject.settings.active)
                {
                    switch (Camobject.alerts.mode)
                    {
                        case "movement":
                        case "np_recog":
                        case "np_notrecog":
                        case "face":
                            if (FlashCounter > 0)
                            {
                                BackColor = (BackColor == MainForm.Conf.ActivityColor.ToColor())
                                                ? MainForm.Conf.BackColor.ToColor()
                                                : MainForm.Conf.ActivityColor.ToColor();
                            }
                            else
                                BackColor = MainForm.Conf.BackColor.ToColor();
                            break;
                        case "nomovement":
                            if (!MovementDetected)
                            {
                                BackColor = (BackColor == MainForm.Conf.NoActivityColor.ToColor())
                                                ? MainForm.Conf.BackColor.ToColor()
                                                : MainForm.Conf.NoActivityColor.ToColor();
                            }
                            else
                                BackColor = MainForm.Conf.BackColor.ToColor();
                            break;
                    }
                }
                else
                {
                    BackColor = MainForm.Conf.BackColor.ToColor();
                }

                if (_secondCount > 1) //every second
                {
                    if (Camobject.settings.ptzautotrack)
                    {
                        if (_ptzneedsstop && _lastAutoTrackSent < DateTime.Now.AddMilliseconds(-1000))
                        {
                            PTZ.SendPTZCommand(Enums.PtzCommand.Stop);
                            _ptzneedsstop = false;
                        }
                        if (_lastAutoTrackSent>DateTime.MinValue && _lastAutoTrackSent<DateTime.Now.AddMilliseconds(-30000))
                        {
                            _lastAutoTrackSent = DateTime.MinValue;                           
                            Calibrating = true;
                            CalibrateCount = 0;
                            _calibrateTarget = Camobject.settings.ptztimetohome;
                            PTZ.SendPTZCommand(Enums.PtzCommand.Center);
                        }
                    }
                    if (Calibrating)
                    {
                        if (!Camera.LastFrameNull)
                        {
                            if (_camera.MotionDetector != null)
                            {
                                if (_camera.MotionDetector.MotionDetectionAlgorithm is CustomFrameDifferenceDetector)
                                {
                                    ((CustomFrameDifferenceDetector)_camera.MotionDetector.MotionDetectionAlgorithm).
                                        SetBackgroundFrame(_camera.LastFrame);
                                }
                            }

                            CalibrateCount += _secondCount;
                            if (CalibrateCount > _calibrateTarget)
                            {
                                Calibrating = false;
                                CalibrateCount = 0;
                            }
                        }
                    }
                    else
                    {
                        if (_camera != null && Camobject.settings.active)
                        {
                            if (Camobject.settings.reconnectinterval > 0 && Camobject.settings.sourceindex == 5)
                                //vlc only
                            {
                                var vlc = ((VlcStream) _camera.VideoSource);
                                if (vlc.Restart && !vlc.IsRunning)
                                {
                                    vlc.Restart = false;
                                    vlc.Start();
                                }
                                else
                                {
                                    ReconnectCount += _secondCount;
                                    if (ReconnectCount > Camobject.settings.reconnectinterval)
                                    {
                                        vlc.Restart = true;
                                        vlc.Stop();
                                        ReconnectCount = 0;
                                    }
                                }
                                

                            }
                        }
                        if (Camobject.settings.notifyondisconnect && MainForm.Conf.Subscribed &&
                            _errorTime != DateTime.MinValue)
                        {
                            int sec = Convert.ToInt32((DateTime.Now - _errorTime).TotalSeconds);
                            if (sec > 10 && sec < 20)
                            {
                                string subject =
                                    LocRm.GetString("CameraNotifyDisconnectMailSubject").Replace("[OBJECTNAME]",
                                                                                                 Camobject.name);
                                string message = LocRm.GetString("CameraNotifyDisconnectMailBody");
                                message = message.Replace("[NAME]", Camobject.name);
                                message = message.Replace("[TIME]", DateTime.Now.ToLongTimeString());
                                MainForm.WSW.SendAlert(Camobject.settings.emailaddress, subject, message);
                                _errorTime = DateTime.MinValue;
                            }
                        }

                        if (Recording && !MovementDetected && !ForcedRecording)
                        {
                            InactiveRecord += _secondCount;
                        }

                        if (Camobject.schedule.active)
                        {
                            DateTime dtnow = DateTime.Now;
                            foreach (
                                var entry in Camobject.schedule.entries.Where(p => p.active))
                            {
                                if (entry.daysofweek.IndexOf(((int) dtnow.DayOfWeek).ToString()) == -1) continue;
                                var stop = entry.stop.Split(':');
                                if (stop[0] != "-")
                                {
                                    if (Convert.ToInt32(stop[0]) == dtnow.Hour)
                                    {
                                        if (Convert.ToInt32(stop[1]) == dtnow.Minute && dtnow.Second < 30) //<5 so can reactivate if needed between 5 and 10 seconds
                                        {
                                            if (Camobject.settings.active)
                                                Disable();
                                            goto skip;
                                        }
                                    }
                                }

                                var start = entry.start.Split(':');
                                if (start[0] != "-")
                                {
                                    if (Convert.ToInt32(start[0]) == dtnow.Hour)
                                    {
                                        if (Convert.ToInt32(start[1]) == dtnow.Minute && dtnow.Second < 59)
                                        {
                                            if (!Camobject.settings.active)
                                                Enable();
                                            Camobject.detector.recordondetect = entry.recordondetect;
                                            Camobject.detector.recordonalert = entry.recordonalert;
                                            Camobject.alerts.active = entry.alerts;
                                            if (Camobject.recorder.timelapseenabled && !entry.timelapseenabled)
                                            {
                                                CloseTimeLapseWriter();
                                                _timeLapseTotal = 0;
                                            }
                                            Camobject.recorder.timelapseenabled = entry.timelapseenabled;
                                            if (entry.recordonstart)
                                            {
                                                ForcedRecording = true;
                                                StartSaving();
                                            }
                                            goto skip;
                                        }
                                    }
                                }
                            }
                        }
                        if (Camobject.settings.active && !Camera.LastFrameNull && !Calibrating)
                        {
                            //Email Interval
                            if (MainForm.Conf.Subscribed && Camobject.notifications.emailgrabinterval != 0)
                            {
                                var tsFg = new TimeSpan(DateTime.Now.Ticks - _lastFrameSent.Ticks);
                                if (tsFg.TotalMinutes >= Camobject.notifications.emailgrabinterval)
                                {
                                    EmailFrame();
                                }
                            }
                            //FTP Interval
                            if (Camobject.ftp.enabled && Camobject.ftp.mode == 2 && Camobject.ftp.interval != 0 &&
                                Camobject.ftp.ready)
                            {
                                var tsFg = new TimeSpan(DateTime.Now.Ticks - _lastFrameUploaded.Ticks);
                                if (tsFg.TotalSeconds >= Camobject.ftp.interval)
                                {
                                    FtpFrame();
                                }
                            }
                            //Check Alert Interval
                            if (Alerted)
                            {
                                _intervalCount += _secondCount;
                                if (_intervalCount > Camobject.alerts.minimuminterval)
                                {
                                    Alerted = false;
                                    _intervalCount = 0;
                                    UpdateFloorplans(false);
                                }
                            }
                            else
                            {
                                //Check new Alert
                                if (Camobject.alerts.active && _camera!=null)
                                {
                                    switch (Camobject.alerts.mode)
                                    {
                                        case "movement":
                                            if (MovementDetected) 
                                            {
                                                MovementCount += _secondCount;
                                                if (_isTrigger || (Camera.MotionDetected && Math.Floor(MovementCount) >= Camobject.detector.movementinterval))
                                                {
                                                    bool al = _isTrigger = false;
                                                    if (Camera.MotionDetector!=null && Camera.MotionDetector.MotionProcessingAlgorithm is BlobCountingObjectsProcessing)
                                                    {
                                                        var blobalg = (BlobCountingObjectsProcessing) Camera.MotionDetector.MotionProcessingAlgorithm;
                                                        if (blobalg.ObjectsCount >= Camobject.alerts.objectcountalert)
                                                        {
                                                            al = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        al = true;
                                                    }
                                                    if (al)
                                                    {
                                                        RemoteCommand(this, new ThreadSafeCommand("bringtofrontcam," + Camobject.id));
                                                        DoAlert();
                                                        MovementCount = 0;
                                                        if (Camobject.detector.recordonalert && !Recording)
                                                        {
                                                            StartSaving();
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                MovementCount = 0;
                                            }

                                            break;
                                        case "nomovement":

                                            if (!MovementDetected)
                                            {
                                                NoMovementCount += _secondCount;
                                                if (NoMovementCount >= Camobject.detector.nomovementinterval)
                                                {
                                                    RemoteCommand(this,
                                                                  new ThreadSafeCommand("bringtofrontcam," +
                                                                                        Camobject.id));
                                                    DoAlert();
                                                    NoMovementCount = 0;
                                                    if (Camobject.detector.recordonalert && !Recording)
                                                    {
                                                        StartSaving();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                NoMovementCount = 0;
                                            }

                                            break;
                                        case "np_recog":
                                        case "np_not_recog":
                                            if (Lpr == null)
                                            {
                                                Lpr = new ANPR();
                                                LprInited = true;
                                            }
                                            switch (Camobject.alerts.numberplatesmode)
                                            {
                                                case 0: //continuous
                                                    if ((DateTime.Now - _lastNumberplateScan).TotalSeconds >=
                                                        Camobject.alerts.numberplatesinterval)
                                                    {
                                                        _lastNumberplateScan = DateTime.Now;
                                                        try
                                                        {
                                                            Plate = Lpr.ExtractNumberplate(_camera.LastFrame,
                                                                                            out Numberplate,
                                                                                            Camobject.alerts.
                                                                                                numberplatesarea);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            string m = ex.Message;
                                                        }
                                                    }
                                                    break;
                                                case 1: //parking
                                                    if (FlashCounter>0 && FlashCounter < 2)
                                                    {
                                                        Plate = Lpr.ExtractNumberplate(_camera.LastFrame,
                                                                                        out Numberplate,
                                                                                        Camobject.alerts.
                                                                                            numberplatesarea);
                                                    }
                                                    break;
                                            }
                                            if (Plate != "")
                                            {
                                                string matched;
                                                bool alert = Lpr.NumberPlateInList(Plate,
                                                                                    Camobject.alerts.numberplates,
                                                                                    Camobject.alerts.
                                                                                        numberplatesaccuracy,
                                                                                    out matched);
                                                if (Camobject.alerts.mode == "np_not_recog")
                                                    alert = !alert;

                                                if (matched != "")
                                                {
                                                    if (_lastmatched != matched)
                                                    {
                                                        MainForm.LogMessageToFile("Found numberplate: " + matched);
                                                        _lastmatched = matched;
                                                    }
                                                }
                                                if (alert)
                                                {
                                                    DoAlert();
                                                    MovementCount = 0;
                                                    if (Camobject.detector.recordonalert && !Recording)
                                                    {
                                                        StartSaving();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Numberplate = Rectangle.Empty;
                                            }
                                            break;
                                        case "face":
                                            //Face Detection
                                            var bmpFace = (Bitmap) _camera.LastFrame.Clone();
                                            Grayscale.CommonAlgorithms.BT709.Apply(bmpFace);
                                            if (HaarDetector.ProcessFrame(bmpFace).Length > 0)
                                            {
                                                DoAlert();
                                                MovementCount = 0;
                                                if (Camobject.detector.recordonalert && !Recording)
                                                {
                                                    StartSaving();
                                                }
                                            }

                                            bmpFace.Dispose();
                                            break;
                                    }
                                }
                            }
                            //Check record
                            if (((Camobject.detector.recordondetect && MovementDetected) || ForcedRecording) &&
                                !Recording)
                            {
                                StartSaving();
                            }
                            else
                            {
                                if (!_stopWrite && Recording)
                                {
                                    if ((_recordingTime > Camobject.recorder.maxrecordtime) ||
                                         ((!MovementDetected && InactiveRecord > Camobject.recorder.inactiverecord) &&
                                          !ForcedRecording && !_pairedRecording))
                                        StopSaving();
                                }
                            }

                            //Check TimeLapse
                            if (Camobject.recorder.timelapseenabled)
                            {
                                if (Camobject.recorder.timelapse > 0)
                                {
                                    _timeLapseTotal += _secondCount;
                                    _timeLapse += _secondCount;
                                    if (_timeLapse >= Camobject.recorder.timelapse)
                                    {
                                        if (!SavingTimeLapse)
                                        {
                                            if (!OpenTimeLapseWriter())
                                                goto skip;
                                        }

                                        Bitmap bm = Camera.LastFrame;
                                        try
                                        {
                                            _timeLapseWriter.AddFrame(bm);
                                        }
                                        catch (Exception ex)
                                        {
                                            MainForm.LogExceptionToFile(ex);
                                        }
                                        finally
                                        {
                                            bm.Dispose();
                                        }
                                        _timeLapse = 0;
                                    }
                                    if (_timeLapseTotal >= 3600)
                                    {
                                        CloseTimeLapseWriter();
                                        _timeLapseTotal = 0;
                                    }
                                }
                                if (Camobject.recorder.timelapseframes > 0 && _camera!=null)
                                {
                                    _timeLapseFrames += _secondCount;
                                    if (_timeLapseFrames >= Camobject.recorder.timelapseframes)
                                    {
                                        Image frame = _camera.LastFrame;
                                        string dir = MainForm.Conf.MediaDirectory + "video\\" +
                                                      Camobject.directory + "\\";
                                        dir += @"timelapseframes\";

                                        DateTime date = DateTime.Now;
                                        string filename = String.Format("Frame_{0}-{1}-{2}_{3}-{4}-{5}.jpg",
                                                                         date.Year, Helper.ZeroPad(date.Month),
                                                                         Helper.ZeroPad(date.Day),
                                                                         Helper.ZeroPad(date.Hour),
                                                                         Helper.ZeroPad(date.Minute),
                                                                         Helper.ZeroPad(date.Second));
                                        if (!Directory.Exists(dir))
                                            Directory.CreateDirectory(dir);
                                        frame.Save(dir + filename, ImageFormat.Jpeg);
                                        frame.Dispose();
                                        _timeLapseFrames = 0;
                                    }
                                }
                            }
                        }
                    }
                }
                skip:
                if (_secondCount > 1)
                    _secondCount = 0;
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex, "Camera " + Camobject.id);
            }
            _processing = false;
        }

        

        private void EmailFrame()
        {
            _lastFrameSent = DateTime.Now;
            var t = new Thread(SendFrameGrabThread)
                        {
                            IsBackground = false,
                            Name = "Sending FrameGrab (" + Camobject.id + ")"
                        };
            t.Start();
        }

        private void FtpFrame()
        {
            var imageStream = new MemoryStream();
            Image myThumbnail = null;
            Graphics g = null;
            var strFormat = new StringFormat();
            try
            {
                myThumbnail = Camera.LastFrame;
                g = Graphics.FromImage(myThumbnail);
                strFormat.Alignment = StringAlignment.Center;
                strFormat.LineAlignment = StringAlignment.Far;
                g.DrawString(Camobject.ftp.text, MainForm.Drawfont, Brushes.White,
                             new RectangleF(0, 0, myThumbnail.Width, myThumbnail.Height), strFormat);
                myThumbnail.Save(imageStream, ImageFormat.Jpeg);
                Camobject.ftp.ready = false;
                ThreadPool.QueueUserWorkItem((new AsynchronousFtpUpLoader()).FTP,
                                             new FTPTask(Camobject.ftp.server + ":" + Camobject.ftp.port,
                                                         Camobject.ftp.usepassive, Camobject.ftp.username,
                                                         Camobject.ftp.password, Camobject.ftp.filename,
                                                         imageStream.ToArray(), Camobject.id));
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
                Camobject.ftp.ready = true;
            }
            _lastFrameUploaded = DateTime.Now;
            if (g != null)
                g.Dispose();
            strFormat.Dispose();
            imageStream.Dispose();
            if (myThumbnail != null)
                myThumbnail.Dispose();
        }

        private bool OpenTimeLapseWriter()
        {
            DateTime date = DateTime.Now;
            String filename = String.Format("TimeLapse_{0}-{1}-{2}_{3}-{4}-{5}",
                                             date.Year, Helper.ZeroPad(date.Month), Helper.ZeroPad(date.Day),
                                             Helper.ZeroPad(date.Hour), Helper.ZeroPad(date.Minute),
                                             Helper.ZeroPad(date.Second));
            TimeLapseVideoFileName = Camobject.id + "_" + filename;
            string folder = MainForm.Conf.MediaDirectory + "video\\" + Camobject.directory + "\\";
            
            if (!Directory.Exists(folder + @"thumbs\"))
                Directory.CreateDirectory(folder + @"thumbs\");

            filename = folder+TimeLapseVideoFileName;


            Bitmap bmpPreview = Camera.LastFrame;


            bmpPreview.Save(folder + @"thumbs/" + TimeLapseVideoFileName + "_large.jpg", ImageFormat.Jpeg);
            Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
            Image myThumbnail = bmpPreview.GetThumbnailImage(96, 72, myCallback, IntPtr.Zero);
            bmpPreview.Dispose();

            Graphics g = Graphics.FromImage(myThumbnail);
            var strFormat = new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far};
            var rect = new RectangleF(0, 0, 96, 72);

            g.DrawString(LocRm.GetString("Timelapse"), MainForm.Drawfont, Brushes.White,
                         rect, strFormat);
            strFormat.Dispose();

            myThumbnail.Save(folder + @"thumbs/" + TimeLapseVideoFileName + ".jpg", ImageFormat.Jpeg);

            g.Dispose();
            myThumbnail.Dispose();


            _timeLapseWriter = null;
            bool success = false;
            try
            {
                _timeLapseWriter = new AVIWriter();
                int fr = 60/Camobject.recorder.timelapse;
                _timeLapseWriter.FrameRate = fr > 0 ? fr : 1;
                string[] compressorOptions = MainForm.Conf.CompressorOptions.Split(',');
                _timeLapseWriter.Open(filename + ".avi", _camera.Width, _camera.Height, false, ref compressorOptions);
                success = true;
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex, "Camera " + Camobject.id);
                if (_timeLapseWriter != null)
                {
                    _timeLapseWriter.Dispose();
                    _timeLapseWriter = null;
                }
                Camobject.recorder.timelapse = 0;
            }
            return success;
        }

        private void CloseTimeLapseWriter()
        {
            if (_timeLapseWriter == null)
                return;
            _timeLapseWriter.Close();
            _timeLapseWriter.Dispose();
            _timeLapseWriter = null;

            int tlinterval = Camobject.recorder.timelapse;
            if (Camobject.recorder.timelapse == 0)
                tlinterval = 10;
            int fr = 60/tlinterval;

            if (fr == 0)
                fr = 1;

            string ffmpeg = "\"" + Program.AppPath + "ffmpeg\\ffmpeg.exe\"";

            string args = Camobject.settings.ffmpeg.Trim();
            if (args != "")
            {
                args = args.Replace("{framerate}", fr.ToString());
                args = args.Replace("{filename}",
                                      MainForm.Conf.MediaDirectory + "video\\" + Camobject.directory + "\\" +
                                      TimeLapseVideoFileName);
                args = args.Replace("{presetdir}", Program.AppPath + @"ffmpeg\presets\");

                bool uyt = false;
                if (Camobject.settings.youtube.autoupload && MainForm.Conf.Subscribed)
                {
                    uyt = true;
                }

                MainForm.FfmpegTasks.Enqueue(new FfmpegTask(ffmpeg, args,
                                                            MainForm.Conf.MediaDirectory + "video\\" +
                                                            Camobject.directory + "\\" + TimeLapseVideoFileName +
                                                            ".avi", Camobject.settings.deleteavi, uyt,
                                                            Camobject.settings.youtube.@public, Camobject.id, 2, 0, "", true, 0));
            }
        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        private bool _ptzneedsstop;
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (NeedSizeUpdate && _camera != null && !_camera.LastFrameNull)
            {
                AutoSize = true;
                UpdatePosition();
            }
            else
                AutoSize = false;
            Monitor.Enter(this);
            Graphics gCam = pe.Graphics;
            var grabBrush = new SolidBrush(Color.DarkGray);
            if (Camobject.newrecordingcount > 0)
                grabBrush.Color = Color.Yellow;
            var borderPen = new Pen(grabBrush);
            var drawBrush = new SolidBrush(Color.White);
            var sb = new SolidBrush(MainForm.Conf.VolumeLevelColor.ToColor());
            var sbTs = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            var pline = new Pen(Color.Green, 2);
            var pNav = new Pen(Color.White, 1);
            Bitmap bm = null;
            var recBrush = new SolidBrush(Color.Red);
            string m = "";
            try
            {
                Rectangle rc = ClientRectangle;
                gCam.DrawRectangle(borderPen, 0, 0, rc.Width - 1, rc.Height - 1);
               
                var grabPoints = new[]
                                     {
                                         new Point(rc.Width - 15, rc.Height), new Point(rc.Width, rc.Height - 15),
                                         new Point(rc.Width, rc.Height)
                                     };
                int textpos = rc.Height - 15;


                gCam.FillPolygon(grabBrush, grabPoints);

                if (_camera != null && _camera.IsRunning)
                {
                    //draw detection graph
                    int w = 2 + Convert.ToInt32((Convert.ToDouble(rc.Width - 4)/100.0)*(_camera.MotionLevel*1000));
                    int ax = 2 + Convert.ToInt32((Convert.ToDouble(rc.Width - 4)/100.0)*(_camera.AlarmLevel*1000));

                    grabPoints = new[]
                                     {
                                         new Point(2, rc.Height - 22), new Point(w, rc.Height - 22),
                                         new Point(w, rc.Height - 15), new Point(2, rc.Height - 15)
                                     };

                    gCam.FillPolygon(sb, grabPoints);

                    gCam.DrawLine(pline, new Point(ax, rc.Height - 22), new Point(ax, rc.Height - 15));
                    pline.Dispose();
                }
                bool message = false;

                if (Camobject.settings.active)
                {
                    if (_camera != null && !_camera.LastFrameNull)
                    {
                        m = string.Format("{0:F2}", _camera.FramerateAverage) + " FPS, ";
                        bm = _camera.LastFrame;

                        gCam.DrawImage(bm, rc.X + 1, rc.Y + 1, rc.Width - 2, rc.Height - 26);

                        if (Calibrating)
                        {
                            int remaining = _calibrateTarget - Convert.ToInt32(CalibrateCount);
                            if (remaining < 0) remaining = 0;

                            gCam.DrawString(
                                LocRm.GetString("Calibrating") + " (" + remaining + "): " + Camobject.name,
                                MainForm.Drawfont, drawBrush, new PointF(5, textpos));
                            message = true;
                        }
                        if (Recording)
                        {
                            gCam.FillEllipse(recBrush, new Rectangle(rc.Width - 10, 2, 8, 8));
                        }
                        if (PTZNavigate)
                        {
                            

                            gCam.FillEllipse(sbTs, PTZReference.X - 40, PTZReference.Y - 40, 80, 80);
                            if (Camobject.ptz > -1)
                            {
                                gCam.DrawEllipse(pNav, PTZReference.X - 10, PTZReference.Y - 10, 20, 20);
                                double angle = Math.Atan2(PTZReference.Y - _mouseLoc.Y, PTZReference.X - _mouseLoc.X);

                                var x = PTZReference.X - 30*Math.Cos(angle);
                                var y = PTZReference.Y - 30*Math.Sin(angle);
                                gCam.DrawLine(pNav, PTZReference, new Point((int) x, (int) y));
                                CalibrateCount = 0;
                                Calibrating = true;
                                PTZ.SendPTZDirection(angle);

                            }
                            else
                            {
                                gCam.DrawString(
                                LocRm.GetString("NoPTZcontrol"),
                                MainForm.Drawfont, drawBrush, new PointF(PTZReference.X-37,PTZReference.Y-7));
                            }
                        }
                        if (VideoSourceErrorState)
                            UpdateFloorplans(false);
                        VideoSourceErrorState = false;
                    }
                    else
                    {
                        if (VideoSourceErrorState)
                        {
                            gCam.DrawString(
                                VideoSourceErrorMessage,
                                MainForm.Drawfont, drawBrush, new PointF(5, 5));
                            gCam.DrawString(
                                LocRm.GetString("Error") + ": " + Camobject.name,
                                MainForm.Drawfont, drawBrush, new PointF(5, textpos));
                            message = true;
                        }
                        else
                        {
                            gCam.DrawString(
                                LocRm.GetString("Connecting") + ": " + Camobject.name,
                                MainForm.Drawfont, drawBrush, new PointF(5, textpos));
                            message = true;
                        }
                    }
                }
                else
                {
                    string txt = Camobject.schedule.active ? LocRm.GetString("Scheduled") : LocRm.GetString("Offline");
                    txt += ": " + Camobject.name;

                    gCam.DrawString(txt,MainForm.Drawfont, drawBrush, new PointF(5, 5));
                }

                
                
                if (Camera != null && Camera.MotionDetector != null && !Calibrating &&
                    Camera.MotionDetector.MotionProcessingAlgorithm is BlobCountingObjectsProcessing)
                {
                    var blobcounter =
                        (BlobCountingObjectsProcessing) Camera.MotionDetector.MotionProcessingAlgorithm;

                    m += blobcounter.ObjectsCount + " " + LocRm.GetString("Objects") + ", ";
                        
                    //tracking
                    var pCenter = new Point(Camera.Width / 2, Camera.Height / 2);
                    if (!PTZNavigate && Camobject.settings.ptzautotrack && blobcounter.ObjectsCount > 0 && blobcounter.ObjectsCount < 4 && !_ptzneedsstop)
                    {
                        List<Rectangle> recs =
                            blobcounter.ObjectRectangles.OrderByDescending(p => p.Width*p.Height).ToList();
                        Rectangle rec = recs.First();
                        //get center point
                        var prec = new Point(rec.X + rec.Width/2, rec.Y + rec.Height/2);

                        double dratiomin = 0.6;
                        prec.X = prec.X - pCenter.X;
                        prec.Y = prec.Y - pCenter.Y;

                        if (Camobject.settings.ptzautotrackmode == 1) //vert only
                        {
                            prec.X = 0;
                            dratiomin = 0.3;
                        }

                        if (Camobject.settings.ptzautotrackmode==2) //horiz only
                        {  
                            prec.Y = 0;
                            dratiomin = 0.3;
                        }

                        double angle = Math.Atan2(-prec.Y, -prec.X);
                        double dist = Math.Sqrt(Math.Pow(prec.X, 2.0d) + Math.Pow(prec.Y, 2.0d));

                        double maxdist = Math.Sqrt(Math.Pow(Camera.Width / 2, 2.0d) + Math.Pow(Camera.Height / 2, 2.0d));
                        double dratio = dist / maxdist;

                        if (dratio > dratiomin)
                        {
                            PTZ.SendPTZDirection(angle, 1);
                            _lastAutoTrackSent = DateTime.Now;
                            _ptzneedsstop = true;
                        }
                    }
                       
                }

                if (!message)
                {
                    gCam.DrawString(m + Camobject.name,
                                     MainForm.Drawfont, drawBrush, new PointF(5, textpos));
                }

                _errorTime = DateTime.MinValue;
            }
            catch (Exception e)
            {
                MainForm.LogExceptionToFile(e, "Camera " + Camobject.id);
            }

            borderPen.Dispose();
            grabBrush.Dispose();
            if (bm != null)
                bm.Dispose();
            drawBrush.Dispose();
            recBrush.Dispose();
            sb.Dispose();
            sbTs.Dispose();
            pline.Dispose();
            pNav.Dispose();
            Monitor.Exit(this);

            base.OnPaint(pe);
        }

        private DateTime _lastAutoTrackSent = DateTime.MinValue;

        private void CameraNewFrame(object sender, EventArgs e)
        {
            try
            {
                if (_writerBuffer == null)
                {
                    if (_videoBuffer.Count == Camobject.recorder.bufferframes)
                    {
                        ((FrameAction) _videoBuffer.Dequeue()).Frame.Dispose();
                    }
                    if (_videoBuffer.Count < Camobject.recorder.bufferframes)
                        _videoBuffer.Enqueue(new FrameAction(_camera.LastFrame, _camera.MotionLevel));
                }
                else
                {
                    if (_writerBuffer.Count < Camobject.recorder.bufferframes || _writerBuffer.Count <30)
                        _writerBuffer.Enqueue(new FrameAction(_camera.LastFrame, _camera.MotionLevel));
                }


                Invalidate();
            }
            catch
            {
                
            }
        }

        private void SendFrameGrabThread()
        {
            string subject = LocRm.GetString("FrameGrabSubject").Replace("[OBJECTNAME]", Camobject.name);
            string message = LocRm.GetString("FrameGrabMail").Replace("[OBJECTNAME]", Camobject.name);

            var imageStream = new MemoryStream();
            Image screengrab = null;

            try
            {
                screengrab = Camera.LastFrame;
                screengrab.Save(imageStream, ImageFormat.Jpeg);

                MainForm.WSW.SendFrameGrab(Camobject.settings.emailaddress, subject, message, imageStream.ToArray());
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
            if (screengrab != null)
                screengrab.Dispose();
            imageStream.Dispose();
        }

        public void StartSaving()
        {
            if (Recording || MainForm.ShownWarningMedia || IsEdit)
                return;

            if (MainForm.RecordingThreads >= MainForm.Conf.MaxRecordingThreads)
            {
                MainForm.LogMessageToFile("Skipped recording - maximum recording thread limit hit. See settings to modify the limit.");
                return;
            }
            _recordingThread = new Thread(Record) { Name = "Recording Thread (" + Camobject.id + ")", IsBackground = false, Priority = ThreadPriority.Normal };
            _recordingThread.Start();
        }
       
        private void Record()
        {
            MainForm.RecordingThreads++;
            try {
                _writerBuffer = new QueueWithEvents<FrameAction>();
                _writerBuffer.Changed += WriterBufferChanged;
                string motionData = "";

                DateTime date = DateTime.Now;

                string filename = String.Format("{0}-{1}-{2}_{3}-{4}-{5}",
                                                date.Year, Helper.ZeroPad(date.Month), Helper.ZeroPad(date.Day),
                                                Helper.ZeroPad(date.Hour), Helper.ZeroPad(date.Minute),
                                                Helper.ZeroPad(date.Second));

                if (Camobject.settings.micpair > -1)
                {
                    var vl = VolumeControl;
                    if (vl != null)
                    {
                        if (vl.PairRecord(ForcedRecording))
                        {
                            _pairedRecording = true;
                        }
                        else
                            _pairedRecording = false;
                    }
                }
                else
                    _pairedRecording = false;

                VideoFileName = Camobject.id + "_" + filename;
                string folder = MainForm.Conf.MediaDirectory + "video\\" + Camobject.directory + "\\";
                string avifilename = folder + VideoFileName + ".avi";
                bool error = false;
                double maxAlarm = 0;
                try
                {
                    double d = (int)Math.Round(Camera.FramerateAverage);
                    int fr;
                    try
                    {
                        fr = (int)d;
                    }
                    catch
                    {
                        MainForm.LogErrorToFile("Failed converting " + d + " to int32");
                        fr = 1;
                    }

                    if (fr < 1)
                        fr = 1;
                    if (fr > Camobject.settings.maxframeraterecord)
                        fr = Camobject.settings.maxframeraterecord;

                    _writer = new  AVIWriter { FrameRate = Convert.ToInt32(fr) };
                    string[] compressorOptions = MainForm.Conf.CompressorOptions.Split(',');
                    _writer.Open(avifilename, _camera.Width, _camera.Height, false, ref compressorOptions);

                    Bitmap bmpPreview = Camera.LastFrame;

                    while (_videoBuffer.Count > 0)
                    {
                        var fa = (FrameAction)_videoBuffer.Dequeue();
                        try
                        {
                            _writer.AddFrame(fa.Frame);
                            if (fa.MotionLevel > maxAlarm)
                            {
                                maxAlarm = fa.MotionLevel;
                                if (bmpPreview != null)
                                    bmpPreview.Dispose();
                                bmpPreview = (Bitmap)fa.Frame.Clone();
                            }
                            motionData +=
                                String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}",
                                              Math.Min(fa.MotionLevel * 1000, 100)) + ",";
                        }
                        catch (Exception ex)
                        {
                            MainForm.LogExceptionToFile(ex);
                        }
                        finally
                        {
                            fa.Frame.Dispose();
                        }

                    }

                    while (!_stopWrite)
                    {
                        while (_writerBuffer.Count > 0)
                        {
                            var fa = _writerBuffer.Dequeue();
                            try
                            {
                                _writer.AddFrame(fa.Frame);
                                if (fa.MotionLevel > maxAlarm)
                                {
                                    maxAlarm = fa.MotionLevel;
                                    if (bmpPreview != null)
                                        bmpPreview.Dispose();
                                    bmpPreview = (Bitmap)fa.Frame.Clone();
                                }
                                motionData +=
                                    String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}",
                                                  Math.Min(fa.MotionLevel * 1000, 100)) + ",";
                            }
                            catch (Exception ex)
                            {
                                MainForm.LogExceptionToFile(ex);
                            }
                            finally
                            {
                                fa.Frame.Dispose();
                            }
                        }
                        _newRecordingFrame.WaitOne(1000);
                    }

                    if (!Directory.Exists(folder + @"thumbs\"))
                        Directory.CreateDirectory(folder + @"thumbs\");
                    if (bmpPreview != null)
                    {
                        bmpPreview.Save(folder + @"thumbs\" + VideoFileName + "_large.jpg", ImageFormat.Jpeg);
                        Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
                        var myThumbnail = bmpPreview.GetThumbnailImage(96, 72, myCallback, IntPtr.Zero);
                        myThumbnail.Save(folder + @"thumbs\" + VideoFileName + ".jpg", ImageFormat.Jpeg);
                        bmpPreview.Dispose();
                        myThumbnail.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    MainForm.LogExceptionToFile(ex, "Camera " + Camobject.id);
                }
                finally
                {
                    _stopWrite = false;
                    if (_writer != null)
                    {
                        _writer.Close();
                        _writer.Dispose(); //do actually need this!
                        _writer = null;
                    }

                    try
                    {
                        while (_writerBuffer.Count > 0)
                        {
                            _writerBuffer.Dequeue().Frame.Dispose();
                        }
                    }
                    catch
                    {
                    }

                    _writerBuffer = null;
                    _recordingTime = 0;
                }
                if (error)
                {
                    try
                    {
                        File.Delete(filename + ".avi");
                    }
                    catch
                    {
                    }
                    return;
                }

                string ffmpeg = Camobject.settings.ffmpeg.Trim();
                string path = MainForm.Conf.MediaDirectory + "video\\" + Camobject.directory + "\\" +
                              VideoFileName;

                if (AudioMergeFilename != "")
                {
                    objectsMicrophone om = MainForm.Microphones.SingleOrDefault(p => p.id == Camobject.settings.micpair);
                    if (om != null)
                    {
                        ffmpeg = ffmpeg.Replace(" -an ", " -i \"" + AudioMergeFilename + "\" -ac 1 -async 1 ");
                    }
                    AudioMergeFilename = "";
                }
                bool deleteavi = Camobject.settings.deleteavi;

                filename = "\"" + Program.AppPath + "ffmpeg\\ffmpeg.exe\"";

                string args = ffmpeg;
                if (args != "")
                {
                    int fr = Convert.ToInt32(Camera.FramerateAverage);
                    if (fr == 0)
                        fr = 1;
                    if (fr > Camobject.settings.maxframeraterecord)
                        fr = Camobject.settings.maxframeraterecord;


                    args = args.Replace("{framerate}", fr.ToString());
                    args = args.Replace("{filename}", path);
                    args = args.Replace("{presetdir}", Program.AppPath + @"ffmpeg\presets\");

                    bool yt = false;
                    if (Camobject.settings.youtube.autoupload && MainForm.Conf.Subscribed)
                    {
                        yt = true;
                    }
                    MainForm.FfmpegTasks.Enqueue(new FfmpegTask(filename, args, path + ".avi", deleteavi, yt,
                                                                Camobject.settings.youtube.@public, Camobject.id, 2,
                                                                Math.Min(maxAlarm*1000, 100),
                                                                Helper.GetMotionDataPoints(motionData), false,
                                                                100 - Camobject.detector.sensitivity));
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
            MainForm.RecordingThreads--;
            Camobject.newrecordingcount++;
            if (Notification != null)
                Notification(this, new NotificationType("NewRecording", Camobject.name));
        }

        void WriterBufferChanged(object sender, EventArgs e)
        {
            _newRecordingFrame.Set();
        }

        

        public void CameraAlarm(object sender, EventArgs e)
        {
            MovementDetected = true;
            FlashCounter = 10;
            if (sender is LocalServer)
                _isTrigger = true;
        }

        private void DoAlert()
        {
            Alerted = true;
            UpdateFloorplans(true);
            var t = new Thread(AlertThread) { Name = "Alert (" + Camobject.id + ")", IsBackground = false };
            t.Start();
        }

        private void AlertThread()
        {
            if (IsEdit)
                return;

            if (Notification != null)
                Notification(this, new NotificationType("ALERT_UC", Camobject.name));
            if (Camobject.alerts.executefile != "")
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                                        {
                                            CreateNoWindow = true,
                                            UseShellExecute = true,
                                            FileName = Camobject.alerts.executefile,
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            Arguments = Camobject.alerts.arguments
                                        };


                    Process.Start(startInfo);
                }
                catch (Exception e)
                {
                    MainForm.LogExceptionToFile(e);
                }
            }

            string[] alertOptions = Camobject.alerts.alertoptions.Split(','); //beep,restore
            if (Convert.ToBoolean(alertOptions[0]))
                Console.Beep();
            if (Convert.ToBoolean(alertOptions[1]))
                RemoteCommand(this, new ThreadSafeCommand("show"));

            var imageStream = new MemoryStream();
            Image screengrab = null;

            try
            {
                screengrab = Camera.LastFrame;
                screengrab.Save(imageStream, ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }

            if (screengrab != null && Camobject.ftp.enabled && Camobject.ftp.mode == 1 && Camobject.ftp.ready)
                FtpFrame();

            if (MainForm.Conf.ServicesEnabled && MainForm.Conf.Subscribed)
            {
                //get image array
                if (Camobject.notifications.sendemail)
                {
                    string subject = LocRm.GetString("CameraAlertMailSubject").Replace("[OBJECTNAME]", Camobject.name);
                    string message = LocRm.GetString("CameraAlertMailBody");
                    message = message.Replace("[OBJECTNAME]", Camobject.name);

                    string body = "";
                    switch (Camobject.alerts.mode)
                    {
                        case "face":
                        case "movement":
                            body = LocRm.GetString("CameraAlertBodyMovement").Replace("[TIME]",
                                                                                       DateTime.Now.ToLongTimeString());

                            if (Camobject.detector.recordondetect)
                            {
                                if (!MainForm.ShownWarningMedia)
                                    body += LocRm.GetString("VideoCaptured");
                                else
                                    body += LocRm.GetString("VideoNotCaptured");
                            }
                            else
                                body += LocRm.GetString("VideoNotCaptured");
                            break;
                        case "nomovement":
                            int minutes = Convert.ToInt32(Camobject.detector.nomovementinterval/60);
                            int seconds = (Camobject.detector.nomovementinterval%60);

                            body =
                                LocRm.GetString("CameraAlertBodyNoMovement").Replace("[TIME]",
                                                                                     DateTime.Now.ToLongTimeString()).
                                    Replace("[MINUTES]", minutes.ToString()).Replace("[SECONDS]", seconds.ToString());

                            break;
                    }

                    message = message.Replace("[BODY]", body);

                    MainForm.WSW.SendAlertWithImage(Camobject.settings.emailaddress, subject, message,
                                                    imageStream.ToArray());
                }


                if (Camobject.notifications.sendsms || Camobject.notifications.sendmms)
                {
                    string message = LocRm.GetString("SMSMovementAlert").Replace("[OBJECTNAME]", Camobject.name) + " ";
                    switch (Camobject.alerts.mode)
                    {
                        case "face":
                        case "movement":
                            message += LocRm.GetString("SMSMovementDetected");
                            if (Camobject.detector.recordondetect)
                                message = message.Replace("[RECORDED]", LocRm.GetString("VideoCaptured"));
                            else
                                message = message.Replace("[RECORDED]", LocRm.GetString("VideoNotCaptured"));
                            break;
                        case "nomovement":
                            int minutes = Convert.ToInt32(Camobject.detector.nomovementinterval/60);
                            int seconds = (Camobject.detector.nomovementinterval%60);

                            message +=
                                LocRm.GetString("SMSNoMovementDetected").Replace("[MINUTES]", minutes.ToString()).
                                    Replace("[SECONDS]", seconds.ToString());
                            break;
                    }

                    if (Camobject.notifications.sendmms)
                    {
                        MainForm.WSW.SendMms(Camobject.settings.smsnumber, message, imageStream.ToArray());
                    }
                    else
                    {
                        MainForm.WSW.SendSms(Camobject.settings.smsnumber, message);
                    }
                }

                if (screengrab != null)
                    screengrab.Dispose();

                imageStream.Dispose();
            }
        }

        private void SourceVideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            VideoSourceErrorMessage = eventArgs.Description;
            if (!VideoSourceErrorState)
            {
                VideoSourceErrorState = true;
                MainForm.LogExceptionToFile(new Exception("VideoSourceError: " + eventArgs.Description),
                                            "Camera " + Camobject.id);
                _errorTime = DateTime.Now;
                //Monitor.Enter(this);
                _camera.LastFrameNull = true; 
                //Monitor.Exit(this);
            }
            if (!ShuttingDown)
                Invalidate();
        }

        private void SourcePlayingFinished(object sender, ReasonToFinishPlaying reason)
        {           
            Camobject.settings.active = false;
            Invalidate();
        }

        public void Disable()
        {
            _processing = true;
            Application.DoEvents();

            if (Recording)
            {
                StopSaving();
            }
            if (SavingTimeLapse)
            {
                CloseTimeLapseWriter();
            }
            if (_camera != null && _camera.IsRunning)
            {
                _camera.ClearMotionZones();
                if (_videoBuffer != null)
                {
                    lock (_videoBuffer.SyncRoot)
                    {
                        while (_videoBuffer.Count > 0)
                        {
                            try {((FrameAction) _videoBuffer.Dequeue()).Frame.Dispose();} catch
                            {
                            }
                        }
                    }
                }
                Calibrating = false;
                if (Camera.MotionDetector != null)
                {
                    try
                    {
                        Camera.MotionDetector.Reset();
                    }
                    catch (Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex, "Camera " + Camobject.id);
                    }
                    Camera.MotionDetector = null;
                }
                if (_camera.IsRunning)
                {
                    _camera.VideoSource.PlayingFinished -= SourcePlayingFinished;
                    _camera.VideoSource.VideoSourceError -= SourceVideoSourceError;
                    
                    try
                    {
                        _camera.SignalToStop();
                        if (_camera.VideoSource is VideoCaptureDevice)
                        {
                            int counter = 0;
                            while (_camera.IsRunning && counter < 2)
                            {
                                Thread.Sleep(500);
                                counter++;
                            }
                            if (_camera.IsRunning)
                                _camera.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex, "Camera " + Camobject.id);
                    }
                    
                    
                    if (_camera.VideoSource is XimeaVideoSource)
                    {
                        _camera.VideoSource = XimeaSource = null;
                    }
                }
                try
                {
                    _camera.LastFrameUnmanaged.Dispose();
                    _camera.LastFrameUnmanaged = null;
                }
                catch
                {
                }

                if (_camera.Mask != null)
                {
                    _camera.Mask.Dispose();
                    _camera.Mask = null;
                }
                BackColor = MainForm.Conf.BackColor.ToColor();
            }
            Camobject.settings.active = false;
            _recordingTime = 0;
            InactiveRecord = 0;
            _timeLapseTotal = 0;
            ForcedRecording = false;
            _pairedRecording = false;
            MovementDetected = false;
            Alerted = false;
            FlashCounter = 0;
            ReconnectCount = 0;
            PTZNavigate = false;
            UpdateFloorplans(false);
            MainForm.NeedsSync = true;
            if (LprInited)
            {
                EndOCR();
            }
            if (!ShuttingDown)
                Invalidate();
            _processing = false;
        }

        private void EndOCR()
        {
            // this needs to be in a method to prevent optimiser loading dll regardless
            try
            {
                if (Lpr != null)
                {
                    if (Lpr.OCRInited)
                    {
                        Lpr.OCR.End();
                        Lpr = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
        }

        public void Enable()
        {
            _processing = true;
            
            switch (Camobject.settings.sourceindex)
            {
                case 0:
                    var jpegSource = new JPEGStream(Camobject.settings.videosourcestring);
                    if (Camobject.settings.frameinterval != 0)
                        jpegSource.FrameInterval = Camobject.settings.frameinterval;
                    if (Camobject.settings.login != "")
                    {
                        jpegSource.Login = Camobject.settings.login;
                        jpegSource.Password = Camobject.settings.password;
                    }
                    //jpegSource.SeparateConnectionGroup = true;
                    jpegSource.RequestTimeout = MainForm.Conf.IPCameraTimeout;
                    OpenVideoSource(jpegSource, false);
                    break;
                case 1:
                    var mjpegSource = new MJPEGStream(Camobject.settings.videosourcestring);
                    mjpegSource.Login = Camobject.settings.login;
                    mjpegSource.Password = Camobject.settings.password;
                    mjpegSource.RequestTimeout = MainForm.Conf.IPCameraTimeout;
                    mjpegSource.HttpUserAgent = Camobject.settings.useragent;
                    //mjpegSource.SeparateConnectionGroup = true;
                    OpenVideoSource(mjpegSource, false);
                    break;
                case 2:
                    var fileSource = new AVIFileVideoSource(Camobject.settings.videosourcestring);
                    OpenVideoSource(fileSource, true);
                    break;
                case 3:
                    var videoSource = new VideoCaptureDevice(Camobject.settings.videosourcestring);
                    string[] wh = Camobject.resolution.Split('x');
                    videoSource.DesiredFrameSize = new Size(Convert.ToInt32(wh[0]), Convert.ToInt32(wh[1]));
                    videoSource.DesiredFrameRate = Camobject.settings.framerate;
                    OpenVideoSource(videoSource, true);
                    break;
                case 4:
                    var desktopSource = new DesktopStream(Convert.ToInt32(Camobject.settings.videosourcestring));
                    if (Camobject.settings.frameinterval != 0)
                        desktopSource.FrameInterval = Camobject.settings.frameinterval;
                    desktopSource.ResizeWidth = Camobject.settings.desktopresizewidth;
                    desktopSource.ResizeHeight = Camobject.settings.desktopresizeheight;
                    OpenVideoSource(desktopSource, false);
                    break;
                case 5:
                    string[] inargs = Camobject.settings.vlcargs.Split(Environment.NewLine.ToCharArray(),
                                                                         StringSplitOptions.RemoveEmptyEntries);
                    var vlcSource = new VlcStream(Camobject.settings.videosourcestring, inargs)
                                        {
                                            ResizeWidth = Camobject.settings.desktopresizewidth,
                                            ResizeHeight = Camobject.settings.desktopresizeheight
                                        };
                    //AsyncVideoSource _asv = new AsyncVideoSource(vlcSource);
                    //_asv.SkipFramesIfBusy = true;
                    OpenVideoSource(vlcSource, false);
                    break;
                case 6:
                    if (XimeaSource==null || !XimeaSource.IsRunning)
                        XimeaSource = new XimeaVideoSource(Convert.ToInt32(NV("device")));
                    OpenVideoSource(XimeaSource, false);
                    break;
                case 7:
                    var kinectDevice = Kinect.GetDevice(Convert.ToInt32(NV("device")));
                    kinectDevice.SetLedColor(Enums.Ledmode[Convert.ToInt32(NV("ledmode"))]);
                    kinectDevice.SetMotorTilt(Convert.ToInt32(NV("tilt")));
                  
                    var kinectCamera = kinectDevice.GetVideoCamera( );
                    kinectCamera.CameraMode = (Convert.ToInt32(NV("videomode")) == 0) ? VideoCameraMode.Color :
                        ((Convert.ToInt32(NV("videomode")) == 1) ? VideoCameraMode.Bayer : VideoCameraMode.InfraRed);
                    OpenVideoSource(kinectCamera, false);
                    break;
            }

            

            if (Camera != null)
            {
                Camera.LastFrameNull = true;
                
                IMotionDetector motionDetector = null;
                IMotionProcessing motionProcessor = null;

                switch (Camobject.detector.type)
                {
                    case "Two Frames":
                        motionDetector = new TwoFramesDifferenceDetector(Camobject.settings.suppressnoise);
                        break;
                    case "Custom Frame":
                        motionDetector = new CustomFrameDifferenceDetector(Camobject.settings.suppressnoise,
                                                                            Camobject.detector.keepobjectedges);
                        break;
                    case "Background Modelling":
                        motionDetector = new SimpleBackgroundModelingDetector(Camobject.settings.suppressnoise,
                                                                               Camobject.detector.keepobjectedges);
                        break;
                    case "None":
                        break;
                }

                if (motionDetector != null)
                {
                    switch (Camobject.detector.postprocessor)
                    {
                        case "Grid Processing":
                            motionProcessor = new GridMotionAreaProcessing
                                           {
                                               HighlightColor = ColorTranslator.FromHtml(Camobject.detector.color),
                                               HighlightMotionGrid = Camobject.detector.highlight
                                           };
                            break;
                        case "Object Tracking":
                            motionProcessor = new BlobCountingObjectsProcessing
                                            {
                                                HighlightColor = ColorTranslator.FromHtml(Camobject.detector.color),
                                                HighlightMotionRegions = Camobject.detector.highlight,
                                                MinObjectsHeight = Camobject.detector.minheight,
                                                MinObjectsWidth = Camobject.detector.minwidth
                                            };

                            break;
                        case "Border Highlighting":
                            motionProcessor = new MotionBorderHighlighting
                                            {
                                                HighlightColor = ColorTranslator.FromHtml(Camobject.detector.color)
                                            };
                            break;
                        case "Area Highlighting":
                            motionProcessor = new MotionAreaHighlighting
                                            {
                                                HighlightColor = ColorTranslator.FromHtml(Camobject.detector.color)
                                            };
                            break;
                        case "None":
                            break;
                    }

                    if (Camera.MotionDetector != null)
                    {
                        Camera.MotionDetector.Reset();
                        Camera.MotionDetector = null;
                    }

                    if (motionProcessor == null)
                        Camera.MotionDetector = new MotionDetector(motionDetector);
                    else
                        Camera.MotionDetector = new MotionDetector(motionDetector, motionProcessor);

                    Camera.AlarmLevel = Helper.CalculateSensitivity(Camobject.detector.sensitivity);
                    NeedMotionZones = true;
                }
                else
                {
                    Camera.MotionDetector = null;
                }

                if (!Camera.IsRunning)
                {
                    Calibrating = true;
                    CalibrateCount = 0;
                    _calibrateTarget = Camobject.detector.calibrationdelay;
                    _lastRun = DateTime.Now.Ticks;
                    Camera.Start();
                }
                if (Camera.VideoSource is XimeaVideoSource)
                {
                    //need to set these after the camera starts
                    try
                    {
                        XimeaSource.SetParam(PRM.IMAGE_DATA_FORMAT, IMG_FORMAT.RGB24);
                    }
                    catch (ApplicationException)
                    {
                        XimeaSource.SetParam(PRM.IMAGE_DATA_FORMAT, IMG_FORMAT.MONO8);
                    }
                    XimeaSource.SetParam(CameraParameter.OffsetX, Convert.ToInt32(NV("x")));
                    XimeaSource.SetParam(CameraParameter.OffsetY, Convert.ToInt32(NV("y")));
                    float gain;
                    float.TryParse(NV("gain"), out gain);
                    XimeaSource.SetParam(CameraParameter.Gain, gain);
                    float exp;
                    float.TryParse(NV("exposure"), out exp);
                    XimeaSource.SetParam(CameraParameter.Exposure, exp * 1000);
                    XimeaSource.SetParam(CameraParameter.Downsampling, Convert.ToInt32(NV("downsampling")));
                    XimeaSource.SetParam(CameraParameter.Width, Convert.ToInt32(NV("width")));
                    XimeaSource.SetParam(CameraParameter.Height, Convert.ToInt32(NV("height")));
                    XimeaSource.FrameInterval = (int)(1000.0f / XimeaSource.GetParamFloat(CameraParameter.FramerateMax));
                }

                Camobject.settings.active = true;
                

                if (File.Exists(Camobject.settings.maskimage))
                {
                    Camera.Mask = Image.FromFile(Camobject.settings.maskimage);
                }

                UpdateFloorplans(false);
            }
            _recordingTime = 0;
            _timeLapseTotal = 0;
            InactiveRecord = 0;
            MovementDetected = false;
            VideoSourceErrorState = false;
            VideoSourceErrorMessage = "";
            Alerted = false;
            PTZNavigate = false;
            Camobject.ftp.ready = true;
            _lastRun = DateTime.Now.Ticks;
            MainForm.NeedsSync = true;
            ReconnectCount = 0;
            Invalidate();
            
            _processing = false;
        }

        private string NV(string name)
        {
            if (String.IsNullOrEmpty(Camobject.settings.namevaluesettings))
                return "";
            string[] settings = Camobject.settings.namevaluesettings.Split(',');
            foreach (string s in settings)
            {
                string[] nv = s.Split('=');
                if (nv[0].ToLower().Trim() == name)
                    return nv[1].ToLower();
            }
            return "";
        }

        public void UpdateFloorplans(bool isAlert)
        {
            foreach (
                objectsFloorplan ofp in
                    MainForm.FloorPlans.Where(
                        p => p.objects.@object.Where(q => q.type == "camera" && q.id == Camobject.id).Count() > 0).
                        ToList())
            {
                ofp.needsupdate = true;
                if (isAlert)
                {
                    FloorPlanControl fpc = ((MainForm) TopLevelControl).GetFloorPlan(ofp.id);
                    fpc.LastAlertTimestamp = DateTime.Now.UnixTicks();
                    fpc.LastOid = Camobject.id;
                    fpc.LastOtid = 2;
                }
            }
        }

        public string RecordSwitch(bool Record)
        {

            if (!Camobject.settings.active)
            {
                Enable();
            }

            if (Record)
            {
                ForcedRecording = true;
                StartSaving();
                return "recording," + LocRm.GetString("RecordingStarted");
            }

            if (_pairedRecording)
            {
                VolumeLevel vl = VolumeControl;
                if (vl != null)
                {
                    vl.ForcedRecording = false;
                    vl.StopSaving(); //will trigger stop saving on camera (need mp3 file first to merge)
                }
                else
                    StopSaving();

            }
            else
            {
                StopSaving();
            }
            ForcedRecording = false;
            return "notrecording," + LocRm.GetString("RecordingStopped");            
        }

        public bool PairRecord(string audioFilename, bool forcedRecording)
        {
            if (_pairedRecording && Recording && Camobject.settings.active)
            {
                ForcedRecording = forcedRecording;
                AudioMergeFilename = audioFilename;
                return true;
            }

            if (Recording || _camera == null || _camera.LastFrameUnmanaged == null || AudioMergeFilename != "" || IsEdit)
                return false;

            ForcedRecording = forcedRecording;
            AudioMergeFilename = audioFilename;
            StartSaving();
            return true;
        }

        private void OpenVideoSource(IVideoSource source, bool @override)
        {
            if (!@override && Camera != null && Camera.VideoSource != null && Camera.VideoSource.Source == source.Source)
            {
                return;
            }
            if (Camera != null && Camera.IsRunning)
            {
                Disable();
            }

            Camera = new Camera(source);
            source.PlayingFinished += SourcePlayingFinished;
            source.VideoSourceError += SourceVideoSourceError;
            return;
        }

        public void StopSaving()
        {
            _stopWrite = true;
            if (_recordingThread != null)
                _recordingThread.Join();
        }

        public void ApplySchedule()
        {
            //find most recent schedule entry
            if (!Camobject.schedule.active || Camobject.schedule == null || Camobject.schedule.entries == null ||
                Camobject.schedule.entries.Count() == 0)
                return;

            DateTime dNow = DateTime.Now;
            TimeSpan shortest = TimeSpan.MaxValue;
            objectsCameraScheduleEntry mostrecent = null;
            bool isstart = true;

            for (int index = 0; index < Camobject.schedule.entries.Length; index++)
            {
                objectsCameraScheduleEntry entry = Camobject.schedule.entries[index];
                string[] dows = entry.daysofweek.Split(',');
                foreach (string dayofweek in dows)
                {
                    int dow = Convert.ToInt32(dayofweek);
                    //when did this last fire?
                    if (entry.start.IndexOf("-") == -1)
                    {
                        string[] start = entry.start.Split(':');
                        var dtstart = new DateTime(dNow.Year, dNow.Month, dNow.Day, Convert.ToInt32(start[0]),
                                                    Convert.ToInt32(start[1]), 0);
                        while ((int) dtstart.DayOfWeek != dow || dtstart > dNow)
                            dtstart = dtstart.AddDays(-1);
                        if (dNow - dtstart < shortest)
                        {
                            shortest = dNow - dtstart;
                            mostrecent = entry;
                            isstart = true;
                        }
                    }
                    if (entry.stop.IndexOf("-") == -1)
                    {
                        string[] stop = entry.stop.Split(':');
                        var dtstop = new DateTime(dNow.Year, dNow.Month, dNow.Day, Convert.ToInt32(stop[0]),
                                                   Convert.ToInt32(stop[1]), 0);
                        while ((int)dtstop.DayOfWeek != dow || dtstop > dNow)
                            dtstop = dtstop.AddDays(-1);
                        if (dNow - dtstop < shortest)
                        {
                            shortest = dNow - dtstop;
                            mostrecent = entry;
                            isstart = false;
                        }
                    }
                }
            }
            if (mostrecent != null)
            {
                if (isstart)
                {
                    Camobject.detector.recordondetect = mostrecent.recordondetect;
                    Camobject.detector.recordonalert = mostrecent.recordonalert;
                    Camobject.alerts.active = mostrecent.alerts;
                    if (!Camobject.settings.active)
                        Enable();
                    if (mostrecent.recordonstart)
                    {
                        ForcedRecording = true;
                        StartSaving();
                    }
                }
                else
                {
                    if (Camobject.settings.active)
                        Disable();
                }
            }
        }

        //potential fix for "not enough memory" error (stack fault)
        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.Handle != null)
            {
                BeginInvoke(new EventHandler(HandleOnSizeChanged), this, e);
            }
            base.OnSizeChanged(e);
        }
        private void HandleOnSizeChanged(object sender, EventArgs e)
        {
            base.OnSizeChanged(e);
        }
        //end potential fix


        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CameraWindow
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MinimumSize = new System.Drawing.Size(160, 120);
            this.Size = new System.Drawing.Size(160, 120);
            this.Resize += this.CameraWindowResize;
            this.ResumeLayout(false);
        }

        #endregion

        #region Nested type: FrameAction

        private struct FrameAction
        {
            public readonly Bitmap Frame;
            public readonly double MotionLevel;

            public FrameAction(Bitmap frame, double motionLevel)
            {
                Frame = frame;
                MotionLevel = motionLevel;
            }
        }

        #endregion
    }


    public class ThreadSafeCommand : EventArgs
    {
        public string Command;
        // Constructor
        public ThreadSafeCommand(string command)
        {
            Command = command;
        }
    }

    public class NotificationType : EventArgs
    {
        public int Objectid;
        public int Objecttypeid;
        public string Text;
        public string Type;
        // Constructor
        public NotificationType(string type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}