namespace iSpyApplication
{
    partial class RemoteCommands
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
            this.label45 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label82 = new System.Windows.Forms.Label();
            this.label83 = new System.Windows.Forms.Label();
            this.lbManualAlerts = new System.Windows.Forms.ListBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.gpbSubscriber = new System.Windows.Forms.GroupBox();
            this.txtExecute = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.btnAddCommand = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.llblHelp = new System.Windows.Forms.LinkLabel();
            this.gpbSubscriber.SuspendLayout();
            this.SuspendLayout();
            // 
            // label45
            // 
            this.label45.Location = new System.Drawing.Point(322, 52);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(265, 134);
            this.label45.TabIndex = 83;
            this.label45.Text = "For Example:\r\n\r\nSwitch on and off cameras, Execute a batch file for home automati" +
                "on,\r\nPlay an MP3 to stop your dogs barking, Sound an alarm";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(296, 46);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(40, 23);
            this.button3.TabIndex = 80;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3Click);
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Location = new System.Drawing.Point(21, 20);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(424, 13);
            this.label82.TabIndex = 79;
            this.label82.Text = "You can trigger remote commands manually from the ispy website or from mobile dev" +
                "ices.";
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.Location = new System.Drawing.Point(16, 51);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(68, 13);
            this.label83.TabIndex = 82;
            this.label83.Text = "Execute File:";
            // 
            // lbManualAlerts
            // 
            this.lbManualAlerts.FormattingEnabled = true;
            this.lbManualAlerts.Location = new System.Drawing.Point(24, 52);
            this.lbManualAlerts.Name = "lbManualAlerts";
            this.lbManualAlerts.Size = new System.Drawing.Size(290, 134);
            this.lbManualAlerts.TabIndex = 84;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(239, 192);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 85;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.BtnDeleteClick);
            // 
            // gpbSubscriber
            // 
            this.gpbSubscriber.Controls.Add(this.txtExecute);
            this.gpbSubscriber.Controls.Add(this.label3);
            this.gpbSubscriber.Controls.Add(this.linkLabel1);
            this.gpbSubscriber.Controls.Add(this.btnAddCommand);
            this.gpbSubscriber.Controls.Add(this.txtName);
            this.gpbSubscriber.Controls.Add(this.label1);
            this.gpbSubscriber.Controls.Add(this.button3);
            this.gpbSubscriber.Controls.Add(this.label83);
            this.gpbSubscriber.Location = new System.Drawing.Point(24, 221);
            this.gpbSubscriber.Name = "gpbSubscriber";
            this.gpbSubscriber.Size = new System.Drawing.Size(592, 142);
            this.gpbSubscriber.TabIndex = 86;
            this.gpbSubscriber.TabStop = false;
            this.gpbSubscriber.Text = "New Remote Command";
            // 
            // txtExecute
            // 
            this.txtExecute.Location = new System.Drawing.Point(151, 48);
            this.txtExecute.Name = "txtExecute";
            this.txtExecute.Size = new System.Drawing.Size(139, 20);
            this.txtExecute.TabIndex = 90;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(353, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(233, 72);
            this.label3.TabIndex = 89;
            this.label3.Text = "Name: Friendly name for display\r\n\r\nExecute: File to execute, or iSpy command\r\n";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(148, 79);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(28, 13);
            this.linkLabel1.TabIndex = 85;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Test";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1LinkClicked);
            // 
            // btnAddCommand
            // 
            this.btnAddCommand.Location = new System.Drawing.Point(230, 74);
            this.btnAddCommand.Name = "btnAddCommand";
            this.btnAddCommand.Size = new System.Drawing.Size(60, 23);
            this.btnAddCommand.TabIndex = 84;
            this.btnAddCommand.Text = "Add";
            this.btnAddCommand.UseVisualStyleBackColor = true;
            this.btnAddCommand.Click += new System.EventHandler(this.BtnAddCommandClick);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(151, 17);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(139, 20);
            this.txtName.TabIndex = 83;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(541, 374);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 86;
            this.button1.Text = "Finish";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(21, 374);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(272, 13);
            this.linkLabel4.TabIndex = 89;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "You need an active subscription to enable these options";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel4LinkClicked);
            // 
            // llblHelp
            // 
            this.llblHelp.AutoSize = true;
            this.llblHelp.Location = new System.Drawing.Point(439, 379);
            this.llblHelp.Name = "llblHelp";
            this.llblHelp.Size = new System.Drawing.Size(29, 13);
            this.llblHelp.TabIndex = 90;
            this.llblHelp.TabStop = true;
            this.llblHelp.Text = "Help";
            this.llblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblHelp_LinkClicked);
            // 
            // RemoteCommands
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 408);
            this.Controls.Add(this.llblHelp);
            this.Controls.Add(this.linkLabel4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.gpbSubscriber);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.label45);
            this.Controls.Add(this.lbManualAlerts);
            this.Controls.Add(this.label82);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RemoteCommands";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remote Commands";
            this.Load += new System.EventHandler(this.ManualAlertsLoad);
            this.gpbSubscriber.ResumeLayout(false);
            this.gpbSubscriber.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.Label label83;
        private System.Windows.Forms.ListBox lbManualAlerts;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.GroupBox gpbSubscriber;
        private System.Windows.Forms.Button btnAddCommand;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtExecute;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel llblHelp;
    }
}