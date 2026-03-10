/// <file>frmAddTopic.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.8</version>
/// <date>March 10th, 2026</date>

using System;
using System.Resources;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmAddTopic : Form
    {
        private frmMain mainForm = null;

        public frmAddTopic(Form callingForm)
        {
            mainForm = callingForm as frmMain;
            InitializeComponent();
        }

        /// <summary>
        /// Form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAddTopic_Load(object sender, EventArgs e)
        {
            LocalizationManager.LoadLocalizedStringsFor(this);
            txtTopic.SelectAll();
        }

        /// <summary>
        /// Closes the form without any change
        /// </summary>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Inserts a topic into the database
        /// </summary>
        public void cmdAddTopic_Click(object sender, EventArgs e)
        {
            if (txtTopic.Text == "")
            {
                MessageBox.Show(LocalizationManager.GetString("youMustFillInANameForYourNewTopic"), LocalizationManager.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Inserts the topic into the database
                mainForm.dbConn.InsertTopic(txtTopic.Text);

                mainForm.LoadTopics();
                mainForm.UpdateAddTaskButtonVisibility();

                // Selects the newly created topic
                foreach (Lists topic in mainForm.cboTopics.Items)
                {
                    if (topic.Title == txtTopic.Text)
                    {
                        mainForm.cboTopics.SelectedItem = topic;
                        break;
                    }
                }

                this.Close();
            } 
        }

        /// <summary>
        /// Avoids the form to be moved somewhere else
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAddTopic_Move(object sender, EventArgs e)
        {
            this.CenterToScreen();
        }

        /// <summary>
        /// Loads all the localized strings for the UI elements based on the current language setting.
        /// </summary>
        public void LoadLocalizedStrings()
        {
            // --- Window title ---
            this.Text = LocalizationManager.GetString("AddTopic");

            // --- Labels ---
            lblTopic.Text = LocalizationManager.GetString("lblTopicText");

            // --- TextBox placeholder / default text ---
            txtTopic.Text = LocalizationManager.GetString("txtTopicText");
        }
    }
}
