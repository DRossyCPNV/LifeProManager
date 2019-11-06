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
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.chkRunStartUp = new System.Windows.Forms.CheckBox();
            this.ilsTabs = new System.Windows.Forms.ImageList(this.components);
            this.calMonth = new System.Windows.Forms.MonthCalendar();
            this.cboTopics = new System.Windows.Forms.ComboBox();
            this.pnlInformations = new System.Windows.Forms.Panel();
            this.lblTaskInformation = new System.Windows.Forms.Label();
            this.cmdNextDay = new System.Windows.Forms.Button();
            this.cmdPreviousDay = new System.Windows.Forms.Button();
            this.cmdAddTopic = new System.Windows.Forms.Button();
            this.cmdAddTask = new System.Windows.Forms.Button();
            this.cmdToday = new System.Windows.Forms.Button();
            this.tabMain.SuspendLayout();
            this.tabDates.SuspendLayout();
            this.tabTopics.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.pnlInformations.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabDates);
            this.tabMain.Controls.Add(this.tabTopics);
            this.tabMain.Controls.Add(this.tabFinished);
            this.tabMain.Controls.Add(this.tabSettings);
            this.tabMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabMain.HotTrack = true;
            this.tabMain.ImageList = this.ilsTabs;
            this.tabMain.ItemSize = new System.Drawing.Size(81, 30);
            this.tabMain.Location = new System.Drawing.Point(13, 21);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(960, 648);
            this.tabMain.TabIndex = 0;
            this.tabMain.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabMain_Selected);
            // 
            // tabDates
            // 
            this.tabDates.Controls.Add(this.pnlWeek);
            this.tabDates.Controls.Add(this.pnlToday);
            this.tabDates.Controls.Add(this.lblWeek);
            this.tabDates.Controls.Add(this.lblToday);
            this.tabDates.ImageKey = "calendar.png";
            this.tabDates.Location = new System.Drawing.Point(4, 34);
            this.tabDates.Name = "tabDates";
            this.tabDates.Padding = new System.Windows.Forms.Padding(3);
            this.tabDates.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tabDates.Size = new System.Drawing.Size(952, 610);
            this.tabDates.TabIndex = 0;
            this.tabDates.Text = "Dates";
            this.tabDates.UseVisualStyleBackColor = true;
            // 
            // pnlWeek
            // 
            this.pnlWeek.Location = new System.Drawing.Point(29, 368);
            this.pnlWeek.Name = "pnlWeek";
            this.pnlWeek.Size = new System.Drawing.Size(863, 218);
            this.pnlWeek.TabIndex = 2;
            // 
            // pnlToday
            // 
            this.pnlToday.Location = new System.Drawing.Point(29, 71);
            this.pnlToday.Name = "pnlToday";
            this.pnlToday.Size = new System.Drawing.Size(863, 246);
            this.pnlToday.TabIndex = 1;
            // 
            // lblWeek
            // 
            this.lblWeek.AutoSize = true;
            this.lblWeek.ForeColor = System.Drawing.Color.Black;
            this.lblWeek.Location = new System.Drawing.Point(52, 333);
            this.lblWeek.Name = "lblWeek";
            this.lblWeek.Size = new System.Drawing.Size(129, 20);
            this.lblWeek.TabIndex = 2;
            this.lblWeek.Text = "7 prochains jours";
            // 
            // lblToday
            // 
            this.lblToday.AutoSize = true;
            this.lblToday.ForeColor = System.Drawing.Color.Black;
            this.lblToday.Location = new System.Drawing.Point(52, 39);
            this.lblToday.Name = "lblToday";
            this.lblToday.Size = new System.Drawing.Size(88, 20);
            this.lblToday.TabIndex = 2;
            this.lblToday.Text = "Aujourd\'hui";
            // 
            // tabTopics
            // 
            this.tabTopics.Controls.Add(this.cmdDeleteTopic);
            this.tabTopics.Controls.Add(this.pnlTopics);
            this.tabTopics.Controls.Add(this.lblTopicsPriority);
            this.tabTopics.Controls.Add(this.cmdNextTopic);
            this.tabTopics.Controls.Add(this.cmdPreviousTopic);
            this.tabTopics.Controls.Add(this.lblTopic);
            this.tabTopics.ImageKey = "topic.png";
            this.tabTopics.Location = new System.Drawing.Point(4, 34);
            this.tabTopics.Name = "tabTopics";
            this.tabTopics.Padding = new System.Windows.Forms.Padding(3);
            this.tabTopics.Size = new System.Drawing.Size(952, 610);
            this.tabTopics.TabIndex = 1;
            this.tabTopics.Text = "Thèmes";
            this.tabTopics.UseVisualStyleBackColor = true;
            // 
            // cmdDeleteTopic
            // 
            this.cmdDeleteTopic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDeleteTopic.BackgroundImage = global::LifeProManager.Properties.Resources.essential_regular_19_close_square;
            this.cmdDeleteTopic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdDeleteTopic.FlatAppearance.BorderSize = 0;
            this.cmdDeleteTopic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdDeleteTopic.Location = new System.Drawing.Point(889, 47);
            this.cmdDeleteTopic.Name = "cmdDeleteTopic";
            this.cmdDeleteTopic.Size = new System.Drawing.Size(21, 21);
            this.cmdDeleteTopic.TabIndex = 13;
            this.cmdDeleteTopic.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cmdDeleteTopic.UseVisualStyleBackColor = false;
            this.cmdDeleteTopic.Click += new System.EventHandler(this.cmdDeleteTopic_Click);
            // 
            // pnlTopics
            // 
            this.pnlTopics.Location = new System.Drawing.Point(46, 104);
            this.pnlTopics.Name = "pnlTopics";
            this.pnlTopics.Size = new System.Drawing.Size(864, 470);
            this.pnlTopics.TabIndex = 14;
            // 
            // lblTopicsPriority
            // 
            this.lblTopicsPriority.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTopicsPriority.Location = new System.Drawing.Point(930, 78);
            this.lblTopicsPriority.Name = "lblTopicsPriority";
            this.lblTopicsPriority.Size = new System.Drawing.Size(56, 29);
            this.lblTopicsPriority.TabIndex = 25;
            this.lblTopicsPriority.Text = "Priorité";
            this.lblTopicsPriority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmdNextTopic
            // 
            this.cmdNextTopic.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_right;
            this.cmdNextTopic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdNextTopic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdNextTopic.FlatAppearance.BorderSize = 0;
            this.cmdNextTopic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdNextTopic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNextTopic.Location = new System.Drawing.Point(617, 45);
            this.cmdNextTopic.Name = "cmdNextTopic";
            this.cmdNextTopic.Size = new System.Drawing.Size(40, 25);
            this.cmdNextTopic.TabIndex = 12;
            this.cmdNextTopic.UseVisualStyleBackColor = true;
            this.cmdNextTopic.Click += new System.EventHandler(this.cmdNextTopic_Click);
            // 
            // cmdPreviousTopic
            // 
            this.cmdPreviousTopic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPreviousTopic.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_left;
            this.cmdPreviousTopic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdPreviousTopic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdPreviousTopic.FlatAppearance.BorderSize = 0;
            this.cmdPreviousTopic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdPreviousTopic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdPreviousTopic.Location = new System.Drawing.Point(298, 45);
            this.cmdPreviousTopic.Name = "cmdPreviousTopic";
            this.cmdPreviousTopic.Size = new System.Drawing.Size(40, 25);
            this.cmdPreviousTopic.TabIndex = 11;
            this.cmdPreviousTopic.UseVisualStyleBackColor = true;
            this.cmdPreviousTopic.Click += new System.EventHandler(this.cmdPreviousTopic_Click);
            // 
            // lblTopic
            // 
            this.lblTopic.ForeColor = System.Drawing.Color.Black;
            this.lblTopic.Location = new System.Drawing.Point(0, 47);
            this.lblTopic.Margin = new System.Windows.Forms.Padding(50);
            this.lblTopic.Name = "lblTopic";
            this.lblTopic.Size = new System.Drawing.Size(952, 20);
            this.lblTopic.TabIndex = 27;
            this.lblTopic.Text = "Thème";
            this.lblTopic.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabFinished
            // 
            this.tabFinished.ImageKey = "validate.png";
            this.tabFinished.Location = new System.Drawing.Point(4, 34);
            this.tabFinished.Name = "tabFinished";
            this.tabFinished.Size = new System.Drawing.Size(952, 610);
            this.tabFinished.TabIndex = 2;
            this.tabFinished.Text = "Terminées";
            this.tabFinished.UseVisualStyleBackColor = true;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.chkRunStartUp);
            this.tabSettings.ImageKey = "settings.png";
            this.tabSettings.Location = new System.Drawing.Point(4, 34);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Size = new System.Drawing.Size(952, 610);
            this.tabSettings.TabIndex = 3;
            this.tabSettings.Text = "Paramètres";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // chkRunStartUp
            // 
            this.chkRunStartUp.AutoSize = true;
            this.chkRunStartUp.ForeColor = System.Drawing.Color.Black;
            this.chkRunStartUp.Location = new System.Drawing.Point(305, 200);
            this.chkRunStartUp.Name = "chkRunStartUp";
            this.chkRunStartUp.Size = new System.Drawing.Size(356, 24);
            this.chkRunStartUp.TabIndex = 15;
            this.chkRunStartUp.Text = "Lancer l\'application au démarrage de Windows";
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
            // calMonth
            // 
            this.calMonth.Cursor = System.Windows.Forms.Cursors.Default;
            this.calMonth.Location = new System.Drawing.Point(1006, 229);
            this.calMonth.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.calMonth.Name = "calMonth";
            this.calMonth.TabIndex = 4;
            this.calMonth.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.calMonth_DateChanged);
            // 
            // cboTopics
            // 
            this.cboTopics.FormattingEnabled = true;
            this.cboTopics.Location = new System.Drawing.Point(1044, 497);
            this.cboTopics.Name = "cboTopics";
            this.cboTopics.Size = new System.Drawing.Size(189, 21);
            this.cboTopics.TabIndex = 9;
            this.cboTopics.Text = "Afficher par thème";
            this.cboTopics.SelectedIndexChanged += new System.EventHandler(this.cboTopics_SelectedIndexChanged);
            // 
            // pnlInformations
            // 
            this.pnlInformations.BackColor = System.Drawing.Color.White;
            this.pnlInformations.Controls.Add(this.lblTaskInformation);
            this.pnlInformations.Location = new System.Drawing.Point(1006, 55);
            this.pnlInformations.Name = "pnlInformations";
            this.pnlInformations.Size = new System.Drawing.Size(227, 130);
            this.pnlInformations.TabIndex = 3;
            // 
            // lblTaskInformation
            // 
            this.lblTaskInformation.AutoSize = true;
            this.lblTaskInformation.ForeColor = System.Drawing.Color.Black;
            this.lblTaskInformation.Location = new System.Drawing.Point(10, 10);
            this.lblTaskInformation.Name = "lblTaskInformation";
            this.lblTaskInformation.Size = new System.Drawing.Size(0, 13);
            this.lblTaskInformation.TabIndex = 11;
            // 
            // cmdNextDay
            // 
            this.cmdNextDay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdNextDay.BackColor = System.Drawing.Color.Transparent;
            this.cmdNextDay.BackgroundImage = global::LifeProManager.Properties.Resources.chevron_right;
            this.cmdNextDay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdNextDay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdNextDay.FlatAppearance.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.cmdNextDay.FlatAppearance.BorderSize = 0;
            this.cmdNextDay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdNextDay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdNextDay.Location = new System.Drawing.Point(1178, 418);
            this.cmdNextDay.Name = "cmdNextDay";
            this.cmdNextDay.Size = new System.Drawing.Size(21, 21);
            this.cmdNextDay.TabIndex = 7;
            this.cmdNextDay.UseVisualStyleBackColor = false;
            this.cmdNextDay.Click += new System.EventHandler(this.CmdNextDay_Click);
            // 
            // cmdPreviousDay
            // 
            this.cmdPreviousDay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdPreviousDay.BackColor = System.Drawing.Color.Transparent;
            this.cmdPreviousDay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdPreviousDay.BackgroundImage")));
            this.cmdPreviousDay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdPreviousDay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdPreviousDay.FlatAppearance.BorderSize = 0;
            this.cmdPreviousDay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdPreviousDay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdPreviousDay.Location = new System.Drawing.Point(1035, 418);
            this.cmdPreviousDay.Name = "cmdPreviousDay";
            this.cmdPreviousDay.Size = new System.Drawing.Size(21, 21);
            this.cmdPreviousDay.TabIndex = 5;
            this.cmdPreviousDay.UseVisualStyleBackColor = false;
            this.cmdPreviousDay.Click += new System.EventHandler(this.CmdPreviousDay_Click);
            // 
            // cmdAddTopic
            // 
            this.cmdAddTopic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddTopic.BackColor = System.Drawing.Color.Transparent;
            this.cmdAddTopic.BackgroundImage = global::LifeProManager.Properties.Resources.plus_square;
            this.cmdAddTopic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdAddTopic.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAddTopic.FlatAppearance.BorderSize = 0;
            this.cmdAddTopic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdAddTopic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAddTopic.Location = new System.Drawing.Point(1014, 497);
            this.cmdAddTopic.Name = "cmdAddTopic";
            this.cmdAddTopic.Size = new System.Drawing.Size(21, 21);
            this.cmdAddTopic.TabIndex = 8;
            this.cmdAddTopic.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdAddTopic.UseVisualStyleBackColor = false;
            this.cmdAddTopic.Click += new System.EventHandler(this.cmdAddTopic_Click);
            // 
            // cmdAddTask
            // 
            this.cmdAddTask.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdAddTask.BackColor = System.Drawing.Color.Transparent;
            this.cmdAddTask.BackgroundImage = global::LifeProManager.Properties.Resources.plus_circle;
            this.cmdAddTask.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdAddTask.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdAddTask.FlatAppearance.BorderSize = 0;
            this.cmdAddTask.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdAddTask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdAddTask.Location = new System.Drawing.Point(1092, 591);
            this.cmdAddTask.Name = "cmdAddTask";
            this.cmdAddTask.Size = new System.Drawing.Size(50, 50);
            this.cmdAddTask.TabIndex = 10;
            this.cmdAddTask.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cmdAddTask.UseVisualStyleBackColor = false;
            this.cmdAddTask.Click += new System.EventHandler(this.cmdAddTask_Click);
            // 
            // cmdToday
            // 
            this.cmdToday.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdToday.BackgroundImage = global::LifeProManager.Properties.Resources.calendar;
            this.cmdToday.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cmdToday.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cmdToday.FlatAppearance.BorderSize = 0;
            this.cmdToday.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.cmdToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdToday.Location = new System.Drawing.Point(1092, 403);
            this.cmdToday.Name = "cmdToday";
            this.cmdToday.Size = new System.Drawing.Size(50, 50);
            this.cmdToday.TabIndex = 6;
            this.cmdToday.UseVisualStyleBackColor = true;
            this.cmdToday.Click += new System.EventHandler(this.cmdToday_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(168)))), ((int)(((byte)(208)))), ((int)(((byte)(230)))));
            this.ClientSize = new System.Drawing.Size(1264, 682);
            this.Controls.Add(this.pnlInformations);
            this.Controls.Add(this.cmdNextDay);
            this.Controls.Add(this.cmdPreviousDay);
            this.Controls.Add(this.cboTopics);
            this.Controls.Add(this.cmdAddTopic);
            this.Controls.Add(this.cmdAddTask);
            this.Controls.Add(this.cmdToday);
            this.Controls.Add(this.calMonth);
            this.Controls.Add(this.tabMain);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Life Pro Manager";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.tabMain.ResumeLayout(false);
            this.tabDates.ResumeLayout(false);
            this.tabDates.PerformLayout();
            this.tabTopics.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.pnlInformations.ResumeLayout(false);
            this.pnlInformations.PerformLayout();
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
        private System.Windows.Forms.MonthCalendar calMonth;
        private System.Windows.Forms.Button cmdAddTask;
        private System.Windows.Forms.ComboBox cboTopics;
        private System.Windows.Forms.Button cmdAddTopic;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabFinished;
        private System.Windows.Forms.Panel pnlTopics;
        private System.Windows.Forms.Panel pnlInformations;
        private System.Windows.Forms.Label lblTopic;
        private System.Windows.Forms.Button cmdDeleteTopic;
        private System.Windows.Forms.ImageList ilsTabs;
        private System.Windows.Forms.Label lblWeek;
        private System.Windows.Forms.Label lblToday;
        private System.Windows.Forms.Panel pnlWeek;
        private System.Windows.Forms.CheckBox chkRunStartUp;
        private System.Windows.Forms.Label lblTaskInformation;
    }
}

