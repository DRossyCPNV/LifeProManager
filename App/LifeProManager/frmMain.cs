/// <file>frmMain.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.2.1</version>
/// <date>December 30th, 2021</date>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmMain : Form
    {
        private string resxFile = "";

        private const int LAYOUT_TOPICS = 0;
        private const int LAYOUT_CURRENT_DATE = 1;
        private const int LAYOUT_PLUS_SEVEN_DAYS = 2;
        private const int LAYOUT_DONE = 3;

        private int selectedTask = -1;
        private List<TaskSelections> taskSelection = new List<TaskSelections>();
        private DateTime selectedDateTypeTime;
        private string selectedDate;
        private string[] plusSevenDays = new string[7]; 

        // Declares and instancies a connection to the database
        private DBConnection dbConn = new DBConnection();

        public DateTime SelectedDateTypeTime
        {
            get { return selectedDateTypeTime; }
            set { selectedDateTypeTime = value; }
        }

        public string SelectedDate
        {
            get { return selectedDate; }
            set { selectedDate = value; }
        }

        public frmMain()
        {
            // If it's the app first launch 
            if (dbConn.ReadSetting(1) == 0)
            {
                // If French is detected as the OS language
                if (CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.StartsWith("fr"))
                {
                    // Translates in French every form currently displayed and next forms to be displayed
                    TranslateAppUI(2);
                }

                else
                {
                    // Translates in English every form currently displayed and next forms to be displayed
                    TranslateAppUI(1);
                }
            }

            // If French is set as language to display in settings and app current UI culture is not French
            else if (dbConn.ReadSetting(1) == 2 && System.Threading.Thread.CurrentThread.CurrentUICulture != System.Globalization.CultureInfo.CreateSpecificCulture("fr"))
            {
                // Translates in French every form currently displayed and next forms to be displayed
                TranslateAppUI(2);
            }

            // If English is set as language to display in settings and app current UI culture is not English
            else if (dbConn.ReadSetting(1) == 1 && System.Threading.Thread.CurrentThread.CurrentUICulture != System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))
            {
                // Translates in English every form currently displayed and next forms to be displayed
                TranslateAppUI(1);
            }

            InitializeComponent();
        }
        
        private void frmMain_Load(object sender, EventArgs e)
        {   
            // If the app native language is set on French
            if (dbConn.ReadSetting(1) == 2)
            {
                // Use French resxFile
                resxFile = @".\\stringsFR.resx";
            }
            else
            {
                // By default use English resxFile
                resxFile = @".\\stringsEN.resx";
            }


            using (ResXResourceSet resourceManager = new ResXResourceSet(resxFile))
            {
                // Checks if the database file exists or not
                if (File.Exists(@Environment.CurrentDirectory + "\\" + "LPM_DB" + ".db"))
                {
                    // Checks if the database integrity is valid
                    bool DBvalid = dbConn.CheckDBIntegrity();

                    // If the database is corrupted
                    if (!DBvalid)
                    {
                        MessageBox.Show("Database has been corrupted.\nDatabase will be rebuilt.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dbConn.CreateTablesAndInsertInitialData();
                    }
                }

                // If the database file cannot be found in the application directory
                else
                {
                    MessageBox.Show("Database file could not be found in the application directory.\nA blank file will be created in the application folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dbConn.CreateFile();
                    dbConn.CreateTablesAndInsertInitialData();
                }
                
                // Loads current language of the app in the language selection combobox
                cmbAppLanguage.SelectedIndex = dbConn.ReadSetting(1) - 1;

                // Sets the selected date to today
                selectedDateTypeTime = DateTime.Today;

                // Converts the date to the format used by the database
                selectedDate = DateTime.Today.ToString("yyyy-MM-dd");

                /// <summary>
                /// Resets and fills in the plus seven days date array
                /// </summary>
                plusSevenDays = new string[7];
                for (int i = 0; i < 7; ++i)
                {
                    DateTime dayPlus = DateTime.Today.AddDays(i + 1);
                    String day = dayPlus.ToString();
                    day = day.Substring(6, 4) + "-" + day.Substring(3, 2) + "-" + day.Substring(0, 2);
                    plusSevenDays[i] = day;
                }

                /// <summary>
                /// Loads the topics from the database
                /// </summary>
                LoadTopics();

                /// <summary>
                /// Loads all the tasks for the different tabs from the database
                /// <summary>
                LoadTasks();

                calMonth.ShowToday = false;
                calMonth.MaxSelectionCount = 1;
                lblToday.Text = resourceManager.GetString("today") + " (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
            }
        }

        /// <summary>
        /// Handles the event when the user selects a date in the calendar
        /// </summary>
        private void calMonth_DateChanged(object sender, DateRangeEventArgs e)
        {
            // If the app native language is set on French
            if (dbConn.ReadSetting(1) == 2)
            {
                // Use French resxFile
                resxFile = @".\\stringsFR.resx";
            }
            else
            {
                // By default use English resxFile
                resxFile = @".\\stringsEN.resx";
            }

            using (ResXResourceSet resourceManager = new ResXResourceSet(resxFile))
            {
                if (calMonth.SelectionStart == DateTime.Today.AddDays(-2))
                {
                    lblToday.Text = resourceManager.GetString("twoDaysAgo") + " (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
                }

                else if (calMonth.SelectionStart == DateTime.Today.AddDays(-1))
                {
                    lblToday.Text = resourceManager.GetString("yesterday") + " (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
                }

                else if (calMonth.SelectionStart == DateTime.Today)
                {
                    lblToday.Text = resourceManager.GetString("today") + " (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
                }

                else if (calMonth.SelectionStart == DateTime.Today.AddDays(1))
                {
                    lblToday.Text = resourceManager.GetString("tomorrow") + " (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
                }

                else if (calMonth.SelectionStart == DateTime.Today.AddDays(2))
                {
                    lblToday.Text = resourceManager.GetString("dayAfterTomorrow") + " (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
                }

                else
                { 
                    // If the app native language is French
                    if (dbConn.ReadSetting(1) == 2)
                    {
                        lblToday.Text = calMonth.SelectionStart.ToString("dd-MMM-yyyy");
                    }

                    // If the app native language is in English
                    else
                    {
                        lblToday.Text = calMonth.SelectionStart.ToString("MMM-dd-yyyy");
                    }           
                }

                // Shows the current date tab
                tabMain.SelectTab(tabDates);

                selectedDateTypeTime = calMonth.SelectionStart;

                // Formats the selected date in the calendar for its use in the database
                selectedDate = calMonth.SelectionStart.ToString("yyyy-MM-dd");

                // Hides the panel which contrains the description of tasks if it has been displayed
                if (pnlTaskDescription.Visible)
                {
                    pnlTaskDescription.Visible = false;
                }

                // Loads the tasks for the selected date
                LoadTasksForDate();
            }
        }

        /// <summary>
        /// Handles the setting to run the program at Windows startup
        /// </summary>
        private void chkRunStartUp_CheckedChanged(object sender, EventArgs e)
        {
            // If the user wants to run the program at Windows startup
            if (chkRunStartUp.Checked == true)
            {
                ExecuteCommand("SCHTASKS /CREATE /SC ONSTART /TN LifeProManager /TR Application.ExecutablePath");
            }
            // If the user doesn't want to run the program at Windows startup
            else
            {
                ExecuteCommand("SCHTASKS /DELETE /TN LifeProManager /f");
            }
        }

        /// <summary>
        /// Shows the form to add a task or the form to add a topic if none has been created yet
        /// </summary>
        private void cmdAddTask_Click(object sender, EventArgs e)
        {
            // If no topic has been created yet
            if (cboTopics.Items.Count == 0)
            {
                cmdAddTopic.PerformClick();
            }

            else
            {
                new frmAddTask(this).Show();
            }
        }

        /// <summary>
        /// Closes the database connection when the user quits the program
        /// </summary>
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            dbConn.Close();
        }

        /// <summary>
        /// Checks if the previous topic and next topic arrow buttons should be displayed
        /// </summary>
        private void CheckIfPreviousNextTopicArrowButtonsUseful()
        {
            if (cboTopics.Items.Count <= 1)
            {
                cmdPreviousTopic.Visible = false;
                cmdNextTopic.Visible = false;
            }
            else
            {
                cmdPreviousTopic.Visible = true;
                cmdNextTopic.Visible = true;
            }
        }

        /// <summary>
        /// Shows the form to add a topic when the user clicks on the small plus button, next to the topics drop-down list
        /// </summary>
        private void cmdAddTopic_Click(object sender, EventArgs e)
        {
            new frmAddTopic(this).Show();
        }

        /// <summary>
        /// Deletes the currently displayed topic and all the tasks associated with
        /// </summary>
        private void cmdDeleteTopic_Click(object sender, EventArgs e)
        {
            // If the app native language is set on French
            if (dbConn.ReadSetting(1) == 2)
            {
                // Use French resxFile
                resxFile = @".\\stringsFR.resx";
            }
            else
            {
                // By default use English resxFile
                resxFile = @".\\stringsEN.resx";
            }

            using (ResXResourceSet resourceManager = new ResXResourceSet(resxFile))
            {

                // Gets the selected topic
                Lists currentTopic = cboTopics.SelectedItem as Lists;

                if (cboTopics.Items.Count != 0)
                {
                    var confirmResult = MessageBox.Show(resourceManager.GetString("delTopicWillRemoveRelTasks"), resourceManager.GetString("confirmTheDeletion"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        dbConn.DeleteTopic(currentTopic.Id);

                        // Loads the topics from the database
                        LoadTopics();

                        // Loads all the tasks for the different tabs from the database
                        LoadTasks();

                        // We must empty the bolded dates of the calendar before adding the new ones
                        calMonth.RemoveAllBoldedDates();

                        // Sets the dates of the calendar in bold when there's one or more deadline for a task on a given day
                        SetDatesInBold();

                        // If the drop-down list of topics is empty
                        if (cboTopics.Items.Count == 0)
                        {
                            tabMain.SelectTab(tabDates);
                            cboTopics.Text = resourceManager.GetString("displayByTopic");

                        }
                        else
                        {
                            // Changes current topic since the previous one has been deleted
                            cboTopics.SelectedIndex = 0;
                        }

                        CheckIfPreviousNextTopicArrowButtonsUseful();
                    }
                }
            }
        }


        /// <summary>
        /// Sets the date to the next day when the user clicks on the right arrow button
        /// </summary>
        private void CmdNextDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(1));
        }

        /// <summary>
        /// Shows the tasks for the next topic, from the drop-down list
        /// </summary>
        private void cmdNextTopic_Click(object sender, EventArgs e)
        {
            if (cboTopics.SelectedIndex < cboTopics.Items.Count - 1)
            {
                cboTopics.SelectedIndex += 1;
            }
            else
            {
                cboTopics.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Sets the date to the previous day when the user clicks on the left arrow button
        /// </summary>
        private void CmdPreviousDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(-1));
        }

        /// <summary>
        /// Shows the tasks for the previous topic, from the drop-down list
        /// </summary>
        private void cmdPreviousTopic_Click(object sender, EventArgs e)
        {
            if (cboTopics.SelectedIndex > 0)
            {
                cboTopics.SelectedIndex -= 1;
            }
            else
            {
                cboTopics.SelectedIndex = cboTopics.Items.Count - 1;
            }
        }

        /// <summary>
        /// Shows the tab topics and loads the tasks for the selected topic in the drop-down list
        /// </summary>
        private void cboTopics_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Shows the topic window
            tabMain.SelectTab(tabTopics);

            // Loads the tasks for the selected topic
            LoadTasksInTopic();
        }

        /// <summary>
        /// Localizes the application
        /// Adapated from this source : https://stackoverflow.com/questions/21067507/change-language-at-runtime-in-c-sharp-winform/21068497
        /// </summary>
        private void cmbAppLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (ResXResourceSet resourceManager = new ResXResourceSet(resxFile))
            {
                int idLanguageToApply = cmbAppLanguage.SelectedIndex + 1;

                // If the language used by the app doesn't match the one selected in the combobox
                if (dbConn.ReadSetting(1) != idLanguageToApply)
                {
                    dbConn.UpdateSetting(1, idLanguageToApply);

                    // Translate current form and next form to be loaded in the language selected in the combobox
                    TranslateAppUI(idLanguageToApply);

                    // Restart app to apply language changes
                    Application.Restart();
                }
            }
        }

        /// <summary>
        /// Sets the date to today when the user clicks on the calendar button
        /// </summary>
        private void cmdToday_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(DateTime.Today);
        }

        /// <summary>
        /// Creates the tasks layout to display them to the user
        /// </summary>
        /// <param name="listOfTasks">The list of the tasks to display</param>
        /// <param name="layout">The name of the layout to display in a panel</param>

        public void CreateTasksLayout(List<Tasks> listOfTasks, int layout)
        {
            // Updates task for the current date
            List<Tasks> tasksList = listOfTasks;
            int nbTasks = tasksList.Count();
            int currentTask = 0;

            // Layout
            int lineHeight = 25;
            int iconHeight = 25;
            int iconWidth = 25;
            int spacingWidth = 15;
            int spacingHeight = 25;

            // Clears the desired layout
            switch (layout)
            {
                case LAYOUT_CURRENT_DATE:
                    pnlToday.Controls.Clear();
                    break;

                case LAYOUT_PLUS_SEVEN_DAYS:
                    pnlWeek.Controls.Clear();
                    break;

                case LAYOUT_TOPICS:
                    pnlTopics.Controls.Clear();
                    break;

                case LAYOUT_DONE:
                    tabFinished.Controls.Clear();
                    break;
            }

            foreach (Tasks task in tasksList)
            {
                // Label that displays the title of the current task
                Label lblTask = new Label();

                // Shows a border around a label when the mouse hovers it
                lblTask.MouseEnter += (object sender_here, EventArgs e_here) =>
                {
                    lblTask.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                };

                // Hides the border around a label when the mouse leaves it
                lblTask.MouseLeave += (object sender_here, EventArgs e_here) =>
                {
                    lblTask.BorderStyle = System.Windows.Forms.BorderStyle.None;
                };

                // Handles the event to make a task label in the Actives tab appear selected when the user click on it
                lblTask.Click += (object sender_here, EventArgs e_here) =>
                {
                    selectedTask = task.Id;
                    RefreshSelectedTask();
                };

                // ====================================================================================================
                // Label that displays the validation date on tasks that are done
                Label lblValidationDate = new Label();

                // ====================================================================================================
                // Label that displays the deadline of the current task
                Label lblDeadline = new Label();

                // ====================================================================================================
                // Binds the label to its related task
                TaskSelections taskSelected = new TaskSelections();
                taskSelected.Task_id = task.Id;
                taskSelected.Task_label = lblTask;
                taskSelected.Task_information = task.Description;
                taskSelection.Add(taskSelected);

                // ====================================================================================================
                // Information icon
                PictureBox picInformationIcon = new PictureBox();

                // ====================================================================================================

                Button cmdApproveTask = new Button();
                cmdApproveTask.Click += (object sender_here, EventArgs e_here) =>
                {
                    DateTime today = DateTime.Today;
                    String validationDate = today.ToString();
                    validationDate = validationDate.Substring(6, 4) + "-" + validationDate.Substring(3, 2) + "-" + validationDate.Substring(0, 2);
                    dbConn.ApproveTask(task.Id, validationDate);

                    // Loads all the tasks for the different tabs from the database
                    LoadTasks();

                    // We must empty the bolded dates of the calendar before adding the new ones
                    calMonth.RemoveAllBoldedDates();

                    // Refreshes the calendar bolded dates
                    calMonth.UpdateBoldedDates();

                    // Sets the dates of the calendar in bold when there's one or more deadline for a task on a given day
                    SetDatesInBold();
                };

                // ====================================================================================================

                Button cmdEditTask = new Button();
                cmdEditTask.Click += (object sender_here, EventArgs e_here) =>
                {
                    new frmEditTask(this, task).Show();
                };

                // ====================================================================================================

                Button cmdDeleteTask = new Button();
                cmdDeleteTask.Click += (object sender_here, EventArgs e_here) =>
                {
                    using (ResXResourceSet resourceManager = new ResXResourceSet(resxFile))
                    {
                        // If the app native language is set on French
                        if (dbConn.ReadSetting(1) == 2)
                        {
                            // Use French resxFile
                            resxFile = @".\\stringsFR.resx";
                        }
                        else
                        {
                            // By default use English resxFile
                            resxFile = @".\\stringsEN.resx";
                        }

                        var confirmResult = MessageBox.Show(resourceManager.GetString("areYouSureDeleteTheTask"),
                        resourceManager.GetString("confirmTheDeletion"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (confirmResult == DialogResult.Yes)
                        {
                            dbConn.DeleteTask(task.Id);                   

                            // Loads all the tasks for the different tabs and sets the dates in the calendar in bold, when a task is due for a day.
                            LoadTasks();
                        }
                    }
                };

                // ====================================================================================================

                Button cmdUnapproveTask = new Button();
                cmdUnapproveTask.Click += (object sender_here, EventArgs e_here) =>
                {
                    dbConn.UnapproveTask(task.Id);

                    // Loads all the tasks for the different tabs from the database
                    LoadTasks();
                };

                // ====================================================================================================
                // Information icon detailed layout
                picInformationIcon.Text = "";
                picInformationIcon.Width = iconWidth;
                picInformationIcon.Height = iconHeight;
                picInformationIcon.Location = new Point(20, spacingHeight + currentTask * (lineHeight + spacingWidth) + lineHeight);
                picInformationIcon.BackColor = Color.Transparent;
                if (DateTime.Parse(task.Deadline) < DateTime.Today)
                {
                    picInformationIcon.BackgroundImage = LifeProManager.Properties.Resources.clock;
                }
                else
                {
                    // If a priority has been assigned to this task
                    if (task.Priority_id > 0)
                    {
                        picInformationIcon.BackgroundImage = LifeProManager.Properties.Resources.important;
                    }
                }
                picInformationIcon.BackgroundImageLayout = ImageLayout.Zoom;

                // ====================================================================================================
                // Task label, detailed layout
                lblTask.Text = task.Title;
                lblTask.Width = 590;
                lblTask.Height = lineHeight;
                lblTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                lblTask.TextAlign = ContentAlignment.MiddleLeft;
                lblTask.ForeColor = Color.Black;

                // ====================================================================================================
                // Deadline label, detailed layout
                lblDeadline.Text = task.Deadline.Substring(0, 10);
                lblDeadline.Width = 100;
                lblDeadline.Height = lineHeight;
                lblDeadline.Location = new Point(20 + picInformationIcon.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                lblDeadline.TextAlign = ContentAlignment.MiddleLeft;
                lblDeadline.ForeColor = Color.Black;

                // ====================================================================================================
                // Approve button for this task, detailed layout
                cmdApproveTask.Text = "";
                cmdApproveTask.Width = iconWidth;
                cmdApproveTask.Height = iconHeight;
                cmdApproveTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                cmdApproveTask.BackColor = Color.Transparent;
                cmdApproveTask.FlatAppearance.BorderSize = 0;
                cmdApproveTask.FlatStyle = FlatStyle.Flat;
                cmdApproveTask.BackgroundImage = LifeProManager.Properties.Resources.tick_circle;
                cmdApproveTask.BackgroundImageLayout = ImageLayout.Zoom;

                // ====================================================================================================
                // Edit button for this task, detailed layout
                cmdEditTask.Text = "";
                cmdEditTask.Width = iconWidth;
                cmdEditTask.Height = iconHeight;
                cmdEditTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + cmdApproveTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                cmdEditTask.BackColor = Color.Transparent;
                cmdEditTask.FlatAppearance.BorderSize = 0;
                cmdEditTask.FlatStyle = FlatStyle.Flat;
                cmdEditTask.BackgroundImage = LifeProManager.Properties.Resources.pen_circle;
                cmdEditTask.BackgroundImageLayout = ImageLayout.Zoom;

                // ====================================================================================================
                // Displays the validation date of the task, detailed layout
                lblValidationDate.Width = 100;
                lblValidationDate.Height = lineHeight;
                lblValidationDate.TextAlign = ContentAlignment.MiddleLeft;
                lblValidationDate.BackColor = Color.Transparent;
                lblValidationDate.ForeColor = Color.Black;
                lblValidationDate.BorderStyle = BorderStyle.None;

                // ====================================================================================================
                // Unapprove button for this task, detailed layout
                cmdUnapproveTask.Text = "";
                cmdUnapproveTask.Width = iconWidth;
                cmdUnapproveTask.Height = iconHeight;
                cmdUnapproveTask.Location = new Point(20 + spacingWidth + lblTask.Width + spacingWidth + lblValidationDate.Width, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                cmdUnapproveTask.BackColor = Color.Transparent;
                cmdUnapproveTask.FlatAppearance.BorderSize = 0;
                cmdUnapproveTask.FlatStyle = FlatStyle.Flat;
                cmdUnapproveTask.BackgroundImage = LifeProManager.Properties.Resources.minus_circle;
                cmdUnapproveTask.BackgroundImageLayout = ImageLayout.Zoom;

                // ====================================================================================================
                // Delete button for this task, detailed layout
                cmdDeleteTask.Text = "";
                cmdDeleteTask.Width = iconWidth;
                cmdDeleteTask.Height = iconHeight;
                cmdDeleteTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + cmdApproveTask.Width + spacingWidth + cmdEditTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                cmdDeleteTask.BackColor = Color.Transparent;
                cmdDeleteTask.FlatAppearance.BorderSize = 0;
                cmdDeleteTask.FlatStyle = FlatStyle.Flat;
                cmdDeleteTask.BackgroundImage = LifeProManager.Properties.Resources.delete_circle;
                cmdDeleteTask.BackgroundImageLayout = ImageLayout.Zoom;

                // ====================================================================================================
                // Adds the controls to the desired layout
                switch (layout)
                {
                    case LAYOUT_CURRENT_DATE:
                        // Corrects the layout for the today panel in the date tab
                        cmdApproveTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdEditTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth + cmdApproveTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdDeleteTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth + cmdApproveTask.Width + spacingWidth + cmdEditTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);

                        pnlToday.Controls.Add(picInformationIcon);
                        pnlToday.Controls.Add(lblTask);
                        pnlToday.Controls.Add(cmdApproveTask);
                        pnlToday.Controls.Add(cmdEditTask);
                        pnlToday.Controls.Add(cmdDeleteTask);
                        break;

                    case LAYOUT_PLUS_SEVEN_DAYS:

                        // Corrects the layout for the next seven days panel in the date tab
                        cmdApproveTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdEditTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth + cmdApproveTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdDeleteTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth + cmdApproveTask.Width + spacingWidth + cmdEditTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);

                        pnlWeek.Controls.Add(picInformationIcon);
                        pnlWeek.Controls.Add(lblTask);
                        pnlWeek.Controls.Add(cmdApproveTask);
                        pnlWeek.Controls.Add(cmdEditTask);
                        pnlWeek.Controls.Add(cmdDeleteTask);
                        break;

                    case LAYOUT_TOPICS:
                        // Corrects the layout for the topic tab
                        lblDeadline.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdApproveTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdEditTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth + cmdApproveTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdDeleteTask.Location = new Point(20 + picInformationIcon.Width + spacingWidth + lblTask.Width + spacingWidth + lblDeadline.Width + spacingWidth + cmdApproveTask.Width + spacingWidth + cmdEditTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);

                        pnlTopics.Controls.Add(picInformationIcon);
                        pnlTopics.Controls.Add(lblTask);
                        pnlTopics.Controls.Add(cmdApproveTask);
                        pnlTopics.Controls.Add(cmdEditTask);
                        pnlTopics.Controls.Add(cmdDeleteTask);
                        pnlTopics.Controls.Add(lblDeadline);
                        break;

                    case LAYOUT_DONE:
                        // Corrects the layout for the done tasks tab
                        lblValidationDate.Text = task.ValidationDate.Substring(0, 10);
                        lblTask.Location = new Point(80, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        lblValidationDate.Location = new Point(80 + lblTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdUnapproveTask.Location = new Point(80 + lblTask.Width + spacingWidth + lblValidationDate.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);
                        cmdDeleteTask.Location = new Point(80 + lblTask.Width + spacingWidth + lblValidationDate.Width + spacingWidth + cmdUnapproveTask.Width + spacingWidth, spacingHeight + currentTask * (lblTask.Height + spacingWidth) + lblTask.Height);

                        tabFinished.Controls.Add(lblTask);
                        tabFinished.Controls.Add(lblValidationDate);
                        tabFinished.Controls.Add(cmdUnapproveTask);
                        tabFinished.Controls.Add(cmdDeleteTask);
                        break;
                }

                // ====================================================================================================

                currentTask += 1;
            }
        }

        /// <summary>
        /// Loads a hidden command prompt and executes the command given in argument
        /// </summary>
        /// <param name="command">The command to process</param>
        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);

            // Hides the shell window
            processInfo.CreateNoWindow = true;

            // Starts the process directly from the executable
            processInfo.UseShellExecute = false;

            // Runs a privilege escalation
            processInfo.Verb = "runas";

            var process = Process.Start(processInfo);
            process.WaitForExit();

            // Indicates that the process was run to the end
            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        /// <summary>
        /// Loads all the tasks in the finished tab
        /// </summary>
        public void LoadDoneTasks()
        {
            // Updates tasks that are done
            List<Tasks> tasksList = dbConn.ReadApprovedTask();
            CreateTasksLayout(tasksList, LAYOUT_DONE);
        }

        /// <summary>
        /// Loads all the tasks for the different tabs and sets the dates in the calendar in bold, when a task is due for a day.
        /// </summary>
        public void LoadTasks()
        {
            // Resets the selected task as -1 for none since we don't have any selected task at reload
            selectedTask = -1;

            // Hides the description panel
            pnlTaskDescription.Visible = false;

            LoadTasksForDate();
            LoadTasksForTodayPlusSeven();
            LoadTasksInTopic();
            LoadDoneTasks();

            // We must empty the bolded dates in the calendar before adding the new ones
            calMonth.RemoveAllBoldedDates();
            calMonth.UpdateBoldedDates();
            SetDatesInBold();         
        }

        /// <summary>
        /// Loads all the tasks for today in the dates tab
        /// </summary>
        public void LoadTasksForDate()
        {
            //Update tasks for the current date
            List<Tasks> tasksList = dbConn.ReadTaskForDate(selectedDate);
            CreateTasksLayout(tasksList, LAYOUT_CURRENT_DATE);
        }

        /// <summary>
        /// Loads all the tasks for the next 7 days in the dates tab
        /// </summary>
        public void LoadTasksForTodayPlusSeven()
        {
            //Updates tasks for the next seven days
            List<Tasks> tasksList = dbConn.ReadTaskForDatePlusSeven(plusSevenDays);
            CreateTasksLayout(tasksList, LAYOUT_PLUS_SEVEN_DAYS);
        }

        /// <summary>
        /// Loads all the tasks in the topics tab
        /// </summary>
        public void LoadTasksInTopic()
        {
            // Gets the selected topic
            Lists currentTopic = cboTopics.SelectedItem as Lists;

            // If a topic has been selected in the topic combobox
            if (currentTopic != null)
            {
                // Updates the label
                lblTopic.Text = currentTopic.Title;

                //Updates the tasks for the current topic
                List<Tasks> tasksList = dbConn.ReadTaskForTopic(currentTopic.Id);
                CreateTasksLayout(tasksList, LAYOUT_TOPICS);
            }
        }

       
        /// <summary>
        /// Loads all the topics in the drop-down list on the right panel
        /// </summary>
        public void LoadTopics()
        {
            cboTopics.Items.Clear();
            foreach (Lists topic in dbConn.ReadTopics())
            {
                cboTopics.Items.Add(topic);
                cboTopics.DisplayMember = "Title";
                cboTopics.ValueMember = "Id";
            }
            
            // Checks if previous and next topic arrow buttons should be displayed
            CheckIfPreviousNextTopicArrowButtonsUseful();
        }

        /// <summary>
        /// Changes the background color of the selected task and changes the background to transparent for the unselected tasks
        /// </summary>
        public void RefreshSelectedTask()
        {
            for (int i = 0; i < taskSelection.Count; ++i)
            {
                if (taskSelection[i].Task_id == selectedTask)
                {
                    if (taskSelection[i].Task_label.BackColor == Color.Transparent)
                    {
                        taskSelection[i].Task_label.BackColor = Color.FromArgb(168, 208, 230);

                        if (taskSelection[i].Task_information != "")
                        {
                            lblTaskDescription.Text = taskSelection[i].Task_information;
                            pnlTaskDescription.Visible = true;
                        }

                        else
                        {
                            pnlTaskDescription.Visible = false;
                        }


                    }
                    else
                    {
                        taskSelection[i].Task_label.BackColor = Color.Transparent;
                        lblTaskDescription.Text = "";
                        pnlTaskDescription.Visible = false;
                    }
                }
                else
                {
                    taskSelection[i].Task_label.BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// Sets the dates of the calendar in bold when there's one or more deadline for a task on a given day
        /// </summary>
        private void SetDatesInBold()
        {
            // Copies the content of the list of string returned by the method dbConnReadData into the list of string deadlinesList
            List<string> deadlinesList = new List<string>(dbConn.ReadDataForDeadlines());

            // Browses the list of string and converts each item to DataTime format 
            foreach (string item in deadlinesList)
            {
                DateTime myDateTime = Convert.ToDateTime(item);

                // Adds each DateTime item as a bolded date in the calendar
                calMonth.AddBoldedDate(myDateTime)
            }

            // Refreshes the calendar bolded dates
            calMonth.UpdateBoldedDates();
        }

        /// <summary>
        /// Handles the event when the user selects a tab
        /// </summary>
        private void tabMain_Selected(object sender, TabControlEventArgs e)
        {
            // We reset the selected task as -1 for none, since the user selected another tab
            selectedTask = -1;
            RefreshSelectedTask();
            lblTaskDescription.Text = "";

            // If the user selects the topics tab
            if (tabMain.SelectedTab == tabTopics)
            {
                // If the drop-down list of topics is empty
                if (cboTopics.Items.Count == 0)
                {
                    cmdAddTopic.PerformClick();

                    // Shows the dates tab
                    tabMain.SelectTab(tabDates);
                }

                // If no topic has been selected
                else if (cboTopics.SelectedIndex == -1)
                {
                    // Selects the first one
                    cboTopics.SelectedIndex = 0;
                }

                // If there's only one topic
                if (cboTopics.Items.Count == 1)
                {
                    cmdPreviousTopic.Visible = false;
                    cmdNextTopic.Visible = false;
                }
                else
                {
                    cmdPreviousTopic.Visible = true;
                    cmdNextTopic.Visible = true;
                }
            }
        }

        /// <summary>
        /// Localizes the controls of every form currently displayed and next ones which will be displayed
        /// </summary>
        /// <param name="idLanguageToApply">The id of the language in which the controls must be localized</param>
        public void TranslateAppUI(int idLanguageToApply)
        {
            // Localizes current form and next form(s) which will be loaded in French
            if (idLanguageToApply == 2)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("fr");
                dbConn.UpdateSetting(1, 2);
            }

            // Localizes current form and next form(s) which will be loaded in English
            else
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
                dbConn.UpdateSetting(1, 1);
            }
        }

        private void picAbout_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("Created by Laurent Barraud.\nUse portions of code and UX elements by David Rossy.\nAlpha-versions tested by Julien Terrapon.\n\nDec. 2021, version 1.2.1\n", "About this application", MessageBoxButtons.OK);
        }
    }
}
