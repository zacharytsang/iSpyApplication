﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace iSpyApplication
{
    public partial class RemoteCommands : Form
    {
        public RemoteCommands()
        {
            InitializeComponent();
            RenderResources();
        }

        private void Button3Click(object sender, EventArgs e)
        {
            var ofdDetect = new OpenFileDialog {FileName = "", InitialDirectory = Program.AppPath + @"sounds\"};
            ofdDetect.ShowDialog();
            if (ofdDetect.FileName != "")
            {
                txtExecute.Text = ofdDetect.FileName;
            }
            ofdDetect.Dispose();
        }

        private void Login()
        {
            ((MainForm) Owner).Connect();
            gpbSubscriber.Enabled = MainForm.Conf.Subscribed;
        }

        private void ManualAlertsLoad(object sender, EventArgs e)
        {
            gpbSubscriber.Enabled = MainForm.Conf.Subscribed;
            linkLabel4.Visible = !gpbSubscriber.Enabled;
            RenderCommands();
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("RemoteCommands");
            btnAddCommand.Text = LocRm.GetString("Add");
            btnDelete.Text = LocRm.GetString("Delete");
            button1.Text = LocRm.GetString("Finish");
            button3.Text = LocRm.GetString("chars_3014702301470230147");
            gpbSubscriber.Text = LocRm.GetString("NewRemoteCommand");
            label1.Text = LocRm.GetString("Name");
            label3.Text = LocRm.GetString("nameFriendlyNameForDispla");
            label45.Text = LocRm.GetString("forExamples");
            label82.Text = LocRm.GetString("YouCanTriggerRemoteComman");
            label83.Text = LocRm.GetString("ExecuteFile");

            llblHelp.Text = LocRm.GetString("help");
        }


        private void RenderCommands()
        {
            lbManualAlerts.Items.Clear();
            foreach (objectsCommand oc in MainForm.RemoteCommands)
            {
                lbManualAlerts.Items.Add(oc.name+" ("+oc.id+")"); //+" (SMS: "+_oc.smscommand+")");
            }
        }

        private void BtnAddCommandClick(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string execute = txtExecute.Text.Trim();
            //string _smscommand = txtSMS.Text.Trim();

            if (MainForm.RemoteCommands.SingleOrDefault(p => p.name == name) != null)
                // || p.smscommand == _smscommand) != null)
            {
                MessageBox.Show(LocRm.GetString("UniqueNameCommand"));
                return;
            }

            var oc = new objectsCommand {name = name, command = execute, id = MainForm.Conf.NextCommandID};
            //_oc.smscommand = _smscommand;
            MainForm.Conf.NextCommandID++;

            MainForm.RemoteCommands.Add(oc);
            RenderCommands();
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            if (lbManualAlerts.SelectedIndex > -1)
            {
                string al = lbManualAlerts.SelectedItem.ToString();
                al = al.Substring(0, al.LastIndexOf("(") - 1).Trim();
                objectsCommand oc = MainForm.RemoteCommands.Where(p => p.name == al).SingleOrDefault();
                if (oc != null)
                {
                    MainForm.RemoteCommands.Remove(oc);
                    RenderCommands();
                }
            }
        }

        private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string execute = txtExecute.Text.Trim();
            if (execute != "")
                Process.Start(execute);
        }

        private void Button1Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Login();
        }

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide-remotecommands.aspx");
        }
    }
}