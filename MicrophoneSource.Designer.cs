namespace iSpyApplication
{
    partial class MicrophoneSource
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ddlDevice = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tcAudioSource = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.txtNetwork = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pnlVLC = new System.Windows.Forms.Panel();
            this.txtReconnect = new System.Windows.Forms.NumericUpDown();
            this.label48 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.cmbVLCURL = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.txtVLCArgs = new System.Windows.Forms.TextBox();
            this.lblInstallVLC = new System.Windows.Forms.Label();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.txtChannels = new System.Windows.Forms.NumericUpDown();
            this.txtBits = new System.Windows.Forms.NumericUpDown();
            this.txtSampleRate = new System.Windows.Forms.NumericUpDown();
            this.llblHelp = new System.Windows.Forms.LinkLabel();
            this.tcAudioSource.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.pnlVLC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtReconnect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtChannels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSampleRate)).BeginInit();
            this.SuspendLayout();
            // 
            // ddlDevice
            // 
            this.ddlDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlDevice.FormattingEnabled = true;
            this.ddlDevice.Location = new System.Drawing.Point(24, 54);
            this.ddlDevice.Name = "ddlDevice";
            this.ddlDevice.Size = new System.Drawing.Size(322, 21);
            this.ddlDevice.TabIndex = 26;
            this.ddlDevice.SelectedIndexChanged += new System.EventHandler(this.DdlDeviceSelectedIndexChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(214, 362);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(81, 23);
            this.button2.TabIndex = 25;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(317, 362);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(67, 23);
            this.button1.TabIndex = 24;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 35;
            this.label2.Text = "Sample Rate";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 37;
            this.label3.Text = "Bits Per Sample";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(205, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 39;
            this.label4.Text = "Channels";
            // 
            // tcAudioSource
            // 
            this.tcAudioSource.Controls.Add(this.tabPage1);
            this.tcAudioSource.Controls.Add(this.tabPage3);
            this.tcAudioSource.Controls.Add(this.tabPage2);
            this.tcAudioSource.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tcAudioSource.Location = new System.Drawing.Point(13, 72);
            this.tcAudioSource.Name = "tcAudioSource";
            this.tcAudioSource.SelectedIndex = 0;
            this.tcAudioSource.Size = new System.Drawing.Size(371, 284);
            this.tcAudioSource.TabIndex = 45;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.ddlDevice);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(363, 258);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Local Device";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(21, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Device";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.linkLabel1);
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.txtNetwork);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(363, 258);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "iSpy Server";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Location = new System.Drawing.Point(12, 57);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(351, 46);
            this.linkLabel1.TabIndex = 32;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Use iSpy Server to connect to USB cameras and Microphones running on other comput" +
                "ers";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1LinkClicked1);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 13);
            this.label9.TabIndex = 31;
            this.label9.Text = "URL";
            // 
            // txtNetwork
            // 
            this.txtNetwork.Location = new System.Drawing.Point(73, 21);
            this.txtNetwork.Name = "txtNetwork";
            this.txtNetwork.Size = new System.Drawing.Size(245, 20);
            this.txtNetwork.TabIndex = 30;
            this.txtNetwork.TextChanged += new System.EventHandler(this.TextBox1TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.pnlVLC);
            this.tabPage2.Controls.Add(this.lblInstallVLC);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(363, 258);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "VLC";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // pnlVLC
            // 
            this.pnlVLC.Controls.Add(this.txtReconnect);
            this.pnlVLC.Controls.Add(this.label48);
            this.pnlVLC.Controls.Add(this.label43);
            this.pnlVLC.Controls.Add(this.label21);
            this.pnlVLC.Controls.Add(this.cmbVLCURL);
            this.pnlVLC.Controls.Add(this.label18);
            this.pnlVLC.Controls.Add(this.label19);
            this.pnlVLC.Controls.Add(this.txtVLCArgs);
            this.pnlVLC.Location = new System.Drawing.Point(3, 3);
            this.pnlVLC.Name = "pnlVLC";
            this.pnlVLC.Size = new System.Drawing.Size(357, 164);
            this.pnlVLC.TabIndex = 59;
            // 
            // txtReconnect
            // 
            this.txtReconnect.Location = new System.Drawing.Point(136, 125);
            this.txtReconnect.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.txtReconnect.Name = "txtReconnect";
            this.txtReconnect.Size = new System.Drawing.Size(62, 20);
            this.txtReconnect.TabIndex = 84;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(209, 127);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(47, 13);
            this.label48.TabIndex = 83;
            this.label48.Text = "seconds";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(18, 127);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(90, 13);
            this.label43.TabIndex = 82;
            this.label43.Text = "Reconnect Every";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(18, 17);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(52, 13);
            this.label21.TabIndex = 50;
            this.label21.Text = "VLC URL";
            // 
            // cmbVLCURL
            // 
            this.cmbVLCURL.FormattingEnabled = true;
            this.cmbVLCURL.Location = new System.Drawing.Point(136, 14);
            this.cmbVLCURL.Name = "cmbVLCURL";
            this.cmbVLCURL.Size = new System.Drawing.Size(208, 21);
            this.cmbVLCURL.TabIndex = 49;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(18, 61);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(57, 13);
            this.label18.TabIndex = 52;
            this.label18.Text = "Arguments";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(47, 38);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(297, 13);
            this.label19.TabIndex = 53;
            this.label19.Text = "eg: http://username:password@192.168.1.4/videostream.asf";
            // 
            // txtVLCArgs
            // 
            this.txtVLCArgs.Location = new System.Drawing.Point(136, 58);
            this.txtVLCArgs.Multiline = true;
            this.txtVLCArgs.Name = "txtVLCArgs";
            this.txtVLCArgs.Size = new System.Drawing.Size(208, 61);
            this.txtVLCArgs.TabIndex = 51;
            // 
            // lblInstallVLC
            // 
            this.lblInstallVLC.Location = new System.Drawing.Point(3, 186);
            this.lblInstallVLC.Name = "lblInstallVLC";
            this.lblInstallVLC.Size = new System.Drawing.Size(357, 62);
            this.lblInstallVLC.TabIndex = 58;
            this.lblInstallVLC.Text = "You can use VLC to connect to many different sources including .asf, .mp4, rtsp s" +
                "treams, udp streams and many more.\r\n\r\nPlease install VLC and restart iSpy to ena" +
                "ble this functionality";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(20, 367);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(78, 13);
            this.linkLabel3.TabIndex = 60;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "Download VLC";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel3LinkClicked);
            // 
            // txtChannels
            // 
            this.txtChannels.Enabled = false;
            this.txtChannels.Location = new System.Drawing.Point(289, 46);
            this.txtChannels.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.txtChannels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtChannels.Name = "txtChannels";
            this.txtChannels.Size = new System.Drawing.Size(56, 20);
            this.txtChannels.TabIndex = 46;
            this.txtChannels.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtBits
            // 
            this.txtBits.Enabled = false;
            this.txtBits.Location = new System.Drawing.Point(143, 46);
            this.txtBits.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.txtBits.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.txtBits.Name = "txtBits";
            this.txtBits.Size = new System.Drawing.Size(56, 20);
            this.txtBits.TabIndex = 47;
            this.txtBits.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // txtSampleRate
            // 
            this.txtSampleRate.Enabled = false;
            this.txtSampleRate.Location = new System.Drawing.Point(143, 16);
            this.txtSampleRate.Maximum = new decimal(new int[] {
            88200,
            0,
            0,
            0});
            this.txtSampleRate.Name = "txtSampleRate";
            this.txtSampleRate.Size = new System.Drawing.Size(113, 20);
            this.txtSampleRate.TabIndex = 48;
            // 
            // llblHelp
            // 
            this.llblHelp.AutoSize = true;
            this.llblHelp.Location = new System.Drawing.Point(140, 367);
            this.llblHelp.Name = "llblHelp";
            this.llblHelp.Size = new System.Drawing.Size(29, 13);
            this.llblHelp.TabIndex = 61;
            this.llblHelp.TabStop = true;
            this.llblHelp.Text = "Help";
            this.llblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblHelp_LinkClicked);
            // 
            // MicrophoneSource
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 395);
            this.Controls.Add(this.llblHelp);
            this.Controls.Add(this.linkLabel3);
            this.Controls.Add(this.txtSampleRate);
            this.Controls.Add(this.txtBits);
            this.Controls.Add(this.txtChannels);
            this.Controls.Add(this.tcAudioSource);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MicrophoneSource";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Microphone Source";
            this.Load += new System.EventHandler(this.MicrophoneSourceLoad);
            this.tcAudioSource.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.pnlVLC.ResumeLayout(false);
            this.pnlVLC.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtReconnect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtChannels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSampleRate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ddlDevice;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tcAudioSource;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox txtNetwork;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Panel pnlVLC;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.ComboBox cmbVLCURL;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtVLCArgs;
        private System.Windows.Forms.Label lblInstallVLC;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.NumericUpDown txtChannels;
        private System.Windows.Forms.NumericUpDown txtBits;
        private System.Windows.Forms.NumericUpDown txtSampleRate;
        private System.Windows.Forms.NumericUpDown txtReconnect;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.LinkLabel llblHelp;
    }
}