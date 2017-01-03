﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Moah;

namespace iSpyApplication
{
    public partial class NetworkTroubleshooter : Form
    {
        private string NL = Environment.NewLine;
        public NetworkTroubleshooter()
        {
            InitializeComponent();
            this.Text = LocRm.GetString("troubleshooting");
            button1.Text = LocRm.GetString("OK");
            LocRm.GetString("retry");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NetworkTroubleshooter_Load(object sender, EventArgs e)
        {
            UISync.Init(this);


            var t = new Thread(Troubleshooter) { IsBackground = false };
            t.Start();
        }

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

        private void Troubleshooter()
        {
            MainForm.StopAndStartServer();
            bool portMapOk = false;
            bool IPv6 = MainForm.Conf.IPMode == "IPv6";
            UISync.Execute(() => button2.Enabled = false);
            string localserver = "http://" + MainForm.IPAddress + ":" + MainForm.Conf.LANPort;
            UISync.Execute(() => rtbOutput.Clear());
            UISync.Execute(() => rtbOutput.Text = "Local iSpy Server: "+localserver+NL);
            if (MainForm.Conf.LANPort!=8080)
            {
                UISync.Execute(() => rtbOutput.Text +=
                    "--Warning, running a local server on a non-standard port (8080) may cause web-browser security errors. Click the link above to test in your web browser. You should see the word 'Denied()'." +
                    NL);
            }
            UISync.Execute(() => rtbOutput.Text += "Checking local server... ");
            Application.DoEvents();
            string res = "";
            if (!loadurl(localserver, out res))
            {
                UISync.Execute(() => rtbOutput.Text += "Failed: " + res);
            }
            else
            {
                if (res.ToLower().IndexOf("denied")!=-1)
                {
                    UISync.Execute(() => rtbOutput.Text += "OK");
                }
                else
                {
                    UISync.Execute(() => rtbOutput.Text += "Unexpected output: " + res);
                }
            }
            UISync.Execute(() => rtbOutput.Text += NL);
            UISync.Execute(() => rtbOutput.Text += "Checking iSpyConnect... ");
            Application.DoEvents();
            if (!loadurl(MainForm.Website+"/webservices/ispy.asmx", out res))
            {
                UISync.Execute(() => rtbOutput.Text += "Webservices not responding.");
            }
            else
            {
                if (res.IndexOf("error occurred while")!=-1)
                    UISync.Execute(() => rtbOutput.Text += "Error with webservices. Please try again later.");
                else
                    UISync.Execute(() => rtbOutput.Text += "OK");
            }
            UISync.Execute(() => rtbOutput.Text += NL);
            UISync.Execute(() => rtbOutput.Text += "Checking your firewall... ");
            Application.DoEvents();
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
                    UISync.Execute(() => rtbOutput.Text += "iSpy is *not* enabled");
                }
                else
                {
                    UISync.Execute(() => rtbOutput.Text += "iSpy is enabled");
                }
            }
            else
            {
                UISync.Execute(() => rtbOutput.Text += "Firewall is off");
            }
            UISync.Execute(() => rtbOutput.Text += NL);

            
            UISync.Execute(() => rtbOutput.Text += "Checking your account... ");

