/// <file>frmMain.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.0</version>
/// <date>November 7th, 2019</date>



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmMain : Form
    {
        private const int LAYOUT_TOPICS = 0;
        private const int LAYOUT_CURRENT_DATE = 1;
        private const int LAYOUT_PLUS_SEVEN_DAYS = 2;
        private const int LAYOUT_DONE = 3;

        private int selectedTask = -1;
        private List<TaskSelections> taskSelection = new List<TaskSelections>();
        private string selectedDate;
        private string[] plusSevenDays = new string[7]; 

        private DBConnection dbConn = new DBConnection();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            /// <summary>
            /// Creates the DB tables and fills in the priorities and status
            /// </summary>
            dbConn.CreateTable();
            dbConn.InsertPriorities();
            dbConn.InsertStatus();

            /// <summary>
            /// Sets the selected date to today
            /// </summary>
            selectedDate = DateTime.Today.ToString();                    // Here date is given in dd-MM-yyyy format 
            selectedDate = selectedDate.Substring(0, 10);
            selectedDate = selectedDate.Substring(6, 4) + "-" + selectedDate.Substring(3, 2) + "-" + selectedDate.Substring(0, 2);   // Now date is in yyyy-MM-dd format, 
                                                                                                                                     // which is the format used by the database. 
            /// <summary>
            /// Resets and fills in the seven date array
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
            /// Sets the selected date to today
            /// </summary>
            selectedDate = DateTime.Today.ToString("yyyy-MM-dd");

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
            lblToday.Text = "Aujourd'hui (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";

        }

        /// <summary>
        /// Loads all the tasks for the different tabs and sets the dates in the calendar in bold, when a task is due for a day.
        /// </summary>
        public void LoadTasks()
        {
            // We reset the selected task as -1 for none since we don't have any selected task at reload
            selectedTask = -1;

            LoadTasksForDate();
            LoadTasksForTodayPlusSeven();
            LoadTasksInTopic();
            LoadDoneTasks();

            // We must empty the bolded dates in the calendar before adding the new ones
            calMonth.RemoveAllBoldedDates();
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
        /// Loads all the tasks in the finished tab
        /// </summary>
        public void LoadDoneTasks()
        {
            // Updates tasks that are done
            List<Tasks> tasksList = dbConn.ReadApprovedTask();
            CreateTasksLayout(tasksList, LAYOUT_DONE);
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
            
        }


        /// <summary>
        /// Sets the dates of the calendar in bold when there's one or more deadline for a task on a given day
        /// </summary>
        private void SetDatesInBold()
        {
            DBConnection dbConn = new DBConnection();

            // Copies the content of the list of string returned by the method dbConnReadData into the list of string deadlinesList
            List<string> deadlinesList = new List<string>(dbConn.ReadDataForDeadlines());

            // Browses the list of string and converts each item to DataTime format 
            foreach (string item in deadlinesList)
            {
                DateTime myDateTime = Convert.ToDateTime(item);

                // Adds each DateTime item as a bolded date in the calendar
                calMonth.AddBoldedDate(myDateTime);
                calMonth.UpdateBoldedDates();
            }

            dbConn.Close();
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
                        lblTaskInformation.Text = "Description :\n\n" + taskSelection[i].Task_information;
                        lblTaskInformation.AutoSize = false;
                        lblTaskInformation.Height = 350;
                        lblTaskInformation.Width = pnlInformations.Width - 2 * 15;
                    }
                    else
                    {
                        taskSelection[i].Task_label.BackColor = Color.Transparent;
                        lblTaskInformation.Text = "";
                    }
                }
                else
                {
                    taskSelection[i].Task_label.BackColor = Color.Transparent;
                }
            }
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
                    var confirmResult = MessageBox.Show("Etes-vous sûr(e) de vouloir supprimer la tâche - " + task.Title + " - ?",
                                                        "Confirmer la suppression.",
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        dbConn.DeleteTask(task.Id);

                        // Loads all the tasks for the different tabs from the database
                        LoadTasks();
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
                    picInformationIcon.BackgroundImage = LifeProManager.Properties.Resources.essential_regular_86_clock;
                }
                else
                {
                    if (task.Priorities_id == 3)
                    {
                        picInformationIcon.BackgroundImage = LifeProManager.Properties.Resources.essential_regular_61_double_exclamation;

                    }
                    else if (task.Priorities_id == 2)
                    {
                        picInformationIcon.BackgroundImage = LifeProManager.Properties.Resources.essential_regular_61_exclamation;
                    }
                }
                picInformationIcon.BackgroundImageLayout = ImageLayout.Zoom;

                // ====================================================================================================
                // Task label, detailed layout
                lblTask.Text = task.Title;
                lblTask.Width = 570;
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
                cmdUnapproveTask.BackgroundImage = LifeProManager.Properties.Resources.essential_regular_17_minus_circle;
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
        /// Handles the event when the user selects a date in the calendar
        /// </summary>
        private void calMonth_DateChanged(object sender, DateRangeEventArgs e)
        {
            if (calMonth.SelectionStart == DateTime.Today.AddDays(-2))
            {
                lblToday.Text = "Avant-hier (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
            }

            else if (calMonth.SelectionStart == DateTime.Today.AddDays(-1))
            {
                lblToday.Text = "Hier (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
            }

            else if (calMonth.SelectionStart == DateTime.Today)
            {
                lblToday.Text = "Aujourd'hui (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
            }

            else if (calMonth.SelectionStart == DateTime.Today.AddDays(1))
            {
                lblToday.Text = "Demain (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
            }

            else if (calMonth.SelectionStart == DateTime.Today.AddDays(2))
            {
                lblToday.Text = "Après-demain (" + calMonth.SelectionStart.ToString("dd-MMM-yyyy") + ")";
            }

            else
            {
                lblToday.Text = calMonth.SelectionStart.ToString("dd-MMM-yyyy");
            }

            // Shows the current date tab
            tabMain.SelectTab(tabDates);

            // Sets the selected date to the date selected in the calendar and formats it for use in the database
            selectedDate = calMonth.SelectionStart.ToString("yyyy-MM-dd");

            // Loads the tasks for the selected date
            LoadTasksForDate();
        }

        /// <summary>
        /// Sets the date to today when the user clicks on the calendar button
        /// </summary>
        private void cmdToday_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(DateTime.Today);
        }

        /// <summary>
        /// Sets the date to the previous day when the user clicks on the left arrow button
        /// </summary>
        private void CmdPreviousDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(-1));
        }

        /// <summary>
        /// Sets the date to the next day when the user clicks on the right arrow button
        /// </summary>
        private void CmdNextDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(1));
        }

        /// <summary>
        /// Shows the form to add a task when the user clicks on the plus button
        /// </summary>
        private void cmdAddTask_Click(object sender, EventArgs e)
        {
            new frmAddTask(this).Show();
        }

        /// <summary>
        /// Closes the database connection when the user quits the program
        /// </summary>
        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            dbConn.Close();
        }

        /// <summary>
        /// Shows the form to add a topic when the user clicks on the small plus button, next to the topics drop-down list
        /// </summary>
        private void cmdAddTopic_Click(object sender, EventArgs e)
        {
            new frmAddTopic(this).Show();
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
        /// Shows the tasks for the previous topic, from the drop-down list
        /// </summary>
        private void cmdPreviousTopic_Click(object sender, EventArgs e)
        {
            int nbTopic = cboTopics.Items.Count;

            if (cboTopics.SelectedIndex > 0)
            {
                cboTopics.SelectedIndex -= 1;
            }
            else
            {
                cboTopics.SelectedIndex = nbTopic - 1;
            }
        }

        /// <summary>
        /// Shows the tasks for the next topic, from the drop-down list
        /// </summary>
        private void cmdNextTopic_Click(object sender, EventArgs e)
        {
            int nbTopic = cboTopics.Items.Count;

            if (cboTopics.SelectedIndex < nbTopic - 1)
            {
                cboTopics.SelectedIndex += 1;
            }
            else
            {
                cboTopics.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Deletes the currently displayed topic and all the tasks associated with
        /// </summary>
        private void cmdDeleteTopic_Click(object sender, EventArgs e)
        {
            // Gets the selected topic
            Lists currentTopic = cboTopics.SelectedItem as Lists;

            if (cboTopics.Items.Count != 0)
            {
                var confirmResult = MessageBox.Show("La suppression de la liste - " + currentTopic.Title + " - entrainera également la suppression des tâches qui lui sont liées.",
                                                    "Confirmer la suppression.",
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);
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
                        cboTopics.Text = "Afficher par thème";

                    } else
                    {
                        // Changes current topic since the previous one has been deleted
                        cboTopics.SelectedIndex = 0;
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("Vous n'avez actuellement aucune liste à supprimer.","Erreur",MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }

        /// <summary>
        /// Handles the event when the user selects a tab
        /// </summary>
        private void tabMain_Selected(object sender, TabControlEventArgs e)
        {
            // If the user selects the topics tab
            if (tabMain.SelectedTab == tabTopics)
            {
                // If the drop-down list of topics is empty
                if (cboTopics.Items.Count == 0)
                {
                    MessageBox.Show("Vous n'avez défini aucune liste de thème.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Shows the dates tab
                    tabMain.SelectTab(tabDates);
                
                // If the drop-down list of topics isn't empty
                } else
                {
                    // Shows in the tab the first list found in the drop-down list
                    cboTopics.SelectedIndex = 0;
                }      
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
        /// Executes a command prompt with administrator rights and processes the command given in argument
        /// </summary>
        /// <param name="command">The command to process</param>
        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.Verb = "runas";

            var process = Process.Start(processInfo);

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
    }
}
