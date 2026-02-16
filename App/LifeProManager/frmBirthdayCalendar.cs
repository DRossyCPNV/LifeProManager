/// <file>frmBirthdayCalendar.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6.2</version>
/// <date>January 23th, 2026</date>

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
            // Fills the birthdays progressively
            CreateBirthdaysLayout(dbConn.ReadTask("WHERE Priorities_id == 4 AND Status_id == 1"));
        }

        public void CreateBirthdaysLayout(List<Tasks> listOfBirthdays)
        {
            
            // Updates birthdays list for the current date
            List<Tasks> birthdaysList = listOfBirthdays;
            
            string monthOfBirthday = "";
            int currentYear;
            int yearOfBirth;
            string dayOfBirthday = "";

            foreach (Tasks task in birthdaysList)
            {
                monthOfBirthday = task.Deadline.Substring(3, 2);
                dayOfBirthday = task.Deadline.Substring(0, 2);

                // Gets current year value
                int.TryParse(DateTime.Now.Year.ToString(), out currentYear);

                // Gets year of birth value
                int.TryParse(task.Description, out yearOfBirth);

                switch (monthOfBirthday)
                {
                    case "01": 
                        // Displays current task title plus the age that person will be between brackets
                        lblJanuaryData.Text += dayOfBirthday + " - " + task.Title + " ("+ (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "02":
                        lblFebruaryData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "03":
                        lblMarchData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "04":
                        lblAprilData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;
                    case "05":
                        lblMayData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "06":
                        lblJuneData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "07":
                        lblJulyData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "08":
                        lblAugustData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "09":
                        lblSeptemberData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "10":
                        lblOctoberData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;
                    
                    case "11":
                        lblNovemberData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;

                    case "12":
                        lblDecemberData.Text += dayOfBirthday + " - " + task.Title + " (" + (currentYear - yearOfBirth).ToString() + ")" + "\n";
                        break;
                }
            }
        }

        private void frmBirthdayCalendar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmBirthdayCalendar_KeyDown(object sender, KeyEventArgs e)
        {
            this.Close();
        }

        private void lblJanuaryData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblFebruaryData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblMarchData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblAprilData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblMayData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblJuneData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblJulyData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblAugustData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblSeptemberData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblOctoberData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblNovemberData_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblDecemberData_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
