using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using AForge.Video.DirectShow;
using AForge.Vision.Motion;


namespace iSpyApplication
{
    public partial class AddCamera : Form
    {
        private readonly string[] _alertmodes = new[] {"movement", "nomovement", "face", "np_recog", "np_not_recog"};

        private readonly string[] _detectortypes = new[] {"Two Frames", "Custom Frame", "Background Modelling", "None"};

        private readonly string[] _processortypes = new[]
                                                        {
                                                            "Grid Processing", "Object Tracking", "Border Highlighting",
                                                            "Area Highlighting", "None"
                                                        };

        public CameraWindow CameraControl;
        internal AreaSelector AreaControl;
        private bool _ffpresetwarned;
        private HSLFilteringForm FilterForm;
        private bool _loaded;


        //private string[] _timestamplocations = new[]
        //                                           {
        //                                               "None", "Top Left", "Top Center", "Top Right", "Bottom Left",
        //                                               "Bottom Center", "Bottom Right"
        //                                           };

        public VideoCaptureDevice CaptureDevice;

        public AddCamera()
        {
            InitializeComponent();
            RenderResources();


            AreaControl = new AreaSelector {Width = 320, Height = 240};
            gbZones.Controls.Add(AreaControl);
            AreaControl.Left = 139;
            AreaControl.Top = 30;

            AreaControl.BoundsChanged += AsBoundsChanged;
            AreaControl.Invalidate();
        }

        private void AsBoundsChanged(object sender, EventArgs e)
        {
            if (CameraControl.Camera != null && CameraControl.Camera.MotionDetector != null)
            {
                CameraControl.Camera.SetMotionZones(AreaControl.MotionZones);
            }
            CameraControl.Camobject.detector.motionzones = AreaControl.MotionZones;
        }

        private void BtnSelectSourceClick(object sender, EventArgs e)
        {
            SelectSource();
        }

        private bool SelectSource()
        {
            bool success = false;
            var vs = new VideoSource {CameraControl = CameraControl};
            vs.ShowDialog(this);
            if (vs.DialogResult == DialogResult.OK)
            {
                CameraControl.Camobject.settings.videosourcestring = vs.VideoSourceString;
                CameraControl.Camobject.settings.sourceindex = vs.SourceIndex;
                CameraControl.Camobject.settings.login = vs.CameraLogin;
                CameraControl.Camobject.settings.password = vs.CameraPassword;
                CameraControl.Camobject.settings.useragent = vs.UserAgent;
                chkActive.Enabled = true;
                chkActive.Checked = false;
                Thread.Sleep(500); //allows unmanaged code to complete shutdown
                chkActive.Checked = true;
                CameraControl.NeedSizeUpdate = true;
                if (txtCameraName.Text.Trim() == "")
                    txtCameraName.Text = "Cam " + MainForm.Conf.NextCameraID;
                success = true;
            }
            vs.Dispose();
            return success;
        }

        private void AddCameraLoad(object sender, EventArgs e)
        {
            _loaded = false;
            CameraControl.IsEdit = true;
            rtbDescription.Text = CameraControl.Camobject.description;
            ddlPreset.Items.Add(new ListItem("Select", ""));

            ddlPreset.Items.Add(new ListItem("MP4 (web/mobile)",
                                             "-i \"{filename}.avi\" -an -r {framerate} -vcodec libx264 -f mp4 -fpre \"{presetdir}libx264-slow.ffpreset\" \"{filename}.mp4\""));
            ddlPreset.Items.Add(new ListItem("FLV (web only)",
                                             "-i \"{filename}.avi\" -an -r {framerate} -f flv \"{filename}.flv\""));

            
            //ddlPreset.Items.Add(new ListItem("WEBM (android/chrome)", "-i \"{filename}.avi\" -an -r {framerate} -vcodec libvpx -f webm \"{filename}.webm\""));

            ddlTimestamp.Text = CameraControl.Camobject.settings.timestampformatter;

            

            ddlPreset.SelectedIndex = 0;
            chkUploadYouTube.Checked = CameraControl.Camobject.settings.youtube.autoupload;
            chkPublic.Checked = CameraControl.Camobject.settings.youtube.@public;
            txtTags.Text = CameraControl.Camobject.settings.youtube.tags;
            chkMovement.Checked = CameraControl.Camobject.alerts.active;

            var youTubeCats = MainForm.Conf.YouTubeCategories.Split(',');

            int i = 0, ytcInd = 0;
            foreach (var cat in youTubeCats)
            {
                ddlCategory.Items.Add(cat);
                if (cat == CameraControl.Camobject.settings.youtube.category)
                {
                    ytcInd = i;
                }
                i++;
            }
            ddlCategory.SelectedIndex = ytcInd;

            ddlFFPreset.Items.Add("None");
            foreach (string entry in Directory.GetFiles(Program.AppPath + @"ffmpeg\presets"))
            {
                var fi = new FileInfo(entry);
                string preset = fi.Name.ToLower();
                if (preset.EndsWith(".ffpreset"))
                {
                    preset = preset.Replace(".ffpreset", "");
                    ddlFFPreset.Items.Add(preset);
                }
            }
            txtPostProcess.Text = CameraControl.Camobject.settings.ffmpeg;
            chkDeleteSource.Checked = CameraControl.Camobject.settings.deleteavi;

            gpbSubscriber.Enabled = gpbSubscriber2.Enabled = MainForm.Conf.Subscribed;

            ddlMic.Items.Add(new ListItem(LocRm.GetString("NotPairedCam"), "-1"));

            int micind = 0, ind = 1;
            foreach (objectsMicrophone om in MainForm.Microphones)
            {
                objectsMicrophone om1 = om;
                if (
                    MainForm.Cameras.Where(p => p.settings.micpair == om1.id && p.id != CameraControl.Camobject.id).
                        Count() == 0)
                {
                    ddlMic.Items.Add(new ListItem(om.name, om.id.ToString()));
                    if (CameraControl.Camobject.settings.micpair == om.id)
                    {
                        micind = ind;
                    }
                    ind++;
                }
            }

            ddlMic.SelectedIndex = micind;

            ddlProcessor.Items.AddRange(LocRm.GetString("ProcessorList").Split(','));
            ddlMotionDetector.Items.AddRange(LocRm.GetString("DetectorList").Split(','));
            //ddlAlertMode.Items.AddRange(LocRm.GetString("AlertModes").Split(','));

            for (int j = 0; j < _detectortypes.Length; j++)
            {
                if (_detectortypes[j] == CameraControl.Camobject.detector.type)
                {
                    ddlMotionDetector.SelectedIndex = j;
                    break;
                }
            }
            for (int j = 0; j < _processortypes.Length; j++)
            {
                if (_processortypes[j] == CameraControl.Camobject.detector.postprocessor)
                {
                    ddlProcessor.SelectedIndex = j;
                    break;
                }
            }
            int iMode = 0;
            for (int j = 0; j < _alertmodes.Length; j++)
            {
                ddlAlertMode.Items.Add(LocRm.GetString(_alertmodes[j]));

                if (_alertmodes[j] == CameraControl.Camobject.alerts.mode)
                {
                    iMode = j;
                }
            }
            ddlAlertMode.SelectedIndex = iMode;

            ddlProcessFrames.SelectedItem = CameraControl.Camobject.detector.processeveryframe.ToString();
            txtCameraName.Text = CameraControl.Camobject.name;
            
            tbSensitivity.Value = Convert.ToInt32(CameraControl.Camobject.detector.sensitivity*10);
            txtSensitivity.Text = CameraControl.Camobject.detector.sensitivity.ToString();
            
            chkRecord.Checked = CameraControl.Camobject.detector.recordondetect;
            txtExecuteMovement.Text = CameraControl.Camobject.alerts.executefile;

            chkSendEmailMovement.Checked = CameraControl.Camobject.notifications.sendemail;
            chkSendSMSMovement.Checked = CameraControl.Camobject.notifications.sendsms;
            txtSMSNumber.Text = CameraControl.Camobject.settings.smsnumber;
            txtEmailAlert.Text = CameraControl.Camobject.settings.emailaddress;
            chkMMS.Checked = CameraControl.Camobject.notifications.sendmms;
            chkSchedule.Checked = CameraControl.Camobject.schedule.active;
            chkFlipX.Checked = CameraControl.Camobject.flipx;
            chkFlipY.Checked = CameraControl.Camobject.flipy;
            chkRotate.Checked = CameraControl.Camobject.rotate90;
            chkTrack.Checked = CameraControl.Camobject.settings.ptzautotrack;
            chkColourProcessing.Checked = CameraControl.Camobject.detector.colourprocessingenabled;
            numMaxFR.Value = CameraControl.Camobject.settings.maxframerate;
            numMaxFRRecording.Value = CameraControl.Camobject.settings.maxframeraterecord;
            txtArguments.Text = CameraControl.Camobject.alerts.arguments;
            txtDirectory.Text = CameraControl.Camobject.directory;
            chkAutoHome.Checked = CameraControl.Camobject.settings.ptzautohome;
            numTTH.Value = CameraControl.Camobject.settings.ptztimetohome;
            ShowSchedule(-1);

            chkActive.Checked = CameraControl.Camobject.settings.active;
            pnlScheduler.Enabled = chkSchedule.Checked;

            AreaControl.MotionZones = CameraControl.Camobject.detector.motionzones;

            chkActive.Enabled = !string.IsNullOrEmpty(CameraControl.Camobject.settings.videosourcestring);
            string[] alertOptions = CameraControl.Camobject.alerts.alertoptions.Split(','); //beep,restore
            chkBeep.Checked = Convert.ToBoolean(alertOptions[0]);
            chkRestore.Checked = Convert.ToBoolean(alertOptions[1]);
            Text = LocRm.GetString("EditCamera");
            if (CameraControl.Camobject.id > -1)
                Text += " (ID: " + CameraControl.Camobject.id + ", DIR: " + CameraControl.Camobject.directory + ")";


            txtTimeLapse.Text = CameraControl.Camobject.recorder.timelapse.ToString();
            pnlMovement.Enabled = chkMovement.Checked;
            chkSuppressNoise.Checked = CameraControl.Camobject.settings.suppressnoise;
            
            ddlObjectCount.Value = CameraControl.Camobject.alerts.objectcountalert;


            for (i = 0; i < ddlFrameRate.Items.Count; i++)
            {
                if (ddlFrameRate.Items[i].ToString() == CameraControl.Camobject.settings.framerate.ToString())
                {
                    ddlFrameRate.SelectedIndex = i;
                    break;
                }
            }
            if (ddlFrameRate.SelectedIndex == -1)
                ddlFrameRate.SelectedIndex = 1;

            gbAdvanced.Enabled = false;
            gbAdvanced.Enabled = chkActive.Checked;

            linkLabel4.Visible = linkLabel9.Visible = !(MainForm.Conf.Subscribed);

            if (CameraControl.Camera != null)
            {
                CameraControl.Camera.NewFrame -= CameraNewFrame;
                CameraControl.Camera.NewFrame += CameraNewFrame;
            }

            txtBuffer.Text = CameraControl.Camobject.recorder.bufferframes.ToString();
            txtCalibrationDelay.Text = CameraControl.Camobject.detector.calibrationdelay.ToString();
            txtInactiveRecord.Text = CameraControl.Camobject.recorder.inactiverecord.ToString();
            txtMinimumInterval.Text = CameraControl.Camobject.alerts.minimuminterval.ToString();
            txtMaxRecordTime.Text = CameraControl.Camobject.recorder.maxrecordtime.ToString();
            txtEmailGrabInterval.Text = CameraControl.Camobject.notifications.emailgrabinterval.ToString();
            btnBack.Enabled = false;

            ddlHourStart.SelectedIndex =
                ddlHourEnd.SelectedIndex = ddlMinuteStart.SelectedIndex = ddlMinuteEnd.SelectedIndex = 0;

            txtFTPServer.Text = CameraControl.Camobject.ftp.server;
            txtFTPUsername.Text = CameraControl.Camobject.ftp.username;
            txtFTPPassword.Text = CameraControl.Camobject.ftp.password;
            txtFTPPort.Text = CameraControl.Camobject.ftp.port.ToString();
            txtUploadEvery.Text = CameraControl.Camobject.ftp.interval.ToString();
            txtFTPFilename.Text = CameraControl.Camobject.ftp.filename;
            chkFTP.Checked = gbFTP.Enabled = CameraControl.Camobject.ftp.enabled;
            txtTimeLapseFrames.Text = CameraControl.Camobject.recorder.timelapseframes.ToString();

            chkTimelapse.Checked = CameraControl.Camobject.recorder.timelapseenabled;
            if (!chkTimelapse.Checked)
                groupBox1.Enabled = false;

            chkEmailOnDisconnect.Checked = CameraControl.Camobject.settings.notifyondisconnect;
            txtMaskImage.Text = CameraControl.Camobject.settings.maskimage;

            chkUsePassive.Checked = CameraControl.Camobject.ftp.usepassive;
            chkPTZFlipX.Checked = CameraControl.Camobject.settings.ptzflipx;
            chkPTZFlipY.Checked = CameraControl.Camobject.settings.ptzflipy;
            chkPTZRotate90.Checked = CameraControl.Camobject.settings.ptzrotate90;

            txtFTPText.Text = CameraControl.Camobject.ftp.text;
            chkRecordAlert.Checked = CameraControl.Camobject.detector.recordonalert;


            rdoFTPMotion.Checked = CameraControl.Camobject.ftp.mode == 0;
            rdoFTPAlerts.Checked = CameraControl.Camobject.ftp.mode == 1;
            rdoFTPInterval.Checked = CameraControl.Camobject.ftp.mode == 2;

            txtUploadEvery.Enabled = rdoFTPInterval.Checked;

            pnlTrack.Enabled = chkTrack.Checked;

            rdoAny.Checked = CameraControl.Camobject.settings.ptzautotrackmode == 0;
            rdoVert.Checked = CameraControl.Camobject.settings.ptzautotrackmode == 1;
            rdoHor.Checked = CameraControl.Camobject.settings.ptzautotrackmode == 2;
            
            LoadPTZs();
            txtPTZURL.Text = CameraControl.Camobject.settings.ptzurlbase;
            if (CameraControl.Camobject.id == -1)
            {
                if (!SelectSource())
                    Close();
            }

            txtAccessGroups.Text = CameraControl.Camobject.settings.accessgroups;
            _loaded = true;
        }

