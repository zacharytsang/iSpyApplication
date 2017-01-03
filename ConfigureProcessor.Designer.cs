namespace iSpyApplication
{
    partial class ConfigureProcessor
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
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.pnlTrackingColor = new System.Windows.Forms.Panel();
            this.chkKeepEdges = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.numHeight = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cdTracking = new System.Windows.Forms.ColorDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.chkHighlight = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(137, 213);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 23);
            this.button1.TabIndex = 78;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(12, 151);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(79, 13);
            this.label48.TabIndex = 83;
            this.label48.Text = "Minimum Width";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(12, 12);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(49, 13);
            this.label47.TabIndex = 81;
            this.label47.Text = "Tracking";
            // 
            // pnlTrackingColor
            // 
            this.pnlTrackingColor.Location = new System.Drawing.Point(137, 12);
            this.pnlTrackingColor.Name = "pnlTrackingColor";
            this.pnlTrackingColor.Size = new System.Drawing.Size(22, 17);
            this.pnlTrackingColor.TabIndex = 80;
            this.pnlTrackingColor.Click += new System.EventHandler(this.pnlTrackingColor_Click);
            // 
            // chkKeepEdges
            // 
            this.chkKeepEdges.AutoSize = true;
            this.chkKeepEdges.Checked = true;
            this.chkKeepEdges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkKeepEdges.Location = new System.Drawing.Point(137, 48);
            this.chkKeepEdges.Name = "chkKeepEdges";
            this.chkKeepEdges.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkKeepEdges.Size = new System.Drawing.Size(15, 14);
            this.chkKeepEdges.TabIndex = 79;
            this.chkKeepEdges.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 84;
            this.label1.Text = "Keep Edges";
            // 
            // numWidth
            // 
            this.numWidth.Location = new System.Drawing.Point(137, 149);
            this.numWidth.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numWidth.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(56, 20);
            this.numWidth.TabIndex = 85;
            this.numWidth.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numHeight
            // 
            this.numHeight.Location = new System.Drawing.Point(137, 175);
            this.numHeight.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numHeight.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(56, 20);
            this.numHeight.TabIndex = 87;
            this.numHeight.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 177);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 86;
            this.label2.Text = "Minimum Height";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 88;
            this.label3.Text = "Object Tracking Options:";
            // 
            // chkHighlight
            // 
            this.chkHighlight.AutoSize = true;
            this.chkHighlight.Checked = true;
            this.chkHighlight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlight.Location = new System.Drawing.Point(137, 80);
            this.chkHighlight.Name = "chkHighlight";
            this.chkHighlight.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkHighlight.Size = new System.Drawing.Size(15, 14);
            this.chkHighlight.TabIndex = 89;
            this.chkHighlight.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 90;
            this.label4.Text = "Highlight";
            // 
            // ConfigureProcessor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(211, 250);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkHighlight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numHeight);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numWidth);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label48);
            this.Controls.Add(this.label47);
            this.Controls.Add(this.pnlTrackingColor);
            this.Controls.Add(this.chkKeepEdges);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ConfigureProcessor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure";
            this.Load += new System.EventHandler(this.ConfigureProcessorLoad);
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Panel pnlTrackingColor;
        private System.Windows.Forms.CheckBox chkKeepEdges;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColorDialog cdTracking;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkHighlight;
        private System.Windows.Forms.Label label4;
    }
}