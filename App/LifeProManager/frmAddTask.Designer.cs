namespace LifeProManager
{
    partial class frmAddTask
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddTask));
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblPriority = new System.Windows.Forms.Label();
            this.lblDeadline = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTopic = new System.Windows.Forms.Label();
            this.cboTopics = new System.Windows.Forms.ComboBox();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdConfirm = new System.Windows.Forms.Button();
            this.dtpDeadline = new System.Windows.Forms.DateTimePicker();
            this.chkImportant = new System.Windows.Forms.CheckBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.chkRepeatable = new System.Windows.Forms.CheckBox();
            this.chkBirthday = new System.Windows.Forms.CheckBox();
            this.lblYear = new System.Windows.Forms.Label();
            this.numYear = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numYear)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.ForeColor = System.Drawing.Color.Black;
            this.lblDescription.Name = "lblDescription";
            // 
            // lblPriority
            // 
            resources.ApplyResources(this.lblPriority, "lblPriority");
            this.lblPriority.ForeColor = System.Drawing.Color.Black;
            this.lblPriority.Name = "lblPriority";
            // 
            // lblDeadline
            // 
            resources.ApplyResources(this.lblDeadline, "lblDeadline");
            this.lblDeadline.ForeColor = System.Drawing.Color.Black;
            this.lblDeadline.Name = "lblDeadline";
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.ForeColor = System.Drawing.Color.Black;
            this.lblTitle.Name = "lblTitle";
            // 
            // lblTopic
            // 
            resources.ApplyResources(this.lblTopic, "lblTopic");
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            this.lblTopic.Name = "lblTopic";
            // 
            // cboTopics
            // 
            this.cboTopics.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTopics.FormattingEnabled = true;
            resources.ApplyResources(this.cboTopics, "cboTopics");
            this.cboTopics.Name = "cboTopics";
            // 
            // txtTitle
            // 
            this.txtTitle.BackColor = System.Drawing.SystemColors.Window;
            this.txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtTitle, "txtTitle");
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.TextChanged += new System.EventHandler(this.txtTitle_TextChanged);
            // 
            // cmdCancel
            // 
            this.cmdCancel.BackColor = System.Drawing.Color.Transparent;
            this.cmdCancel.BackgroundImage = global::LifeProManager.Properties.Resources.cancel;
            resources.ApplyResources(this.cmdCancel, "cmdCancel");
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = false;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdConfirm
            // 
            resources.ApplyResources(this.cmdConfirm, "cmdConfirm");
            this.cmdConfirm.BackColor = System.Drawing.Color.Transparent;
            this.cmdConfirm.BackgroundImage = global::LifeProManager.Properties.Resources.validate;
            this.cmdConfirm.FlatAppearance.BorderSize = 0;
            this.cmdConfirm.Name = "cmdConfirm";
            this.cmdConfirm.UseVisualStyleBackColor = false;
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
            this.chkImportant.BackColor = System.Drawing.Color.Transparent;
            this.chkImportant.Name = "chkImportant";
            this.chkImportant.UseVisualStyleBackColor = false;
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.SystemColors.Window;
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtDescription, "txtDescription");
            this.txtDescription.Name = "txtDescription";
            // 
            // chkRepeatable
            // 
            resources.ApplyResources(this.chkRepeatable, "chkRepeatable");
            this.chkRepeatable.BackColor = System.Drawing.Color.Transparent;
            this.chkRepeatable.Name = "chkRepeatable";
            this.chkRepeatable.UseVisualStyleBackColor = false;
            // 
            // chkBirthday
            // 
            resources.ApplyResources(this.chkBirthday, "chkBirthday");
            this.chkBirthday.BackColor = System.Drawing.Color.Transparent;
            this.chkBirthday.Name = "chkBirthday";
            this.chkBirthday.UseVisualStyleBackColor = false;
            this.chkBirthday.CheckedChanged += new System.EventHandler(this.chkBirthday_CheckedChanged);
            // 
            // lblYear
            // 
            resources.ApplyResources(this.lblYear, "lblYear");
            this.lblYear.Name = "lblYear";
            // 
            // numYear
            // 
            resources.ApplyResources(this.numYear, "numYear");
            this.numYear.Maximum = new decimal(new int[] {
            2022,
            0,
            0,
            0});
            this.numYear.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.numYear.Name = "numYear";
            this.numYear.Value = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            // 
            // frmAddTask
            // 
            this.AcceptButton = this.cmdConfirm;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(235)))), ((int)(((byte)(239)))));
            this.CancelButton = this.cmdCancel;
            this.Controls.Add(this.lblYear);
            this.Controls.Add(this.numYear);
            this.Controls.Add(this.chkBirthday);
            this.Controls.Add(this.chkRepeatable);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.chkImportant);
            this.Controls.Add(this.dtpDeadline);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.cboTopics);
            this.Controls.Add(this.lblTopic);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdConfirm);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblDeadline);
            this.Controls.Add(this.lblPriority);
            this.Controls.Add(this.lblDescription);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAddTask";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmAddTask_FormClosing);
            this.Load += new System.EventHandler(this.frmAddTask_Load);
            this.Move += new System.EventHandler(this.frmAddTask_Move);
            ((System.ComponentModel.ISupportInitialize)(this.numYear)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.Label lblDeadline;
        private System.Windows.Forms.Button cmdConfirm;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.Label lblTopic;
        private System.Windows.Forms.ComboBox cboTopics;
        public System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.DateTimePicker dtpDeadline;
        private System.Windows.Forms.CheckBox chkImportant;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.CheckBox chkRepeatable;
        private System.Windows.Forms.CheckBox chkBirthday;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.NumericUpDown numYear;
    }
}