        private void RenderResources()
        {
            btnBack.Text = LocRm.GetString("Back");
            btnDelete.Text = LocRm.GetString("Delete");
            btnDetectMovement.Text = LocRm.GetString("chars_3014702301470230147");
            btnFinish.Text = LocRm.GetString("Finish");
            btnMaskImage.Text = LocRm.GetString("chars_3014702301470230147");
            btnNext.Text = LocRm.GetString("Next");
            btnProperties.Text = LocRm.GetString("AdvProperties");
            btnSaveFTP.Text = LocRm.GetString("Test");
            btnSelectSource.Text = LocRm.GetString("chars_3014702301470230147");
            btnUpdate.Text = LocRm.GetString("Update");
            button1.Text = LocRm.GetString("ClearAll");
            button2.Text = LocRm.GetString("Add");
            chkActive.Text = LocRm.GetString("CameraActive");
            chkBeep.Text = LocRm.GetString("Beep");
            chkDeleteSource.Text = LocRm.GetString("DeleteAviAfterConversion");
            chkEmailOnDisconnect.Text = LocRm.GetString("SendEmailOnDisconnect");
            chkFlipX.Text = LocRm.GetString("Flipx");
            chkFlipY.Text = LocRm.GetString("Flipy");
            chkFri.Text = LocRm.GetString("Fri");
            chkFTP.Text = LocRm.GetString("FtpEnabled");
            
            chkMMS.Text = LocRm.GetString("SendAsMmsWithImage2Credit");
            chkMon.Text = LocRm.GetString("Mon");
            chkMovement.Text = LocRm.GetString("AlertsEnabled");
            chkPublic.Text = LocRm.GetString("PubliccheckThisToMakeYour");
            chkRecord.Text = LocRm.GetString("RecordOnMovementDetection");
            chkRecordAlert.Text = LocRm.GetString("RecordOnAlert");
            chkRecordSchedule.Text = LocRm.GetString("RecordOnScheduleStart");
            chkRestore.Text = LocRm.GetString("ShowIspyWindow");
            chkSat.Text = LocRm.GetString("Sat");
            chkSchedule.Text = LocRm.GetString("ScheduleCamera");
            chkScheduleActive.Text = LocRm.GetString("ScheduleActive");
            chkScheduleAlerts.Text = LocRm.GetString("AlertsEnabled");
            chkScheduleRecordOnDetect.Text = LocRm.GetString("RecordOnDetect");
            chkRecordAlertSchedule.Text = LocRm.GetString("RecordOnAlert");
            chkSendEmailMovement.Text = LocRm.GetString("SendEmailOnAlert");
            chkSendSMSMovement.Text = LocRm.GetString("SendSmsOnAlert");
            chkSun.Text = LocRm.GetString("Sun");
            chkSuppressNoise.Text = LocRm.GetString("SupressNoise");
            chkThu.Text = LocRm.GetString("Thu");
            chkTue.Text = LocRm.GetString("Tue");
            chkUploadYouTube.Text = LocRm.GetString("AutomaticallyUploadGenera");
            chkUsePassive.Text = LocRm.GetString("PassiveMode");
            chkWed.Text = LocRm.GetString("Wed");
            chkScheduleTimelapse.Text = LocRm.GetString("TimelapseEnabled");
            chkTimelapse.Text = LocRm.GetString("TimelapseEnabled");
            gbAdvanced.Text = LocRm.GetString("AdvancedCameraProperties");
            gbFTP.Text = LocRm.GetString("FtpDetails");
            gbZones.Text = LocRm.GetString("DetectionZones");
            gpbSubscriber.Text = gpbSubscriber2.Text = LocRm.GetString("WebServiceOptions");
            groupBox1.Text = LocRm.GetString("TimelapseRecording");
            groupBox2.Text = LocRm.GetString("FfmpegConversion");
            groupBox3.Text = LocRm.GetString("VideoSource");
            groupBox4.Text = LocRm.GetString("RecordingSettings");
            groupBox5.Text = LocRm.GetString("Detector");
            label1.Text = LocRm.GetString("Name");
            label10.Text = LocRm.GetString("chars_3801146");
            label11.Text = LocRm.GetString("TimeStamp");
            label12.Text = LocRm.GetString("UseDetector");
            label13.Text = LocRm.GetString("Seconds");
            label14.Text = LocRm.GetString("RecordTimelapse");
            label15.Text = LocRm.GetString("DistinctAlertInterval");
            label17.Text = LocRm.GetString("Frames");
            label19.Text = LocRm.GetString("Microphone");
            label2.Text = LocRm.GetString("Source");
            label20.Text = LocRm.GetString("Resolution");
            label21.Text = LocRm.GetString("FrameRate");
            label22.Text = LocRm.GetString("AlertOnObjectCount");
            label23.Text = LocRm.GetString("dependsOnYourWebcamCapabi");
            label24.Text = LocRm.GetString("Seconds");
            label25.Text = LocRm.GetString("CalibrationDelay");
            label26.Text = LocRm.GetString("PrebufferFrames");
            label27.Text = LocRm.GetString("Frames");
            label28.Text = LocRm.GetString("Seconds");
            label29.Text = LocRm.GetString("Buffer");
            label3.Text = LocRm.GetString("Sensitivity");
            label30.Text = LocRm.GetString("MaxRecordTime");
            label31.Text = LocRm.GetString("Seconds");
            label32.Text = LocRm.GetString("InactivityRecord");
            label33.Text = LocRm.GetString("Seconds");
            label34.Text = LocRm.GetString("MaxRecordTime");
            label35.Text = LocRm.GetString("Seconds");
            label36.Text = LocRm.GetString("Seconds");
            label37.Text = rdoFTPInterval.Text = LocRm.GetString("Interval");
            label38.Text = LocRm.GetString("MaxCalibrationDelay");
            label39.Text = LocRm.GetString("Seconds");
            label4.Text = LocRm.GetString("Mode");
            label40.Text = LocRm.GetString("InactivityRecord");
            label41.Text = LocRm.GetString("Seconds");
            label42.Text = LocRm.GetString("Minutesenter0ForNoEmails");
            label44.Text = LocRm.GetString("savesAFrameToAMovieFileNS");
            label45.Text = LocRm.GetString("EmailAddress");
            label46.Text = LocRm.GetString("DisplayStyle");
            label48.Text = LocRm.GetString("ColourFiltering");
           
            label49.Text = LocRm.GetString("Days");
            label5.Text = LocRm.GetString("EmailFrameEvery");
            label50.Text = LocRm.GetString("ImportantMakeSureYourSche");
            label51.Text = LocRm.GetString("ProcessEvery");
            label52.Text = LocRm.GetString("Server");
            label53.Text = LocRm.GetString("Port");
            label54.Text = LocRm.GetString("Username");
            label55.Text = LocRm.GetString("Password");
            label56.Text = LocRm.GetString("Filename");
            label57.Text = LocRm.GetString("UploadOn");
            label58.Text = LocRm.GetString("Seconds");
            label59.Text = LocRm.GetString("ispyCanUploadAnImageFromT");
            label6.Text = LocRm.GetString("ExecuteFile");
            label60.Text = LocRm.GetString("Egimagesmycamimagejpg");
            label61.Text = LocRm.GetString("Command");
            label62.Text = LocRm.GetString("ConversionFormat");
            label63.Text = LocRm.GetString("ispyUsesFfmpegToEncodeThe");
            label64.Text = LocRm.GetString("Frames");
            label65.Text = LocRm.GetString("UsePreset");
            label66.Text = LocRm.GetString("Description");
            label67.Text = LocRm.GetString("Images");
            label68.Text = LocRm.GetString("Interval");
            label69.Text = LocRm.GetString("Seconds");
            label7.Text = LocRm.GetString("Start");
            label70.Text = LocRm.GetString("savesAFrameEveryNSecondsn");
            label71.Text = LocRm.GetString("Movie");
            label73.Text = LocRm.GetString("CameraModel");
            //label74.Text = LocRM.GetString("NoteOnlyAvailableForIpCam");
            label75.Text = LocRm.GetString("ExtendedCommands");
            label76.Text = LocRm.GetString("ExitThisToEnableAlertsAnd");
            label77.Text = LocRm.GetString("Tags");
            label78.Text = LocRm.GetString("Category");
            label79.Text = LocRm.GetString("TipYouCanSelectivelyUploa");
            label8.Text = LocRm.GetString("chars_3801146");
            label80.Text = LocRm.GetString("TipToCreateAScheduleOvern");
            label81.Text = LocRm.GetString("tipUseADateStringFormatTo");
            label82.Text = LocRm.GetString("YourSmsNumber");
            label83.Text = LocRm.GetString("ClickAndDragTodraw");
            label84.Text = LocRm.GetString("MaskImage");
            label85.Text = LocRm.GetString("createATransparentpngImag");
            label86.Text = LocRm.GetString("OverlayText");
            label9.Text = LocRm.GetString("Stop");
            lblVideoSource.Text = LocRm.GetString("VideoSource");
            linkLabel1.Text = LocRm.GetString("UsageTips");
            linkLabel2.Text = LocRm.GetString("ScriptToRenderThisImageOn");
            linkLabel3.Text = LocRm.GetString("Info");
            linkLabel4.Text = linkLabel9.Text = LocRm.GetString("YouNeedAnActiveSubscripti");
            linkLabel5.Text = LocRm.GetString("HowToEnterYourNumber");
            linkLabel6.Text = LocRm.GetString("GetLatestList");
            linkLabel7.Text = LocRm.GetString("YoutubeSettings");
            linkLabel8.Text = LocRm.GetString("help");
            pnlScheduler.Text = LocRm.GetString("Scheduler");
            tabPage1.Text = LocRm.GetString("Camera");
            tabPage2.Text = rdoFTPAlerts.Text = LocRm.GetString("Alerts");
            tabPage3.Text = rdoFTPMotion.Text = LocRm.GetString("MotionDetection");
            tabPage4.Text = LocRm.GetString("Recording");
            tabPage5.Text = LocRm.GetString("Scheduling");
            tabPage6.Text = LocRm.GetString("Webservices");
            tabPage7.Text = LocRm.GetString("Ftp");
            tabPage8.Text = LocRm.GetString("Ptz");
            tabPage9.Text = LocRm.GetString("Youtube");
            tabPage10.Text = LocRm.GetString("Microphone");
            toolTip1.SetToolTip(txtMaskImage, LocRm.GetString("ToolTip_CameraName"));
            toolTip1.SetToolTip(txtCameraName, LocRm.GetString("ToolTip_CameraName"));
            toolTip1.SetToolTip(tbSensitivity, LocRm.GetString("ToolTip_MotionSensitivity"));
            toolTip1.SetToolTip(ddlObjectCount, LocRm.GetString("ToolTip_ObjectTracking"));
            toolTip1.SetToolTip(txtExecuteMovement, LocRm.GetString("ToolTip_EGMP3"));
            toolTip1.SetToolTip(txtTimeLapseFrames, LocRm.GetString("ToolTip_TimeLapseFrames"));
            toolTip1.SetToolTip(txtTimeLapse, LocRm.GetString("ToolTip_TimeLapseVideo"));
            toolTip1.SetToolTip(txtMaxRecordTime, LocRm.GetString("ToolTip_MaxDuration"));
            toolTip1.SetToolTip(txtInactiveRecord, LocRm.GetString("ToolTip_InactiveRecord"));
            toolTip1.SetToolTip(txtBuffer, LocRm.GetString("ToolTip_BufferFrames"));
            toolTip1.SetToolTip(txtCalibrationDelay, LocRm.GetString("ToolTip_DelayAlerts"));
            toolTip1.SetToolTip(lbSchedule, LocRm.GetString("ToolTip_PressDelete"));
            label16.Text = LocRm.GetString("PTZNote");
            chkRotate.Text = LocRm.GetString("Rotate90");
            chkPTZFlipX.Text = LocRm.GetString("Flipx");
            chkPTZFlipY.Text = LocRm.GetString("Flipy");
            chkPTZRotate90.Text = LocRm.GetString("Rotate90");
            label43.Text = LocRm.GetString("MaxFramerate");
            label47.Text = LocRm.GetString("WhenRecording");
            label74.Text = LocRm.GetString("Directory");
            chkAutoHome.Text = LocRm.GetString("AutoHome");
            label87.Text = LocRm.GetString("TimeToHome");
            llblHelp.Text = LocRm.GetString("help");

            chkColourProcessing.Text = LocRm.GetString("Apply");
            Text = LocRm.GetString("AddCamera");
            label72.Text = LocRm.GetString("arguments");
            rdoAny.Text = LocRm.GetString("AnyDirection");
            rdoVert.Text = LocRm.GetString("VertOnly");
            rdoHor.Text = LocRm.GetString("HorOnly");
            lblAccessGroups.Text = LocRm.GetString("AccessGroups");
        }


