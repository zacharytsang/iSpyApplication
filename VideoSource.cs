using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Video.Ximea;
using AForge.Video.Kinect;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using xiApi.NET;

namespace iSpyApplication
{
    public partial class VideoSource : Form
    {
        public CameraWindow CameraControl;
        public string CameraLogin;
        public string CameraPassword;
        public string FriendlyName = "";
        public int SourceIndex;
        public string UserAgent;
        public string VideoSourceString;
        private IVideoPlayer _player;
        private bool _loaded;

        public VideoSource()
        {
            InitializeComponent();
            RenderResources();
        }

        private void VideoSourceLoad(object sender, EventArgs e)
        {
            UISync.Init(this);

            //tcSource.Controls.RemoveAt(5);
            pnlVLC.Enabled = VlcHelper.VlcInstalled;
            linkLabel3.Visible = lblInstallVLC.Visible = !pnlVLC.Enabled;

            MainForm.VideoFilters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            cmbJPEGURL.Text = MainForm.Conf.JPEGURL;
            cmbMJPEGURL.Text = MainForm.Conf.MJPEGURL;
            cmbVLCURL.Text = MainForm.Conf.VLCURL;
            cmbFile.Text = MainForm.Conf.AVIFileName;
            txtLogin.Text = txtLogin2.Text = CameraControl.Camobject.settings.login;
            txtPassword.Text = txtPassword2.Text = CameraControl.Camobject.settings.password;
            txtUserAgent.Text = txtUserAgent2.Text = CameraControl.Camobject.settings.useragent;
            txtResizeWidth.Value = txtResizeWidth2.Value = CameraControl.Camobject.settings.desktopresizewidth;
            txtResizeHeight.Value = txtResizeHeight2.Value = CameraControl.Camobject.settings.desktopresizeheight;

            VideoSourceString = CameraControl.Camobject.settings.videosourcestring;
            SourceIndex = CameraControl.Camobject.settings.sourceindex;
            txtFrameInterval.Text = txtFrameInterval2.Text = CameraControl.Camobject.settings.frameinterval.ToString();

            txtVLCArgs.Text = CameraControl.Camobject.settings.vlcargs.Replace("\r\n","\n").Replace("\n\n","\n").Replace("\n", Environment.NewLine);
            txtReconnect.Value = CameraControl.Camobject.settings.reconnectinterval;

            switch (SourceIndex)
            {
                case 0:
                    cmbJPEGURL.Text = VideoSourceString;
                    txtFrameInterval.Text = CameraControl.Camobject.settings.frameinterval.ToString();
                    break;
                case 1:
                    cmbMJPEGURL.Text = VideoSourceString;
                    break;
                case 2:
                    cmbFile.Text = VideoSourceString;
                    break;
                case 5:
                    cmbVLCURL.Text = VideoSourceString;
                    break;
            }

            cmbJPEGURL.Items.AddRange(MainForm.Conf.RecentJPGList.Split('|'));
            cmbMJPEGURL.Items.AddRange(MainForm.Conf.RecentMJPGList.Split('|'));
            cmbFile.Items.AddRange(MainForm.Conf.RecentFileList.Split('|'));
            cmbVLCURL.Items.AddRange(MainForm.Conf.RecentVLCList.Split('|'));

            if (MainForm.VideoFilters.Count == 0)
            {
                ddlDevice.Items.Add(LocRm.GetString("NoCaptureDevices"));
                ddlDevice.Enabled = false;
                ddlDevice.SelectedIndex = 0;
                ddlDevice.Enabled = false;
            }
            else
            {
                ddlDevice.SuspendLayout();
                int i = -1;
                for (int c = 0; c < MainForm.VideoFilters.Count; c++)
                {
                    var li = new ListItem {Name = MainForm.VideoFilters[c].Name};
                    int i2 = 0;
                    for (int d = 0; d < c; d++)
                    {
                        if (MainForm.VideoFilters[d].Name == li.Name)
                            i2++;
                    }
                    if (i2 > 0)
                        li.Name += " (" + i2 + ")";

                    li.Value = MainForm.VideoFilters[c].MonikerString;
                    ddlDevice.Items.Add(li);
                    if (CameraControl.Camobject.settings.videosourcestring == MainForm.VideoFilters[c].MonikerString)
                        i = c;
                }

                ddlDevice.ResumeLayout();
                ddlDevice.Items.Insert(0, LocRm.GetString("PleaseSelect"));
                if (SourceIndex == 3)
                {
                    if (i > 1)
                        ddlDevice.SelectedIndex = i + 1;
                    else
                        ddlDevice.SelectedIndex = 0;
                }
            }

            ddlScreen.SuspendLayout();
            foreach (Screen s in Screen.AllScreens)
            {
                ddlScreen.Items.Add(s.DeviceName);
            }
            ddlScreen.Items.Insert(0, LocRm.GetString("PleaseSelect"));
            if (SourceIndex == 4)
            {
                int screenIndex = Convert.ToInt32(VideoSourceString) + 1;
                ddlScreen.SelectedIndex = ddlScreen.Items.Count>screenIndex ? screenIndex : 1;
            }
            else
                ddlScreen.SelectedIndex = 0;
            ddlScreen.ResumeLayout();

            tcSource.SelectedIndex = SourceIndex;


            //ximea

            int deviceCount = 0;

            try
            {
                deviceCount = XimeaCamera.CamerasCount;
            }
            catch(Exception ex)
            {
                //Ximea DLL not installed
                MainForm.LogExceptionToFile(ex);
            }

            pnlXimea.Enabled = deviceCount>0;

            if (pnlXimea.Enabled)
            {
                for (int i = 0; i < deviceCount; i++)
                {
                    ddlXimeaDevice.Items.Add("Device " + i);
                }
                if (NV("type")=="ximea")
                {
                    int deviceIndex = Convert.ToInt32(NV("device"));
                    ddlXimeaDevice.SelectedIndex = ddlXimeaDevice.Items.Count > deviceIndex?deviceIndex:0;
                    numXimeaWidth.Text = NV("width");
                    numXimeaHeight.Text = NV("height");
                    numXimeaOffsetX.Value = Convert.ToInt32(NV("x"));
                    numXimeaOffestY.Value = Convert.ToInt32(NV("y"));

                    decimal gain = 0;
                    decimal.TryParse(NV("gain"), out gain);
                    numXimeaGain.Value =  gain;

                    decimal exp;
                    decimal.TryParse(NV("exposure"), out exp);
                    if (exp == 0)
                        exp = 100;
                    numXimeaExposure.Value = exp;

                    combo_dwnsmpl.SelectedItem  = NV("downsampling");
                }
            }
            else
            {
                ddlXimeaDevice.Items.Add(LocRm.GetString("NoDevicesFound"));
                ddlXimeaDevice.SelectedIndex = 0;
            }
                 
            deviceCount = 0;

            try
            {
                deviceCount = Kinect.DeviceCount;
            }
            catch
            {
            }

            pnlKinect.Enabled = deviceCount>0;

            if (pnlKinect.Enabled)
            {
                for (int i = 0; i < Kinect.DeviceCount; i++)
                {
                    ddlKinectDevice.Items.Add("Device " + i);
                }
                if (NV("type") == "kinect")
                {
                    int deviceIndex = Convert.ToInt32(NV("device"));
                    ddlKinectDevice.SelectedIndex = ddlKinectDevice.Items.Count > deviceIndex ? deviceIndex : 0;
                    ddlLEDMode.SelectedIndex = Convert.ToInt32(NV("ledmode"));
                    numKinectTilt.Value = Convert.ToInt32(NV("tilt"));
                    ddlKinectMode.SelectedIndex = Convert.ToInt32(NV("videomode"));
                }
            }
            else
            {
                ddlKinectDevice.Items.Add(LocRm.GetString("NoDevicesFound"));
                ddlKinectDevice.SelectedIndex = 0;
                ddlKinectMode.SelectedIndex = 0;
            }

            _loaded = true;
        }

