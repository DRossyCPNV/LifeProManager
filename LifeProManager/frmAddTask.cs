/// <file>frmAddTask.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7</version>
/// <date>February 22th, 2026</date>

using System;
using System.Resources;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmAddTask : Form
    {
        private DBConnection dbConn => Program.DbConn;

        // Declaration of the type of main form
        private frmMain mainForm = null;
        private Tasks task;


        public frmAddTask(Form callingForm, Tasks task)
        {
            // Allows us to re-use the methods of frmMain
            mainForm = callingForm as frmMain;
            this.task = task;

            InitializeComponent();
        }

        /// <summary>
        /// Closes the form without any change
        /// </summary>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
        /// <summary>
        /// Loads the topics and priorities in the combo boxes, then selects the first topic, lower priority and today's date automatically
        /// </summary>
        private void frmAddTask_Load(object sender, EventArgs e)
        {
            LocalizationManager.LoadLocalizedStringsFor(this);

            // Loads the topics in the combo box
            cboTopics.Items.Clear();
            foreach (Lists topic in dbConn.ReadTopics())
            {
                cboTopics.Items.Add(topic);
                cboTopics.DisplayMember = "Title";
                cboTopics.ValueMember = "Id";
            }

            numYear.Maximum = DateTime.Now.Year;

            if (mainForm.CopyLastTaskValues)
            {
                // If a priority of 1 or 3 has been assigned to this task (it's important)
                if (task.Priorities_id % 2 != 0)
                {
                    chkImportant.Checked = true;
                }

                // If a priority of 2 or 3 has been assigned to this task (it's repeatable)
                if (task.Priorities_id >= 2)
                {
                    chkRepeatable.Checked = true;
                }

                // If a priority of 4 has been assigned to this task (it's a birthday)
                if (task.Priorities_id == 4)
                {
                    chkBirthday.Checked = true;

                    // Sets the new deadline on the same day, on next year, by default
                    dtpDeadline.Value = DateTime.Today.AddYears(1);
                }

                else
                {
                    // Sets the new deadline on the next day by default
                    dtpDeadline.Value = DateTime.Today.AddDays(1);

                    txtDescription.Text = task.Description;
                }
       
                // Pre-fills the task title, description and topic by making a copy of last task
                txtTitle.Text = task.Title;

                // Sets the topic affected to the task in the topic combobox
                cboTopics.Text = dbConn.ReadTopicName(task.Lists_id);             
            }

            // Current selected date in the calendar will be used, with a blank title and blank description
            else
            {
                // Loads the date selected in the calendar of the main form into the deadline date time picker
                dtpDeadline.Value = mainForm.SelectedDateTypeTime;

                // If a topic has been selected
                if (mainForm.cboTopics.SelectedIndex != -1)
                {
                    // Sets the topic for the new task as the one selected for display in the main form
                    cboTopics.SelectedIndex = mainForm.cboTopics.SelectedIndex;
                }
                else
                {
                    // Sets the topic for the new task as the first available
                    cboTopics.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Adds the task specified in the textboxes for the date specified in the comboboxes into the database
        /// </summary>
        private void cmdConfirm_Click(object sender, EventArgs e)
        {
            // Checks if the task's title is empty
            if (txtTitle.Text == "")
            {
                MessageBox.Show(LocalizationManager.GetString("youMustGiveATitleToYourTask"), LocalizationManager.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            else
            {
                // Escapes single quotes by doubling them to prevent the SQL insert from crashing the app
                if (txtTitle.Text.Contains("'"))
                {
                    txtTitle.Text = txtTitle.Text.Replace("'", "''");
                }

                // Gets the value of the date time picker and affects it to the deadline string variable
                // in the format used by the database
                string deadline = dtpDeadline.Value.ToString("yyyy-MM-dd");

                // Gets the selected topic from the combo box
                Lists currentTopic = cboTopics.SelectedItem as Lists;

                int priorityChosen = 0;

                // If only the important checkbox is checked
                if (chkImportant.Checked == true && chkRepeatable.Checked == false && chkBirthday.Checked == false)
                {
                    priorityChosen = 1;
                }

                // If only the repeatable checkbox is checked
                else if (chkImportant.Checked == false && chkRepeatable.Checked == true && chkBirthday.Checked == false)
                {                        
                    priorityChosen = 2;                  
                }

                // If both the important and repeatable checkboxes are checked
                else if (chkImportant.Checked == true && chkRepeatable.Checked == true && chkBirthday.Checked == false)
                {
                    priorityChosen = 3;
                }

                // If the birthday checkbox is checked
                else if (chkBirthday.Checked == true)
                {
                    priorityChosen = 4;
                }

                // Task insertion into the database 
                if (priorityChosen == 4)
                {
                    dbConn.InsertTask(txtTitle.Text, numYear.Value.ToString(), deadline, priorityChosen, currentTopic.Id, 1);
                    
                }

                else
                {
                    dbConn.InsertTask(txtTitle.Text, txtDescription.Text, deadline, priorityChosen, currentTopic.Id, 1);
                }

                // Reloads topics in the main form
                mainForm.LoadTasks();

                // Closes the window
                this.Close();
            }
        }

        /// <summary>
        /// Loads all the localized strings for the UI elements based on the current language setting.
        /// </summary>
        public void LoadLocalizedStrings()
        {
            // --- Window title ---
            this.Text = LocalizationManager.GetString("AddTask");

            // --- Labels ---
            lblDescription.Text = LocalizationManager.GetString("lblDescriptionText");
            lblPriority.Text = LocalizationManager.GetString("lblPriorityText");
            lblDeadline.Text = LocalizationManager.GetString("lblDeadlineText");
            lblTitle.Text = LocalizationManager.GetString("lblTitleText");
            lblTopic.Text = LocalizationManager.GetString("lblTopicText");
            lblYear.Text = LocalizationManager.GetString("lblYearText");

            // --- Checkboxes ---
            chkImportant.Text = LocalizationManager.GetString("chkImportantText");
            chkRepeatable.Text = LocalizationManager.GetString("chkRepeatableText");
            chkBirthday.Text = LocalizationManager.GetString("chkBirthdayText");
        }

        /// <summary>
        /// If the user types certain characters in the title of a task, 
        /// it automatically checks the corresponding priority checkboxes 
        /// and moves the focus to the description field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            if (txtTitle.TextLength == txtTitle.MaxLength)
            {
                txtDescription.Focus();
            }

            // If the user types an exclamation mark in the title of a task and the important checkbox isn't checked
            else if (txtTitle.Text.Contains("!") && chkImportant.Checked == false)
            {
                chkImportant.Checked = true;
                txtDescription.Focus();
            }

            // If the user types a question mark in the title of a task and the repeatable checkbox isn't checked
            else if (txtTitle.Text.Contains("?") && chkRepeatable.Checked == false)
            {
                chkRepeatable.Checked = true;
                txtDescription.Focus();
            }
        }

        /// <summary>
        /// Prevents unwanted copying of values when adding a new task after editing a task.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAddTask_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (mainForm.CopyLastTaskValues)
            {
                mainForm.CopyLastTaskValues = false;
            }
        }

        /// <summary>
        /// If the birthday checkbox is checked, the description field, the important and repeatable checkboxes are hidden
        /// and the year numeric up down control is shown. 
        /// The title field changes to "First name" and its maximum length is reduced to 20 characters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkBirthday_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBirthday.Checked)
            {
                txtDescription.Visible = false;
                lblDescription.Visible = false;
                chkImportant.Visible = false;
                chkRepeatable.Visible = false;
                lblYear.Visible = true;
                numYear.Visible = true;
                lblPriority.Top += 36;
                txtTitle.MaxLength = 20;
                txtTitle.Width = 150;
                lblTitle.Text = LocalizationManager.GetString("firstName");
            }
            else
            {
                txtDescription.Visible = true;
                lblDescription.Visible = true;
                chkImportant.Visible = true;
                chkRepeatable.Visible = true;
                lblYear.Visible = false;
                numYear.Visible = false;
                lblPriority.Top -= 36;
                txtTitle.MaxLength = 70;
                txtTitle.Width = 206;
                lblTitle.Text = LocalizationManager.GetString("title");
            }
        }
    }
}