        private void LoadPTZs()
        {
            ddlPTZ.Items.Clear();
            ddlPTZ.Items.Add(new ListItem(LocRm.GetString("NoPTZcontrol"), "-1"));
            int ptzind = 0;
            int ind = 1;
            if (MainForm.PTZs != null)
            {
                foreach (PTZSettingsCamera ptz in MainForm.PTZs)
                {
                    ddlPTZ.Items.Add(new ListItem(ptz.Make + " v" + ptz.Version, ptz.id.ToString()));
                    if (CameraControl.Camobject.ptz == ptz.id)
                    {
                        ptzind = ind;
                        if (CameraControl.Camobject.settings.ptzurlbase == "")
                            CameraControl.Camobject.settings.ptzurlbase = ptz.CommandURL;
                    }
                    ind++;
                }
                ddlPTZ.SelectedIndex = ptzind;
            }
        }

        private void ShowSchedule(int selectedIndex)
        {
            lbSchedule.Items.Clear();
            int i = 0;
            foreach (string sched in CameraControl.ScheduleDetails)
            {
                if (sched != "")
                {
                    lbSchedule.Items.Add(new ListItem(sched, i.ToString()));
                    i++;
                }
            }
            if (selectedIndex > -1 && selectedIndex < lbSchedule.Items.Count)
                lbSchedule.SelectedIndex = selectedIndex;
        }

        private void CameraNewFrame(object sender, EventArgs e)
        {
            AreaControl.LastFrame = CameraControl.Camera.LastFrame;
            if (FilterForm != null)
                FilterForm.ImageProcess = CameraControl.Camera.LastFrame;
        }

        private void TbSensitivityScroll(object sender, EventArgs e)
        {
        }

        private void BtnNextClick(object sender, EventArgs e)
        {
            GoNext();
        }

        private void GoNext()
        {
            tcCamera.SelectedIndex++;
        }

        private void GoPrevious()
        {
            tcCamera.SelectedIndex--;
        }

