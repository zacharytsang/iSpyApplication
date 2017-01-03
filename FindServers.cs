using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ListNetworkComputers;

namespace iSpyApplication
{
    public partial class FindServers : Form
    {
        public ArrayList Addresses = new ArrayList();
        public int FindMode = 1;
        public FindServers()
        {
            InitializeComponent();
        }

        private void FindServers_Load(object sender, EventArgs e)
        {
            if (FindMode == 1)
            {
                tabControl1.Controls.RemoveAt(1);
            }
            if (FindMode == 2)
            {
                tabControl1.Controls.RemoveAt(0);
            }
            NetworkBrowser _NB = new NetworkBrowser();
            ArrayList _NC = _NB.getNetworkComputers();
            foreach(string s in _NC)
            {
                cmbComputers.Items.Add(s);
            }
            if (cmbComputers.Items.Count>0)
                cmbComputers.SelectedIndex = 0;

            
            ddlCameraType.Items.Add(new ListItem("Airlink", "http://ip:port/mjpeg.cgi"));
            ddlCameraType.Items.Add(new ListItem("Axis", "http://ip:port/axis-cgi/mjpg/video.cgi"));
            ddlCameraType.Items.Add(new ListItem("D-Link", "http://ip:port/mjpeg.cgi"));
            ddlCameraType.Items.Add(new ListItem("Foscam", "http://ip:port/videostream.cgi"));
            ddlCameraType.Items.Add(new ListItem("Trendnet", "http://ip:port/cgi/mjpg/mjpeg.cgi"));

            ddlCameraType.SelectedIndex = 0;
        }
        private struct ListItem
        {
            internal string Name;
            internal string Value;
            public override string ToString()
            {
                return Name;
            }
            public ListItem(string Name, string Value) { this.Name = Name; this.Value = Value; }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Addresses.Clear();
            string s = cmbComputers.Text;
            if (s != "")
            {
                string IP = s;
                IPAddress _IP;
                if (!IPAddress.TryParse(IP, out _IP))
                {
                    IPAddress[] _ipaddresses = null;
                    try
                    {
                        _ipaddresses = Dns.GetHostEntry(s).AddressList;
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                    if (_ipaddresses == null)
                    {
                        MessageBox.Show("Could not find " + cmbComputers.Text);
                        return;
                    }
                    foreach (IPAddress _IPS in _ipaddresses)
                    {
                        if (_IPS.AddressFamily.ToString() == "InterNetwork")
                        {
                            IP = _IPS.ToString();
                            break;
                        }
                    }
                    
                }
                Find(IP);
            }
            if (Addresses.Count > 0)
                this.Close();
            else
                MessageBox.Show("Found no available cameras");
        }
        private void Find(string IP)
        {
            string addr = "http://" + IP + ":" + txtPort.Text + "/?c=l";
            
            try
            {
                string list = "";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(addr);
                myReq.Timeout = 2000;
                HttpWebResponse response = (HttpWebResponse)
                myReq.GetResponse();

                // we will read data via the response stream
                Stream data = response.GetResponseStream();
                StreamReader reader = new StreamReader(data);
                string str = "";
                str = reader.ReadLine();
                if (str != null)
                {
                    string c = str.Substring(str.IndexOf("=")+1);
                    list += "http://" + IP + ":" + txtPort.Text + "/?c=" + c;
                }
                data.Close();
                string[] cameras = list.Split('|');
                foreach (string _s in cameras)
                {
                    Addresses.Add(_s);
                }
            }
            catch (WebException ex)
            {
                string m = ex.Message;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Addresses.Clear();
            button2.Enabled = false;
            foreach(string s in cmbComputers.Items)
            {
                if (s!="")
                {
                    string IP = s;
                    IPAddress _IP;
                    if (!IPAddress.TryParse(IP, out _IP))
                    {
                        foreach (IPAddress _IPS in Dns.GetHostEntry(s).AddressList)
                        {
                            if (_IPS.AddressFamily.ToString() == "InterNetwork")
                            {
                                IP = _IPS.ToString();
                                break;
                            }
                        }
                    }
                      
                    Find(IP);
                }
            }
            button2.Enabled = true;
            if (Addresses.Count>0)
                this.Close();
            else 
                MessageBox.Show("Found no available cameras");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Addresses.Clear();
            string _add = txtTemplate.Text;
            _add = _add.ReplaceString("ip", txtIPAddress.Text);
            _add = _add.ReplaceString("port", txtPort2.Text);
            Addresses.Add(_add);
            this.Close();
            
        }

        private void ddlCameraType_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtTemplate.Text = ((ListItem)ddlCameraType.SelectedItem).Value;
        }

        
    }
}