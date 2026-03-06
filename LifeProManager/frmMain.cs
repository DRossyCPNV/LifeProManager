/// <file>frmMain.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.8</version>
/// <date>March 6th, 2026</date>

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmMain : Form
    {
        private const int LAYOUT_TOPICS = 0;
        private const int LAYOUT_TODAY = 1;
        private const int LAYOUT_WEEK = 2;
        private const int LAYOUT_FINISHED = 3;
        private const int LAYOUT_SEARCH = 5;

        // Maps each button to its original image file path
        private readonly Dictionary<Button, string> _buttonOriginalImagePaths = new Dictionary<Button, string>();

        // Allows to copy last task values if it has been set with "repeatable" priority
        private bool copyLastTaskValues = false;

        // Indicates whether the form should play the fade‑in animation when shown.
        private readonly bool _enableFadeIn = false;

        // Timer used to perform the fade‑in animation when the form is shown.
        private System.Windows.Forms.Timer fadeInTimer;

        // Language codes mapped to ComboBox indices
        private readonly string[] _languageCodes = { "en", "fr", "es" };

        // Stores the original image of the last hovered button to restore it on mouse leave
        private Image _lastHoveredButtonOriginalImage;

        // List of task selections used to display a border around the selected task
        private List<TaskSelection> lstTaskSelection = new List<TaskSelection>();

        // Array to store the next seven days in "yyyy-MM-dd" format for quick access
        private string[] plusSevenDays = new string[7];
        
        private int nbTasksToComplete = 0;

        // Stores the currently selected date in both DateTime and string formats
        private DateTime selectedDateTypeDateTime;
        private string selectedDateString;

        // Stores the ID of the currently selected task
        private int selectedTaskId = -1;

        private SmartSearch smartSearch;

        // Provides access to the global database connection created in Program.cs.
        // This ensures all forms use the same connection instance.
        public DBConnection dbConn => Program.DbConn;

        public DateTime SelectedDateTypeDateTime
        {
            get { return selectedDateTypeDateTime; }
            set { selectedDateTypeDateTime = value; }
        }

        public string SelectedDateString
        {
            get { return selectedDateString; }
            set { selectedDateString = value; }
        }

        public bool CopyLastTaskValues
        {
            get { return copyLastTaskValues; }
            set { copyLastTaskValues = value; }
        }

        public frmMain(bool enableFadeIn = false)
        {
            InitializeComponent();

            smartSearch = new SmartSearch();

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
        /// loads the topics and tasksFound, and applies the language settings based 
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
            selectedDateTypeDateTime = DateTime.Today;

            // Converts the date to the format used by the database
            selectedDateString = DateTime.Today.ToString("yyyy-MM-dd");

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

            // Loads the saved font size from application settings
            int savedTaskDescriptionFontSize = Properties.Settings.Default.taskDescriptionFontSize; 
            
            // Applies the saved size to the task description label
            lblTaskDescription.Font = new Font(lblTaskDescription.Font.FontFamily, savedTaskDescriptionFontSize); 
            
            // Synchronize the NumericUpDown with the saved value
            nudTaskDescriptionFontSize.Value = savedTaskDescriptionFontSize;

            // Sets the directory path for the resources folder, where all the button images are stored
            string resourcesDir = Path.Combine(Application.StartupPath, "Resources");

            // Sets the path for each button image by combining the resources directory path with the specific image filename
            string cmdPreviousDayPath = Path.Combine(resourcesDir, "left-chevron.png");
            string cmdTodayPath = Path.Combine(resourcesDir, "calendar-today.png");
            string cmdNextDayPath = Path.Combine(resourcesDir, "right-chevron.png");
            string cmdExportToHtmlPath = Path.Combine(resourcesDir, "exportToHtml.png");
            string cmdBirthdayCalendarPath = Path.Combine(resourcesDir, "birthday-cake.png");
            string cmdAddTopicPath = Path.Combine(resourcesDir, "add-topic.png");
            string cmdAddTaskPath = Path.Combine(resourcesDir, "add-task.png");
            string cmdSearchByKeywordsPath = Path.Combine(resourcesDir, "search.png");
            string cmdDeleteTopicPath = Path.Combine(resourcesDir, "delete-trash.png");
            string cmdDeleteFinishedTasksPath = Path.Combine(resourcesDir, "delete-trash.png");
            string cmdDeleteTopicSuperHoverPath = Path.Combine(resourcesDir, "delete-trash-super-hover.png");
            string cmdDeleteFinishedTasksSuperHoverPath = Path.Combine(resourcesDir, "delete-trash-super-hover.png");

            // Assign background images from embedded resources
            cmdPreviousDay.BackgroundImage = Properties.Resources.left_chevron;
            cmdToday.BackgroundImage = Properties.Resources.calendar_today;
            cmdNextDay.BackgroundImage = Properties.Resources.right_chevron;
            cmdExportToHtml.BackgroundImage = Properties.Resources.exportToHtml;
            cmdBirthdayCalendar.BackgroundImage = Properties.Resources.birthday_cake;
            cmdAddTopic.BackgroundImage = Properties.Resources.add_topic;
            cmdAddTask.BackgroundImage = Properties.Resources.add_task;
            cmdSearchByKeywords.BackgroundImage = Properties.Resources.search;
            cmdDeleteTopic.BackgroundImage = Properties.Resources.delete_trash;
            cmdDeleteFinishedTasks.BackgroundImage = Properties.Resources.delete_trash;

            // Super-hover icons
            _buttonOriginalImagePaths[cmdDeleteTopic] = "delete-trash.png";
            _buttonOriginalImagePaths[cmdDeleteFinishedTasks] = "delete-trash.png";

            // Truth table for restoring original images
            _buttonOriginalImagePaths[cmdPreviousDay] = "left-chevron.png";
            _buttonOriginalImagePaths[cmdToday] = "calendar-today.png";
            _buttonOriginalImagePaths[cmdNextDay] = "right-chevron.png";
            _buttonOriginalImagePaths[cmdExportToHtml] = "exportToHtml.png";
            _buttonOriginalImagePaths[cmdBirthdayCalendar] = "birthday-cake.png";
            _buttonOriginalImagePaths[cmdAddTopic] = "add-topic.png";
            _buttonOriginalImagePaths[cmdAddTask] = "add-task.png";
            _buttonOriginalImagePaths[cmdSearchByKeywords] = "search.png";

            // Buttons hover events
            cmdPreviousDay.MouseEnter += Button_MouseEnter;
            cmdPreviousDay.MouseLeave += Button_MouseLeave;
            cmdToday.MouseEnter += Button_MouseEnter;
            cmdToday.MouseLeave += Button_MouseLeave;
            cmdNextDay.MouseEnter += Button_MouseEnter;
            cmdNextDay.MouseLeave += Button_MouseLeave;
            cmdExportToHtml.MouseEnter += Button_MouseEnter;
            cmdExportToHtml.MouseLeave += Button_MouseLeave;
            cmdBirthdayCalendar.MouseEnter += Button_MouseEnter;
            cmdBirthdayCalendar.MouseLeave += Button_MouseLeave;
            cmdAddTopic.MouseEnter += Button_MouseEnter;
            cmdAddTopic.MouseLeave += Button_MouseLeave;
            cmdAddTask.MouseEnter += Button_MouseEnter;
            cmdAddTask.MouseLeave += Button_MouseLeave;
            cmdSearchByKeywords.MouseEnter += Button_MouseEnter;
            cmdSearchByKeywords.MouseLeave += Button_MouseLeave;
            cmdDeleteTopic.MouseEnter += Button_MouseEnter;
            cmdDeleteTopic.MouseLeave += Button_MouseLeave;
            cmdDeleteFinishedTasks.MouseEnter += Button_MouseEnter;
            cmdDeleteFinishedTasks.MouseLeave += Button_MouseLeave;
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

            // Selects the current language based on its tokenIndex in _languageCodes
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

            // Adjusts the tasksFound in each panel
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
        /// Handles the mouse-enter event for any button by replacing its background image
        /// with the corresponding hover version.
        /// </summary>
        public void Button_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            // Retrieves the original resource name for this button
            if (!_buttonOriginalImagePaths.TryGetValue(btn, out string normalResource))
            {
                return;
            }

            // Stores the original image
            _lastHoveredButtonOriginalImage = btn.BackgroundImage;

            bool isDeleteButton = (btn == cmdDeleteTopic || btn == cmdDeleteFinishedTasks);

            // Super-hover
            if (isDeleteButton && Control.ModifierKeys == Keys.Shift)
            {
                btn.BackgroundImage = Properties.Resources.delete_trash_super_hover;
                return;
            }

            // Normal hover for delete buttons
            if (isDeleteButton)
            {
                btn.BackgroundImage = Properties.Resources.delete_trash_hover;
                return;
            }
            
            // Hover for non-delete buttons 

            // Example: "calendar-today.png" becomes "calendar_today_hover"
            string baseName = Path.GetFileNameWithoutExtension(normalResource);

            // Embedded resources use underscores instead of hyphens
            baseName = baseName.Replace("-", "_");

            // Append "_hover" to match the naming convention
            string hoverName = baseName + "_hover";

            // Retrieve the hover image from the Resources manager using the resource key
            object hoverObject = Properties.Resources.ResourceManager.GetObject(hoverName);

            if (hoverObject is Image hoverImage)
            {
                btn.BackgroundImage = hoverImage;
            }
        }

        /// <summary>
        /// Handles the mouse-leave event for any button by restoring its original background
        /// image. The method uses the previously stored image to revert the button to its
        /// default visual state.
        /// </summary>
        public void Button_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            // If no original image was stored, nothing to restore
            if (_lastHoveredButtonOriginalImage == null)
            {
                return;
            }

            // Restores the original image that was saved during MouseEnter
            btn.BackgroundImage = _lastHoveredButtonOriginalImage;

            // Clears the stored reference so the next hover starts clean
            _lastHoveredButtonOriginalImage = null;
        }

        /// <summary>
        /// Handles the event when the user selects a date in the calendar.
        /// Updates the Today label using localized text and reloads tasksFound.
        /// </summary>
        private void calMonth_DateChanged(object sender, DateRangeEventArgs e)
        {
            string labelText = GetCurrentDateLabel();
            DateTime selectedDateTypeDateTime = calMonth.SelectionStart;

            this.selectedDateTypeDateTime = selectedDateTypeDateTime;

            if (labelText == null)
            {
                // For dates beyond ±2 days: show only the date
                lblToday.Text = selectedDateTypeDateTime.ToString("d", CultureInfo.CurrentUICulture);
            }
            else
            {
                // For dates close to today: show currentDateLabel and date
                lblToday.Text = $"{labelText} ({selectedDateTypeDateTime:dd-MMM-yyyy})";
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

            // Converts the selected tokenIndex into a language code
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
        /// Loads the tasksFound for the selected topic
        /// </summary>
        private void cboTopics_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ignores placeholder
            if (cboTopics.SelectedIndex == -1)
            {
                return;
            }

            tabMain.SelectedTab = tabTopics;
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
            RegistryKey registryRunKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // The checkbox has been checked by the user and the value in the registry doesn't exist
            if (chkRunAtWindowsStartup.Checked && registryRunKey.GetValue("LifeProManager") == null)
            {
                // Add the value in the registry so that the application runs at startup
                registryRunKey.SetValue("LifeProManager", Application.ExecutablePath);
            }

            // If the checkbox has been unchecked by the user and the application has been set to run at startup
            else if (chkRunAtWindowsStartup.Checked == false && registryRunKey.GetValue("LifeProManager") != null)
            {
                // Remove the value from the registry so that the application doesn't start
                registryRunKey.DeleteValue("LifeProManager", false);
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

        private void cmdAddTask_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdAddTask_MouseLeave(object sender, EventArgs e)
        {

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

        private void cmdAddTopic_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdAddTopic_MouseLeave(object sender, EventArgs e)
        {

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

        private void cmdBirthdayCalendar_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdBirthdayCalendar_MouseLeave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Deletes all finished tasks from the database.
        /// SHIFT + Click = deletes ALL tasks from the database.
        /// </summary>
        private void cmdDeleteFinishedTasks_Click(object sender, EventArgs e)
        {
            // Hidden shortcut: SHIFT + Click
            if (Control.ModifierKeys == Keys.Shift)
            {
                    DialogResult dialogResult = MessageBox.Show(LocalizationManager.GetString("deleteAllTasksWarning"),
                    LocalizationManager.GetString("deleteAllTasksTitle"), MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.OK)
                {
                    try
                    {
                        dbConn.ExecuteRawSql("DELETE FROM Tasks;");
                        dbConn.ExecuteRawSql("VACUUM;");

                        LoadDoneTasks();
                        lblTaskDescription.Visible = false;
                        cmdDeleteFinishedTasks.Visible = false;

                        MessageBox.Show(LocalizationManager.GetString("deleteAllTasksSuccess"));
                    }
                    catch
                    {
                        MessageBox.Show(LocalizationManager.GetString("deleteAllTasksError"));
                    }
                }

                return;
            }

            // Normal behavior: delete only finished tasks
            dbConn.DeleteAllDoneTasks();
            LoadDoneTasks();
            lblTaskDescription.Visible = false;
            cmdDeleteFinishedTasks.Visible = false;
        }

        /// <summary>
        /// Deletes the currently displayed topic and therefore all tasks associated with it.
        /// SHIFT + Click = deletes all topics and all tasks.
        /// </summary>
        private void cmdDeleteTopic_Click(object sender, EventArgs e)
        {
            // Hidden shortcut: SHIFT + Click
            if (Control.ModifierKeys == Keys.Shift)
            {
                DialogResult dialogResult = MessageBox.Show(LocalizationManager.GetString("deleteAllTopicsWarning"),
                    LocalizationManager.GetString("deleteAllTopicsTitle"), MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.OK)
                {
                    try
                    {
                        // Deletes Tasks before Lists
                        dbConn.ExecuteRawSql("DELETE FROM Tasks;");
                        dbConn.ExecuteRawSql("DELETE FROM Lists;");

                        // Reset auto-increment counters (safe only when DB is fully empty)
                        dbConn.ExecuteRawSql("DELETE FROM sqlite_sequence;");

                        // Compacts database
                        dbConn.ExecuteRawSql("VACUUM;");

                        // Refreshes UI
                        LoadTopics();
                        LoadTasks();
                        UpdateAddTaskButtonVisibility();
                        CheckIfPreviousNextTopicArrowButtonsUseful();

                        cboTopics.Text = LocalizationManager.GetString("displayByTopic");

                        MessageBox.Show(LocalizationManager.GetString("deleteAllTopicsSuccess"), LocalizationManager.GetString("success"), MessageBoxButtons.OK);

                        // Switches to Dates tab so the refreshed layout is visible
                        tabMain.SelectTab(tabDates);
                    }
                    
                    catch
                    {
                        MessageBox.Show(LocalizationManager.GetString("deleteAllTopicsError"), LocalizationManager.GetString("error"), MessageBoxButtons.OK);
                    }
                }

                return;
            }

            // Normal behavior: delete only the selected topic
            Lists currentTopic = cboTopics.SelectedItem as Lists;

            if (cboTopics.Items.Count == 0)
            {
                return;
            }

            var confirmResult = MessageBox.Show(LocalizationManager.GetString("delTopicWillRemoveRelTasks"),
                LocalizationManager.GetString("confirmDeletion"), MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.OK)
            {
                dbConn.DeleteTopic(currentTopic.Id);

                LoadTopics();
                UpdateAddTaskButtonVisibility();
                LoadTasks();

                if (cboTopics.Items.Count == 0)
                {
                    tabMain.SelectTab(tabDates);
                    cboTopics.Text = LocalizationManager.GetString("displayByTopic");
                }
                else
                {
                    cboTopics.SelectedIndex = 0;
                }

                CheckIfPreviousNextTopicArrowButtonsUseful();
            }
        }

        private void cmdDeleteTopic_MouseEnter(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (Control.ModifierKeys == Keys.Shift)
            {
                button.BackgroundImage = Properties.Resources.delete_trash_super_hover;
            }
            else
            {
                button.BackgroundImage = Properties.Resources.delete_trash_hover;
            }
        }

        private void cmdDeleteTopic_MouseLeave(object sender, EventArgs e)
        {
            var button = (Button)sender;

            // Restores the original image stored on MouseEnter
            button.BackgroundImage = _lastHoveredButtonOriginalImage;
        }

        /// <summary>
        /// Export all data to a web page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdexportToHtml_Click(object sender, EventArgs e)
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
                    StreamWriter streamWriter = new StreamWriter(saveFileDialog1.FileName);

                    streamWriter.WriteLine(stringToWrite);
                    streamWriter.Close();
                }

                catch (Exception exceptionRaised)
                {
                    Console.WriteLine("Exception: " + exceptionRaised.Message);
                }
            }
        }

        private void cmdExportToHtml_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdExportToHtml_MouseLeave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Sets the date to the next day when the user clicks on the right arrow button
        /// </summary>
        private void cmdNextDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(1));
        }

        /// <summary>
        /// Shows the tasksFound for the next topic, from the drop-down list
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

        private void cmdNextDay_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdNextDay_MouseLeave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Shows the tasksFound for the previous topic, from the drop-down list
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

        private void cmdPreviousDay_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdPreviousDay_MouseLeave(object sender, EventArgs e)
        {

        }

        private void cmdSearchByKeywords_Click(object sender, EventArgs e)
        {
            ShowSearchPopup();
        }

        /// <summary>
        /// Sets the date to today when the user clicks on the calendar button.
        /// </summary>
        private void cmdToday_Click(object sender, EventArgs e)
        {
            // Detects if we are currently in search mode
            bool isSearchLayout = lblToday.Text == LocalizationManager.GetString("SearchResults");

            if (isSearchLayout)
            {
                // If Today is already selected, force a temporary date change
                // so the calendar triggers DateChanged again
                if (calMonth.SelectionStart == DateTime.Today)
                {
                    calMonth.SetDate(DateTime.Today.AddDays(1));
                }

                // Sets the date to Today, which triggers the DateChanged event,
                // reloads today's tasks and restores the Today label
                calMonth.SetDate(DateTime.Today);
                return;
            }

            // Normal behavior when not in search mode:
            // - If Today is not selected, selects it
            // - If Today is already selected, does nothing
            if (calMonth.SelectionStart != DateTime.Today)
            {
                calMonth.SetDate(DateTime.Today);
            }
        }

        private void cmdToday_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdToday_MouseLeave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Creates the layout for the tasksFound in the Today, Week, Topics or Finished panel, 
        /// based on the provided list of tasksFound and the target layout.
        /// </summary>
        /// <param name="tasksFound"></param>
        /// <param name="targetLayout"></param>
        public void CreateTasksLayout(List<Tasks> tasksFound, int targetLayout)
        {
            // Layout constants
            const int ROW_HEIGHT = 32;
            const int ICON_SIZE = 22;
            const int BUTTON_SIZE = 25;
            const int HORIZONTAL_GAP = 10;
            const int VERTICAL_GAP = 12;
            const int RIGHT_PADDING = 15;
            const int DATE_LABEL_WIDTH = 105;

            // Selects target panel
            Panel targetPanel = null;

            if (targetLayout == LAYOUT_TODAY)
            {
                targetPanel = pnlToday;
            }
            else if (targetLayout == LAYOUT_WEEK)
            {
                targetPanel = pnlWeek;
            }
            else if (targetLayout == LAYOUT_TOPICS)
            {
                targetPanel = pnlTopics;
            }
            else if (targetLayout == LAYOUT_FINISHED)
            {
                targetPanel = pnlFinished;
            }

            else if (targetLayout == LAYOUT_SEARCH) 
            { 
                targetPanel = pnlToday;
                selectedTaskId = -1;
            }

            if (targetPanel == null)
            {
                return;
            }

            targetPanel.AutoScroll = false;
            targetPanel.Controls.Clear();

            int currentRowTopY = 10;

            // Iterates through tasksFound 
            foreach (Tasks task in tasksFound)
            {
                // Special case: dummy tasks (-1 = no results, -2 = search crashed)
                if (task.Id == -1 || task.Id == -2)
                {
                    Label lbl = new Label
                    {
                        Text = task.Title,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 11),
                        ForeColor = Color.Gray,
                        Left = 20,
                        Top = currentRowTopY,
                        Cursor = Cursors.Hand
                    };

                    lbl.Click += (s, e) =>
                    {
                        cmdToday.PerformClick();
                    };

                    targetPanel.Controls.Add(lbl);
                    currentRowTopY += ROW_HEIGHT + VERTICAL_GAP;
                    continue;
                }

                // Parses deadline for normal tasks
                DateTime deadlineDateTime;
                if (!DateTime.TryParse(task.Deadline, out deadlineDateTime))
                {
                    continue;
                }

                deadlineDateTime = deadlineDateTime.Date;

                // Row container
                Panel rowPanel = new Panel
                {
                    Left = 10,
                    Top = currentRowTopY,
                    Width = targetPanel.ClientSize.Width - 20,
                    Height = ROW_HEIGHT,
                    BackColor = Color.Transparent,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                // Right panel (date + buttons) : adjusts its width by removing
                // date space for Today/Week layouts, keeping full date+buttons width
                // for other layouts 
                int rightPanelWidth = (targetLayout == LAYOUT_TODAY || targetLayout == LAYOUT_WEEK)
                ? (BUTTON_SIZE + HORIZONTAL_GAP) * 3 + RIGHT_PADDING
                : DATE_LABEL_WIDTH + (BUTTON_SIZE + HORIZONTAL_GAP) * 3 + RIGHT_PADDING;

                Panel rightPanel = new Panel
                {
                    Width = rightPanelWidth,
                    Height = ROW_HEIGHT,
                    Left = rowPanel.Width - rightPanelWidth,
                    Top = 0,
                    BackColor = Color.Transparent,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
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

                if (targetLayout == LAYOUT_TOPICS)
                {
                    lblDate.Text = deadlineDateTime.ToString("yyyy-MM-dd");
                }
                
                else if (targetLayout == LAYOUT_FINISHED && !string.IsNullOrEmpty(task.ValidationDate))
                {
                    if (DateTime.TryParse(task.ValidationDate, out DateTime validationDate))
                    {
                        string currentLangCode = LocalizationManager.GetCurrentLanguageCode(); 
                        CultureInfo currentCultureInfo = new CultureInfo(currentLangCode);

                        string dateFormat = (currentCultureInfo.TwoLetterISOLanguageName == "fr" ||
                            currentCultureInfo.TwoLetterISOLanguageName == "es")
                            ? "dd/MM/yyyy"
                            : "MM/dd/yyyy";

                        lblDate.Text = validationDate.ToString(dateFormat);
                    }
                }

                // Button factory
                Button CreateButton(Image imgButton)
                {
                    Button buttonForTask = new Button
                    {
                        Size = new Size(BUTTON_SIZE, BUTTON_SIZE),
                        BackgroundImage = imgButton,
                        BackgroundImageLayout = ImageLayout.Zoom,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.Transparent,
                        Top = (ROW_HEIGHT - BUTTON_SIZE) / 2,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right
                    };
                    buttonForTask.FlatAppearance.BorderSize = 0;
                    return buttonForTask;
                }

                Button btnApprove = CreateButton(Properties.Resources.validate_task);
                Button btnEdit = CreateButton(Properties.Resources.edit_task);
                Button btnDelete = CreateButton(Properties.Resources.delete_task);
                Button btnUnapprove = CreateButton(Properties.Resources.unapprove_task);

                // Button events

                // Approve button hover
                btnApprove.MouseEnter += (s, e) =>
                {
                    btnApprove.BackgroundImage = Properties.Resources.validate;
                    btnApprove.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 255, 255, 255); // halo effect
                };

                btnApprove.MouseLeave += (s, e) =>
                {
                    btnApprove.BackgroundImage = Properties.Resources.validate_task;
                    btnApprove.FlatAppearance.MouseOverBackColor = Color.Transparent; // removes halo effect
                    btnApprove.FlatAppearance.BorderSize = 0;
                };

                // Edit button hover
                btnEdit.MouseEnter += (s, e) =>
                {
                    btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 255, 255, 255);
                };

                btnEdit.MouseLeave += (s, e) =>
                {
                    btnEdit.FlatAppearance.MouseOverBackColor = Color.Transparent;
                };

                // Delete button hover
                btnDelete.MouseEnter += (s, e) =>
                {
                    btnDelete.BackgroundImage = Properties.Resources.delete_red;
                    btnDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 255, 255, 255);
                };

                btnDelete.MouseLeave += (s, e) =>
                {
                    btnDelete.BackgroundImage = Properties.Resources.delete_task;
                    btnDelete.FlatAppearance.MouseOverBackColor = Color.Transparent;
                };

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

                btnEdit.Click += (s, e) =>
                {
                    new frmEditTask(this, task).ShowDialog();
                };

                btnDelete.Click += (s, e) =>
                {
                    DialogResult result = MessageBox.Show(LocalizationManager.GetString("areYouSureDeleteTheTask"),
                        LocalizationManager.GetString("confirmDeletion"),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.OK)
                    {
                        dbConn.DeleteTask(task.Id);
                        LoadTasks();
                    }
                };

                btnUnapprove.Click += (s, e) =>
                {
                    dbConn.UnapproveTask(task.Id);
                    LoadTasks();
                };

                // Adjusts buttons offset:
                // no date column in Today/Week, full date offset in other layouts
                int buttonsStartPosX = (targetLayout == LAYOUT_TODAY || targetLayout == LAYOUT_WEEK)
                ? HORIZONTAL_GAP
                : DATE_LABEL_WIDTH + HORIZONTAL_GAP;

                if (targetLayout == LAYOUT_FINISHED)
                {
                    btnUnapprove.Left = buttonsStartPosX;
                    btnDelete.Left = buttonsStartPosX + BUTTON_SIZE + HORIZONTAL_GAP;

                    rightPanel.Controls.Add(btnUnapprove);
                    rightPanel.Controls.Add(btnDelete);
                }
                else
                {
                    btnApprove.Left = buttonsStartPosX;
                    btnEdit.Left = buttonsStartPosX + BUTTON_SIZE + HORIZONTAL_GAP;
                    btnDelete.Left = buttonsStartPosX + 2 * (BUTTON_SIZE + HORIZONTAL_GAP);

                    rightPanel.Controls.Add(btnApprove);
                    rightPanel.Controls.Add(btnEdit);
                    rightPanel.Controls.Add(btnDelete);
                }

                if (targetLayout != LAYOUT_TODAY && targetLayout != LAYOUT_WEEK)
                {
                    rightPanel.Controls.Add(lblDate);
                }

                // Left panel (icon + title)
                Panel leftPanel = new Panel
                {
                    Left = 0,
                    Top = 0,
                    Width = rightPanel.Left - 5,
                    Height = ROW_HEIGHT,
                    BackColor = Color.Transparent,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                PictureBox picOnLeftTaskTitle = new PictureBox
                {
                    Size = new Size(ICON_SIZE, ICON_SIZE),
                    Left = 0,
                    Top = (ROW_HEIGHT - ICON_SIZE) / 2,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    BackColor = Color.Transparent
                };

                // Icon logic
                if (task.Priorities_id == 4)
                {
                    picOnLeftTaskTitle.BackgroundImage = Properties.Resources.birthday_cake;
                }
                else if (deadlineDateTime < DateTime.Today)
                {
                    picOnLeftTaskTitle.BackgroundImage = Properties.Resources.clock;
                }
                else if (task.Priorities_id % 2 != 0)
                {
                    picOnLeftTaskTitle.BackgroundImage = Properties.Resources.important;
                }

                Label lblTaskTitle = new Label
                {
                    Left = ICON_SIZE + HORIZONTAL_GAP,
                    Top = 0,
                    Height = ROW_HEIGHT,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 11),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                // Title logic

                // For birthday tasksFound, if the description can be parsed as a birth year,
                // calculates and displays the age reached this year in parentheses next to the title
                if (task.Priorities_id == 4 && int.TryParse(task.Description, out int birthYear))
                {
                    int ageReachedThisYear = DateTime.Now.Year - birthYear;
                    lblTaskTitle.Text = task.Title + " (" + ageReachedThisYear + ")";
                }
                else
                {
                    lblTaskTitle.Text = task.Title;
                }

                // Title events

                // Only real tasks have clickable titles, prevents potential issues
                // with the dummy task used for the "No tasks found" message in search results
                if (task.Id > 0)
                {
                    lblTaskTitle.Click += (s, e) =>
                    {
                        selectedTaskId = task.Id;
                        RefreshSelectedTask();
                        lblTaskTitle.Focus();
                    };

                    lblTaskTitle.DoubleClick += (s, e) =>
                    {
                        new frmEditTask(this, task).ShowDialog();
                    };
                }

                // Adds panels
                rowPanel.Controls.Add(leftPanel);
                rowPanel.Controls.Add(rightPanel);

                lblTaskTitle.Width = leftPanel.Width - (ICON_SIZE + HORIZONTAL_GAP);

                // Registers for selection highlighting
                lstTaskSelection.Add(new TaskSelection
                {
                    TaskId = task.Id,
                    TaskLabel = lblTaskTitle,
                    TaskInformation = task.Description,
                    Task_priority = task.Priorities_id
                });

                leftPanel.Controls.Add(picOnLeftTaskTitle);
                leftPanel.Controls.Add(lblTaskTitle);

                // Adds row to panel
                targetPanel.Controls.Add(rowPanel);
                
                // Adjusts right panel position when rowPanel has its final width,
                // ensuring the buttons stay visible after layout initialization
                rightPanel.Left = rowPanel.Width - rightPanel.Width;

                currentRowTopY += ROW_HEIGHT + VERTICAL_GAP;
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
        /// Splits the normalized query into individual tokens.
        /// Operators AND, OR and + are preserved as standalone tokens.
        /// </summary>
        private List<string> ExtractTokens(string normalizedQuery)
        {
            List<string> lstTokens = new List<string>();

            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return lstTokens;
            }

            // Ensures operators are isolated as tokens
            string preparedQuery = normalizedQuery.Replace("+", " + ").Replace(" and ", " AND ")
                .Replace(" or ", " OR ");

            // Splits on spaces
            // StringSplitOptions is an enumeration that tells Split() how to treat empty entries.
            // In a search engine pipeline, RemoveEmptyEntries is always the correct choice.
            string[] rawPartsOfQuery = preparedQuery.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawPart in rawPartsOfQuery)
            {
                string trimmedToken = rawPart.Trim();

                if (trimmedToken.Length > 0)
                {
                    lstTokens.Add(trimmedToken);
                }
            }

            return lstTokens;
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
        /// Keyboard shortcuts (global + task navigation)
        /// </summary>
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard shortcuts

            // Ctrl+A to add a new task
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                cmdAddTask.PerformClick();
                return;
            }

            // Ctrl+B to open the birthday calendar
            if (e.KeyCode == Keys.B && e.Modifiers == Keys.Control)
            {
                cmdBirthdayCalendar.PerformClick();
                return;
            }

            // Ctrl+E to export all tasksFound to an HTML file
            if (e.KeyCode == Keys.E && e.Modifiers == Keys.Control)
            {
                cmdExportToHtml.PerformClick();
                return;
            }

            // CTRL+F, Ctrl+K or Ctrl+S to open the search popup
            if ((e.KeyCode == Keys.F || e.KeyCode == Keys.K || e.KeyCode == Keys.S) && e.Control)
            {
                ShowSearchPopup();
                return;
            }

            // Ctrl+N or Alt+Right to set the date in the calendar to the next day
            if ((e.KeyCode == Keys.N && e.Modifiers == Keys.Control) || (e.KeyCode == Keys.Right && e.Modifiers == Keys.Alt))
            {
                cmdNextDay.PerformClick();
                return;
            }

            // Ctrl+P to set the date in the calendar to the previous day
            if ((e.KeyCode == Keys.P && e.Modifiers == Keys.Control) || (e.KeyCode == Keys.Left && e.Modifiers == Keys.Alt))
            {
                cmdPreviousDay.PerformClick();
                return;
            }
           
            // Ctrl+T or Ctrl+D to set the date in the calendar to today
            if ((e.KeyCode == Keys.T || e.KeyCode == Keys.D) && e.Modifiers == Keys.Control)
            {
                cmdToday.PerformClick();
                return;
            }           

            // Shift+W or Alt+Up to navigate the calendar backwards by a week
            if ((e.KeyCode == Keys.W && e.Modifiers == Keys.Shift) || (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt))
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(-7));
                return;
            }

            // Ctrl+W or Alt+Down to navigate the calendar forwards by a week
            if ((e.KeyCode == Keys.W && e.Modifiers == Keys.Control) || (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt))
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(+7));
                return;
            }

            // Shift+Del to delete all finished tasksFound
            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Shift)
            {
                if (tabMain.SelectedTab == tabFinished)
                {
                    if (cmdDeleteFinishedTasks.Visible == true)
                    {
                        cmdDeleteFinishedTasks.PerformClick();
                    }
                }
                return;
            }

            // Task navigation and actions
           
            if (selectedTaskId == -1)
            {
                return;
            }

            // Finds the tokenIndex of the selected task in the lstTaskSelection list with a lambda expression
            int indexSelectedTask = lstTaskSelection.FindIndex(task => task.TaskId == selectedTaskId);
            
            if (indexSelectedTask == -1)
            {
                return;
            }

            // Arrow Up to select the previous task in the current panel 
            if (e.KeyCode == Keys.Up && e.Modifiers == Keys.None)
            {
                if (indexSelectedTask > 0)
                {
                    selectedTaskId = lstTaskSelection[indexSelectedTask - 1].TaskId;
                }

                // If the selected task is the first in the list, selects the last one instead
                else
                {
                    selectedTaskId = lstTaskSelection[lstTaskSelection.Count - 1].TaskId;
                }

                RefreshSelectedTask();
                return;
            }

            // Arrow Down to select the next task in the current panel
            if (e.KeyCode == Keys.Down && e.Modifiers == Keys.None)
            {
                
                if (indexSelectedTask < lstTaskSelection.Count - 1)
                {
                    selectedTaskId = lstTaskSelection[indexSelectedTask + 1].TaskId;
                }

                // If the selected task is the last in the list, selects the first one instead
                else
                {
                    selectedTaskId = lstTaskSelection[0].TaskId;
                }

                RefreshSelectedTask();
                return;
            }

            // Enter or V to approve a selected task
            if ((e.KeyCode == Keys.Enter && e.Modifiers == Keys.None) ||
                (e.KeyCode == Keys.V && e.Modifiers == Keys.None))
            {
                string validationDate = DateTime.Today.ToString("yyyy-MM-dd");
                dbConn.ApproveTask(selectedTaskId, validationDate);
                LoadTasks();
                return;
            }

            // Space, E or M to edit a selected task
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.E || e.KeyCode == Keys.M) && e.Modifiers == Keys.None)
            {
                Tasks taskToEdit = dbConn.ReadTaskById(selectedTaskId);

                if (taskToEdit != null)
                {
                    new frmEditTask(this, taskToEdit).ShowDialog();
                }

                return;
            }

            // Delete key to delete a selected task
            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
            {
                DialogResult result = MessageBox.Show(LocalizationManager.GetString("areYouSureDeleteTheTask"),
                    LocalizationManager.GetString("confirmDeletion"), MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.OK)
                {
                    dbConn.DeleteTask(selectedTaskId);
                    LoadTasks();
                }

                return;
            }

            // Panel cycling with Tab and Shift+Tab when focus is on a task or inside a panel
            bool focusIsOnPanel = pnlToday.ContainsFocus || pnlWeek.ContainsFocus || 
                pnlTopics.ContainsFocus || pnlFinished.ContainsFocus;

            // Checks if focus is on a task label, by verifying that the active control is a Label and that it exists in the lstTaskSelection list
            bool focusIsOnTask = ActiveControl is Label && 
                lstTaskSelection.Any(taskSelected => taskSelected.TaskLabel == ActiveControl);

            if (focusIsOnPanel || focusIsOnTask)
            {
                if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.None)
                {
                    MoveToNextPanel();
                    return;
                }

                if (e.KeyCode == Keys.Tab && e.Modifiers == Keys.Shift)
                {
                    MoveToPreviousPanel();
                    return;
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
        /// When the form is shown, loads all the topics and tasksFound.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Shown(object sender, EventArgs e)
        {
            LoadTopics();
            UpdateAddTaskButtonVisibility();
            LoadTasks();
        }

        /// <summary>
        /// Resizes the tasksFound width in the different panels when the form is resized 
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
        /// Generates all possible variants of a token by applying
        /// insertion, deletion, substitution and transposition.
        /// </summary>
        private List<string> GenerateLevenshteinVariants(string token)
        {
            List<string> lstVariants = new List<string>();

            if (string.IsNullOrWhiteSpace(token))
            {
                return lstVariants;
            }

            string alphabet = "abcdefghijklmnopqrstuvwxyz";

            // Deletion of each character
            for (int index = 0; index < token.Length; index++)
            {
                string tokenVariant = token.Remove(index, 1);
                lstVariants.Add(tokenVariant);
            }

            // Insertion of each letter of the alphabet at each position
            for (int index = 0; index <= token.Length; index++)
            {
                foreach (char letter in alphabet)
                {
                    string tokenVariant = token.Insert(index, letter.ToString());
                    lstVariants.Add(tokenVariant);
                }
            }

            // Substitution of each character with each letter of the alphabet
            for (int index = 0; index < token.Length; index++)
            {
                foreach (char letter in alphabet)
                {
                    if (letter != token[index])
                    {
                        string tokenVariant = token.Substring(0, index) + letter + token.Substring(index + 1);
                        lstVariants.Add(tokenVariant);
                    }
                }
            }

            // Transposition of adjacent characters
            for (int index = 0; index < token.Length - 1; index++)
            {
                // Converts the token to a character array to swap characters
                char[] charArray = token.ToCharArray();

                // Stores the character at position tokenIndex in a temporary variable
                char tempChar = charArray[index];

                // Swapping characters at positions tokenIndex and tokenIndex + 1
                charArray[index] = charArray[index + 1];

                // Places the tempChar (original character at tokenIndex) in tokenIndex + 1
                charArray[index + 1] = tempChar;

                // Converts the character array back to a string
                string tokenVariant = new string(charArray);

                // Adds the generated variant to the list of variants
                lstVariants.Add(tokenVariant);
            }

            return lstVariants;
        }

        /// <summary>
        /// Gets a list of all unique words contained in the title and description of all tasksFound.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllWordsFromTasks()
        {
            // Retrieves all tasks directly from the database
            List<Tasks> allTasks = dbConn.ReadTask("");

                HashSet<string> words = new HashSet<string>();

            foreach (var task in allTasks)
            {
                string text = (task.Title + " " + task.Description).ToLowerInvariant();
                
                foreach (string word in text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    words.Add(word);
                }
            }

            return words.ToList();
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
        /// Computes the next or previous occurrence of a given weekday
        /// relative to the current date, based on the direction sign.
        /// directionSign = +1 for next occurrence, -1 for previous occurrence.
        /// </summary>
        private DateTime GetRelativeWeekday(DateTime currentDate, DayOfWeek targetDayOfWeek, int directionSign)
        {
            int currentDayIndex = (int)currentDate.DayOfWeek;
            int targetDayIndex = (int)targetDayOfWeek;

            int dayOffset = (targetDayIndex - currentDayIndex + 7 * directionSign) % 7;

            // If offset is 0, we move one full week in the chosen direction.
            if (dayOffset == 0)
            {
                dayOffset = 7 * directionSign;
            }

            return currentDate.Date.AddDays(dayOffset);
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

        private void lnkAppInLanguage_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }

        /// <summary>
        /// Opens a dialog to insert tasks from a SQL script file.
        /// Executes the entire script in one block.
        /// </summary>
        private void lnkInsertTasksFromSql_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = LocalizationManager.GetString("sqlScriptFilter");
            fileDialog.Title = LocalizationManager.GetString("sqlScriptTitle");

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string sqlContent = File.ReadAllText(fileDialog.FileName);

                try
                {
                    dbConn.ExecuteRawSql(sqlContent);
                                   
                    MessageBox.Show(LocalizationManager.GetString("sqlScriptSuccess"), LocalizationManager.GetString("success"), MessageBoxButtons.OK);
                    LoadTasks();
                }
                catch
                {
                    MessageBox.Show(LocalizationManager.GetString("sqlScriptError"), LocalizationManager.GetString("error"), MessageBoxButtons.OK);
                }
            }
        }

        /// <summary>
        /// Loads all the tasksFound in the finished tab
        /// </summary>
        public void LoadDoneTasks()
        {
            // Updates tasksFound that are done
            List<Tasks> tasksList = dbConn.ReadApprovedTask();
            CreateTasksLayout(tasksList, LAYOUT_FINISHED);

            cmdDeleteFinishedTasks.Visible = (pnlFinished.Controls.Count > 0);
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
            lnkAppInLanguage.Text = LocalizationManager.GetString("appInLanguage");
            lblTopic.Text = LocalizationManager.GetString("topic");
            lblExportDeadlineAndTitle.Text = LocalizationManager.GetString("exportDeadlineAndTitle");
            lblTaskDescription.Text = LocalizationManager.GetString("taskDescription");
            lblTaskDescriptionFontSize.Text = LocalizationManager.GetString("taskDescriptionFontSizeText");

            // -- Links ---
            lnkInsertTasksFromSql.Text = LocalizationManager.GetString("lnkInsertTasksFromSqlText");

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
            selectedTaskId = -1;

            // Resets selection list used for task highlighting
            lstTaskSelection.Clear();

            // Hides description panel
            lblTaskDescription.Visible = false;

            // Reloads each layout based on the current reference date
            LoadTasksForDate();
            LoadTasksForTodayPlusSeven();
            LoadTasksInTopic();
            LoadDoneTasks();

            // Refreshes bolded dates in the calendar
            calMonth.RemoveAllBoldedDates();
            SetDatesInBold();

            // Updates total tasksFound counter
            nbTasksToComplete = dbConn.CountTotalTasksToComplete();

            ttpTotalTasksToComplete.SetToolTip(cmdExportToHtml,
                LocalizationManager.GetString("totalTasksToComplete") + " " + nbTasksToComplete.ToString()
            );
        }

        /// <summary>
        /// Loads all the tasksFound for today in the dates tab
        /// </summary>
        public void LoadTasksForDate()
        {
            // Updates tasksFound for the current date
            List<Tasks> tasksList = dbConn.ReadTaskForDate(selectedDateString);
            CreateTasksLayout(tasksList, LAYOUT_TODAY);
        }

        /// <summary>
        /// Loads all the tasksFound for the next 7 days in the dates tab
        /// </summary>
        public void LoadTasksForTodayPlusSeven()
        {
            // Updates tasksFound for the next seven days
            List<Tasks> tasksList = dbConn.ReadTaskForDatePlusSeven(plusSevenDays);
            CreateTasksLayout(tasksList, LAYOUT_WEEK);
        }

        /// <summary>
        /// Loads all the tasksFound in the topics tab
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

                // Updates the tasksFound for the current topic
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
        /// Cycles forward through the panels and selects the first task of the target panel.
        /// </summary>
        private void MoveToNextPanel()
        {
            Panel targetPanel = null;

            // Determines which panel is currently focused and sets the target panel
            // to the next one in the cycle
            if (pnlToday.ContainsFocus)
            {
                targetPanel = pnlWeek;
            }
            else if (pnlWeek.ContainsFocus)
            {
                targetPanel = pnlTopics;
            }
            else if (pnlTopics.ContainsFocus)
            {
                targetPanel = pnlFinished;
            }
            else
            {
                targetPanel = pnlToday;
            }

            targetPanel.Focus();

            foreach (TaskSelection selTask in lstTaskSelection)
            {
                // Determines whether this task belongs to the target panel by walking up the control hierarchy.
                // The structure created in CreateTasksLayout() is:
                //   targetPanel (pnlToday / pnlWeek / pnlTopics / pnlFinished)
                //     └── rowPanel
                //          └── leftPanel
                //               └── TaskLabel (the title label)
                // Therefore, TaskLabel.Parent.Parent.Parent refers to the panel that contains the task.
                // If this matches targetPanel, the task is part of that panel.

                if (selTask.TaskLabel.Parent != null &&
                    selTask.TaskLabel.Parent.Parent != null &&
                    selTask.TaskLabel.Parent.Parent.Parent == targetPanel)
                {
                    selectedTaskId = selTask.TaskId;
                    RefreshSelectedTask();
                    return;
                }
            }

            // No task in this panel: no selection
            selectedTaskId = -1;
            RefreshSelectedTask();
        }

        /// <summary>
        /// Cycles backward through the panels and selects the last task of the target panel.
        /// </summary>
        private void MoveToPreviousPanel()
        {
            Panel targetPanel = null;

            if (pnlToday.ContainsFocus)
            {
                targetPanel = pnlFinished;
            }
            else if (pnlFinished.ContainsFocus)
            {
                targetPanel = pnlTopics;
            }
            else if (pnlTopics.ContainsFocus)
            {
                targetPanel = pnlWeek;
            }
            else
            {
                targetPanel = pnlToday;
            }

            targetPanel.Focus();

            for (int i = lstTaskSelection.Count - 1; i >= 0; --i)
            {
                TaskSelection selTask = lstTaskSelection[i];

                if (selTask.TaskLabel.Parent != null &&
                    selTask.TaskLabel.Parent.Parent != null &&
                    selTask.TaskLabel.Parent.Parent.Parent == targetPanel)
                {
                    selectedTaskId = selTask.TaskId;
                    RefreshSelectedTask();
                    return;
                }
            }

            // No task in this panel: no selection
            selectedTaskId = -1;
            RefreshSelectedTask();
        }

        /// <summary>
        /// Gets the new font size for the task description from the numeric up-down control 
        /// and applies it immediately to the label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nudTaskDescriptionFontSize_ValueChanged(object sender, EventArgs e)
        {
            int newFontSize = (int)nudTaskDescriptionFontSize.Value;
            lblTaskDescription.Font = new Font(lblTaskDescription.Font.FontFamily, newFontSize);

            Properties.Settings.Default.taskDescriptionFontSize = newFontSize;
            Properties.Settings.Default.Save();
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
        /// Changes the background color of the selected task and changes the background to transparent for the unselected tasksFound
        /// </summary>
        public void RefreshSelectedTask()
        {
            for (int i = 0; i < lstTaskSelection.Count; ++i)
            {
                if (lstTaskSelection[i].TaskId == selectedTaskId)
                {
                    if (lstTaskSelection[i].TaskLabel.BackColor == Color.Transparent)
                    {
                        // Sets the back of the currentDateLabel on light blue color
                        lstTaskSelection[i].TaskLabel.BackColor = Color.FromArgb(168, 208, 230);

                        // Sets the text foreground color on black 
                        lstTaskSelection[i].TaskLabel.ForeColor = Color.Black;

                        // Hides description for birthday tasksFound
                        if (lstTaskSelection[i].Task_priority != 4 && lstTaskSelection[i].TaskInformation != "")
                        {
                            lblTaskDescription.Text = lstTaskSelection[i].TaskInformation;
                            lblTaskDescription.Visible = true;
                        }
                        else
                        {
                            lblTaskDescription.Visible = false;
                        }

                    }
                    else
                    {
                        lstTaskSelection[i].TaskLabel.BackColor = Color.Transparent;
                        lstTaskSelection[i].TaskLabel.ForeColor = Color.Black;


                        lblTaskDescription.Text = "";
                        lblTaskDescription.Visible = false;
                    }
                }
                else
                {
                    lstTaskSelection[i].TaskLabel.BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// Repositions all task-related buttons and resizes the task currentDateLabel 
        /// on each row so that buttons always remain visible inside the panel, 
        /// even when the window shrinks.
        /// The task currentDateLabel expands to fill all remaining horizontal space when the window grows.
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

            // Groups all controls by their vertical position.
            // Each unique "Top" value corresponds to one logical task row.
            var taskRows = panel.Controls
                                .Cast<Control>()
                                .GroupBy(control => control.Top)
                                .ToList();

            foreach (var row in taskRows)
            {
                // Identifies the main task currentDateLabel for this row.
                Label taskLabel = row
                    .OfType<Label>()
                    .Where(label => label.Height == 25 && label.Left > 40)
                    .FirstOrDefault();

                // If no task currentDateLabel is found, this row is not a task row, so we skip it.
                if (taskLabel == null)
                {
                    continue;
                }

                // Collects all buttons on this row sorting by their original Left
                // ensures a predictable right-to-left order.
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
             List<string> deadlinesList = new List<string>(dbConn.ReadDataForDeadlines());

            // Browses the list of string and converts each item to DataTime format
            // then adds it to the calendar bolded dates 
            foreach (string item in deadlinesList)
            {
                DateTime myDateTime = Convert.ToDateTime(item);

                calMonth.AddBoldedDate(myDateTime);
            }

            calMonth.UpdateBoldedDates();
        }

        /// <summary>
        /// Displays a lightweight popup search box using ToolStripDropDown.
        /// Auto-closes when clicking outside and triggers SmartSearch() on Enter.
        /// </summary>
        private void ShowSearchPopup()
        {
            // Textbox inside popup
            TextBox txtKeywords = new TextBox
            {
                Width = 220,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Button inside popup
            Button cmdPopupSearch = new Button
            {
                Text = LocalizationManager.GetString("Search"),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 0)
            };

            // Popup container with the same background as the main window
            ToolStripDropDown tlstrpDropDown = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                BackColor = this.BackColor
            };

            // Enter triggers the button click
            txtKeywords.KeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.Enter)
                {
                    cmdPopupSearch.PerformClick();
                }
            };

            // Prevents popup from closing when typing AltGr (for ñ, accents, etc.)
            txtKeywords.PreviewKeyDown += (s, ev) =>
            {
                if (ev.KeyCode == Keys.Alt ||
                    ev.KeyCode == Keys.Menu ||
                    ev.KeyCode == Keys.ControlKey)
                {
                    ev.IsInputKey = true;
                }
            };

            // Button click triggers the search and updates the UI with results
            cmdPopupSearch.Click += (s, ev) =>
            {
                List<Tasks> lstTasksFound = smartSearch.Search(txtKeywords.Text);

                tlstrpDropDown.Close();
                tabMain.SelectedTab = tabDates;

                lblToday.Text = LocalizationManager.GetString("SearchResults");

                // If no results are found, creates a dummy task with a "No results found"
                // message to display in the UI
                if (lstTasksFound == null || lstTasksFound.Count == 0)
                {
                    Tasks noResultTask = new Tasks();
                    noResultTask.Id = -1;
                    noResultTask.Title = LocalizationManager.GetString("NoResultsFound");
                    noResultTask.Description = string.Empty;
                    noResultTask.Deadline = null;

                    lstTasksFound = new List<Tasks>();
                    lstTasksFound.Add(noResultTask);
                }

                CreateTasksLayout(lstTasksFound, LAYOUT_SEARCH);
            };

            // Host controls
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // Adds controls to panel
            panel.Controls.Add(txtKeywords);
            panel.Controls.Add(cmdPopupSearch);

            ToolStripControlHost host = new ToolStripControlHost(panel)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // Adds host to dropdown control
            tlstrpDropDown.Items.Add(host);

            // Forces layout so the dropdown knows its real size
            tlstrpDropDown.AutoSize = true;
            tlstrpDropDown.PerformLayout();
            tlstrpDropDown.Width = host.Size.Width;
            tlstrpDropDown.Height = host.Size.Height;

            // Button screen position
            Point buttonScreenPos = cmdSearchByKeywords.PointToScreen(Point.Empty);

            // Centers horizontally under the button
            int centeredPosX = buttonScreenPos.X + (cmdSearchByKeywords.Width / 2) - (tlstrpDropDown.Width / 2) - 50;

            // Vertical position just below the button
            int y = buttonScreenPos.Y + cmdSearchByKeywords.Height + 2;

            // Final popup position
            Point finalPos = new Point(centeredPosX, y);

            // Shows popup at corrected position
            tlstrpDropDown.Show(finalPos);

            txtKeywords.Focus();
        }

        /// <summary>
        /// Handles the event when the user selects a tab
        /// </summary>
        private void tabMain_Selected(object sender, TabControlEventArgs e)
        {
            selectedTaskId = -1;
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

        /// <summary>
        /// Updates the visibility of the "Add Task" button based on whether at least one topic exists.
        /// The button is hidden when no topics are available, preventing users from attempting to
        /// create tasks before defining a topic.
        /// </summary>
        public void UpdateAddTaskButtonVisibility()
        {
            cmdAddTask.Visible = cboTopics.Items.Count > 0;
        }
    }
}
