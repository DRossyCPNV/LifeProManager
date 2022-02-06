namespace LifeProManager
{
    partial class frmMain
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabDates = new System.Windows.Forms.TabPage();
            this.pnlWeek = new System.Windows.Forms.Panel();
            this.pnlToday = new System.Windows.Forms.Panel();
            this.lblWeek = new System.Windows.Forms.Label();
            this.lblToday = new System.Windows.Forms.Label();
            this.tabTopics = new System.Windows.Forms.TabPage();
            this.cmdDeleteTopic = new System.Windows.Forms.Button();
            this.pnlTopics = new System.Windows.Forms.Panel();
            this.lblTopicsPriority = new System.Windows.Forms.Label();
            this.cmdNextTopic = new System.Windows.Forms.Button();
            this.cmdPreviousTopic = new System.Windows.Forms.Button();
            this.lblTopic = new System.Windows.Forms.Label();
            this.tabFinished = new System.Windows.Forms.TabPage();
            this.pnlFinished = new System.Windows.Forms.Panel();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.cmbAppLanguage = new System.Windows.Forms.ComboBox();
            this.lblAppInLanguage = new System.Windows.Forms.Label();
            this.chkRunStartUp = new System.Windows.Forms.CheckBox();
            this.ilsTabs = new System.Windows.Forms.ImageList(this.components);
            this.picAbout = new System.Windows.Forms.PictureBox();
            this.calMonth = new System.Windows.Forms.MonthCalendar();
            this.cboTopics = new System.Windows.Forms.ComboBox();
            this.pnlTaskDescription = new System.Windows.Forms.Panel();
            this.lblTaskDescription = new System.Windows.Forms.Label();
            this.cmdNextDay = new System.Windows.Forms.Button();
            this.cmdPreviousDay = new System.Windows.Forms.Button();
            this.cmdAddTopic = new System.Windows.Forms.Button();
            this.cmdAddTask = new System.Windows.Forms.Button();
            this.cmdToday = new System.Windows.Forms.Button();
            this.tabMain.SuspendLayout();
            this.tabDates.SuspendLayout();
            this.tabTopics.SuspendLayout();
            this.tabFinished.SuspendLayout();
            this.tabSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAbout)).BeginInit();
            this.pnlTaskDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabDates);
            this.tabMain.Controls.Add(this.tabTopics);
            this.tabMain.Controls.Add(this.tabFinished);
            this.tabMain.Controls.Add(this.tabSettings);
            resources.ApplyResources(this.tabMain, "tabMain");
            this.tabMain.HotTrack = true;
            this.tabMain.ImageList = this.ilsTabs;
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabMain_Selected);
            // 
            // tabDates
            // 
            this.tabDates.Controls.Add(this.pnlWeek);
            this.tabDates.Controls.Add(this.pnlToday);
            this.tabDates.Controls.Add(this.lblWeek);
            this.tabDates.Controls.Add(this.lblToday);
            resources.ApplyResources(this.tabDates, "tabDates");
            this.tabDates.Name = "tabDates";
            this.tabDates.UseVisualStyleBackColor = true;
            // 
            // pnlWeek
            // 
            resources.ApplyResources(this.pnlWeek, "pnlWeek");
            this.pnlWeek.Name = "pnlWeek";
            // 
            // pnlToday
            // 
            resources.ApplyResources(this.pnlToday, "pnlToday");
            this.pnlToday.Name = "pnlToday";
            // 
            // lblWeek
            // 
            resources.ApplyResources(this.lblWeek, "lblWeek");
            this.lblWeek.ForeColor = System.Drawing.Color.Black;
            this.lblWeek.Name = "lblWeek";
            // 
            // lblToday
            // 
            resources.ApplyResources(this.lblToday, "lblToday");
            this.lblToday.ForeColor = System.Drawing.Color.Black;
            this.lblToday.Name = "lblToday";
            // 
            // tabTopics
            // 
            this.tabTopics.Controls.Add(this.cmdDeleteTopic);
            this.tabTopics.Controls.Add(this.pnlTopics);
            this.tabTopics.Controls.Add(this.lblTopicsPriority);
            this.tabTopics.Controls.Add(this.cmdNextTopic);
            this.tabTopics.Controls.Add(this.cmdPreviousTopic);
            this.tabTopics.Controls.Add(this.lblTopic);
            resources.ApplyResources(this.tabTopics, "tabTopics");
            this.tabTopics.Name = "tabTopics";
            this.tabTopics.UseVisualStyleBackColor = true;
            // 
            // cmdDeleteTopic
            // 
            resources.ApplyResources(this.cmdDeleteTopic, "cmdDeleteTopic");
            this.cmdDeleteTopic.BackgroundImage = global::LifeProManager.Properties.Resources.delete_square;
            this.cmdDeleteTopic.FlatAppearance.BorderSize = 0;
            this.cmdDeleteTopic.Name = "cmdDeleteTopic";
            this.cmdDeleteTopic.UseVisualStyleBackColor = false;
            this.cmdDeleteTopic.Click += new System.EventHandler(this.cmdDeleteTopic_Click);
            // 
            // pnlTopics
            // 
            resources.ApplyResources(this.pnlTopics, "pnlTopics");
            this.pnlTopics.Name = "pnlTopics";
            // 
            // lblTopicsPriority
            // 
            resources.ApplyResources(this.lblTopicsPriority, "lblTopicsPriority");
            this.lblTopicsPriority.Name = "lblTopicsPriority";
            // 
            // cmdNextTopic
            // 
            this.cmdNextTopic.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_right;
            resources.ApplyResources(this.cmdNextTopic, "cmdNextTopic");
            this.cmdNextTopic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdNextTopic.FlatAppearance.BorderSize = 0;
            this.cmdNextTopic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdNextTopic.Name = "cmdNextTopic";
            this.cmdNextTopic.UseVisualStyleBackColor = true;
            this.cmdNextTopic.Click += new System.EventHandler(this.cmdNextTopic_Click);
            // 
            // cmdPreviousTopic
            // 
            resources.ApplyResources(this.cmdPreviousTopic, "cmdPreviousTopic");
            this.cmdPreviousTopic.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_left;
            this.cmdPreviousTopic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdPreviousTopic.FlatAppearance.BorderSize = 0;
            this.cmdPreviousTopic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdPreviousTopic.Name = "cmdPreviousTopic";
            this.cmdPreviousTopic.UseVisualStyleBackColor = true;
            this.cmdPreviousTopic.Click += new System.EventHandler(this.cmdPreviousTopic_Click);
            // 
            // lblTopic
            // 
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.lblTopic, "lblTopic");
            this.lblTopic.Name = "lblTopic";
            // 
            // tabFinished
            // 
            resources.ApplyResources(this.tabFinished, "tabFinished");
            this.tabFinished.Controls.Add(this.pnlFinished);
            this.tabFinished.Name = "tabFinished";
            this.tabFinished.UseVisualStyleBackColor = true;
            // 
            // pnlFinished
            // 
            resources.ApplyResources(this.pnlFinished, "pnlFinished");
            this.pnlFinished.Name = "pnlFinished";
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.cmbAppLanguage);
            this.tabSettings.Controls.Add(this.lblAppInLanguage);
            this.tabSettings.Controls.Add(this.chkRunStartUp);
            resources.ApplyResources(this.tabSettings, "tabSettings");
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // cmbAppLanguage
            // 
            resources.ApplyResources(this.cmbAppLanguage, "cmbAppLanguage");
            this.cmbAppLanguage.FormattingEnabled = true;
            this.cmbAppLanguage.Items.AddRange(new object[] {
            resources.GetString("cmbAppLanguage.Items"),
            resources.GetString("cmbAppLanguage.Items1")});
            this.cmbAppLanguage.Name = "cmbAppLanguage";
            this.cmbAppLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbAppLanguage_SelectedIndexChanged);
            // 
            // lblAppInLanguage
            // 
            resources.ApplyResources(this.lblAppInLanguage, "lblAppInLanguage");
            this.lblAppInLanguage.ForeColor = System.Drawing.Color.Black;
            this.lblAppInLanguage.Name = "lblAppInLanguage";
            // 
            // chkRunStartUp
            // 
            resources.ApplyResources(this.chkRunStartUp, "chkRunStartUp");
            this.chkRunStartUp.ForeColor = System.Drawing.Color.Black;
            this.chkRunStartUp.Name = "chkRunStartUp";
            this.chkRunStartUp.UseVisualStyleBackColor = true;
            this.chkRunStartUp.CheckedChanged += new System.EventHandler(this.chkRunStartUp_CheckedChanged);
            // 
            // ilsTabs
            // 
            this.ilsTabs.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilsTabs.ImageStream")));
            this.ilsTabs.TransparentColor = System.Drawing.Color.Transparent;
            this.ilsTabs.Images.SetKeyName(0, "calendar.png");
            this.ilsTabs.Images.SetKeyName(1, "topic.png");
            this.ilsTabs.Images.SetKeyName(2, "validate.png");
            this.ilsTabs.Images.SetKeyName(3, "settings.png");
            // 
            // picAbout
            // 
            resources.ApplyResources(this.picAbout, "picAbout");
            this.picAbout.Name = "picAbout";
            this.picAbout.TabStop = false;
            this.picAbout.DoubleClick += new System.EventHandler(this.picAbout_DoubleClick);
            // 
            // calMonth
            // 
            this.calMonth.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.calMonth, "calMonth");
            this.calMonth.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.calMonth.Name = "calMonth";
            this.calMonth.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.calMonth_DateChanged);
            // 
            // cboTopics
            // 
            this.cboTopics.FormattingEnabled = true;
            resources.ApplyResources(this.cboTopics, "cboTopics");
            this.cboTopics.Name = "cboTopics";
            this.cboTopics.SelectedIndexChanged += new System.EventHandler(this.cboTopics_SelectedIndexChanged);
            // 
            // pnlTaskDescription
            // 
            resources.ApplyResources(this.pnlTaskDescription, "pnlTaskDescription");
            this.pnlTaskDescription.BackColor = System.Drawing.Color.White;
            this.pnlTaskDescription.Controls.Add(this.lblTaskDescription);
            this.pnlTaskDescription.Name = "pnlTaskDescription";
            // 
            // lblTaskDescription
            // 
            this.lblTaskDescription.ForeColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.lblTaskDescription, "lblTaskDescription");
            this.lblTaskDescription.Name = "lblTaskDescription";
            // 
            // cmdNextDay
            // 
            resources.ApplyResources(this.cmdNextDay, "cmdNextDay");
            this.cmdNextDay.BackColor = System.Drawing.Color.Transparent;
            this.cmdNextDay.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_right;
            this.cmdNextDay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdNextDay.FlatAppearance.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.cmdNextDay.FlatAppearance.BorderSize = 0;
            this.cmdNextDay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdNextDay.Name = "cmdNextDay";
            this.cmdNextDay.UseVisualStyleBackColor = false;
            this.cmdNextDay.Click += new System.EventHandler(this.CmdNextDay_Click);
            // 
            // cmdPreviousDay
            // 
            resources.ApplyResources(this.cmdPreviousDay, "cmdPreviousDay");
            this.cmdPreviousDay.BackColor = System.Drawing.Color.Transparent;
            this.cmdPreviousDay.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_left;
            this.cmdPreviousDay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdPreviousDay.FlatAppearance.BorderSize = 0;
            this.cmdPreviousDay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdPreviousDay.Name = "cmdPreviousDay";
            this.cmdPreviousDay.UseVisualStyleBackColor = false;
            this.cmdPreviousDay.Click += new System.EventHandler(this.CmdPreviousDay_Click);
            // 
            // cmdAddTopic
            // 
            resources.ApplyResources(this.cmdAddTopic, "cmdAddTopic");
            this.cmdAddTopic.BackColor = System.Drawing.Color.Transparent;
            this.cmdAddTopic.BackgroundImage = global::LifeProManager.Properties.Resources.plus_square;
            this.cmdAddTopic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAddTopic.FlatAppearance.BorderSize = 0;
            this.cmdAddTopic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdAddTopic.Name = "cmdAddTopic";
            this.cmdAddTopic.UseVisualStyleBackColor = false;
            this.cmdAddTopic.Click += new System.EventHandler(this.cmdAddTopic_Click);
            // 
            // cmdAddTask
            // 
            resources.ApplyResources(this.cmdAddTask, "cmdAddTask");
            this.cmdAddTask.BackColor = System.Drawing.Color.Transparent;
            this.cmdAddTask.BackgroundImage = global::LifeProManager.Properties.Resources.plus_circle;
            this.cmdAddTask.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAddTask.FlatAppearance.BorderSize = 0;
            this.cmdAddTask.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdAddTask.Name = "cmdAddTask";
            this.cmdAddTask.UseVisualStyleBackColor = false;
            this.cmdAddTask.Click += new System.EventHandler(this.cmdAddTask_Click);
            // 
            // cmdToday
            // 
            resources.ApplyResources(this.cmdToday, "cmdToday");
            this.cmdToday.BackgroundImage = global::LifeProManager.Properties.Resources.calendar_today;
            this.cmdToday.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdToday.FlatAppearance.BorderSize = 0;
            this.cmdToday.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdToday.Name = "cmdToday";
            this.cmdToday.UseVisualStyleBackColor = true;
            this.cmdToday.Click += new System.EventHandler(this.cmdToday_Click);
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(235)))), ((int)(((byte)(239)))));
            this.Controls.Add(this.picAbout);
            this.Controls.Add(this.pnlTaskDescription);
            this.Controls.Add(this.cmdNextDay);
            this.Controls.Add(this.cmdPreviousDay);
            this.Controls.Add(this.cboTopics);
            this.Controls.Add(this.cmdAddTopic);
            this.Controls.Add(this.cmdAddTask);
            this.Controls.Add(this.cmdToday);
            this.Controls.Add(this.calMonth);
            this.Controls.Add(this.tabMain);
            this.ForeColor = System.Drawing.Color.White;
            this.KeyPreview = true;
            this.Name = "frmMain";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.tabMain.ResumeLayout(false);
            this.tabDates.ResumeLayout(false);
            this.tabDates.PerformLayout();
            this.tabTopics.ResumeLayout(false);
            this.tabFinished.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAbout)).EndInit();
            this.pnlTaskDescription.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabDates;
        private System.Windows.Forms.TabPage tabTopics;
        private System.Windows.Forms.Panel pnlToday;
        private System.Windows.Forms.Button cmdNextDay;
        private System.Windows.Forms.Button cmdPreviousDay;
        private System.Windows.Forms.Label lblTopicsPriority;
        private System.Windows.Forms.Button cmdNextTopic;
        private System.Windows.Forms.Button cmdPreviousTopic;
        private System.Windows.Forms.Button cmdToday;
        private System.Windows.Forms.Button cmdAddTask;
        private System.Windows.Forms.Button cmdAddTopic;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabFinished;
        private System.Windows.Forms.Panel pnlTopics;
        private System.Windows.Forms.Panel pnlTaskDescription;
        private System.Windows.Forms.Label lblTopic;
        private System.Windows.Forms.Button cmdDeleteTopic;
        private System.Windows.Forms.ImageList ilsTabs;
        private System.Windows.Forms.Label lblWeek;
        private System.Windows.Forms.Label lblToday;
        private System.Windows.Forms.Panel pnlWeek;
        private System.Windows.Forms.CheckBox chkRunStartUp;
        private System.Windows.Forms.Label lblTaskDescription;
        public System.Windows.Forms.ComboBox cboTopics;
        private System.Windows.Forms.MonthCalendar calMonth;
        private System.Windows.Forms.ComboBox cmbAppLanguage;
        private System.Windows.Forms.Label lblAppInLanguage;
        private System.Windows.Forms.PictureBox picAbout;
        private System.Windows.Forms.Panel pnlFinished;
    }
}

