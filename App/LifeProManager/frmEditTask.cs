/// <file>frmEditTask.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.1</version>
/// <date>November 14th, 2019</date>


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmEditTask : Form
    {
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
            // Loads the priorities in the combo box
            cboPriorities.Items.Clear();
            foreach (string priority in dbConn.ReadPrioritiesDenomination())
            {
                cboPriorities.Items.Add(priority);
            }

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
            cboPriorities.SelectedIndex = task.Priorities_id - 1;

            // Sets the deadline affected to the task in the date picker 
            dtpDeadline.Value = Convert.ToDateTime(task.Deadline);
             
            // Automatically selects the first topic in the list
            cboTopics.SelectedIndex = 0;
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
            // Checks if the task's title is empty
            if (txtTitle.Text == "")
            {
                MessageBox.Show("Vous devez donner un titre à votre tâche.");
            }
            else
            {
                // Gets the value of the date time picker and affects it to the deadline string variable
                // in the format used by the database
                string deadline = dtpDeadline.Value.ToString("yyyy-MM-dd");

                // Gets the selected topic
                Lists currentTopic = cboTopics.SelectedItem as Lists;

                // Since ids of priorities and topics start at 0 in their respective combo box, but at 1 in the database we simply add 1 to each of them
                // Status is automatically set to 1 which refers to "A faire"
                dbConn.EditTask(task.Id, txtTitle.Text, txtDescription.Text, deadline, cboPriorities.SelectedIndex + 1, currentTopic.Id);

                // Reloads tasks in the main form
                mainForm.LoadTasks();

                // Closes the window
                this.Close();
            }
        }
    }
}
