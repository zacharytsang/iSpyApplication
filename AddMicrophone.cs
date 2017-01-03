using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace iSpyApplication
{
    public partial class AddMicrophone : Form
    {
        public VolumeLevel VolumeLevel;

        public AddMicrophone()
        {
            InitializeComponent();
            RenderResources();
        }

        private void BtnSelectSourceClick(object sender, EventArgs e)
        {
            SelectSource();
        }

        private bool SelectSource()
        {
            bool success = false;
            var ms = new MicrophoneSource {Mic = VolumeLevel.Micobject};


            ms.ShowDialog(this);
            if (ms.DialogResult == DialogResult.OK)
            {
                chkActive.Enabled = true;
                chkActive.Checked = false;
                Application.DoEvents();
                lblAudioSource.Text = VolumeLevel.Micobject.settings.sourcename;
                if (txtMicrophoneName.Text.Trim() == "")
                    txtMicrophoneName.Text = "Mic " + MainForm.Conf.NextMicrophoneID;
                chkActive.Checked = true;
                success = true;
            }
            ms.Dispose();
            return success;
        }

        private void AddMicrophoneLoad(object sender, EventArgs e)
        {
            ddlPreset.Items.Add(new ListItem("Select", ""));
            VolumeLevel.IsEdit = true;
            //string[] options = MainForm.Conf.FFMPEG_Microphone.Split('|');
            ddlPreset.Items.Add(new ListItem("MP3",
                                             "-i \"{filename}.wav\" -ac {channels} -ar {samples} -f mp3 \"{filename}.mp3\""));
            ddlPreset.SelectedIndex = 0;

            txtPostProcess.Text = VolumeLevel.Micobject.settings.ffmpeg;
            chkDeleteSource.Checked = VolumeLevel.Micobject.settings.deletewav;
            rtbDescription.Text = VolumeLevel.Micobject.description;

            btnBack.Enabled = false;
            gpbSubscriber.Enabled = MainForm.Conf.Subscribed;
            txtMicrophoneName.Text = VolumeLevel.Micobject.name;
            tbSensitivity.Value = VolumeLevel.Micobject.detector.sensitivity;
            chkSound.Checked = VolumeLevel.Micobject.alerts.active;
            chkRecord.Checked = VolumeLevel.Micobject.detector.recordondetect;

            txtExecuteAudio.Text = VolumeLevel.Micobject.alerts.executefile;

            if (VolumeLevel.Micobject.alerts.mode == "sound")
                rdoMovement.Checked = true;
            else
                rdoNoMovement.Checked = true;

            chkSendEmailSound.Checked = VolumeLevel.Micobject.notifications.sendemail;
            chkSendSMSMovement.Checked = VolumeLevel.Micobject.notifications.sendsms;

            chkRecordAlert.Checked = VolumeLevel.Micobject.detector.recordonalert;
            txtEmailAlert.Text = VolumeLevel.Micobject.settings.emailaddress;
            txtSMSNumber.Text = VolumeLevel.Micobject.settings.smsnumber;
            chkSchedule.Checked = VolumeLevel.Micobject.schedule.active;
            chkActive.Checked = VolumeLevel.Micobject.settings.active;

            chkActive.Enabled = VolumeLevel.Micobject.settings.sourcename != "";
            if (VolumeLevel.Micobject.settings.sourcename != "")
            {
                lblAudioSource.Text = VolumeLevel.Micobject.settings.sourcename;
            }
            else
            {
                lblAudioSource.Text = LocRm.GetString("NoSource");
                chkActive.Checked = false;
            }


            string[] alertOptions = VolumeLevel.Micobject.alerts.alertoptions.Split(','); //beep,restore
            chkBeep.Checked = Convert.ToBoolean(alertOptions[0]);
            chkRestore.Checked = Convert.ToBoolean(alertOptions[1]);
            Text = LocRm.GetString("EditMicrophone");
            if (VolumeLevel.Micobject.id > -1)
                Text += " (ID: " + VolumeLevel.Micobject.id + ", DIR: " + VolumeLevel.Micobject.directory + ")";

            txtNoSound.Text = VolumeLevel.Micobject.detector.nosoundinterval.ToString();
            txtSound.Text = VolumeLevel.Micobject.detector.soundinterval.ToString();
            pnlSound.Enabled = chkSound.Checked;
            pnlSchedule.Enabled = chkSchedule.Checked;

            txtBuffer.Text = VolumeLevel.Micobject.settings.buffer.ToString();
            txtInactiveRecord.Text = VolumeLevel.Micobject.recorder.inactiverecord.ToString();
            txtMinimumInterval.Text = VolumeLevel.Micobject.alerts.minimuminterval.ToString();
            txtMaxRecordTime.Text = VolumeLevel.Micobject.recorder.maxrecordtime.ToString();

            ddlHourStart.SelectedIndex =
                ddlHourEnd.SelectedIndex = ddlMinuteStart.SelectedIndex = ddlMinuteEnd.SelectedIndex = 0;
            linkLabel5.Visible = !(MainForm.Conf.Subscribed);
            ShowSchedule(-1);
            chkEmailOnDisconnect.Checked = VolumeLevel.Micobject.settings.notifyondisconnect;
            txtArguments.Text = VolumeLevel.Micobject.alerts.arguments;

            txtAccessGroups.Text = VolumeLevel.Micobject.settings.accessgroups;
            txtDirectory.Text = VolumeLevel.Micobject.directory;

            if (VolumeLevel.Micobject.id == -1)
            {
                if (!SelectSource())
                    Close();
            }
        }

        private void RenderResources()
        {
            btnBack.Text = LocRm.GetString("Back");
            btnDelete.Text = LocRm.GetString("Delete");
            btnDetectSound.Text = LocRm.GetString("chars_3014702301470230147");
            btnFinish.Text = LocRm.GetString("Finish");
            btnNext.Text = LocRm.GetString("Next");
            btnSelectSource.Text = LocRm.GetString("chars_3014702301470230147");
            btnUpdate.Text = LocRm.GetString("Update");
            button2.Text = LocRm.GetString("Add");
            chkActive.Text = LocRm.GetString("MicrophoneActive");
            chkBeep.Text = LocRm.GetString("Beep");
            chkDeleteSource.Text = LocRm.GetString("DeleteWavFileAfterConvers");
            chkEmailOnDisconnect.Text = LocRm.GetString("SendEmailOnDisconnect");
            chkFri.Text = LocRm.GetString("Fri");
            chkMon.Text = LocRm.GetString("Mon");
            chkRecord.Text = LocRm.GetString("RecordOnSoundDetection");
            chkRecordAlert.Text = LocRm.GetString("RecordOnAlert");
            chkRecordSchedule.Text = LocRm.GetString("RecordOnScheduleStart");
            chkRestore.Text = LocRm.GetString("ShowIspyWindow");
            chkSat.Text = LocRm.GetString("Sat");
            chkSchedule.Text = LocRm.GetString("ScheduleMicrophone");
            chkScheduleActive.Text = LocRm.GetString("ScheduleActive");
            chkScheduleAlerts.Text = LocRm.GetString("AlertsEnabled");
            chkScheduleRecordOnDetect.Text = LocRm.GetString("RecordOnDetect");
            chkRecordAlertSchedule.Text = LocRm.GetString("RecordOnAlert");
            chkSendEmailSound.Text = LocRm.GetString("SendEmailOnAlert");
            chkSendSMSMovement.Text = LocRm.GetString("SendSmsOnAlert");
            chkSound.Text = LocRm.GetString("AlertsEnabled");
            chkSun.Text = LocRm.GetString("Sun");
            chkThu.Text = LocRm.GetString("Thu");
            chkTue.Text = LocRm.GetString("Tue");
            chkWed.Text = LocRm.GetString("Wed");
            gpbSubscriber.Text = LocRm.GetString("WebServiceOptions");
            groupBox2.Text = LocRm.GetString("FfmpegConversion");
            label1.Text = LocRm.GetString("Name");
            label10.Text = LocRm.GetString("chars_3801146");
            label11.Text = LocRm.GetString("SmsNumber");
            label12.Text = LocRm.GetString("MaxRecordTime");
            label13.Text = LocRm.GetString("Seconds");
            label14.Text = LocRm.GetString("Seconds");
            label15.Text = LocRm.GetString("DistinctAlertInterval");
            label16.Text = LocRm.GetString("Seconds");
            label17.Text = LocRm.GetString("Seconds");
            label19.Text = LocRm.GetString("InactivityRecord");
            label2.Text = LocRm.GetString("Source");
            label20.Text = LocRm.GetString("BufferAudio");
            label21.Text = LocRm.GetString("ExitThisToEnableAlertsAnd");
            label3.Text = LocRm.GetString("Sensitivity");
            label4.Text = LocRm.GetString("WhenSound");
            label45.Text = LocRm.GetString("EmailAddress");
            label48.Text = LocRm.GetString("Seconds");
            label49.Text = LocRm.GetString("Days");
            label5.Text = LocRm.GetString("Seconds");
            label50.Text = LocRm.GetString("ImportantMakeSureYourSche");
            label6.Text = LocRm.GetString("ExecuteFile");
            label61.Text = LocRm.GetString("Command");
            label62.Text = LocRm.GetString("ConversionFormat");
            label63.Text = LocRm.GetString("ispyUsesFfmpegToEncodeThe");
            label66.Text = LocRm.GetString("Description");
            label7.Text = LocRm.GetString("Start");
            label8.Text = LocRm.GetString("chars_3801146");
            label80.Text = LocRm.GetString("TipToCreateAScheduleOvern");
            label9.Text = LocRm.GetString("Stop");
            lblAudioSource.Text = LocRm.GetString("Audiosource");
            linkLabel1.Text = LocRm.GetString("HowToEnterYourNumber");
            linkLabel3.Text = LocRm.GetString("Info");
            linkLabel5.Text = LocRm.GetString("YouNeedAnActiveSubscripti");
            rdoMovement.Text = LocRm.GetString("IsDetectedFor");
            rdoNoMovement.Text = LocRm.GetString("IsNotDetectedFor");
            tabPage1.Text = LocRm.GetString("Microphone");
            tabPage2.Text = LocRm.GetString("Alerts");
            tabPage3.Text = LocRm.GetString("Scheduling");
            tabPage4.Text = LocRm.GetString("Recording");
            tabPage5.Text = LocRm.GetString("Webservices");
            Text = LocRm.GetString("Addmicrophone");

            toolTip1.SetToolTip(txtMicrophoneName, LocRm.GetString("ToolTip_MicrophoneName"));
            toolTip1.SetToolTip(txtMinimumInterval, LocRm.GetString("ToolTip_AlertInterval"));
            toolTip1.SetToolTip(txtInactiveRecord, LocRm.GetString("ToolTip_InactiveRecordAudio"));
            toolTip1.SetToolTip(txtBuffer, LocRm.GetString("ToolTip_BufferAudio"));
            toolTip1.SetToolTip(lbSchedule, LocRm.GetString("ToolTip_PressDelete"));
            llblHelp.Text = LocRm.GetString("help");
            label72.Text = LocRm.GetString("arguments");
            lblAccessGroups.Text = LocRm.GetString("AccessGroups");
            label74.Text = LocRm.GetString("Directory");
        }


        private void TbSensitivityScroll(object sender, EventArgs e)
        {
            VolumeLevel.Micobject.detector.sensitivity = tbSensitivity.Value;
        }

        private void BtnNextClick(object sender, EventArgs e)
        {
            GoNext();
        }

        private void GoNext()
        {
            tcMicrophone.SelectedIndex++;
        }

        private void GoPrevious()
        {
            tcMicrophone.SelectedIndex--;
        }

        private bool CheckStep1()
        {
            string err = "";
            string name = txtMicrophoneName.Text.Trim();
            if (name == "")
                err += LocRm.GetString("Validate_Microphone_EnterName") + Environment.NewLine;
            if (
                MainForm.Microphones.SingleOrDefault(
                    p => p.name.ToLower() == name.ToLower() && p.id != VolumeLevel.Micobject.id) != null)
                err += LocRm.GetString("Validate_Microphone_NameInUse") + Environment.NewLine;


            if (VolumeLevel.Micobject.settings.sourcename == "")
            {
                err += LocRm.GetString("Validate_Microphone_SelectSource"); //"";
            }
            if (err != "")
            {
                MessageBox.Show(err, LocRm.GetString("Error"));
                return false;
            }
            return true;
        }

        private void BtnFinishClick(object sender, EventArgs e)
        {
            //validate page 0
            if (CheckStep1())
            {
                string err = "";
                if (chkSendSMSMovement.Checked && MainForm.Conf.ServicesEnabled &&
                    txtSMSNumber.Text.Trim() == "")
                    err += LocRm.GetString("Validate_Camera_MobileNumber") + Environment.NewLine;

                int nosoundinterval;
                if (!int.TryParse(txtNoSound.Text, out nosoundinterval))
                    err += LocRm.GetString("Validate_Microphone_NoSound") + Environment.NewLine;
                int soundinterval;
                if (!int.TryParse(txtSound.Text, out soundinterval))
                    err += LocRm.GetString("Validate_Microphone_Sound") + Environment.NewLine;


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
                    chkSendEmailSound.Checked = false;
                    chkEmailOnDisconnect.Checked = false;
                }
                if (sms == "")
                {
                    chkSendSMSMovement.Checked = false;
                }

                if (txtBuffer.Text.Length < 1 || txtInactiveRecord.Text.Length < 1 || txtMinimumInterval.Text.Length < 1 ||
                    txtMaxRecordTime.Text.Length < 1)
                {
                    err += LocRm.GetString("Validate_Camera_RecordingSettings") + Environment.NewLine;
                }

                if (err != "")
                {
                    MessageBox.Show(err, LocRm.GetString("Error"));
                    return;
                }


                VolumeLevel.Micobject.settings.buffer = Convert.ToInt32(txtBuffer.Value);
                VolumeLevel.Micobject.recorder.inactiverecord = Convert.ToInt32(txtInactiveRecord.Value);
                VolumeLevel.Micobject.alerts.minimuminterval = Convert.ToInt32(txtMinimumInterval.Value);
                VolumeLevel.Micobject.recorder.maxrecordtime = Convert.ToInt32(txtMaxRecordTime.Value);

                VolumeLevel.Micobject.description = rtbDescription.Text;
                VolumeLevel.Micobject.name = txtMicrophoneName.Text.Trim();
                VolumeLevel.Micobject.detector.sensitivity = tbSensitivity.Value;
                VolumeLevel.Micobject.alerts.active = chkSound.Checked;
                VolumeLevel.Micobject.alerts.executefile = txtExecuteAudio.Text;
                VolumeLevel.Micobject.alerts.alertoptions = chkBeep.Checked + "," + chkRestore.Checked;
                VolumeLevel.Micobject.alerts.mode = "sound";
                if (rdoNoMovement.Checked)
                    VolumeLevel.Micobject.alerts.mode = "nosound";
                VolumeLevel.Micobject.detector.nosoundinterval = nosoundinterval;
                VolumeLevel.Micobject.detector.soundinterval = soundinterval;
                VolumeLevel.Micobject.notifications.sendemail = chkSendEmailSound.Checked;
                VolumeLevel.Micobject.notifications.sendsms = chkSendSMSMovement.Checked;
                VolumeLevel.Micobject.settings.smsnumber = sms;
                VolumeLevel.Micobject.settings.emailaddress = email;
                VolumeLevel.Micobject.settings.ffmpeg = txtPostProcess.Text.Replace(Environment.NewLine, " ");
                VolumeLevel.Micobject.settings.deletewav = chkDeleteSource.Checked;

                VolumeLevel.Micobject.schedule.active = chkSchedule.Checked;
                VolumeLevel.Micobject.width = VolumeLevel.Width;
                VolumeLevel.Micobject.height = VolumeLevel.Height;

                VolumeLevel.Micobject.settings.active = chkActive.Checked;
                VolumeLevel.Micobject.detector.recordondetect = chkRecord.Checked;
                VolumeLevel.Micobject.settings.notifyondisconnect = chkEmailOnDisconnect.Checked;

                VolumeLevel.Micobject.alerts.arguments = txtArguments.Text;

                VolumeLevel.Micobject.settings.accessgroups = txtAccessGroups.Text;

                if (txtDirectory.Text.Trim() == "")
                    txtDirectory.Text = MainForm.RandomString(5);

                if (VolumeLevel.Micobject.directory != txtDirectory.Text)
                {
                    string path = MainForm.Conf.MediaDirectory + "audio\\" + txtDirectory.Text + "\\";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                VolumeLevel.Micobject.directory = txtDirectory.Text;


                DialogResult = DialogResult.OK;
                MainForm.NeedsSync = true;
                Close();
            }
        }

        public bool IsNumeric(string numberString)
        {
            return numberString.All(c => char.IsNumber(c));
        }

        private void ChkSoundCheckedChanged(object sender, EventArgs e)
        {
            pnlSound.Enabled = chkSound.Checked;
            VolumeLevel.Micobject.alerts.active = chkSound.Checked;
        }

        private void BtnDetectMovementClick(object sender, EventArgs e)
        {
            ofdDetect.FileName = "";
            ofdDetect.InitialDirectory = Program.AppPath + @"sounds\";
            ofdDetect.ShowDialog();
            if (ofdDetect.FileName != "")
            {
                txtExecuteAudio.Text = ofdDetect.FileName;
            }
        }

        private void ChkScheduleCheckedChanged(object sender, EventArgs e)
        {
            pnlSchedule.Enabled = chkSchedule.Checked;
            btnDelete.Enabled = btnUpdate.Enabled = lbSchedule.SelectedIndex > -1;
            lbSchedule.Refresh();
        }

        private void TxtMicrophoneNameTextChanged(object sender, EventArgs e)
        {
            VolumeLevel.Micobject.name = txtMicrophoneName.Text;
        }

        private void ChkSendSmsMovementCheckedChanged(object sender, EventArgs e)
        {
        }

        private void ChkSendEmailSoundCheckedChanged(object sender, EventArgs e)
        {
        }

        private void AddMicrophoneFormClosing(object sender, FormClosingEventArgs e)
        {
            VolumeLevel.IsEdit = false;
        }

        private void ChkActiveCheckedChanged(object sender, EventArgs e)
        {
            if (chkActive.Checked != VolumeLevel.Micobject.settings.active)
            {
                if (chkActive.Checked)
                    VolumeLevel.Enable();
                else
                    VolumeLevel.Disable();
            }
        }

        private void RdoMovementCheckedChanged(object sender, EventArgs e)
        {
            if (VolumeLevel.Micobject.alerts.mode != "sound" && rdoMovement.Checked)
            {
                VolumeLevel.Micobject.alerts.mode = "sound";
            }
        }

        private void RdoNoMovementCheckedChanged(object sender, EventArgs e)
        {
            if (VolumeLevel.Micobject.alerts.mode != "nosound" && rdoNoMovement.Checked)
            {
                VolumeLevel.Micobject.alerts.mode = "nosound";
            }
        }

        private void Button1Click(object sender, EventArgs e)
        {
            GoPrevious();
        }


        private void TcMicrophoneSelectedIndexChanged(object sender, EventArgs e)
        {
            btnBack.Enabled = tcMicrophone.SelectedIndex != 0;

            btnNext.Enabled = tcMicrophone.SelectedIndex != tcMicrophone.TabCount - 1;
        }

        private void Button2Click(object sender, EventArgs e)
        {
            List<objectsMicrophoneScheduleEntry> scheds = VolumeLevel.Micobject.schedule.entries.ToList();
            var sched = new objectsMicrophoneScheduleEntry();
            if (ConfigureSchedule(sched))
            {
                scheds.Add(sched);
                VolumeLevel.Micobject.schedule.entries = scheds.ToArray();
                ShowSchedule(VolumeLevel.Micobject.schedule.entries.Count() - 1);
            }
        }

        private bool ConfigureSchedule(objectsMicrophoneScheduleEntry sched)
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
                MessageBox.Show(LocRm.GetString("Validate_Camera_SelectOneDay")); //"Please select at least one day");
                return false;
            }

            sched.recordonstart = chkRecordSchedule.Checked;
            sched.active = chkScheduleActive.Checked;
            sched.recordondetect = chkScheduleRecordOnDetect.Checked;
            sched.alerts = chkScheduleAlerts.Checked;
            return true;
        }

        private void LbScheduleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSchedule();
            }
        }

        private void ShowSchedule(int selectedIndex)
        {
            lbSchedule.Items.Clear();
            int i = 0;
            foreach (string sched in VolumeLevel.ScheduleDetails)
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

        private void LinkLabel3LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MainForm.StartBrowser("http://www.ffmpeg.org/ffmpeg-doc.html");
        }

        private void DdlPresetSelectedIndexChanged(object sender, EventArgs e)
        {
            string val = ((ListItem) ddlPreset.SelectedItem).Value;
            if (val != "")
            {
                txtPostProcess.Text = val;
            }
        }

        private void LinkLabel5LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login();
        }

        private void Login()
        {
            ((MainForm) Owner).Connect(MainForm.Website+"/subscribe.aspx");
            gpbSubscriber.Enabled = MainForm.Conf.Subscribed;
        }

        private void ChkRecordCheckedChanged(object sender, EventArgs e)
        {
            if (chkRecord.Checked)
                chkRecordAlert.Checked = false;
            VolumeLevel.Micobject.detector.recordondetect = chkRecord.Checked;
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            DeleteSchedule();
        }

        private void DeleteSchedule()
        {
            if (lbSchedule.SelectedIndex > -1)
            {
                int i = lbSchedule.SelectedIndex;
                List<objectsMicrophoneScheduleEntry> scheds = VolumeLevel.Micobject.schedule.entries.ToList();
                scheds.RemoveAt(i);
                VolumeLevel.Micobject.schedule.entries = scheds.ToArray();
                int j = i - 1;
                if (j < 0)
                    j = 0;
                ShowSchedule(j);
                if (lbSchedule.Items.Count == 0)
                    btnDelete.Enabled = btnUpdate.Enabled = false;
                else
                    btnDelete.Enabled = btnUpdate.Enabled = (lbSchedule.SelectedIndex > -1);
            }
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
                    objectsMicrophoneScheduleEntry sched = VolumeLevel.Micobject.schedule.entries[i];

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
                }
            }
        }

        private void LinkLabel1LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(null, MainForm.Website+"/countrycodes.aspx");
        }

        private void BtnUpdateClick(object sender, EventArgs e)
        {
            int i = lbSchedule.SelectedIndex;
            objectsMicrophoneScheduleEntry sched = VolumeLevel.Micobject.schedule.entries[i];

            if (ConfigureSchedule(sched))
            {
                ShowSchedule(i);
            }
        }

        private void LbScheduleDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            int i = e.Index;
            objectsMicrophoneScheduleEntry sched = VolumeLevel.Micobject.schedule.entries[i];

            Font f = sched.active ? new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold) : new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
            Brush b = !chkSchedule.Checked ? Brushes.Gray : Brushes.Black;

            e.Graphics.DrawString(lbSchedule.Items[e.Index].ToString(), f, b, e.Bounds);
            e.DrawFocusRectangle();
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

        private void ChkRecordAlertCheckedChanged(object sender, EventArgs e)
        {
            if (chkRecordAlert.Checked)
                chkRecord.Checked = false;
            VolumeLevel.Micobject.detector.recordonalert = chkRecordAlert.Checked;
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

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = MainForm.Website+"/userguide-microphone-settings.aspx";
            switch (tcMicrophone.SelectedIndex)
            {
                case 0:
                    url = MainForm.Website+"/userguide-microphone-settings.aspx#1";
                    break;
                case 1:
                    url = MainForm.Website+"/userguide-microphone-alerts.aspx";
                    break;
                case 2:
                    url = MainForm.Website+"/userguide-microphone-webservices.aspx";
                    break;
                case 3:
                    url = MainForm.Website+"/userguide-microphone-recording.aspx#2";
                    break;
                case 4:
                    url = MainForm.Website+"/userguide-scheduling.aspx#6";
                    break;
            }
            Help.ShowHelp(this, url);
        }
    }
}