namespace iSpyApplication
{
    partial class AddFloorPlan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddFloorPlan));
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnChooseFile = new System.Windows.Forms.Button();
            this.btnFinish = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.lbObjects = new System.Windows.Forms.ListBox();
            this.ttObject = new System.Windows.Forms.ToolTip(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblResized = new System.Windows.Forms.Label();
            this.llblHelp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtName
            // 
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.Name = "txtName";
            this.txtName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TxtNameKeyUp);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnChooseFile
            // 
            resources.ApplyResources(this.btnChooseFile, "btnChooseFile");
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.UseVisualStyleBackColor = true;
            this.btnChooseFile.Click += new System.EventHandler(this.BtnChooseFileClick);
            // 
            // btnFinish
            // 
            resources.ApplyResources(this.btnFinish, "btnFinish");
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.UseVisualStyleBackColor = true;
            this.btnFinish.Click += new System.EventHandler(this.BtnFinishClick);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // lbObjects
            // 
            this.lbObjects.FormattingEnabled = true;
            resources.ApplyResources(this.lbObjects, "lbObjects");
            this.lbObjects.Name = "lbObjects";
            this.lbObjects.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.LbObjectsQueryContinueDrag);
            this.lbObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LbObjectsMouseDown);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // lblResized
            // 
            resources.ApplyResources(this.lblResized, "lblResized");
            this.lblResized.Name = "lblResized";
            // 
            // llblHelp
            // 
            resources.ApplyResources(this.llblHelp, "llblHelp");
            this.llblHelp.Name = "llblHelp";
            this.llblHelp.TabStop = true;
            this.llblHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblHelp_LinkClicked);
            // 
            // AddFloorPlan
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.llblHelp);
            this.Controls.Add(this.lblResized);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbObjects);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnFinish);
            this.Controls.Add(this.btnChooseFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AddFloorPlan";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddFloorPlan_FormClosing);
            this.Load += new System.EventHandler(this.AddFloorPlanLoad);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AddFloorPlanPaint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnChooseFile;
        private System.Windows.Forms.Button btnFinish;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lbObjects;
        private System.Windows.Forms.ToolTip ttObject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblResized;
        private System.Windows.Forms.LinkLabel llblHelp;
    }
}