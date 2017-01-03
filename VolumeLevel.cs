using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Declarations;
using Declarations.Media;
using Declarations.Players;
using g711audio;
using Implementation;
using iSpyApplication.Properties;
using NAudio.Wave;
using PictureBox = AForge.Controls.PictureBox;

namespace iSpyApplication
{
    public sealed partial class VolumeLevel : PictureBox
    {
        #region Private

        private Thread _networkAudioThread;
        private WaveFileWriter _writer;
        public bool Alerted;
        private Queue _audioBuffer;
        private int _audioBufferSize;
        private int _bitsPerSample = 16;
        private const int BytePacket = 400;
        private int _channels = 1;
        private double _intervalCount;
        private long _lastRun = DateTime.Now.Ticks;
        private int _milliCount;
        private double _noSoundCount;
        private int _playBufferSize;
        private bool _processing;
        private double _recordingTime;
        private int _sampleRate = 22050;
        private bool _stopWrite;
        private double _value;
        private QueueWithEvents<AudioAction> _writerBuffer;
        private DateTime _errorTime = DateTime.MinValue;
        private List<FilesFile> _filelist;
        private readonly AutoResetEvent _newRecordingFrame = new AutoResetEvent(false);
        private IMediaPlayerFactory _mFactory;
        private IMedia _mMedia;
        private IAudioPlayer _mPlayer;
        private NamedPipeServerStream _pipeStream;
        private WaveFormat _recordingFormat;
        private SampleAggregator _sampleAggregator;
        private Socket _tcpClient;
        private WaveIn _waveIn;
        private IWavePlayer _waveOut;
        private Thread _recordingThread;  

        #endregion

        #region Public

        #region Delegates

        public delegate void NewDataAvailable(object sender, NewDataAvailableArgs eventArgs);

        public delegate void NotificationEventHandler(object sender, NotificationType e);

        public delegate void RemoteCommandEventHandler(object sender, ThreadSafeCommand e);

        #endregion

        public string AudioFileName = "";
        public Socket BroadcastSocket;
        public Stream BroadcastStream;
        public WaveFileWriter BroadcastWriter;
        public bool CloseStream;
        public Rectangle RestoreRect = Rectangle.Empty;
        public int FlashCounter;
        public bool ForcedRecording;
        public double InactiveRecord;
        public bool IsEdit;
        public bool NoSource;
        public bool PairedRecording;
        public Queue<short> PlayBuffer;
        public bool ResizeParent;
        public bool SoundDetected;

        private bool _paired = false;
        public bool Paired
        {
            get { return _paired; }
        }
        public objectsMicrophone Micobject;
        public double ReconnectCount;
        public double SoundCount;

        public bool Recording
        {
            get
            {
                return _recordingThread != null && _recordingThread.IsAlive;       
            }
        }

        public bool Listening
        {
            get
            {
                if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
                    return true;
                return false;
            }
            set
            {
                if (_waveOut != null)
                {
                    if (value)
                    {
                        _waveOut.Play();
                    }
                    else
                    {
                        _waveOut.Stop();
                    }
                }
            }
        }

        public List<FilesFile> FileList
        {
            get
            {
                if (_filelist != null)
                    return _filelist;
                string dir = MainForm.Conf.MediaDirectory + "audio\\" +
                                                      Micobject.directory + "\\";
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
                    catch (System.Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex);
                    }
                    fs.Close();
                    
                    if (_filelist != null)
                        return _filelist;
                }

                //else build from directory contents
                
                _filelist = new List<FilesFile>();
                var dirinfo = new DirectoryInfo(MainForm.Conf.MediaDirectory + "audio\\" +
                                                      Micobject.directory + "\\");