            var result = MainForm.WSW.TestConnection(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, false);
            if (result[0] != "OK")
            {
                UISync.Execute(() => rtbOutput.Text += result[0]);
            }
            else
            {

                UISync.Execute(() => rtbOutput.Text += "Found: " + result[2]);
                if (Convert.ToBoolean(result[1]))
                {
                    UISync.Execute(() => rtbOutput.Text += NL + "Your subscription is valid." + NL);
                    if (MainForm.Conf.IPMode == "IPv4")
                    {

                        UISync.Execute(() => rtbOutput.Text += "IPv4: Checking port mappings... " + NL);
                        int i = 3;
                        while (NATControl.Mappings == null && i > 0)
                        {
                            Thread.Sleep(2000);
                            i--;
                        }
                        if (NATControl.Mappings == null)
                        {
                            UISync.Execute(
                                () =>
                                rtbOutput.Text +=
                                "IPv4 Port mappings are unavailable - set up port mapping manually, instructions here: http://portforward.com/english/routers/port_forwarding/routerindex.htm" +
                                NL);
                        }
                        else
                        {
                            int j = 2;
                            while (!portMapOk && j > 0)
                            {
                                var enumerator = NATControl.Mappings.GetEnumerator();

                                while (enumerator.MoveNext())
                                {
                                    var map = (NATUPNPLib.IStaticPortMapping) enumerator.Current;
                                    UISync.Execute(
                                        () =>
                                        rtbOutput.Text +=
                                        map.ExternalPort + " -> " + map.InternalPort + " on " + map.InternalClient +
                                        " (" +
                                        map.Protocol + ")" + NL);
                                    if (map.ExternalPort == MainForm.Conf.ServerPort)
                                    {
                                        if (map.InternalPort != MainForm.Conf.LANPort)
                                        {
                                            UISync.Execute(
                                                () =>
                                                rtbOutput.Text +=
                                                "*** External port is routing to " + map.InternalPort + " instead of " +
                                                MainForm.Conf.LANPort + NL);
                                        }
                                        else
                                        {
                                            if (map.InternalClient != MainForm.AddressIPv4)
                                            {
                                                UISync.Execute(
                                                    () =>
                                                    rtbOutput.Text +=
                                                    "*** Port is mapping to IP Address " + map.InternalClient +
                                                    " - should be " +
                                                    MainForm.AddressIPv4 +
                                                    ". Set a static IP address for your computer and then update the port mapping." +
                                                    NL);
                                            }
                                            else
                                            {
                                                portMapOk = true;
                                            }
                                        }
                                    }
                                }
                                if (!portMapOk)
                                {
                                    //add port mapping
                                    UISync.Execute(() => rtbOutput.Text += "IPv4: Fixing port mapping... " + NL);
                                    if (!NATControl.SetPorts(MainForm.Conf.ServerPort, MainForm.Conf.LANPort))
                                    {
                                        UISync.Execute(() => rtbOutput.Text += LocRm.GetString("ErrorPortMapping"));
                                    }

                                    j--;
                                    if (j>0)
                                        UISync.Execute(() => rtbOutput.Text += "IPv4: Checking port mappings... " + NL);
                                }
                            }
                        }
                    }

                    UISync.Execute(() => rtbOutput.Text += "Checking external access... "+NL);

                    result = MainForm.WSW.TestConnection(MainForm.Conf.WSUsername, MainForm.Conf.WSPassword, true);

                    if (result[3] != "")
                    {
                        UISync.Execute(() => rtbOutput.Text += "Failed: " + result[6] + NL);
                        UISync.Execute(() => rtbOutput.Text += "iSpy Connect is trying to contact: http://" + MainForm.IPAddressExternal +
                                          ":" + MainForm.Conf.ServerPort + NL);
                        if (!IPv6)
                        {
                            UISync.Execute(
                                () =>
                                rtbOutput.Text +=
                                "Your router should be configured to forward TCP traffic from WAN (external) port " +
                                MainForm.Conf.ServerPort + " to internal (LAN) port " +
                                MainForm.Conf.LANPort + " on IP address " + MainForm.AddressIPv4 +
                                NL);
                            UISync.Execute(
                                () =>
                                rtbOutput.Text +=
                                "Check http://www.ispyconnect.com/userguide-connecting.aspx#6 for troubleshooting.");
                            if (portMapOk)
                            {
                                UISync.Execute(
                                    () =>
                                    rtbOutput.Text +=
                                    NL +
                                    "Your port mapping seems to be OK - try turning your router off and on again. Failing that we recommend checking with your ISP to see if they are blocking port " +
                                    MainForm.Conf.ServerPort +
                                    " or check if your antivirus protection (eset, zonealarm etc) is blocking iSpy. ");
                            }
                        }
                        UISync.Execute(() => rtbOutput.Text += NL+NL+"If you still cannot get it working, please copy and paste this information into http://www.ispyconnect.com/contact.aspx");
                    }
                    else
                    {
                        UISync.Execute(() => rtbOutput.Text +=
                            "Success!"+NL+NL+"If you cannot access iSpy content locally please ensure 'Use LAN IP when available' is checked on http://www.ispyconnect.com/account.aspx and also ensure you're using an up to date web browser (we recommend google Chrome. Opera is incompatible)");
                    }
                }
                else
                {
                    UISync.Execute(() => rtbOutput.Text += NL +
                                      "Not subscribed - local access only. http://www.ispyconnect.com/subscribe.aspx");

                }
            }
            UISync.Execute(() => rtbOutput.Text+=NL);
            Application.DoEvents();
            UISync.Execute(() => button2.Enabled = true);
        }

        private void rtbOutput_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            Help.ShowHelp(this,e.LinkText);
        }


        private bool loadurl(string url, out string result)
        {
            result = "";
            try
            {
                var HttpWReq = (HttpWebRequest) WebRequest.Create(url);
                HttpWReq.Method = "GET";

                var myResponse = (HttpWebResponse) HttpWReq.GetResponse();

                var read = new StreamReader(myResponse.GetResponseStream());
                result = read.ReadToEnd();
                myResponse.Close();
                
                return true;
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var t = new Thread(Troubleshooter) { IsBackground = false };
            t.Start();
        }
    }
}
