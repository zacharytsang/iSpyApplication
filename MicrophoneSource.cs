using System;
using System.Windows.Forms;
using NAudio.Wave;

namespace iSpyApplication
{
    public partial class MicrophoneSource : Form
    {
        private readonly string _noDevices = LocRm.GetString("NoAudioDevices");
        public objectsMicrophone Mic;

        public MicrophoneSource()
        {
            InitializeComponent();
            RenderResources();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            Finish();
        }

        private void Finish()
        {
            switch (tcAudioSource.SelectedIndex)
            {
                case 0:
                    if (!ddlDevice.Enabled)
                    {
                        Close();
                        return;
                    }
                    Mic.settings.sourcename = ddlDevice.SelectedItem.ToString();
                    break;
                case 1:
                    try
                    {
                        var url = new Uri(txtNetwork.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    Mic.settings.sourcename = txtNetwork.Text;
                    break;
                case 2:
                    string t = cmbVLCURL.Text.Trim();
                    if (t == String.Empty)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_Microphone_SelectSource"), LocRm.GetString("Error"));
                        return;
                    }
                    var iReconnect = (int)txtReconnect.Value;
                    if (iReconnect<30 && iReconnect!=0)
                    {
                        MessageBox.Show(LocRm.GetString("Validate_ReconnectInterval"), LocRm.GetString("Note"));
                        return;
                    }
                    Mic.settings.reconnectinterval = iReconnect;
                    Mic.settings.sourcename = t;
                    break;
            }

            MainForm.Conf.VLCURL = cmbVLCURL.Text.Trim();
            if (!MainForm.Conf.RecentVLCList.Contains(MainForm.Conf.VLCURL) &&
                MainForm.Conf.VLCURL != "")
            {
                MainForm.Conf.RecentVLCList =
                    (MainForm.Conf.RecentVLCList + "|" + MainForm.Conf.VLCURL).Trim('|');
            }

            Mic.settings.typeindex = tcAudioSource.SelectedIndex;
            Mic.settings.decompress = true; // chkDecompress.Checked;
            Mic.settings.vlcargs = txtVLCArgs.Text.Trim();
            

            int samplerate;
            if (Int32.TryParse(txtSampleRate.Text, out samplerate))
                Mic.settings.samples = samplerate;

            int bits;
            if (Int32.TryParse(txtBits.Text, out bits))
                Mic.settings.bits = bits;

            int channels;
            if (Int32.TryParse(txtChannels.Text, out channels))
                Mic.settings.channels = channels;

            // Mic.settings.username = txtUsername.Text;
            // Mic.settings.password = txtPassword.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button2Click(object sender, EventArgs e)
        {
            //cancel
            Close();
        }

        private void MicrophoneSourceLoad(object sender, EventArgs e)
        {
            pnlVLC.Enabled = VlcHelper.VlcInstalled;
            linkLabel3.Visible = lblInstallVLC.Visible = !pnlVLC.Enabled;
            cmbVLCURL.Text = MainForm.Conf.VLCURL;
            cmbVLCURL.Items.AddRange(MainForm.Conf.RecentVLCList.Split('|'));
            try
            {
                int i = 0, selind = -1;
                for (int n = 0; n < WaveIn.DeviceCount; n++)
                {
                    ddlDevice.Items.Add(WaveIn.GetCapabilities(n).ProductName);
                    if (WaveIn.GetCapabilities(n).ProductName == Mic.settings.sourcename)
                        selind = i;
                    i++;
                }
                ddlDevice.Enabled = true;
                if (selind > -1)
                    ddlDevice.SelectedIndex = selind;
                else
                {
                    if (ddlDevice.Items.Count == 0)
                    {
                        ddlDevice.Items.Add(_noDevices);
                        ddlDevice.Enabled = false;
                    }
                    else
                        ddlDevice.SelectedIndex = 0;
                }
            }
            catch (ApplicationException ex)
            {
                MainForm.LogExceptionToFile(ex);
                ddlDevice.Items.Add(_noDevices);
                ddlDevice.Enabled = false;
            }

            tcAudioSource.SelectedIndex = Mic.settings.typeindex;

            if (Mic.settings.typeindex == 0 && ddlDevice.Items.Count > 0)
            {
                tcAudioSource.SelectedIndex = 0;
            }
            if (Mic.settings.typeindex == 1)
            {
                txtNetwork.Text = Mic.settings.sourcename;
            }
            if (Mic.settings.typeindex == 2)
            {
                cmbVLCURL.Text = Mic.settings.sourcename;
            }

            txtVLCArgs.Text = Mic.settings.vlcargs;


            //chkDecompress.Checked = Mic.settings.decompress;

            txtBits.Text = Mic.settings.bits.ToString();
            txtChannels.Text = Mic.settings.channels.ToString();
            txtSampleRate.Text = Mic.settings.samples.ToString();
            txtReconnect.Value = Mic.settings.reconnectinterval;
            //txtUsername.Text = Mic.settings.username;
            //txtPassword.Text = Mic.settings.password;
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("Microphonesource");
            button1.Text = LocRm.GetString("Ok");
            button2.Text = LocRm.GetString("Cancel");
            //chkDecompress.Text = LocRM.GetString("DecompressNetworkAudiog71");
            label2.Text = LocRm.GetString("SampleRate");
            label3.Text = LocRm.GetString("BitsPerSample");
            label4.Text = LocRm.GetString("Channels");
            //label5.Text = LocRM.GetString("Username");
            //label6.Text = LocRM.GetString("Password");
            label8.Text = LocRm.GetString("Device");
            label9.Text = LocRm.GetString("Url");
            tabPage1.Text = LocRm.GetString("LocalDevice");
            tabPage3.Text = LocRm.GetString("iSpyServer");
            tabPage2.Text = LocRm.GetString("VLCPlugin");

            label18.Text = LocRm.GetString("Arguments");
            lblInstallVLC.Text = LocRm.GetString("VLCConnectInfo");
            linkLabel3.Text = LocRm.GetString("DownloadVLC");
            linkLabel1.Text = LocRm.GetString("UseiSpyServerText");

            label43.Text = LocRm.GetString("ReconnectEvery");
            label48.Text = LocRm.GetString("Seconds");

            llblHelp.Text = LocRm.GetString("help");
        }


        private void DdlDeviceSelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void TextBox1TextChanged(object sender, EventArgs e)
        {
        }

        private void LinkLabel3LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, "http://www.videolan.org/vlc/download-windows.html");
        }

        private void LinkLabel1LinkClicked1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/download_ispyserver.aspx");
        }

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = MainForm.Website+"/userguide-microphones.aspx";
            switch (tcAudioSource.SelectedIndex)
            {
                case 0:
                    url = MainForm.Website+"/userguide-microphones.aspx#1";
                    break;
                case 1:
                    url = MainForm.Website+"/userguide-microphones.aspx#3";
                    break;
                case 2:
                    url = MainForm.Website+"/userguide-microphones.aspx#2";
                    break;
            }
            Help.ShowHelp(this, url);
        }
    }
}