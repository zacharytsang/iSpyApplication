using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Serialization;
using AForge.Video.DirectShow;
using C2BP;
using CodeVendor.Controls;
using iSpyApplication.Properties;
using Renderers;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using PictureBox = AForge.Controls.PictureBox;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using Timer = System.Timers.Timer;

namespace iSpyApplication
{
    /// <summary>
    /// Summary description for MainForm
    /// </summary>
    public class MainForm : Form
    {
        #region Delegates

        public delegate void UpdateLevelHandler(int newLevel);

        #endregion

        public static string Website = "http://www.ispyconnect.com";
        public static bool NeedsSync;
        public static bool LoopBack;
        public static bool ShownWarningMedia;
        public static string NL = Environment.NewLine;
        public static Font Drawfont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel);
        public static string NextLog = "";
        public static string Identifier;
        public static string EmailAddress = "", MobileNumber = "";
        public static FilterInfoCollection VideoFilters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        public static Queue<FfmpegTask> FfmpegTasks = new Queue<FfmpegTask>();
        public static bool FfmpegTaskProcessing;
        public static configuration conf;
        public static double ThrottleFramerate = 40;
        public static int CpuUsage;
        public static int RecordingThreads = 0;
        public object ContextTarget;
        public bool SilentStartup;
        internal static LocalServer MWS;
        internal static WsWrapper WSW = new WsWrapper();

        private static bool _logging;
        private static string _counters = "";
        private static readonly Random Random = new Random();
        private static int _pingCounter;
        private static readonly StringBuilder LogFile = new StringBuilder();
        public static bool NeedsRedraw = false;

        private static readonly string LogTemplate =
            "<html><head><title>iSpy v<!--VERSION--> Log File</title><style type=\"text/css\">body,td,th,div {font-family:Verdana;font-size:10px}</style></head><body><h1>Log Start (v<!--VERSION-->): " +
            DateTime.Now.ToLongDateString() +
            "</h1><p><table cellpadding=\"2px\"><!--CONTENT--></table></p></body></html>";

        private static string _lastlog = "";

        private static List<objectsMicrophone> _microphones;


        private static List<objectsFloorplan> _floorplans;
        private static List<objectsCommand> _remotecommands;
        private static List<objectsCamera> _cameras;
        private static List<PTZSettingsCamera> _ptzs;
        private readonly PerformanceCounter _cpuCounter;
        private static IPAddress[] _ipv4Addresses, _ipv6Addresses;
        private Timer _houseKeepingTimer;
        private bool _shuttingDown;        
        private string _startCommand = "";
        private Timer _updateTimer;
        private bool _cleaningFiles;
        private bool _closing;
        private FileSystemWatcher _fsw;
        private WebBrowser _wbGettingStarted;
        private MenuItem _aboutHelpItem;
        private ToolStripMenuItem _activateToolStripMenuItem;
        private ToolStripMenuItem _addCameraToolStripMenuItem;
        private ToolStripMenuItem _addFloorPlanToolStripMenuItem;
        private ToolStripMenuItem _addMicrophoneToolStripMenuItem;
        private ToolStripMenuItem _applyScheduleToolStripMenuItem;
        private ToolStripMenuItem _applyScheduleToolStripMenuItem1;
        private ToolStripMenuItem _autoLayoutToolStripMenuItem;
        private Button _btnOk;
        private CheckBox _chkShowGettingStarted;
        private IContainer components;
        private ContextMenuStrip _ctxtMainForm;
        private ContextMenuStrip _ctxtMnu;
        private ContextMenuStrip _ctxtTaskbar;
        private ComboBox _ddlLanguage;
        private ToolStripMenuItem _deleteToolStripMenuItem;
        private ToolStripMenuItem _editToolStripMenuItem;
        private MenuItem _exitFileItem;
        private ToolStripMenuItem _exitToolStripMenuItem;
        private MenuItem _fileItem;
        private ToolStripMenuItem _fileMenuToolStripMenuItem;
        private ToolStripMenuItem _floorPlanToolStripMenuItem;
        private ToolStripMenuItem _fullScreenToolStripMenuItem;
        private MenuItem _helpItem;
        private ToolStripMenuItem _helpToolstripMenuItem;
        private ToolStripMenuItem _iPCameraToolStripMenuItem;
        private ToolStripMenuItem _listenToolStripMenuItem;
        private ToolStripMenuItem _localCameraToolStripMenuItem;
        private PersistWindowState _mWindowState;
        private MainMenu _mainMenu;
        private MenuItem _menuItem1;
        private MenuItem _menuItem10;
        private MenuItem _menuItem11;
        private MenuItem _menuItem12;
        private MenuItem _menuItem13;
        private MenuItem _menuItem14;
        private MenuItem _menuItem15;
        private MenuItem _menuItem16;
        private MenuItem _menuItem17;
        private MenuItem _menuItem18;
        private MenuItem _menuItem19;
        private MenuItem _menuItem2;
        private MenuItem _menuItem20;
        private MenuItem _menuItem21;
        private MenuItem _menuItem22;
        private MenuItem _menuItem23;
        private MenuItem _menuItem24;
        private MenuItem _menuItem25;
        private MenuItem _menuItem26;
        private MenuItem _menuItem27;
        private MenuItem _menuItem28;
        private MenuItem _menuItem29;
        private MenuItem _menuItem3;
        private MenuItem _menuItem30;
        private MenuItem _menuItem31;
        private MenuItem _menuItem32;
        private MenuItem _menuItem33;
        private MenuItem _menuItem34;
        private MenuItem _menuItem35;
        private MenuItem _menuItem36;
        private MenuItem _menuItem37;
        private MenuItem _menuItem38;
        private MenuItem _menuItem39;
        private MenuItem _menuItem4;
        private MenuItem _menuItem5;
        private MenuItem _menuItem6;
        private MenuItem _menuItem7;
        private MenuItem _menuItem8;
        private MenuItem _menuItem9;
        private MenuItem _miApplySchedule;
        private MenuItem _miOffAll;
        private MenuItem _miOffSched;
        private MenuItem _miOnAll;
        private MenuItem _miOnSched;
        private ToolStripMenuItem _microphoneToolStripMenuItem;
        private NotifyIcon _notifyIcon1;
        private ToolStripMenuItem _onMobileDevicesToolStripMenuItem;
        private ToolStripMenuItem _opacityToolStripMenuItem;
        private ToolStripMenuItem _opacityToolStripMenuItem1;
        private ToolStripMenuItem _opacityToolStripMenuItem2;
        private Grouper _pnlGettingStarted;
        private ToolStripMenuItem _positionToolStripMenuItem;
        private ToolStripMenuItem _recordNowToolStripMenuItem;
        private ToolStripMenuItem _remoteCommandsToolStripMenuItem;
        private ToolStripMenuItem _resetRecordingCounterToolStripMenuItem;
        private ToolStripMenuItem _resetSizeToolStripMenuItem;
        private ToolStripMenuItem _setInactiveToolStripMenuItem;
        private ToolStripMenuItem _settingsToolStripMenuItem;
        private ToolStripMenuItem _showFilesToolStripMenuItem;
        private ToolStripMenuItem _showISpy100PercentOpacityToolStripMenuItem;
        private ToolStripMenuItem _showISpy10PercentOpacityToolStripMenuItem;
        private ToolStripMenuItem _showISpy30OpacityToolStripMenuItem;
        private ToolStripMenuItem _showToolstripMenuItem;
        private ToolStripMenuItem _statusBarToolStripMenuItem;
        private StatusStrip _statusStrip1;
        private ToolStripMenuItem _switchAllOffToolStripMenuItem;
        private ToolStripMenuItem _switchAllOnToolStripMenuItem;
        private ToolStripMenuItem _takePhotoToolStripMenuItem;
        private ToolStripMenuItem _thruWebsiteToolStripMenuItem;
        private System.Windows.Forms.Timer _tmrStartup;
        private ToolStrip _toolStrip1;
        private ToolStripButton _toolStripButton1;
        private ToolStripButton _toolStripButton4;
        private ToolStripButton _toolStripButton8;
        private ToolStripDropDownButton _toolStripDropDownButton1;
        private ToolStripDropDownButton _toolStripDropDownButton2;
        private ToolStripMenuItem _toolStripMenuItem1;
        private ToolStripMenuItem _toolStripToolStripMenuItem;
        private ToolStripStatusLabel _tsslStats;
        private ToolStripMenuItem _uSbCamerasAndMicrophonesOnOtherToolStripMenuItem;
        private ToolStripMenuItem _unlockToolstripMenuItem;
        private ToolStripMenuItem _vLcMicrophoneSourceToolStripMenuItem;
        private ToolStripMenuItem _vLcSourceToolStripMenuItem;
        private ToolStripMenuItem _viewMediaOnAMobileDeviceToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem fullScreenToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private MenuItem mnuSaveLayout;
        private MenuItem mnuResetLayout;
        private ToolStripMenuItem _websiteToolstripMenuItem;
        private ToolStripMenuItem saveLayoutToolStripMenuItem;
        private ToolStripMenuItem resetLayoutToolStripMenuItem;
        private ToolStripMenuItem pTZToolStripMenuItem;
        private ToolStripStatusLabel tsslMonitor;
        private ToolStripStatusLabel tsslPerformance;
        private ToolStripMenuItem inExplorerToolStripMenuItem;
        private MenuItem menuItem1;
        private Layout _pnlCameras;
        private static List<LayoutItem> SavedLayout = new List<LayoutItem>();

        public MainForm(bool silent, string command)
        {
            SilentStartup = silent;
            if (Conf.Enable_Password_Protect)
                SilentStartup = true;

            if (!SilentStartup)
            {
                _mWindowState = new PersistWindowState {Parent = this, RegistryPath = @"Software\ispy\startup"};
                // set registry path in HKEY_CURRENT_USER
            }
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            RenderResources();
            
            _cpuCounter = new PerformanceCounter
                             {
                                 CategoryName = "Processor",
                                 CounterName = "% Processor Time",
                                 InstanceName = "_Total",

                             };

            _startCommand = command;

            _toolStrip1.Renderer = new WindowsVistaRenderer();
            _pnlCameras.BackColor = Conf.MainColor.ToColor();

            if (SilentStartup)
            {
                ShowInTaskbar = false;
                ShowIcon = false;
                WindowState = FormWindowState.Minimized;
            }
        }

        public static configuration Conf
        {
            get
            {
                if (conf != null)
                    return conf;
                var s = new XmlSerializer(typeof (configuration));
                var fs = new FileStream(Program.AppDataPath + @"XML\config.xml", FileMode.Open);
                TextReader reader = new StreamReader(fs);

                fs.Position = 0;
                conf = (configuration)s.Deserialize(reader);
                fs.Close();
                if (conf.CPUMax == 0)
                    conf.CPUMax = 90;
                if (conf.MaxRecordingThreads == 0)
                    conf.MaxRecordingThreads = 4;
                return conf;
            }
        }

