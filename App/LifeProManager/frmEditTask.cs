/// <file>frmEditTask.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.3</version>
/// <date>February 9th, 2022</date>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
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
            // --- Theme appliance ----------------------------------------------------------

            // If dark theme will be applied    
            if (dbConn.ReadSetting(2) == 1)
            {
                SkinApplier.ApplyTheme(1);
            }

            // Loads the priority denomination in the checkbox label
            chkImportant.Text = dbConn.ReadPrioritiesDenomination();

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
           
            // If a priority has been assigned for this task
            if (task.Priority_id > 0)
            {
                chkImportant.Checked = true;
            }

            else
            {
                chkImportant.Checked = false;
            }

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

                    // Gets the selected topic
                    Lists currentTopic = cboTopics.SelectedItem as Lists;

                    // Status is automatically set to 1 which refers to "A faire"
                    dbConn.EditTask(task.Id, txtTitle.Text, txtDescription.Text, deadline, chkImportant.Checked ? 1 : 0, currentTopic.Id);

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

            else if (txtTitle.Text.Contains("!") && chkImportant.Checked == false)
            {
                txtTitle.Text = txtTitle.Text.Replace("!", "");
                chkImportant.Checked = true;
                txtDescription.Focus();
            }
        }
    }
}
