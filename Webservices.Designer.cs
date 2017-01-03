namespace iSpyApplication
{
    partial class Webservices
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.Next = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lbIPv4Address = new System.Windows.Forms.ListBox();
            this.chkReroute = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.lblIPAddresses = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.llblHelp = new System.Windows.Forms.LinkLabel();
            this.tcIPMode = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtLANPort = new System.Windows.Forms.NumericUpDown();
            this.chkuPNP = new System.Windows.Forms.CheckBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.ddlPort = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtPort = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.lbIPv6Address = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.btnTroubleshooting = new System.Windows.Forms.Button();
            this.tcIPMode.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtLANPort)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label1.Location = new System.Drawing.Point(51, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.Location = new System.Drawing.Point(51, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(144, 73);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(100, 20);
            this.txtUsername.TabIndex = 2;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(144, 99);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(100, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(337, 40);
            this.label4.TabIndex = 7;
            this.label4.Text = "To view your recorded and live content locally and remotely you need \r\nto configu" +
                "re the built in web server. \r\n";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(141, 122);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(112, 13);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Create a new account";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1LinkClicked);
            // 
            // Next
            // 
            this.Next.Location = new System.Drawing.Point(283, 457);
            this.Next.Name = "Next";
            this.Next.Size = new System.Drawing.Size(67, 23);
            this.Next.TabIndex = 5;
            this.Next.Text = "Finish";
            this.Next.UseVisualStyleBackColor = true;
            this.Next.Click += new System.EventHandler(this.Button1Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.Location = new System.Drawing.Point(156, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "WAN Port";
            this.toolTip1.SetToolTip(this.label3, "This is the port that is accessible externally (from the internet)");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 12);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 13);
            this.label10.TabIndex = 39;
            this.label10.Text = "LAN Port";
            this.toolTip1.SetToolTip(this.label10, "This is the port that is accessible internally over your LAN");
            // 
            // lbIPv4Address
            // 
            this.lbIPv4Address.FormattingEnabled = true;
            this.lbIPv4Address.Location = new System.Drawing.Point(12, 36);
            this.lbIPv4Address.Name = "lbIPv4Address";
            this.lbIPv4Address.Size = new System.Drawing.Size(317, 56);
            this.lbIPv4Address.TabIndex = 44;
            this.toolTip1.SetToolTip(this.lbIPv4Address, "Select the IP address you want to use");
            this.lbIPv4Address.SelectedIndexChanged += new System.EventHandler(this.lbIPv4Address_SelectedIndexChanged);
            // 
            // chkReroute
            // 
            this.chkReroute.AutoSize = true;
            this.chkReroute.Location = new System.Drawing.Point(10, 121);
            this.chkReroute.Name = "chkReroute";
            this.chkReroute.Size = new System.Drawing.Size(97, 17);
            this.chkReroute.TabIndex = 49;
            this.chkReroute.Text = "DHCP Reroute";
            this.toolTip1.SetToolTip(this.chkReroute, "iSpy can monitor your connection and re-configure your router if your LAN IP addr" +
                    "ess changes");
            this.chkReroute.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(26, 13);
            this.label8.TabIndex = 47;
            this.label8.Text = "Port";
            this.toolTip1.SetToolTip(this.label8, "This is the port that is accessible internally over your LAN");
            // 
            // lblIPAddresses
            // 
            this.lblIPAddresses.AutoSize = true;
            this.lblIPAddresses.Location = new System.Drawing.Point(12, 194);
            this.lblIPAddresses.Name = "lblIPAddresses";
            this.lblIPAddresses.Size = new System.Drawing.Size(49, 13);
            this.lblIPAddresses.TabIndex = 29;
            this.lblIPAddresses.Text = "Public IP";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(339, 50);
            this.label6.TabIndex = 28;
            this.label6.Text = "To access your cameras, microphones and captured content over the web or with mob" +
                "ile devices and to use iSpy alerting services  you will need an iSpy Connect acc" +
                "ount.";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // llblHelp
            // 
            this.llblHelp.AutoSize = true;
            this.llblHelp.Location = new System.Drawing.Point(12, 462);
            this.llblHelp.Name = "llblHelp";
            this.llblHelp.Size = new System.Drawing.Size(29, 13);
            this.llblHelp.TabIndex = 62;
            this.llblHelp.TabStop = true;
            this.llblHelp.Text = "Help";
            this.llblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblHelp_LinkClicked);
            // 
            // tcIPMode
            // 
            this.tcIPMode.Controls.Add(this.tabPage1);
            this.tcIPMode.Controls.Add(this.tabPage2);
            this.tcIPMode.Location = new System.Drawing.Point(11, 215);
            this.tcIPMode.Name = "tcIPMode";
            this.tcIPMode.SelectedIndex = 0;
            this.tcIPMode.Size = new System.Drawing.Size(343, 195);
            this.tcIPMode.TabIndex = 45;
            this.tcIPMode.SelectedIndexChanged += new System.EventHandler(this.tcIPMode_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtLANPort);
            this.tabPage1.Controls.Add(this.chkReroute);
            this.tabPage1.Controls.Add(this.lbIPv4Address);
            this.tabPage1.Controls.Add(this.chkuPNP);
            this.tabPage1.Controls.Add(this.linkLabel2);
            this.tabPage1.Controls.Add(this.ddlPort);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(335, 169);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "UPNP (IPv4)";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtLANPort
            // 
            this.txtLANPort.Location = new System.Drawing.Point(76, 10);
            this.txtLANPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtLANPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtLANPort.Name = "txtLANPort";
            this.txtLANPort.Size = new System.Drawing.Size(74, 20);
            this.txtLANPort.TabIndex = 50;
            this.txtLANPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // chkuPNP
            // 
            this.chkuPNP.AutoSize = true;
            this.chkuPNP.Location = new System.Drawing.Point(10, 98);
            this.chkuPNP.Name = "chkuPNP";
            this.chkuPNP.Size = new System.Drawing.Size(150, 17);
            this.chkuPNP.TabIndex = 43;
            this.chkuPNP.Text = "Auto configure with UPNP";
            this.chkuPNP.UseVisualStyleBackColor = true;
            this.chkuPNP.CheckedChanged += new System.EventHandler(this.ChkuPnpCheckedChanged);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(7, 144);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(172, 13);
            this.linkLabel2.TabIndex = 42;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "... or manually configure your router";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2LinkClicked);
            // 
            // ddlPort
            // 
            this.ddlPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlPort.FormattingEnabled = true;
            this.ddlPort.Location = new System.Drawing.Point(226, 9);
            this.ddlPort.Name = "ddlPort";
            this.ddlPort.Size = new System.Drawing.Size(82, 21);
            this.ddlPort.TabIndex = 32;
            this.ddlPort.SelectedIndexChanged += new System.EventHandler(this.ddlPort_SelectedIndexChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtPort);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.lbIPv6Address);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(335, 169);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Tunneling (IPv6)";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(76, 10);
            this.txtPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(74, 20);
            this.txtPort.TabIndex = 51;
            this.txtPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(6, 118);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(321, 48);
            this.label7.TabIndex = 46;
            this.label7.Text = "Using IPv6 iSpy *might* be able to configure your NAT\r\nautomatically";
            // 
            // lbIPv6Address
            // 
            this.lbIPv6Address.FormattingEnabled = true;
            this.lbIPv6Address.Location = new System.Drawing.Point(9, 36);
            this.lbIPv6Address.Name = "lbIPv6Address";
            this.lbIPv6Address.Size = new System.Drawing.Size(320, 69);
            this.lbIPv6Address.TabIndex = 45;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 413);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(330, 41);
            this.label5.TabIndex = 41;
            this.label5.Text = "If you are connecting multiple instances of iSpy, you must select a\r\ndifferent po" +
                "rt combination for each instance. ";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(71, 457);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 31;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2Click);
            // 
            // btnTroubleshooting
            // 
            this.btnTroubleshooting.Location = new System.Drawing.Point(152, 457);
            this.btnTroubleshooting.Name = "btnTroubleshooting";
            this.btnTroubleshooting.Size = new System.Drawing.Size(125, 23);
            this.btnTroubleshooting.TabIndex = 64;
            this.btnTroubleshooting.Text = "Troubleshooter";
            this.btnTroubleshooting.UseVisualStyleBackColor = true;
            this.btnTroubleshooting.Click += new System.EventHandler(this.button1_Click);
            // 
            // Webservices
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(366, 503);
            this.ControlBox = false;
            this.Controls.Add(this.btnTroubleshooting);
            this.Controls.Add(this.llblHelp);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tcIPMode);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblIPAddresses);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Next);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.button2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Webservices";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Web Access";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(1)))), ((int)(((byte)(1)))));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Webservices_FormClosing);
            this.Load += new System.EventHandler(this.WebservicesLoad);
            this.tcIPMode.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtLANPort)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button Next;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblIPAddresses;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.CheckBox chkuPNP;
        private System.Windows.Forms.TabControl tcIPMode;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox lbIPv4Address;
        private System.Windows.Forms.ListBox lbIPv6Address;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkReroute;
        private System.Windows.Forms.LinkLabel llblHelp;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown txtLANPort;
        private System.Windows.Forms.NumericUpDown txtPort;
        private System.Windows.Forms.ComboBox ddlPort;
        private System.Windows.Forms.Button btnTroubleshooting;
    }
}