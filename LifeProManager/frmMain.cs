/// <file>frmMain.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.4</version>
/// <date>March 1st, 2026</date>

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
        private const int LAYOUT_TODAY = 1;
        private const int LAYOUT_WEEK = 2;
        private const int LAYOUT_FINISHED = 3;

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

        // Stores the original image path of the last hovered button to restore it on mouse leave
        private string _lastHoveredButtonOriginalImagePath;

        // Array to store the next seven days in "yyyy-MM-dd" format for quick access
        private string[] plusSevenDays = new string[7];
        
        private int nbTasksToComplete = 0;

        // Stores the currently selected date in both DateTime and string formats
        private DateTime selectedDateTypeDateTime;
        private string selectedDateString;

        // Stores the ID of the currently selected task
        private int selectedTaskId = -1;

        private List<TaskSelection> lstTaskSelection = new List<TaskSelection>();

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
            string cmdDeleteTopicPath = Path.Combine(resourcesDir, "delete-trash.png");
            string cmdDeleteFinishedTasksPath = Path.Combine(resourcesDir, "delete-trash.png");


            // Assigns the background images to the buttons using the loaded paths
            cmdPreviousDay.BackgroundImage = Image.FromFile(cmdPreviousDayPath);
            cmdToday.BackgroundImage = Image.FromFile(cmdTodayPath);
            cmdNextDay.BackgroundImage = Image.FromFile(cmdNextDayPath);
            cmdExportToHtml.BackgroundImage = Image.FromFile(cmdExportToHtmlPath);
            cmdBirthdayCalendar.BackgroundImage = Image.FromFile(cmdBirthdayCalendarPath);
            cmdAddTopic.BackgroundImage = Image.FromFile(cmdAddTopicPath);
            cmdAddTask.BackgroundImage = Image.FromFile(cmdAddTaskPath);
            cmdDeleteTopic.BackgroundImage = Image.FromFile(cmdDeleteTopicPath);
            cmdDeleteFinishedTasks.BackgroundImage = Image.FromFile(cmdDeleteFinishedTasksPath);

            // Fills the truth table that links each button to its original image path, 
            // for later restoration on mouse leave
            _buttonOriginalImagePaths[cmdPreviousDay] = cmdPreviousDayPath;
            _buttonOriginalImagePaths[cmdToday] = cmdTodayPath;
            _buttonOriginalImagePaths[cmdNextDay] = cmdNextDayPath;
            _buttonOriginalImagePaths[cmdExportToHtml] = cmdExportToHtmlPath;
            _buttonOriginalImagePaths[cmdBirthdayCalendar] = cmdBirthdayCalendarPath;
            _buttonOriginalImagePaths[cmdAddTopic] = cmdAddTopicPath;
            _buttonOriginalImagePaths[cmdAddTask] = cmdAddTaskPath;
            _buttonOriginalImagePaths[cmdDeleteTopic] = cmdDeleteTopicPath;
            _buttonOriginalImagePaths[cmdDeleteFinishedTasks] = cmdDeleteFinishedTasksPath;

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
        /// The method automatically derives the hover filename by inserting "-hover" 
        /// before the image extension, stores the original image path for later 
        /// restoration, and applies the hover image if it exists.
        /// </summary>
        public void Button_MouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (!_buttonOriginalImagePaths.TryGetValue(btn, out string normalPath))
            {
                return;
            }

            if (!File.Exists(normalPath))
            {
                return;
            }

            _lastHoveredButtonOriginalImagePath = normalPath;

            string directory = Path.GetDirectoryName(normalPath);
            string filenameWithoutExt = Path.GetFileNameWithoutExtension(normalPath);
            string extension = Path.GetExtension(normalPath);

            string hoverPath = Path.Combine(directory, filenameWithoutExt + "-hover" + extension);

            if (File.Exists(hoverPath))
            {
                btn.BackgroundImage = Image.FromFile(hoverPath);
            }
        }

        /// <summary>
        /// Handles the mouse-leave event for any button by restoring its original background
        /// image. The method uses the previously stored file path of the normal image
        /// and reloads it to revert the button to its default visual state.
        /// </summary>
        public void Button_MouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (string.IsNullOrEmpty(_lastHoveredButtonOriginalImagePath))
            {
                return;
            }

            if (File.Exists(_lastHoveredButtonOriginalImagePath))
            {
                btn.BackgroundImage = Image.FromFile(_lastHoveredButtonOriginalImagePath);
            }

            _lastHoveredButtonOriginalImagePath = null;
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
        /// Deletes all finished tasksFound from the database
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
        /// Deletes the currently displayed topic and all the tasksFound associated with it
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
                UpdateAddTaskButtonVisibility();
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


        /// <summary>
        /// Sets the date to today when the user clicks on the calendar button
        /// </summary>
        private void cmdToday_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(DateTime.Today);
        }

        private void cmdToday_MouseEnter(object sender, EventArgs e)
        {

        }

        private void cmdToday_MouseLeave(object sender, EventArgs e)
        {

        }

        public void CreateTasksLayout(List<Tasks> tasksFound, int layout)
        {
            // Layout constants
            const int ROW_HEIGHT = 32;
            const int ICON_SIZE = 22;
            const int BUTTON_SIZE = 25;
            const int HORIZONTAL_GAP = 10;
            const int VERTICAL_GAP = 12;
            const int RIGHT_PADDING = 15;
            const int DATE_LABEL_WIDTH = 90;

            // Selects target panel
            Panel targetPanel = null;

            if (layout == LAYOUT_TODAY)
            {
                targetPanel = pnlToday;
            }
            else if (layout == LAYOUT_WEEK)
            {
                targetPanel = pnlWeek;
            }
            else if (layout == LAYOUT_TOPICS)
            {
                targetPanel = pnlTopics;
            }
            else if (layout == LAYOUT_FINISHED)
            {
                targetPanel = pnlFinished;
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
                // Parses deadline
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

                // Right panel (date + buttons)
                int rightPanelWidth = DATE_LABEL_WIDTH + (BUTTON_SIZE + HORIZONTAL_GAP) * 3 + RIGHT_PADDING;

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

                if (layout == LAYOUT_TOPICS)
                {
                    lblDate.Text = deadlineDateTime.ToString("yyyy-MM-dd");
                    lblDate.Top = 7;
                }
                else if (layout == LAYOUT_FINISHED && !string.IsNullOrEmpty(task.ValidationDate))
                {
                    lblDate.Text = task.ValidationDate.Substring(0, 10);
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

                btnUnapprove.Click += (s, e) =>
                {
                    dbConn.UnapproveTask(task.Id);
                    LoadTasks();
                };

                // Button placement
                int buttonsStartPosX = DATE_LABEL_WIDTH + HORIZONTAL_GAP;

                if (layout == LAYOUT_FINISHED)
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

                rightPanel.Controls.Add(lblDate);

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
                lblTaskTitle.Click += (s, e) =>
                {
                    selectedTaskId = task.Id;
                    RefreshSelectedTask();
                };

                lblTaskTitle.DoubleClick += (s, e) =>
                {
                    new frmEditTask(this, task).ShowDialog();
                };

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

            // T or D to set the date in the calendar to today
            if ((e.KeyCode == Keys.T || e.KeyCode == Keys.D) && e.Modifiers == Keys.None)
            {
                cmdToday.PerformClick();
                return;
            }

            // Ctrl+T or Ctrl+D identical
            if ((e.KeyCode == Keys.T || e.KeyCode == Keys.D) && e.Modifiers == Keys.Control)
            {
                cmdToday.PerformClick();
                return;
            }

            // A or Ctrl+A to add a new task
            if (e.KeyCode == Keys.A && (e.Modifiers == Keys.None || e.Modifiers == Keys.Control))
            {
                cmdAddTask.PerformClick();
                return;
            }

            // B or Ctrl+B to open the birthday calendar
            if (e.KeyCode == Keys.B && (e.Modifiers == Keys.None || e.Modifiers == Keys.Control))
            {
                cmdBirthdayCalendar.PerformClick();
                return;
            }

            // E or Ctrl+E to export all tasksFound to an HTML file
            if (e.KeyCode == Keys.E && (e.Modifiers == Keys.None || e.Modifiers == Keys.Control))
            {
                cmdExportToHtml.PerformClick();
                return;
            }

            // P or Ctrl+P to set the date in the calendar to the previous day
            if (e.KeyCode == Keys.P && (e.Modifiers == Keys.None || e.Modifiers == Keys.Control))
            {
                cmdPreviousDay.PerformClick();
                return;
            }

            // Alt+Left identical
            if (e.KeyCode == Keys.Left && e.Modifiers == Keys.Alt)
            {
                cmdPreviousDay.PerformClick();
                return;
            }

            // N or Ctrl+N to set the date in the calendar to the next day
            if (e.KeyCode == Keys.N && (e.Modifiers == Keys.None || e.Modifiers == Keys.Control))
            {
                cmdNextDay.PerformClick();
                return;
            }

            // Alt+Right identical
            if (e.KeyCode == Keys.Right && e.Modifiers == Keys.Alt)
            {
                cmdNextDay.PerformClick();
                return;
            }

            // Shift+W to navigate the calendar backwards by a week
            if (e.KeyCode == Keys.W && e.Modifiers == Keys.Shift)
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(-7));
                return;
            }

            // Alt+Up identical
            if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(-7));
                return;
            }

            // W or S to navigate the calendar forwards by a week
            if ((e.KeyCode == Keys.W || e.KeyCode == Keys.S) && e.Modifiers == Keys.None)
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(+7));
                return;
            }

            // Ctrl+W or Ctrl+S identical
            if ((e.KeyCode == Keys.W || e.KeyCode == Keys.S) && e.Modifiers == Keys.Control)
            {
                calMonth.SetDate(calMonth.SelectionStart.AddDays(+7));
                return;
            }

            // Alt+Down identical
            if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
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

            // Finds the index of the selected task in the lstTaskSelection list with a lambda expression
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
            if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.V) && e.Modifiers == Keys.None)
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
                DialogResult result = MessageBox.Show(
                    LocalizationManager.GetString("areYouSureDeleteTheTask"),
                    LocalizationManager.GetString("confirmDeletion"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
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
            lblAppInLanguage.Text = LocalizationManager.GetString("appInLanguage");
            lblTopic.Text = LocalizationManager.GetString("topic");
            lblExportDeadlineAndTitle.Text = LocalizationManager.GetString("exportDeadlineAndTitle");
            lblTaskDescription.Text = LocalizationManager.GetString("taskDescription");
            lblTaskDescriptionFontSize.Text = LocalizationManager.GetString("taskDescriptionFontSizeText");

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
            calMonth.UpdateBoldedDates();
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

        private void nudTaskDescriptionFontSize_ValueChanged(object sender, EventArgs e)
        {
            // Gets the new size selected by the user
            int newFontSize = (int)nudTaskDescriptionFontSize.Value;

            // Applies the new size immediately to the label (live update)
            lblTaskDescription.Font = new Font(lblTaskDescription.Font.FontFamily, newFontSize);

            Properties.Settings.Default.taskDescriptionFontSize = newFontSize;
            Properties.Settings.Default.Save();
        }
    }
}
