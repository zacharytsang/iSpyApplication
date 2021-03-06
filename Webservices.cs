using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Moah;

namespace iSpyApplication
{
    public partial class Webservices : Form
    {
        public static string NL = Environment.NewLine;
        public string EmailAddress = "";
        public string MobileNumber = "";
        public bool SupportsUpnp;


        public Webservices()
        {
            InitializeComponent();
            RenderResources();
        }

        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MainForm.StartBrowser(MainForm.Website+"/newuser.aspx");
        }

        private bool SetupNetwork(out int port, out int localport, out string error)
        {
            port = Convert.ToInt32(ddlPort.Text);
            localport = (int)txtLANPort.Value;
            if (tcIPMode.SelectedIndex==1)
            {
                localport = (int) txtPort.Value;
            }

            MainForm.WSW = new WsWrapper();
            MainForm.Conf.ServerPort = port;
            MainForm.Conf.LANPort = localport;
            MainForm.WSW.Wsa.Url = MainForm.Website+"/webservices/ispy.asmx";
            MainForm.MWS.NumErr = 0;

            switch (tcIPMode.SelectedIndex)
            {
                case 0:
                    MainForm.Conf.IPMode = "IPv4";
                    MainForm.Conf.IPv4Address = lbIPv4Address.SelectedItem.ToString();
                    MainForm.AddressIPv4 = MainForm.Conf.IPv4Address;
                    break;
                case 1:
                    MainForm.Conf.IPMode = "IPv6";
                    MainForm.Conf.IPv6Address = lbIPv6Address.SelectedItem.ToString();
                    MainForm.AddressIPv6 = MainForm.Conf.IPv6Address;
                    break;
            }
            MainForm.Conf.WSUsername = txtUsername.Text.Trim();
            MainForm.Conf.WSPassword = txtPassword.Text.Trim();

            error = MainForm.StopAndStartServer();
            Application.DoEvents();
            return error=="";
        }