        private string NV(string name)
        {
            if (String.IsNullOrEmpty(CameraControl.Camobject.settings.namevaluesettings))
                return "";
            string[] settings = CameraControl.Camobject.settings.namevaluesettings.Split(',');
            foreach(string s in settings)
            {
                string[] nv = s.Split('=');
                if (nv[0].ToLower().Trim()==name)
                    return nv[1].ToLower();
            }
            return "";
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("VideoSource");
            button1.Text = LocRm.GetString("Ok");
            button2.Text = LocRm.GetString("Cancel");
            button3.Text = LocRm.GetString("chars_3014702301470230147");
            label1.Text = LocRm.GetString("JpegUrl");
            label10.Text = LocRm.GetString("milliseconds");
            label11.Text = LocRm.GetString("Screen");
            label12.Text = LocRm.GetString("milliseconds");
            label13.Text = LocRm.GetString("FrameInterval");
            label14.Text = label22.Text = LocRm.GetString("ResizeTo");
            label15.Text = LocRm.GetString("Username");
            label16.Text = LocRm.GetString("UserAgent");
            label17.Text = LocRm.GetString("Password");
            label2.Text = LocRm.GetString("MjpegUrl");
            label3.Text = LocRm.GetString("OpenFile");
            label4.Text = LocRm.GetString("SelectLocalDevice");
            label5.Text = LocRm.GetString("Username");
            label6.Text = LocRm.GetString("Password");
            label7.Text = LocRm.GetString("UserAgent");
            label8.Text = LocRm.GetString("X");
            label9.Text = LocRm.GetString("FrameInterval");
            linkLabel1.Text = LocRm.GetString("HelpMeFindTheRightUrl");
            linkLabel2.Text = LocRm.GetString("HelpMeFindTheRightUrl");
            tabPage1.Text = LocRm.GetString("JpegUrl");
            tabPage2.Text = LocRm.GetString("MjpegUrl");
            tabPage3.Text = LocRm.GetString("AviFile");
            tabPage4.Text = LocRm.GetString("LocalDevice");
            tabPage5.Text = LocRm.GetString("Desktop");
            tabPage6.Text = LocRm.GetString("VLCPlugin");
            linkLabel5.Text = LocRm.GetString("AddPublicCamera");
            label32.Text = label36.Text = LocRm.GetString("device");
            label34.Text = LocRm.GetString("tilt");
            label33.Text = LocRm.GetString("videomode");
            label31.Text = LocRm.GetString("Name");
            label31.Text = LocRm.GetString("Name");
            label30.Text = LocRm.GetString("serial");
            label29.Text = LocRm.GetString("type");
            label26.Text = LocRm.GetString("Width");
            label25.Text = LocRm.GetString("Height");
            label24.Text = LocRm.GetString("offsetx");
            label23.Text = LocRm.GetString("offsety");
            label27.Text = LocRm.GetString("gain");
            label28.Text = LocRm.GetString("exposure");


            label18.Text = LocRm.GetString("Arguments");
            lblInstallVLC.Text = LocRm.GetString("VLCConnectInfo");
            linkLabel3.Text = LocRm.GetString("DownloadVLC");
            btnGetStreamSize.Text = LocRm.GetString("GetStreamSize");
            linkLabel4.Text = LocRm.GetString("UseiSpyServerText");
            label48.Text = LocRm.GetString("Seconds");
            label43.Text = LocRm.GetString("ReconnectEvery");

            llblHelp.Text = LocRm.GetString("help");
        }

