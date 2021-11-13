/// <file>frmAddTopic.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.2</version>
/// <date>November 11th, 2021</date>


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
    public partial class frmAddTopic : Form
    {
        private string resxFile = "";

        private DBConnection dbConn = new DBConnection();
        private frmMain mainForm = null;

        //Code from https://stackoverflow.com/questions/4822980/how-to-access-a-form-control-for-another-form
        public frmAddTopic(Form callingForm)
        {
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
        /// Inserts a topic into the database
        /// </summary>
        public void cmdAddTopic_Click(object sender, EventArgs e)
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

                if (txtTopic.Text == "")
                {
                    MessageBox.Show(resourceManager.GetString("youMustFillInANameForYourNewTopic"), resourceManager.GetString("error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Inserts the topic into the database
                    dbConn.InsertTopic(txtTopic.Text);

                    // Reloads the topics list in the main form
                    mainForm.LoadTopics();

                    // Closes the window
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Puts the focus on the button to Add a topic
        /// </summary>
        private void frmAddTopic_Load(object sender, EventArgs e)
        {
            cmdAddTopic.Focus();
        }
    }
}
