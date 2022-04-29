/// <file>frmEditTask.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.4</version>
/// <date>April 25th, 2022</date>

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

        public frmEditTask(Form callingForm, Tasks task)
        {
            // Allows us to re-use the methods of frmMain
            mainForm = callingForm as frmMain;
            this.task = task;

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

                // Loads the task in the form
                txtTitle.Text = task.Title;
                txtDescription.Text = task.Description;

                // If priority 1 or 3 has been assigned for this task (odd number)
                if (task.Priority_id % 2 != 0)
                {                 
                    chkImportant.Checked = true;                    
                }

                // If priority 2 or above has been assigned for this task
                if (task.Priority_id >= 2)
                {
                    chkRepeatable.Checked = true;
                }

                // Sets the deadline affected to the task in the date picker 
                dtpDeadline.Value = Convert.ToDateTime(task.Deadline);

                // Sets the topic affected to the task in the topic combobox
                cboTopics.SelectedText = dbConn.ReadTopicName(task.Lists_id);
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

                    // Calculates the priority to assign to the task
                    if (chkImportant.Checked == true)
                    {
                        priorityChosen = 1;

                        if (chkRepeatable.Checked == true)
                        {
                            priorityChosen = 3;
                        }
                    }

                    // If the important checkbox isn't checked
                    else
                    {
                        if (chkRepeatable.Checked == true)
                        {
                            priorityChosen = 2;
                        }
                    }                  

                    // Edit the task informations in the database
                    dbConn.EditTask(task.Id, txtTitle.Text, txtDescription.Text, deadline, priorityChosen, currentTopic.Id);

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
