using System;
using System.ComponentModel;
using System.Windows.Forms;
using CodeVendor.Controls;

namespace iSpyApplication
{
    /// <summary>
    /// Summary description for AboutForm.
    /// </summary>
    public class NewVersion : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private Button _button1;
        private Button _button2;

        private Grouper _grouper1;
        private Panel _panel1;
        private WebBrowser _wbProductHistory;

        public NewVersion()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            RenderResources();
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void NewVersionLoad(object sender, EventArgs e)
        {
            _wbProductHistory.Navigate(MainForm.Website+"/producthistory.aspx?productid=11");
        }

        private void RenderResources()
        {
            Text = LocRm.GetString("About");
            _button1.Text = LocRm.GetString("GetLatestVersion");
            _button2.Text = LocRm.GetString("NoThanks");
            _grouper1.GroupTitle = LocRm.GetString("NewVersion");
        }



        private void Grouper1Load(object sender, EventArgs e)
        {
        }

        private void Button2Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button1Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, MainForm.Website+"/download.aspx");

            MessageBox.Show(LocRm.GetString("ExportWarning"), LocRm.GetString("Note"));
            Close();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._grouper1 = new CodeVendor.Controls.Grouper();
            this._wbProductHistory = new System.Windows.Forms.WebBrowser();
            this._panel1 = new System.Windows.Forms.Panel();
            this._button2 = new System.Windows.Forms.Button();
            this._button1 = new System.Windows.Forms.Button();
            this._grouper1.SuspendLayout();
            this._panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _grouper1
            // 
            this._grouper1.BackgroundColor = System.Drawing.Color.White;
            this._grouper1.BackgroundGradientColor = System.Drawing.Color.DarkGray;
            this._grouper1.BackgroundGradientMode = CodeVendor.Controls.Grouper.GroupBoxGradientMode.ForwardDiagonal;
            this._grouper1.BorderColor = System.Drawing.Color.Black;
            this._grouper1.BorderThickness = 1F;
            this._grouper1.Controls.Add(this._wbProductHistory);
            this._grouper1.Controls.Add(this._panel1);
            this._grouper1.CustomGroupBoxColor = System.Drawing.Color.White;
            this._grouper1.GroupImage = null;
            this._grouper1.GroupTitle = "New Version Available!";
            this._grouper1.Location = new System.Drawing.Point(12, 12);
            this._grouper1.Name = "_grouper1";
            this._grouper1.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this._grouper1.PaintGroupBox = false;
            this._grouper1.RoundCorners = 10;
            this._grouper1.ShadowColor = System.Drawing.Color.DarkGray;
            this._grouper1.ShadowControl = false;
            this._grouper1.ShadowThickness = 3;
            this._grouper1.Size = new System.Drawing.Size(516, 504);
            this._grouper1.TabIndex = 18;
            // 
            // _wbProductHistory
            // 
            this._wbProductHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this._wbProductHistory.Location = new System.Drawing.Point(20, 30);
            this._wbProductHistory.MinimumSize = new System.Drawing.Size(20, 20);
            this._wbProductHistory.Name = "_wbProductHistory";
            this._wbProductHistory.Size = new System.Drawing.Size(476, 420);
            this._wbProductHistory.TabIndex = 0;
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._button2);
            this._panel1.Controls.Add(this._button1);
            this._panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panel1.Location = new System.Drawing.Point(20, 450);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(476, 34);
            this._panel1.TabIndex = 1;
            // 
            // _button2
            // 
            this._button2.Location = new System.Drawing.Point(333, 6);
            this._button2.Name = "_button2";
            this._button2.Size = new System.Drawing.Size(140, 23);
            this._button2.TabIndex = 1;
            this._button2.Text = "No Thanks";
            this._button2.UseVisualStyleBackColor = true;
            this._button2.Click += new System.EventHandler(this.Button2Click);
            // 
            // _button1
            // 
            this._button1.Location = new System.Drawing.Point(3, 6);
            this._button1.Name = "_button1";
            this._button1.Size = new System.Drawing.Size(165, 23);
            this._button1.TabIndex = 0;
            this._button1.Text = "Get latest version";
            this._button1.UseVisualStyleBackColor = true;
            this._button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // NewVersion
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.ClientSize = new System.Drawing.Size(540, 528);
            this.Controls.Add(this._grouper1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewVersion";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.Load += new System.EventHandler(this.NewVersionLoad);
            this._grouper1.ResumeLayout(false);
            this._panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion


    }
}