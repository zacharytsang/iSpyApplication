﻿using System;
using System.Windows.Forms;

namespace iSpyApplication
{
    public partial class ConfigureSeconds : Form
    {
        public int Seconds = 0;

        public ConfigureSeconds()
        {
            InitializeComponent();
            RenderResources();
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("Configure");
            label48.Text = LocRm.GetString("Seconds");
            label1.Text = LocRm.GetString("For");
            button1.Text = LocRm.GetString("OK");
        }

        private void ForSecondsLoad(object sender, EventArgs e)
        {
            txtSeconds.Value = Seconds;
        }

        private void Button1Click(object sender, EventArgs e)
        {
            Seconds = Convert.ToInt32(txtSeconds.Value);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
