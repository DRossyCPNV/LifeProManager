﻿/// <file>frmAddTask.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6.1</version>
/// <date>January 17th, 2025</date>

using System;
using System.Resources;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmAddTask : Form
    {
        private string resxFile = "";

        private DBConnection dbConn = new DBConnection();

        private Tasks task;

        // Declaration of the type of main form
        private frmMain mainForm = null;

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
        }


        /// <summary>
        /// Adds the task specified in the textboxes for the date specified in the comboboxes into the database
        /// </summary>
        private void cmdConfirm_Click(object sender, EventArgs e)
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
                // Checks if the task's title is empty
                if (txtTitle.Text == "")
                {
                    MessageBox.Show(resourceManager.GetString("youMustGiveATitleToYourTask"), resourceManager.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    if (priorityChosen != 4)
                    {
                        dbConn.InsertTask(txtTitle.Text, txtDescription.Text, deadline, priorityChosen, currentTopic.Id, 1);
                    }

                    // If the priority of the task is a birthday (4)
                    else
                    {
                        dbConn.InsertTask(txtTitle.Text, numYear.Value.ToString(), deadline, priorityChosen, currentTopic.Id, 1);
                    }

                    // Reloads topics in the main form
                    mainForm.LoadTasks();

                    // Closes the window
                    this.Close();
                }
            }
        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            if (txtTitle.TextLength == txtTitle.MaxLength)
            {
                txtDescription.Focus();
            }

            // If the user types an exclamation mark in the title of a task and the important checkbox isn't checked
            else if (txtTitle.Text.Contains("!") && chkImportant.Checked == false)
            {
                txtTitle.Text = txtTitle.Text.Replace("!", "");
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
        /// Avoids the form to be moved somewhere else
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAddTask_Move(object sender, EventArgs e)
        {
            this.CenterToScreen();
        }

        private void frmAddTask_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Avoid getting the data from last validated task if the user has closed the form
            if (mainForm.CopyLastTaskValues)
            {
                mainForm.CopyLastTaskValues = false;
            }
        }

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

                // If the app native language is set on French
                if (dbConn.ReadSetting(1) == 2)
                {
                    lblTitle.Text = "Prénom";
                }

                // If the app native language is set on English
                else
                {
                    lblTitle.Text = "First name";
                }      
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

                // If the app native language is set on French
                if (dbConn.ReadSetting(1) == 2)
                {
                    lblTitle.Text = "Titre";
                }

                // If the app native language is set on English
                else
                {
                    lblTitle.Text = "Title";
                }
            }
        }
    }
}
