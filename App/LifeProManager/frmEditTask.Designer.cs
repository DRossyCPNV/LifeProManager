namespace LifeProManager
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
            this.cboMonth = new System.Windows.Forms.ComboBox();
            this.cboYear = new System.Windows.Forms.ComboBox();
            this.cboDay = new System.Windows.Forms.ComboBox();
            this.lblDeadline = new System.Windows.Forms.Label();
            this.cboPriorities = new System.Windows.Forms.ComboBox();
            this.lblPriority = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.RichTextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdConfirm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(112, 21);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(129, 20);
            this.txtTitle.TabIndex = 0;
            // 
            // cboTopics
            // 
            this.cboTopics.FormattingEnabled = true;
            this.cboTopics.Location = new System.Drawing.Point(111, 262);
            this.cboTopics.Name = "cboTopics";
            this.cboTopics.Size = new System.Drawing.Size(134, 21);
            this.cboTopics.TabIndex = 6;
            // 
            // lblTopic
            // 
            this.lblTopic.AutoSize = true;
            this.lblTopic.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            this.lblTopic.Location = new System.Drawing.Point(17, 262);
            this.lblTopic.Name = "lblTopic";
            this.lblTopic.Size = new System.Drawing.Size(58, 20);
            this.lblTopic.TabIndex = 32;
            this.lblTopic.Text = "Thème";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.Black;
            this.lblName.Location = new System.Drawing.Point(17, 21);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(42, 20);
            this.lblName.TabIndex = 29;
            this.lblName.Text = "Nom";
            // 
            // cboMonth
            // 
            this.cboMonth.FormattingEnabled = true;
            this.cboMonth.Items.AddRange(new object[] {
            "Janvier",
            "Février",
            "Mars",
            "Avril",
            "Mai",
            "Juin",
            "Juillet",
            "Août",
            "Septembre",
            "Octobre",
            "Novembre",
            "Décembre"});
            this.cboMonth.Location = new System.Drawing.Point(170, 223);
            this.cboMonth.Name = "cboMonth";
            this.cboMonth.Size = new System.Drawing.Size(75, 21);
            this.cboMonth.TabIndex = 4;
            this.cboMonth.Text = "Mois";
            // 
            // cboYear
            // 
            this.cboYear.FormattingEnabled = true;
            this.cboYear.Location = new System.Drawing.Point(251, 223);
            this.cboYear.Name = "cboYear";
            this.cboYear.Size = new System.Drawing.Size(55, 21);
            this.cboYear.TabIndex = 5;
            this.cboYear.Text = "Année";
            // 
            // cboDay
            // 
            this.cboDay.FormattingEnabled = true;
            this.cboDay.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31"});
            this.cboDay.Location = new System.Drawing.Point(112, 223);
            this.cboDay.Name = "cboDay";
            this.cboDay.Size = new System.Drawing.Size(52, 21);
            this.cboDay.TabIndex = 3;
            this.cboDay.Text = "Jour";
            // 
            // lblDeadline
            // 
            this.lblDeadline.AutoSize = true;
            this.lblDeadline.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDeadline.ForeColor = System.Drawing.Color.Black;
            this.lblDeadline.Location = new System.Drawing.Point(17, 223);
            this.lblDeadline.Name = "lblDeadline";
            this.lblDeadline.Size = new System.Drawing.Size(81, 20);
            this.lblDeadline.TabIndex = 25;
            this.lblDeadline.Text = "Echéance";
            // 
            // cboPriorities
            // 
            this.cboPriorities.FormattingEnabled = true;
            this.cboPriorities.Location = new System.Drawing.Point(112, 187);
            this.cboPriorities.Name = "cboPriorities";
            this.cboPriorities.Size = new System.Drawing.Size(133, 21);
            this.cboPriorities.TabIndex = 2;
            // 
            // lblPriority
            // 
            this.lblPriority.AutoSize = true;
            this.lblPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPriority.ForeColor = System.Drawing.Color.Black;
            this.lblPriority.Location = new System.Drawing.Point(17, 187);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new System.Drawing.Size(58, 20);
            this.lblPriority.TabIndex = 23;
            this.lblPriority.Text = "Priorité";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.ForeColor = System.Drawing.Color.Black;
            this.lblDescription.Location = new System.Drawing.Point(17, 52);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(89, 20);
            this.lblDescription.TabIndex = 22;
            this.lblDescription.Text = "Description";
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.txtDescription.Location = new System.Drawing.Point(112, 52);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(194, 119);
            this.txtDescription.TabIndex = 1;
            this.txtDescription.Text = "";
            // 
            // cmdCancel
            // 
            this.cmdCancel.BackgroundImage = global::LifeProManager.Properties.Resources.cancel;
            this.cmdCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdCancel.FlatAppearance.BorderSize = 0;
            this.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdCancel.Location = new System.Drawing.Point(217, 329);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(42, 42);
            this.cmdCancel.TabIndex = 7;
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdConfirm
            // 
            this.cmdConfirm.BackgroundImage = global::LifeProManager.Properties.Resources.validate;
            this.cmdConfirm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdConfirm.FlatAppearance.BorderSize = 0;
            this.cmdConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdConfirm.Location = new System.Drawing.Point(284, 329);
            this.cmdConfirm.Name = "cmdConfirm";
            this.cmdConfirm.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdConfirm.Size = new System.Drawing.Size(42, 42);
            this.cmdConfirm.TabIndex = 8;
            this.cmdConfirm.UseVisualStyleBackColor = true;
            this.cmdConfirm.Click += new System.EventHandler(this.cmdConfirm_Click);
            // 
            // frmEditTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(208)))), ((int)(((byte)(230)))));
            this.ClientSize = new System.Drawing.Size(341, 383);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.cboTopics);
            this.Controls.Add(this.lblTopic);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdConfirm);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cboMonth);
            this.Controls.Add(this.cboYear);
            this.Controls.Add(this.cboDay);
            this.Controls.Add(this.lblDeadline);
            this.Controls.Add(this.cboPriorities);
            this.Controls.Add(this.lblPriority);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmEditTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modifier une tâche";
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
        private System.Windows.Forms.ComboBox cboMonth;
        private System.Windows.Forms.ComboBox cboYear;
        private System.Windows.Forms.ComboBox cboDay;
        private System.Windows.Forms.Label lblDeadline;
        private System.Windows.Forms.ComboBox cboPriorities;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.RichTextBox txtDescription;
    }
}