/// <file>frmMain.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.1</version>
/// <date>February 24th, 2026</date>

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmMain : Form
    {
        private const int LAYOUT_TOPICS = 0;
        private const int LAYOUT_CURRENT_DATE = 1;
        private const int LAYOUT_PLUS_SEVEN_DAYS = 2;
        private const int LAYOUT_DONE = 3;

        // Allows to copy last task values if it has been set with "repeatable" priority
        private bool copyLastTaskValues = false;

        // Indicates whether the form should play the fade‑in animation when shown.
        private readonly bool _enableFadeIn = false;

        // Timer used to perform the fade‑in animation when the form is shown.
        private System.Windows.Forms.Timer fadeInTimer;

        // Language codes mapped to ComboBox indices
        private readonly string[] _languageCodes = { "en", "fr", "es" };

        // Array to store the next seven days in "yyyy-MM-dd" format for quick access
        private string[] plusSevenDays = new string[7];
        
        private int nbTasksToComplete = 0;

        // Stores the currently selected date in both DateTime and string formats
        private DateTime selectedDateTypeTime;
        private string selectedDate;

        // Stores the ID of the currently selected task
        private int selectedTask = -1;

        private List<TaskSelection> taskSelection = new List<TaskSelection>();

        // Provides access to the global database connection created in Program.cs.
        // This ensures all forms use the same connection instance.
        public DBConnection dbConn => Program.DbConn;

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

        public bool CopyLastTaskValues
        {
            get { return copyLastTaskValues; }
            set { copyLastTaskValues = value; }
        }

        public frmMain(bool enableFadeIn = false)
        {
            InitializeComponent();

            _enableFadeIn = enableFadeIn;

            // Restores window width from user settings.
            RestoreWindowWidthFromSettings();

            // Handles dynamic layout updates when the window is resized.
            this.SizeChanged += frmMain_SizeChanged;

            // Only initializes fade-in if enabled
            if (_enableFadeIn) 
            { 
                InitializeFadeInAnimation(); 
            }
        }

        /// <summary>
        /// This method checks for the existence and integrity of the database,
        /// loads the topics and tasks, and applies the language settings based 
        /// on the user's preferences. 
        /// It also sets up the calendar and other UI elements.
        /// </summary>
        private void frmMain_Load(object sender, EventArgs e)
        {
            LocalizationManager.LoadLocalizedStringsFor(this);

            string currentAppLanguageCode = Properties.Settings.Default.appLanguageCode;
            int indexCurrentAppLanguage = Array.IndexOf(_languageCodes, currentAppLanguageCode);

            if (indexCurrentAppLanguage == -1)
            {
                indexCurrentAppLanguage = 0; // fallback to English
            }

            // Updates the language ComboBox to reflect the current setting
            cboAppLanguage.SelectedIndex = indexCurrentAppLanguage;

            // Sets the selected date to today
            selectedDateTypeTime = DateTime.Today;

            // Converts the date to the format used by the database
            selectedDate = DateTime.Today.ToString("yyyy-MM-dd");

            /// Resets and fills the plus-seven-days date array
            plusSevenDays = new string[7];

            for (int i = 0; i < 7; ++i)
            {
                DateTime dayPlus = DateTime.Today.AddDays(i + 1);

                // Converts the date to yyyy-MM-dd format
                string dayFormatted = dayPlus.ToString("yyyy-MM-dd");

                plusSevenDays[i] = dayFormatted;
            }

            // Reads export mode from application settings
            switch (Properties.Settings.Default.exportMode)
            {
                case 1:
                    {
                        chkDescriptions.Checked = true;
                        break;
                    }

                case 2:
                    {
                        chkTopics.Checked = true;
                        break;
                    }

                case 3:
                    {
                        chkDescriptions.Checked = true;
                        chkTopics.Checked = true;
                        break;
                    }
            }

            // The path to the key where Windows looks for startup applications
            RegistryKey runKeyApp = Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // If a value in the registry is found, the application has been set to run at startup
            if (runKeyApp.GetValue("LifeProManager") != null)
            {
                chkRunAtWindowsStartup.Checked = true;
            }
        }

        /// <summary>
        /// Applies the localized language names to the language selection ComboBox 
        /// and selects the current language.
        /// </summary>
        private void ApplyLanguageComboBoxItems()
        {
            cboAppLanguage.Items.Clear();

            // Adds localized language names in the same order as _languageCodes
            cboAppLanguage.Items.Add(LocalizationManager.GetString("langEnglish"));
            cboAppLanguage.Items.Add(LocalizationManager.GetString("langFrench"));
            cboAppLanguage.Items.Add(LocalizationManager.GetString("langSpanish"));

            // Selects the current language based on its index in _languageCodes
            string currentLanguageCode = LocalizationManager.GetCurrentLanguageCode();

            int index = Array.IndexOf(_languageCodes, currentLanguageCode);

            if (index == -1)
            {
                index = 0; // fallback to English
            }

            cboAppLanguage.SelectedIndex = index;
        }

        /// <summary>
        /// Applies the responsive layout rules to the main window.
        /// </summary>
        private void ApplyResponsiveLayout()
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            // Adjusts the width of the TabControl to keep it aligned with the side panel
            tabMain.Width = this.ClientSize.Width - pnlRight.Width - tabMain.Left - 30;

            // Adjust the internal panels
            pnlToday.Width = tabDates.Width - 60;
            pnlWeek.Width = tabDates.Width - 60;
            pnlTopics.Width = tabMain.Width - 60;
            pnlFinished.Width = tabMain.Width - 60;

            // Adjusts the tasks in each panel
            ResizeTasksInPanel(pnlToday);
            ResizeTasksInPanel(pnlWeek);
            ResizeTasksInPanel(pnlTopics);
            ResizeTasksInPanel(pnlFinished);
        }

        /// <summary>
        /// Asks the user if he/she wants to copy last approved task to repeat it in the future
        /// </summary>
        /// <param name="task">The task that will be copied</param>
        public void AskForCopyingTask(Tasks task)
        {
            // Localized confirmation dialog
            var confirmCopy = MessageBox.Show(LocalizationManager.GetString("repeatTaskAnotherDay"), LocalizationManager.GetString("confirmCopy"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmCopy == DialogResult.Yes)
            {
                // Allows to pre-fill title and description of the task
                copyLastTaskValues = true;

                new frmAddTask(this, task).ShowDialog();
            }
        }

        /// <summary>
        /// Handles the event when the user selects a date in the calendar.
        /// Updates the Today label using localized text and reloads tasks.
        /// </summary>
        private void calMonth_DateChanged(object sender, DateRangeEventArgs e)
        {
            string labelText = GetCurrentDateLabel();
            DateTime selectedDate = calMonth.SelectionStart;

            if (labelText == null)
            {
                // For dates beyond ±2 days: show only the date
                lblToday.Text = selectedDate.ToString("d", CultureInfo.CurrentUICulture);
            }
            else
            {
                // For dates close to today: show currentDateLabel and date
                lblToday.Text = $"{labelText} ({selectedDate:dd-MMM-yyyy})";
            }

            LoadTasks();
        }

        /// <summary>
        /// Handles live application localization when the user selects a new language
        /// from the ComboBox.  
        /// If the selected language differs from the stored one, the UI culture is
        /// updated first, then the main form is recreated so that all resources reload
        /// in the new language without restarting the application.
        /// </summary>
        private void cboAppLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboAppLanguage.SelectedIndex < 0)
            {
                return;
            }

            // Converts the selected index into a language code
            string selectedLanguageCode = _languageCodes[cboAppLanguage.SelectedIndex];

            // Only proceeds if the language has actually changed
            if (Properties.Settings.Default.appLanguageCode != selectedLanguageCode)
            {
                Properties.Settings.Default.appLanguageCode = selectedLanguageCode;
                Properties.Settings.Default.Save();

                // Applies the culture before recreating the form
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguageCode);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguageCode);

                // Applies culture to the custom localization manager
                LocalizationManager.SetLanguage(selectedLanguageCode);
 
                // Creates a new instance of the main form using the updated culture
                frmMain newForm = new frmMain(enableFadeIn: true);

                // Selects the settings tab in the new form
                newForm.SelectSettingsTab();

                // Replaces the current main form without restarting the application
                Program.SwitchMainForm(newForm);
            }
        }

        /// <summary>
        /// Loads the tasks for the selected topic
        /// </summary>
        private void cboTopics_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ignores placeholder
            if (cboTopics.SelectedIndex == -1)
            {
                return;
            }

            LoadTasksInTopic();
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

        private void chkDescriptions_CheckedChanged(object sender, EventArgs e)
        {
            ExportCheckboxesResult();
        }

        private void chkRunAtWindowsStartup_CheckedChanged(object sender, EventArgs e)
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey runKeyApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // The checkbox has been checked by the user and the value in the registry doesn't exist
            if (chkRunAtWindowsStartup.Checked && runKeyApp.GetValue("LifeProManager") == null)
            {
                // Add the value in the registry so that the application runs at startup
                runKeyApp.SetValue("LifeProManager", Application.ExecutablePath);
            }

            // If the checkbox has been unchecked by the user and the application has been set to run at startup
            else if (chkRunAtWindowsStartup.Checked == false && runKeyApp.GetValue("LifeProManager") != null)
            {
                // Remove the value from the registry so that the application doesn't start
                runKeyApp.DeleteValue("LifeProManager", false);
            }
        }

        private void chkTopics_CheckedChanged(object sender, EventArgs e)
        {
            ExportCheckboxesResult();
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
                new frmAddTask(this, null).ShowDialog();
            }
        }

        /// <summary>
        /// Shows the form to add a topic when the user clicks on the small plus button,
        /// next to the topics drop-down list
        /// </summary>
        private void cmdAddTopic_Click(object sender, EventArgs e)
        {
            frmAddTopic addTopicForm = new frmAddTopic(this);
            addTopicForm.ShowDialog();

            LoadTopics();

            // Selects the newly created topic (the last in the list)
            if (cboTopics.Items.Count > 0)
            {
                cboTopics.SelectedIndex = cboTopics.Items.Count - 1;
            }

            // Updates the visibility of the previous/next topic arrow buttons
            CheckIfPreviousNextTopicArrowButtonsUseful();
        }

        /// <summary>
        /// Displays the birthday calendar form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdBirthdayCalendar_Click(object sender, EventArgs e)
        {
            new frmBirthdayCalendar().ShowDialog();
        }

        /// <summary>
        /// Deletes all finished tasks from the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdDeleteFinishedTasks_Click(object sender, EventArgs e)
        {
            dbConn.DeleteAllDoneTasks();
            LoadDoneTasks();
            lblTaskDescription.Visible = false;
            cmdDeleteFinishedTasks.Visible = false;
        }


        /// <summary>
        /// Deletes the currently displayed topic and all the tasks associated with it
        /// </summary>
        private void cmdDeleteTopic_Click(object sender, EventArgs e)
        {
            // Gets the selected topic
            Lists currentTopic = cboTopics.SelectedItem as Lists;

            if (cboTopics.Items.Count == 0)
            {
                return;
            }

            var confirmResult = MessageBox.Show(LocalizationManager.GetString("delTopicWillRemoveRelTasks"),
                LocalizationManager.GetString("confirmDeletion"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                dbConn.DeleteTopic(currentTopic.Id);

                LoadTopics();
                LoadTasks();

                if (cboTopics.Items.Count == 0)
                {
                    tabMain.SelectTab(tabDates);
                    cboTopics.Text = LocalizationManager.GetString("displayByTopic");
                }
                else
                {
                    // Selects first topic after deletion
                    cboTopics.SelectedIndex = 0;
                }

                CheckIfPreviousNextTopicArrowButtonsUseful();
            }
        }

        /// <summary>
        /// Export all data to a web page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdExportToHtml_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = LocalizationManager.GetString("exportHtmlFilter");
            saveFileDialog1.Title = LocalizationManager.GetString("exportHtmlTitle");
            saveFileDialog1.FileName = "LPM-data.html";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string opens it for saving.
            if (saveFileDialog1.FileName != "")
            {
                string stringToWrite = "<html> <head> <style>" +
                "table { font - family: arial, sans - serif;" +
                "border - collapse: collapse;" +
                "width: 100 %;" +
                "}" +
                "td, th {" +
                "border: 1px solid #dddddd;" +
                "text - align: left;" +
                "padding: 8px;" +
                "}" +
                "tr: nth - child(even) {" +
                "background - color: #dddddd;" +
                "}" +
                "</style>" +
                "</head> <body> ";

                stringToWrite += "<table> ";

                List<Tasks> taskListToWrite = new List<Tasks>();
                taskListToWrite = dbConn.ReadTask("WHERE Status_id = 1;");

                List<Lists> taskListsListToWrite = new List<Lists>();
                taskListsListToWrite = dbConn.ReadTopics();

                if (Properties.Settings.Default.exportMode == 0)
                {
                    // Export mode #1
                    foreach (Tasks taskToWrite in taskListToWrite)
                    {
                        stringToWrite += "<tr style ='background-color:#708090;color:#ffffff;'> <th>" + taskToWrite.Deadline.Substring(0, 10) + "</th> </tr>";
                        stringToWrite += "</td>";
                        stringToWrite += "<tr> <td>" + taskToWrite.Title;

                        // If the priority is an odd number
                        if (taskToWrite.Priorities_id % 2 != 0)
                        {
                            stringToWrite += ", Important";
                        }

                        stringToWrite += "</td> </tr>";
                    }
                }

                else if (Properties.Settings.Default.exportMode == 1)
                {
                    // Export mode #2
                    foreach (Tasks taskToWrite in taskListToWrite)
                    {
                        stringToWrite += "<tr style ='background-color:#708090;color:#ffffff;'> <th>" + taskToWrite.Deadline.Substring(0, 10) + "</th> </tr>";
                        stringToWrite += "</td>";
                        stringToWrite += "<tr> <td>" + taskToWrite.Title;

                        // If the priority is an odd number
                        if (taskToWrite.Priorities_id % 2 != 0)
                        {
                            stringToWrite += ", Important";
                        }

                        stringToWrite += "</td> </tr>";
                        stringToWrite += "<td>" + taskToWrite.Description + "</td>";
                        stringToWrite += " </tr>";
                    }
                }

                else if (Properties.Settings.Default.exportMode == 2)
                {
                    // Export mode #3
                    foreach (Tasks taskToWrite in taskListToWrite)
                    {
                        stringToWrite += "<tr style ='background-color:#708090;color:#ffffff;'> <th>" + taskToWrite.Deadline.Substring(0, 10) + "</th> </tr>";
                        stringToWrite += "<tr> <td>" + dbConn.ReadTopicName(taskToWrite.Lists_id);
                        stringToWrite += "</td>";
                        stringToWrite += "<tr> <td>" + taskToWrite.Title;

                        // If the priority is an odd number
                        if (taskToWrite.Priorities_id % 2 != 0)
                        {
                            stringToWrite += ", Important";
                        }

                        stringToWrite += "</td> </tr>";
                    }
                }

                else
                {
                    // Export mode #4
                    foreach (Tasks taskToWrite in taskListToWrite)
                    {
                        stringToWrite += "<tr style ='background-color:#708090;color:#ffffff;'> <th>" + taskToWrite.Deadline.Substring(0, 10) + "</th> </tr>";
                        stringToWrite += "<tr> <td>" + dbConn.ReadTopicName(taskToWrite.Lists_id);

                        // If the priority is an odd number
                        if (taskToWrite.Priorities_id % 2 != 0)
                        {
                            stringToWrite += ", Important";
                        }

                        stringToWrite += "</td>";
                        stringToWrite += "<tr> <td>" + taskToWrite.Title + "</td>";
                        stringToWrite += "<td>" + taskToWrite.Description + "</td>";
                        stringToWrite += " </tr>";
                    }
                }

                stringToWrite += "</table> </body> </html>";

                try
                {
                    // Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter(saveFileDialog1.FileName);

                    sw.WriteLine(stringToWrite);
                    sw.Close();
                }

                catch (Exception exceptionRaised)
                {
                    Console.WriteLine("Exception: " + exceptionRaised.Message);
                }
            }
        }

        /// <summary>
        /// Sets the date to the next day when the user clicks on the right arrow button
        /// </summary>
        private void cmdNextDay_Click(object sender, EventArgs e)
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
        private void cmdPreviousDay_Click(object sender, EventArgs e)
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
        /// Sets the date to today when the user clicks on the calendar button
        /// </summary>
        private void cmdToday_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(DateTime.Today);
        }

        public void CreateTasksLayout(List<Tasks> tasks, int layout)
        {
            // ------------------------------------
            // Layout constants
            // ------------------------------------
            const int ROW_HEIGHT = 32;
            const int ICON_SIZE = 22;
            const int BUTTON_SIZE = 25;
            const int HORIZONTAL_GAP = 10;
            const int VERTICAL_GAP = 12;
            const int RIGHT_PADDING = 15;
            const int DATE_LABEL_WIDTH = 90;

            // ------------------------------------
            // Select target panel
            // ------------------------------------
            Panel targetPanel = null;

            if (layout == LAYOUT_CURRENT_DATE)
            {
                targetPanel = pnlToday;
            }
            else if (layout == LAYOUT_PLUS_SEVEN_DAYS)
            {
                targetPanel = pnlWeek;
            }
            else if (layout == LAYOUT_TOPICS)
            {
                targetPanel = pnlTopics;
            }
            else if (layout == LAYOUT_DONE)
            {
                targetPanel = pnlFinished;
            }
            else
            {
                return;
            }

            if (targetPanel == null)
            {
                return;
            }

            targetPanel.Controls.Clear();
    
            DateTime selectedDateValue = DateTime.Parse(selectedDate);
            int currentY = 10;

            // ------------------------------------
            // Iterate through tasks
            // ------------------------------------
            foreach (var task in tasks)
            {
                // ------------------------------------
                // Parse deadline
                // ------------------------------------
                DateTime deadline;
                if (!DateTime.TryParse(task.Deadline, out deadline))
                {
                    continue;
                }

                deadline = deadline.Date;

                // ------------------------------------
                // Filter tasks depending on layout
                // ------------------------------------
                if (layout == LAYOUT_CURRENT_DATE)
                {
                    if (deadline > selectedDateValue.Date)
                    {
                        continue;
                    }
                }
                else if (layout == LAYOUT_PLUS_SEVEN_DAYS)
                {
                    DateTime today = selectedDateValue.Date;
                    DateTime sevenDaysLater = today.AddDays(7);

                    if (!(deadline > today && deadline <= sevenDaysLater))
                    {
                        continue;
                    }
                }

                // ------------------------------------
                // Create row panel (container)
                // ------------------------------------
                Panel rowPanel = new Panel
                {
                    Left = 10,
                    Top = currentY,
                    Width = targetPanel.Width - 20,
                    Height = ROW_HEIGHT,
                    BackColor = Color.Transparent
                };

                // ------------------------------------
                // RIGHT PANEL (date + buttons)
                // ------------------------------------
                Panel rightPanel = new Panel
                {
                    Width = DATE_LABEL_WIDTH + (BUTTON_SIZE + HORIZONTAL_GAP) * 3 + RIGHT_PADDING,
                    Height = ROW_HEIGHT,
                    Left = rowPanel.Width - (DATE_LABEL_WIDTH + (BUTTON_SIZE + HORIZONTAL_GAP) * 3 + RIGHT_PADDING),
                    Top = 0,
                    BackColor = Color.Transparent
                };

                // Date label
                Label lblDate = new Label
                {
                    Left = 0,
                    Top = 0,
                    Width = DATE_LABEL_WIDTH,
                    Height = ROW_HEIGHT,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Black
                };

                if (layout == LAYOUT_TOPICS)
                {
                    lblDate.Text = deadline.ToString("yyyy-MM-dd");
                }
                else if (layout == LAYOUT_DONE && !string.IsNullOrEmpty(task.ValidationDate))
                {
                    lblDate.Text = task.ValidationDate.Substring(0, 10);
                }

                // ------------------------------------
                // Button factory method
                // ------------------------------------
                Button CreateButton(Image img)
                {
                    Button btn = new Button();
                    btn.Size = new Size(BUTTON_SIZE, BUTTON_SIZE);
                    btn.BackgroundImage = img;
                    btn.BackgroundImageLayout = ImageLayout.Zoom;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = Color.Transparent;
                    btn.Top = (ROW_HEIGHT - BUTTON_SIZE) / 2;
                    return btn;
                }

                Button btnApprove = CreateButton(Properties.Resources.tick_circle);
                Button btnEdit = CreateButton(Properties.Resources.pen_circle);
                Button btnDelete = CreateButton(Properties.Resources.delete_circle);
                Button btnUnapprove = CreateButton(Properties.Resources.minus_circle);

                // ------------------------------------
                // Button click events
                // ------------------------------------

                // Approve task
                btnApprove.Click += (s, e) =>
                {
                    string validationDate = DateTime.Today.ToString("yyyy-MM-dd");
                    dbConn.ApproveTask(task.Id, validationDate);
                    LoadTasks();

                    if (task.Priorities_id >= 2)
                    {
                        AskForCopyingTask(task);
                    }
                };

                // Edit task
                btnEdit.Click += (s, e) =>
                {
                    new frmEditTask(this, task).ShowDialog();
                };

                // Delete task
                btnDelete.Click += (s, e) =>
                {
                    DialogResult result = MessageBox.Show(
                        LocalizationManager.GetString("areYouSureDeleteTheTask"),
                        LocalizationManager.GetString("confirmDeletion"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        dbConn.DeleteTask(task.Id);
                        LoadTasks();
                    }
                };

                // Unapprove task (in DONE layout)
                btnUnapprove.Click += (s, e) =>
                {
                    dbConn.UnapproveTask(task.Id);
                    LoadTasks();
                };

                // ------------------------------------
                // Position buttons depending on layout
                // ------------------------------------
                int xPos = DATE_LABEL_WIDTH + HORIZONTAL_GAP;

                if (layout == LAYOUT_DONE)
                {
                    btnUnapprove.Left = xPos;
                    btnDelete.Left = xPos + BUTTON_SIZE + HORIZONTAL_GAP;

                    rightPanel.Controls.Add(btnUnapprove);
                    rightPanel.Controls.Add(btnDelete);
                }
                else
                {
                    btnApprove.Left = xPos;
                    btnEdit.Left = xPos + BUTTON_SIZE + HORIZONTAL_GAP;
                    btnDelete.Left = xPos + 2 * (BUTTON_SIZE + HORIZONTAL_GAP);

                    rightPanel.Controls.Add(btnApprove);
                    rightPanel.Controls.Add(btnEdit);
                    rightPanel.Controls.Add(btnDelete);
                }

                rightPanel.Controls.Add(lblDate);

                // ------------------------------------
                // LEFT PANEL (icon + title)
                // ------------------------------------
                Panel leftPanel = new Panel
                {
                    Left = 0,
                    Top = 0,
                    Width = rightPanel.Left - 5,
                    Height = ROW_HEIGHT,
                    BackColor = Color.Transparent
                };

                PictureBox iconBox = new PictureBox
                {
                    Size = new Size(ICON_SIZE, ICON_SIZE),
                    Left = 0,
                    Top = (ROW_HEIGHT - ICON_SIZE) / 2,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    BackColor = Color.Transparent
                };

                if (deadline < selectedDateValue.Date)
                {
                    iconBox.BackgroundImage = Properties.Resources.clock;
                }
                else if (task.Priorities_id == 4)
                {
                    iconBox.BackgroundImage = Properties.Resources.birthday_cake_small;
                }
                else if (task.Priorities_id % 2 != 0)
                {
                    iconBox.BackgroundImage = Properties.Resources.important;
                }

                Label lblTitle = new Label
                {
                    Left = ICON_SIZE + HORIZONTAL_GAP,
                    Top = 0,
                    Width = leftPanel.Width - (ICON_SIZE + HORIZONTAL_GAP),
                    Height = ROW_HEIGHT,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 11)
                };

                if (task.Priorities_id == 4 && int.TryParse(task.Description, out int birthYear))
                {
                    int age = DateTime.Now.Year - birthYear;
                    lblTitle.Text = task.Title + " (" + age + ")";
                }
                else
                {
                    lblTitle.Text = task.Title;
                }

                // Click events
                lblTitle.Click += (s, e) =>
                {
                    selectedTask = task.Id;
                    RefreshSelectedTask();
                };

                lblTitle.DoubleClick += (s, e) =>
                {
                    new frmEditTask(this, task).ShowDialog();
                };

                // ------------------------------------
                // Add panels to row
                // ------------------------------------
                rowPanel.Controls.Add(leftPanel);
                rowPanel.Controls.Add(rightPanel);

                // Recalculates left panel and title width after layout
                leftPanel.Width = rightPanel.Left - 5;
                lblTitle.Width = leftPanel.Width - (ICON_SIZE + HORIZONTAL_GAP);

                // Register this task for selection highlighting
                taskSelection.Add(new TaskSelection
                {
                    Task_id = task.Id,
                    Task_label = lblTitle,
                    Task_information = task.Description
                });

                leftPanel.Controls.Add(iconBox);
                leftPanel.Controls.Add(lblTitle);

                // ------------------------------------
                // Add row to target panel
                // ------------------------------------
                targetPanel.Controls.Add(rowPanel);

                currentY += ROW_HEIGHT + VERTICAL_GAP;
            }
        }

        /// <summary>
        /// Calculates which export mode to store in application settings
        /// based on the state of the checkboxes.
        /// </summary>
        private void ExportCheckboxesResult()
        {
            if (chkDescriptions.Checked == true && chkTopics.Checked == true)
            {
                Properties.Settings.Default.exportMode = 3;
            }
            else if (chkDescriptions.Checked == true && chkTopics.Checked == false)
            {
                Properties.Settings.Default.exportMode = 1;
            }
            else if (chkDescriptions.Checked == false && chkTopics.Checked == true)
            {
                Properties.Settings.Default.exportMode = 2;
            }
            else
            {
                // chkDescriptions and chkTopics are both false
                Properties.Settings.Default.exportMode = 0;
            }

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Handles each timer tick during the fade‑in animation by gradually
        /// increasing the form's opacity until it becomes fully visible.
        /// </summary>
        private void FadeInTimer_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1)
            {
                this.Opacity += 0.06;
            }
            else
            {
                fadeInTimer.Stop();
            }
        }

        /// <summary>
        /// Keyboard shortcuts 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Sets the date selection in the calendar on today
            if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
            {
                cmdToday.PerformClick();
            }

            // Adds a new task
            else if (e.KeyCode == Keys.T || e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                cmdAddTask.PerformClick();
            }

            // Displays the birthday calendar
            else if (e.KeyCode == Keys.B && e.Modifiers == Keys.Control)
            {
                cmdBirthdayCalendar.PerformClick();
            }

            // Exports all tasks to a webpage
            else if (e.KeyCode == Keys.E && e.Modifiers == Keys.Control)
            {
                cmdExportToHtml.PerformClick();
            }

            // Selects the previous day on the calendar
            else if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Alt)
            {
                cmdPreviousDay.PerformClick();
            }

            // Selects the next day on the calendar
            else if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Alt)
            {
                cmdNextDay.PerformClick();
            }

            // Selects previous week on the calendar
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(-7));
            }

            // Selects next week on the calendar
            else if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(+7));
            }


            // Keyboard shortcut to delete all the tasks displayed in the finished tab
            else if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Shift)
            {
                if (tabMain.SelectedTab == tabFinished && cmdDeleteFinishedTasks.Visible == true)
                {
                    cmdDeleteFinishedTasks.PerformClick();
                }
            }
        }

        private void frmMain_Layout(object sender, LayoutEventArgs e)
        {
            // Ignores layout events fired before the window handle exists
            if (!this.IsHandleCreated)
            {
                return;
            }

            // Only react when the form itself is being laid out
            if (e.AffectedControl != this)
            {
                return;
            }

            ApplyResponsiveLayout();
        }

        /// <summary>
        /// When the form is shown, loads all the topics and tasks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Shown(object sender, EventArgs e)
        {
            LoadTopics(); 
            LoadTasks();
        }

        /// <summary>
        /// Resizes the tasks width in the different panels when the form is resized 
        /// by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            // Ignores resize events fired before the window handle exists (startup phase)
            if (!this.IsHandleCreated)
            {
                return;
            }

            ApplyResponsiveLayout();

            // Saves window width
            Properties.Settings.Default.WindowWidth = this.Width;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Returns the localized currentDateLabel for the currently selected date
        /// (today, yesterday, tomorrow, etc.).
        /// </summary>
        private string GetCurrentDateLabel()
        {
            DateTime selectedDate = calMonth.SelectionStart;
            DateTime today = DateTime.Today;

            if (selectedDate == today.AddDays(-2))
            {
                return LocalizationManager.GetString("twoDaysAgo");
            }

            if (selectedDate == today.AddDays(-1))
            {
                return LocalizationManager.GetString("yesterday");
            }

            if (selectedDate == today)
            {
                return LocalizationManager.GetString("today");
            }

            if (selectedDate == today.AddDays(1))
            {
                return LocalizationManager.GetString("tomorrow");
            }

            if (selectedDate == today.AddDays(2))
            {
                return LocalizationManager.GetString("dayAfterTomorrow");
            }

            // For dates beyond ±2 days: return null to indicate “just show the date”
            return null;
        }

        /// <summary>
        /// Configures the fade‑in animation used when the form is shown.
        /// </summary>
        private void InitializeFadeInAnimation()
        {
            // Only start transparent if fade‑in is enabled
            if (_enableFadeIn) 
            { 
                this.Opacity = 0; 
            }

            // Timer used to perform the fade‑in animation when the form is shown.
            fadeInTimer = new System.Windows.Forms.Timer();
            fadeInTimer.Interval = 8;
            fadeInTimer.Tick += FadeInTimer_Tick;
        }

        private void lblAppInLanguage_DoubleClick(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
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
        /// Loads all the localized strings for the UI elements based on the current language setting.
        /// </summary>
        public void LoadLocalizedStrings()
        {
            // --- Tabs ---
            tabMain.Text = LocalizationManager.GetString("tabMainText");
            tabTopics.Text = LocalizationManager.GetString("tabTopicsText");
            tabFinished.Text = LocalizationManager.GetString("tabFinishedText");
            tabSettings.Text = LocalizationManager.GetString("tabSettingsText");

            // --- Labels ---
            string label = GetCurrentDateLabel();
            DateTime selectedDate = calMonth.SelectionStart;

            if (label == null)
            {
                // Beyond ±2 days → show only the date
                lblToday.Text = selectedDate.ToString("d", CultureInfo.CurrentUICulture);
            }
            else
            {
                lblToday.Text = $"{label} ({selectedDate:dd-MMM-yyyy})";
            }

            lblWeek.Text = LocalizationManager.GetString("nextDays");
            lblAppInLanguage.Text = LocalizationManager.GetString("appInLanguage");
            lblTopic.Text = LocalizationManager.GetString("topic");
            lblExportDeadlineAndTitle.Text = LocalizationManager.GetString("exportDeadlineAndTitle");
            lblTaskDescription.Text = LocalizationManager.GetString("taskDescription");

            // --- Checkboxes ---
            chkTopics.Text = LocalizationManager.GetString("chkTopicsText");
            chkDescriptions.Text = LocalizationManager.GetString("chkDescriptionsText");
            chkRunAtWindowsStartup.Text = LocalizationManager.GetString("chkRunAtWindowsStartupText");

            // --- ComboBox: Display by topic ---
            cboTopics.Text = LocalizationManager.GetString("displayByTopic");

            // --- ComboBox: language selection ---
            ApplyLanguageComboBoxItems();
        }

        /// <summary>
        /// Reloads all task lists for every tab (Today, Next 7 Days, Topics, Done)
        /// and refreshes the calendar bold dates and task counters.
        /// </summary>
        public void LoadTasks()
        {
            // Resets selected task
            selectedTask = -1;

            // Resets selection list used for task highlighting
            taskSelection.Clear();

            // Hides description panel
            lblTaskDescription.Visible = false;

            // Reloads each layout based on the current reference date
            LoadTasksForDate();
            LoadTasksForTodayPlusSeven();
            LoadTasksInTopic();
            LoadDoneTasks();

            // Refreshes bolded dates in the calendar
            calMonth.RemoveAllBoldedDates();
            calMonth.UpdateBoldedDates();
            SetDatesInBold();

            // Updates total tasks counter
            nbTasksToComplete = dbConn.CountTotalTasksToComplete();

            ttpTotalTasksToComplete.SetToolTip(cmdExportToHtml,
                LocalizationManager.GetString("totalTasksToComplete") + " " + nbTasksToComplete.ToString()
            );
        }

        /// <summary>
        /// Loads all the tasks for today in the dates tab
        /// </summary>
        public void LoadTasksForDate()
        {
            // Updates tasks for the current date
            List<Tasks> tasksList = dbConn.ReadTaskForDate(selectedDate);
            CreateTasksLayout(tasksList, LAYOUT_CURRENT_DATE);
        }

        /// <summary>
        /// Loads all the tasks for the next 7 days in the dates tab
        /// </summary>
        public void LoadTasksForTodayPlusSeven()
        {
            // Updates tasks for the next seven days
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
                // Updates the currentDateLabel
                lblTopic.Text = currentTopic.Title;

                // Updates the tasks for the current topic
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
            }

            cboTopics.DisplayMember = "Title";
            cboTopics.ValueMember = "Id";

            CheckIfPreviousNextTopicArrowButtonsUseful();
        }

        /// <summary>
        /// Structure used by Windows to define minimum and maximum window sizes.
        /// </summary>
        private struct MINMAXINFO
        {
            public POINT Reserved;
            public POINT MaxSize;
            public POINT MaxPosition;
            public POINT MinimumTrackSize;
            public POINT MaximumTrackSize;
        }

        /// <summary>
        /// Overrides the form's lifecycle to start the fade‑in animation as soon as
        /// the window becomes visible, when explicitly enabled.
        /// This provides a smooth transition ffect without blocking the UI thread.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            if (_enableFadeIn) 
            { 
                fadeInTimer.Start(); 
            }
        }

        /// <summary>
        /// Represents a point (x,rowPosY) used by the MINMAXINFO structure.
        /// </summary>
        private struct POINT
        {
            public int X;
            public int Y;
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
                        // Sets the back of the currentDateLabel on light blue color
                        taskSelection[i].Task_label.BackColor = Color.FromArgb(168, 208, 230);

                        // Sets the text foreground color on black 
                        taskSelection[i].Task_label.ForeColor = Color.Black;


                        if (taskSelection[i].Task_information != "")
                        {
                            lblTaskDescription.Text = taskSelection[i].Task_information;
                            lblTaskDescription.Visible = true;
                        }

                        else
                        {
                            lblTaskDescription.Visible = false;
                        }
                    }
                    else
                    {
                        taskSelection[i].Task_label.BackColor = Color.Transparent;
                        taskSelection[i].Task_label.ForeColor = Color.Black;


                        lblTaskDescription.Text = "";
                        lblTaskDescription.Visible = false;
                    }
                }
                else
                {
                    taskSelection[i].Task_label.BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// Repositions all task-related buttons (Validate, Edit, Delete, etc.)
        /// and resizes the task currentDateLabel on each row so that buttons always remain
        /// visible inside the panel, even when the window shrinks.
        /// The task currentDateLabel expands to fill all remaining horizontal space when the window grows,
        /// giving a responsive layout effect.
        /// </summary>
        /// <param name="panel">Panel containing dynamically created task controls.</param>
        /// <summary>
        private void ResizeTasksInPanel(Panel panel)
        {
            // Total horizontal space available inside this task panel
            int availablePanelWidth = panel.ClientSize.Width;

            // Space to keep between the rightmost button and the panel border
            int rightSidePadding = 20;

            // Horizontal spacing between adjacent buttons
            int buttonHorizontalSpacing = 10;

            // Group all controls by their vertical position.
            // Each unique "Top" value corresponds to one logical task row.
            var taskRows = panel.Controls
                                .Cast<Control>()
                                .GroupBy(control => control.Top)
                                .ToList();

            foreach (var row in taskRows)
            {
                // Identify the main task currentDateLabel for this row.
                Label taskLabel = row
                    .OfType<Label>()
                    .Where(label => label.Height == 25 && label.Left > 40)
                    .FirstOrDefault();

                // If no task currentDateLabel is found, this row is not a task row, so we skip it.
                if (taskLabel == null)
                {
                    continue;
                }

                // Collects all buttons on this row (Validate, Edit, Delete, etc.)
                // Sorting by their original Left ensures a predictable right-to-left order.
                var taskRowButtons = row
                    .OfType<Button>()
                    .OrderByDescending(button => button.Left)
                    .ToList();

                // Starting X coordinate for the rightmost button.
                // Buttons will be placed from right to left.
                int nextButtonRightEdge = availablePanelWidth - rightSidePadding;

                // Repositions each button from right to left.
                foreach (Button button in taskRowButtons)
                {
                    // Aligns vertically with the task currentDateLabel (same row)
                    button.Top = taskLabel.Top;

                    // Positions the button so its right edge aligns with nextButtonRightEdge
                    button.Left = nextButtonRightEdge - button.Width;

                    // Moves the cursor left for the next button
                    nextButtonRightEdge = button.Left - buttonHorizontalSpacing;
                }

                // Now resizes the task currentDateLabel so it fills all remaining space
                // between its original Left and the first button.
                int maximumLabelRightEdge = nextButtonRightEdge;

                // Prevents the currentDateLabel from collapsing too much
                int minimumLabelWidth = 50;

                // Computes the new width for the task currentDateLabel
                int computedLabelWidth = maximumLabelRightEdge - taskLabel.Left;

                if (computedLabelWidth < minimumLabelWidth)
                {
                    computedLabelWidth = minimumLabelWidth;
                }

                taskLabel.Width = computedLabelWidth;
            }
        }

        /// <summary>
        /// Restores the width of the window from the settings when the application is launched.
        /// </summary>
        private void RestoreWindowWidthFromSettings()
        {
            int savedWidth = Properties.Settings.Default.WindowWidth;

            if (savedWidth > 0)
            {
                this.Width = savedWidth;
            }
        }

        /// <summary>
        /// Selects the Settings tab in the main TabControl.
        /// </summary>
        public void SelectSettingsTab()
        {
            tabMain.SelectedTab = tabSettings;
        }

        /// <summary>
        /// Sets the dates of the calendar in bold when there's one or more deadline for a task on a given day
        /// </summary>
        private void SetDatesInBold()
        {
            // Copies the content of the list of string returned by the method into the list of string
            List<string> deadlinesList = new List<string>(dbConn.ReadDataForDeadlines());

            // Browses the list of string and converts each item to DataTime format 
            foreach (string item in deadlinesList)
            {
                DateTime myDateTime = Convert.ToDateTime(item);

                // Adds each DateTime item as a bolded date in the calendar
                calMonth.AddBoldedDate(myDateTime);
            }

            // Refreshes the calendar bolded dates
            calMonth.UpdateBoldedDates();
        }

        /// <summary>
        /// Handles the event when the user selects a tab
        /// </summary>
        private void tabMain_Selected(object sender, TabControlEventArgs e)
        {
            selectedTask = -1;
            RefreshSelectedTask();
            lblTaskDescription.Text = "";
            lblTaskDescription.Visible = false;

            if (tabMain.SelectedTab == tabTopics)
            {
                if (cboTopics.Items.Count == 0)
                {
                    cmdAddTopic.PerformClick();
                    tabMain.SelectTab(tabDates);
                }

                else if (cboTopics.SelectedIndex == -1)
                {
                    // Clears the placeholder text so the ComboBox can accept a real selection
                    cboTopics.Text = string.Empty;

                    // Selects the first topic, which will trigger SelectedIndexChanged
                    cboTopics.SelectedIndex = 0;
                }

                CheckIfPreviousNextTopicArrowButtonsUseful();
            }
            else if (tabMain.SelectedTab == tabFinished)
            {
                if (dbConn.ReadApprovedTask().Count > 0)
                {
                    cmdDeleteFinishedTasks.Visible = true;
                }
            }
        }
    }
}
