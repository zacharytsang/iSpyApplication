using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace iSpyApplication
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class AboutForm : Form
    {
        private Label _lblCopyright;
        private PictureBox _pictureBox1;
        private CodeVendor.Controls.Grouper _grouper1;
        private LinkLabel _linkLabel2;
        private Label _label1;
        private Label _lblVersion;
        private Button _btnOk;
        
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		public AboutForm( )
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

        private void RenderResources()
        {
            _lblVersion.Text = "iSpy v" + Application.ProductVersion;
            _grouper1.GroupTitle = LocRm.GetString("AboutiSpy");
            _label1.Text = LocRm.GetString("HomePage");
            _lblCopyright.Text = LocRm.GetString("Copyright");
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this._lblCopyright = new System.Windows.Forms.Label();
            this._pictureBox1 = new System.Windows.Forms.PictureBox();
            this._grouper1 = new CodeVendor.Controls.Grouper();
            this._btnOk = new System.Windows.Forms.Button();
            this._linkLabel2 = new System.Windows.Forms.LinkLabel();
            this._label1 = new System.Windows.Forms.Label();
            this._lblVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox1)).BeginInit();
            this._grouper1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblCopyright
            // 
            this._lblCopyright.Location = new System.Drawing.Point(152, 54);
            this._lblCopyright.Name = "_lblCopyright";
            this._lblCopyright.Size = new System.Drawing.Size(215, 16);
            this._lblCopyright.TabIndex = 13;
            this._lblCopyright.Text = "Copyright © 2011 iSpyConnect.com";
            this._lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _pictureBox1
            // 
            this._pictureBox1.Image = global::iSpyApplication.Properties.Resources.ispy;
            this._pictureBox1.Location = new System.Drawing.Point(19, 36);
            this._pictureBox1.Name = "_pictureBox1";
            this._pictureBox1.Size = new System.Drawing.Size(127, 123);
            this._pictureBox1.TabIndex = 17;
            this._pictureBox1.TabStop = false;
            // 
            // _grouper1
            // 
            this._grouper1.BackgroundColor = System.Drawing.Color.White;
            this._grouper1.BackgroundGradientColor = System.Drawing.Color.Silver;
            this._grouper1.BackgroundGradientMode = CodeVendor.Controls.Grouper.GroupBoxGradientMode.ForwardDiagonal;
            this._grouper1.BorderColor = System.Drawing.Color.Black;
            this._grouper1.BorderThickness = 1F;
            this._grouper1.Controls.Add(this._btnOk);
            this._grouper1.Controls.Add(this._linkLabel2);
            this._grouper1.Controls.Add(this._label1);
            this._grouper1.Controls.Add(this._lblVersion);
            this._grouper1.Controls.Add(this._pictureBox1);
            this._grouper1.Controls.Add(this._lblCopyright);
            this._grouper1.CustomGroupBoxColor = System.Drawing.Color.White;
            this._grouper1.GroupImage = null;
            this._grouper1.GroupTitle = "About iSpy";
            this._grouper1.Location = new System.Drawing.Point(12, 12);
            this._grouper1.Name = "_grouper1";
            this._grouper1.Padding = new System.Windows.Forms.Padding(20);
            this._grouper1.PaintGroupBox = false;
            this._grouper1.RoundCorners = 10;
            this._grouper1.ShadowColor = System.Drawing.Color.DarkGray;
            this._grouper1.ShadowControl = false;
            this._grouper1.ShadowThickness = 3;
            this._grouper1.Size = new System.Drawing.Size(399, 181);
            this._grouper1.TabIndex = 18;
            this._grouper1.Load += new System.EventHandler(this.Grouper1Load);
            // 
            // _btnOk
            // 
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnOk.Location = new System.Drawing.Point(292, 136);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 23);
            this._btnOk.TabIndex = 19;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // _linkLabel2
            // 
            this._linkLabel2.AutoSize = true;
            this._linkLabel2.Location = new System.Drawing.Point(184, 98);
            this._linkLabel2.Name = "_linkLabel2";
            this._linkLabel2.Size = new System.Drawing.Size(145, 13);
            this._linkLabel2.TabIndex = 20;
            this._linkLabel2.TabStop = true;
            this._linkLabel2.Text = MainForm.Website+"";
            this._linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2LinkClicked);
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(152, 74);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(65, 13);
            this._label1.TabIndex = 19;
            this._label1.Text = "Homepage: ";
            // 
            // _lblVersion
            // 
            this._lblVersion.AutoSize = true;
            this._lblVersion.Location = new System.Drawing.Point(152, 38);
            this._lblVersion.Name = "_lblVersion";
            this._lblVersion.Size = new System.Drawing.Size(42, 13);
            this._lblVersion.TabIndex = 18;
            this._lblVersion.Text = "Version";
            // 
            // AboutForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.Yellow;
            this.ClientSize = new System.Drawing.Size(418, 228);
            this.Controls.Add(this._grouper1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            this.TransparencyKey = System.Drawing.Color.Yellow;
            this.Load += new System.EventHandler(this.AboutFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox1)).EndInit();
            this._grouper1.ResumeLayout(false);
            this._grouper1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

        private void AboutFormLoad(object sender, EventArgs e)
        {
            
        }

        private void LinkLabel2LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MainForm.StartBrowser(MainForm.Website+"/");
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            Close();
        }

        private void Grouper1Load(object sender, EventArgs e)
        {

        }

        
	}
}
