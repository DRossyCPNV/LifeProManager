/// <file>frmEditTask.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6.1</version>
/// <date>January 17th, 2025</date>

using System;
using System.Resources;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmEditTask : Form
    {
        private string resxFile = "";

        private DBConnection dbConn = new DBConnection();
        
        private Tasks task;
        private frmMain mainForm = null;

        public frmEditTask(Form callingForm, Tasks taskProvided)
        {
            // Allows us to re-use the methods of frmMain
            mainForm = callingForm as frmMain;
            this.task = taskProvided;

            InitializeComponent();
        }

        /// <summary>
        /// Loads the priorities and topics in the combo boxes, automatically selects the first topic, fills in the year and loads the task in the form
        /// </summary>
        private void frmEditTask_Load(object sender, EventArgs e)
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

                // Loads the task in the form
                txtTitle.Text = task.Title;
                txtDescription.Text = task.Description;

                // If priority 1 or 3 has been assigned for this task (odd number)
                if (task.Priorities_id % 2 != 0)
                {                 
                    chkImportant.Checked = true;                    
                }

                // If priority 2 or above has been assigned for this task
                if (task.Priorities_id >= 2)
                {
                    chkRepeatable.Checked = true;
                }

                // If priority 4 has been assigned for this task
                if (task.Priorities_id == 4)
                {
                    chkBirthday.Checked = true;

                    // Affects to the numeric up down control the value stored in the description field
                    int numYearValue = 0;
                    int.TryParse(task.Description, out numYearValue);
                    numYear.Value = numYearValue;
                }

                // Sets the deadline affected to the task in the date picker 
                dtpDeadline.Value = Convert.ToDateTime(task.Deadline);

                // Sets the topic affected to the task in the topic combobox
                cboTopics.Text = dbConn.ReadTopicName(task.Lists_id);
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
                txtTitle.Width = 206;

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
                txtTitle.Width = 275;

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

        /// <summary>
        /// Closes the form without any change
        /// </summary>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Edits the task in the database
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
                        // Edit the task informations in the database
                        dbConn.EditTask(this.task.Id, txtTitle.Text, txtDescription.Text, deadline, priorityChosen, currentTopic.Id);
                    }

                    // If the priority of the task is a birthday (4)
                    else
                    {
                        dbConn.EditTask(this.task.Id, txtTitle.Text, numYear.Value.ToString(), deadline, priorityChosen, currentTopic.Id);
                    }  

                    // Reloads tasks in the main form
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
        private void frmEditTask_Move(object sender, EventArgs e)
        {
            this.CenterToScreen();
        }
    }
}