        public static List<objectsCamera> Cameras
        {
            get
            {
                if (_cameras == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _cameras;
            }
            set { _cameras = value; }
        }

        public static List<PTZSettingsCamera> PTZs
        {
            get
            {
                if (_ptzs == null)
                {
                    LoadPTZs(Program.AppDataPath + @"XML\ptz.xml");
                }
                return _ptzs;
            }
            set { _ptzs = value; }
        }

        public static List<objectsMicrophone> Microphones
        {
            get
            {
                if (_microphones == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _microphones;
            }
            set { _microphones = value; }
        }

        public static List<objectsCommand> RemoteCommands
        {
            get
            {
                if (_remotecommands == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _remotecommands;
            }
            set { _remotecommands = value; }
        }

        public static List<objectsFloorplan> FloorPlans
        {
            get
            {
                if (_floorplans == null)
                {
                    LoadObjects(Program.AppDataPath + @"XML\objects.xml");
                }
                return _floorplans;
            }
            set { _floorplans = value; }
        }

        public static int MediaDirectorySizeMB
        {
            get
            {
                string[] a = Directory.GetFiles(Conf.MediaDirectory, "*.*",
                                                SearchOption.AllDirectories);
                long b = 0;
                foreach (string name in a)
                {
                    try
                    {
                        var info = new FileInfo(name);
                        b += info.Length;
                    }
                    catch
                    {
                        //file may have been deleted
                    }
                }
                return (int) b/1048576;
            }
        }

        public static IPAddress[] AddressListIPv4
        {
            get
            {
                if (_ipv4Addresses != null)
                    return _ipv4Addresses;
                _ipv4Addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                return _ipv4Addresses;
            }
        }

        //IPv6
        public static IPAddress[] AddressListIPv6
        {
            get
            {
                if (_ipv6Addresses != null)
                    return _ipv6Addresses;
                var _ipv6adds = new List<IPAddress>();
                var addressInfoCollection = IPGlobalProperties.GetIPGlobalProperties().GetUnicastAddresses();

                foreach (var addressInfo in addressInfoCollection)
                {
                    if (addressInfo.Address.IsIPv6Teredo || (addressInfo.Address.AddressFamily == AddressFamily.InterNetworkV6 && (!addressInfo.Address.IsIPv6LinkLocal && !addressInfo.Address.IsIPv6SiteLocal)))
                    {
                        if (!System.Net.IPAddress.IsLoopback(addressInfo.Address))
                        {
                            _ipv6adds.Add(addressInfo.Address);
                        }
                    }
                }
                _ipv6Addresses = _ipv6adds.ToArray();
                return _ipv6Addresses;
            }
        }

        private static string _ipv4Address = "";
        public static string AddressIPv4
        {
            get
            {
                if (_ipv4Address != "")
                    return _ipv4Address;

                string detectip = "";
                foreach (IPAddress ip in AddressListIPv4)
                {
                    if (detectip == "")
                        detectip = ip.ToString();

                    if (Conf.IPv4Address == ip.ToString())
                    {
                        _ipv4Address = ip.ToString();
                        break;
                    }

                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {

                        if (!System.Net.IPAddress.IsLoopback(ip))
                        {
                            if (detectip=="")
                                detectip = ip.ToString();
                        }
                    }
                }
                if (_ipv4Address == "")
                    _ipv4Address = detectip;
                Conf.IPv4Address = _ipv4Address;

                return _ipv4Address;
            }
            set { _ipv4Address = value; }
        }

        //IPv6
        private static string _ipv6Address = "";
        public static string AddressIPv6
        {
            get
            {
                if (_ipv6Address != "")
                    return _ipv6Address;

                string detectip = "";
                foreach (IPAddress ip in AddressListIPv6)
                {
                    if (detectip == "")
                        detectip = ip.ToString();

                    if (Conf.IPv6Address == ip.ToString())
                    {
                        _ipv6Address = ip.ToString();
                        break;
                    }

                    if (ip.IsIPv6Teredo)
                    {
                         detectip = ip.ToString();
                    }
                }

                if (_ipv6Address == "")
                    _ipv6Address = detectip;
                Conf.IPv6Address = _ipv6Address;

                return _ipv6Address;

            }
            set { _ipv6Address = value; }
        }

        public static string IPAddress
        {
            get
            {
                if (Conf.IPMode == "IPv4")
                    return AddressIPv4;
                return MakeIPv6Url(AddressIPv6);
            }
        }

        public static string IPAddressExternal
        {
            get
            {
                if (Conf.IPMode == "IPv4")
                    return WSW.ExternalIPv4(false);
                return MakeIPv6Url(AddressIPv6);
            }
        }
        private static string MakeIPv6Url(string IP)
        {
            //strip scope id
            if (IP.IndexOf("%") != -1)
                IP = IP.Substring(0, IP.IndexOf("%"));
            return "[" + IP + "]";
        }

        private static void LoadObjects(string path)
        {
            objects c;
            var s = new XmlSerializer(typeof (objects));
            var fs = new FileStream(path, FileMode.Open);
            TextReader reader = new StreamReader(fs);
            try
            {
                fs.Position = 0;
                c = (objects) s.Deserialize(reader);
                fs.Close();

                _cameras = c.cameras != null ? c.cameras.ToList() : new List<objectsCamera>();
                for (int index = 0; index < _cameras.Count; index++)
                {
                    objectsCamera oc = _cameras[index];
                    //fix for uppercase mode
                    oc.alerts.mode = oc.alerts.mode.ToLower();
                    int rw = oc.settings.desktopresizewidth;
                    if (rw == 0)
                        throw new Exception("err_old_config");
                }

                _microphones = c.microphones != null ? c.microphones.ToList() : new List<objectsMicrophone>();

                _floorplans = c.floorplans != null ? c.floorplans.ToList() : new List<objectsFloorplan>();

                _remotecommands = c.remotecommands != null ? c.remotecommands.ToList() : new List<objectsCommand>();

                if (_remotecommands.Count == 0)
                {
                    //add default remote commands

                    var cmd = new objectsCommand
                                  {
                                      command = "ispy ALLON",
                                      id = 0,
                                      name = "Switch all on",
                                      smscommand = "ALL ON"
                                  };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                              {
                                  command = "ispy ALLOFF",
                                  id = 1,
                                  name = "Switch all off",
                                  smscommand = "ALL OFF"
                              };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                              {
                                  command = "ispy ALLSCHEDULEDON",
                                  id = 2,
                                  name = "Switch all scheduled on",
                                  smscommand = "ALL SCHED ON"
                              };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                              {
                                  command = "ispy ALLSCHEDULEDOFF",
                                  id = 3,
                                  name = "Switch all scheduled off",
                                  smscommand = "ALL SCHED OFF"
                              };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                              {
                                  command = "ispy APPLYSCHEDULE",
                                  id = 4,
                                  name = "Apply schedule",
                                  smscommand = "APPLY SCHEDULE"
                              };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                    {
                        command = "ispy RECORDONDETECTON",
                        id = 5,
                        name = "Record on detect (all)",
                        smscommand = "RECORDONDETECT"
                    };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                    {
                        command = "ispy RECORDONDETECTOFF",
                        id = 6,
                        name = "Record on detect off (all)",
                        smscommand = "RECORDONDETECTOFF"
                    };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                    {
                        command = "ispy ALERTON",
                        id = 7,
                        name = "Alerts on (all)",
                        smscommand = "ALERTON"
                    };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                    {
                        command = "ispy ALERTOFF",
                        id = 8,
                        name = "Alerts off (all)",
                        smscommand = "ALERTSOFF"
                    };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                    {
                        command = "ispy RECORD",
                        id = 9,
                        name = "Record (all)",
                        smscommand = "RECORD"
                    };
                    _remotecommands.Add(cmd);

                    cmd = new objectsCommand
                    {
                        command = "ispy RECORDOFF",
                        id = 10,
                        name = "Stop Record (all)",
                        smscommand = "RECORDOFF"
                    };
                    _remotecommands.Add(cmd);
                }
                bool bVlc = VlcHelper.VlcInstalled;
                bool bAlertVlc = false;
                int camid = 0;
                string path2;
                foreach (objectsCamera cam in _cameras)
                {
                    if (cam.id >= camid)
                        camid = cam.id + 1;

                    path2 = Conf.MediaDirectory + "video\\" + cam.directory + "\\";
                    if (cam.settings.sourceindex == 5 && !bVlc)
                    {
                        bAlertVlc = true;
                    }
                    if (cam.settings.youtube == null)
                    {
                        cam.settings.youtube = new objectsCameraSettingsYoutube
                                                    {
                                                        autoupload = false,
                                                        category = Conf.YouTubeDefaultCategory,
                                                        tags = "iSpy, Motion Detection, Surveillance",
                                                        @public = true
                                                    };
                    }
                    cam.newrecordingcount = 0;
                    if (cam.settings.maxframerate == 0)
                        cam.settings.maxframerate = 10;
                    if (cam.settings.maxframeraterecord == 0)
                        cam.settings.maxframeraterecord = 10;
                    if (cam.settings.timestampfontsize == 0)
                        cam.settings.timestampfontsize = 10;

                    if (cam.detector.minwidth == 0)
                    {
                        cam.detector.minwidth = 20;
                        cam.detector.minheight = 20;
                        cam.detector.highlight = true;
                        cam.settings.reconnectinterval = 0;
                    }
                    if (cam.settings.accessgroups == null)
                        cam.settings.accessgroups = "";
                    if (cam.settings.ptztimetohome == 0)
                        cam.settings.ptztimetohome = 100;
                    if (cam.directory == null)
                        throw new Exception("err_old_config");

                    if (!Directory.Exists(path2))
                        Directory.CreateDirectory(path2);

                    path2 = Conf.MediaDirectory + "video\\" + cam.directory + "\\thumbs\\";
                    if (!Directory.Exists(path2))
                        Directory.CreateDirectory(path2);

                }
                int micid = 0;
                foreach (objectsMicrophone mic in _microphones)
                {
                    if (mic.id >= micid)
                        micid = mic.id + 1;
                    if (mic.directory == null)
                        throw new Exception("err_old_config");
                    mic.newrecordingcount = 0;
                    path2 = Conf.MediaDirectory + "audio\\" + mic.directory + "\\";
                    if (!Directory.Exists(path2))
                        Directory.CreateDirectory(path2);

                    if (mic.settings.accessgroups == null)
                        mic.settings.accessgroups = "";
                }
                int fpid = 0;
                foreach (objectsFloorplan ofp in _floorplans)
                {
                    if (ofp.id >= fpid)
                        fpid = ofp.id + 1;
                }
                int rcid = 0;
                foreach (objectsCommand ocmd in _remotecommands)
                {
                    if (ocmd.id >= rcid)
                        rcid = ocmd.id + 1;
                }
                if (bAlertVlc)
                    MessageBox.Show(LocRm.GetString("CamerasNotLoadedVLC"), LocRm.GetString("Message"));
                Conf.NextCameraID = camid;
                Conf.NextMicrophoneID = micid;
                Conf.NextFloorPlanID = fpid;
                Conf.NextCommandID = rcid;

                NeedsSync = true;
            }
            catch (Exception)
            {
                MessageBox.Show(LocRm.GetString("ConfigurationChanged"), LocRm.GetString("Error"));
                _cameras = new List<objectsCamera>();
                _microphones = new List<objectsMicrophone>();
                _remotecommands = new List<objectsCommand>();
                _floorplans = new List<objectsFloorplan>();
            }
            reader.Dispose();
            if (fs != null)
                fs.Dispose();
        }

        private static void LoadPTZs(string path)
        {
            try
            {
                var s = new XmlSerializer(typeof (PTZSettings));
                var fs = new FileStream(path, FileMode.Open);
                TextReader reader = new StreamReader(fs);
                fs.Position = 0;
                var c = (PTZSettings) s.Deserialize(reader);
                fs.Close();
                reader.Dispose();
                _ptzs = c.Camera != null ? c.Camera.ToList() : new List<PTZSettingsCamera>();
            }
            catch (Exception)
            {
                MessageBox.Show(LocRm.GetString("PTZError"), LocRm.GetString("Error"));
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _notifyIcon1.Visible = false;
                _notifyIcon1.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
                if (_mWindowState != null)
                    _mWindowState.Dispose();

                Drawfont.Dispose();
                _updateTimer.Dispose();
                _houseKeepingTimer.Dispose();
                _fsw.Dispose();
            }
            base.Dispose(disposing);
        }

        // Close the main form
        private void ExitFileItemClick(object sender, EventArgs e)
        {
            Close();
        }

        // On "Help->About"
        private void AboutHelpItemClick(object sender, EventArgs e)
        {
            var form = new AboutForm();
            form.ShowDialog();
            form.Dispose();
        }

        private void VolumeControlDoubleClick(object sender, EventArgs e)
        {
            Maximise(sender);
        }

        private void FloorPlanDoubleClick(object sender, EventArgs e)
        {
            Maximise(sender);
        }

        private static string Zeropad(int i)
        {
            if (i > 9)
                return i.ToString();
            return "0" + i;
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            //this initializes the port mapping collection
            NATUPNPLib.IStaticPortMappingCollection map = NATControl.Mappings;
            if (Conf.MediaDirectory == "NotSet")
            {
                Conf.MediaDirectory = Program.AppDataPath + @"WebServerRoot\Media\";
            }
            if (!Directory.Exists(Conf.MediaDirectory))
            {
                string notfound = Conf.MediaDirectory;
                Conf.MediaDirectory = Program.AppDataPath + @"WebServerRoot\Media\";
                MessageBox.Show("Media directory could not be found (" + notfound + ") - reset to " + Conf.MediaDirectory);
            }

            DateTime logdate = DateTime.Now;

            FileInfo fi;
            foreach (string s in Directory.GetFiles(Program.AppDataPath, "log_*", SearchOption.TopDirectoryOnly))
            {
                fi = new FileInfo(s);
                if (fi.CreationTime < DateTime.Now.AddDays(-5))
                    File.Delete(s);
            }
            NextLog = Zeropad(logdate.Day) + Zeropad(logdate.Month) + logdate.Year;
            int i = 1;
            if (File.Exists(Program.AppDataPath + "log_" + NextLog + ".htm"))
            {
                while (File.Exists(Program.AppDataPath + "log_" + NextLog + "_" + i + ".htm"))
                    i++;
                NextLog += "_" + i;
            }
            _fsw = new FileSystemWatcher
                       {
                           Path = Program.AppDataPath,
                           IncludeSubdirectories = false,
                           Filter = "external_command.txt",
                           NotifyFilter = NotifyFilters.LastWrite
                       };
            _fsw.Changed += FswChanged;
            _fsw.EnableRaisingEvents = true;
            GC.KeepAlive(_fsw);

            try
            {
                File.WriteAllText(Program.AppDataPath + "log_" + NextLog + ".htm", DateTime.Now + Environment.NewLine);
                _logging = true;
            }
            catch (Exception ex)
            {
                if (
                    MessageBox.Show(LocRm.GetString("LogStartError").Replace("[MESSAGE]", ex.Message),
                                    LocRm.GetString("Warning"), MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    Close();
                    return;
                }
            }

            AVIWriter writer = null;

            try
            {
                if (File.Exists(Conf.MediaDirectory + "test.avi"))
                    File.Delete(Conf.MediaDirectory + "test.avi");
            }
            catch
            {
            }

            string[] compressorOptions = Conf.CompressorOptions.Split(',');

            writer = new AVIWriter();
            try
            {
                writer.Open(Conf.MediaDirectory + "test.avi", 2, 2, false, ref compressorOptions);
                Conf.CompressorOptions = String.Join(",", compressorOptions);
            }
            catch (UnauthorizedAccessException)
            {
                //TODO: Add error message about UAC
            }
            catch (Exception)
            {
                if (
                    MessageBox.Show(LocRm.GetString("CodecPrompt"), LocRm.GetString("InstallCodec"),
                                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process p = Process.Start(Program.AppPath + "wmv9VCMsetup.exe");
                    if (p != null) p.WaitForExit();
                }
            }
            try {
            writer.Close();
            }
            catch
            {
            }


            try
            {
                if (File.Exists(Conf.MediaDirectory + "test.avi"))
                    File.Delete(Conf.MediaDirectory + "test.avi");
            }
            catch
            {
            }
            Menu = _mainMenu;
            _notifyIcon1.ContextMenuStrip = _ctxtTaskbar;
            Identifier = Guid.NewGuid().ToString();
            MWS = new LocalServer(this)
                      {
                          ServerRoot = Program.AppDataPath + @"WebServerRoot\",
                      };
            if (Conf.AllowedIPList == null)
                Conf.AllowedIPList = "";
            LocalServer.AllowedIPs = Conf.AllowedIPList.Split(',').ToList();
            LocalServer.AllowedIPs.RemoveAll(p => p == "");
            GC.KeepAlive(Program.Mutex);
            GC.KeepAlive(MWS);

            SetBackground();

            _toolStrip1.Visible = Conf.ShowToolbar;
            _statusStrip1.Visible = Conf.ShowStatus;
            Menu = !Conf.ShowFileMenu ? null : _mainMenu;


            if (Conf.Fullscreen && !SilentStartup)
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
                WinApi.SetWinFullScreen(Handle);
            }

            _statusBarToolStripMenuItem.Checked = Conf.ShowStatus;
            _toolStripToolStripMenuItem.Checked = Conf.ShowToolbar;
            _fileMenuToolStripMenuItem.Checked = Conf.ShowFileMenu;
            _fullScreenToolStripMenuItem.Checked = Conf.Fullscreen;
            alwaysOnTopToolStripMenuItem.Checked = Conf.AlwaysOnTop;
            this.TopMost = Conf.AlwaysOnTop;

            double dOpacity;
            Double.TryParse(Conf.Opacity.ToString(), out dOpacity);
            Opacity = dOpacity/100.0;

            _chkShowGettingStarted.Checked = Conf.Enabled_ShowGettingStarted;
            _pnlGettingStarted.Visible = Conf.Enabled_ShowGettingStarted && !SilentStartup;

            if (Conf.ServerName == "NotSet")
            {
                Conf.ServerName = SystemInformation.ComputerName;
            }

            _notifyIcon1.Text = Conf.TrayIconText;
            _notifyIcon1.BalloonTipClicked += NotifyIcon1BalloonTipClicked;
            _autoLayoutToolStripMenuItem.Checked = Conf.AutoLayout;

            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;
            _updateTimer.AutoReset = true;
            _updateTimer.SynchronizingObject = this;
            GC.KeepAlive(_updateTimer);

            _houseKeepingTimer = new Timer(1000);
            _houseKeepingTimer.Elapsed += HouseKeepingTimerElapsed;
            _houseKeepingTimer.AutoReset = true;
            _houseKeepingTimer.SynchronizingObject = this;
            GC.KeepAlive(_houseKeepingTimer);

            i = 0;
            int selind = 0;
            foreach (TranslationsTranslationSet set in LocRm.TranslationSets.OrderBy(p=>p.Name))
            {
                _ddlLanguage.Items.Add(new ListItem(set.Name, new[] {set.CultureCode}));
                if (set.CultureCode == Conf.Language)
                    selind = i;
                i++;
            }
            _ddlLanguage.SelectedIndex = selind;


            _pnlGettingStarted.MouseEnter += PanelMouseEnter;
            _pnlGettingStarted.MouseDown += PanelMouseDown;
            _pnlGettingStarted.MouseUp += PanelMouseUp;
            _pnlGettingStarted.MouseLeave += PanelMouseLeave;
            _pnlGettingStarted.MouseMove += PanelMouseMove;

            _wbGettingStarted = new WebBrowser();
            _pnlGettingStarted.Controls.Add(_wbGettingStarted);
            _wbGettingStarted.Location = new Point(23, 33);
            _wbGettingStarted.Size = new Size(552, 400);
            _wbGettingStarted.Navigate(MainForm.Website+"/getting_started.aspx");

            resetLayoutToolStripMenuItem.Enabled = mnuResetLayout.Enabled = false; //reset layout
            _tmrStartup.Start();

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            if (Conf.DHCPReroute && Conf.IPMode == "IPv4")
            {
                //check if IP address has changed
                _ipv4Addresses = null;
                bool iplisted = false;
                foreach (IPAddress ip in AddressListIPv4)
                {
                    if (Conf.IPv4Address == ip.ToString())
                        iplisted = true;
                }
                if (!iplisted)
                {
                    LogErrorToFile(
                        "Your IP address has changed. Please set a static IP address for your local computer to ensure uninterrupted connectivity.");
                    _ipv4Address = "";
                    Conf.IPv4Address = AddressIPv4;
                    if (Conf.UseUPNP && Conf.WSUsername != "")
                    {
                        //change router ports
                        if (NATControl.SetPorts(Conf.ServerPort, Conf.LANPort))
                            LogMessageToFile("Router port forwarding has been updated. (" +
                                             Conf.IPv4Address + ")");
                    }
                }
            }
            if (Conf.IPMode == "IPv6")
            {
                _ipv6Addresses = null;
                bool iplisted = false;
                foreach (IPAddress ip in AddressListIPv6)
                {
                    if (Conf.IPv6Address == ip.ToString())
                        iplisted = true;
                }
                if (!iplisted)
                {
                    LogErrorToFile(
                        "Your IP address has changed. Please set a static IP address for your local computer to ensure uninterrupted connectivity.");
                    _ipv6Address = "";
                    Conf.IPv6Address = AddressIPv6;
                }
            }
        }

        private void RenderResources()
        {
            Text = "iSpy v" + Application.ProductVersion;
            _aboutHelpItem.Text = LocRm.GetString("About");
            _activateToolStripMenuItem.Text = LocRm.GetString("Switchon");
            _addCameraToolStripMenuItem.Text = LocRm.GetString("AddCamera");
            _addFloorPlanToolStripMenuItem.Text = LocRm.GetString("AddFloorplan");
            _addMicrophoneToolStripMenuItem.Text = LocRm.GetString("Addmicrophone");
            _autoLayoutToolStripMenuItem.Text = LocRm.GetString("AutoLayout");
            _btnOk.Text = LocRm.GetString("Ok");
            _chkShowGettingStarted.Text = LocRm.GetString("ShowThisAtStartup");
            _deleteToolStripMenuItem.Text = LocRm.GetString("remove");
            _editToolStripMenuItem.Text = LocRm.GetString("Edit");
            _exitFileItem.Text = LocRm.GetString("Exit");
            _exitToolStripMenuItem.Text = LocRm.GetString("Exit");
            _fileItem.Text = LocRm.GetString("file");
            _fileMenuToolStripMenuItem.Text = LocRm.GetString("Filemenu");
            _floorPlanToolStripMenuItem.Text = LocRm.GetString("FloorPlan");
            _fullScreenToolStripMenuItem.Text = LocRm.GetString("fullScreen");
            _helpItem.Text = LocRm.GetString("help");
            _helpToolstripMenuItem.Text = LocRm.GetString("help");
            _iPCameraToolStripMenuItem.Text = LocRm.GetString("IpCamera");
            _menuItem24.Text = LocRm.GetString("ShowGettingStarted");
            _listenToolStripMenuItem.Text = LocRm.GetString("Listen");
            _localCameraToolStripMenuItem.Text = LocRm.GetString("LocalCamera");
            _menuItem1.Text = LocRm.GetString("chars_2949165");
            _menuItem10.Text = LocRm.GetString("checkForUpdates");
            _menuItem11.Text = LocRm.GetString("reportBugFeedback");
            _menuItem13.Text = LocRm.GetString("chars_2949165");
            _menuItem15.Text = LocRm.GetString("ResetAllRecordingCounters");
            _menuItem16.Text = LocRm.GetString("View");
            _menuItem17.Text = inExplorerToolStripMenuItem.Text = LocRm.GetString("files");
            _menuItem18.Text = LocRm.GetString("clearCaptureDirectories");
            _menuItem19.Text = LocRm.GetString("saveObjectList");
            _menuItem2.Text = LocRm.GetString("help");
            _menuItem20.Text = LocRm.GetString("Logfile");
            _menuItem21.Text = LocRm.GetString("openObjectList");
            _menuItem22.Text = LocRm.GetString("LogFiles");
            _menuItem23.Text = LocRm.GetString("audiofiles");
            _menuItem25.Text = LocRm.GetString("MediaOnAMobiledeviceiphon");
            _menuItem26.Text = LocRm.GetString("supportIspyWithADonation");
            _menuItem27.Text = LocRm.GetString("chars_2949165");
            _menuItem29.Text = LocRm.GetString("Current");
            _menuItem3.Text = LocRm.GetString("MediaoverTheWeb");
            _menuItem30.Text = LocRm.GetString("chars_2949165");
            _menuItem31.Text = LocRm.GetString("removeAllObjects");
            _menuItem32.Text = LocRm.GetString("chars_2949165");
            _menuItem33.Text = LocRm.GetString("switchOff");
            _menuItem34.Text = LocRm.GetString("Switchon");
            _miOnAll.Text = LocRm.GetString("All");
            _miOffAll.Text = LocRm.GetString("All");
            _miOnSched.Text = LocRm.GetString("Scheduled");
            _miOffSched.Text = LocRm.GetString("Scheduled");
            _miApplySchedule.Text = _applyScheduleToolStripMenuItem1.Text = LocRm.GetString("ApplySchedule");
            _applyScheduleToolStripMenuItem.Text = LocRm.GetString("ApplySchedule");
            _menuItem35.Text = LocRm.GetString("ConfigureremoteCommands");
            _menuItem36.Text = LocRm.GetString("Edit");
            _menuItem37.Text = LocRm.GetString("CamerasAndMicrophones");
            _menuItem38.Text = LocRm.GetString("ViewUpdateInformation");
            _menuItem39.Text = LocRm.GetString("AutoLayoutObjects");
            _menuItem4.Text = LocRm.GetString("ConfigureremoteAccess");
            _menuItem5.Text = LocRm.GetString("GoTowebsite");
            _menuItem6.Text = LocRm.GetString("chars_2949165");
            _menuItem7.Text = LocRm.GetString("videofiles");
            _menuItem8.Text = LocRm.GetString("settings");
            _menuItem9.Text = LocRm.GetString("options");
            _microphoneToolStripMenuItem.Text = LocRm.GetString("Microphone");
            _notifyIcon1.Text = LocRm.GetString("Ispy");
            _onMobileDevicesToolStripMenuItem.Text = LocRm.GetString("MobileDevices");
            _opacityToolStripMenuItem.Text = LocRm.GetString("Opacity10");
            _opacityToolStripMenuItem1.Text = LocRm.GetString("Opacity30");
            _opacityToolStripMenuItem2.Text = LocRm.GetString("Opacity100");
            _positionToolStripMenuItem.Text = LocRm.GetString("Position");
            _recordNowToolStripMenuItem.Text = LocRm.GetString("RecordNow");
            _remoteCommandsToolStripMenuItem.Text = LocRm.GetString("RemoteCommands");
            _resetRecordingCounterToolStripMenuItem.Text = LocRm.GetString("ResetRecordingCounter");
            _resetSizeToolStripMenuItem.Text = LocRm.GetString("ResetSize");
            _setInactiveToolStripMenuItem.Text = LocRm.GetString("switchOff");
            _settingsToolStripMenuItem.Text = LocRm.GetString("settings");
            _showFilesToolStripMenuItem.Text = LocRm.GetString("ShowFiles");
            _showISpy100PercentOpacityToolStripMenuItem.Text = LocRm.GetString("ShowIspy100Opacity");
            _showISpy10PercentOpacityToolStripMenuItem.Text = LocRm.GetString("ShowIspy10Opacity");
            _showISpy30OpacityToolStripMenuItem.Text = LocRm.GetString("ShowIspy30Opacity");
            _showToolstripMenuItem.Text = LocRm.GetString("showIspy");
            _statusBarToolStripMenuItem.Text = LocRm.GetString("Statusbar");
            _switchAllOffToolStripMenuItem.Text = LocRm.GetString("SwitchAllOff");
            _switchAllOnToolStripMenuItem.Text = LocRm.GetString("SwitchAllOn");
            _takePhotoToolStripMenuItem.Text = LocRm.GetString("TakePhoto");
            _thruWebsiteToolStripMenuItem.Text = LocRm.GetString("Online");
            _toolStripButton1.Text = LocRm.GetString("WebSettings");
            _toolStripButton4.Text = LocRm.GetString("settings");
            _toolStripButton8.Text = LocRm.GetString("Commands");
            _toolStripDropDownButton1.Text = LocRm.GetString("AccessMedia");
            _toolStripDropDownButton2.Text = LocRm.GetString("Add");
            _toolStripMenuItem1.Text = LocRm.GetString("Viewmedia");
            _toolStripToolStripMenuItem.Text = LocRm.GetString("toolStrip");
            _tsslStats.Text = LocRm.GetString("Loading");
            _unlockToolstripMenuItem.Text = LocRm.GetString("unlock");
            _viewMediaOnAMobileDeviceToolStripMenuItem.Text = LocRm.GetString("ViewMediaOnAMobiledevice");
            _websiteToolstripMenuItem.Text = LocRm.GetString("website");
            _pnlGettingStarted.GroupTitle = LocRm.GetString("GettingStarted");
            _uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Text =
                LocRm.GetString("CamerasAndMicrophonesOnOtherComputers");
            fullScreenToolStripMenuItem.Text = LocRm.GetString("Fullscreen");
            alwaysOnTopToolStripMenuItem.Text = LocRm.GetString("AlwaysOnTop");

            _vLcMicrophoneSourceToolStripMenuItem.Text = LocRm.GetString("AddVLCMicSource");
            _vLcSourceToolStripMenuItem.Text = LocRm.GetString("AddVLCCamSource");
            _exitToolStripMenuItem.Text = LocRm.GetString("Exit");

            mnuSaveLayout.Text = saveLayoutToolStripMenuItem.Text = LocRm.GetString("SaveLayout");
            mnuResetLayout.Text = resetLayoutToolStripMenuItem.Text = LocRm.GetString("ResetLayout");
        }


        private void HouseKeepingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _houseKeepingTimer.Stop();
            if (NeedsRedraw)
            {
                _pnlCameras.Invalidate();
                NeedsRedraw = false;
            }
            try
            {
                CpuUsage = Convert.ToInt32(_cpuCounter.NextValue());
                _counters =  "CPU: " + CpuUsage + "% RAM Usage: " + Convert.ToInt32(Process.GetCurrentProcess().PrivateMemorySize64/1048576) + "Mb";
                tsslMonitor.Text = _counters;
            }
            catch
            {
            }
            if (CpuUsage > conf.CPUMax)
            {
                if (ThrottleFramerate>1)
                    ThrottleFramerate--;
            }
            else
            {
                if (ThrottleFramerate < 40)
                    ThrottleFramerate++;
            }

            _pingCounter++;
            if (_pingCounter == 301)
            {
                _pingCounter = 0;
            }

            try
            {
                if (!MWS.Running && MWS.NumErr >= 5)
                    StopAndStartServer();

                if (WSW.WebsiteLive && Conf.ServicesEnabled)
                {
                    _tsslStats.Text = LocRm.GetString("Online");
                    if (LoopBack && Conf.Subscribed)
                        _tsslStats.Text += " (" + LocRm.GetString("loopback") + ")";
                    else
                    {
                        if (!Conf.Subscribed)
                            _tsslStats.Text += " (" + LocRm.GetString("LANonlynotsubscribed") + ")";
                        else
                            _tsslStats.Text += " (" + LocRm.GetString("LANonlyNoLoopback") + ")";
                    }
                }
                else
                    _tsslStats.Text = LocRm.GetString("Offline");

                if (Conf.ServicesEnabled)
                {
                    try
                    {
                        if (NeedsSync)
                        {
                            WSW.ForceSync();
                            NeedsSync = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Housekeeping Error: " + ex.Message);
                    }


                    if (_pingCounter == 180)
                    {
                        WSW.PingServer();
                    }
                }
                if (Conf.Enable_Storage_Management)
                {
                    if (_pingCounter == 60)
                    {
                        DeleteOldFiles();
                    }

                    if (_pingCounter == 300)
                    {
                        if (MediaDirectorySizeMB > Conf.MaxMediaFolderSizeMB)
                        {
                            if (!ShownWarningMedia)
                            {
                                ShownWarningMedia = true;
                                MessageBox.Show(LocRm.GetString("MediaStorageLimit").Replace("[AMOUNT]",
                                                                                             Conf.
                                                                                                 MaxMediaFolderSizeMB.
                                                                                                 ToString()));
                            }
                        }
                        else
                            ShownWarningMedia = false;
                    }
                }
                if (_pingCounter == 290)
                {
                    if (!_cleaningFiles)
                    {
                        var t = new Thread(() => CleanTemporaryFiles());
                        t.IsBackground = true;
                        t.Name = "Cleaning Files";
                        t.Start();
                    }
                }

                if (_pingCounter == 80)
                {
                    var t = new Thread(() => SaveFileData());
                    t.IsBackground = true;
                    t.Name = "Saving File Data";
                    t.Start();
                }

                if (FfmpegTasks.Count > 0 && (!FfmpegTaskProcessing || !Conf.FFMPEG_SingleProcess))
                {
                    ThreadPool.QueueUserWorkItem((new Ffmpeg(this)).ProcessTask, FfmpegTasks.Dequeue());
                    FfmpegTaskProcessing = true;
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            WriteLog();
            if (!_shuttingDown)
                _houseKeepingTimer.Start();
        }

        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();

            foreach (Control c in _pnlCameras.Controls)
            {
                try
                {
                    switch (c.GetType().ToString())
                    {
                        case "iSpyApplication.CameraWindow":
                            ((CameraWindow) c).Tick();
                            break;
                        case "iSpyApplication.VolumeLevel":
                            ((VolumeLevel) c).Tick();
                            break;
                        case "iSpyApplication.FloorPlanControl":
                            var fpc = ((FloorPlanControl) c);
                            if (fpc.Fpobject.needsupdate)
                            {
                                fpc.NeedsRefresh=true;
                                fpc.Fpobject.needsupdate = false;
                            }
                            fpc.Tick();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogExceptionToFile(ex);
                }
            }
            if (!_shuttingDown)
                _updateTimer.Start();
        }

        private void FswChanged(object sender, FileSystemEventArgs e)
        {
            _fsw.EnableRaisingEvents = false;
            bool err = true;
            int i = 0;
            try
            {
                string txt = "";
                while (err && i < 5)
                {
                    try
                    {
                        using (var fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                while (sr.EndOfStream == false)
                                {
                                    txt = sr.ReadLine();
                                    err = false;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogExceptionToFile(ex);
                        i++;
                        Thread.Sleep(500);
                    }
                }
                if (txt != null)
                    if (txt.Trim() != "")
                        ParseCommand(txt);
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            _fsw.EnableRaisingEvents = true;
        }

        private void ParseCommand(string command)
        {
            if (command == null) throw new ArgumentNullException("command");
            try
            {
                command = Uri.UnescapeDataString(command);

                LogMessageToFile("Running External Command: " + command);

                if (command.ToLower().StartsWith("open "))
                {
                    if (InvokeRequired)
                        Invoke(new ExternalCommandDelegate(LoadObjectList), command.Substring(5).Trim('"'));
                    else
                        LoadObjectList(command.Substring(5).Trim('"'));
                }
                if (command.ToLower().StartsWith("commands "))
                {
                    string cmd = command.Substring(9).Trim('"');
                    string[] commands = cmd.Split('|');
                    foreach (string command2 in commands)
                    {
                        if (command2 != "")
                        {
                            if (InvokeRequired)
                                Invoke(new ExternalCommandDelegate(ProcessCommandInternal), command2.Trim('"'));
                            else
                                ProcessCommandInternal(command2.Trim('"'));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
                MessageBox.Show(LocRm.GetString("LoadFailed").Replace("[MESSAGE]", ex.Message));
            }
        }

        private static void ProcessCommandInternal(string command)
        {
            //parse command into new format
            string[] cfg = command.Split(',');
            string newcommand = "";
            if (cfg.Length == 1)
                newcommand = cfg[0];
            else
            {
                newcommand = cfg[0] + "?ot=" + cfg[1] + "&oid=" + cfg[2];
            }
            MWS.ProcessCommandInternal(newcommand);
        }

        public void SetBackground()
        {
            _pnlCameras.BackColor = Conf.MainColor.ToColor();
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _notifyIcon1.Visible = false;

                _notifyIcon1.Icon.Dispose();
                _notifyIcon1.Dispose();
            }
            catch
            {
            }
            base.OnClosed(e);
        }

        private void MenuItem2Click(object sender, EventArgs e)
        {
            StartBrowser(MainForm.Website+"/userguide.aspx");
        }

        internal static string StopAndStartServer()
        {
            string message = "";
            try
            {
                MWS.StopServer();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            Application.DoEvents();
            try {
                message = MWS.StartServer();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            return message;
        }

        private void MenuItem4Click(object sender, EventArgs e)
        {
            WebConnect();
        }

        private void MenuItem5Click(object sender, EventArgs e)
        {
            StartBrowser(MainForm.Website+"/");
        }

        private void MenuItem10Click(object sender, EventArgs e)
        {
            CheckForUpdates(false);
        }

        private void CheckForUpdates(bool suppressMessages)
        {
            string version = "";
            try
            {
                version = WSW.ProductLatestVersion(11);
                if (version == WSW.DownMessage)
                {
                    MessageBox.Show(WSW.DownMessage, LocRm.GetString("Error"));
                    throw new Exception("down");
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
                if (!suppressMessages)
                    MessageBox.Show(LocRm.GetString("CheckUpdateError"), LocRm.GetString("Error"));
            }
            if (version != "" && version != WSW.DownMessage)
            {
                var verThis = new Version(Application.ProductVersion);
                var verLatest = new Version(version);
                if (verThis < verLatest)
                {
                    var nv = new NewVersion();
                    nv.ShowDialog(this);
                    nv.Dispose();
                }
                else
                {
                    if (!suppressMessages)
                        MessageBox.Show(LocRm.GetString("HaveLatest"), LocRm.GetString("Note"), MessageBoxButtons.OK);
                }
            }
        }

        private void MenuItem8Click(object sender, EventArgs e)
        {
            ShowSettings(0);
        }

        public void ShowSettings(int tabindex)
        {
            var settings = new Settings {Owner = this, InitialTab = tabindex};
            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                _pnlCameras.BackColor = Conf.MainColor.ToColor();
                _notifyIcon1.Text = Conf.TrayIconText;
            }

            if (settings.ReloadResources)
                RenderResources();
            AddressIPv4 = "";//forces reload
            AddressIPv6 = "";
            settings.Dispose();
            SaveConfig();
        }

        private void MenuItem11Click(object sender, EventArgs e)
        {
            using (var fb = new Feedback())
            {
                fb.ShowDialog();
            }
        }

        private void MainFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                if (Conf.BalloonTips)
                {
                    if (Conf.BalloonTips)
                    {
                        _notifyIcon1.BalloonTipText = LocRm.GetString("RunningInTaskBar");
                        _notifyIcon1.ShowBalloonTip(1500);
                    }
                }
            }
            else
            {
                if (Conf.AutoLayout)
                    LayoutObjects(0, 0);
            }
        }

        private void NotifyIcon1DoubleClick(object sender, EventArgs e)
        {
            ShowIfUnlocked();
        }

        private void ShowIfUnlocked()
        {
            if (Visible == false || WindowState == FormWindowState.Minimized)
            {
                if (Conf.Enable_Password_Protect)
                {
                    using (var cp = new CheckPassword())
                    {
                        cp.ShowDialog(this);
                        if (cp.DialogResult == DialogResult.OK)
                        {
                            ShowForm(-1);
                        }
                    }
                }
                else
                {
                    ShowForm(-1);
                }
            }
            else
            {
                ShowForm(-1);
            }
        }

        private void MainFormFormClosing1(object sender, FormClosingEventArgs e)
        {
            Exit();
        }

        private void Exit()
        {
            _houseKeepingTimer.Stop();
            _updateTimer.Stop();
            _shuttingDown = true;

            if (Conf.BalloonTips)
            {
                if (Conf.BalloonTips)
                {
                    _notifyIcon1.BalloonTipText = LocRm.GetString("ShuttingDown");
                    _notifyIcon1.ShowBalloonTip(1500);
                }
            }
            _closing = true;
            try
            {
                SaveObjects("");
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            try
            {
                SaveConfig();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }

            try
            {
                RemoveObjects();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            try
            {
                MWS.StopServer();
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            try {
            Application.DoEvents();
                if (Conf.ServicesEnabled)
                {
                    try
                    {
                        WSW.Disconnect();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                LogExceptionToFile(ex);
            }
            ////restore screensaver
            //if (_origScreenSaveSetting != null)
            //{
            //    try
            //    {
            //        RegistryKey regkeyScreenSaver =
            //            Registry.CurrentUser.OpenSubKey("Control Panel");
            //        if (regkeyScreenSaver!=null)
            //        {
            //            regkeyScreenSaver = regkeyScreenSaver.OpenSubKey("Desktop", true);
            //            if (regkeyScreenSaver!=null)
            //            {
            //                regkeyScreenSaver.SetValue("ScreenSaveActive", _origScreenSaveSetting);
            //                regkeyScreenSaver.Close();
            //            }
            //        }
                    
            //    }
            //    catch (Exception ex)
            //    {
            //        LogExceptionToFile(ex);
            //    }
            //}
            WriteLog();
        }

        private void WriteLog()
        {
            if (_logging)
            {
                try
                {
                    if (LogFile.Length > Conf.LogFileSizeKB*1024)
                    {
                        LogFile.Append(
                            "<tr><td style=\"color:red\" valign=\"top\">Logging Exiting</td><td valign=\"top\">" +
                            DateTime.Now.ToLongTimeString() +
                            "</td><td valign=\"top\">Logging is being disabled as it has reached the maximum size (" +
                            Conf.LogFileSizeKB + "kb).</td></tr>");
                        _logging = false;
                    }
                    if (_lastlog != LogFile.ToString())
                    {
                        string fc = LogTemplate.Replace("<!--CONTENT-->", LogFile.ToString()).Replace("<!--VERSION-->", Application.ProductVersion);
                        File.WriteAllText(Program.AppDataPath + @"log_" + NextLog + ".htm", fc);
                        _lastlog = LogFile.ToString();
                    }
                }
                catch (System.Exception)
                {
                    _logging = false;
                }
            }
        }

        private void RemoveObjects()
        {
            bool removed = true;
            while (removed)
            {
                removed = false;
                foreach (Control c in _pnlCameras.Controls)
                {
                    if (c.GetType() == typeof (CameraWindow))
                    {
                        var cameraControl = (CameraWindow) c;
                        RemoveCamera(cameraControl);
                        removed = true;
                        break;
                    }
                    if (c.GetType() == typeof (VolumeLevel))
                    {
                        var volumeControl = (VolumeLevel) c;
                        RemoveMicrophone(volumeControl);
                        removed = true;
                        break;
                    }
                    if (c.GetType() == typeof (FloorPlanControl))
                    {
                        var floorPlanControl = (FloorPlanControl) c;
                        RemoveFloorplan(floorPlanControl);
                        removed = true;
                        break;
                    }
                }
            }
        }


        private void RenderObjects()
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c.GetType() == typeof (CameraWindow))
                {
                    var cameraControl = (CameraWindow) c;
                    RemoveCamera(cameraControl);
                }

                if (c.GetType() == typeof (VolumeLevel))
                {
                    var volumeControl = (VolumeLevel) c;
                    RemoveMicrophone(volumeControl);
                }

                if (c.GetType() == typeof (FloorPlanControl))
                {
                    var floorPlanControl = (FloorPlanControl)c;
                    RemoveFloorplan(floorPlanControl);
                }
            }

            foreach (objectsCamera oc in Cameras)
            {
                DisplayCamera(oc);
            }
            foreach (objectsMicrophone om in Microphones)
            {
                DisplayMicrophone(om);
            }
            foreach (objectsFloorplan ofp in FloorPlans)
            {
                DisplayFloorPlan(ofp);
            }

            NeedsSync = true;
        }

        private void SetCameraEvents(CameraWindow cameraControl)
        {
            cameraControl.MouseEnter += CameraControlMouseEnter;
            cameraControl.MouseDown += CameraControlMouseDown;
            cameraControl.MouseWheel += CameraControlMouseWheel;
            cameraControl.MouseUp += CameraControlMouseUp;
            cameraControl.MouseLeave += CameraControlMouseLeave;
            cameraControl.MouseMove += CameraControlMouseMove;
            cameraControl.DoubleClick += CameraControlDoubleClick;
            cameraControl.RemoteCommand += CameraControlRemoteCommand;
            cameraControl.Notification += ControlNotification;
        }

        

        private void ControlNotification(object sender, NotificationType e)
        {
            if (Conf.BalloonTips)
            {
                _notifyIcon1.BalloonTipText = LocRm.GetString(e.Type) + ":" + NL + e.Text;
                _notifyIcon1.ShowBalloonTip(1500);
            }
        }

        private void NotifyIcon1BalloonTipClicked(object sender, EventArgs e)
        {
            ShowIfUnlocked();
        }

        private void SetMicrophoneEvents(VolumeLevel vw)
        {
            vw.DoubleClick += VolumeControlDoubleClick;
            vw.MouseEnter += VolumeControlMouseEnter;
            vw.MouseDown += VolumeControlMouseDown;
            vw.MouseUp += VolumeControlMouseUp;
            vw.MouseLeave += VolumeControlMouseLeave;
            vw.MouseMove += VolumeControlMouseMove;
            vw.RemoteCommand += VolumeControlRemoteCommand;
            vw.Notification += ControlNotification;
        }

        private void SetFloorPlanEvents(FloorPlanControl fpc)
        {
            fpc.DoubleClick += FloorPlanDoubleClick;
            fpc.MouseEnter += FloorPlanMouseEnter;
            fpc.MouseDown += FloorPlanMouseDown;
            fpc.MouseUp += FloorPlanMouseUp;
            fpc.MouseLeave += FloorPlanMouseLeave;
            fpc.MouseMove += FloorPlanMouseMove;
        }

        internal void DisplayMicrophone(objectsMicrophone mic)
        {
            var micControl = new VolumeLevel(mic);
            SetMicrophoneEvents(micControl);
            micControl.BackColor = Conf.BackColor.ToColor();
            _pnlCameras.Controls.Add(micControl);
            micControl.Location = new Point(mic.x, mic.y);
            micControl.Size = new Size(mic.width, mic.height);
            micControl.BringToFront();

            if (Conf.AutoSchedule && mic.schedule.active && mic.schedule.entries.Count() > 0)
            {
                mic.settings.active = false;
                micControl.ApplySchedule();
            }
            else
            {
                if (mic.settings.active)
                    micControl.Enable();
            }

            string path = Conf.MediaDirectory + "audio\\" + mic.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        internal void DisplayFloorPlan(objectsFloorplan ofp)
        {
            var fpControl = new FloorPlanControl(ofp, this);
            SetFloorPlanEvents(fpControl);
            fpControl.BackColor = Conf.BackColor.ToColor();
            _pnlCameras.Controls.Add(fpControl);
            fpControl.Location = new Point(ofp.x, ofp.y);
            fpControl.Size = new Size(ofp.width, ofp.height);
            fpControl.BringToFront();
        }

        internal void EditCamera(objectsCamera cr)
        {
            int cameraId = Convert.ToInt32(cr.id);
            CameraWindow cw = null;

            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (CameraWindow)) continue;
                var cameraControl = (CameraWindow) c;
                if (cameraControl.Camobject.id == cameraId)
                {
                    cw = cameraControl;
                    break;
                }
            }

            if (cw == null) return;
            var ac = new AddCamera {CameraControl = cw};
            ac.ShowDialog(this);
            ac.Dispose();
        }

        internal void EditMicrophone(objectsMicrophone om)
        {
            VolumeLevel vlf = null;

            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (VolumeLevel)) continue;
                var vl = (VolumeLevel) c;
                if (vl.Micobject.id == om.id)
                {
                    vlf = vl;
                    break;
                }
            }

            if (vlf != null)
            {
                var am = new AddMicrophone {VolumeLevel = vlf};
                am.ShowDialog(this);
                am.Dispose();
            }
        }

        internal void EditFloorplan(objectsFloorplan ofp)
        {
            FloorPlanControl fpc = null;

            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (FloorPlanControl)) continue;
                var fp = (FloorPlanControl) c;
                if (fp.Fpobject.id != ofp.id) continue;
                fpc = fp;
                break;
            }

            if (fpc != null)
            {
                var afp = new AddFloorPlan {Fpc = fpc, Owner =  this};
                afp.ShowDialog(this);
                afp.Dispose();
                fpc.Invalidate();
            }
        }

        public CameraWindow GetCamera(int cameraId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (CameraWindow)) continue;
                var cw = (CameraWindow) c;
                if (cw.Camobject.id != cameraId) continue;
                return cw;
            }
            return null;
        }

        public VolumeLevel GetMicrophone(int microphoneId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (VolumeLevel)) continue;
                var vw = (VolumeLevel) c;
                if (vw.Micobject.id != microphoneId) continue;
                return vw;
            }
            return null;
        }

        public FloorPlanControl GetFloorPlan(int floorPlanId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (FloorPlanControl)) continue;
                var fp = (FloorPlanControl) c;
                if (fp.Fpobject.id != floorPlanId) continue;
                return fp;
            }
            return null;
        }

        public void RemoveCamera(CameraWindow cameraControl)
        {
            cameraControl.ShuttingDown = true;
            cameraControl.MouseEnter -= CameraControlMouseEnter;
            cameraControl.MouseDown -= CameraControlMouseDown;
            cameraControl.MouseUp -= CameraControlMouseUp;
            cameraControl.MouseLeave -= CameraControlMouseLeave;
            cameraControl.MouseMove -= CameraControlMouseMove;
            cameraControl.DoubleClick -= CameraControlDoubleClick;
            cameraControl.RemoteCommand -= CameraControlRemoteCommand;
            cameraControl.Notification -= ControlNotification;
            cameraControl.Disable();

            if (InvokeRequired)
                Invoke(new CameraCommandDelegate(RemoveCameraPanel), cameraControl);
            else
                RemoveCameraPanel(cameraControl);
        }

        private void RemoveCameraPanel(CameraWindow cameraControl)
        {
            
            _pnlCameras.Controls.Remove(cameraControl);
            if (!_closing)
            {
                CameraWindow control = cameraControl;
                objectsCamera oc = Cameras.SingleOrDefault(p => p.id == control.Camobject.id);
                if (oc != null)
                    Cameras.Remove(oc);

                NeedsSync = true;
                SetNewStartPosition();
            }
            Application.DoEvents();
            cameraControl.Dispose();
        }

        public void RemoveMicrophone(VolumeLevel volumeControl)
        {
            
            volumeControl.MouseEnter -= VolumeControlMouseEnter;
            volumeControl.MouseDown -= VolumeControlMouseDown;
            volumeControl.MouseUp -= VolumeControlMouseUp;
            volumeControl.MouseLeave -= VolumeControlMouseLeave;
            volumeControl.MouseMove -= VolumeControlMouseMove;
            volumeControl.DoubleClick -= VolumeControlDoubleClick;
            volumeControl.RemoteCommand -= VolumeControlRemoteCommand;
            volumeControl.Notification -= ControlNotification;
            
            volumeControl.Disable();

            if (InvokeRequired)
                Invoke(new MicrophoneCommandDelegate(RemoveMicrophonePanel), volumeControl);
            else
                RemoveMicrophonePanel(volumeControl);
        }

        private void RemoveMicrophonePanel(VolumeLevel volumeControl)
        {
            _pnlCameras.Controls.Remove(volumeControl);

            if (!_closing)
            {
                var control = volumeControl;
                var om = Microphones.SingleOrDefault(p => p.id == control.Micobject.id);
                for (var index = 0; index < Cameras.Where(p => p.settings.micpair == om.id).ToList().Count; index++)
                {
                    var oc = Cameras.Where(p => p.settings.micpair == om.id).ToList()[index];
                    oc.settings.micpair = -1;
                }
                if (om != null)
                    Microphones.Remove(om);
                SetNewStartPosition();
                NeedsSync = true;
            }
            Application.DoEvents();
            volumeControl.Dispose();
        }

        private void RemoveFloorplan(FloorPlanControl fpc)
        {
            _pnlCameras.Controls.Remove(fpc);

            if (!_closing)
            {
                objectsFloorplan ofp = FloorPlans.SingleOrDefault(p => p.id == fpc.Fpobject.id);
                if (ofp != null)
                    FloorPlans.Remove(ofp);
                SetNewStartPosition();
                NeedsSync = true;
            }
            fpc.Dispose();
        }

        public void CleanTemporaryFiles()
        {
            _cleaningFiles = true;
            //clean-up files in windows temp directory
            var files = Directory.GetFiles(Path.GetTempPath(), "*.tmp");
            foreach (string t in files)
            {
                try
                {
                    string fn = t.Substring(t.LastIndexOf("\\") + 1);
                    if (fn.StartsWith("wmv"))
                    {
                        File.Delete(t);
                    }
                }
                catch
                {
                    //file may have been deleted
                }
            }
            _cleaningFiles = false;
        }

        public void SaveFileData()
        {
            foreach (objectsCamera oc in Cameras)
            {
                CameraWindow occ = GetCameraWindow(oc.id);
                if (occ != null)
                {
                    occ.SaveFileList();
                }
            }
           
            foreach (objectsMicrophone om in Microphones)
            {
                VolumeLevel omc = GetMicrophone(om.id);
                if (omc != null)
                {
                    omc.SaveFileList();
                }
            }
        }

        private static void DeleteOldFiles()
        {
            if (Conf.DeleteFilesOlderThanDays <= 0)
                return;
            string loc = Conf.MediaDirectory;

            DateTime dtref = DateTime.Now.AddDays(0 - Conf.DeleteFilesOlderThanDays);

            if (MediaDirectorySizeMB <= Conf.MaxMediaFolderSizeMB*0.7) return;
            string[] files = Directory.GetFiles(loc, "*.*", SearchOption.AllDirectories);
            foreach (string t in files)
            {
                try
                {
                    var fi = new FileInfo(t);
                    if (fi.CreationTime < dtref && fi.Extension.ToLower()!=".xml")
                    {
                        try
                        {
                            File.Delete(t);
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                    //file may have been deleted
                }
            }
        }

        private void AddCamera(int videoSourceIndex)
        {
            CameraWindow cw = NewCameraWindow(videoSourceIndex);

            var ac = new AddCamera {CameraControl = cw};
            ac.ShowDialog(this);
            if (ac.DialogResult == DialogResult.OK)
            {
                ac.CameraControl.Camobject.id = Conf.NextCameraID;
                Cameras.Add(cw.Camobject);
                string path = Conf.MediaDirectory + "video\\" + cw.Camobject.directory + "\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = Conf.MediaDirectory + "video\\" + cw.Camobject.directory + "\\thumbs\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                
                Conf.NextCameraID++;

                SetNewStartPosition();
                NeedsSync = true;
            }
            else
            {
                cw.Disable();
                _pnlCameras.Controls.Remove(cw);
                cw.Dispose();
            }
            ac.Dispose();
        }

        private CameraWindow NewCameraWindow(int videoSourceIndex)
        {
            var oc = new objectsCamera
                         {
                             alerts = new objectsCameraAlerts(),
                             detector = new objectsCameraDetector
                                            {
                                                motionzones =
                                                    new objectsCameraDetectorZone
                                                    [0]
                                            },
                             notifications = new objectsCameraNotifications(),
                             recorder = new objectsCameraRecorder(),
                             schedule = new objectsCameraSchedule {entries = new objectsCameraScheduleEntry[0]},
                             settings = new objectsCameraSettings(),
                             ftp = new objectsCameraFtp(),
                             id = -1,
                             directory = RandomString(5),
                             ptz = -1,
                             x = 0,
                             y = 0
                         };

            oc.flipx = oc.flipy = false;
            oc.width = 320;
            oc.height = 240;
            oc.name = "";
            oc.description = "";
            oc.resolution = "320x240";
            oc.newrecordingcount = 0;

            oc.alerts.active = true;
            oc.alerts.mode = "movement";
            oc.alerts.alertoptions = "false,false";
            oc.alerts.objectcountalert = 1;
            oc.alerts.minimuminterval = 180;
            oc.alerts.numberplatesinterval = 1;
            oc.alerts.numberplatesaccuracy = 80;
            oc.alerts.numberplatesarea = "10,33,80,77";

            oc.notifications.sendemail = false;
            oc.notifications.sendsms = false;
            oc.notifications.sendmms = false;
            oc.notifications.emailgrabinterval = 0;

            oc.ftp.enabled = false;
            oc.ftp.port = 21;
            oc.ftp.mode = 0;
            oc.ftp.server = "ftp://";
            oc.ftp.interval = 10;
            oc.ftp.filename = "mylivecamerafeed.jpg";
            oc.ftp.ready = true;
            oc.ftp.text = "www.ispyconnect.com";

            oc.schedule.active = false;

            oc.settings.active = false;
            oc.settings.deleteavi = true;
            oc.settings.ffmpeg = Conf.FFMPEG_Camera;
            oc.settings.emailaddress = EmailAddress;
            oc.settings.smsnumber = MobileNumber;
            oc.settings.suppressnoise = true;
            oc.settings.login = "";
            oc.settings.password = "";
            oc.settings.useragent = "Mozilla/5.0";
            oc.settings.frameinterval = 10;
            oc.settings.sourceindex = videoSourceIndex;
            oc.settings.micpair = -1;
            oc.settings.frameinterval = 200;
            oc.settings.maxframerate = 10;
            oc.settings.maxframeraterecord = 10;
            oc.settings.ptzautotrack = false;
            oc.settings.framerate = 10;
            oc.settings.timestamplocation = 1;
            oc.settings.ptztimetohome = 100;
            oc.settings.timestampformatter = "FPS: {FPS} {0:G} ";
            oc.settings.timestampfontsize = 10;
            oc.settings.notifyondisconnect = false;

            oc.settings.youtube = new objectsCameraSettingsYoutube
                                      {
                                          autoupload = false,
                                          category = Conf.YouTubeDefaultCategory,
                                          tags = "iSpy, Motion Detection, Surveillance",
                                          @public = true
                                      };
            oc.settings.desktopresizeheight = 480;
            oc.settings.desktopresizewidth = 640;
            if (VlcHelper.VlcInstalled)
                oc.settings.vlcargs = "-I" + NL + "dummy" + NL + "noaudio" + NL + "--ignore-config" + NL +
                                       "--plugin-path=\"" + VlcHelper.VlcPluginsFolder + "\"";
            else
                oc.settings.vlcargs = "";

            oc.detector.recordondetect = true;
            oc.detector.keepobjectedges = false;
            oc.detector.processeveryframe = 1;
            oc.detector.nomovementinterval = 30;
            oc.detector.movementinterval = 0;
            oc.detector.calibrationdelay = 15;
            oc.detector.color = ColorTranslator.ToHtml(Conf.TrackingColor.ToColor());
            oc.detector.type = "Two Frames";
            oc.detector.postprocessor = "None";
            oc.detector.sensitivity = 80;
            oc.detector.minwidth = 20;
            oc.detector.minheight = 20;
            oc.detector.highlight = true;

            oc.recorder.bufferframes = 30;
            oc.recorder.inactiverecord = 5;
            oc.recorder.timelapse = 0;
            oc.recorder.timelapseframes = 0;
            oc.recorder.maxrecordtime = 900;


            var cameraControl = new CameraWindow(oc) { BackColor = Conf.BackColor.ToColor() };
            _pnlCameras.Controls.Add(cameraControl);

            cameraControl.Location = new Point(oc.x, oc.y);
            cameraControl.Size = new Size(320, 240);
            cameraControl.AutoSize = true;
            cameraControl.BringToFront();
            SetCameraEvents(cameraControl);
            if (Conf.AutoLayout)
                LayoutObjects(0, 0);

            
            return cameraControl;
        }

        private void AddMicrophone(int audioSourceIndex)
        {
            VolumeLevel vl = NewVolumeLevel(audioSourceIndex);

            var am = new AddMicrophone {VolumeLevel = vl};
            am.ShowDialog(this);


            if (am.DialogResult == DialogResult.OK)
            {
                am.VolumeLevel.Micobject.id = Conf.NextMicrophoneID;
                Conf.NextMicrophoneID++;
                Microphones.Add(vl.Micobject);
                string path = Conf.MediaDirectory + "audio\\" + vl.Micobject.directory + "\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                SetNewStartPosition();
                NeedsSync = true;
            }
            else
            {
                vl.Disable();
                _pnlCameras.Controls.Remove(vl);
                vl.Dispose();
            }
            am.Dispose();
        }

        private VolumeLevel NewVolumeLevel(int audioSourceIndex)
        {
            var om = new objectsMicrophone
                         {
                             alerts = new objectsMicrophoneAlerts(),
                             detector = new objectsMicrophoneDetector(),
                             notifications = new objectsMicrophoneNotifications(),
                             recorder = new objectsMicrophoneRecorder(),
                             schedule = new objectsMicrophoneSchedule
                                            {
                                                entries
                                                    =
                                                    new objectsMicrophoneScheduleEntry
                                                    [
                                                    0
                                                    ]
                                            },
                             settings = new objectsMicrophoneSettings(),
                             id = -1,
                             directory = RandomString(5),
                             x = 0,
                             y = 0,
                             width = 160,
                             height = 40,
                             name = "",
                             description = "",
                             newrecordingcount = 0
                         };

            om.settings.typeindex = audioSourceIndex;
            om.settings.deletewav = true;
            om.settings.ffmpeg = Conf.FFMPEG_Microphone;
            om.settings.buffer = 4;
            om.settings.samples = 22050;
            om.settings.bits = 16;
            om.settings.volume = 50;
            om.settings.channels = 1;
            om.settings.decompress = true;
            om.settings.smsnumber = MobileNumber;
            om.settings.emailaddress = EmailAddress;
            om.settings.active = false;
            om.settings.notifyondisconnect = false;
            if (VlcHelper.VlcInstalled)
                om.settings.vlcargs = "-I" + NL + "dummy" + NL + "--ignore-config" + NL + "--plugin-path=\"" +
                                       VlcHelper.VlcPluginsFolder + "\"";
            else
                om.settings.vlcargs = "";

            om.detector.sensitivity = 60;
            om.detector.nosoundinterval = 30;
            om.detector.soundinterval = 0;
            om.detector.recordondetect = true;

            om.alerts.mode = "sound";
            om.alerts.minimuminterval = 180;
            om.alerts.executefile = "";
            om.alerts.active = true;
            om.alerts.alertoptions = "false,false";

            om.recorder.inactiverecord = 5;
            om.recorder.maxrecordtime = 900;

            om.notifications.sendemail = false;
            om.notifications.sendsms = false;

            om.schedule.active = false;

            var volumeControl = new VolumeLevel(om) { BackColor = Conf.BackColor.ToColor() };
            _pnlCameras.Controls.Add(volumeControl);

            volumeControl.Location = new Point(om.x, om.y);
            volumeControl.Size = new Size(160, 40);
            volumeControl.BringToFront();
            SetMicrophoneEvents(volumeControl);

            if (Conf.AutoLayout)
                LayoutObjects(0, 0);
            return volumeControl;
        }

        public static string RandomString(int length)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                char ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26*Random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        private void AddFloorPlan()
        {
            var ofp = new objectsFloorplan
                          {
                              objects = new objectsFloorplanObjects
                                            {@object = new objectsFloorplanObjectsEntry[0]},
                              id = -1,
                              name = "",
                              image = "",
                              height = 480,
                              width = 640,
                              x = 0,
                              y = 0
                          };

            var fpc = new FloorPlanControl(ofp, this) { BackColor = Conf.BackColor.ToColor() };
            _pnlCameras.Controls.Add(fpc);

            fpc.Location = new Point(ofp.x, ofp.y);
            fpc.Size = new Size(320, 240);
            fpc.BringToFront();

            var afp = new AddFloorPlan { Fpc = fpc, Owner = this };
            afp.ShowDialog(this);
            if (afp.DialogResult == DialogResult.OK)
            {
                afp.Fpc.Fpobject.id = Conf.NextFloorPlanID;
                FloorPlans.Add(ofp);
                Conf.NextFloorPlanID++;
                SetFloorPlanEvents(fpc);
                SetNewStartPosition();
                fpc.Invalidate();
            }
            else
            {
                _pnlCameras.Controls.Remove(fpc);
                fpc.Dispose();
            }
            afp.Dispose();
        }

        private void SetNewStartPosition()
        {
            if (Conf.AutoLayout)
                LayoutObjects(0, 0);
        }

        private void VolumeControlRemoteCommand(object sender, VolumeLevel.ThreadSafeCommand e)
        {
            InvokeMethod i = DoInvoke;
            Invoke(i, new object[] {e.Command});
        }

        private void TmrStartupTick(object sender, EventArgs e)
        {
            _tmrStartup.Stop();

            //load in object list

            if (_startCommand.Trim().StartsWith("open"))
            {
                ParseCommand(_startCommand);
                _startCommand = "";
            }
            else
            {
                if (!File.Exists(Program.AppDataPath + @"XML\objects.xml"))
                {
                    File.Copy(Program.AppDataPath + @"objects.xml", Program.AppDataPath + @"XML\objects.xml");
                }
                ParseCommand("open " + Program.AppDataPath + @"XML\objects.xml");
            }
            if (_startCommand != "")
            {
                ParseCommand(_startCommand);
            }

            StopAndStartServer();
            Conf.Subscribed = false;
            if (Conf.ServicesEnabled)
            {
                if (Conf.UseUPNP)
                {
                    NATControl.SetPorts(Conf.ServerPort, Conf.LANPort);
                }

                string[] result =
                    WSW.TestConnection(Conf.WSUsername, Conf.WSPassword, Conf.Loopback);

                if (result[0] == "OK")
                {
                    WSW.Connect();
                    NeedsSync = true;
                    EmailAddress = result[2];
                    MobileNumber = result[4];

                    Conf.ServicesEnabled = true;
                    Conf.Subscribed = (Convert.ToBoolean(result[1]));

                    Text = "iSpy v" + Application.ProductVersion;
                    if (Conf.WSUsername != "")
                    {
                        Text += " (" + Conf.WSUsername + ")";
                    }

                    if (result[3] == "")
                    {
                        LoopBack = Conf.Loopback;
                        WSW.Connect(Conf.Loopback);
                    }
                    else
                    {
                        LoopBack = false;
                        if (!SilentStartup)
                        {
                            MessageBox.Show(result[3], LocRm.GetString("Error"));
                        }
                    }
                }
                else
                {
                    if (!SilentStartup)
                    {
                        MessageBox.Show(result[0], LocRm.GetString("Error"));
                    }
                }
            }

            if (Conf.Enable_Update_Check && !SilentStartup)
            {
                CheckForUpdates(true);
            }
            if (SilentStartup)
            {
                _mWindowState = new PersistWindowState {Parent = this, RegistryPath = @"Software\ispy\startup"};
            }

            SilentStartup = false;

            _updateTimer.Start();
            _houseKeepingTimer.Start();
            _tmrStartup.Dispose();
        }

        public CameraWindow GetCameraWindow(int cameraId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (CameraWindow)) continue;
                var cw = (CameraWindow) c;
                if (cw.Camobject.id == cameraId)
                    return cw;
            }
            return null;
        }

        public VolumeLevel GetVolumeLevel(int microphoneId)
        {
            for (int index = 0; index < _pnlCameras.Controls.Count; index++)
            {
                Control c = _pnlCameras.Controls[index];
                if (c.GetType() != typeof (VolumeLevel)) continue;
                var vw = (VolumeLevel) c;
                if (vw.Micobject.id == microphoneId)
                    return vw;
            }
            return null;
        }

        private void SetInactiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.Disable();
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vf = ((VolumeLevel) ContextTarget);
                    vf.Disable();
                }
            }
        }

        private void EditToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                EditCamera(((CameraWindow) ContextTarget).Camobject);
            }
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                EditMicrophone(((VolumeLevel) ContextTarget).Micobject);
            }
            if (ContextTarget.GetType() == typeof (FloorPlanControl))
            {
                EditFloorplan(((FloorPlanControl) ContextTarget).Fpobject);
            }
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                RemoveCamera((CameraWindow) ContextTarget);
            }
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                RemoveMicrophone((VolumeLevel) ContextTarget);
            }
            if (ContextTarget.GetType() == typeof (FloorPlanControl))
            {
                RemoveFloorplan((FloorPlanControl) ContextTarget);
            }
        }


        private void ToolStripButton4Click(object sender, EventArgs e)
        {
            ShowSettings(0);
        }


        internal static void LogExceptionToFile(Exception ex, string info)
        {
            ex.HelpLink = info + ": " + ex.Message;
            LogExceptionToFile(ex);
        }

        
        internal static void LogExceptionToFile(Exception ex)
        {
            if (!_logging)
                return;
            try
            {
                string em = ex.HelpLink + "<br/>" + ex.Message + "<br/>" + ex.Source + "<br/>" + ex.StackTrace +
                             "<br/>" + ex.InnerException + "<br/>" + ex.Data;
                LogFile.Append("<tr><td style=\"color:red\" valign=\"top\">Exception:</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + em + "</td></tr>");
            }
            catch
            {
                
            }
        }

        internal static void LogMessageToFile(String message)
        {
            if (!_logging)
                return;

            try
            {
                LogFile.Append("<tr><td style=\"color:green\" valign=\"top\">Message</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + message + "</td></tr>");
            }
            catch
            {
                //do nothing
            }
        }

        internal static void LogErrorToFile(String message)
        {
            if (!_logging)
                return;

            try
            {
                LogFile.Append("<tr><td style=\"color:red\" valign=\"top\">Error</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + message + "</td></tr>");
            }
            catch
            {
                //do nothing
            }
        }

        public static void GoSubscribe()
        {
            Help.ShowHelp(null, MainForm.Website+"/subscribe.aspx");
        }

        private void ActivateToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.Enable();
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vf = ((VolumeLevel) ContextTarget);
                    vf.Enable();
                }
            }
        }

        private void WebsiteToolstripMenuItemClick(object sender, EventArgs e)
        {
            StartBrowser(MainForm.Website+"/");
        }

        private void HelpToolstripMenuItemClick(object sender, EventArgs e)
        {
            StartBrowser(MainForm.Website+"/help.aspx");
        }

        private void ShowToolstripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(-1);
        }

        private void ShowForm(double opacity)
        {
            Activate();
            Visible = true;
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            if (opacity > -1)
                Opacity = opacity;
            TopMost = true;
            TopMost = false;//need to force a switch to move above other forms
            TopMost = Conf.AlwaysOnTop;
            BringToFront();
            Focus();
        }

        private void UnlockToolstripMenuItemClick(object sender, EventArgs e)
        {
            ShowUnlock();
        }

        private void ShowUnlock()
        {
            var cp = new CheckPassword();
            cp.ShowDialog(this);
            if (cp.DialogResult == DialogResult.OK)
            {
                Activate();
                Visible = true;
                if (WindowState == FormWindowState.Minimized)
                {
                    Show();
                    WindowState = FormWindowState.Normal;
                }
                Focus();
            }
            cp.Dispose();
        }

        private void NotifyIcon1Click(object sender, EventArgs e)
        {
        }

        private void AddCameraToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(3);
        }

        private void AddMicrophoneToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddMicrophone(0);
        }

        private void CtxtMainFormOpening(object sender, CancelEventArgs e)
        {
            if (_ctxtMnu.Visible)
                e.Cancel = true;
        }


        public static void StartBrowser(string url)
        {
            if (url != "")
                Help.ShowHelp(null, url);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuItem3Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void MenuItem18Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(LocRm.GetString("AreYouSure"), LocRm.GetString("Confirm"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                return;
            string loc = Conf.MediaDirectory + "audio\\";

            if (Directory.Exists(loc))
            {

                string[] files = Directory.GetFiles(loc, "*.*", SearchOption.AllDirectories);
                foreach (string t in files)
                {
                    try
                    {

                        File.Delete(t);
                    }
                    catch
                    {
                    }
                }
            }
            loc = Conf.MediaDirectory + "video\\";
            if (Directory.Exists(loc))
            {
                string[] files = Directory.GetFiles(loc, "*.*", SearchOption.AllDirectories);
                foreach (string t in files)
                {
                    try
                    {
                        File.Delete(t);
                    }
                    catch
                    {
                    }
                }
            }
            foreach (objectsCamera oc in Cameras)
            {
                CameraWindow occ = GetCameraWindow(oc.id);
                if (occ != null)
                {
                    occ.FileList.Clear();
                }
            }
            foreach (objectsMicrophone om in Microphones)
            {
                VolumeLevel omc = GetMicrophone(om.id);
                if (omc != null)
                {
                   omc.FileList.Clear();
                }
            }
            MessageBox.Show(LocRm.GetString("FilesDeleted"), LocRm.GetString("Note"));
        }

        private void MenuItem20Click(object sender, EventArgs e)
        {
            Process.Start(Program.AppDataPath + "log_" + NextLog + ".htm");
        }

        private void ResetSizeToolStripMenuItemClick(object sender, EventArgs e)
        {
            Minimize(ContextTarget, true);           
        }

        private void Minimize(object obj, bool tocontents)
        {
            if (obj.GetType() == typeof(CameraWindow))
            {
                var cw = (CameraWindow)obj;
                if (cw.RestoreRect != Rectangle.Empty && !tocontents)
                {
                    cw.Location = cw.RestoreRect.Location;
                    cw.Height = cw.RestoreRect.Height;
                    cw.Width = cw.RestoreRect.Width;
                }
                else
                {
                    if (cw.Camera != null && !cw.Camera.LastFrameNull)
                    {
                        cw.Width = cw.Camera.LastFrameUnmanaged.Width + 2;
                        cw.Height = cw.Camera.LastFrameUnmanaged.Height + 26;
                    }
                    else
                    {
                        cw.Width = 322;
                        cw.Height = 266;
                    }
                }
                cw.Invalidate();
            }

            if (obj.GetType() == typeof(FloorPlanControl))
            {
                var fp = (FloorPlanControl)obj;
                if (fp.RestoreRect != Rectangle.Empty && !tocontents)
                {
                    fp.Location = fp.RestoreRect.Location;
                    fp.Height = fp.RestoreRect.Height;
                    fp.Width = fp.RestoreRect.Width;
                    fp.Invalidate();
                }
                else
                {
                    if (fp.ImgPlan != null)
                    {
                        fp.Width = fp.ImgPlan.Width + 2;
                        fp.Height = fp.ImgPlan.Height + 26;
                    }
                    else
                    {
                        fp.Width = 322;
                        fp.Height = 266;
                    }
                }
            }
        }

        private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowSettings(0);
        }

        private void FullScreenToolStripMenuItemClick(object sender, EventArgs e)
        {
            _fullScreenToolStripMenuItem.Checked = !_fullScreenToolStripMenuItem.Checked;
            if (_fullScreenToolStripMenuItem.Checked)
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.None;
                WinApi.SetWinFullScreen(Handle);
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.Sizable;
            }
            Conf.Fullscreen = _fullScreenToolStripMenuItem.Checked;
        }

        private void MenuItem19Click(object sender, EventArgs e)
        {
            if (Cameras.Count == 0 && Microphones.Count == 0)
            {
                MessageBox.Show(LocRm.GetString("NothingToExport"), LocRm.GetString("Error"));
                return;
            }

            var saveFileDialog = new SaveFileDialog
                                     {
                                         InitialDirectory = Program.AppDataPath,
                                         Filter = "iSpy Files (*.ispy)|*.ispy|XML Files (*.xml)|*.xml"
                                     };

            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;

                if (fileName.Trim() != "")
                {
                    SaveObjects(fileName);
                }
            }
            saveFileDialog.Dispose();
        }


        private void MenuItem21Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = Program.AppDataPath;
                ofd.Filter = "iSpy Files (*.ispy)|*.ispy|XML Files (*.xml)|*.xml";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string fileName = ofd.FileName;

                    if (fileName.Trim() != "")
                    {
                        LoadObjectList(fileName.Trim());
                    }
                }
            }
        }

        private void LoadObjectList(string fileName)
        {
            _houseKeepingTimer.Stop();
            _tsslStats.Text = LocRm.GetString("Loading");
            Application.DoEvents();
            RemoveObjects();
            LoadObjects(fileName);
            RenderObjects();
            Application.DoEvents();
            _houseKeepingTimer.Start();
        }

        public void AddObjectExternal(int objectTypidId, int sourceIndex, int width, int height, string name, string url)
        {
            if (!VlcHelper.VlcInstalled && sourceIndex == 5)
                return;
            switch (objectTypidId)
            {
                case 2:
                    if (Cameras.FirstOrDefault(p => p.settings.videosourcestring == url) == null)
                    {
                        if (InvokeRequired)
                            Invoke(new AddObjectExternalDelegate(AddCameraExternal), sourceIndex, url, width, height,
                                   name);
                        else
                            AddCameraExternal(sourceIndex, url, width, height, name);
                    }
                    break;
                case 1:
                    if (Microphones.FirstOrDefault(p => p.settings.sourcename == url) == null)
                    {
                        if (InvokeRequired)
                            Invoke(new AddObjectExternalDelegate(AddMicrophoneExternal), sourceIndex, url, width, height,
                                   name);
                        else
                            AddMicrophoneExternal(sourceIndex, url, width, height, name);
                    }
                    break;
            }
            NeedsSync = true;
        }

        private void AddCameraExternal(int sourceIndex, string url, int width, int height, string name)
        {
            CameraWindow cw = NewCameraWindow(sourceIndex);
            cw.Camobject.settings.desktopresizewidth = width;
            cw.Camobject.settings.desktopresizeheight = height;
            cw.Camobject.name = name;

            cw.Camobject.settings.videosourcestring = url;

            cw.Camobject.id = Conf.NextCameraID;
            Cameras.Add(cw.Camobject);
            string path = Conf.MediaDirectory + "video\\" + cw.Camobject.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Conf.MediaDirectory + "video\\" + cw.Camobject.directory + "\\thumbs\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Conf.NextCameraID++;
            SetNewStartPosition();
            cw.Enable();
            cw.NeedSizeUpdate = true;
        }

        private void AddMicrophoneExternal(int sourceIndex, string url, int width, int height, string name)
        {
            VolumeLevel vl = NewVolumeLevel(sourceIndex);
            vl.Micobject.name = name;
            vl.Micobject.settings.sourcename = url;

            vl.Micobject.id = Conf.NextMicrophoneID;
            Microphones.Add(vl.Micobject);
            string path = Conf.MediaDirectory + "audio\\" + vl.Micobject.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Conf.NextCameraID++;
            SetNewStartPosition();
            vl.Enable();
        }


        private void ToolStripMenuItem1Click(object sender, EventArgs e)
        {
            Connect();
        }

        public void Connect()
        {
            Connect(MainForm.Website+"/watch.aspx");
        }

        public void Connect(string successUrl)
        {
            if (!MWS.Running)
                StopAndStartServer();
            if (WSW.WebsiteLive)
            {
                if (Conf.WSUsername != null && Conf.WSUsername.Trim() != "")
                {
                    if (Conf.UseUPNP)
                    {
                        NATControl.SetPorts(Conf.ServerPort, Conf.LANPort);
                    }
                    WSW.Connect();
                    WSW.ForceSync();
                    if (WSW.WebsiteLive)
                    {
                        if (successUrl != "")
                            StartBrowser(successUrl);
                        return;
                    }
                    MessageBox.Show(LocRm.GetString("WebsiteDown"), LocRm.GetString("Error"));
                    return;
                }
                var ws = new Webservices();
                ws.ShowDialog(this);
                if (ws.EmailAddress != "")
                    EmailAddress = ws.EmailAddress;
                if (ws.DialogResult == DialogResult.Yes || ws.DialogResult == DialogResult.No)
                {
                    ws.Dispose();
                    Connect(successUrl);
                    return;
                }
                ws.Dispose();
            }
        }

        private void MenuItem7Click(object sender, EventArgs e)
        {
            string foldername = Conf.MediaDirectory + "video\\";
            if (!foldername.EndsWith(@"\"))
                foldername += @"\";
            Process.Start(foldername);
        }

        private void MenuItem23Click(object sender, EventArgs e)
        {
            string foldername = Conf.MediaDirectory + "audio\\";
            if (!foldername.EndsWith(@"\"))
                foldername += @"\";
            Process.Start(foldername);
        }

        private void MenuItem25Click(object sender, EventArgs e)
        {
            ViewMobile();
        }

        
        private void MainFormHelpButtonClicked(object sender, CancelEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/help.aspx");
        }

       
        private void BtnOkClick(object sender, EventArgs e)
        {
            _pnlGettingStarted.Hide();
        }

        private void ShowISpy10PercentOpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(.1);
        }

        private void ShowISpy30OpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(.3);
        }

        private void ShowISpy100PercentOpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(1);
        }

        private void CtxtTaskbarOpening(object sender, CancelEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (Conf.Enable_Password_Protect)
                {
                    _unlockToolstripMenuItem.Visible = true;
                    _showToolstripMenuItem.Visible =
                        _showISpy10PercentOpacityToolStripMenuItem.Visible =
                        _showISpy30OpacityToolStripMenuItem.Visible =
                        _showISpy100PercentOpacityToolStripMenuItem.Visible = false;
                    _exitToolStripMenuItem.Visible = false;
                    _websiteToolstripMenuItem.Visible = false;
                    _helpToolstripMenuItem.Visible = false;
                    _switchAllOffToolStripMenuItem.Visible = false;
                    _switchAllOnToolStripMenuItem.Visible = false;
                }
                else
                {
                    _unlockToolstripMenuItem.Visible = false;
                    _showToolstripMenuItem.Visible =
                        _showISpy10PercentOpacityToolStripMenuItem.Visible =
                        _showISpy30OpacityToolStripMenuItem.Visible =
                        _showISpy100PercentOpacityToolStripMenuItem.Visible = true;
                    _exitToolStripMenuItem.Visible = true;
                    _websiteToolstripMenuItem.Visible = true;
                    _helpToolstripMenuItem.Visible = true;
                    _switchAllOffToolStripMenuItem.Visible = true;
                    _switchAllOnToolStripMenuItem.Visible = true;
                }
            }
            else
            {
                _showToolstripMenuItem.Visible = false;
                _showISpy10PercentOpacityToolStripMenuItem.Visible =
                    _showISpy30OpacityToolStripMenuItem.Visible =
                    _showISpy100PercentOpacityToolStripMenuItem.Visible = true;
                _unlockToolstripMenuItem.Visible = false;
                _exitToolStripMenuItem.Visible = true;
                _websiteToolstripMenuItem.Visible = true;
                _helpToolstripMenuItem.Visible = true;
                _switchAllOffToolStripMenuItem.Visible = true;
                _switchAllOnToolStripMenuItem.Visible = true;
            }
        }

        private void SaveObjects(string fileName)
        {
            if (fileName == "")
                fileName = Program.AppDataPath + @"XML\objects.xml";
            var c = new objects();
            foreach (objectsCamera oc in Cameras)
            {
                CameraWindow occ = GetCameraWindow(oc.id);
                if (occ != null)
                {
                    oc.width = occ.Width;
                    oc.height = occ.Height;
                    oc.x = occ.Location.X;
                    oc.y = occ.Location.Y;
                    occ.SaveFileList();
                }
            }
            c.cameras = Cameras.ToArray();
            foreach (objectsMicrophone om in Microphones)
            {
                VolumeLevel omc = GetMicrophone(om.id);
                if (omc != null)
                {
                    om.width = omc.Width;
                    om.height = omc.Height;
                    om.x = omc.Location.X;
                    om.y = omc.Location.Y;
                    omc.SaveFileList();
                }
            }
            c.microphones = Microphones.ToArray();
            foreach (objectsFloorplan of in FloorPlans)
            {
                FloorPlanControl fpc = GetFloorPlan(of.id);
                if (fpc != null)
                {
                    of.width = fpc.Width;
                    of.height = fpc.Height;
                    of.x = fpc.Location.X;
                    of.y = fpc.Location.Y;
                }
            }
            c.floorplans = FloorPlans.ToArray();
            c.remotecommands = RemoteCommands.ToArray();

            var s = new XmlSerializer(typeof (objects));
            var fs = new FileStream(fileName, FileMode.Create);
            TextWriter writer = new StreamWriter(fs);
            fs.Position = 0;
            s.Serialize(writer, c);
            fs.Close();
        }

        private void SaveConfig()
        {

            string fileName = Program.AppDataPath + @"XML\config.xml";
            //save configuration
            var s = new XmlSerializer(typeof(configuration));
            var fs = new FileStream(fileName, FileMode.Create);
            var writer = new StreamWriter(fs);
            fs.Position = 0;
            s.Serialize(writer, Conf);
            fs.Close();
        }

        private void StatusBarToolStripMenuItemClick(object sender, EventArgs e)
        {
            _statusBarToolStripMenuItem.Checked = !_statusBarToolStripMenuItem.Checked;
            _statusStrip1.Visible = _statusBarToolStripMenuItem.Checked;

            Conf.ShowStatus = _statusBarToolStripMenuItem.Checked;
        }

        private void FileMenuToolStripMenuItemClick(object sender, EventArgs e)
        {
            _fileMenuToolStripMenuItem.Checked = !_fileMenuToolStripMenuItem.Checked;
            Menu = !_fileMenuToolStripMenuItem.Checked ? null : _mainMenu;

            Conf.ShowFileMenu = _fileMenuToolStripMenuItem.Checked;
        }

        private void ToolStripToolStripMenuItemClick(object sender, EventArgs e)
        {
            _toolStripToolStripMenuItem.Checked = !_toolStripToolStripMenuItem.Checked;
            _toolStrip1.Visible = _toolStripToolStripMenuItem.Checked;
            Conf.ShowToolbar = _toolStripToolStripMenuItem.Checked;
        }

        private void MenuItem26Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/donate.aspx");
        }

        private void RecordNowToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.RecordSwitch(!cameraControl.Recording);
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var volumeControl = ((VolumeLevel) ContextTarget);
                    volumeControl.RecordSwitch(!volumeControl.Recording);
                }
            }
        }

        private void ShowFilesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cw = ((CameraWindow) ContextTarget);
                string foldername = Conf.MediaDirectory + "video\\" + cw.Camobject.directory +
                                     "\\";
                if (!foldername.EndsWith(@"\"))
                    foldername += @"\";
                Process.Start(foldername);
                cw.Camobject.newrecordingcount = 0;
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vl = ((VolumeLevel) ContextTarget);
                    string foldername = Conf.MediaDirectory + "audio\\" + vl.Micobject.directory +
                                         "\\";
                    if (!foldername.EndsWith(@"\"))
                        foldername += @"\";
                    Process.Start(foldername);
                    vl.Micobject.newrecordingcount = 0;
                }
                else
                {
                    string foldername = Conf.MediaDirectory;
                    Process.Start(foldername);
                }
            }
        }

        private void ViewMediaOnAMobileDeviceToolStripMenuItemClick(object sender, EventArgs e)
        {
            ViewMobile();
        }

        private void ViewMobile()
        {
            Connect(MainForm.Website+"/mobile/");
        }

        private void AddFloorPlanToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddFloorPlan();
        }

        private void ListenToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                var vf = ((VolumeLevel) ContextTarget);
                vf.Listening = !vf.Listening;
            }
        }

        private void PnlCamerasMouseUp(object sender, MouseEventArgs e)
        {
        }

        private void OpacityToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowForm(.1);
        }

        private void OpacityToolStripMenuItem1Click(object sender, EventArgs e)
        {
            ShowForm(.3);
        }

        private void OpacityToolStripMenuItem2Click(object sender, EventArgs e)
        {
            ShowForm(1);
        }

        private void MenuItem31Click(object sender, EventArgs e)
        {
            RemoveObjects();
        }

        private void MenuItem34Click(object sender, EventArgs e)
        {
        }

        public void SwitchObjects(bool scheduledOnly, bool on)
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c.GetType() == typeof (CameraWindow))
                {
                    var cameraControl = (CameraWindow) c;
                    if (on && !cameraControl.Camobject.settings.active)
                    {
                        if (!scheduledOnly || cameraControl.Camobject.schedule.active)
                            cameraControl.Enable();
                    }

                    if (!on && cameraControl.Camobject.settings.active)
                    {
                        if (!scheduledOnly || cameraControl.Camobject.schedule.active)
                            cameraControl.Disable();
                    }
                }
                if (c.GetType() == typeof (VolumeLevel))
                {
                    var volumeControl = (VolumeLevel) c;

                    if (on && !volumeControl.Micobject.settings.active)
                    {
                        if (!scheduledOnly || volumeControl.Micobject.schedule.active)
                            volumeControl.Enable();
                    }

                    if (!on && volumeControl.Micobject.settings.active)
                    {
                        if (!scheduledOnly || volumeControl.Micobject.schedule.active)
                            volumeControl.Disable();
                    }
                }
            }
        }

        public void RecordOnDetect(bool on)
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c.GetType() == typeof(CameraWindow))
                {
                    var cameraControl = (CameraWindow)c;
                    cameraControl.Camobject.detector.recordondetect = on;
                }
                if (c.GetType() == typeof(VolumeLevel))
                {
                    var volumeControl = (VolumeLevel)c;
                    volumeControl.Micobject.detector.recordondetect = on;
                }
            }
        }

        public void AlertsActive(bool on)
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c.GetType() == typeof(CameraWindow))
                {
                    var cameraControl = (CameraWindow)c;
                    cameraControl.Camobject.alerts.active = on;
                }
                if (c.GetType() == typeof(VolumeLevel))
                {
                    var volumeControl = (VolumeLevel)c;
                    volumeControl.Micobject.alerts.active = on;
                }
            }
        }

        public void RecordAll(bool record)
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c.GetType() == typeof(CameraWindow))
                {
                    var cameraControl = (CameraWindow)c;
                    cameraControl.RecordSwitch(record);
                }
                if (c.GetType() == typeof(VolumeLevel))
                {
                    var volumeControl = (VolumeLevel)c;
                    volumeControl.RecordSwitch(record);
                }
            }
        }

        private void MenuItem33Click(object sender, EventArgs e)
        {
        }

        private void ToolStripButton8Click1(object sender, EventArgs e)
        {
            ShowRemoteCommands();
        }

        private void MenuItem35Click(object sender, EventArgs e)
        {
            ShowRemoteCommands();
        }

        private void ToolStrip1ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        private void RemoteCommandsToolStripMenuItemClick(object sender, EventArgs e)
        {
            ShowRemoteCommands();
        }


        private void ShowRemoteCommands()
        {
            var ma = new RemoteCommands {Owner = this};
            ma.ShowDialog();
            ma.Dispose();
        }

        private void MenuItem37Click(object sender, EventArgs e)
        {
            MessageBox.Show(LocRm.GetString("EditInstruct"), LocRm.GetString("Note"));
        }

        private void PositionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var p = (PictureBox) ContextTarget;
            int w = p.Width;
            int h = p.Height;
            int x = p.Location.X;
            int y = p.Location.Y;

            var le = new LayoutEditor {X = x, Y = y, W = w, H = h};


            if (le.ShowDialog() == DialogResult.OK)
            {
                PositionPanel(p, new Point(le.X, le.Y), le.W, le.H);
            }
            le.Dispose();
        }

        private static void PositionPanel(PictureBox p, Point xy, int w, int h)
        {
            p.Width = w;
            p.Height = h;
            p.Location = new Point(xy.X, xy.Y);
        }

        private void ChkShowGettingStartedCheckedChanged(object sender, EventArgs e)
        {
            Conf.Enabled_ShowGettingStarted = _chkShowGettingStarted.Checked;
        }

        private void MenuItem38Click(object sender, EventArgs e)
        {
            StartBrowser(MainForm.Website+"/producthistory.aspx?productid=11");
        }

        private void AutoLayoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            _autoLayoutToolStripMenuItem.Checked = !_autoLayoutToolStripMenuItem.Checked;
            Conf.AutoLayout = _autoLayoutToolStripMenuItem.Checked;
            if (Conf.AutoLayout)
                LayoutObjects(0, 0);
        }

        private void LayoutObjects(int w, int h)
        {
            _pnlCameras.HorizontalScroll.Value = 0;
            _pnlCameras.VerticalScroll.Value = 0;
            _pnlCameras.Refresh();
            int num = _pnlCameras.Controls.Count;
            if (_pnlGettingStarted.Visible)
                num--;
            if (num==0)
                return;
            // Get data.
            var rectslist = new List<Rectangle>();
            
            foreach (Control c in _pnlCameras.Controls)
            {
                bool _skip = false;
                if (c is PictureBox)
                {
                    var p = (PictureBox) c;
                    if (w > 0)
                    {
                        p.Width = w;
                        p.Height = h;
                    }
                    if (w==-1)
                    {
                        if (c is CameraWindow)
                        {
                            var cw = ((CameraWindow) c);
                            if (cw.Camera != null && cw.Camera.LastFrame!=null)
                            {
                                p.Width = cw.Camera.LastFrame.Width + 2;
                                p.Height = cw.Camera.LastFrame.Height + 32;
                            }
                        }
                        else
                        {
                            p.Width = c.Width;
                            p.Height = c.Height;
                        }
                    }
                    int nh = p.Height;
                    if (c is CameraWindow)
                    {
                        if (((CameraWindow)c).VolumeControl != null)
                            nh += 40;
                    }
                    if (c is VolumeLevel)
                    {
                        if (((VolumeLevel)c).Paired)
                            _skip = true;
                    }
                    if (!_skip)
                    {
                        rectslist.Add(new Rectangle(0, 0, p.Width, nh));
                    }
                }
            }
            // Arrange the rectangles.
            Rectangle[] rects = rectslist.ToArray();
            int binWidth = _pnlCameras.Width;
            var proc = new C2BPProcessor();
            proc.SubAlgFillOneColumn(binWidth, rects);
            rectslist = rects.ToList();
            bool assigned = true;
            var indexesassigned = new List<int>();
            while (assigned)
            {
                assigned = false;
                foreach (Rectangle r in rectslist)
                {
                    for (int i = 0; i < _pnlCameras.Controls.Count; i++)
                    {
                        Control c = _pnlCameras.Controls[i];
                        bool _skip = false;
                        int _hoffset = 0;
                        if (!indexesassigned.Contains(i) && c is PictureBox)
                        {
                            if (c is CameraWindow)
                            {
                                var cw = ((CameraWindow)c);
                                if (cw.VolumeControl != null)
                                    _hoffset = 40;
                            }
                            if (c is VolumeLevel)
                            {
                                if (((VolumeLevel)c).Paired)
                                    _skip = true;
                            }
                            if (!_skip && c.Width == r.Width && c.Height + _hoffset == r.Height)
                            {
                                PositionPanel((PictureBox)c, new Point(r.X, r.Y), r.Width, r.Height - _hoffset);
                                rectslist.Remove(r);
                                assigned = true;
                                indexesassigned.Add(i);
                                break;
                            }
                        }
                    }
                    if (assigned)
                        break;
                }
            }
            NeedsRedraw = true;
        }

        private void MenuItem39Click(object sender, EventArgs e)
        {
        }

        private void TakePhotoToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                Help.ShowHelp(this,
                              "http://" + IPAddress + ":" + Conf.LANPort + "/livefeed?oid=" +
                              cameraControl.Camobject.id + "&r=" + Random.NextDouble() + "&full=1&auth=" + Identifier);
            }
        }

        private void ToolStripDropDownButton1Click(object sender, EventArgs e)
        {
        }

        private void ThruWebsiteToolStripMenuItemClick(object sender, EventArgs e)
        {
            WebConnect();
        }

        private void OnMobileDevicesToolStripMenuItemClick(object sender, EventArgs e)
        {
            ViewMobile();
        }

        private void LocalCameraToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(3);
        }

        private void IpCameraToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(1);
        }

        private void MicrophoneToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddMicrophone(0);
        }

        private void FloorPlanToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddFloorPlan();
        }

        private void MenuItem12Click(object sender, EventArgs e)
        {
            //+26 height for control bar
            LayoutObjects(164, 146);
        }

        private void MenuItem14Click(object sender, EventArgs e)
        {
            LayoutObjects(324, 266);
        }

        private void MenuItem29Click1(object sender, EventArgs e)
        {
            LayoutObjects(0, 0);
        }

        private void ToolStripButton1Click1(object sender, EventArgs e)
        {
            WebConnect();
        }

        private void WebConnect()
        {
            var ws = new Webservices();
            ws.ShowDialog(this);
            if (ws.EmailAddress != "")
            {
                EmailAddress = ws.EmailAddress;
                MobileNumber = ws.MobileNumber;
            }
            if (ws.DialogResult == DialogResult.Yes)
            {
                Connect();
            }
            ws.Dispose();
            Text = "iSpy v" + Application.ProductVersion;
            if (Conf.WSUsername != "")
            {
                Text += " (" + Conf.WSUsername + ")";
            }
        }

        private void MenuItem17Click(object sender, EventArgs e)
        {
        }

        private void ResetRecordingCounterToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var _cw = ((CameraWindow) ContextTarget);
                _cw.Camobject.newrecordingcount = 0;
                if (_cw.VolumeControl != null)
                {
                    _cw.VolumeControl.Micobject.newrecordingcount = 0;
                    _cw.VolumeControl.Invalidate();
                }
                _cw.Invalidate();
            }
            if (ContextTarget.GetType() == typeof (VolumeLevel))
            {
                var _vw = ((VolumeLevel)ContextTarget);
                _vw.Micobject.newrecordingcount = 0;
                if (_vw.Paired)
                {
                    objectsCamera oc = MainForm.Cameras.SingleOrDefault(p => p.settings.micpair == _vw.Micobject.id);
                    if (oc != null)
                    {
                        CameraWindow _cw = GetCameraWindow(oc.id);
                        _cw.Camobject.newrecordingcount = 0;
                        _cw.Invalidate();
                    }
                }
                _vw.Invalidate();
            }
        }

        private void MenuItem15Click(object sender, EventArgs e)
        {
            foreach (Control c in _pnlCameras.Controls)
            {
                if (c.GetType() == typeof (CameraWindow))
                {
                    var cameraControl = (CameraWindow) c;
                    cameraControl.Camobject.newrecordingcount = 0;
                    cameraControl.Invalidate();
                }
                if (c.GetType() != typeof (VolumeLevel)) continue;
                var volumeControl = (VolumeLevel) c;
                volumeControl.Micobject.newrecordingcount = 0;
                volumeControl.Invalidate();
            }
        }

        private void SwitchAllOnToolStripMenuItemClick(object sender, EventArgs e)
        {
            SwitchObjects(false, true);
        }

        private void SwitchAllOffToolStripMenuItemClick(object sender, EventArgs e)
        {
            SwitchObjects(false, false);
        }

        private void MenuItem22Click1(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              InitialDirectory = Program.AppDataPath,
                              Filter = "iSpy Log Files (*.htm)|*.htm"
                          };

            if (ofd.ShowDialog(this) != DialogResult.OK) return;
            string fileName = ofd.FileName;

            if (fileName.Trim() != "")
            {
                Process.Start(ofd.FileName);
            }
        }

        private void USbCamerasAndMicrophonesOnOtherToolStripMenuItemClick(object sender, EventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/download_ispyserver.aspx");
        }

        private void PnlCamerasPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }

        private void DdlLanguageSelectedIndexChanged(object sender, EventArgs e)
        {
            string lang = ((ListItem) _ddlLanguage.SelectedItem).Value[0];
            if (lang != Conf.Language)
            {
                Conf.Language = lang;
                LocRm.CurrentSet = null;
                RenderResources();
            }
        }

        private void VLcSourceToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddCamera(5);
        }

        private void VLcMicrophoneSourceToolStripMenuItemClick(object sender, EventArgs e)
        {
            AddMicrophone(2);
        }

        private void MenuItem24Click(object sender, EventArgs e)
        {
            SwitchObjects(false, true);
        }

        private void MenuItem40Click(object sender, EventArgs e)
        {
            SwitchObjects(false, false);
        }

        private void MenuItem41Click(object sender, EventArgs e)
        {
            SwitchObjects(true, false);
        }

        private void MenuItem28Click1(object sender, EventArgs e)
        {
            SwitchObjects(true, true);
        }

        private void MenuItem24Click1(object sender, EventArgs e)
        {
            ApplySchedule();
        }

        public void ApplySchedule()
        {
            foreach (objectsCamera cam in _cameras)
            {
                if (cam.schedule.active)
                {
                    CameraWindow cw = GetCamera(cam.id);
                    cw.ApplySchedule();
                }
            }


            foreach (objectsMicrophone mic in _microphones)
            {
                if (mic.schedule.active)
                {
                    VolumeLevel vl = GetMicrophone(mic.id);
                    vl.ApplySchedule();
                }
            }
        }

        private void ApplyScheduleToolStripMenuItemClick1(object sender, EventArgs e)
        {
            ApplySchedule();
        }

        private void ApplyScheduleToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (ContextTarget.GetType() == typeof (CameraWindow))
            {
                var cameraControl = ((CameraWindow) ContextTarget);
                cameraControl.ApplySchedule();
            }
            else
            {
                if (ContextTarget.GetType() == typeof (VolumeLevel))
                {
                    var vf = ((VolumeLevel) ContextTarget);
                    vf.ApplySchedule();
                }
            }
        }

        private void MenuItem24Click2(object sender, EventArgs e)
        {
            _pnlGettingStarted.Show();
            _wbGettingStarted.Navigate(MainForm.Website+"/getting_started.aspx");
            _pnlGettingStarted.BringToFront();
        }

        private void PnlGettingStartedLoad(object sender, EventArgs e)
        {
        }

        private void MenuItem28Click2(object sender, EventArgs e)
        {
            LayoutObjects(644, 506);
        }

        #region PanelEvents

        private static void PanelMouseMove(object sender, MouseEventArgs e)
        {
            var ctrl = (Grouper) sender;
            if (e.Button == MouseButtons.Left && ctrl.Tag!=null)
            {
                int newLeft = ctrl.Left + (e.X - ((Point) ctrl.Tag).X);
                int newTop = ctrl.Top + (e.Y - ((Point) ctrl.Tag).Y);
                ctrl.Left = newLeft;
                ctrl.Top = newTop;
            }
            ctrl.Focus();
        }

        private static void PanelMouseDown(object sender, MouseEventArgs e)
        {
            var ctrl = (Grouper) sender;

            ctrl.SuspendLayout();
            if (e.Button != MouseButtons.Left) return;
            ctrl.Tag = new Point(e.X, e.Y);
            ctrl.BringToFront();
        }

        private static void PanelMouseUp(object sender, MouseEventArgs e)
        {
            var ctrl = (Grouper) sender;
            ctrl.ResumeLayout();
            if (e.Button == MouseButtons.Left)
            {
                ctrl.Tag = new Point(e.X, e.Y);
            }
        }

        private static void PanelMouseLeave(object sender, EventArgs e)
        {
            var ctrl = (Grouper) sender;
            ctrl.Cursor = Cursors.Default;
        }

        private static void PanelMouseEnter(object sender, EventArgs e)
        {
            var ctrl = (Grouper) sender;
            ctrl.Cursor = Cursors.Hand;
        }

        #endregion

        #region CameraEvents

        private void CameraControlMouseMove(object sender, MouseEventArgs e)
        {
            var cameraControl = (CameraWindow) sender;
            if (e.Button == MouseButtons.Left)
            {
                int newLeft = cameraControl.Left + (e.X - cameraControl.Camobject.x);
                int newTop = cameraControl.Top + (e.Y - cameraControl.Camobject.y);
                if (newLeft + cameraControl.Width > 5 && newLeft < ClientRectangle.Width - 5)
                {
                    cameraControl.Left = newLeft;
                }
                if (newTop + cameraControl.Height > 5 && newTop < ClientRectangle.Height - 50)
                {
                    cameraControl.Top = newTop;
                }
            }
            cameraControl.Focus();
        }

        private void CameraControlMouseDown(object sender, MouseEventArgs e)
        {
            var cameraControl = (CameraWindow) sender;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    cameraControl.Camobject.x = e.X;
                    cameraControl.Camobject.y = e.Y;
                    cameraControl.BringToFront();
                    break;
                case MouseButtons.Right:
                    ContextTarget = cameraControl;
                    _setInactiveToolStripMenuItem.Visible = false;
                    _activateToolStripMenuItem.Visible = false;
                    _recordNowToolStripMenuItem.Visible = false;
                    _listenToolStripMenuItem.Visible = false;
                    _applyScheduleToolStripMenuItem1.Visible = true;
                    _resetRecordingCounterToolStripMenuItem.Visible = true;
                    _resetRecordingCounterToolStripMenuItem.Text = LocRm.GetString("ResetRecordingCounter") + " (" +
                                                                   cameraControl.Camobject.newrecordingcount + ")";
                    pTZToolStripMenuItem.Visible = false;
                    if (cameraControl.Camobject.settings.active)
                    {
                        _setInactiveToolStripMenuItem.Visible = true;
                        _recordNowToolStripMenuItem.Visible = true;
                        _takePhotoToolStripMenuItem.Visible = true;
                        if (cameraControl.Camobject.ptz > -1)
                        {
                            pTZToolStripMenuItem.Visible = true;
                            pTZToolStripMenuItem.DropDownItems.Clear();

                            PTZSettingsCamera ptz = PTZs.Single(p => p.id == cameraControl.Camobject.ptz);

                            foreach (PTZSettingsCameraCommandsCommand extcmd in ptz.Commands.ExtendedCommands)
                            {
                                ToolStripItem tsi = new ToolStripMenuItem {Text = extcmd.Name, Tag = cameraControl.Camobject.id+"|"+extcmd.Value};
                                tsi.Click += new EventHandler(tsi_Click);
                                pTZToolStripMenuItem.DropDownItems.Add(tsi);
                            }

                        }
                    }
                    else
                    {
                        _activateToolStripMenuItem.Visible = true;
                        _recordNowToolStripMenuItem.Visible = false;
                        _takePhotoToolStripMenuItem.Visible = false;
                    }
                    _recordNowToolStripMenuItem.Text = LocRm.GetString(cameraControl.Recording ? "StopRecording" : "StartRecording");
                    _ctxtMnu.Show(cameraControl, new Point(e.X, e.Y));
                    break;
                case MouseButtons.Middle:
                    cameraControl.PTZReference = new Point(cameraControl.Width/2, cameraControl.Height/2);
                    cameraControl.PTZNavigate = true;
                    break;
            }
        }

        void tsi_Click(object sender, EventArgs e)
        {
            string[] cfg = ((ToolStripMenuItem) sender).Tag.ToString().Split('|');
            int camid = Convert.ToInt32(cfg[0]);
            GetCameraWindow(camid).PTZ.SendPTZCommand(cfg[1]);
        }

        private static void CameraControlMouseWheel(object sender, MouseEventArgs e)
        {
            var cameraControl = (CameraWindow)sender;

            cameraControl.PTZNavigate = false;
            PTZSettingsCamera ptz = PTZs.SingleOrDefault(p => p.id == cameraControl.Camobject.ptz);
            if (ptz == null) return;
            if (e != null)
            {
                cameraControl.CalibrateCount = 0;
                cameraControl.Calibrating = true;
                cameraControl.PTZ.SendPTZCommand(e.Delta > 0 ? ptz.Commands.ZoomIn : ptz.Commands.ZoomOut, true);
            }
        }

        private static void CameraControlMouseUp(object sender, MouseEventArgs e)
        {
            var cameraControl = (CameraWindow) sender;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    cameraControl.Camobject.x = cameraControl.Left;
                    cameraControl.Camobject.y = cameraControl.Top;
                    break;
                case MouseButtons.Middle:
                    cameraControl.PTZNavigate = false;
                    PTZSettingsCamera ptz = PTZs.SingleOrDefault(p => p.id == cameraControl.Camobject.ptz);
                    if (ptz != null)
                        cameraControl.PTZ.SendPTZCommand(ptz.Commands.Stop,true);
                    break;
            }          
        }

        private static void CameraControlMouseLeave(object sender, EventArgs e)
        {
            var cameraControl = (CameraWindow) sender;
            cameraControl.Cursor = Cursors.Default;
        }

        private static void CameraControlMouseEnter(object sender, EventArgs e)
        {
            var cameraControl = (CameraWindow) sender;
            cameraControl.Cursor = Cursors.Hand;
        }

        private void CameraControlDoubleClick(object sender, EventArgs e)
        {
            Maximise(sender);
        }

        #endregion

        #region VolumeEvents

        private void VolumeControlMouseDown(object sender, MouseEventArgs e)
        {
            var volumeControl = (VolumeLevel) sender;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (!volumeControl.Paired)
                    {
                        volumeControl.Micobject.x = e.X;
                        volumeControl.Micobject.y = e.Y;
                    }
                    volumeControl.BringToFront();
                    break;
                case MouseButtons.Right:
                    ContextTarget = volumeControl;
                    _setInactiveToolStripMenuItem.Visible = false;
                    _activateToolStripMenuItem.Visible = false;
                    _listenToolStripMenuItem.Visible = true;
                    _takePhotoToolStripMenuItem.Visible = false;
                    _resetRecordingCounterToolStripMenuItem.Visible = true;
                    _applyScheduleToolStripMenuItem1.Visible = true;
                    pTZToolStripMenuItem.Visible = false;
                    _resetRecordingCounterToolStripMenuItem.Text = LocRm.GetString("ResetRecordingCounter") + " (" + volumeControl.Micobject.newrecordingcount + ")";
                    if (volumeControl.Listening)
                    {
                        _listenToolStripMenuItem.Text = LocRm.GetString("StopListening");
                        _listenToolStripMenuItem.Image = Resources.listenoff2;
                    }
                    else
                    {
                        _listenToolStripMenuItem.Text = LocRm.GetString("Listen");
                        _listenToolStripMenuItem.Image = Resources.listen2;
                    }
                    _recordNowToolStripMenuItem.Visible = false;
                    if (volumeControl.Micobject.settings.active)
                    {
                        _setInactiveToolStripMenuItem.Visible = true;
                        _recordNowToolStripMenuItem.Visible = true;
                        _listenToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        _activateToolStripMenuItem.Visible = true;
                        _recordNowToolStripMenuItem.Visible = false;
                        _listenToolStripMenuItem.Enabled = false;
                    }
                    _recordNowToolStripMenuItem.Text = LocRm.GetString(volumeControl.ForcedRecording ? "StopRecording" : "StartRecording");
                    _ctxtMnu.Show(volumeControl, new Point(e.X, e.Y));
                    break;
            }
        }

        private static void VolumeControlMouseUp(object sender, MouseEventArgs e)
        {
            var volumeControl = (VolumeLevel) sender;
            if (e.Button == MouseButtons.Left && !volumeControl.Paired)
            {
                volumeControl.Micobject.x = volumeControl.Left;
                volumeControl.Micobject.y = volumeControl.Top;
            }
        }

        private static void VolumeControlMouseLeave(object sender, EventArgs e)
        {
            var volumeControl = (VolumeLevel) sender;
            volumeControl.Cursor = Cursors.Default;
        }

        private static void VolumeControlMouseEnter(object sender, EventArgs e)
        {
            var volumeControl = (VolumeLevel) sender;
            if (!volumeControl.Paired)
                volumeControl.Cursor = Cursors.Hand;
        }

        private void VolumeControlMouseMove(object sender, MouseEventArgs e)
        {
            var volumeControl = (VolumeLevel) sender;
            if (e.Button == MouseButtons.Left && !volumeControl.Paired)
            {
                int newLeft = volumeControl.Left + (e.X - Convert.ToInt32(volumeControl.Micobject.x));
                int newTop = volumeControl.Top + (e.Y - Convert.ToInt32(volumeControl.Micobject.y));
                if (newLeft + volumeControl.Width > 5 && newLeft < ClientRectangle.Width - 5)
                {
                    volumeControl.Left = newLeft;
                }
                if (newTop + volumeControl.Height > 5 && newTop < ClientRectangle.Height - 50)
                {
                    volumeControl.Top = newTop;
                }
            }
            volumeControl.Focus();
        }

        #endregion

        #region FloorPlanEvents

        private void FloorPlanMouseDown(object sender, MouseEventArgs e)
        {
            var fpc = (FloorPlanControl) sender;
            if (e.Button == MouseButtons.Left)
            {
                fpc.Fpobject.x = e.X;
                fpc.Fpobject.y = e.Y;
                fpc.BringToFront();
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    ContextTarget = fpc;
                    _setInactiveToolStripMenuItem.Visible = false;
                    _listenToolStripMenuItem.Visible = false;
                    _activateToolStripMenuItem.Visible = false;
                    _resetRecordingCounterToolStripMenuItem.Visible = false;
                    _recordNowToolStripMenuItem.Visible = false;
                    _takePhotoToolStripMenuItem.Visible = false;
                    _applyScheduleToolStripMenuItem1.Visible = false;
                    pTZToolStripMenuItem.Visible = false;

                    _ctxtMnu.Show(fpc, new Point(e.X, e.Y));
                }
            }
        }

        private static void FloorPlanMouseUp(object sender, MouseEventArgs e)
        {
            var fpc = (FloorPlanControl) sender;
            if (e.Button == MouseButtons.Left)
            {
                fpc.Fpobject.x = fpc.Left;
                fpc.Fpobject.y = fpc.Top;
            }
        }

        private static void FloorPlanMouseLeave(object sender, EventArgs e)
        {
            var fpc = (FloorPlanControl) sender;
            fpc.Cursor = Cursors.Default;
        }

        private static void FloorPlanMouseEnter(object sender, EventArgs e)
        {
            var fpc = (FloorPlanControl) sender;
            fpc.Cursor = Cursors.Hand;
        }

        private void FloorPlanMouseMove(object sender, MouseEventArgs e)
        {
            var fpc = (FloorPlanControl) sender;
            if (e.Button == MouseButtons.Left)
            {
                int newLeft = fpc.Left + (e.X - Convert.ToInt32(fpc.Fpobject.x));
                int newTop = fpc.Top + (e.Y - Convert.ToInt32(fpc.Fpobject.y));
                if (newLeft + fpc.Width > 5 && newLeft < ClientRectangle.Width - 5)
                {
                    fpc.Left = newLeft;
                }
                if (newTop + fpc.Height > 5 && newTop < ClientRectangle.Height - 50)
                {
                    fpc.Top = newTop;
                }
            }
            fpc.Focus();
        }

        #endregion

        #region RestoreSavedCameras

        internal void DisplayCamera(objectsCamera cam)
        {
            var cameraControl = new CameraWindow(cam);
            SetCameraEvents(cameraControl);
            cameraControl.BackColor = Conf.BackColor.ToColor();
            _pnlCameras.Controls.Add(cameraControl);
            cameraControl.Location = new Point(cam.x, cam.y);
            cameraControl.Size = new Size(cam.width, cam.height);
            cameraControl.BringToFront();

            if (Conf.AutoSchedule && cam.schedule.active && cam.schedule.entries.Count() > 0)
            {
                cam.settings.active = false;
                cameraControl.ApplySchedule();
            }
            else
            {
                if (cam.settings.active)
                    cameraControl.Enable();
            }

            string path = Conf.MediaDirectory + "video\\" + cam.directory + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Conf.MediaDirectory + "video\\" + cam.directory + "\\thumbs\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                //move existing thumbs into directory
                List<string> lfi =
                    Directory.GetFiles(Conf.MediaDirectory + "video\\" + cam.directory + "\\","*.jpg").ToList();
                foreach(string file in lfi)
                {
                    string destfile = file;
                    int i = destfile.LastIndexOf(@"\");
                    destfile = file.Substring(0, i) + @"\thumbs" + file.Substring(i);
                    File.Move(file,destfile);
                }
            }
        }

        private void DoInvoke(string methodName)
        {
            if (methodName == "show")
            {
                Activate();
                Visible = true;
                if (WindowState == FormWindowState.Minimized)
                {
                    Show();
                    WindowState = FormWindowState.Normal;
                }
                return;
            }
            if (methodName.StartsWith("bringtofrontcam"))
            {
                int camid = Convert.ToInt32(methodName.Split(',')[1]);
                foreach (Control c in _pnlCameras.Controls)
                {
                    if (c.GetType() == typeof (CameraWindow))
                    {
                        var cameraControl = (CameraWindow) c;
                        if (cameraControl.Camobject.id == camid)
                        {
                            cameraControl.BringToFront();
                            break;
                        }
                    }
                }
                return;
            }
            if (methodName.StartsWith("bringtofrontmic"))
            {
                int micid = Convert.ToInt32(methodName.Split(',')[1]);
                foreach (Control c in _pnlCameras.Controls)
                {
                    if (c.GetType() == typeof (VolumeLevel))
                    {
                        var vl = (VolumeLevel) c;
                        if (vl.Micobject.id == micid)
                        {
                            vl.BringToFront();
                            break;
                        }
                    }
                }
                return;
            }
        }

        private void CameraControlRemoteCommand(object sender, ThreadSafeCommand e)
        {
            InvokeMethod i = DoInvoke;
            Invoke(i, new object[] {e.Command});
        }

        private delegate void InvokeMethod(string command);

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this._fileItem = new System.Windows.Forms.MenuItem();
            this._menuItem19 = new System.Windows.Forms.MenuItem();
            this._menuItem21 = new System.Windows.Forms.MenuItem();
            this._menuItem1 = new System.Windows.Forms.MenuItem();
            this._exitFileItem = new System.Windows.Forms.MenuItem();
            this._menuItem36 = new System.Windows.Forms.MenuItem();
            this._menuItem37 = new System.Windows.Forms.MenuItem();
            this._menuItem16 = new System.Windows.Forms.MenuItem();
            this._menuItem17 = new System.Windows.Forms.MenuItem();
            this._menuItem7 = new System.Windows.Forms.MenuItem();
            this._menuItem23 = new System.Windows.Forms.MenuItem();
            this._menuItem3 = new System.Windows.Forms.MenuItem();
            this._menuItem25 = new System.Windows.Forms.MenuItem();
            this._menuItem13 = new System.Windows.Forms.MenuItem();
            this._menuItem39 = new System.Windows.Forms.MenuItem();
            this._menuItem12 = new System.Windows.Forms.MenuItem();
            this._menuItem14 = new System.Windows.Forms.MenuItem();
            this._menuItem28 = new System.Windows.Forms.MenuItem();
            this._menuItem29 = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mnuSaveLayout = new System.Windows.Forms.MenuItem();
            this.mnuResetLayout = new System.Windows.Forms.MenuItem();
            this._menuItem20 = new System.Windows.Forms.MenuItem();
            this._menuItem22 = new System.Windows.Forms.MenuItem();
            this._menuItem9 = new System.Windows.Forms.MenuItem();
            this._menuItem18 = new System.Windows.Forms.MenuItem();
            this._menuItem8 = new System.Windows.Forms.MenuItem();
            this._menuItem15 = new System.Windows.Forms.MenuItem();
            this._menuItem6 = new System.Windows.Forms.MenuItem();
            this._menuItem34 = new System.Windows.Forms.MenuItem();
            this._miOnAll = new System.Windows.Forms.MenuItem();
            this._miOnSched = new System.Windows.Forms.MenuItem();
            this._menuItem33 = new System.Windows.Forms.MenuItem();
            this._miOffAll = new System.Windows.Forms.MenuItem();
            this._miOffSched = new System.Windows.Forms.MenuItem();
            this._menuItem31 = new System.Windows.Forms.MenuItem();
            this._miApplySchedule = new System.Windows.Forms.MenuItem();
            this._menuItem32 = new System.Windows.Forms.MenuItem();
            this._menuItem4 = new System.Windows.Forms.MenuItem();
            this._menuItem35 = new System.Windows.Forms.MenuItem();
            this._helpItem = new System.Windows.Forms.MenuItem();
            this._aboutHelpItem = new System.Windows.Forms.MenuItem();
            this._menuItem30 = new System.Windows.Forms.MenuItem();
            this._menuItem2 = new System.Windows.Forms.MenuItem();
            this._menuItem24 = new System.Windows.Forms.MenuItem();
            this._menuItem10 = new System.Windows.Forms.MenuItem();
            this._menuItem38 = new System.Windows.Forms.MenuItem();
            this._menuItem11 = new System.Windows.Forms.MenuItem();
            this._menuItem5 = new System.Windows.Forms.MenuItem();
            this._menuItem27 = new System.Windows.Forms.MenuItem();
            this._menuItem26 = new System.Windows.Forms.MenuItem();
            this._ctxtMainForm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._addCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addMicrophoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addFloorPlanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._remoteCommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._applyScheduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._opacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._opacityToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._opacityToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this._autoLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._statusBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._fileMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this._localCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._iPCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._vLcSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._microphoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._vLcMicrophoneSourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._floorPlanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this._thruWebsiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._onMobileDevicesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this._toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this._toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this._notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this._tmrStartup = new System.Windows.Forms.Timer(this.components);
            this._ctxtMnu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._viewMediaOnAMobileDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._activateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._setInactiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._recordNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._takePhotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pTZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._listenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._applyScheduleToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._positionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._resetSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._resetRecordingCounterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxtTaskbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._unlockToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._switchAllOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._switchAllOffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showISpy10PercentOpacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showISpy30OpacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._showISpy100PercentOpacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._helpToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._websiteToolstripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._statusStrip1 = new System.Windows.Forms.StatusStrip();
            this._tsslStats = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslMonitor = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslPerformance = new System.Windows.Forms.ToolStripStatusLabel();
            this._pnlCameras = new iSpyApplication.Layout();
            this._pnlGettingStarted = new CodeVendor.Controls.Grouper();
            this._ddlLanguage = new System.Windows.Forms.ComboBox();
            this._chkShowGettingStarted = new System.Windows.Forms.CheckBox();
            this._btnOk = new System.Windows.Forms.Button();
            this._ctxtMainForm.SuspendLayout();
            this._toolStrip1.SuspendLayout();
            this._ctxtMnu.SuspendLayout();
            this._ctxtTaskbar.SuspendLayout();
            this._statusStrip1.SuspendLayout();
            this._pnlCameras.SuspendLayout();
            this._pnlGettingStarted.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mainMenu
            // 
            this._mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._fileItem,
            this._menuItem36,
            this._menuItem16,
            this._menuItem9,
            this._helpItem});
            // 
            // _fileItem
            // 
            this._fileItem.Index = 0;
            this._fileItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem19,
            this._menuItem21,
            this._menuItem1,
            this._exitFileItem});
            this._fileItem.Text = "&File";
            // 
            // _menuItem19
            // 
            this._menuItem19.Index = 0;
            this._menuItem19.Text = "&Save Object List";
            this._menuItem19.Click += new System.EventHandler(this.MenuItem19Click);
            // 
            // _menuItem21
            // 
            this._menuItem21.Index = 1;
            this._menuItem21.Text = "&Open Object List";
            this._menuItem21.Click += new System.EventHandler(this.MenuItem21Click);
            // 
            // _menuItem1
            // 
            this._menuItem1.Index = 2;
            this._menuItem1.Text = "-";
            // 
            // _exitFileItem
            // 
            this._exitFileItem.Index = 3;
            this._exitFileItem.Text = "E&xit";
            this._exitFileItem.Click += new System.EventHandler(this.ExitFileItemClick);
            // 
            // _menuItem36
            // 
            this._menuItem36.Index = 1;
            this._menuItem36.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem37});
            this._menuItem36.Text = "Edit";
            // 
            // _menuItem37
            // 
            this._menuItem37.Index = 0;
            this._menuItem37.Text = "Cameras and Microphones";
            this._menuItem37.Click += new System.EventHandler(this.MenuItem37Click);
            // 
            // _menuItem16
            // 
            this._menuItem16.Index = 2;
            this._menuItem16.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem17,
            this._menuItem3,
            this._menuItem25,
            this._menuItem13,
            this._menuItem39,
            this.mnuSaveLayout,
            this.mnuResetLayout,
            this._menuItem20,
            this._menuItem22});
            this._menuItem16.Text = "View";
            // 
            // _menuItem17
            // 
            this._menuItem17.Index = 0;
            this._menuItem17.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem7,
            this._menuItem23});
            this._menuItem17.Text = "&Files";
            this._menuItem17.Click += new System.EventHandler(this.MenuItem17Click);
            // 
            // _menuItem7
            // 
            this._menuItem7.Index = 0;
            this._menuItem7.Text = "&Video (files)";
            this._menuItem7.Click += new System.EventHandler(this.MenuItem7Click);
            // 
            // _menuItem23
            // 
            this._menuItem23.Index = 1;
            this._menuItem23.Text = "&Audio (files)";
            this._menuItem23.Click += new System.EventHandler(this.MenuItem23Click);
            // 
            // _menuItem3
            // 
            this._menuItem3.Index = 1;
            this._menuItem3.Text = "Media &Over the Web";
            this._menuItem3.Click += new System.EventHandler(this.MenuItem3Click);
            // 
            // _menuItem25
            // 
            this._menuItem25.Index = 2;
            this._menuItem25.Text = "Media on a Mobile &Device (iPhone/ Android/ Windows 7 etc)";
            this._menuItem25.Click += new System.EventHandler(this.MenuItem25Click);
            // 
            // _menuItem13
            // 
            this._menuItem13.Index = 3;
            this._menuItem13.Text = "-";
            // 
            // _menuItem39
            // 
            this._menuItem39.Index = 4;
            this._menuItem39.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem12,
            this._menuItem14,
            this._menuItem28,
            this._menuItem29,
            this.menuItem1});
            this._menuItem39.Text = "Auto Layout Objects";
            this._menuItem39.Click += new System.EventHandler(this.MenuItem39Click);
            // 
            // _menuItem12
            // 
            this._menuItem12.Index = 0;
            this._menuItem12.Text = "160 x 120";
            this._menuItem12.Click += new System.EventHandler(this.MenuItem12Click);
            // 
            // _menuItem14
            // 
            this._menuItem14.Index = 1;
            this._menuItem14.Text = "320 x 240";
            this._menuItem14.Click += new System.EventHandler(this.MenuItem14Click);
            // 
            // _menuItem28
            // 
            this._menuItem28.Index = 2;
            this._menuItem28.Text = "640 x 480";
            this._menuItem28.Click += new System.EventHandler(this.MenuItem28Click2);
            // 
            // _menuItem29
            // 
            this._menuItem29.Index = 3;
            this._menuItem29.Text = "Current";
            this._menuItem29.Click += new System.EventHandler(this.MenuItem29Click1);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 4;
            this.menuItem1.Text = "Native";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click_1);
            // 
            // mnuSaveLayout
            // 
            this.mnuSaveLayout.Index = 5;
            this.mnuSaveLayout.Text = "&Save Layout";
            this.mnuSaveLayout.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // mnuResetLayout
            // 
            this.mnuResetLayout.Index = 6;
            this.mnuResetLayout.Text = "&Reset Layout";
            this.mnuResetLayout.Click += new System.EventHandler(this.mnuResetLayout_Click);
            // 
            // _menuItem20
            // 
            this._menuItem20.Index = 7;
            this._menuItem20.Text = "Log &File";
            this._menuItem20.Click += new System.EventHandler(this.MenuItem20Click);
            // 
            // _menuItem22
            // 
            this._menuItem22.Index = 8;
            this._menuItem22.Text = "Log F&iles";
            this._menuItem22.Click += new System.EventHandler(this.MenuItem22Click1);
            // 
            // _menuItem9
            // 
            this._menuItem9.Index = 3;
            this._menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._menuItem18,
            this._menuItem8,
            this._menuItem15,
            this._menuItem6,
            this._menuItem34,
            this._menuItem33,
            this._menuItem31,
            this._miApplySchedule,
            this._menuItem32,
            this._menuItem4,
            this._menuItem35});
            this._menuItem9.Text = "&Options";
            // 
            // _menuItem18
            // 
            this._menuItem18.Index = 0;
            this._menuItem18.Text = "&Clear Capture Directories";
            this._menuItem18.Click += new System.EventHandler(this.MenuItem18Click);
            // 
            // _menuItem8
            // 
            this._menuItem8.Index = 1;
            this._menuItem8.Text = "&Settings";
            this._menuItem8.Click += new System.EventHandler(this.MenuItem8Click);
            // 
            // _menuItem15
            // 
            this._menuItem15.Index = 2;
            this._menuItem15.Text = "Reset all Recording Counters";
            this._menuItem15.Click += new System.EventHandler(this.MenuItem15Click);
            // 
            // _menuItem6
            // 
            this._menuItem6.Index = 3;
            this._menuItem6.Text = "-";
            // 
            // _menuItem34
            // 
            this._menuItem34.Index = 4;
            this._menuItem34.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._miOnAll,
            this._miOnSched});
            this._menuItem34.Text = "Switch On";
            this._menuItem34.Click += new System.EventHandler(this.MenuItem34Click);
            // 
            // _miOnAll
            // 
            this._miOnAll.Index = 0;
            this._miOnAll.Text = "All";
            this._miOnAll.Click += new System.EventHandler(this.MenuItem24Click);
            // 
            // _miOnSched
            // 
            this._miOnSched.Index = 1;
            this._miOnSched.Text = "Scheduled";
            this._miOnSched.Click += new System.EventHandler(this.MenuItem28Click1);
            // 
            // _menuItem33
            // 
            this._menuItem33.Index = 5;
            this._menuItem33.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._miOffAll,
            this._miOffSched});
            this._menuItem33.Text = "Switch Off";
            this._menuItem33.Click += new System.EventHandler(this.MenuItem33Click);
            // 
            // _miOffAll
            // 
            this._miOffAll.Index = 0;
            this._miOffAll.Text = "All";
            this._miOffAll.Click += new System.EventHandler(this.MenuItem40Click);
            // 
            // _miOffSched
            // 
            this._miOffSched.Index = 1;
            this._miOffSched.Text = "Scheduled";
            this._miOffSched.Click += new System.EventHandler(this.MenuItem41Click);
            // 
            // _menuItem31
            // 
            this._menuItem31.Index = 6;
            this._menuItem31.Text = "&Remove All Objects";
            this._menuItem31.Click += new System.EventHandler(this.MenuItem31Click);
            // 
            // _miApplySchedule
            // 
            this._miApplySchedule.Index = 7;
            this._miApplySchedule.Text = "Apply Schedule";
            this._miApplySchedule.Click += new System.EventHandler(this.MenuItem24Click1);
            // 
            // _menuItem32
            // 
            this._menuItem32.Index = 8;
            this._menuItem32.Text = "-";
            // 
            // _menuItem4
            // 
            this._menuItem4.Index = 9;
            this._menuItem4.Text = "Configure &Remote Access";
            this._menuItem4.Click += new System.EventHandler(this.MenuItem4Click);
            // 
            // _menuItem35
            // 
            this._menuItem35.Index = 10;
            this._menuItem35.Text = "Configure &Remote Commands";
            this._menuItem35.Click += new System.EventHandler(this.MenuItem35Click);
            // 
            // _helpItem
            // 
            this._helpItem.Index = 4;
            this._helpItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._aboutHelpItem,
            this._menuItem30,
            this._menuItem2,
            this._menuItem24,
            this._menuItem10,
            this._menuItem38,
            this._menuItem11,
            this._menuItem5,
            this._menuItem27,
            this._menuItem26});
            this._helpItem.Text = "&Help";
            // 
            // _aboutHelpItem
            // 
            this._aboutHelpItem.Index = 0;
            this._aboutHelpItem.Text = "&About";
            this._aboutHelpItem.Click += new System.EventHandler(this.AboutHelpItemClick);
            // 
            // _menuItem30
            // 
            this._menuItem30.Index = 1;
            this._menuItem30.Text = "-";
            // 
            // _menuItem2
            // 
            this._menuItem2.Index = 2;
            this._menuItem2.Text = "&Help";
            this._menuItem2.Click += new System.EventHandler(this.MenuItem2Click);
            // 
            // _menuItem24
            // 
            this._menuItem24.Index = 3;
            this._menuItem24.Text = "Show &Getting Started";
            this._menuItem24.Click += new System.EventHandler(this.MenuItem24Click2);
            // 
            // _menuItem10
            // 
            this._menuItem10.Index = 4;
            this._menuItem10.Text = "&Check For Updates";
            this._menuItem10.Click += new System.EventHandler(this.MenuItem10Click);
            // 
            // _menuItem38
            // 
            this._menuItem38.Index = 5;
            this._menuItem38.Text = "View Update Information";
            this._menuItem38.Click += new System.EventHandler(this.MenuItem38Click);
            // 
            // _menuItem11
            // 
            this._menuItem11.Index = 6;
            this._menuItem11.Text = "&Report Bug/ Feedback";
            this._menuItem11.Click += new System.EventHandler(this.MenuItem11Click);
            // 
            // _menuItem5
            // 
            this._menuItem5.Index = 7;
            this._menuItem5.Text = "Go to &Website";
            this._menuItem5.Click += new System.EventHandler(this.MenuItem5Click);
            // 
            // _menuItem27
            // 
            this._menuItem27.Index = 8;
            this._menuItem27.Text = "-";
            // 
            // _menuItem26
            // 
            this._menuItem26.Index = 9;
            this._menuItem26.Text = "&Support iSpy With a Donation";
            this._menuItem26.Click += new System.EventHandler(this.MenuItem26Click);
            // 
            // _ctxtMainForm
            // 
            this._ctxtMainForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._addCameraToolStripMenuItem,
            this._addMicrophoneToolStripMenuItem,
            this._addFloorPlanToolStripMenuItem,
            this._remoteCommandsToolStripMenuItem,
            this._settingsToolStripMenuItem,
            this._applyScheduleToolStripMenuItem,
            this._opacityToolStripMenuItem,
            this._opacityToolStripMenuItem1,
            this._opacityToolStripMenuItem2,
            this._autoLayoutToolStripMenuItem,
            this.saveLayoutToolStripMenuItem,
            this.resetLayoutToolStripMenuItem,
            this._fullScreenToolStripMenuItem,
            this._statusBarToolStripMenuItem,
            this._fileMenuToolStripMenuItem,
            this._toolStripToolStripMenuItem,
            this.alwaysOnTopToolStripMenuItem,
            this.exitToolStripMenuItem});
            this._ctxtMainForm.Name = "_ctxtMainForm";
            this._ctxtMainForm.Size = new System.Drawing.Size(181, 400);
            this._ctxtMainForm.Opening += new System.ComponentModel.CancelEventHandler(this.CtxtMainFormOpening);
            // 
            // _addCameraToolStripMenuItem
            // 
            this._addCameraToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.camera;
            this._addCameraToolStripMenuItem.Name = "_addCameraToolStripMenuItem";
            this._addCameraToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._addCameraToolStripMenuItem.Text = "Add &Camera";
            this._addCameraToolStripMenuItem.Click += new System.EventHandler(this.AddCameraToolStripMenuItemClick);
            // 
            // _addMicrophoneToolStripMenuItem
            // 
            this._addMicrophoneToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Mic;
            this._addMicrophoneToolStripMenuItem.Name = "_addMicrophoneToolStripMenuItem";
            this._addMicrophoneToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._addMicrophoneToolStripMenuItem.Text = "Add &Microphone";
            this._addMicrophoneToolStripMenuItem.Click += new System.EventHandler(this.AddMicrophoneToolStripMenuItemClick);
            // 
            // _addFloorPlanToolStripMenuItem
            // 
            this._addFloorPlanToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.floorplan;
            this._addFloorPlanToolStripMenuItem.Name = "_addFloorPlanToolStripMenuItem";
            this._addFloorPlanToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._addFloorPlanToolStripMenuItem.Text = "Add Floor &Plan";
            this._addFloorPlanToolStripMenuItem.Click += new System.EventHandler(this.AddFloorPlanToolStripMenuItemClick);
            // 
            // _remoteCommandsToolStripMenuItem
            // 
            this._remoteCommandsToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Command_Prompt;
            this._remoteCommandsToolStripMenuItem.Name = "_remoteCommandsToolStripMenuItem";
            this._remoteCommandsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._remoteCommandsToolStripMenuItem.Text = "Remote Commands";
            this._remoteCommandsToolStripMenuItem.Click += new System.EventHandler(this.RemoteCommandsToolStripMenuItemClick);
            // 
            // _settingsToolStripMenuItem
            // 
            this._settingsToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.settings;
            this._settingsToolStripMenuItem.Name = "_settingsToolStripMenuItem";
            this._settingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._settingsToolStripMenuItem.Text = "&Settings";
            this._settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItemClick);
            // 
            // _applyScheduleToolStripMenuItem
            // 
            this._applyScheduleToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.schedule;
            this._applyScheduleToolStripMenuItem.Name = "_applyScheduleToolStripMenuItem";
            this._applyScheduleToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._applyScheduleToolStripMenuItem.Text = "Apply Schedule";
            this._applyScheduleToolStripMenuItem.Click += new System.EventHandler(this.ApplyScheduleToolStripMenuItemClick1);
            // 
            // _opacityToolStripMenuItem
            // 
            this._opacityToolStripMenuItem.Name = "_opacityToolStripMenuItem";
            this._opacityToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._opacityToolStripMenuItem.Text = "10% Opacity";
            this._opacityToolStripMenuItem.Click += new System.EventHandler(this.OpacityToolStripMenuItemClick);
            // 
            // _opacityToolStripMenuItem1
            // 
            this._opacityToolStripMenuItem1.Name = "_opacityToolStripMenuItem1";
            this._opacityToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this._opacityToolStripMenuItem1.Text = "30% Opacity";
            this._opacityToolStripMenuItem1.Click += new System.EventHandler(this.OpacityToolStripMenuItem1Click);
            // 
            // _opacityToolStripMenuItem2
            // 
            this._opacityToolStripMenuItem2.Name = "_opacityToolStripMenuItem2";
            this._opacityToolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
            this._opacityToolStripMenuItem2.Text = "100% Opacity";
            this._opacityToolStripMenuItem2.Click += new System.EventHandler(this.OpacityToolStripMenuItem2Click);
            // 
            // _autoLayoutToolStripMenuItem
            // 
            this._autoLayoutToolStripMenuItem.Checked = true;
            this._autoLayoutToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._autoLayoutToolStripMenuItem.Name = "_autoLayoutToolStripMenuItem";
            this._autoLayoutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._autoLayoutToolStripMenuItem.Text = "Auto Layout";
            this._autoLayoutToolStripMenuItem.Click += new System.EventHandler(this.AutoLayoutToolStripMenuItemClick);
            // 
            // saveLayoutToolStripMenuItem
            // 
            this.saveLayoutToolStripMenuItem.Name = "saveLayoutToolStripMenuItem";
            this.saveLayoutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveLayoutToolStripMenuItem.Text = "Save Layout";
            this.saveLayoutToolStripMenuItem.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // resetLayoutToolStripMenuItem
            // 
            this.resetLayoutToolStripMenuItem.Name = "resetLayoutToolStripMenuItem";
            this.resetLayoutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.resetLayoutToolStripMenuItem.Text = "Reset Layout";
            this.resetLayoutToolStripMenuItem.Click += new System.EventHandler(this.mnuResetLayout_Click);
            // 
            // _fullScreenToolStripMenuItem
            // 
            this._fullScreenToolStripMenuItem.Checked = true;
            this._fullScreenToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._fullScreenToolStripMenuItem.Name = "_fullScreenToolStripMenuItem";
            this._fullScreenToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._fullScreenToolStripMenuItem.Text = "&Full Screen";
            this._fullScreenToolStripMenuItem.Click += new System.EventHandler(this.FullScreenToolStripMenuItemClick);
            // 
            // _statusBarToolStripMenuItem
            // 
            this._statusBarToolStripMenuItem.Checked = true;
            this._statusBarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._statusBarToolStripMenuItem.Name = "_statusBarToolStripMenuItem";
            this._statusBarToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._statusBarToolStripMenuItem.Text = "Status &Bar";
            this._statusBarToolStripMenuItem.Click += new System.EventHandler(this.StatusBarToolStripMenuItemClick);
            // 
            // _fileMenuToolStripMenuItem
            // 
            this._fileMenuToolStripMenuItem.Checked = true;
            this._fileMenuToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._fileMenuToolStripMenuItem.Name = "_fileMenuToolStripMenuItem";
            this._fileMenuToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._fileMenuToolStripMenuItem.Text = "File &Menu";
            this._fileMenuToolStripMenuItem.Click += new System.EventHandler(this.FileMenuToolStripMenuItemClick);
            // 
            // _toolStripToolStripMenuItem
            // 
            this._toolStripToolStripMenuItem.Checked = true;
            this._toolStripToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._toolStripToolStripMenuItem.Name = "_toolStripToolStripMenuItem";
            this._toolStripToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this._toolStripToolStripMenuItem.Text = "&Tool Strip";
            this._toolStripToolStripMenuItem.Click += new System.EventHandler(this.ToolStripToolStripMenuItemClick);
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.Checked = true;
            this.alwaysOnTopToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "Always on Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // _toolStrip1
            // 
            this._toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this._toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripDropDownButton2,
            this._toolStripDropDownButton1,
            this._toolStripButton8,
            this._toolStripButton1,
            this._toolStripButton4});
            this._toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this._toolStrip1.Location = new System.Drawing.Point(0, 0);
            this._toolStrip1.Name = "_toolStrip1";
            this._toolStrip1.Size = new System.Drawing.Size(1015, 39);
            this._toolStrip1.TabIndex = 0;
            this._toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolStrip1ItemClicked);
            // 
            // _toolStripDropDownButton2
            // 
            this._toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._localCameraToolStripMenuItem,
            this._iPCameraToolStripMenuItem,
            this._vLcSourceToolStripMenuItem,
            this._microphoneToolStripMenuItem,
            this._vLcMicrophoneSourceToolStripMenuItem,
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem,
            this._floorPlanToolStripMenuItem});
            this._toolStripDropDownButton2.Image = global::iSpyApplication.Properties.Resources.add;
            this._toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripDropDownButton2.Name = "_toolStripDropDownButton2";
            this._toolStripDropDownButton2.Size = new System.Drawing.Size(83, 36);
            this._toolStripDropDownButton2.Text = "Add...";
            // 
            // _localCameraToolStripMenuItem
            // 
            this._localCameraToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.addcam;
            this._localCameraToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._localCameraToolStripMenuItem.Name = "_localCameraToolStripMenuItem";
            this._localCameraToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._localCameraToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._localCameraToolStripMenuItem.Text = "Local Camera";
            this._localCameraToolStripMenuItem.Click += new System.EventHandler(this.LocalCameraToolStripMenuItemClick);
            // 
            // _iPCameraToolStripMenuItem
            // 
            this._iPCameraToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.ipcam;
            this._iPCameraToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._iPCameraToolStripMenuItem.Name = "_iPCameraToolStripMenuItem";
            this._iPCameraToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._iPCameraToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._iPCameraToolStripMenuItem.Text = "IP Camera";
            this._iPCameraToolStripMenuItem.Click += new System.EventHandler(this.IpCameraToolStripMenuItemClick);
            // 
            // _vLcSourceToolStripMenuItem
            // 
            this._vLcSourceToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.VLC_logo;
            this._vLcSourceToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._vLcSourceToolStripMenuItem.Name = "_vLcSourceToolStripMenuItem";
            this._vLcSourceToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._vLcSourceToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._vLcSourceToolStripMenuItem.Text = "VLC Camera Source";
            this._vLcSourceToolStripMenuItem.Click += new System.EventHandler(this.VLcSourceToolStripMenuItemClick);
            // 
            // _microphoneToolStripMenuItem
            // 
            this._microphoneToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.Mic;
            this._microphoneToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._microphoneToolStripMenuItem.Name = "_microphoneToolStripMenuItem";
            this._microphoneToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._microphoneToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._microphoneToolStripMenuItem.Text = "Microphone";
            this._microphoneToolStripMenuItem.Click += new System.EventHandler(this.MicrophoneToolStripMenuItemClick);
            // 
            // _vLcMicrophoneSourceToolStripMenuItem
            // 
            this._vLcMicrophoneSourceToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.VLC_logo;
            this._vLcMicrophoneSourceToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._vLcMicrophoneSourceToolStripMenuItem.Name = "_vLcMicrophoneSourceToolStripMenuItem";
            this._vLcMicrophoneSourceToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._vLcMicrophoneSourceToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._vLcMicrophoneSourceToolStripMenuItem.Text = "VLC Microphone Source";
            this._vLcMicrophoneSourceToolStripMenuItem.Click += new System.EventHandler(this.VLcMicrophoneSourceToolStripMenuItemClick);
            // 
            // _uSbCamerasAndMicrophonesOnOtherToolStripMenuItem
            // 
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.LAN;
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Name = "_uSbCamerasAndMicrophonesOnOtherToolStripMenuItem";
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Text = "Cameras and Microphones on Other Computers ";
            this._uSbCamerasAndMicrophonesOnOtherToolStripMenuItem.Click += new System.EventHandler(this.USbCamerasAndMicrophonesOnOtherToolStripMenuItemClick);
            // 
            // _floorPlanToolStripMenuItem
            // 
            this._floorPlanToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.floorplan;
            this._floorPlanToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._floorPlanToolStripMenuItem.Name = "_floorPlanToolStripMenuItem";
            this._floorPlanToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._floorPlanToolStripMenuItem.Size = new System.Drawing.Size(331, 20);
            this._floorPlanToolStripMenuItem.Text = "Floor Plan";
            this._floorPlanToolStripMenuItem.Click += new System.EventHandler(this.FloorPlanToolStripMenuItemClick);
            // 
            // _toolStripDropDownButton1
            // 
            this._toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._thruWebsiteToolStripMenuItem,
            this._onMobileDevicesToolStripMenuItem,
            this.inExplorerToolStripMenuItem});
            this._toolStripDropDownButton1.Image = global::iSpyApplication.Properties.Resources.Play_Normal_icon;
            this._toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripDropDownButton1.Name = "_toolStripDropDownButton1";
            this._toolStripDropDownButton1.Size = new System.Drawing.Size(124, 36);
            this._toolStripDropDownButton1.Text = "Access Media";
            this._toolStripDropDownButton1.Click += new System.EventHandler(this.ToolStripDropDownButton1Click);
            // 
            // _thruWebsiteToolStripMenuItem
            // 
            this._thruWebsiteToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.web;
            this._thruWebsiteToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._thruWebsiteToolStripMenuItem.Name = "_thruWebsiteToolStripMenuItem";
            this._thruWebsiteToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._thruWebsiteToolStripMenuItem.Size = new System.Drawing.Size(154, 20);
            this._thruWebsiteToolStripMenuItem.Text = "Online";
            this._thruWebsiteToolStripMenuItem.Click += new System.EventHandler(this.ThruWebsiteToolStripMenuItemClick);
            // 
            // _onMobileDevicesToolStripMenuItem
            // 
            this._onMobileDevicesToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources._48_iphone;
            this._onMobileDevicesToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this._onMobileDevicesToolStripMenuItem.Name = "_onMobileDevicesToolStripMenuItem";
            this._onMobileDevicesToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0);
            this._onMobileDevicesToolStripMenuItem.Size = new System.Drawing.Size(154, 20);
            this._onMobileDevicesToolStripMenuItem.Text = "Mobile Devices";
            this._onMobileDevicesToolStripMenuItem.Click += new System.EventHandler(this.OnMobileDevicesToolStripMenuItemClick);
            // 
            // inExplorerToolStripMenuItem
            // 
            this.inExplorerToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.LAN1;
            this.inExplorerToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.inExplorerToolStripMenuItem.Name = "inExplorerToolStripMenuItem";
            this.inExplorerToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.inExplorerToolStripMenuItem.Text = "Files";
            this.inExplorerToolStripMenuItem.Click += new System.EventHandler(this.inExplorerToolStripMenuItem_Click);
            // 
            // _toolStripButton8
            // 
            this._toolStripButton8.Image = global::iSpyApplication.Properties.Resources.Command_Prompt;
            this._toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton8.Name = "_toolStripButton8";
            this._toolStripButton8.Size = new System.Drawing.Size(105, 36);
            this._toolStripButton8.Text = "Commands";
            this._toolStripButton8.Click += new System.EventHandler(this.ToolStripButton8Click1);
            // 
            // _toolStripButton1
            // 
            this._toolStripButton1.Image = global::iSpyApplication.Properties.Resources.Connected;
            this._toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton1.Name = "_toolStripButton1";
            this._toolStripButton1.Size = new System.Drawing.Size(112, 36);
            this._toolStripButton1.Text = "Web Settings";
            this._toolStripButton1.Click += new System.EventHandler(this.ToolStripButton1Click1);
            // 
            // _toolStripButton4
            // 
            this._toolStripButton4.Image = global::iSpyApplication.Properties.Resources.settings;
            this._toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._toolStripButton4.Name = "_toolStripButton4";
            this._toolStripButton4.Size = new System.Drawing.Size(85, 36);
            this._toolStripButton4.Text = "Settings";
            this._toolStripButton4.Click += new System.EventHandler(this.ToolStripButton4Click);
            // 
            // _notifyIcon1
            // 
            this._notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("_notifyIcon1.Icon")));
            this._notifyIcon1.Text = "iSpy";
            this._notifyIcon1.Visible = true;
            this._notifyIcon1.Click += new System.EventHandler(this.NotifyIcon1Click);
            this._notifyIcon1.DoubleClick += new System.EventHandler(this.NotifyIcon1DoubleClick);
            // 
            // _tmrStartup
            // 
            this._tmrStartup.Interval = 1000;
            this._tmrStartup.Tick += new System.EventHandler(this.TmrStartupTick);
            // 
            // _ctxtMnu
            // 
            this._ctxtMnu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripMenuItem1,
            this._viewMediaOnAMobileDeviceToolStripMenuItem,
            this._activateToolStripMenuItem,
            this._setInactiveToolStripMenuItem,
            this._recordNowToolStripMenuItem,
            this._takePhotoToolStripMenuItem,
            this.pTZToolStripMenuItem,
            this._listenToolStripMenuItem,
            this._editToolStripMenuItem,
            this._applyScheduleToolStripMenuItem1,
            this._positionToolStripMenuItem,
            this.fullScreenToolStripMenuItem,
            this._resetSizeToolStripMenuItem,
            this._resetRecordingCounterToolStripMenuItem,
            this._showFilesToolStripMenuItem,
            this._deleteToolStripMenuItem});
            this._ctxtMnu.Name = "_ctxtMnu";
            this._ctxtMnu.Size = new System.Drawing.Size(240, 356);
            // 
            // _toolStripMenuItem1
            // 
            this._toolStripMenuItem1.Image = global::iSpyApplication.Properties.Resources.Connected;
            this._toolStripMenuItem1.Name = "_toolStripMenuItem1";
            this._toolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this._toolStripMenuItem1.Text = "View &Media ";
            this._toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1Click);
            // 
            // _viewMediaOnAMobileDeviceToolStripMenuItem
            // 
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.mobile;
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Name = "_viewMediaOnAMobileDeviceToolStripMenuItem";
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Text = "View Media on a Mobile &Device";
            this._viewMediaOnAMobileDeviceToolStripMenuItem.Click += new System.EventHandler(this.ViewMediaOnAMobileDeviceToolStripMenuItemClick);
            // 
            // _activateToolStripMenuItem
            // 
            this._activateToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.active;
            this._activateToolStripMenuItem.Name = "_activateToolStripMenuItem";
            this._activateToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._activateToolStripMenuItem.Text = "Switch &On";
            this._activateToolStripMenuItem.Click += new System.EventHandler(this.ActivateToolStripMenuItemClick);
            // 
            // _setInactiveToolStripMenuItem
            // 
            this._setInactiveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("_setInactiveToolStripMenuItem.Image")));
            this._setInactiveToolStripMenuItem.Name = "_setInactiveToolStripMenuItem";
            this._setInactiveToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._setInactiveToolStripMenuItem.Text = "&Switch Off";
            this._setInactiveToolStripMenuItem.Click += new System.EventHandler(this.SetInactiveToolStripMenuItemClick);
            // 
            // _recordNowToolStripMenuItem
            // 
            this._recordNowToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.startrec;
            this._recordNowToolStripMenuItem.Name = "_recordNowToolStripMenuItem";
            this._recordNowToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._recordNowToolStripMenuItem.Text = "Record Now";
            this._recordNowToolStripMenuItem.Click += new System.EventHandler(this.RecordNowToolStripMenuItemClick);
            // 
            // _takePhotoToolStripMenuItem
            // 
            this._takePhotoToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.snapshot;
            this._takePhotoToolStripMenuItem.Name = "_takePhotoToolStripMenuItem";
            this._takePhotoToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._takePhotoToolStripMenuItem.Text = "Take Photo";
            this._takePhotoToolStripMenuItem.Click += new System.EventHandler(this.TakePhotoToolStripMenuItemClick);
            // 
            // pTZToolStripMenuItem
            // 
            this.pTZToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.move1;
            this.pTZToolStripMenuItem.Name = "pTZToolStripMenuItem";
            this.pTZToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.pTZToolStripMenuItem.Text = "PTZ";
            // 
            // _listenToolStripMenuItem
            // 
            this._listenToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.listen2;
            this._listenToolStripMenuItem.Name = "_listenToolStripMenuItem";
            this._listenToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._listenToolStripMenuItem.Text = "Listen";
            this._listenToolStripMenuItem.Click += new System.EventHandler(this.ListenToolStripMenuItemClick);
            // 
            // _editToolStripMenuItem
            // 
            this._editToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.edit;
            this._editToolStripMenuItem.Name = "_editToolStripMenuItem";
            this._editToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._editToolStripMenuItem.Text = "&Edit";
            this._editToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItemClick);
            // 
            // _applyScheduleToolStripMenuItem1
            // 
            this._applyScheduleToolStripMenuItem1.Image = global::iSpyApplication.Properties.Resources.schedule;
            this._applyScheduleToolStripMenuItem1.Name = "_applyScheduleToolStripMenuItem1";
            this._applyScheduleToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            this._applyScheduleToolStripMenuItem1.Text = "Apply Schedule";
            this._applyScheduleToolStripMenuItem1.Click += new System.EventHandler(this.ApplyScheduleToolStripMenuItem1Click);
            // 
            // _positionToolStripMenuItem
            // 
            this._positionToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.move1;
            this._positionToolStripMenuItem.Name = "_positionToolStripMenuItem";
            this._positionToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._positionToolStripMenuItem.Text = "Position";
            this._positionToolStripMenuItem.Click += new System.EventHandler(this.PositionToolStripMenuItemClick);
            // 
            // fullScreenToolStripMenuItem
            // 
            this.fullScreenToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.fullscreen;
            this.fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
            this.fullScreenToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this.fullScreenToolStripMenuItem.Text = "Full Screen";
            this.fullScreenToolStripMenuItem.Click += new System.EventHandler(this.fullScreenToolStripMenuItem_Click);
            // 
            // _resetSizeToolStripMenuItem
            // 
            this._resetSizeToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.restore;
            this._resetSizeToolStripMenuItem.Name = "_resetSizeToolStripMenuItem";
            this._resetSizeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._resetSizeToolStripMenuItem.Text = "Reset Si&ze";
            this._resetSizeToolStripMenuItem.Click += new System.EventHandler(this.ResetSizeToolStripMenuItemClick);
            // 
            // _resetRecordingCounterToolStripMenuItem
            // 
            this._resetRecordingCounterToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.reset;
            this._resetRecordingCounterToolStripMenuItem.Name = "_resetRecordingCounterToolStripMenuItem";
            this._resetRecordingCounterToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._resetRecordingCounterToolStripMenuItem.Text = "Reset Recording Counter";
            this._resetRecordingCounterToolStripMenuItem.Click += new System.EventHandler(this.ResetRecordingCounterToolStripMenuItemClick);
            // 
            // _showFilesToolStripMenuItem
            // 
            this._showFilesToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.files;
            this._showFilesToolStripMenuItem.Name = "_showFilesToolStripMenuItem";
            this._showFilesToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._showFilesToolStripMenuItem.Text = "Show Files";
            this._showFilesToolStripMenuItem.Click += new System.EventHandler(this.ShowFilesToolStripMenuItemClick);
            // 
            // _deleteToolStripMenuItem
            // 
            this._deleteToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.remove;
            this._deleteToolStripMenuItem.Name = "_deleteToolStripMenuItem";
            this._deleteToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            this._deleteToolStripMenuItem.Text = "&Remove";
            this._deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItemClick);
            // 
            // _ctxtTaskbar
            // 
            this._ctxtTaskbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._unlockToolstripMenuItem,
            this._switchAllOnToolStripMenuItem,
            this._switchAllOffToolStripMenuItem,
            this._showToolstripMenuItem,
            this._showISpy10PercentOpacityToolStripMenuItem,
            this._showISpy30OpacityToolStripMenuItem,
            this._showISpy100PercentOpacityToolStripMenuItem,
            this._helpToolstripMenuItem,
            this._websiteToolstripMenuItem,
            this._exitToolStripMenuItem});
            this._ctxtTaskbar.Name = "_ctxtMnu";
            this._ctxtTaskbar.Size = new System.Drawing.Size(219, 224);
            this._ctxtTaskbar.Opening += new System.ComponentModel.CancelEventHandler(this.CtxtTaskbarOpening);
            // 
            // _unlockToolstripMenuItem
            // 
            this._unlockToolstripMenuItem.Image = global::iSpyApplication.Properties.Resources.unlock;
            this._unlockToolstripMenuItem.Name = "_unlockToolstripMenuItem";
            this._unlockToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._unlockToolstripMenuItem.Text = "&Unlock";
            this._unlockToolstripMenuItem.Click += new System.EventHandler(this.UnlockToolstripMenuItemClick);
            // 
            // _switchAllOnToolStripMenuItem
            // 
            this._switchAllOnToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.switchon;
            this._switchAllOnToolStripMenuItem.Name = "_switchAllOnToolStripMenuItem";
            this._switchAllOnToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._switchAllOnToolStripMenuItem.Text = "Switch All On";
            this._switchAllOnToolStripMenuItem.Click += new System.EventHandler(this.SwitchAllOnToolStripMenuItemClick);
            // 
            // _switchAllOffToolStripMenuItem
            // 
            this._switchAllOffToolStripMenuItem.Image = global::iSpyApplication.Properties.Resources.switchoff;
            this._switchAllOffToolStripMenuItem.Name = "_switchAllOffToolStripMenuItem";
            this._switchAllOffToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._switchAllOffToolStripMenuItem.Text = "Switch All Off";
            this._switchAllOffToolStripMenuItem.Click += new System.EventHandler(this.SwitchAllOffToolStripMenuItemClick);
            // 
            // _showToolstripMenuItem
            // 
            this._showToolstripMenuItem.Image = global::iSpyApplication.Properties.Resources.active;
            this._showToolstripMenuItem.Name = "_showToolstripMenuItem";
            this._showToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showToolstripMenuItem.Text = "&Show iSpy";
            this._showToolstripMenuItem.Click += new System.EventHandler(this.ShowToolstripMenuItemClick);
            // 
            // _showISpy10PercentOpacityToolStripMenuItem
            // 
            this._showISpy10PercentOpacityToolStripMenuItem.Name = "_showISpy10PercentOpacityToolStripMenuItem";
            this._showISpy10PercentOpacityToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showISpy10PercentOpacityToolStripMenuItem.Text = "Show iSpy @ 10% opacity";
            this._showISpy10PercentOpacityToolStripMenuItem.Click += new System.EventHandler(this.ShowISpy10PercentOpacityToolStripMenuItemClick);
            // 
            // _showISpy30OpacityToolStripMenuItem
            // 
            this._showISpy30OpacityToolStripMenuItem.Name = "_showISpy30OpacityToolStripMenuItem";
            this._showISpy30OpacityToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showISpy30OpacityToolStripMenuItem.Text = "Show iSpy @ 30% opacity";
            this._showISpy30OpacityToolStripMenuItem.Click += new System.EventHandler(this.ShowISpy30OpacityToolStripMenuItemClick);
            // 
            // _showISpy100PercentOpacityToolStripMenuItem
            // 
            this._showISpy100PercentOpacityToolStripMenuItem.Name = "_showISpy100PercentOpacityToolStripMenuItem";
            this._showISpy100PercentOpacityToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._showISpy100PercentOpacityToolStripMenuItem.Text = "Show iSpy @ 100 % opacity";
            this._showISpy100PercentOpacityToolStripMenuItem.Click += new System.EventHandler(this.ShowISpy100PercentOpacityToolStripMenuItemClick);
            // 
            // _helpToolstripMenuItem
            // 
            this._helpToolstripMenuItem.Image = global::iSpyApplication.Properties.Resources.help;
            this._helpToolstripMenuItem.Name = "_helpToolstripMenuItem";
            this._helpToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._helpToolstripMenuItem.Text = "&Help";
            this._helpToolstripMenuItem.Click += new System.EventHandler(this.HelpToolstripMenuItemClick);
            // 
            // _websiteToolstripMenuItem
            // 
            this._websiteToolstripMenuItem.Image = global::iSpyApplication.Properties.Resources.web;
            this._websiteToolstripMenuItem.Name = "_websiteToolstripMenuItem";
            this._websiteToolstripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._websiteToolstripMenuItem.Text = "&Website";
            this._websiteToolstripMenuItem.Click += new System.EventHandler(this.WebsiteToolstripMenuItemClick);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(218, 22);
            this._exitToolStripMenuItem.Text = "Exit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
            // 
            // _statusStrip1
            // 
            this._statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsslStats,
            this.tsslMonitor,
            this.tsslPerformance});
            this._statusStrip1.Location = new System.Drawing.Point(0, 824);
            this._statusStrip1.Name = "_statusStrip1";
            this._statusStrip1.Size = new System.Drawing.Size(1015, 22);
            this._statusStrip1.TabIndex = 0;
            // 
            // _tsslStats
            // 
            this._tsslStats.Name = "_tsslStats";
            this._tsslStats.Size = new System.Drawing.Size(59, 17);
            this._tsslStats.Text = "Loading...";
            // 
            // tsslMonitor
            // 
            this.tsslMonitor.Name = "tsslMonitor";
            this.tsslMonitor.Size = new System.Drawing.Size(76, 17);
            this.tsslMonitor.Text = "Monitoring...";
            // 
            // tsslPerformance
            // 
            this.tsslPerformance.ForeColor = System.Drawing.Color.Blue;
            this.tsslPerformance.IsLink = true;
            this.tsslPerformance.Name = "tsslPerformance";
            this.tsslPerformance.Size = new System.Drawing.Size(56, 17);
            this.tsslPerformance.Text = "Perf. Tips";
            this.tsslPerformance.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // _pnlCameras
            // 
            this._pnlCameras.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._pnlCameras.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(66)))), ((int)(((byte)(66)))));
            this._pnlCameras.ContextMenuStrip = this._ctxtMainForm;
            this._pnlCameras.Controls.Add(this._pnlGettingStarted);
            this._pnlCameras.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlCameras.Location = new System.Drawing.Point(0, 39);
            this._pnlCameras.Name = "_pnlCameras";
            this._pnlCameras.Size = new System.Drawing.Size(1015, 785);
            this._pnlCameras.TabIndex = 19;
            // 
            // _pnlGettingStarted
            // 
            this._pnlGettingStarted.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._pnlGettingStarted.BackgroundColor = System.Drawing.Color.White;
            this._pnlGettingStarted.BackgroundGradientColor = System.Drawing.Color.Silver;
            this._pnlGettingStarted.BackgroundGradientMode = CodeVendor.Controls.Grouper.GroupBoxGradientMode.ForwardDiagonal;
            this._pnlGettingStarted.BorderColor = System.Drawing.Color.Black;
            this._pnlGettingStarted.BorderThickness = 1F;
            this._pnlGettingStarted.Controls.Add(this._ddlLanguage);
            this._pnlGettingStarted.Controls.Add(this._chkShowGettingStarted);
            this._pnlGettingStarted.Controls.Add(this._btnOk);
            this._pnlGettingStarted.CustomGroupBoxColor = System.Drawing.Color.White;
            this._pnlGettingStarted.GroupImage = null;
            this._pnlGettingStarted.GroupTitle = "Getting Started";
            this._pnlGettingStarted.Location = new System.Drawing.Point(212, 162);
            this._pnlGettingStarted.Name = "_pnlGettingStarted";
            this._pnlGettingStarted.Padding = new System.Windows.Forms.Padding(20);
            this._pnlGettingStarted.PaintGroupBox = false;
            this._pnlGettingStarted.RoundCorners = 10;
            this._pnlGettingStarted.ShadowColor = System.Drawing.Color.DarkGray;
            this._pnlGettingStarted.ShadowControl = false;
            this._pnlGettingStarted.ShadowThickness = 3;
            this._pnlGettingStarted.Size = new System.Drawing.Size(598, 484);
            this._pnlGettingStarted.TabIndex = 19;
            this._pnlGettingStarted.Visible = false;
            this._pnlGettingStarted.Load += new System.EventHandler(this.PnlGettingStartedLoad);
            // 
            // _ddlLanguage
            // 
            this._ddlLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._ddlLanguage.FormattingEnabled = true;
            this._ddlLanguage.Location = new System.Drawing.Point(360, 440);
            this._ddlLanguage.Name = "_ddlLanguage";
            this._ddlLanguage.Size = new System.Drawing.Size(134, 21);
            this._ddlLanguage.TabIndex = 52;
            this._ddlLanguage.SelectedIndexChanged += new System.EventHandler(this.DdlLanguageSelectedIndexChanged);
            // 
            // _chkShowGettingStarted
            // 
            this._chkShowGettingStarted.AutoSize = true;
            this._chkShowGettingStarted.Location = new System.Drawing.Point(23, 442);
            this._chkShowGettingStarted.Name = "_chkShowGettingStarted";
            this._chkShowGettingStarted.Size = new System.Drawing.Size(119, 17);
            this._chkShowGettingStarted.TabIndex = 42;
            this._chkShowGettingStarted.Text = "Show this at startup";
            this._chkShowGettingStarted.UseVisualStyleBackColor = true;
            this._chkShowGettingStarted.CheckedChanged += new System.EventHandler(this.ChkShowGettingStartedCheckedChanged);
            // 
            // _btnOk
            // 
            this._btnOk.Location = new System.Drawing.Point(500, 438);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 23);
            this._btnOk.TabIndex = 19;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1015, 846);
            this.ContextMenuStrip = this._ctxtTaskbar;
            this.Controls.Add(this._pnlCameras);
            this.Controls.Add(this._toolStrip1);
            this.Controls.Add(this._statusStrip1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(0, 180);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "iSpy";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.MainFormHelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing1);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Resize += new System.EventHandler(this.MainFormResize);
            this._ctxtMainForm.ResumeLayout(false);
            this._toolStrip1.ResumeLayout(false);
            this._toolStrip1.PerformLayout();
            this._ctxtMnu.ResumeLayout(false);
            this._ctxtTaskbar.ResumeLayout(false);
            this._statusStrip1.ResumeLayout(false);
            this._statusStrip1.PerformLayout();
            this._pnlCameras.ResumeLayout(false);
            this._pnlGettingStarted.ResumeLayout(false);
            this._pnlGettingStarted.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region Nested type: AddObjectExternalDelegate

        private delegate void AddObjectExternalDelegate(int sourceIndex, string url, int width, int height, string name);

        #endregion

        #region Nested type: CameraCommandDelegate

        private delegate void CameraCommandDelegate(CameraWindow target);

        #endregion

        #region Nested type: ExternalCommandDelegate

        private delegate void ExternalCommandDelegate(string command);

        #endregion

        #region Nested type: ListItem

        private struct ListItem
        {
            private readonly string _name;
            internal readonly string[] Value;

            public ListItem(string name, string[] value)
            {
                _name = name;
                Value = value;
            }

            public override string ToString()
            {
                return _name;
            }
        }

        #endregion

        #region Nested type: MicrophoneCommandDelegate

        private delegate void MicrophoneCommandDelegate(VolumeLevel target);

        #endregion

        #region Nested type: clsCompareFileInfo

        public class ClsCompareFileInfo : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                var file1 = (FileInfo) x;
                var file2 = (FileInfo) y;

                return 0 - DateTime.Compare(file1.CreationTime, file2.CreationTime);
            }

            #endregion
        }

        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Maximise(ContextTarget);

        }

        private void Maximise(object obj)
        {
            if (obj.GetType() == typeof(CameraWindow))
            {
                
                var cameraControl = ((CameraWindow)obj);
                cameraControl.BringToFront();
                if (cameraControl.Width == _pnlCameras.Width)
                    Minimize(obj,false);
                else
                {
                    cameraControl.RestoreRect = new Rectangle(cameraControl.Location.X,cameraControl.Location.Y, cameraControl.Width,cameraControl.Height);
                    cameraControl.Location = new Point(0, 0);
                    cameraControl.Width = _pnlCameras.Width;
                    cameraControl.Height = _pnlCameras.Height;
                    if (cameraControl.VolumeControl != null)
                        cameraControl.Height -= 40;
                }
            }

            if (obj.GetType() == typeof(VolumeLevel))
            {
                var vf = ((VolumeLevel)obj);
                vf.BringToFront();
                if (vf.Paired)
                {
                    CameraWindow _cw = GetCameraWindow(Cameras.Single(p => p.settings.micpair == vf.Micobject.id).id);
                    if (vf.Width == _pnlCameras.Width)
                        Minimize(_cw, false);
                    else
                        Maximise(_cw);
                }
            }

            if (obj.GetType() == typeof(FloorPlanControl))
            {
                var fp = ((FloorPlanControl)obj);
                fp.BringToFront();
                if (fp.Width == _pnlCameras.Width)
                    Minimize(obj,false);
                else
                {
                    fp.RestoreRect = new Rectangle(fp.Location.X, fp.Location.Y, fp.Width, fp.Height);
                    fp.Location = new Point(0, 0);
                    fp.Width = _pnlCameras.Width;
                    fp.Height = _pnlCameras.Height;
                }

            }
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            alwaysOnTopToolStripMenuItem.Checked = !alwaysOnTopToolStripMenuItem.Checked;
            Conf.AlwaysOnTop = alwaysOnTopToolStripMenuItem.Checked;
            this.TopMost = Conf.AlwaysOnTop;
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            //save layout
            SavedLayout.Clear();

            foreach (Control c in _pnlCameras.Controls)
            {
                if (!(c is PictureBox)) continue;
                var r = new Rectangle(c.Location.X,c.Location.Y,c.Width,c.Height);
                if (c is CameraWindow)
                {
                    SavedLayout.Add(new LayoutItem
                                        {
                                            LayoutRectangle = r,
                                            ObjectId = ((CameraWindow) c).Camobject.id,
                                            ObjectTypeId = 2
                                        });
                }
                if (c is FloorPlanControl)
                {
                    SavedLayout.Add(new LayoutItem
                                        {
                                            LayoutRectangle = r,
                                            ObjectId = ((FloorPlanControl)c).Fpobject.id,
                                            ObjectTypeId = 3
                                        });
                }
                if (c is VolumeLevel)
                {
                    SavedLayout.Add(new LayoutItem
                                        {
                                            LayoutRectangle = r,
                                            ObjectId = ((VolumeLevel)c).Micobject.id,
                                            ObjectTypeId = 1
                                        });
                }
            }
            resetLayoutToolStripMenuItem.Enabled = mnuResetLayout.Enabled = true;
        }

        private void mnuResetLayout_Click(object sender, EventArgs e)
        {
            foreach(LayoutItem li in SavedLayout)
            {
                switch (li.ObjectTypeId)
                {
                    case 1:
                        VolumeLevel vl = GetMicrophone(li.ObjectId);
                        if (vl != null)
                        {
                            vl.Location = new Point(li.LayoutRectangle.X, li.LayoutRectangle.Y);
                            vl.Size = new Size(li.LayoutRectangle.Width, li.LayoutRectangle.Height);
                        }
                        break;
                    case 2:
                        CameraWindow cw = GetCameraWindow(li.ObjectId);
                        if (cw != null)
                        {
                            cw.Location = new Point(li.LayoutRectangle.X, li.LayoutRectangle.Y);
                            cw.Size = new Size(li.LayoutRectangle.Width, li.LayoutRectangle.Height);
                        }
                        break;
                    case 3:
                        FloorPlanControl fp = GetFloorPlan(li.ObjectId);
                        if (fp != null)
                        {
                            fp.Location = new Point(li.LayoutRectangle.X, li.LayoutRectangle.Y);
                            fp.Size = new Size(li.LayoutRectangle.Width, li.LayoutRectangle.Height);
                        }
                        break;
                }
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide.aspx#4");
        }

        private void inExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string foldername = Conf.MediaDirectory;
            if (!foldername.EndsWith(@"\"))
                foldername += @"\";
            Process.Start(foldername);
        }

        private void menuItem1_Click_1(object sender, EventArgs e)
        {
            LayoutObjects(-1, -1);
        }
    }

    public class LayoutItem
    {
        public int ObjectTypeId;
        public int ObjectId;
        public Rectangle LayoutRectangle;
    }
}