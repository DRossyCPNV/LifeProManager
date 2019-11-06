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
    public partial class frmAddTopic : Form
    {
        DBConnection dbConn = new DBConnection();
        private frmMain mainForm = null;

        //Code from https://stackoverflow.com/questions/4822980/how-to-access-a-form-control-for-another-form
        public frmAddTopic(Form callingForm)
        {
            mainForm = callingForm as frmMain;
            InitializeComponent();
        }


        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdAddTopic_Click(object sender, EventArgs e)
        {
            if (txtTopic.Text == "")
            {
                MessageBox.Show("Veuillez introduire un nom pour le nouveau thème");
            }
            else
            {
                //Insert the topic into the data base
                dbConn.InsertTopic(txtTopic.Text);

                //Reload topics in the main form
                mainForm.LoadTopics();

                //Close the window
                this.Close();
            }
        }

        private void frmAddTopic_Load(object sender, EventArgs e)
        {
            cmdAddTopic.Focus();
        }
    }
}