        private bool CheckStep1()
        {
            string err = "";
            string name = txtCameraName.Text.Trim();
            if (name == "")
                err += LocRm.GetString("Validate_Camera_EnterName") + Environment.NewLine;
            if (
                MainForm.Cameras.SingleOrDefault(
                    p => p.name.ToLower() == name.ToLower() && p.id != CameraControl.Camobject.id) != null)
                err += LocRm.GetString("Validate_Camera_NameInUse") + Environment.NewLine;

            if (string.IsNullOrEmpty(CameraControl.Camobject.settings.videosourcestring))
            {
                err += LocRm.GetString("Validate_Camera_SelectVideoSource") + Environment.NewLine;
            }

            if (err != "")
            {
                MessageBox.Show(err, LocRm.GetString("Error"));
                tcCamera.SelectedIndex = 0;
                return false;
            }
            return true;
        }

        private void BtnFinishClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void Finish()
        {
            //validate page 0
            if (CheckStep1())
            {
                string err = "";
                if (chkSendSMSMovement.Checked && MainForm.Conf.ServicesEnabled &&
                    txtSMSNumber.Text.Trim() == "")
                    err += LocRm.GetString("Validate_Camera_MobileNumber") + Environment.NewLine;

                string sms = txtSMSNumber.Text.Trim().Replace(" ", "");
                if (sms.StartsWith("00"))
                    sms = sms.Substring(2);
                if (sms.StartsWith("+"))
                    sms = sms.Substring(1);
                if (sms != "")
                {
                    if (!IsNumeric(sms))
                        err += LocRm.GetString("Validate_Camera_SMSNumbers") + Environment.NewLine;
                }
                string email = txtEmailAlert.Text.Replace(" ", "");
                if (email != "" && !email.IsValidEmail())
                {
                    err += LocRm.GetString("Validate_Camera_EmailAlerts") + Environment.NewLine;
                }
                if (email == "")
                {
                    chkSendEmailMovement.Checked = false;
                    chkEmailOnDisconnect.Checked = false;
                    txtEmailGrabInterval.Value = 0;
                }
                if (sms == "")
                {
                    chkSendSMSMovement.Checked = false;
                    chkMMS.Checked = false;
                }

                if (txtBuffer.Text.Length < 1 || txtInactiveRecord.Text.Length < 1 ||
                    txtCalibrationDelay.Text.Length < 1 || txtMaxRecordTime.Text.Length < 1)
                {
                    err += LocRm.GetString("Validate_Camera_RecordingSettings") + Environment.NewLine;
                }
                if (err != "")
                {
                    MessageBox.Show(err, LocRm.GetString("Error"));
                    return;
                }

                if (chkFTP.Checked)
                {
                    try
                    {
                        var request = (FtpWebRequest) WebRequest.Create(txtFTPServer.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_Camera_CheckFTP"));
                        return;
                    }
                }

                if (chkSendEmailMovement.Checked)
                {
                    if (MainForm.Conf.WSUsername == "")
                    {
                        if (MessageBox.Show(
                            LocRm.GetString("Validate_Camera_Login"), LocRm.GetString("Note"), MessageBoxButtons.YesNo) ==
                            DialogResult.Yes)
                        {
                            var ws = new Webservices();
                            ws.ShowDialog(this);
                            if (ws.DialogResult != DialogResult.Yes)
                            {
                                chkSendEmailMovement.Checked = false;
                                chkMMS.Checked = false;
                                chkEmailOnDisconnect.Checked = false;
                                chkSendSMSMovement.Checked = false;
                            }
                            ws.Dispose();
                        }
                    }
                }
                int ftpport;
                if (!int.TryParse(txtFTPPort.Text, out ftpport))
                {
                    MessageBox.Show(LocRm.GetString("Validate_Camera_FTPPort"));
                    return;
                }
                int ftpinterval;
                if (!int.TryParse(txtUploadEvery.Text, out ftpinterval))
                {
                    MessageBox.Show(LocRm.GetString("Validate_Camera_FTPInterval"));
                    return;
                }

                int timelapseframes;
                if (!int.TryParse(txtTimeLapseFrames.Text, out timelapseframes))
                {
                    MessageBox.Show(LocRm.GetString("Validate_Camera_TimelapseInterval"));
                    return;
                }

                int timelapsemovie;

                if (!int.TryParse(txtTimeLapse.Text, out timelapsemovie))
                {
                    MessageBox.Show(LocRm.GetString("Validate_Camera_TimelapseBuffer"));
                    return;
                }


                CameraControl.Camobject.description = rtbDescription.Text;

                CameraControl.Camobject.detector.processeveryframe =
                    Convert.ToInt32(ddlProcessFrames.SelectedItem.ToString());
                CameraControl.Camobject.detector.motionzones = AreaControl.MotionZones;
                CameraControl.Camobject.detector.type = _detectortypes[ddlMotionDetector.SelectedIndex];
                CameraControl.Camobject.detector.postprocessor = _processortypes[ddlProcessor.SelectedIndex];
                CameraControl.Camobject.name = txtCameraName.Text.Trim();
                CameraControl.Camobject.alerts.active = chkMovement.Checked;
                CameraControl.Camobject.alerts.executefile = txtExecuteMovement.Text;
                CameraControl.Camobject.alerts.alertoptions = chkBeep.Checked + "," + chkRestore.Checked;
                CameraControl.Camobject.notifications.sendemail = chkSendEmailMovement.Checked;
                CameraControl.Camobject.notifications.sendsms = chkSendSMSMovement.Checked;
                CameraControl.Camobject.notifications.sendmms = chkMMS.Checked;
                CameraControl.Camobject.settings.emailaddress = email;
                CameraControl.Camobject.settings.smsnumber = sms;
                CameraControl.Camobject.settings.notifyondisconnect = chkEmailOnDisconnect.Checked;
                
                
                if (txtDirectory.Text.Trim() == "")
                    txtDirectory.Text = MainForm.RandomString(5);

                if (CameraControl.Camobject.directory != txtDirectory.Text)
                {
                    string path = MainForm.Conf.MediaDirectory + "video\\" + txtDirectory.Text + "\\";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    path = MainForm.Conf.MediaDirectory + "video\\" + txtDirectory.Text + "\\thumbs\\";

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }

                CameraControl.Camobject.directory = txtDirectory.Text;

                CameraControl.Camobject.schedule.active = chkSchedule.Checked;
                CameraControl.Camobject.settings.active = chkActive.Checked;
                CameraControl.Camobject.detector.recordondetect = chkRecord.Checked;
                CameraControl.Camobject.alerts.objectcountalert = Convert.ToInt32(ddlObjectCount.Value);

                int bufferframes,
                    calibrationdelay,
                    inactiveRecord,
                    minimuminterval,
                    maxrecord,
                    emailgrabinterval;
                int.TryParse(txtBuffer.Text, out bufferframes);
                int.TryParse(txtCalibrationDelay.Text, out calibrationdelay);
                int.TryParse(txtInactiveRecord.Text, out inactiveRecord);
                int.TryParse(txtMinimumInterval.Text, out minimuminterval);
                int.TryParse(txtMaxRecordTime.Text, out maxrecord);
                int.TryParse(txtEmailGrabInterval.Text, out emailgrabinterval);


                CameraControl.Camobject.recorder.bufferframes = bufferframes;
                CameraControl.Camobject.detector.calibrationdelay = calibrationdelay;
                CameraControl.Camobject.recorder.inactiverecord = inactiveRecord;
                CameraControl.Camobject.alerts.minimuminterval = minimuminterval;
                CameraControl.Camobject.recorder.maxrecordtime = maxrecord;
                CameraControl.Camobject.recorder.timelapseenabled = chkTimelapse.Checked;
                CameraControl.Camobject.notifications.emailgrabinterval = emailgrabinterval;

                CameraControl.Camobject.ftp.enabled = chkFTP.Checked;
                CameraControl.Camobject.ftp.server = txtFTPServer.Text;
                CameraControl.Camobject.ftp.usepassive = chkUsePassive.Checked;
                CameraControl.Camobject.ftp.username = txtFTPUsername.Text;
                CameraControl.Camobject.ftp.password = txtFTPPassword.Text;
                CameraControl.Camobject.ftp.port = ftpport;
                CameraControl.Camobject.ftp.interval = ftpinterval;
                CameraControl.Camobject.ftp.filename = txtFTPFilename.Text;
                CameraControl.Camobject.ftp.text = txtFTPText.Text;
                CameraControl.Camobject.settings.ffmpeg = txtPostProcess.Text.Replace(Environment.NewLine, " ");
                CameraControl.Camobject.settings.deleteavi = chkDeleteSource.Checked;
                CameraControl.Camobject.settings.ptzautotrack = chkTrack.Checked;
                CameraControl.Camobject.settings.ptzautohome = chkAutoHome.Checked;
                CameraControl.Camobject.settings.ptzautotrackmode = 0;

                if (rdoVert.Checked)
                    CameraControl.Camobject.settings.ptzautotrackmode = 1;
                if (rdoHor.Checked)
                    CameraControl.Camobject.settings.ptzautotrackmode = 2;

                CameraControl.Camobject.settings.ptztimetohome = Convert.ToInt32(numTTH.Value);

                int ftpmode = 0;
                if (rdoFTPAlerts.Checked)
                    ftpmode = 1;
                if (rdoFTPInterval.Checked)
                    ftpmode = 2;

                CameraControl.Camobject.ftp.mode = ftpmode;

                CameraControl.Camobject.recorder.timelapseframes = timelapseframes;
                CameraControl.Camobject.recorder.timelapse = timelapsemovie;

                CameraControl.Camobject.settings.youtube.autoupload = chkUploadYouTube.Checked;
                CameraControl.Camobject.settings.youtube.category = ddlCategory.SelectedItem.ToString();
                CameraControl.Camobject.settings.youtube.@public = chkPublic.Checked;
                CameraControl.Camobject.settings.youtube.tags = txtTags.Text;
                CameraControl.Camobject.settings.maxframeraterecord = (int)numMaxFRRecording.Value;

                CameraControl.Camobject.alerts.arguments = txtArguments.Text;

                CameraControl.Camobject.settings.accessgroups = txtAccessGroups.Text;

                CameraControl.UpdateFloorplans(false);

                DialogResult = DialogResult.OK;
                MainForm.NeedsSync = true;
                Close();
                return;
            }
            return;
        }

        private static bool IsNumeric(IEnumerable<char> numberString)
        {
            return numberString.All(c => char.IsNumber(c));
        }

        private void ChkMovementCheckedChanged(object sender, EventArgs e)
        {
            pnlMovement.Enabled = (chkMovement.Checked);
            CameraControl.Camobject.alerts.active = chkMovement.Checked;
        }

        private void BtnDetectMovementClick(object sender, EventArgs e)
        {
            ofdDetect.FileName = "";
            ofdDetect.InitialDirectory = Program.AppPath + @"sounds\";
            ofdDetect.ShowDialog();
            if (ofdDetect.FileName != "")
            {
                txtExecuteMovement.Text = ofdDetect.FileName;
            }
        }

        private void ChkScheduleCheckedChanged(object sender, EventArgs e)
        {
            pnlScheduler.Enabled = chkSchedule.Checked;
            btnDelete.Enabled = btnUpdate.Enabled = lbSchedule.SelectedIndex > -1;
            lbSchedule.Refresh();
        }

        private void ChkSendSmsMovementCheckedChanged(object sender, EventArgs e)
        {
            if (chkSendSMSMovement.Checked)
                chkMMS.Checked = false;
        }

        private void ChkSendEmailMovementCheckedChanged(object sender, EventArgs e)
        {
        }

        private void TxtCameraNameKeyUp(object sender, KeyEventArgs e)
        {
            CameraControl.Camobject.name = txtCameraName.Text;
        }


        private void ChkActiveCheckedChanged(object sender, EventArgs e)
        {
            if (CameraControl.Camobject.settings.active != chkActive.Checked)
            {
                if (chkActive.Checked)
                {
                    CameraControl.Enable();
                    if (CameraControl.Camera != null)
                        CameraControl.Camera.NewFrame += CameraNewFrame;
                }
                else
                {
                    if (CameraControl.Camera != null)
                        CameraControl.Camera.NewFrame -= CameraNewFrame;
                    CameraControl.Disable();
                }
            }
            gbAdvanced.Enabled = chkActive.Checked;
        }

        private void TxtCameraNameTextChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.name = txtCameraName.Text;
        }

