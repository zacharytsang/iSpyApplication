using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using AForge.Video.VFW;
using Microsoft.Win32;

namespace iSpyApplication
{
    public partial class Settings : Form
    {
        private const int Rgbmax = 255;
        public int InitialTab;
        public bool ReloadResources;
        private RegistryKey _rkApp;
        private bool _youtubeaccountChanged;

        public Settings()
        {
            InitializeComponent();
            RenderResources();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            string password = txtPassword.Text;
            if (chkPasswordProtect.Checked)
            {
                if (password.Length < 3)
                {
                    MessageBox.Show(LocRm.GetString("Validate_Password"), LocRm.GetString("Note"));
                    return;
                }
            }
            string err = "";

            if (!Directory.Exists(txtMediaDirectory.Text))
            {
                err += LocRm.GetString("Validate_MediaDirectory") + "\n";
            }

            if (err != "")
            {
                MessageBox.Show(err, LocRm.GetString("Error"));
                return;
            }

            MainForm.Conf.Enable_Error_Reporting = chkErrorReporting.Checked;
            MainForm.Conf.Enable_Update_Check = chkCheckForUpdates.Checked;
            MainForm.Conf.Enable_Password_Protect = chkPasswordProtect.Checked;
            MainForm.Conf.Password_Protect_Password = password;

            string dir = txtMediaDirectory.Text.Trim();
            if (!dir.EndsWith("\\"))
                dir += "\\";


            if (MainForm.Conf.MediaDirectory != dir)
            {
                MainForm.Conf.MediaDirectory = dir;
                Directory.CreateDirectory(dir + "audio");
                Directory.CreateDirectory(dir + "video");
                foreach (objectsCamera cam in MainForm.Cameras)
                {
                    Directory.CreateDirectory(dir + "video\\" + cam.directory);
                    Directory.CreateDirectory(dir + "video\\" + cam.directory+"\\thumbs");
                }
                foreach (objectsMicrophone mic in MainForm.Microphones)
                {
                    Directory.CreateDirectory(dir + "audio\\" + mic.directory);
                }
            }

            MainForm.Conf.NoActivityColor = btnNoDetectColor.BackColor.ToRGBString();
            MainForm.Conf.TimestampColor = btnTimestampColor.BackColor.ToRGBString();
            MainForm.Conf.ActivityColor = btnDetectColor.BackColor.ToRGBString();
            MainForm.Conf.TrackingColor = btnColorTracking.BackColor.ToRGBString();
            MainForm.Conf.VolumeLevelColor = btnColorVolume.BackColor.ToRGBString();
            MainForm.Conf.MainColor = btnColorMain.BackColor.ToRGBString();
            MainForm.Conf.AreaColor = btnColorArea.BackColor.ToRGBString();
            MainForm.Conf.BackColor = btnColorBack.BackColor.ToRGBString();
            MainForm.Conf.Enabled_ShowGettingStarted = chkShowGettingStarted.Checked;
            MainForm.Conf.MaxMediaFolderSizeMB = Convert.ToInt32(txtMaxMediaSize.Value);
            MainForm.Conf.DeleteFilesOlderThanDays = Convert.ToInt32(txtDaysDelete.Value);
            MainForm.Conf.Opacity = tbOpacity.Value;
            MainForm.Conf.Enable_Storage_Management = chkStorage.Checked;
            MainForm.Conf.YouTubeAccount = txtYouTubeAccount.Text;
            MainForm.Conf.YouTubePassword = txtYouTubePassword.Text;
            MainForm.Conf.YouTubeUsername = txtYouTubeUsername.Text;
            MainForm.Conf.LogFFMPEGCommands = chkLogFFMPEG.Checked;
            MainForm.Conf.BalloonTips = chkBalloon.Checked;
            MainForm.Conf.TrayIconText = txtTrayIcon.Text;
            MainForm.Conf.FFMPEG_SingleProcess = chkFFMPEGSingle.Checked;
            MainForm.Conf.IPCameraTimeout = Convert.ToInt32(txtIPCameraTimeout.Value);
            MainForm.Conf.ServerReceiveTimeout = Convert.ToInt32(txtServerReceiveTimeout.Value);
            MainForm.Conf.ServerName = txtServerName.Text;
            MainForm.Conf.AutoSchedule = chkAutoSchedule.Checked;
            MainForm.Conf.CPUMax = Convert.ToInt32(numMaxCPU.Value);
            MainForm.Conf.MaxRecordingThreads = (int)numMaxRecordingThreads.Value;

            var ips = rtbAccessList.Text.Trim().Split(',');
            var t = ips.Select(ip => ip.Trim()).Where(ip2 => ip2 != "").Aggregate("", (current, ip2) => current + (ip2 + ","));
            MainForm.Conf.AllowedIPList = t.Trim(',');
            LocalServer.AllowedIPs = MainForm.Conf.AllowedIPList.Split(',').ToList();
            LocalServer.AllowedIPs.RemoveAll(p => p == "");


            string lang = ((ListItem) ddlLanguage.SelectedItem).Value[0];
            if (lang != MainForm.Conf.Language)
            {
                ReloadResources = true;
                LocRm.CurrentSet = null;
            }
            MainForm.Conf.Language = lang;

            if (chkStartup.Checked)
            {
                try
                {
                    _rkApp = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    if (_rkApp != null)
                        _rkApp.SetValue("iSpy", "\"" + Application.ExecutablePath + "\" -silent", RegistryValueKind.String);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MainForm.LogExceptionToFile(ex);
                }
            }
            else
            {
                try
                {
                    _rkApp = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    if (_rkApp != null) _rkApp.DeleteValue("iSpy", false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MainForm.LogExceptionToFile(ex);
                }
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button2Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SettingsLoad(object sender, EventArgs e)
        {
            tcTabs.SelectedIndex = InitialTab;
            //lblBackground.Text = MainForm.Conf.BackgroundImage;
            chkErrorReporting.Checked = MainForm.Conf.Enable_Error_Reporting;
            chkCheckForUpdates.Checked = MainForm.Conf.Enable_Update_Check;
            chkPasswordProtect.Checked = MainForm.Conf.Enable_Password_Protect;
            chkShowGettingStarted.Checked = MainForm.Conf.Enabled_ShowGettingStarted;

            if (MainForm.Conf.Password_Protect_Password != "")
            {
                txtPassword.Text = MainForm.Conf.Password_Protect_Password;
            }
            _rkApp = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
            chkStartup.Checked = (_rkApp != null && _rkApp.GetValue("iSpy") != null);
            txtMediaDirectory.Text = MainForm.Conf.MediaDirectory;

            btnDetectColor.BackColor = MainForm.Conf.ActivityColor.ToColor();
            btnNoDetectColor.BackColor = MainForm.Conf.NoActivityColor.ToColor();
            btnColorTracking.BackColor = MainForm.Conf.TrackingColor.ToColor();
            btnColorVolume.BackColor = MainForm.Conf.VolumeLevelColor.ToColor();
            btnColorMain.BackColor = MainForm.Conf.MainColor.ToColor();
            btnColorArea.BackColor = MainForm.Conf.AreaColor.ToColor();
            btnColorBack.BackColor = MainForm.Conf.BackColor.ToColor();
            btnTimestampColor.BackColor = MainForm.Conf.TimestampColor.ToColor();
            txtDaysDelete.Text = MainForm.Conf.DeleteFilesOlderThanDays.ToString();
            txtMaxMediaSize.Value = MainForm.Conf.MaxMediaFolderSizeMB;
            chkAutoSchedule.Checked = MainForm.Conf.AutoSchedule;
            numMaxCPU.Value = MainForm.Conf.CPUMax;
            numMaxRecordingThreads.Value = MainForm.Conf.MaxRecordingThreads;

            txtTrayIcon.Text = MainForm.Conf.TrayIconText;

            //ddlImageMode.SelectedItem = MainForm.Conf.BackgroundImageMode;

            tbOpacity.Value = MainForm.Conf.Opacity;
            SetColors();

            gbStorage.Enabled = chkStorage.Checked = MainForm.Conf.Enable_Storage_Management;

            txtYouTubeAccount.Text = MainForm.Conf.YouTubeAccount;
            txtYouTubePassword.Text = MainForm.Conf.YouTubePassword;
            txtYouTubeUsername.Text = MainForm.Conf.YouTubeUsername;
            chkBalloon.Checked = MainForm.Conf.BalloonTips;

            lblCodec.Text = LocRm.GetString("RecordingCodec") + ": " +
                            MainForm.Conf.CompressorOptions.Split(',')[4];

            chkLogFFMPEG.Checked = MainForm.Conf.LogFFMPEGCommands;
            chkFFMPEGSingle.Checked = MainForm.Conf.FFMPEG_SingleProcess;

            txtIPCameraTimeout.Value = MainForm.Conf.IPCameraTimeout;
            txtServerReceiveTimeout.Value = MainForm.Conf.ServerReceiveTimeout;
            txtServerName.Text = MainForm.Conf.ServerName;
            rtbAccessList.Text = MainForm.Conf.AllowedIPList;

            int i = 0, selind = 0;
            foreach (TranslationsTranslationSet set in LocRm.TranslationSets.OrderBy(p => p.Name))
            {
                ddlLanguage.Items.Add(new ListItem(set.Name, new[] {set.CultureCode}));
                if (set.CultureCode == MainForm.Conf.Language)
                    selind = i;
                i++;
            }
            ddlLanguage.SelectedIndex = selind;
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("settings");
            btnBrowseVideo.Text = LocRm.GetString("chars_3014702301470230147");
            btnColorArea.Text = LocRm.GetString("AreaHighlight");
            btnColorBack.Text = LocRm.GetString("ObjectBack");
            btnColorMain.Text = LocRm.GetString("MainPanel");
            btnColorTracking.Text = LocRm.GetString("Tracking");
            btnColorVolume.Text = LocRm.GetString("Level");
            btnDetectColor.Text = LocRm.GetString("Activity");
            btnNoDetectColor.Text = LocRm.GetString("NoActivity");
            btnTimestampColor.Text = LocRm.GetString("TimeStamp");
            button1.Text = LocRm.GetString("Ok");
            button2.Text = LocRm.GetString("Cancel");
            button3.Text = LocRm.GetString("Change");
            chkBalloon.Text = LocRm.GetString("ShowBalloonTips");
            chkCheckForUpdates.Text = LocRm.GetString("AutomaticallyCheckForUpda");
            chkErrorReporting.Text = LocRm.GetString("AnonymousErrorReporting");
            chkFFMPEGSingle.Text = LocRm.GetString("RunFfmpegInSingleProcess");
            chkLogFFMPEG.Text = LocRm.GetString("LogFfmpegCommands");
            chkPasswordProtect.Text = LocRm.GetString("PasswordProtectWhenMinimi");
            chkShowGettingStarted.Text = LocRm.GetString("ShowGettingStarted");
            chkStartup.Text = LocRm.GetString("RunOnStartupthisUserOnly");
            chkStorage.Text = LocRm.GetString("EnableStorageManagementwa");
            chkAutoSchedule.Text = LocRm.GetString("AutoApplySchedule");
            gbStorage.Text = LocRm.GetString("StorageManagement");
            label1.Text = LocRm.GetString("Password");
            label10.Text = LocRm.GetString("MaxMediaFolderSize");
            label11.Text = LocRm.GetString("WhenOver70FullDeleteFiles");
            label12.Text = LocRm.GetString("DaysOld0ForNoDeletions");
            label13.Text = LocRm.GetString("AccountName");
            label14.Text = LocRm.GetString("IspyServerName");
            label16.Text = LocRm.GetString("ispyOpacitymayNotW");
            label19.Text = LocRm.GetString("ispyCanAutomaticallyUploa");
            label2.Text = LocRm.GetString("ServerReceiveTimeout");
            label20.Text = LocRm.GetString("additionalControlsForYout");
            label21.Text = LocRm.GetString("TrayIconText");
            label3.Text = LocRm.GetString("MediaDirectory");
            label4.Text = LocRm.GetString("ms");
            label5.Text = LocRm.GetString("YoutubeUsername");
            label6.Text = LocRm.GetString("YoutubePassword");
            label7.Text = LocRm.GetString("ms");
            label8.Text = LocRm.GetString("MjpegReceiveTimeout");
            label9.Text = LocRm.GetString("Mb");
            label18.Text = LocRm.GetString("MaxRecordingThreads");
            lblCodec.Text = LocRm.GetString("RecordingCodec");
            linkLabel4.Text = LocRm.GetString("InstallWindowsMediaVideo9");
            tabPage1.Text = LocRm.GetString("Colors");
            tabPage2.Text = LocRm.GetString("Storage");
            tabPage3.Text = LocRm.GetString("Youtube");
            tabPage4.Text = LocRm.GetString("Timeouts");
            tabPage5.Text = LocRm.GetString("Codec");
            tabPage6.Text = LocRm.GetString("options");
            groupBox1.Text = LocRm.GetString("Language");
            linkLabel1.Text = LocRm.GetString("GetLatestList");
            Text = LocRm.GetString("settings");
            linkLabel2.Text = LocRm.GetString("HelpTranslateISpy");
            

            llblHelp.Text = LocRm.GetString("help");
        }


        private void SetColors()
        {
            btnDetectColor.ForeColor = InverseColor(btnDetectColor.BackColor);
            btnNoDetectColor.ForeColor = InverseColor(btnNoDetectColor.BackColor);
            btnColorTracking.ForeColor = InverseColor(btnColorTracking.BackColor);
            btnColorVolume.ForeColor = InverseColor(btnColorVolume.BackColor);
            btnColorMain.ForeColor = InverseColor(btnColorMain.BackColor);
            btnColorArea.ForeColor = InverseColor(btnColorArea.BackColor);
            btnColorBack.ForeColor = InverseColor(btnColorBack.BackColor);
            btnTimestampColor.ForeColor = InverseColor(btnTimestampColor.BackColor);
        }

        private static Color InverseColor(Color colorIn)
        {
            return Color.FromArgb(Rgbmax - colorIn.R,
                                  Rgbmax - colorIn.G, Rgbmax - colorIn.B);
        }

        private void chkStartup_CheckedChanged(object sender, EventArgs e)
        {
        }


        private void BtnBrowseVideoClick(object sender, EventArgs e)
        {
            if (Directory.Exists(txtMediaDirectory.Text))
                fbdSaveLocation.SelectedPath = txtMediaDirectory.Text;
            fbdSaveLocation.ShowDialog();
            if (fbdSaveLocation.SelectedPath != "")
            {
                bool success = false;
                try
                {
                    string path = fbdSaveLocation.SelectedPath;
                    if (!path.EndsWith("\\"))
                        path += "\\";
                    Directory.CreateDirectory(path + "video");
                    success = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                if (success)
                {
                    txtMediaDirectory.Text = fbdSaveLocation.SelectedPath;
                    if (!txtMediaDirectory.Text.EndsWith(@"\"))
                        txtMediaDirectory.Text += @"\";
                }
            }
        }

        private void Button3Click(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnNoDetectColor.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnNoDetectColor.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void BtnDetectColorClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnDetectColor.BackColor;

            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnDetectColor.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void BtnColorTrackingClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnColorTracking.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnColorTracking.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void BtnColorVolumeClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnColorVolume.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnColorVolume.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void BtnColorMainClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnColorMain.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnColorMain.BackColor = cdColorChooser.Color;
                MainForm.Conf.MainColor = btnColorMain.BackColor.ToString();
                SetColors();
                ((MainForm) Owner).SetBackground();
            }
        }

        private void BtnColorBackClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnColorBack.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnColorBack.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void BtnColorAreaClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnColorArea.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnColorArea.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void chkPasswordProtect_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void TbOpacityScroll(object sender, EventArgs e)
        {
            (Owner).Opacity = Convert.ToDouble(tbOpacity.Value)/100;
        }


        private void ChkStorageCheckedChanged(object sender, EventArgs e)
        {
            gbStorage.Enabled = chkStorage.Checked;
        }

        private void BtnTimestampColorClick(object sender, EventArgs e)
        {
            cdColorChooser.Color = btnColorBack.BackColor;
            if (cdColorChooser.ShowDialog() == DialogResult.OK)
            {
                btnTimestampColor.BackColor = cdColorChooser.Color;
                SetColors();
            }
        }

        private void TxtYouTubeUsernameKeyUp(object sender, KeyEventArgs e)
        {
            UpdateAccount();
        }

        private void UpdateAccount()
        {
            // only autoupdate this if it has not changed yet
            if (_youtubeaccountChanged == false)
            {
                string value = txtYouTubeUsername.Text;
                string firstpart = value.Split('@')[0];
                txtYouTubeAccount.Text = firstpart;
            }
        }

        private void TxtYouTubeAccountKeyPress(object sender, KeyPressEventArgs e)
        {
            _youtubeaccountChanged = true;
        }

        private void Button3Click1(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(MainForm.Conf.MediaDirectory + "test.avi"))
                    File.Delete(MainForm.Conf.MediaDirectory + "test.avi");
            }
            catch
            {
            }


            string co = MainForm.Conf.CompressorOptions;
            string[] compressorOptions = co.Split(',');
            bool error = true;
            bool cancel = false;
            var writer = new AVIWriter();
            while (error && !cancel)
            {
                try
                {
                    writer.Open(MainForm.Conf.MediaDirectory + "test.avi", 2, 2, true, ref compressorOptions);
                    writer.AddFrame(new Bitmap(2, 2));
                    writer.Close();
                    MainForm.Conf.CompressorOptions = String.Join(",", compressorOptions);
                    error = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LocRm.GetString("ErrorCodec") + ": " + ex.Message);
                    error = true;
                    if (
                        MessageBox.Show(LocRm.GetString("TryCodec"), LocRm.GetString("Confirm"), MessageBoxButtons.YesNo) ==
                        DialogResult.No)
                        cancel = true;
                }
                writer.Close();
                writer = null;
                try
                {
                    File.Delete(MainForm.Conf.MediaDirectory + "test.avi");
                }
                catch
                {
                }
                if (!cancel && error)
                {
                    compressorOptions = MainForm.Conf.CompressorOptions.Split(',');
                    writer = new AVIWriter();
                }
            }
            if (cancel)
                MainForm.Conf.CompressorOptions = co;

            lblCodec.Text = LocRm.GetString("RecordingCodec") + ": " +
                            MainForm.Conf.CompressorOptions.Split(',')[4];
        }

        private void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process p = Process.Start(Program.AppPath + "wmv9VCMsetup.exe");
            if (p != null) p.WaitForExit();
        }

        private void chkErrorReporting_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void chkShowGettingStarted_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void ddlLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void LinkLabel1LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool success = false;
            try
            {
                var rdr = new XmlTextReader(MainForm.Website+"/admin/translations.xml");
                var doc = new XmlDocument();
                doc.Load(rdr);
                rdr.Close();
                doc.Save(Program.AppDataPath + @"XML\Translations.xml");
                LocRm.TranslationsList = null;
                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LocRm.GetString("Error"));
            }
            if (success)
            {
                ddlLanguage.Items.Clear();
                RenderResources();
                int i = 0, selind = 0;
                foreach (TranslationsTranslationSet set in LocRm.TranslationSets.OrderBy(p=>p.Name))
                {
                    ddlLanguage.Items.Add(new ListItem(set.Name, new[] {set.CultureCode}));
                    if (set.CultureCode == MainForm.Conf.Language)
                        selind = i;
                    i++;
                }
                ddlLanguage.SelectedIndex = selind;
                MessageBox.Show(LocRm.GetString("ResourcesUpdated"));
                ReloadResources = true;
            }
        }

        private void LinkLabel2LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/yaf/forum.aspx?g=posts&m=678&#post678#post678");
        }

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

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide-settings.aspx");
        }
    }
}