using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace iSpyApplication
{
    public partial class ConfigureTimestamp : Form
    {
        public int TimeStampLocation = 0, FontSize=10;
        public ConfigureTimestamp()
        {
            InitializeComponent();
            RenderResources();
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("Configure");
            label1.Text = LocRm.GetString("Location");
            label2.Text = LocRm.GetString("Size");
            button1.Text = LocRm.GetString("OK");
        }

        private void ConfigureTimestamp_Load(object sender, EventArgs e)
        {
            ddlTimestampLocation.Items.AddRange(LocRm.GetString("TimeStampLocations").Split(','));
            ddlTimestampLocation.SelectedIndex = TimeStampLocation;
            numFontSize.Value = FontSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TimeStampLocation = ddlTimestampLocation.SelectedIndex;
            FontSize = (int) numFontSize.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