                var lFi = new List<FileInfo>();
                lFi.AddRange(dirinfo.GetFiles());
                lFi = lFi.FindAll(f => f.Extension.ToLower() == ".wav" || f.Extension.ToLower() == ".fla" || f.Extension.ToLower() == ".mp3");
                lFi = lFi.OrderByDescending(f => f.CreationTime).ToList();
                //sanity check existing data
                foreach (FileInfo fi in lFi)
                {
                    if (_filelist.Where(p => p.Filename == fi.Name).Count()==0)
                    {
                        _filelist.Add(new FilesFile
                        {
                            CreatedDateTicks = fi.CreationTime.Ticks,
                            Filename = fi.Name,
                            SizeBytes = fi.Length,
                            MaxAlarm = 0,
                            AlertData = "0",
                                              DurationSeconds = 0,
                                              IsTimelapse = false
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
            set { lock (_filelist) { _filelist = value; } }
        }


        public void SaveFileList()
        {
            try
            {
                if (_filelist != null)
                    lock (_filelist)
                    {
                        var fl = new Files {File = FileList.ToArray()};
                        string fn = MainForm.Conf.MediaDirectory + "audio\\" +
                                    Micobject.directory + "\\data.xml";
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

        public event NewDataAvailable DataAvailable;

        #endregion

        #region SizingControls

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
            if ((ResizeParent) && (Parent != null) && (Parent.IsHandleCreated))
            {
                hwnd = Parent.Handle;
            }
            if (!Paired)
            {
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
                            NativeCalls.SendMessage(hwnd, NativeCalls.WmSyscommand, NativeCalls.ScDragsizeSe,
                                                    IntPtr.Zero);
                        }
                        break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
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
                var ar = Convert.ToDouble(MinimumSize.Width)/Convert.ToDouble(MinimumSize.Height);
                Width = Convert.ToInt32(ar*Height);
            }

            base.OnResize(eventargs);
            if (Width < MinimumSize.Width) Width = MinimumSize.Width;
            if (Height < MinimumSize.Height) Height = MinimumSize.Height;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Default;
        }

        #region Nested type: MousePos

        private enum MousePos
        {
            NoWhere,
            Right,
            Bottom,
            BottomRight
        }

        #endregion

        #endregion

        public VolumeLevel(objectsMicrophone om)
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
            Margin = new Padding(0, 0, 0, 0);
            Padding = new Padding(0, 0, 5, 5);
            BorderStyle = BorderStyle.None;
            BackColor = MainForm.Conf.BackColor.ToColor();
            Micobject = om;
        }


        [DefaultValue(false)]
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                Invalidate();
            }
        }

        public WaveFormat RecordingFormat
        {
            get { return _recordingFormat; }
            set
            {
                _recordingFormat = value;
                _sampleAggregator.NotificationCount = value.SampleRate/10;
            }
        }

        public string[] ScheduleDetails
        {
            get
            {
                var entries = new List<string>();
                foreach (var sched in Micobject.schedule.entries)
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
                    if (!sched.active)
                        s += " (" + LocRm.GetString("INACTIVE_UC") + ")";

                    entries.Add(s);
                }
                return entries.ToArray();
            }
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
                _milliCount += ts.Milliseconds;
                _lastRun = DateTime.Now.Ticks;

                if (FlashCounter == 5)
                {
                    SoundDetected = false;
                }

                if (FlashCounter > 5)
                    InactiveRecord = 0;

                double secondCount = (_milliCount/1000.0);

                while (_milliCount > 1000)
                    _milliCount -= 1000;

                if (FlashCounter == 1)
                {
                    UpdateFloorplans(false);
                }

                if (FlashCounter > 0)
                    FlashCounter--;

                if (Recording)
                    _recordingTime += Convert.ToDouble(ts.TotalMilliseconds) / 1000.0;

                //if (_recordingTime < Micobject.recorder.maxrecordtime && Recording)
                //{
                //    _recordingTime += Convert.ToDouble(ts.Milliseconds)/1000.0;
                //}

                _paired = MainForm.Cameras.SingleOrDefault(p => p.settings.micpair == Micobject.id) != null;

                if (Micobject.alerts.active && Micobject.settings.active)
                {
                    switch (Micobject.alerts.mode)
                    {
                        case "sound":
                            if (FlashCounter > 0)
                            {
                                BackColor = (BackColor == MainForm.Conf.ActivityColor.ToColor())
                                                ? MainForm.Conf.BackColor.ToColor()
                                                : MainForm.Conf.ActivityColor.ToColor();
                            }
                            else
                                BackColor = MainForm.Conf.BackColor.ToColor();
                            break;
                        case "nosound":
                            if (!SoundDetected)
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

                if (secondCount > 1) //approx every second
                {
                    if (Micobject.settings.active)
                    {
                        if (Micobject.settings.reconnectinterval > 0 && Micobject.settings.typeindex == 2)
                        //vlc only
                        {
                            ReconnectCount += secondCount;
                            if (ReconnectCount > Micobject.settings.reconnectinterval)
                            {
                                _mPlayer.Stop();
                                ReconnectCount = 0;
                                _mPlayer.Play();
                            }

                        }
                    }

                    if (Micobject.settings.notifyondisconnect && MainForm.Conf.Subscribed &&
                        _errorTime != DateTime.MinValue)
                    {
                        int sec = Convert.ToInt32((DateTime.Now - _errorTime).TotalSeconds);
                        if (sec > 10 && sec < 20)
                        {
                            string subject =
                                LocRm.GetString("MicrophoneNotifyDisconnectMailSubject").Replace("[OBJECTNAME]",
                                                                                                 Micobject.name);
                            string message = LocRm.GetString("MicrophoneNotifyDisconnectMailBody");
                            message = message.Replace("[NAME]", Micobject.name);
                            message = message.Replace("[TIME]", DateTime.Now.ToLongTimeString());
                            MainForm.WSW.SendAlert(Micobject.settings.emailaddress, subject, message);
                            _errorTime = DateTime.MinValue;
                        }
                    }

                    if (Recording && (!SoundDetected || !Micobject.detector.recordondetect) && !ForcedRecording)
                    {
                        InactiveRecord += secondCount;

                        if (PairedRecording &&
                            (InactiveRecord >= Micobject.recorder.inactiverecord || !Micobject.detector.recordondetect))
                        {
                            objectsCamera oc =
                                MainForm.Cameras.SingleOrDefault(p => p.settings.micpair == Micobject.id);
                            if (oc != null)
                            {
                                if (TopLevelControl != null)
                                {
                                    CameraWindow cw = ((MainForm) TopLevelControl).GetCameraWindow(oc.id);
                                    if (cw.InactiveRecord > oc.recorder.inactiverecord || !oc.settings.active)
                                    {
                                        StopSaving();
                                        // will trigger close which will write the mp3 file and then close the paired camera
                                    }
                                }
                            }
                        }
                    }

                    DateTime dtnow = DateTime.Now;
                    foreach (objectsMicrophoneScheduleEntry entry in Micobject.schedule.entries.Where(p => p.active))
                    {
                        if (entry.daysofweek.IndexOf(((int) dtnow.DayOfWeek).ToString()) != -1)
                        {
                            string[] stop = entry.stop.Split(':');
                            if (stop[0] != "-")
                            {
                                if (Convert.ToInt32(stop[0]) == dtnow.Hour)
                                {
                                    if (Convert.ToInt32(stop[1]) == dtnow.Minute && dtnow.Second < 30)//<5 so can reactivate if needed between 5 and 10 seconds
                                    {
                                        if (Micobject.settings.active)
                                            Disable();
                                        goto skip;
                                    }
                                }
                            }

                            string[] start = entry.start.Split(':');
                            if (start[0] != "-")
                            {
                                if (Convert.ToInt32(start[0]) == dtnow.Hour)
                                {
                                    if (Convert.ToInt32(start[1]) == dtnow.Minute && dtnow.Second < 59)
                                    {
                                        if (!Micobject.settings.active)
                                            Enable();
                                        Micobject.detector.recordondetect = entry.recordondetect;
                                        Micobject.detector.recordonalert = entry.recordonalert;
                                        Micobject.alerts.active = entry.alerts;
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
                    if (Alerted)
                    {
                        _intervalCount += secondCount;
                        if (_intervalCount > Micobject.alerts.minimuminterval)
                        {
                            Alerted = false;
                            _intervalCount = 0;
                            UpdateFloorplans(false);
                        }
                    }
                    else
                    {
                        switch (Micobject.alerts.mode)
                        {
                            case "sound":
                                if (Micobject.settings.active && Micobject.alerts.active)
                                {
                                    if (SoundDetected)
                                    {
                                        SoundCount += secondCount;
                                        if (!Alerted && SoundCount > Micobject.detector.soundinterval)
                                        {
                                            RemoteCommand(this,
                                                          new ThreadSafeCommand("bringtofrontmic," + Micobject.id));
                                            DoAudioAlert();
                                            SoundCount = 0;
                                            if (Micobject.detector.recordonalert && !Recording)
                                            {
                                                StartSaving();
                                            }
                                        }
                                    }
                                    else
                                        SoundCount = 0;
                                }
                                break;
                            case "nosound":
                                if (Micobject.settings.active && Micobject.alerts.active)
                                {
                                    if (!SoundDetected)
                                    {
                                        _noSoundCount += secondCount;
                                        if (!Alerted && _noSoundCount >= Micobject.detector.nosoundinterval)
                                        {
                                            RemoteCommand(this,
                                                          new ThreadSafeCommand("bringtofrontmic," + Micobject.id));
                                            DoAudioAlert();
                                            _noSoundCount = 0;
                                            if (Micobject.detector.recordonalert && !Recording)
                                            {
                                                StartSaving();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _noSoundCount = 0;
                                    }
                                }
                                break;
                        }
                    }
                    //Check record
                    if (((Micobject.detector.recordondetect && SoundDetected) || ForcedRecording) && !Recording)
                    {
                        StartSaving();
                    }
                    else
                    {
                        if (!_stopWrite && Recording)
                        {
                            if (((_recordingTime > Micobject.recorder.maxrecordtime) ||
                                 ((!SoundDetected && InactiveRecord > Micobject.recorder.inactiverecord) &&
                                  !ForcedRecording && !PairedRecording)))
                                StopSaving();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
            skip:
            _processing = false;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // lock
            Monitor.Enter(this);

            var gMic = pe.Graphics;
            var rc = ClientRectangle;

            var grabPoints = new[]
                                 {
                                     new Point(rc.Width - 15, rc.Height), new Point(rc.Width, rc.Height - 15),
                                     new Point(rc.Width, rc.Height)
                                 };
            var grabBrush = new SolidBrush(Color.DarkGray);
            if (Micobject.newrecordingcount > 0)
                grabBrush.Color = Color.Yellow;
            var borderPen = new Pen(grabBrush);
            var drawBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            var drawPen = new Pen(drawBrush);
            if (!Paired)
                gMic.FillPolygon(grabBrush, grabPoints);

            if (!Paired)
                gMic.DrawRectangle(borderPen, 0, 0, rc.Width - 1, rc.Height - 1);
            else
            {
                gMic.DrawLine(borderPen,0,0,0,rc.Height-1);
                gMic.DrawLine(borderPen, 0, rc.Height-1, rc.Width-1, rc.Height - 1);
                gMic.DrawLine(borderPen, rc.Width-1,rc.Height-1, rc.Width-1, 0);
            }
            if (Micobject.settings.active)
            {
                int drawW = Convert.ToInt32(Convert.ToDouble((rc.Width - 1.0))*(Value/100.0));
                if (drawW < 1)
                    drawW = 1;

                Brush b = new SolidBrush(MainForm.Conf.VolumeLevelColor.ToColor());

                gMic.FillRectangle(b, rc.X + 2, rc.Y + 2, drawW - 4, rc.Height - 20);

                var mx =
                    (float) ((Convert.ToDouble(rc.Width)/100.00)*Convert.ToDouble(Micobject.detector.sensitivity));
                var pline = new Pen(Color.Green, 2);
                gMic.DrawLine(pline, mx, 2, mx, rc.Height - 20);
                pline.Dispose();

                gMic.DrawString(Micobject.name, MainForm.Drawfont, drawBrush, new PointF(5, rc.Height - 18));

                if (Recording)
                {
                    var recBrush = new SolidBrush(Color.Red);
                    gMic.FillEllipse(recBrush, new Rectangle(rc.Width - 14, 2, 8, 8));
                    recBrush.Dispose();
                }
                if (Listening)
                {
                    gMic.DrawLine(drawPen, rc.Width - 13, 13, rc.Width - 8, 18);
                    gMic.DrawLine(drawPen, rc.Width - 8, 18, rc.Width - 13, 23);
                    gMic.DrawLine(drawPen, rc.Width - 10, 13, rc.Width - 5, 18);
                    gMic.DrawLine(drawPen, rc.Width - 5, 18, rc.Width - 10, 23);
                }

                b.Dispose();
            }
            else
            {
                if (NoSource)
                {
                    gMic.DrawString(LocRm.GetString("NoSource") + ": " + Micobject.name,
                                     MainForm.Drawfont, drawBrush, new PointF(5, 5));
                }
                else
                {
                    if (Micobject.schedule.active)
                    {
                        gMic.DrawString(LocRm.GetString("Scheduled") + ": " + Micobject.name,
                                         MainForm.Drawfont, drawBrush, new PointF(5, 5));
                    }
                    else
                    {
                        gMic.DrawString(LocRm.GetString("Inactive") + ": " + Micobject.name,
                                         MainForm.Drawfont, drawBrush, new PointF(5, 5));
                    }
                }
            }
            borderPen.Dispose();
            grabBrush.Dispose();
            drawBrush.Dispose();
            drawPen.Dispose();
            Monitor.Exit(this);

            base.OnPaint(pe);
        }

        public void UpdateLevel(double newLevel)
        {
            if (newLevel != 0)
            {
                //work out percentage
                if (newLevel < 0)
                    newLevel = 0;
                Value = (newLevel)*100;
                if (Value > Micobject.detector.sensitivity)
                {
                    SoundDetected = true;
                    FlashCounter = 10;
                    InactiveRecord = 0;
                }
            }
        }

        private void WaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                if (DataAvailable != null)
                {
                    DataAvailable(this, new NewDataAvailableArgs((byte[]) e.Buffer.Clone()));
                }
                lock (obj)
                {
                    for (int index = 0; index < e.BytesRecorded; index += 2)
                    {
                        var sample = (short) ((e.Buffer[index + 1] << 8) |
                                              e.Buffer[index + 0]);
                        float sample32 = sample/32768f;
                        _sampleAggregator.Add(sample32);
                        if (Listening)
                        {
                            PlayBuffer.Enqueue(sample);
                        }
                    }

                    if (_writer == null || _writerBuffer == null)
                    {
                        if (_audioBuffer.Count >= _audioBufferSize)
                            _audioBuffer.Dequeue();
                        if (_audioBuffer.Count < _audioBufferSize)
                            _audioBuffer.Enqueue(new AudioAction((byte[]) e.Buffer.Clone(), Value));
                    }
                    else
                    {
                        _writerBuffer.Enqueue(new AudioAction((byte[]) e.Buffer.Clone(), Value));
                    }
                }
            }
            catch (System.Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }
        }

        public void StopSaving()
        {
            _stopWrite = true;
        }

        public void StartSaving()
        {
            if (Recording || MainForm.ShownWarningMedia || IsEdit)
                return;

            _recordingThread = new Thread(Record) { Name = "Recording Thread (" + Micobject.id + ")", IsBackground = false, Priority = ThreadPriority.Normal };
            _recordingThread.Start();
        }

        private void Record()
        {
            try
            {
                _writerBuffer = new QueueWithEvents<AudioAction>();
                _writerBuffer.Changed += WriterBufferChanged;
                string soundData = "";

                DateTime date = DateTime.Now;

                string filename = String.Format("{0}-{1}-{2}_{3}-{4}-{5}",
                                                date.Year, Helper.ZeroPad(date.Month), Helper.ZeroPad(date.Day),
                                                Helper.ZeroPad(date.Hour), Helper.ZeroPad(date.Minute),
                                                Helper.ZeroPad(date.Second));


                AudioFileName = Micobject.id + "_" + filename;
                filename = MainForm.Conf.MediaDirectory + "audio\\" + Micobject.directory + "\\";
                filename += AudioFileName;


                objectsCamera oc = MainForm.Cameras.SingleOrDefault(p => p.settings.micpair == Micobject.id);
                CameraWindow cw = null;
                if (oc != null)
                {
                    if (TopLevelControl != null) cw = ((MainForm) TopLevelControl).GetCameraWindow(oc.id);

                    if (cw != null)
                    {
                        PairedRecording = cw.PairRecord(filename + ".mp3", ForcedRecording);
                        if (PairedRecording)
                        {
                            Micobject.recorder.maxrecordtime = cw.Camobject.recorder.maxrecordtime;
                        }
                    }
                    
                }
                _writer = new WaveFileWriter(filename + ".wav", RecordingFormat);
                double maxlevel = 0;
                if (_audioBuffer.Count > 0)
                {
                    while (_audioBuffer.Count > 0)
                    {
                        var b = (AudioAction) _audioBuffer.Dequeue();
                        _writer.WriteData(b.Decoded, 0, b.Decoded.Length);

                        soundData +=
                            String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}", b.SoundLevel) +
                            ",";
                        if (b.SoundLevel > maxlevel)
                            maxlevel = b.SoundLevel;
                    }
                }


                try
                {
                    while (!_stopWrite)
                    {
                        while (_writerBuffer.Count > 0)
                        {
                            var b = _writerBuffer.Dequeue().Decoded;
                            _writer.WriteData(b, 0, b.Length);
                            double d = Value;
                            soundData +=
                                String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}", d) + ",";
                            if (d > maxlevel)
                                maxlevel = d;
                        }
                        _newRecordingFrame.WaitOne(1000);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("VolumeLevel Save Audio Error: " + ex.Message);
                    MainForm.LogExceptionToFile(ex);
                    if (cw != null)
                    {
                        cw.StopSaving();
                    }
                    _writer.Close();
                    _writer.Dispose();
                    _writer = null;
                    return;
                }
                _stopWrite = false;

                string ffmpeg = "\"" + Program.AppPath + "ffmpeg\\ffmpeg.exe\"";

                string args = Micobject.settings.ffmpeg.Trim();
                if (args != "")
                {
                    args = args.Replace("{channels}", RecordingFormat.Channels.ToString());
                    args = args.Replace("{samples}", RecordingFormat.SampleRate.ToString());
                    args = args.Replace("{filename}", filename);

                    MainForm.FfmpegTasks.Enqueue(new FfmpegTask(ffmpeg, args, filename + ".wav",
                                                                Micobject.settings.deletewav, false, false, Micobject.id,
                                                                1, maxlevel, Helper.GetMotionDataPoints(soundData),
                                                                false, Micobject.detector.sensitivity));
                }
                if (cw != null)
                {
                    cw.StopSaving();
                }

                _writer.Close();
                _writer.Dispose();
                _writer = null;

                // NewRecording(this, new EventArgs());
                _recordingTime = 0;
                UpdateFloorplans(false);
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
            }

            Micobject.newrecordingcount++;
            if (Notification != null)
                Notification(this, new NotificationType("NewRecording", Micobject.name));
        }

        void WriterBufferChanged(object sender, EventArgs e)
        {
            _newRecordingFrame.Set();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _processing = true;
                if (_writer != null)
                    _writer.Close();
                if (_waveOut != null)
                    _waveOut.Dispose();
            }
            base.Dispose(disposing);
        }

        private void StopVLCSource()
        {

            if (_mPlayer != null)
            {
                _mPlayer.Stop();
                _mMedia.Dispose();
                _mFactory.Dispose();
                _mPlayer = null;
                _mMedia = null;
                _mFactory = null;
                if (_pipeStream != null)
                {
                    if (_pipeStream.IsConnected)
                        _pipeStream.Disconnect();
                    _pipeStream.Close();
                    _pipeStream = null;
                }
            }
        }
        public void Disable()
        {
            _processing = true;
            StopSaving();

            if (_waveIn != null)
            {
                try
                {
                    _waveIn.StopRecording();
                }
                catch
                {
                }
            }
            StopVLCSource();
            
            //allow operations to complete in other threads
            Thread.Sleep(250);

            if (_audioBuffer != null)
            {
                _audioBuffer.Clear();
            }

            SoundDetected = false;
            ForcedRecording = false;
            Alerted = false;
            NoSource = false;
            FlashCounter = 0;
            _recordingTime = 0;
            Listening = false;
            ReconnectCount = 0;

            UpdateFloorplans(false);
            Micobject.settings.active = false;

            int i = 5;
            while ((_networkAudioThread != null && _networkAudioThread.IsAlive) && i > 0)
            {
                Thread.Sleep(200);
                i--;
            }
            if (_networkAudioThread != null && _networkAudioThread.IsAlive)
                _networkAudioThread.Abort();

            MainForm.NeedsSync = true;
            Invalidate();
            _processing = false;
        }

        public void Enable()
        {
            _processing = true;
            _waveOut = new DirectSoundOut(100);
            _waveOut.Init(new LiveSoundProvider16(this));
            _waveOut.PlaybackStopped += WaveOutPlaybackStopped;

            _sampleAggregator = new SampleAggregator();

            _sampleRate = Micobject.settings.samples;
            _bitsPerSample = Micobject.settings.bits;
            _channels = Micobject.settings.channels;

            RecordingFormat = new WaveFormat(_sampleRate, _bitsPerSample, _channels);
            switch (Micobject.settings.typeindex)
            {
                case 0: //usb
                    _playBufferSize = 4;

                    //local device
                    int i = 0, selind = -1;
                    for (int n = 0; n < WaveIn.DeviceCount; n++)
                    {
                        if (WaveIn.GetCapabilities(n).ProductName == Micobject.settings.sourcename)
                            selind = i;
                        i++;
                    }
                    if (selind == -1)
                    {
                        //device no longer connected
                        Micobject.settings.active = false;
                        NoSource = true;
                        _processing = false;
                        return;
                    }
                    _waveIn = new WaveIn {DeviceNumber = selind, WaveFormat = RecordingFormat};
                    _waveIn.DataAvailable += WaveInDataAvailable;
                    _waveIn.RecordingStopped += WaveInRecordingStopped;
                    
                    int iBuffer = Micobject.settings.buffer;
                    if (iBuffer == 0) iBuffer = 1;

                    _audioBufferSize = iBuffer * (1000 / _waveIn.BufferMilliseconds);


                    try
                    {
                        _waveIn.StartRecording();
                    }
                    catch (Exception ex)
                    {
                        MainForm.LogExceptionToFile(ex);
                        MessageBox.Show(LocRm.GetString("AudioMonitoringError") + ": " + ex.Message,
                                        LocRm.GetString("Error"));
                        _waveOut.Dispose();
                        _sampleAggregator = null;
                        _processing = false;
                        return;
                    }
                    break;
                case 1: //ispy server
                    _playBufferSize = _sampleRate;
                    _audioBufferSize = (_sampleRate/BytePacket)*Micobject.settings.buffer;


                    _networkAudioThread = new Thread(SpyServerListener);
                    _networkAudioThread.Name = "iSpyServer Audio Receiver (" + Micobject.id + ")";
                    _networkAudioThread.Start();
                    break;
                case 2: //VLC listener
                    Micobject.settings.active = true;
                    _playBufferSize = _sampleRate;
                    _audioBufferSize = (_sampleRate/BytePacket)*Micobject.settings.buffer;

                    _pipeStream = new NamedPipeServerStream("VLCMic" + Micobject.directory, PipeDirection.InOut, 1,
                                                           PipeTransmissionMode.Byte, PipeOptions.Asynchronous,
                                                           _audioBufferSize*10, _audioBufferSize*10);
                    AsyncCallback myCallback = AsyncPipeCallback;
                    _pipeStream.BeginWaitForConnection(myCallback, null);

                    _mFactory = new MediaPlayerFactory();
                    _mPlayer = _mFactory.CreatePlayer<IAudioPlayer>();
                    string[] args = Micobject.settings.vlcargs.Trim(',').Split(Environment.NewLine.ToCharArray(),
                                                                                 StringSplitOptions.RemoveEmptyEntries);
                    List<String> inargs = args.ToList();
                    inargs.Add(":sout=#transcode{vcodec=none,acodec=alaw,channels=" + Micobject.settings.channels +
                                ",samplerate=" + Micobject.settings.samples + ",ab=" + Micobject.settings.bits +
                                "}:std{access=file,mux=raw,dst=" + @"\\\.\pipe\VLCMic" + Micobject.directory +
                                "}:Display");
                    inargs.Add(":no-sout-rtp-sap");
                    inargs.Add(":no-sout-standard-sap");
                    inargs.Add(":ttl=1");
                    inargs.Add(":sout-keep");


                    _mMedia = _mFactory.CreateMedia<IMedia>(Micobject.settings.sourcename, inargs.ToArray());
                    _mPlayer.Open(_mMedia);

                    _mPlayer.Play();

                    break;
            }

            _sampleAggregator.MaximumCalculated += SampleAggregatorMaximumCalculated;


            _audioBuffer = new Queue(_audioBufferSize);
            PlayBuffer = new Queue<short>(_playBufferSize);

            SoundDetected = false;
            Alerted = false;
            NoSource = false;
            FlashCounter = 0;
            _recordingTime = 0;
            ReconnectCount = 0;
            Listening = false;

            UpdateFloorplans(false);
            Micobject.settings.active = true;

            MainForm.NeedsSync = true;
            Invalidate();
            _processing = false;
        }

        private void UpdateFloorplans(bool isAlert)
        {
            foreach (
                var ofp in
                    MainForm.FloorPlans.Where(
                        p => p.objects.@object.Where(q => q.type == "microphone" && q.id == Micobject.id).Count() > 0).
                        ToList())
            {
                ofp.needsupdate = true;
                if (isAlert)
                {
                    FloorPlanControl fpc = ((MainForm)TopLevelControl).GetFloorPlan(ofp.id);
                    fpc.LastAlertTimestamp = DateTime.Now.UnixTicks();
                    fpc.LastOid = Micobject.id;
                    fpc.LastOtid = 1;
                }
            }
        }

        private object obj = new object();

        private void AsyncPipeCallback(IAsyncResult result)
        {
            if (_pipeStream != null)
            {
                try
                {
                    _pipeStream.EndWaitForConnection(result);
                    lock (obj)
                    {

                        while (_pipeStream.IsConnected)
                        {

                            var data = new byte[BytePacket];
                            byte[] decoded;
                            int recbytesize = _pipeStream.Read(data, 0, BytePacket);

                            ALawDecoder.ALawDecode(data, out decoded, recbytesize);

                            if (DataAvailable != null)
                            {
                                DataAvailable(this, new NewDataAvailableArgs((byte[]) decoded.Clone()));
                            }

                            if (_writer == null || _writerBuffer == null)
                            {
                                if (_audioBuffer.Count >= _audioBufferSize)
                                    _audioBuffer.Dequeue();
                                if (_audioBuffer.Count < _audioBufferSize)
                                    _audioBuffer.Enqueue(new AudioAction((byte[]) decoded.Clone(), Value));
                            }
                            else
                            {
                                _writerBuffer.Enqueue(new AudioAction((byte[]) decoded.Clone(), Value));
                            }

                            for (int index = 0; index < decoded.Length; index += 2)
                            {
                                var sample = (short) ((decoded[index + 1] << 8) |
                                                      decoded[index + 0]);
                                float sample32 = sample/32768f;
                                _sampleAggregator.Add(sample32);
                                if (Listening)
                                {
                                    if (PlayBuffer.Count < _playBufferSize)
                                        PlayBuffer.Enqueue(sample);
                                }
                            }
                            _errorTime = DateTime.MinValue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                }

                if (Micobject.settings.active)
                {
                    try
                    {
                        AsyncCallback myCallback = AsyncPipeCallback;

                        _pipeStream.BeginWaitForConnection(myCallback, null);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #region Nested type: AudioAction

        private struct AudioAction
        {
            public readonly byte[] Decoded;
            public readonly double SoundLevel;

            public AudioAction(byte[] decoded, double soundLevel)
            {
                Decoded = decoded;
                SoundLevel = soundLevel;
            }
        }

        #endregion

        private void WaveOutPlaybackStopped(object sender, EventArgs e)
        {
            PlayBuffer.Clear();
        }

        private void SpyServerListener()
        {
            var url = new Uri(Micobject.settings.sourcename);
            _tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpClient.ReceiveBufferSize = _sampleRate;
            try
            {
                _tcpClient.Connect(new IPEndPoint(IPAddress.Parse(url.Host), url.Port));
                _tcpClient.Blocking = true;
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
                NoSource = true;
                Disable();
                return;
            }

            var data = new byte[BytePacket];
            byte[] decoded;

            try
            {
                while (Micobject.settings.active)
                {
                    if (_tcpClient.Available > 0)
                    {
                        int recbytesize = _tcpClient.Receive(data);
                        ALawDecoder.ALawDecode(data, out decoded, recbytesize);

                        if (_writer == null || _writerBuffer == null)
                        {
                            if (_audioBuffer.Count >= _audioBufferSize)
                                _audioBuffer.Dequeue();
                            if (_audioBuffer.Count < _audioBufferSize)
                                _audioBuffer.Enqueue(new AudioAction((byte[])decoded.Clone(), Value));
                        }
                        else
                        {
                            _writerBuffer.Enqueue(new AudioAction((byte[])decoded.Clone(), Value));
                        }

                        if (DataAvailable != null)
                        {
                            DataAvailable(this, new NewDataAvailableArgs((byte[]) decoded.Clone()));
                        }

                        for (int index = 0; index < decoded.Length; index += 2)
                        {
                            var sample = (short) ((decoded[index + 1] << 8) |
                                                  decoded[index + 0]);
                            float sample32 = sample/32768f;
                            _sampleAggregator.Add(sample32);
                            if (Listening)
                            {
                                if (PlayBuffer.Count < _playBufferSize)
                                    PlayBuffer.Enqueue(sample);
                            }
                        }
                        _errorTime = DateTime.MinValue;
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                MainForm.LogExceptionToFile(e);
                _errorTime = DateTime.Now;
            }
            try
            {
                _tcpClient.Shutdown(SocketShutdown.Receive);
            }
            catch
            {
            }
            _tcpClient.Close();
            _tcpClient = null;
        }

        private void WaveInRecordingStopped(object sender, EventArgs e)
        {
            Micobject.settings.active = false;
            if (_waveIn != null)
            {
                _waveIn.Dispose();
                _waveIn = null;
            }
        }

        private void SampleAggregatorMaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            UpdateLevel(Math.Max(e.MaxSample, Math.Abs(e.MinSample)));
        }

        public void DoAudioAlert()
        {
            Alerted = true;
            UpdateFloorplans(true);

            var t = new Thread(AlertThread) { Name = "Alert (" + Micobject.id + ")", IsBackground = false };
            t.Start();
        }

        private void AlertThread()
        {
            if (Notification != null)
                Notification(this, new NotificationType("ALERT_UC", Micobject.name));

            if (Micobject.alerts.executefile.Trim() != "")
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        FileName = Micobject.alerts.executefile,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = Micobject.alerts.arguments
                    };
                    Process.Start(startInfo);
                }
                catch (Exception e)
                {
                    MainForm.LogExceptionToFile(e);
                }
            }

            string[] alertOptions = Micobject.alerts.alertoptions.Split(','); //beep,restore
            if (Convert.ToBoolean(alertOptions[0]))
                Console.Beep();
            if (Convert.ToBoolean(alertOptions[1]))
                RemoteCommand(this, new ThreadSafeCommand("show"));

            if (MainForm.Conf.ServicesEnabled && MainForm.Conf.Subscribed)
            {
                if (Micobject.notifications.sendemail)
                {
                    string subject = LocRm.GetString("MicrophoneAlertMailSubject").Replace("[OBJECTNAME]",
                                                                                            Micobject.name);
                    string message = LocRm.GetString("MicrophoneAlertMailBody");
                    message = message.Replace("[OBJECTNAME]", Micobject.name);

                    string body = "";
                    switch (Micobject.alerts.mode)
                    {
                        case "sound":
                            body = LocRm.GetString("MicrophoneAlertBodySound").Replace("[TIME]",
                                                                                        DateTime.Now.ToLongTimeString());

                            if (Micobject.detector.recordondetect)
                            {
                                if (!MainForm.ShownWarningMedia)
                                    body += LocRm.GetString("AudioCaptured");
                                else
                                    body += LocRm.GetString("AudioNotCaptured");
                            }
                            else
                                body += LocRm.GetString("AudioNotCaptured");
                            break;
                        case "nosound":

                            int minutes = Convert.ToInt32(Micobject.detector.nosoundinterval/60);
                            int seconds = (Micobject.detector.nosoundinterval%60);

                            body =
                                LocRm.GetString("MicrophoneAlertBodyNoSound").Replace("[TIME]",
                                                                                      DateTime.Now.ToLongTimeString()).
                                    Replace("[MINUTES]", minutes.ToString()).Replace("[SECONDS]", seconds.ToString());
                            break;
                    }

                    message = message.Replace("[BODY]", body);

                    MainForm.WSW.SendAlert(Micobject.settings.emailaddress, subject, message);
                }

                if (Micobject.notifications.sendsms)
                {
                    string message = LocRm.GetString("SMSAudioAlert").Replace("[OBJECTNAME]", Micobject.name) + " ";
                    switch (Micobject.alerts.mode)
                    {
                        case "sound":
                            message += LocRm.GetString("SMSAudioDetected");
                            message = message.Replace("[RECORDED]", Micobject.detector.recordondetect ? LocRm.GetString("AudioCaptured") : LocRm.GetString("AudioNotCaptured"));
                            break;
                        case "nosound":
                            int minutes = Convert.ToInt32(Micobject.detector.nosoundinterval/60);
                            int seconds = (Micobject.detector.nosoundinterval%60);

                            message +=
                                LocRm.GetString("SMSNoAudioDetected").Replace("[MINUTES]", minutes.ToString()).Replace(
                                    "[SECONDS]", seconds.ToString());
                            break;
                    }

                    MainForm.WSW.SendSms(Micobject.settings.smsnumber, message);
                }
            }
        }

        private void VolumeLevelFinal_Resize(object sender, EventArgs e)
        {
        }


        public string RecordSwitch(bool Record)
        {
            if (!Micobject.settings.active)
            {
                Enable();
            }

            if (Record)
            {
                ForcedRecording = true;
                StartSaving();
                return "recording," + LocRm.GetString("RecordingStarted");
            }
           
            if (PairedRecording)
            {
                objectsCamera oc = MainForm.Cameras.SingleOrDefault(p => p.settings.micpair == Micobject.id);
                CameraWindow cw = null;
                if (oc != null)
                {
                    if (TopLevelControl != null) cw = ((MainForm) TopLevelControl).GetCameraWindow(oc.id);

                    if (cw != null)
                    {
                        cw.ForcedRecording = false;
                    }
                }
            }
            ForcedRecording = false;
            StopSaving();
            return "notrecording," + LocRm.GetString("RecordingStopped");
        }

        public bool PairRecord(bool forcedRecording)
        {
            if (PairedRecording && Recording && Micobject.settings.active)
            {
                ForcedRecording = forcedRecording;
                return true;
            }
            if (!Micobject.settings.active || Recording || IsEdit)
                return false;

            PairedRecording = true;
            ForcedRecording = forcedRecording;
            StartSaving();

            return true;
        }

        private void VolumeLevel_MouseDown(object sender, MouseEventArgs e)
        {
        }

        public void ApplySchedule()
        {
            if (!Micobject.schedule.active || Micobject.schedule == null || Micobject.schedule.entries == null ||
                Micobject.schedule.entries.Count() == 0)
                return;
            //find most recent schedule entry
            DateTime dNow = DateTime.Now;
            TimeSpan shortest = TimeSpan.MaxValue;
            objectsMicrophoneScheduleEntry mostrecent = null;
            bool isstart = true;

            foreach (objectsMicrophoneScheduleEntry entry in Micobject.schedule.entries)
            {
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
                        while ((int) dtstop.DayOfWeek != dow || dtstop > dNow)
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
                    Micobject.detector.recordondetect = mostrecent.recordondetect;
                    Micobject.detector.recordonalert = mostrecent.recordonalert;
                    Micobject.alerts.active = mostrecent.alerts;
                    if (!Micobject.settings.active)
                        Enable();
                    if (mostrecent.recordonstart)
                    {
                        ForcedRecording = true;
                        StartSaving();
                    }
                }
                else
                {
                    if (Micobject.settings.active)
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

        #region Nested type: ThreadSafeCommand

        public class ThreadSafeCommand : EventArgs
        {
            public string Command;
            // Constructor
            public ThreadSafeCommand(string command)
            {
                Command = command;
            }
        }

        #endregion

        #region Nested type: VolumeChangedEventArgs

        public class VolumeChangedEventArgs : EventArgs
        {
            public int NewLevel;

            public VolumeChangedEventArgs(int newLevel)
            {
                NewLevel = newLevel;
            }
        }

        #endregion
    }

    public class SampleAggregator
    {
        // volume
        private int _count;
        public float MaxValue;
        public float MinValue;
        public int NotificationCount { get; set; }
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        public event EventHandler Restart = delegate { };

        public void RaiseRestart()
        {
            Restart(this, EventArgs.Empty);
        }

        private void Reset()
        {
            _count = 0;
            MaxValue = MinValue = 0;
        }

        public void Add(float value)
        {
            MaxValue = Math.Max(MaxValue, value);
            MinValue = Math.Min(MinValue, value);
            _count++;
            if (_count >= NotificationCount && NotificationCount > 0)
            {
                if (MaximumCalculated != null)
                {
                    MaximumCalculated(this, new MaxSampleEventArgs(MinValue, MaxValue));
                }
                Reset();
            }
        }
    }

    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            MaxSample = maxValue;
            MinSample = minValue;
        }

        public float MaxSample { get; private set; }
        public float MinSample { get; private set; }
    }

    public class LiveSoundProvider16 : WaveProvider16
    {
        private readonly VolumeLevel _source;
        private int _sample;

        public LiveSoundProvider16(VolumeLevel source)
        {
            SetWaveFormat(22050, 1);
            _source = source;
        }


        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            _sample = 0;
            for (int n = 0; n < sampleCount; n++)
            {
                if (_source.PlayBuffer.Count > 0)
                    buffer[n + offset] = _source.PlayBuffer.Dequeue();
                else
                    buffer[n + offset] = 0;
                _sample++;
            }
            return _sample;
        }
    }

    public class NewDataAvailableArgs : EventArgs
    {
        private readonly byte[] _decodedData;

        public NewDataAvailableArgs(byte[] decodedData)
        {
            _decodedData = decodedData;
        }

        public byte[] DecodedData
        {
            get { return _decodedData; }
        }
    }
}