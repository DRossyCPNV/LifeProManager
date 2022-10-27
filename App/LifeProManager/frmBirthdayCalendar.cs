/// <file>frmBirthdayCalendar.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6</version>
/// <date>October 27th, 2022</date>

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
    public partial class frmBirthdayCalendar : Form
    {
        private DBConnection dbConn = new DBConnection();

        public frmBirthdayCalendar()
        {
            InitializeComponent();
        }

        private void frmBirthdayCalendar_Load(object sender, EventArgs e)
        {
            CreateBirthdaysLayout(dbConn.ReadTask("WHERE Priorities_id == 4"));
        }

        public void CreateBirthdaysLayout(List<Tasks> listOfBirthdays)
        {
            
            // Updates birthdays list for the current date
            List<Tasks> birthdaysList = listOfBirthdays;

            foreach (Tasks task in birthdaysList)
            {
                
            }
        }
    }
}
