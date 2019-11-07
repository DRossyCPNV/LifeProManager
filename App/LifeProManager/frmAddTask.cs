/// <file>frmAddTask.cs</file>
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
    public partial class frmAddTask : Form
    {
        private DBConnection dbConn = new DBConnection();
        private frmMain mainForm = null;
        private String errorMsg;

        public frmAddTask(Form callingForm)
        {
            // Allows to re-use the methods coded in frmMain
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
            // Checks if the task's title is empty
            if (txtTitle.Text == "")
            {
                errorMsg += "Vous devez donner un titre à votre tâche.";
                MessageBox.Show(this, errorMsg, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Adds an extra 1 to the month number, since the first month is referenced as 0 in the combo box 
                // but as 1 for the month number in every day life
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

                // Gets the selected topic from the combo box
                Lists currentTopic = cboTopics.SelectedItem as Lists;

                // Since ids of priorities and topics start at 0 in their respective combo box, but at 1 in the database, we simply add 1 to each of them
                // Status is automatically set to 1, which refers to "A faire"
                dbConn.InsertTask(txtTitle.Text, txtDescription.Text, deadline, cboPriorities.SelectedIndex + 1, currentTopic.Id, 1);

                // Reloads topics in the main form
                mainForm.LoadTasks();
                
                // Closes the window
                this.Close();
            }
        }

        /// <summary>
        /// Loads the topics and priorities in the combo boxes, then selects the first topic, lower priority and today's date automatically
        /// </summary>
        private void frmAddTask_Load(object sender, EventArgs e)
        {
            // No error message at initialization
            errorMsg = "";

            //Load the topics in the combo box
            cboTopics.Items.Clear();
            foreach (Lists topic in dbConn.ReadTopics())
            {
                cboTopics.Items.Add(topic);
                cboTopics.DisplayMember = "Title";
                cboTopics.ValueMember = "Id";
            }

            // If no list has been created yet it closes the add task window
            if (cboTopics.Items.Count == 0)
            {
                errorMsg += "Vous n'avez pas encore créé de liste.\n" +
                            "Vous ne pouvez pas créer de tâche sans l'assigner à une liste.";
                MessageBox.Show(errorMsg, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                this.Close();
            }
            else
            {
                // Loads the priorities in the combo box
                cboPriorities.Items.Clear();
                foreach (string priority in dbConn.ReadPrioritiesDenomination())
                {
                    cboPriorities.Items.Add(priority);
                }

                // Selects the first topic in the combobox automatically
                cboTopics.SelectedIndex = 0;

                // Selects the lower priority automatically
                cboPriorities.SelectedIndex = 0;

                // Gets the date of today
                String today = DateTime.Today.ToString();
                String dayToday = today.Substring(0, 2);
                String monthToday = today.Substring(3, 2);
                String yearToday = today.Substring(6, 4);
                int month;
                int year;
                int yearPlus100;

                if (int.TryParse(yearToday, out year) && int.TryParse(monthToday, out month))
                {
                    // Fills in the year (goes from this year until +100 years)
                    yearPlus100 = year + 100;
                    for (int i = year; i <= yearPlus100; ++i)
                    {
                        cboYear.Items.Add(i.ToString());
                    }

                    // Selects today as a date automatically
                    cboDay.SelectedIndex = cboDay.Items.IndexOf(dayToday);
                    cboMonth.SelectedIndex = month - 1;
                    cboYear.SelectedIndex = 0;
                }
                else
                {
                    errorMsg += "Erreur de conversion des dates";
                    MessageBox.Show(this, errorMsg, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


    }
}