        private void Button1Click(object sender, EventArgs e)
        {
            bool bIPv6 = tcIPMode.SelectedIndex == 1;
            int port, localPort;
            string error = "";
            
            if (!SetupNetwork(out port, out localPort, out error))
            {
                MessageBox.Show(error+" - Try a different port.");
                return;
            }

            if (txtUsername.Text.Trim() != "")
            {
                try
                {
                    var fw = new WinXPSP2FireWall();
                    fw.Initialize();

                    bool bOn = false;
                    fw.IsWindowsFirewallOn(ref bOn);
                    if (bOn)
                    {
                        string strApplication = Application.StartupPath + "\\iSpy.exe";
                        bool bEnabled = false;
                        fw.IsAppEnabled(strApplication, ref bEnabled);
                        if (!bEnabled)
                        {
                            fw.AddApplication(strApplication, "iSpy");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MainForm.LogExceptionToFile(ex);
                }

                Next.Enabled = false;
                Next.Text = "...";
                Application.DoEvents();
                
                MainForm.Conf.DHCPReroute = chkReroute.Checked;
                bool failed = false;


                var result = MainForm.WSW.TestConnection(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, true);
                if (result[0] != "OK")
                {
                    MessageBox.Show(result[0], LocRm.GetString("Error"));
                    failed = true;
                }

                if (!failed)
                {
                    if (result[0] == "OK")
                    {
                        EmailAddress = result[2];
                        MobileNumber = result[4];

                        MainForm.Conf.ServicesEnabled = true;
                        MainForm.Conf.Subscribed = Convert.ToBoolean(result[1]);
                        if (result[3] != "")
                        {
                            if (!bIPv6)
                            {
                                //try setting port automatically
                                if (chkuPNP.Checked)
                                {
                                    if (!NATControl.SetPorts(port, localPort))
                                    {
                                        MessageBox.Show(LocRm.GetString("ErrorPortMapping"), LocRm.GetString("Error"));
                                        chkuPNP.Checked = false;
                                    }
                                }
                            }
                            result = MainForm.WSW.TestConnection(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, true);
                            
                            if (result[3] != "")
                            {
                                MainForm.Conf.Loopback = false;
                                Next.Enabled = true;
                                Next.Text = LocRm.GetString("Finish");
                                MainForm.LoopBack = false;
                                if (!bIPv6)
                                {
                                    switch (
                                        MessageBox.Show(
                                            LocRm.GetString("ErrorLoopback").Replace("[PORT]", port.ToString()),
                                            LocRm.GetString("Error"), MessageBoxButtons.YesNoCancel))
                                    {
                                        case DialogResult.Yes:
                                            ShowTroubleShooter();
                                            return;
                                        case DialogResult.No:
                                            MainForm.Conf.Loopback = false;
                                            MainForm.LoopBack = false;
                                            DialogResult = DialogResult.Yes;
                                            Close();
                                            return;
                                        case DialogResult.Cancel:
                                            return;
                                    }
                                }
                                else
                                {
                                    switch (
                                        MessageBox.Show(
                                            LocRm.GetString("ErrorLoopbackIPv6").Replace("[PORT]", localPort.ToString()),
                                            LocRm.GetString("Error"), MessageBoxButtons.YesNoCancel))
                                    {
                                        case DialogResult.Yes:
                                            ShowTroubleShooter();
                                            return;
                                        case DialogResult.No:
                                            MainForm.Conf.Loopback = false;
                                            MainForm.LoopBack = false;
                                            DialogResult = DialogResult.Yes;
                                            Close();
                                            return;
                                        case DialogResult.Cancel:
                                            return;
                                    }
                                }
                            }
                        }
                        if (result[3] == "")
                        {
                            MainForm.Conf.Loopback = true;
                            MainForm.LoopBack = true;
                            
                            DialogResult = DialogResult.Yes;
                            Close();
                            return;
                        }
                        Next.Enabled = true;
                        Next.Text = LocRm.GetString("Finish");
                    }
                    else
                    {
                        if (result[0].ToLower().IndexOf("login") == -1)
                        {
                            MessageBox.Show(result[0], LocRm.GetString("Error"));
                        }
                        else
                        {
                            MessageBox.Show(result[0], LocRm.GetString("ConnectFailed"));
                        }
                    }
                }
            }
            else
            {
                Next.Enabled = true;
                Next.Text = LocRm.GetString("Finish");
                if (
                    MessageBox.Show(LocRm.GetString("WarningLogin"), LocRm.GetString("Note"), MessageBoxButtons.OKCancel) ==
                    DialogResult.Cancel)
                {
                    return;
                }
                MainForm.Conf.ServicesEnabled = false;
                MainForm.Conf.Subscribed = false;
                MainForm.Conf.WSUsername = "";
                MainForm.Conf.WSPassword = "";
                DialogResult = DialogResult.OK;
                Close();
            }
            Next.Enabled = true;
            Next.Text = LocRm.GetString("Finish");
        }


        private void WebservicesLoad(object sender, EventArgs e)
        {
            var ports = new[] {"21", "25", "80", "110", "143", "443", "587", "8889", "1433", "3306", "8080", "11433"};
            foreach (string port in ports)
            {
                ddlPort.Items.Add(port);
            }
            for (int i = 23010; i <= 23110; i++)
            {
                ddlPort.Items.Add(i.ToString());
            }

            ddlPort.SelectedItem = MainForm.Conf.ServerPort.ToString();


            txtUsername.Text = MainForm.Conf.WSUsername;
            txtPassword.Text = MainForm.Conf.WSPassword;
            txtLANPort.Text = MainForm.Conf.LANPort.ToString();
            txtPort.Text = MainForm.Conf.LANPort.ToString();
            chkReroute.Checked = MainForm.Conf.DHCPReroute;

            chkuPNP.Checked = MainForm.Conf.UseUPNP;
            if (!chkuPNP.Checked)
                chkReroute.Checked = chkReroute.Enabled = false;

            lblIPAddresses.Text = LocRm.GetString("PublicIPAddress").Replace("[IP]", MainForm.IPAddressExternal);

            int i2 = 0;
            foreach (IPAddress ipadd in MainForm.AddressListIPv4)
            {
                if (ipadd.AddressFamily == AddressFamily.InterNetwork)
                {
                    lbIPv4Address.Items.Add(ipadd.ToString());
                    if (ipadd.ToString() == MainForm.AddressIPv4)
                        lbIPv4Address.SelectedIndex = i2;
                    i2++;
                }
            }
            if (lbIPv4Address.Items.Count > 0 && lbIPv4Address.SelectedIndex == -1)
                lbIPv4Address.SelectedIndex = 0;

            
            int _i = 0;
            foreach (IPAddress _ipadd in MainForm.AddressListIPv6)
            {
                lbIPv6Address.Items.Add(_ipadd.ToString());
                if (_ipadd.ToString() == MainForm.AddressIPv6)
                    lbIPv6Address.SelectedIndex = _i;

                _i++;
            }
            

            if (tcIPMode.TabPages.Count == 2)
            {
                switch (MainForm.Conf.IPMode)
                {
                    case "IPv4":
                        tcIPMode.SelectedIndex = 0;
                        break;
                    case "IPv6":
                        tcIPMode.SelectedIndex = 1;
                        break;
                }
            }

            EnableNext();
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("WebAccess");
            button2.Text = LocRm.GetString("Cancel");
            chkReroute.Text = LocRm.GetString("DhcpReroute");
            chkuPNP.Text = LocRm.GetString("AutoConfigureWithUpnp");
            label1.Text = LocRm.GetString("Username");
            label10.Text = LocRm.GetString("LanPort");
            label2.Text = LocRm.GetString("Password");
            label3.Text = LocRm.GetString("WanPort");
            label4.Text = LocRm.GetString("toViewYourRecordedAndLive");
            label5.Text = LocRm.GetString("ifYouAreConnectingMultipl");
            label6.Text = LocRm.GetString("toAccessYourCamerasMicrop");
            label7.Text = LocRm.GetString("usingIpv6IspymightBeAbleT");
            label8.Text = LocRm.GetString("Port");
            lblIPAddresses.Text = LocRm.GetString("PublicIp");
            linkLabel1.Text = LocRm.GetString("CreateANewAccount");
            linkLabel2.Text = LocRm.GetString("OrManuallyConfigureYourRo");
            Next.Text = LocRm.GetString("Finish");
            tabPage1.Text = LocRm.GetString("Upnpipv4");
            tabPage2.Text = LocRm.GetString("Tunnelingipv6");

            toolTip1.SetToolTip(label3, LocRm.GetString("ToolTip_AccessPortExternal"));
            toolTip1.SetToolTip(label10, LocRm.GetString("ToolTip_AccessPortInternal"));
            toolTip1.SetToolTip(lbIPv4Address, LocRm.GetString("ToolTip_SelectIP"));
            toolTip1.SetToolTip(chkReroute, LocRm.GetString("ToolTip_LANIPMonitor"));

            Text = LocRm.GetString("WebServerSettings");
            btnTroubleshooting.Text = LocRm.GetString("troubleshooting");
            llblHelp.Text = LocRm.GetString("help");
        }


        private void Webservices_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void Button2Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void ddlPort_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void LinkLabel2LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MainForm.StartBrowser(MainForm.Website+"/userguide-connecting.aspx#3");
        }

        private void ChkuPnpCheckedChanged(object sender, EventArgs e)
        {
            MainForm.Conf.UseUPNP = chkuPNP.Checked;
            chkReroute.Checked = chkReroute.Enabled = chkuPNP.Checked;
        }

        private void lblIPv6_Click(object sender, EventArgs e)
        {
        }

        private void lbIPv4Address_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide-connecting.aspx");
        }


        private void ShowTroubleShooter()
        {
            int port, localPort;
            string error = "";
            if (!SetupNetwork(out port, out localPort, out error))
            {
                MessageBox.Show(error);
                return;
            }

            var nt = new NetworkTroubleshooter();
            nt.ShowDialog(this);
            nt.Dispose();
        }

        private void tcIPMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableNext();

        }

        private void EnableNext()
        {
            switch (tcIPMode.SelectedIndex)
            {
                case 0:
                    Next.Enabled = btnTroubleshooting.Enabled = lbIPv4Address.SelectedIndex != -1;
                    break;
                case 1:
                    Next.Enabled = btnTroubleshooting.Enabled = lbIPv6Address.SelectedIndex != -1;
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowTroubleShooter();
        }

    }
}