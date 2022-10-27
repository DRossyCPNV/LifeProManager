/// <file>frmAddTopic.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6</version>
/// <date>October 27th, 2022</date>

using System;
using System.Resources;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmAddTopic : Form
    {
        private string resxFile = "";
        private frmMain mainForm = null;

        //Code from https://stackoverflow.com/questions/4822980/how-to-access-a-form-control-for-another-form
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
            // If the app native language is set on French
            if (mainForm.dbConn.ReadSetting(1) == 2)
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

                if (txtTopic.Text == "")
                {
                    MessageBox.Show(resourceManager.GetString("youMustFillInANameForYourNewTopic"), resourceManager.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Inserts the topic into the database
                    mainForm.dbConn.InsertTopic(txtTopic.Text);

                    // Reloads the topics list in the main form
                    mainForm.LoadTopics();

                    // Selects the created topic in the combobox
                    mainForm.cboTopics.SelectedIndex = mainForm.cboTopics.Items.Count - 1;

                    // Closes the window
                    this.Close();
                }
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
    }
}
