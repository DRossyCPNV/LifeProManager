﻿namespace LifeProManager
{
    partial class frmEditTask
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEditTask));
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.cboTopics = new System.Windows.Forms.ComboBox();
            this.lblTopic = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDeadline = new System.Windows.Forms.Label();
            this.lblPriority = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.RichTextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdConfirm = new System.Windows.Forms.Button();
            this.dtpDeadline = new System.Windows.Forms.DateTimePicker();
            this.chkImportant = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtTitle
            // 
            resources.ApplyResources(this.txtTitle, "txtTitle");
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.TextChanged += new System.EventHandler(this.txtTitle_TextChanged);
            // 
            // cboTopics
            // 
            resources.ApplyResources(this.cboTopics, "cboTopics");
            this.cboTopics.FormattingEnabled = true;
            this.cboTopics.Name = "cboTopics";
            // 
            // lblTopic
            // 
            resources.ApplyResources(this.lblTopic, "lblTopic");
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            this.lblTopic.Name = "lblTopic";
            // 
            // lblName
            // 
            resources.ApplyResources(this.lblName, "lblName");
            this.lblName.ForeColor = System.Drawing.Color.Black;
            this.lblName.Name = "lblName";
            // 
            // lblDeadline
            // 
            resources.ApplyResources(this.lblDeadline, "lblDeadline");
            this.lblDeadline.ForeColor = System.Drawing.Color.Black;
            this.lblDeadline.Name = "lblDeadline";
            // 
            // lblPriority
            // 
            resources.ApplyResources(this.lblPriority, "lblPriority");
            this.lblPriority.ForeColor = System.Drawing.Color.Black;
            this.lblPriority.Name = "lblPriority";
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.ForeColor = System.Drawing.Color.Black;
            this.lblDescription.Name = "lblDescription";
            // 
            // txtDescription
            // 
            resources.ApplyResources(this.txtDescription, "txtDescription");
            this.txtDescription.BackColor = System.Drawing.Color.White;
            this.txtDescription.Name = "txtDescription";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(this.cmdCancel, "cmdCancel");
            this.cmdCancel.BackgroundImage = global::LifeProManager.Properties.Resources.cancel;
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdConfirm
            // 
            resources.ApplyResources(this.cmdConfirm, "cmdConfirm");
            this.cmdConfirm.BackgroundImage = global::LifeProManager.Properties.Resources.validate;
            this.cmdConfirm.FlatAppearance.BorderSize = 0;
            this.cmdConfirm.Name = "cmdConfirm";
            this.cmdConfirm.UseVisualStyleBackColor = true;
            this.cmdConfirm.Click += new System.EventHandler(this.cmdConfirm_Click);
            // 
            // dtpDeadline
            // 
            resources.ApplyResources(this.dtpDeadline, "dtpDeadline");
            this.dtpDeadline.Name = "dtpDeadline";
            // 
            // chkImportant
            // 
            resources.ApplyResources(this.chkImportant, "chkImportant");
            this.chkImportant.Name = "chkImportant";
            this.chkImportant.UseVisualStyleBackColor = true;
            // 
            // frmEditTask
            // 
            this.AcceptButton = this.cmdConfirm;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(235)))), ((int)(((byte)(239)))));
            this.CancelButton = this.cmdCancel;
            this.Controls.Add(this.chkImportant);
            this.Controls.Add(this.dtpDeadline);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.cboTopics);
            this.Controls.Add(this.lblTopic);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdConfirm);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblDeadline);
            this.Controls.Add(this.lblPriority);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmEditTask";
            this.Load += new System.EventHandler(this.frmEditTask_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.ComboBox cboTopics;
        private System.Windows.Forms.Label lblTopic;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Button cmdConfirm;
        public System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblDeadline;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.RichTextBox txtDescription;
        private System.Windows.Forms.DateTimePicker dtpDeadline;
        private System.Windows.Forms.CheckBox chkImportant;
    }
}