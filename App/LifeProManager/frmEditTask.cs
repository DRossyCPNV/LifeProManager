/// <file>frmEditTask.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.0</version>
/// <date>November 7th, 2019</date>


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

            // Fills in the year (goes from the year 2000 until the current year +100 years)
            String today = DateTime.Today.ToString();
            String yearToday = today.Substring(6, 4);
            int year;
            int yearPlus100;
            if (int.TryParse(yearToday, out year))
            {
                yearPlus100 = year + 100;
                for (int i = 2000; i <= yearPlus100; ++i)
                {
                    cboYear.Items.Add(i.ToString());
                }
            }
            else
            {
                MessageBox.Show(this, "Une erreur est survenue lors de la génération des dates", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
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

            int month;
            if (int.TryParse(task.Deadline.Substring(3, 2), out month))
            {
                cboDay.SelectedIndex = cboDay.Items.IndexOf(task.Deadline.Substring(0, 2));
                cboMonth.SelectedIndex = month - 1;
                cboYear.SelectedIndex = cboYear.Items.IndexOf(task.Deadline.Substring(6, 4));
            }
            else
            {
                MessageBox.Show("Une erreur est survenue lors de l'extraction de la date\n" +
                                task.Deadline);
                this.Close();
            }

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
                // Adds an extra 1 to the month number, since the first month is referenced as 0 in the combo box 
                // but as 1 in month number in every day life
                int monthNumber = cboMonth.SelectedIndex + 1;
                string month;

                // Adds an extra 0 for month 1 to month 9, since the database string format in SQLite for date is YYYY-MM-DD
                if (monthNumber < 10)
                {
                    month = "0" + monthNumber.ToString();
                }
                else
                {
                    month = monthNumber.ToString();
                }
                string deadline = cboYear.Text + "-" + month + "-" + cboDay.Text;

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
