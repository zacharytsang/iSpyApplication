using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace iSpyApplication
{
    public partial class ConfigureNumberPlates : Form
    {
        public string NumberPlates = "";
        public int Mode;
        public int FrameInterval = 1;
        public bool Overlay;
        public bool AutoThreshold;
        public int Accuracy;
        public int Threshold;
        public string Area;

        public Bitmap Framegrab;
        private readonly Thresholder _thresholder1;

        public ConfigureNumberPlates()
        {
            InitializeComponent();

            _thresholder1 = new Thresholder
                               {
                                   BackColor = Color.Black,
                                   BackgroundImageLayout = ImageLayout.Stretch,
                                   Cursor = Cursors.Hand,
                                   Location = new Point(7, 13),
                                   Margin = new Padding(0),
                                   MinimumSize = new Size(100, 100),
                                   Name = "thresholder1",
                                   Padding = new Padding(0, 0, 3, 3),
                                   Size = new Size(320, 240),
                                   TabIndex = 4
                               };
            pnlAdvanced.Controls.Add(_thresholder1);
            _thresholder1.BoundsChanged += new EventHandler(_thresholder1_BoundsChanged);
            RenderResources();
        }

        void _thresholder1_BoundsChanged(object sender, EventArgs e)
        {
            _thresholder1.Invalidate();
        }

        

        private void RenderResources()
        {
            Text = LocRm.GetString("Configure");
            lblNumberPlatesConfig.Text = LocRm.GetString("NumberPlatesConfig");
            button1.Text = LocRm.GetString("OK");
            rdoMode.Text = LocRm.GetString("Continuous");
            rdoMode2.Text = LocRm.GetString("Parking");
            label3.Text = LocRm.GetString("Interval");
            label1.Text = LocRm.GetString("Seconds");
            label2.Text = LocRm.GetString("NumberplateProcessingMode");
            label4.Text = LocRm.GetString("RequireAccuracy");
            label5.Text = LocRm.GetString("percent");
            label6.Text = LocRm.GetString("NumberplateInstructions");
           
            chkOverlay.Text = LocRm.GetString("Overlay");
            llblHelp.Text = LocRm.GetString("help");
        }

        private void ConfigureNumberPlatesLoad(object sender, EventArgs e)
        {
            rtbNumberPlates.Text = NumberPlates;
            if (Mode == 0)
                rdoMode.Checked = true;
            else
            {
                rdoMode2.Checked = true;
            }
            if (FrameInterval == 0)
                FrameInterval = 1;
            numNPRFrame.Value = FrameInterval;
            chkOverlay.Checked = Overlay;
            numAccuracy.Value = Accuracy;
            pnlAdvanced.Enabled = false;

            if (Framegrab != null)
            {
                _thresholder1.LastFrame = Framegrab;
                _thresholder1.Area = Area;
                pnlAdvanced.Enabled = true;
                _thresholder1.Invalidate();
            }
        }

        private void Button1Click(object sender, EventArgs e)
        {
            string np = "";
            string[] plates = rtbNumberPlates.Text.Split(',');
            for (int index = 0; index < plates.Length; index++)
            {
                string plate = plates[index];
                if (plate != "")
                {
                    np += plate.ToUpper().Replace(" ", "").Trim() + ",";
                }
            }
            np = np.Trim(',');

            NumberPlates = np;
            DialogResult = DialogResult.OK;
            Mode = 0;
            if (rdoMode2.Checked) Mode = 1;
            FrameInterval = (int) numNPRFrame.Value;
            Accuracy = (int) numAccuracy.Value;
            Overlay = chkOverlay.Checked;
            Area = _thresholder1.Area;
            Close();
        }

        private void rdoMode2_CheckedChanged(object sender, EventArgs e)
        {
            numNPRFrame.Enabled = !rdoMode2.Checked;
        }

        private void llblHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/userguide-alpr.aspx");
        }
    }
}