        private void AddCameraFormClosing(object sender, FormClosingEventArgs e)
        {
            if (CameraControl.Camera != null)
                CameraControl.Camera.NewFrame -= CameraNewFrame;
            AreaControl.Dispose();
            CameraControl.IsEdit = false;
        }

        private void ChkRecordCheckedChanged(object sender, EventArgs e)
        {
            if (chkRecord.Checked)
                chkRecordAlert.Checked = false;

            CameraControl.Camobject.detector.recordondetect = chkRecord.Checked;
        }

        private void BtnPropertiesClick(object sender, EventArgs e)
        {
            if (CameraControl.Camera != null && CameraControl.Camera.VideoSource != null &&
                CameraControl.Camera.VideoSource is VideoCaptureDevice)
            {
                ((VideoCaptureDevice) CameraControl.Camera.VideoSource).DisplayPropertyPage(Handle);
            }
        }

        private void DdlFrameSizeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                SetResolution();
            }
        }

        private void DdlFrameRateSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                SetFramerate();
            }
        }

        private void DdlMovementDetectorSelectedIndexChanged1(object sender, EventArgs e)
        {
            ddlProcessor.Enabled = _detectortypes[ddlMotionDetector.SelectedIndex] != "None";

            if (CameraControl.Camera != null && CameraControl.Camera.VideoSource != null)
            {
                if (_detectortypes[ddlMotionDetector.SelectedIndex] != CameraControl.Camobject.detector.type)
                {
                    CameraControl.Camobject.detector.type = _detectortypes[ddlMotionDetector.SelectedIndex];
                    SetDetector();
                }
            }
            CameraControl.Camobject.detector.type = _detectortypes[ddlMotionDetector.SelectedIndex];
        }

        private void SetDetector()
        {
            if (CameraControl.Camera == null)
                return;
            CameraControl.Camera.MotionDetector = null;
            switch (CameraControl.Camobject.detector.type)
            {
                case "Two Frames":
                    CameraControl.Camera.MotionDetector =
                        new MotionDetector(
                            new TwoFramesDifferenceDetector(CameraControl.Camobject.settings.suppressnoise));
                    SetProcessor();
                    break;
                case "Custom Frame":
                    CameraControl.Camera.MotionDetector =
                        new MotionDetector(
                            new CustomFrameDifferenceDetector(CameraControl.Camobject.settings.suppressnoise,
                                                              CameraControl.Camobject.detector.keepobjectedges));
                    SetProcessor();
                    break;
                case "Background Modelling":
                    CameraControl.Camera.MotionDetector =
                        new MotionDetector(
                            new SimpleBackgroundModelingDetector(CameraControl.Camobject.settings.suppressnoise,
                                                                 CameraControl.Camobject.detector.keepobjectedges));
                    SetProcessor();
                    break;
                case "None":
                    break;
            }
        }

        private void SetProcessor()
        {
            if (CameraControl.Camera == null || CameraControl.Camera.MotionDetector == null)
                return;
            CameraControl.Camera.MotionDetector.MotionProcessingAlgorithm = null;
            
            switch (CameraControl.Camobject.detector.postprocessor)
            {
                case "Grid Processing":
                    CameraControl.Camera.MotionDetector.MotionProcessingAlgorithm = new GridMotionAreaProcessing
                                   {
                                       HighlightColor = ColorTranslator.FromHtml(CameraControl.Camobject.detector.color),
                                       HighlightMotionGrid = CameraControl.Camobject.detector.highlight
                                   };
                    break;
                case "Object Tracking":
                    CameraControl.Camera.MotionDetector.MotionProcessingAlgorithm = new BlobCountingObjectsProcessing
                                    {
                                        HighlightColor = ColorTranslator.FromHtml(CameraControl.Camobject.detector.color),
                                        HighlightMotionRegions = CameraControl.Camobject.detector.highlight,
                                        MinObjectsHeight = CameraControl.Camobject.detector.minheight,
                                        MinObjectsWidth = CameraControl.Camobject.detector.minwidth
                                    };

                    break;
                case "Border Highlighting":
                    CameraControl.Camera.MotionDetector.MotionProcessingAlgorithm = new MotionBorderHighlighting
                                    {
                                        HighlightColor = ColorTranslator.FromHtml(CameraControl.Camobject.detector.color)
                                    };
                    break;
                case "Area Highlighting":
                    CameraControl.Camera.MotionDetector.MotionProcessingAlgorithm = new MotionAreaHighlighting
                                    {
                                        HighlightColor = ColorTranslator.FromHtml(CameraControl.Camobject.detector.color)
                                    };
                    break;
                case "None":
                    break;
            }
        }

        private void ChkSuppressNoiseCheckedChanged(object sender, EventArgs e)
        {
            if (CameraControl.Camera != null && CameraControl.Camera.VideoSource != null)
            {
                if (CameraControl.Camobject.settings.suppressnoise != chkSuppressNoise.Checked)
                {
                    CameraControl.Camobject.settings.suppressnoise = chkSuppressNoise.Checked;
                    SetDetector();
                }
            }
        }


        private void Button2Click(object sender, EventArgs e)
        {
            GoPrevious();
        }

        private void Label23Click(object sender, EventArgs e)
        {
        }

        private void TcCameraSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcCamera.SelectedIndex == 0)
                btnBack.Enabled = false;
            else
                btnBack.Enabled = true;

            if (tcCamera.SelectedIndex == tcCamera.TabCount - 1)
                btnNext.Enabled = false;
            else
                btnNext.Enabled = true;
        }

        private void Button1Click1(object sender, EventArgs e)
        {
            AreaControl.ClearRectangles();
            if (CameraControl.Camera != null && CameraControl.Camera.MotionDetector != null)
            {
                CameraControl.Camera.ClearMotionZones();
            }
        }

        private void DdlProcessorSelectedIndexChanged(object sender, EventArgs e)
        {
            if (CameraControl.Camera != null && CameraControl.Camera.VideoSource != null &&
                CameraControl.Camera.MotionDetector != null)
            {
                if (_processortypes[ddlProcessor.SelectedIndex] != CameraControl.Camobject.detector.postprocessor)
                {
                    CameraControl.Camobject.detector.postprocessor = _processortypes[ddlProcessor.SelectedIndex];
                    SetProcessor();
                }
            }
            CameraControl.Camobject.detector.postprocessor = _processortypes[ddlProcessor.SelectedIndex];

            ddlObjectCount.Enabled = (ddlProcessor.SelectedIndex == 1 || ddlProcessor.SelectedIndex == 2);
        }

        private void Button2Click1(object sender, EventArgs e)
        {
            List<objectsCameraScheduleEntry> scheds = CameraControl.Camobject.schedule.entries.ToList();
            var sched = new objectsCameraScheduleEntry();
            if (ConfigureSchedule(sched))
            {
                scheds.Add(sched);
                CameraControl.Camobject.schedule.entries = scheds.ToArray();
                ShowSchedule(CameraControl.Camobject.schedule.entries.Count() - 1);
            }
        }

        private bool ConfigureSchedule(objectsCameraScheduleEntry sched)
        {
            if (ddlHourStart.SelectedItem.ToString() == "-" || ddlMinuteStart.SelectedItem.ToString() == "-")
            {
                sched.start = "-:-";
            }
            else
                sched.start = ddlHourStart.SelectedItem + ":" + ddlMinuteStart.SelectedItem;
            if (ddlHourEnd.SelectedItem.ToString() == "-" || ddlMinuteEnd.SelectedItem.ToString() == "-")
            {
                sched.stop = "-:-";
            }
            else
                sched.stop = ddlHourEnd.SelectedItem + ":" + ddlMinuteEnd.SelectedItem;

            sched.daysofweek = "";
            if (chkMon.Checked)
            {
                sched.daysofweek += "1,";
            }
            if (chkTue.Checked)
            {
                sched.daysofweek += "2,";
            }
            if (chkWed.Checked)
            {
                sched.daysofweek += "3,";
            }
            if (chkThu.Checked)
            {
                sched.daysofweek += "4,";
            }
            if (chkFri.Checked)
            {
                sched.daysofweek += "5,";
            }
            if (chkSat.Checked)
            {
                sched.daysofweek += "6,";
            }
            if (chkSun.Checked)
            {
                sched.daysofweek += "0,";
            }
            sched.daysofweek = sched.daysofweek.Trim(',');
            if (sched.daysofweek == "")
            {
                MessageBox.Show(LocRm.GetString("Validate_Camera_SelectOneDay"));
                return false;
            }

            sched.recordonstart = chkRecordSchedule.Checked;
            sched.active = chkScheduleActive.Checked;
            sched.recordondetect = chkScheduleRecordOnDetect.Checked;
            sched.recordonalert = chkRecordAlertSchedule.Checked;
            sched.alerts = chkScheduleAlerts.Checked;
            sched.timelapseenabled = chkScheduleTimelapse.Checked;
            return true;
        }

        private void LbScheduleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSchedule();
            }
        }

        private void DeleteSchedule()
        {
            if (lbSchedule.SelectedIndex > -1)
            {
                int i = lbSchedule.SelectedIndex;
                List<objectsCameraScheduleEntry> scheds = CameraControl.Camobject.schedule.entries.ToList();
                scheds.RemoveAt(i);
                CameraControl.Camobject.schedule.entries = scheds.ToArray();
                int j = i;
                if (j == scheds.Count)
                    j--;
                if (j < 0)
                    j = 0;
                ShowSchedule(j);
                if (lbSchedule.Items.Count == 0)
                    btnDelete.Enabled = btnUpdate.Enabled = false;
                else
                    btnDelete.Enabled = btnUpdate.Enabled = (lbSchedule.SelectedIndex > -1);
            }
        }

        private void DdlHourStartSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void LinkLabel1LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/help.aspx#4");
        }

        private void BtnSaveFtpClick(object sender, EventArgs e)
        {
            btnSaveFTP.Enabled = false;
            Application.DoEvents();

            var imageStream = new MemoryStream();

            Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
            Image myThumbnail = null;

            try
            {
                if (CameraControl.Camera != null && CameraControl.Camera.LastFrame != null)
                    myThumbnail = CameraControl.Camera.LastFrame;
                        //CameraControl.Camera.LastFrame.GetThumbnailImage(320, 240, myCallback, IntPtr.Zero);
                else
                    myThumbnail =
                        Image.FromFile(Program.AppDataPath + @"WebServerRoot\images\camoffline.jpg").GetThumbnailImage(
                            320, 240, myCallback, IntPtr.Zero);

                // put the image into the memory stream

                myThumbnail.Save(imageStream, ImageFormat.Jpeg);
                string error;
                txtFTPServer.Text = txtFTPServer.Text.Trim('/');
                string fn = String.Format(System.Globalization.CultureInfo.InvariantCulture, txtFTPFilename.Text, DateTime.Now);
                if ((new AsynchronousFtpUpLoader()).FTP(txtFTPServer.Text + ":" + txtFTPPort.Text, chkUsePassive.Checked,
                                                        txtFTPUsername.Text, txtFTPPassword.Text, fn,
                                                        imageStream.ToArray(), out error))
                {
                    MessageBox.Show(LocRm.GetString("ImageUploaded"), LocRm.GetString("Success"));
                }
                else
                    MessageBox.Show(LocRm.GetString("UploadFailed")+": "+error, LocRm.GetString("Failed"));
            }
            catch (Exception ex)
            {
                MainForm.LogExceptionToFile(ex);
                MessageBox.Show(ex.Message);
            }
            imageStream.Dispose();
            if (myThumbnail != null)
                myThumbnail.Dispose();
            btnSaveFTP.Enabled = true;
        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        private void CheckBox1CheckedChanged(object sender, EventArgs e)
        {
            gbFTP.Enabled = chkFTP.Checked;
        }

        private void LinkLabel2LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/help.aspx#8.6");
        }

        private void DdlPresetSelectedIndexChanged(object sender, EventArgs e)
        {
            string val = ((ListItem) ddlPreset.SelectedItem).Value;
            if (val != "")
            {
                txtPostProcess.Text = val;
            }
        }

        private void LinkLabel3LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MainForm.StartBrowser("http://www.ffmpeg.org/ffmpeg-doc.html");
        }


        private void DdlProcessFramesSelectedIndexChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.detector.processeveryframe = Convert.ToInt32(ddlProcessFrames.SelectedItem);
        }

        private void DdlFfPresetSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_ffpresetwarned)
            {
                _ffpresetwarned = true;
                MessageBox.Show(LocRm.GetString("Validate_Camera_FFPresetChange"));
            }
            string fpre = ddlFFPreset.SelectedItem.ToString();
                // "-i \"{filename}.avi\" -r {framerate} -vcodec libx264 -f mp4 -fpre \"{presetdir}libx264-slow.ffpreset\" -aspect 4:3 -an \"{filename}.mp4\""));
            if (ddlFFPreset.SelectedIndex == 0)
            {
                fpre = "";
            }
            string cmd = txtPostProcess.Text;
            if (cmd.IndexOf("-fpre ") != -1)
            {
                string part1 = cmd.Substring(0, cmd.IndexOf("-fpre"));
                string part2 = cmd.Substring(cmd.IndexOf("-fpre")) + " ";
                int i = part2.IndexOf("\"");
                if (i != -1)
                {
                    part2 = part2.Substring(i + 1);
                    i = part2.IndexOf("\"");
                    if (i != -1)
                    {
                        part2 = part2.Substring(i + 1);
                    }
                }
                cmd = part1.Trim() + " -fpre \"{presetdir}" + fpre + ".ffpreset\"" + " " + part2.Trim();
            }
            else
                cmd = cmd.Trim() + " -fpre \"{presetdir}" + fpre + ".ffpreset\"";

            txtPostProcess.Text = cmd;
        }

        private void Login()
        {
            ((MainForm) Owner).Connect(MainForm.Website+"/subscribe.aspx");
            gpbSubscriber.Enabled = gpbSubscriber2.Enabled = MainForm.Conf.Subscribed;
        }


        private void Button3Click(object sender, EventArgs e)
        {
            DeleteSchedule();
        }

        private void LbScheduleSelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbSchedule.Items.Count == 0)
                btnDelete.Enabled = btnUpdate.Enabled = false;
            else
            {
                btnUpdate.Enabled = btnDelete.Enabled = (lbSchedule.SelectedIndex > -1);
                if (btnUpdate.Enabled)
                {
                    int i = lbSchedule.SelectedIndex;
                    objectsCameraScheduleEntry sched = CameraControl.Camobject.schedule.entries[i];

                    string[] start = sched.start.Split(':');
                    string[] stop = sched.stop.Split(':');


                    ddlHourStart.SelectedItem = start[0];
                    ddlHourEnd.SelectedItem = stop[0];
                    ddlMinuteStart.SelectedItem = start[1];
                    ddlMinuteEnd.SelectedItem = stop[1];

                    chkMon.Checked = sched.daysofweek.IndexOf("1") != -1;
                    chkTue.Checked = sched.daysofweek.IndexOf("2") != -1;
                    chkWed.Checked = sched.daysofweek.IndexOf("3") != -1;
                    chkThu.Checked = sched.daysofweek.IndexOf("4") != -1;
                    chkFri.Checked = sched.daysofweek.IndexOf("5") != -1;
                    chkSat.Checked = sched.daysofweek.IndexOf("6") != -1;
                    chkSun.Checked = sched.daysofweek.IndexOf("0") != -1;

                    chkRecordSchedule.Checked = sched.recordonstart;
                    chkScheduleActive.Checked = sched.active;
                    chkScheduleRecordOnDetect.Checked = sched.recordondetect;
                    chkScheduleAlerts.Checked = sched.alerts;
                    chkRecordAlertSchedule.Checked = sched.recordonalert;
                    chkScheduleTimelapse.Checked = sched.timelapseenabled;
                }
            }
        }

        private void SetResolution()
        {
            int fr = Convert.ToInt32(ddlFrameRate.SelectedItem.ToString());

            CameraControl.Camobject.settings.framerate = fr;
            if (CameraControl.Camera != null && CameraControl.Camera.VideoSource != null &&
                CameraControl.Camera.VideoSource is VideoCaptureDevice && _loaded && ddlFrameSize.SelectedItem != null)
            {
                string[] sz = ddlFrameSize.SelectedItem.ToString().Split(' ');
                sz = sz[0].Split('x');
                var newSize = new Size(Convert.ToInt32(sz[0]), Convert.ToInt32(sz[1]));
                if (CameraControl.Camobject.resolution != ddlFrameSize.SelectedItem.ToString())
                {
                    CameraControl.Camobject.resolution = newSize.Width + "x" + newSize.Height;
                    CameraControl.NeedSizeUpdate = true;
                    if (CameraControl.Camobject.settings.active)
                    {
                        CameraControl.Enable();
                        if (CameraControl.Camera != null)
                        {
                            CameraControl.Camera.NewFrame -= CameraNewFrame;
                            CameraControl.Camera.NewFrame += CameraNewFrame;
                        }
                    }
                }
            }
        }

        private void SetFramerate()
        {
            int fr = Convert.ToInt32(ddlFrameRate.SelectedItem.ToString());

            CameraControl.Camobject.settings.framerate = fr;
            if (CameraControl.Camera != null && CameraControl.Camera.VideoSource != null &&
                CameraControl.Camera.VideoSource is VideoCaptureDevice && _loaded && ddlFrameSize.SelectedItem != null)
            {
                CameraControl.Enable();
                if (CameraControl.Camera != null)
                {
                    CameraControl.Camera.NewFrame -= CameraNewFrame;
                    CameraControl.Camera.NewFrame += CameraNewFrame;
                    CameraControl.NeedSizeUpdate = true;
                }
            }
        }

        private void DdlObjectCountValueChanged(object sender, EventArgs e)
        {
            if (CameraControl.Camobject.alerts.objectcountalert != Convert.ToInt32(ddlObjectCount.Value))
            {
                CameraControl.Camobject.alerts.objectcountalert = Convert.ToInt32(ddlObjectCount.Value);
                CameraControl.Enable();
            }
        }

        private void DdlMicSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                var li = (ListItem) ddlMic.SelectedItem;
                CameraControl.Camobject.settings.micpair = Convert.ToInt32(li.Value);
                MainForm.NeedsRedraw = true;
            }
        }

        private void PnlPtzMouseDown(object sender, MouseEventArgs e)
        {
            ProcessPtzInput(e.Location);
        }


        private void ProcessPtzInput(Point p)
        {
            Enums.PtzCommand comm = Enums.PtzCommand.Center;
            if (p.X < 60 && p.Y > 60 && p.Y < 106)
            {
                comm = Enums.PtzCommand.Left;
            }
            if (p.X < 60 && p.Y < 60)
            {
                comm = Enums.PtzCommand.Upleft;
            }
            if (p.X > 60 && p.X < 104 && p.Y < 60)
            {
                comm = Enums.PtzCommand.Up;
            }
            if (p.X > 104 && p.X < 164 && p.Y < 60)
            {
                comm = Enums.PtzCommand.UpRight;
            }
            if (p.X > 104 && p.X < 170 && p.Y > 60 && p.Y < 104)
            {
                comm = Enums.PtzCommand.Right;
            }
            if (p.X > 104 && p.X < 170 && p.Y > 104)
            {
                comm = Enums.PtzCommand.DownRight;
            }
            if (p.X > 60 && p.X < 104 && p.Y > 104)
            {
                comm = Enums.PtzCommand.Down;
            }
            if (p.X < 60 && p.Y > 104)
            {
                comm = Enums.PtzCommand.DownLeft;
            }

            if (p.X > 170 && p.Y < 45)
            {
                comm = Enums.PtzCommand.ZoomIn;
            }
            if (p.X > 170 && p.Y > 45 && p.Y < 90)
            {
                comm = Enums.PtzCommand.ZoomOut;
            }

            CameraControl.PTZ.SendPTZCommand(comm);
        }

        private void DdlPtzSelectedIndexChanged(object sender, EventArgs e)
        {
            
            CameraControl.Camobject.ptz = Convert.ToInt32(((ListItem) ddlPTZ.SelectedItem).Value);
            lbExtended.Items.Clear();
            if (CameraControl.Camobject.ptz > -1)
            {
                PTZSettingsCamera ptz = MainForm.PTZs.Single(p => p.id == CameraControl.Camobject.ptz);
                foreach (PTZSettingsCameraCommandsCommand extcmd in ptz.Commands.ExtendedCommands)
                {
                    lbExtended.Items.Add(new ListItem(extcmd.Name, extcmd.Value));
                }
                if (_loaded)    
                    txtPTZURL.Text = ptz.CommandURL;
            }
            pnlPTZControls.Enabled = CameraControl.Camobject.ptz > -1;            
        }

        private void PnlPtzPaint(object sender, PaintEventArgs e)
        {
        }

        private void LbExtendedClick(object sender, EventArgs e)
        {
            if (lbExtended.SelectedIndex > -1)
            {
                var li = ((ListItem) lbExtended.SelectedItem);
                SendPtzCommand(li.Value, true);
            }
        }


        private void PnlPtzMouseUp(object sender, MouseEventArgs e)
        {
            PTZSettingsCamera ptz = MainForm.PTZs.SingleOrDefault(p => p.id == CameraControl.Camobject.ptz);
            if (ptz != null && ptz.Commands.Stop!="")
                SendPtzCommand(ptz.Commands.Stop,true);
        }

        private void SendPtzCommand(string cmd, bool wait)
        {
            if (cmd == "")
            {
                MessageBox.Show(LocRm.GetString("CommandNotSupported"));
                return;
            }
            try
            {
                CameraControl.PTZ.SendPTZCommand(cmd, wait);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LocRm.GetString("Validate_Camera_PTZIPOnly") + Environment.NewLine + Environment.NewLine +
                    ex.Message, LocRm.GetString("Error"));
            }
        }

        private void PnlPtzMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //todo: add drag to move cam around
            }
        }

        private void LbExtendedSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ChkUploadYouTubeCheckedChanged(object sender, EventArgs e)
        {
            if (chkUploadYouTube.Checked)
            {
                if (string.IsNullOrEmpty(MainForm.Conf.YouTubeUsername))
                {
                    if (
                        MessageBox.Show(LocRm.GetString("Validate_Camera_YouTubeDetails"), LocRm.GetString("Confirm"),
                                        MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        string lang = MainForm.Conf.Language;
                        ((MainForm) Owner).ShowSettings(4);
                        if (lang != MainForm.Conf.Language)
                            RenderResources();
                    }
                }
            }

            if (string.IsNullOrEmpty(MainForm.Conf.YouTubeUsername))
            {
                chkUploadYouTube.Checked = false;
            }
        }

        private void LinkLabel6LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(MainForm.Website+"/PTZ.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            doc.Save(Program.AppDataPath + @"XML\PTZ.xml");
            MainForm.PTZs = null;
            LoadPTZs();
            MessageBox.Show(LocRm.GetString("LoadedPTZDefinitions"), LocRm.GetString("Note"));
        }

        private void TabPage9Click(object sender, EventArgs e)
        {
        }

        private void LinkLabel7LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string lang = MainForm.Conf.Language;
            ((MainForm) Owner).ShowSettings(4);
            if (lang != MainForm.Conf.Language)
                RenderResources();
        }

        private void TxtSensitivityKeyUp(object sender, KeyEventArgs e)
        {
            double sv;
            if (!double.TryParse(txtSensitivity.Text, out sv)) return;
            CameraControl.Camobject.detector.sensitivity = sv;
            if (CameraControl.Camera != null)
            {
                CameraControl.Camera.AlarmLevel = Helper.CalculateSensitivity(sv);
            }
        }

        private void TbSensitivityValueChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                if (CameraControl.Camera != null)
                {
                    CameraControl.Camera.AlarmLevel =
                        Helper.CalculateSensitivity(Convert.ToDouble(tbSensitivity.Value)/10.0);
                }
                CameraControl.Camobject.detector.sensitivity = Convert.ToDouble(tbSensitivity.Value)/10.0;
                txtSensitivity.Text = CameraControl.Camobject.detector.sensitivity.ToString();
            }
        }

        private void GbAdvancedEnabledChanged(object sender, EventArgs e)
        {
            if (gbAdvanced.Enabled)
            {
                if (CameraControl.Camera != null && CameraControl.Camera.VideoSource is VideoCaptureDevice)
                {
                    ddlFrameSize.Items.Clear();
                    try
                    {
                        var vscap = new VideoCaptureDevice(CameraControl.Camobject.settings.videosourcestring);
                        foreach (VideoCapabilities capabilityInfo in vscap.VideoCapabilities)
                        {
                            ddlFrameSize.Items.Add(string.Format("{0}x{1} (max {2} fps)\r\n",
                                                                 capabilityInfo.FrameSize.Width,
                                                                 capabilityInfo.FrameSize.Height,
                                                                 capabilityInfo.FrameRate));
                        }
                        for (int i = 0; i < ddlFrameSize.Items.Count; i++)
                        {
                            if (ddlFrameSize.Items[i].ToString().StartsWith(CameraControl.Camobject.resolution))
                            {
                                ddlFrameSize.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                else
                    gbAdvanced.Enabled = false;
            }
        }

        private void LinkLabel4LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login();
        }

        private void DdlTimestampKeyUp(object sender, KeyEventArgs e)
        {
            CameraControl.Camobject.settings.timestampformatter = ddlTimestamp.Text;
        }

        private void BtnMaskImageClick(object sender, EventArgs e)
        {
            ofdDetect.FileName = "";
            ofdDetect.InitialDirectory = Program.AppPath + @"backgrounds\";
            ofdDetect.Filter = "Image Files (*.png)|*.png";
            ofdDetect.ShowDialog();
            if (ofdDetect.FileName != "")
            {
                txtMaskImage.Text = ofdDetect.FileName;
            }
        }

        private void TxtMaskImageTextChanged(object sender, EventArgs e)
        {
            if (File.Exists(txtMaskImage.Text))
            {
                try
                {
                    CameraControl.Camera.Mask = Image.FromFile(txtMaskImage.Text);
                    CameraControl.Camobject.settings.maskimage = txtMaskImage.Text;
                }
                catch (Exception)
                {
                }
            }
            else
            {
                CameraControl.Camera.Mask = null;
                CameraControl.Camobject.settings.maskimage = "";
            }
        }

        private void LinkLabel5LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(null, MainForm.Website+"/countrycodes.aspx");
        }

        private void ChkEmailOnDisconnectCheckedChanged(object sender, EventArgs e)
        {
        }

        private void ChkFlipYCheckedChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.flipy = chkFlipY.Checked;
        }

        private void ChkFlipXCheckedChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.flipx = chkFlipX.Checked;
        }

        private void BtnUpdateClick(object sender, EventArgs e)
        {
            int i = lbSchedule.SelectedIndex;
            objectsCameraScheduleEntry sched = CameraControl.Camobject.schedule.entries[i];

            if (ConfigureSchedule(sched))
            {
                ShowSchedule(i);
            }
        }

        private void ChkScheduleActiveCheckedChanged(object sender, EventArgs e)
        {
        }

        private void LbScheduleDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            int i = e.Index;
            if (i >= 0)
            {
                objectsCameraScheduleEntry sched = CameraControl.Camobject.schedule.entries[i];

                Font f = sched.active ? new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold) : new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
                Brush b = !chkSchedule.Checked ? Brushes.Gray : Brushes.Black;

                e.Graphics.DrawString(lbSchedule.Items[i].ToString(), f, b, e.Bounds);
                e.DrawFocusRectangle();
            }
        }


        private void ChkRecordScheduleCheckedChanged(object sender, EventArgs e)
        {
            if (chkRecordSchedule.Checked)
            {
                chkScheduleRecordOnDetect.Checked = false;
                chkRecordAlertSchedule.Checked = false;
            }
        }

        private void ChkScheduleRecordOnDetectCheckedChanged(object sender, EventArgs e)
        {
            if (chkScheduleRecordOnDetect.Checked)
            {
                chkRecordSchedule.Checked = false;
                chkRecordAlertSchedule.Checked = false;
            }
        }

        private void LinkLabel8LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide-pairing.aspx");
        }

        private void ChkRecordAlertCheckedChanged(object sender, EventArgs e)
        {
            if (chkRecordAlert.Checked)
                chkRecord.Checked = false;
            CameraControl.Camobject.detector.recordonalert = chkRecordAlert.Checked;
        }

        private void ChkRecordAlertScheduleCheckedChanged(object sender, EventArgs e)
        {
            if (chkRecordAlertSchedule.Checked)
            {
                chkRecordSchedule.Checked = false;
                chkScheduleRecordOnDetect.Checked = false;
                chkScheduleAlerts.Checked = true;
            }
        }

        private void ChkScheduleAlertsCheckedChanged(object sender, EventArgs e)
        {
            if (!chkScheduleAlerts.Checked)
                chkRecordAlertSchedule.Checked = false;
        }

        private void ChkMmsCheckedChanged(object sender, EventArgs e)
        {
            if (chkMMS.Checked)
                chkSendSMSMovement.Checked = false;
        }

        private void RdoFtpIntervalCheckedChanged(object sender, EventArgs e)
        {
            txtUploadEvery.Enabled = rdoFTPInterval.Checked;
        }

        private void rdoFTPAlerts_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void gbAdvanced_Enter(object sender, EventArgs e)
        {
        }

        private void Button3Click3(object sender, EventArgs e)
        {
            ConfigureSeconds cf;
            ConfigureNumberPlates cnp;
            switch (ddlAlertMode.SelectedIndex)
            {
                case 0:
                    cf = new ConfigureSeconds
                             {
                                 Seconds = CameraControl.Camobject.detector.movementinterval
                             };
                    cf.ShowDialog();
                    if (cf.DialogResult == DialogResult.OK)
                        CameraControl.Camobject.detector.movementinterval = cf.Seconds;
                    cf.Dispose();
                    break;
                case 1:
                    cf = new ConfigureSeconds
                             {
                                 Seconds = CameraControl.Camobject.detector.nomovementinterval
                             };
                    cf.ShowDialog();
                    if (cf.DialogResult == DialogResult.OK)
                        CameraControl.Camobject.detector.nomovementinterval = cf.Seconds;
                    cf.Dispose();
                    break;
                case 2:
                    break;
                case 3:
                case 4:
                    cnp = new ConfigureNumberPlates
                              {
                                  NumberPlates = CameraControl.Camobject.alerts.numberplates,
                                  Mode = CameraControl.Camobject.alerts.numberplatesmode,
                                  FrameInterval = CameraControl.Camobject.alerts.numberplatesinterval,
                                  Accuracy = CameraControl.Camobject.alerts.numberplatesaccuracy,
                                  Overlay =  CameraControl.Camobject.alerts.overlay,
                                  Framegrab = CameraControl.Camera.LastFrame,
                                  Area = CameraControl.Camobject.alerts.numberplatesarea
                              };
                    cnp.ShowDialog();
                    if (cnp.DialogResult == DialogResult.OK)
                    {
                        CameraControl.Camobject.alerts.numberplates = cnp.NumberPlates;
                        CameraControl.Camobject.alerts.numberplatesmode = cnp.Mode;
                        CameraControl.Camobject.alerts.numberplatesinterval = cnp.FrameInterval;
                        CameraControl.Camobject.alerts.numberplatesaccuracy = cnp.Accuracy;
                        CameraControl.Camobject.alerts.overlay = cnp.Overlay;
                        CameraControl.Camobject.alerts.numberplatesarea = cnp.Area;
                    }
                    cnp.Dispose();
                    break;
            }
        }

        private void DdlAlertModeSelectedIndexChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.alerts.mode = _alertmodes[ddlAlertMode.SelectedIndex];
            button3.Enabled = ddlAlertMode.SelectedIndex != 2;
        }

        private void ChkTimelapseCheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = chkTimelapse.Checked;
        }

        private void chkPublic_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void txtSensitivity_TextChanged(object sender, EventArgs e)
        {
        }

        private void LinkLabel9LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login();
        }

        #region Nested type: ListItem

        private struct ListItem
        {
            private readonly string _name;
            internal readonly string Value;

            public ListItem(string name, string value)
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

        private void chkRotate_CheckedChanged(object sender, EventArgs e)
        {
            bool changed = CameraControl.Camobject.rotate90 != chkRotate.Checked;
            CameraControl.Camobject.rotate90 = chkRotate.Checked;
            if (changed)
            {
                CameraControl.NeedSizeUpdate = true;
                if (CameraControl.Camobject.settings.active)
                {
                    chkActive.Enabled = true;
                    chkActive.Checked = false;
                    Thread.Sleep(500); //allows unmanaged code to complete shutdown
                    chkActive.Checked = true;
                    CameraControl.NeedSizeUpdate = true;
                }
            }           
        }

        private void chkPTZFlipX_CheckedChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.settings.ptzflipx = chkPTZFlipX.Checked;
        }

        private void chkPTZFlipY_CheckedChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.settings.ptzflipy = chkPTZFlipY.Checked;
        }

        private void chkPTZRotate90_CheckedChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.settings.ptzrotate90 = chkPTZRotate90.Checked;
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void txtPTZURL_TextChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.settings.ptzurlbase = txtPTZURL.Text;
        }

        private void numMaxFR_ValueChanged(object sender, EventArgs e)
        {
            CameraControl.Camobject.settings.maxframerate = (int)numMaxFR.Value;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ConfigureProcessor cp = new ConfigureProcessor(CameraControl);
            if (cp.ShowDialog()== DialogResult.OK)
            {
                if (CameraControl.Camera != null && CameraControl.Camera.MotionDetector != null)
                {
                    SetDetector();
                }
            }

        }

        private void chkTrack_CheckedChanged(object sender, EventArgs e)
        {
            pnlTrack.Enabled = chkTrack.Checked;
            if (chkTrack.Checked)
            {
                ddlMotionDetector.SelectedIndex = 0;
                ddlProcessor.SelectedIndex = 1;
                CameraControl.Camobject.settings.ptzautotrack = true;
                CameraControl.Camobject.detector.highlight = false;
                SetDetector();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ConfigFilter();
        }

        private void chkColourProcessing_CheckedChanged(object sender, EventArgs e)
        {
            if (chkColourProcessing.Checked)
            {
                if (String.IsNullOrEmpty(CameraControl.Camobject.detector.colourprocessing))
                {
                    if (!ConfigFilter())
                        chkColourProcessing.Checked = false;
                }
            }
            CameraControl.Camobject.detector.colourprocessingenabled = chkColourProcessing.Checked;
        }

        private bool ConfigFilter()
        {
            FilterForm = new HSLFilteringForm(CameraControl.Camobject.detector.colourprocessing) { ImageProcess = CameraControl.Camera==null?null: CameraControl.Camera.LastFrame };
            FilterForm.ShowDialog();
            if (FilterForm.DialogResult == DialogResult.OK)
            {
                CameraControl.Camobject.detector.colourprocessing = FilterForm.Configuration;
                if (CameraControl.Camera!=null)
                    CameraControl.Camera.FilterChanged();
                FilterForm.Dispose();
                FilterForm = null;
                chkColourProcessing.Checked = true;
                return true;
            }

            FilterForm.Dispose();
            FilterForm = null;
            return false;
        }

        private void AddCamera_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide-camera-settings.aspx");
        }

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = MainForm.Website+"/userguide-camera-settings.aspx";
            switch (tcCamera.SelectedIndex)
            {
                case 0:
                    url=MainForm.Website+"/userguide-camera-settings.aspx";
                    break;
                case 1:
                    url = MainForm.Website + "/userguide-pairing.aspx";
                    break;
                case 2:
                    url = MainForm.Website+"/userguide-motion-detection.aspx";
                    break;
                case 3:
                    url = MainForm.Website+"/userguide-alerts.aspx";
                    break;
                case 4:
                    url = MainForm.Website+"/userguide-webservices.aspx";
                    break;
                case 5:
                    url = MainForm.Website+"/userguide-recording.aspx";
                    break;
                case 6:
                    url = MainForm.Website+"/userguide-ptz.aspx";
                    break;
                case 7:
                    url = MainForm.Website+"/userguide-ftp.aspx";
                    break;
                case 8:
                    url = MainForm.Website+"/userguide-youtube.aspx";
                    break;
                case 9:
                    url = MainForm.Website+"/userguide-scheduling.aspx";
                    break;
            }
            Help.ShowHelp(this, url);
        }

        private void btnTimestamp_Click(object sender, EventArgs e)
        {
            var ct = new ConfigureTimestamp
                         {
                             TimeStampLocation = CameraControl.Camobject.settings.timestamplocation,
                             FontSize = CameraControl.Camobject.settings.timestampfontsize
                         };
            if (ct.ShowDialog(this)== DialogResult.OK)
            {
                CameraControl.Camobject.settings.timestamplocation = ct.TimeStampLocation;
                CameraControl.Camobject.settings.timestampfontsize = ct.FontSize;
                CameraControl.Camera.Drawfont = null;
            }
            ct.Dispose();
        }
    }
}