        private void Button1Click(object sender, EventArgs e)
        {
            MainForm.Conf.JPEGURL = cmbJPEGURL.Text.Trim();
            MainForm.Conf.MJPEGURL = cmbMJPEGURL.Text.Trim();
            MainForm.Conf.AVIFileName = cmbFile.Text.Trim();
            MainForm.Conf.VLCURL = cmbVLCURL.Text.Trim();

            string nv = "";
            SourceIndex = tcSource.SelectedIndex;
            CameraLogin = txtLogin.Text;
            CameraPassword = txtPassword.Text;
            UserAgent = txtUserAgent.Text;
            string url;
            switch (SourceIndex)
            {
                case 0:
                    int frameinterval;
                    if (!Int32.TryParse(txtFrameInterval.Text, out frameinterval))
                    {
                        MessageBox.Show(LocRm.GetString("Validate_FrameInterval"));
                        return;
                    }
                    url = cmbJPEGURL.Text.Trim();
                    if (url == String.Empty)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    VideoSourceString = url;
                    CameraControl.Camobject.settings.frameinterval = frameinterval;
                    FriendlyName = VideoSourceString;
                    break;
                case 1:
                    url = cmbMJPEGURL.Text.Trim();
                    if (url == String.Empty)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    VideoSourceString = url;
                    FriendlyName = VideoSourceString;
                    CameraLogin = txtLogin2.Text;
                    CameraPassword = txtPassword2.Text;
                    UserAgent = txtUserAgent2.Text;
                    break;
                case 2:
                    url = cmbFile.Text.Trim();
                    if (url == String.Empty)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    VideoSourceString = url;
                    FriendlyName = VideoSourceString;
                    break;
                case 3:
                    if (ddlDevice.SelectedIndex < 1)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    VideoSourceString = ((ListItem) ddlDevice.SelectedItem).Value;
                    FriendlyName = ((ListItem) ddlDevice.SelectedItem).Name;
                    break;
                case 4:
                    int frameinterval2;
                    if (!Int32.TryParse(txtFrameInterval2.Text, out frameinterval2))
                    {
                        MessageBox.Show(LocRm.GetString("Validate_FrameInterval"));
                        return;
                    }
                    if (ddlScreen.SelectedIndex < 1)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    VideoSourceString = (ddlScreen.SelectedIndex - 1).ToString();
                    FriendlyName = ddlScreen.SelectedItem.ToString();
                    CameraControl.Camobject.settings.frameinterval = frameinterval2;
                    CameraControl.Camobject.settings.desktopresizewidth = Convert.ToInt32(txtResizeWidth.Value);
                    CameraControl.Camobject.settings.desktopresizeheight = Convert.ToInt32(txtResizeHeight.Value);
                    break;
                case 5:
                    url = cmbVLCURL.Text.Trim();
                    if (url == String.Empty)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    var iReconnect = (int)txtReconnect.Value;
                    if (iReconnect<30 && iReconnect!=0)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_ReconnectInterval"), LocRm.GetString("Note"));
                        return;
                    }
                    VideoSourceString = url;
                    FriendlyName = VideoSourceString;
                    CameraControl.Camobject.settings.vlcargs = txtVLCArgs.Text.Trim();
                    CameraControl.Camobject.settings.desktopresizewidth = Convert.ToInt32(txtResizeWidth2.Value);
                    CameraControl.Camobject.settings.desktopresizeheight = Convert.ToInt32(txtResizeHeight2.Value);
                    CameraControl.Camobject.settings.reconnectinterval = iReconnect;
                    break;
                case 6:
                    if (!pnlXimea.Enabled)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    nv = "type=ximea";
                    nv += ",device=" + ddlXimeaDevice.SelectedIndex;
                    nv += ",width=" + numXimeaWidth.Text;
                    nv += ",height=" + numXimeaHeight.Text;
                    nv += ",x=" + (int)numXimeaOffsetX.Value;
                    nv += ",y=" + (int)numXimeaOffestY.Value;
                    nv += ",gain=" +
                          String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}",
                                        numXimeaGain.Value);
                    nv += ",exposure=" + String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000}",
                                        numXimeaExposure.Value);
                    nv += ",downsampling=" + combo_dwnsmpl.SelectedItem;
                    VideoSourceString = nv;

                    CameraControl.Camobject.settings.namevaluesettings = nv;
                    break;
                case 7:
                    if (!pnlKinect.Enabled)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                        return;
                    }
                    nv = "type=kinect";
                    nv += ",device=" + ddlKinectDevice.SelectedIndex;
                    nv += ",ledmode=" + ddlLEDMode.SelectedIndex;
                    nv += ",tilt=" + (int)numKinectTilt.Value;
                    nv += ",videomode=" + (int)ddlKinectMode.SelectedIndex;

                    VideoSourceString = nv;
                    CameraControl.Camobject.settings.namevaluesettings = nv;
                    break;
            }


            if (String.IsNullOrEmpty(VideoSourceString))
            {
                MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                return;
            }

            if (!MainForm.Conf.RecentFileList.Contains(MainForm.Conf.AVIFileName) &&
                MainForm.Conf.AVIFileName != "")
            {
                MainForm.Conf.RecentFileList =
                    (MainForm.Conf.RecentFileList + "|" + MainForm.Conf.AVIFileName).Trim('|');
            }
            if (!MainForm.Conf.RecentJPGList.Contains(MainForm.Conf.JPEGURL) &&
                MainForm.Conf.JPEGURL != "")
            {
                MainForm.Conf.RecentJPGList =
                    (MainForm.Conf.RecentJPGList + "|" + MainForm.Conf.JPEGURL).Trim('|');
            }
            if (!MainForm.Conf.RecentMJPGList.Contains(MainForm.Conf.MJPEGURL) &&
                MainForm.Conf.MJPEGURL != "")
            {
                MainForm.Conf.RecentMJPGList =
                    (MainForm.Conf.RecentMJPGList + "|" + MainForm.Conf.MJPEGURL).Trim('|');
            }
            if (!MainForm.Conf.RecentVLCList.Contains(MainForm.Conf.VLCURL) &&
                MainForm.Conf.VLCURL != "")
            {
                MainForm.Conf.RecentVLCList =
                    (MainForm.Conf.RecentVLCList + "|" + MainForm.Conf.VLCURL).Trim('|');
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button3Click(object sender, EventArgs e)
        {
            ofd.Filter = "Video Files|*.avi";
            ofd.InitialDirectory = MainForm.Conf.MediaDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                cmbFile.Text = ofd.FileName;
            }
        }

        private void Button2Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmbJPEGURL_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cmbJPEGURL_Click(object sender, EventArgs e)
        {
        }

        private void cmbMJPEGURL_Click(object sender, EventArgs e)
        {
        }

        private void ddlDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cmbFile_TextChanged(object sender, EventArgs e)
        {
        }


        private void cmbFile_Click(object sender, EventArgs e)
        {
        }


        private void VideoSource_FormClosing(object sender, FormClosingEventArgs e)
        {
        }


        private void cmbFile_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cmbMJPEGURL_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ddlScreen_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void LinkLabel2LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/sources.aspx");
        }

        private void LinkLabel1LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/sources.aspx");
        }

        private void LinkLabel3LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, "http://www.videolan.org/vlc/download-windows.html");
        }

        private void pnlVLC_Paint(object sender, PaintEventArgs e)
        {
        }

        private void lblInstallVLC_Click(object sender, EventArgs e)
        {
        }

        private void Button4Click(object sender, EventArgs e)
        {
            string url = cmbVLCURL.Text.Trim();
            if (url == String.Empty)
            {
                MessageBox.Show(LocRm.GetString("Validate_SelectCamera"), LocRm.GetString("Note"));
                return;
            }

            btnGetStreamSize.Enabled = false;
            if (_player != null)
            {
                StopPlayer();
            }
            try
            {
                var factory = new MediaPlayerFactory();
                _player = factory.CreatePlayer<IVideoPlayer>();
                var media = factory.CreateMedia<IMedia>(url);
                _player.Open(media);
                _player.Mute = true;
                _player.Events.PlayerPositionChanged += EventsPlayerPositionChanged;
                _player.Events.PlayerEncounteredError += EventsPlayerEncounteredError;
                _player.CustomRenderer.SetCallback(bmp => bmp.Dispose());
                _player.CustomRenderer.SetFormat(new BitmapFormat(100, 100, ChromaType.RV24));

                _player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LocRm.GetString("Error"));
            }
        }

        private void EventsPlayerEncounteredError(object sender, EventArgs e)
        {
            MessageBox.Show("VLC Error", LocRm.GetString("Error"));
            UISync.Execute(() => btnGetStreamSize.Enabled = true);
        }

        private void SetVideoSize(Size size)
        {
            txtResizeWidth2.Value = size.Width;
            txtResizeHeight2.Value = size.Height;
        }

        private void StopPlayer()
        {
            if (_player != null)
            {
                _player.Stop();
                _player.Dispose();
                _player = null;
            }
            btnGetStreamSize.Enabled = true;
        }

        private void EventsPlayerPositionChanged(object sender, MediaPlayerPositionChanged e)
        {
            Size size = _player.GetVideoSize(0);
            if (!size.IsEmpty)
            {
                UISync.Execute(() => SetVideoSize(size));
                UISync.Execute(StopPlayer);
            }
        }

        private void LinkLabel5LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/publicsources.aspx");
        }

        #region Nested type: ListItem

        private struct ListItem
        {
            internal string Name;
            internal string Value;

            public override string ToString()
            {
                return Name;
            }
        }

        #endregion

        #region Nested type: UISync

        private class UISync
        {
            private static ISynchronizeInvoke _sync;

            public static void Init(ISynchronizeInvoke sync)
            {
                _sync = sync;
            }

            public static void Execute(Action action)
            {
                _sync.BeginInvoke(action, null);
            }
        }

        #endregion

        private void ddlXimeaDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectXimea();
        }


        private void ConnectXimea()
        {
            // close whatever is open now
            if (!pnlXimea.Enabled) return;
            try
            {
                if (CameraControl.XimeaSource==null)
                    CameraControl.XimeaSource = new XimeaVideoSource( ddlXimeaDevice.SelectedIndex );
                    
                // start the camera
                if (!CameraControl.XimeaSource.IsRunning)
                    CameraControl.XimeaSource.Start();

                // get some parameters
                nameBox.Text = CameraControl.XimeaSource.GetParamString(CameraParameter.DeviceName);
                snBox.Text = CameraControl.XimeaSource.GetParamString(CameraParameter.DeviceSerialNumber);
                typeBox.Text = CameraControl.XimeaSource.GetParamString(CameraParameter.DeviceType);

                // width
                numXimeaWidth.Text = CameraControl.XimeaSource.GetParamInt(CameraParameter.Width ).ToString();

                // height
                numXimeaHeight.Text = CameraControl.XimeaSource.GetParamInt(CameraParameter.Height).ToString();

                // exposure
                numXimeaExposure.Minimum = (decimal)CameraControl.XimeaSource.GetParamFloat(CameraParameter.ExposureMin) / 1000;
                numXimeaExposure.Maximum = (decimal)CameraControl.XimeaSource.GetParamFloat(CameraParameter.ExposureMax) / 1000;
                numXimeaExposure.Value = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.Exposure)) / 1000;
                if (numXimeaExposure.Value == 0)
                    numXimeaExposure.Value = 100;

                // gain
                numXimeaGain.Minimum = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.GainMin));
                numXimeaGain.Maximum = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.GainMax));
                numXimeaGain.Value = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.Gain));

                int maxDwnsmpl = CameraControl.XimeaSource.GetParamInt(CameraParameter.DownsamplingMax);

                switch (maxDwnsmpl)
                {
                    case 8:
                        combo_dwnsmpl.Items.Add("1");
                        combo_dwnsmpl.Items.Add("2");
                        combo_dwnsmpl.Items.Add("4");
                        combo_dwnsmpl.Items.Add("8");
                        break;
                    case 6:
                        combo_dwnsmpl.Items.Add("1");
                        combo_dwnsmpl.Items.Add("2");
                        combo_dwnsmpl.Items.Add("4");
                        combo_dwnsmpl.Items.Add("6");
                        break;
                    case 4:
                        combo_dwnsmpl.Items.Add("1");
                        combo_dwnsmpl.Items.Add("2");
                        combo_dwnsmpl.Items.Add("4");
                        break;
                    case 2:
                        combo_dwnsmpl.Items.Add("1");
                        combo_dwnsmpl.Items.Add("2");
                        break;
                    default:
                        combo_dwnsmpl.Items.Add("1");
                        break;
                }
                combo_dwnsmpl.SelectedIndex = combo_dwnsmpl.Items.Count-1;
            }
            catch ( Exception ex )
            {
                MainForm.LogExceptionToFile(ex);
                MessageBox.Show( ex.Message, LocRm.GetString("Error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error );
            }

        }

        private void devicesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!pnlKinect.Enabled) return;

        }

        private void offsetYUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = MainForm.Website+"/userguide-connecting-cameras.aspx";
            switch (tcSource.SelectedIndex)
            {
                case 0:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#4";
                    break;
                case 1:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#4";
                    break;
                case 2:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx";
                    break;
                case 3:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#2";
                    break;
                case 4:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#6";
                    break;
                case 5:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#5";
                    break;
                case 6:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#7";
                    break;
                case 7:
                    url = MainForm.Website+"/userguide-connecting-cameras.aspx#8";
                    break;
            }
            Help.ShowHelp(this, url);
        }

        private void combo_dwnsmpl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded)
                return;
            if (combo_dwnsmpl.SelectedIndex > -1 && CameraControl.XimeaSource!=null)
            {
                CameraControl.XimeaSource.SetParam(CameraParameter.Downsampling,
                                                   Convert.ToInt32(
                                                       combo_dwnsmpl.Items[combo_dwnsmpl.SelectedIndex].ToString()));

                //update width and height info
                numXimeaWidth.Text = CameraControl.XimeaSource.GetParamInt(CameraParameter.Width).ToString();
                numXimeaHeight.Text = CameraControl.XimeaSource.GetParamInt(CameraParameter.Height).ToString();

                //reset gain slider
                numXimeaGain.Minimum = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.GainMin));
                numXimeaGain.Maximum = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.GainMax));
                numXimeaGain.Value = new Decimal(CameraControl.XimeaSource.GetParamFloat(CameraParameter.Gain));
            }
        }

        private void numXimeaExposure_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numXimeaGain_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}