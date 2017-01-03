namespace iSpyApplication
{
    partial class ConfigureNumberPlates
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
            this.button1 = new System.Windows.Forms.Button();
            this.rtbNumberPlates = new System.Windows.Forms.RichTextBox();
            this.lblNumberPlatesConfig = new System.Windows.Forms.Label();
            this.rdoMode = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numNPRFrame = new System.Windows.Forms.NumericUpDown();
            this.rdoMode2 = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.chkOverlay = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numAccuracy = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.pnlAdvanced = new System.Windows.Forms.Panel();
            this.llblHelp = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNPRFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAccuracy)).BeginInit();
            this.pnlAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(586, 423);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 78;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // rtbNumberPlates
            // 
            this.rtbNumberPlates.Location = new System.Drawing.Point(12, 25);
            this.rtbNumberPlates.Name = "rtbNumberPlates";
            this.rtbNumberPlates.Size = new System.Drawing.Size(279, 247);
            this.rtbNumberPlates.TabIndex = 79;
            this.rtbNumberPlates.Text = global::iSpyApplication.Properties.Resources.nothing;
            // 
            // lblNumberPlatesConfig
            // 
            this.lblNumberPlatesConfig.AutoSize = true;
            this.lblNumberPlatesConfig.Location = new System.Drawing.Point(12, 9);
            this.lblNumberPlatesConfig.Name = "lblNumberPlatesConfig";
            this.lblNumberPlatesConfig.Size = new System.Drawing.Size(35, 13);
            this.lblNumberPlatesConfig.TabIndex = 80;
            this.lblNumberPlatesConfig.Text = "label1";
            // 
            // rdoMode
            // 
            this.rdoMode.AutoSize = true;
            this.rdoMode.Location = new System.Drawing.Point(12, 12);
            this.rdoMode.Name = "rdoMode";
            this.rdoMode.Size = new System.Drawing.Size(78, 17);
            this.rdoMode.TabIndex = 81;
            this.rdoMode.TabStop = true;
            this.rdoMode.Text = "Continuous";
            this.rdoMode.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.numNPRFrame);
            this.panel1.Controls.Add(this.rdoMode2);
            this.panel1.Controls.Add(this.rdoMode);
            this.panel1.Location = new System.Drawing.Point(12, 294);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(279, 104);
            this.panel1.TabIndex = 82;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(201, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 86;
            this.label1.Text = "Seconds";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 85;
            this.label3.Text = "Interval";
            // 
            // numNPRFrame
            // 
            this.numNPRFrame.Location = new System.Drawing.Point(141, 35);
            this.numNPRFrame.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numNPRFrame.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNPRFrame.Name = "numNPRFrame";
            this.numNPRFrame.Size = new System.Drawing.Size(54, 20);
            this.numNPRFrame.TabIndex = 83;
            this.numNPRFrame.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // rdoMode2
            // 
            this.rdoMode2.AutoSize = true;
            this.rdoMode2.Location = new System.Drawing.Point(12, 69);
            this.rdoMode2.Name = "rdoMode2";
            this.rdoMode2.Size = new System.Drawing.Size(148, 17);
            this.rdoMode2.TabIndex = 82;
            this.rdoMode2.TabStop = true;
            this.rdoMode2.Text = "Parking (after motion stop)";
            this.rdoMode2.UseVisualStyleBackColor = true;
            this.rdoMode2.CheckedChanged += new System.EventHandler(this.rdoMode2_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 275);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 13);
            this.label2.TabIndex = 83;
            this.label2.Text = "Numberplate Processing Mode";
            // 
            // chkOverlay
            // 
            this.chkOverlay.AutoSize = true;
            this.chkOverlay.Location = new System.Drawing.Point(15, 408);
            this.chkOverlay.Name = "chkOverlay";
            this.chkOverlay.Size = new System.Drawing.Size(62, 17);
            this.chkOverlay.TabIndex = 84;
            this.chkOverlay.Text = "Overlay";
            this.chkOverlay.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 428);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 85;
            this.label4.Text = "Require Accuracy";
            // 
            // numAccuracy
            // 
            this.numAccuracy.Location = new System.Drawing.Point(157, 426);
            this.numAccuracy.Name = "numAccuracy";
            this.numAccuracy.Size = new System.Drawing.Size(50, 20);
            this.numAccuracy.TabIndex = 86;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(213, 428);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 87;
            this.label5.Text = "Percent";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(3, 281);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(312, 57);
            this.label6.TabIndex = 1;
            this.label6.Text = "Drag out a region of the image to isolate for detection.";
            // 
            // pnlAdvanced
            // 
            this.pnlAdvanced.Controls.Add(this.label6);
            this.pnlAdvanced.Location = new System.Drawing.Point(320, 25);
            this.pnlAdvanced.Name = "pnlAdvanced";
            this.pnlAdvanced.Size = new System.Drawing.Size(341, 373);
            this.pnlAdvanced.TabIndex = 88;
            // 
            // llblHelp
            // 
            this.llblHelp.AutoSize = true;
            this.llblHelp.Location = new System.Drawing.Point(496, 428);
            this.llblHelp.Name = "llblHelp";
            this.llblHelp.Size = new System.Drawing.Size(29, 13);
            this.llblHelp.TabIndex = 89;
            this.llblHelp.TabStop = true;
            this.llblHelp.Text = "Help";
            this.llblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblHelp_LinkClicked);
            // 
            // ConfigureNumberPlates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 460);
            this.Controls.Add(this.llblHelp);
            this.Controls.Add(this.pnlAdvanced);
            this.Controls.Add(this.lblNumberPlatesConfig);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.rtbNumberPlates);
            this.Controls.Add(this.chkOverlay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numAccuracy);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ConfigureNumberPlates";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure";
            this.Load += new System.EventHandler(this.ConfigureNumberPlatesLoad);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numNPRFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAccuracy)).EndInit();
            this.pnlAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox rtbNumberPlates;
        private System.Windows.Forms.Label lblNumberPlatesConfig;
        private System.Windows.Forms.RadioButton rdoMode;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numNPRFrame;
        private System.Windows.Forms.RadioButton rdoMode2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkOverlay;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numAccuracy;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pnlAdvanced;
        private System.Windows.Forms.LinkLabel llblHelp;
    }
}