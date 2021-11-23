/// <file>frmAddTask.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.2</version>
/// <date>November 17th, 2021</date>

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
    public partial class frmAddTask : Form
    {
        private string resxFile = "";

        private DBConnection dbConn = new DBConnection();

        // Declaration of the type of main form
        private frmMain mainForm = null;

        public frmAddTask(Form callingForm)
        {
            // Allows to access and re-use the methods coded in frmMain
            mainForm = callingForm as frmMain;
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

                    // Status is set to 1, which means "To Do"
                    dbConn.InsertTask(txtTitle.Text, txtDescription.Text, deadline, chkImportant.Checked ? 1 : 0, currentTopic.Id, 1);

                    // Reloads topics in the main form
                    mainForm.LoadTasks();

                    // Closes the window
                    this.Close();
                }
            }
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

            // Loads the date selected in the calendar of the main form into the deadline date time picker
            dtpDeadline.Value = mainForm.SelectedDateTypeTime;

            // Loads the topics in the combo box
            cboTopics.Items.Clear();
            foreach (Lists topic in dbConn.ReadTopics())
            {
                cboTopics.Items.Add(topic);
                cboTopics.DisplayMember = "Title";
                cboTopics.ValueMember = "Id";
            }
            
            // Loads the priority denomination in the checkbox label
            chkImportant.Text = dbConn.ReadPrioritiesDenomination();

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

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            if (txtTitle.TextLength == txtTitle.MaxLength)
            {
                txtDescription.Focus();
            }
        }
    }
}
