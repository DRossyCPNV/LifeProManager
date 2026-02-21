/// <file>frmBirthdayCalendar.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7</version>
/// <date>February 22th, 2026</date>

using System;
using System.Collections.Generic;
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
            LocalizationManager.LoadLocalizedStringsFor(this);

            // Fills the birthdays progressively
            CreateBirthdaysLayout(dbConn.ReadTask("WHERE Priorities_id == 4 AND Status_id == 1"));
        }

        private void cmdConfirm_Click(object sender, EventArgs e)
        {
            this.Close();
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

        /// <summary>
        /// Loads all the localized strings for the UI elements based on the current language setting.
        /// </summary>
        public void LoadLocalizedStrings()
        {
            // --- Window title ---
            this.Text = LocalizationManager.GetString("BirthdayCalendar");

            // --- Labels ---
            lblBirthdayCalendar.Text = LocalizationManager.GetString("lblBirthdayCalendarText");

            lblJanuary.Text = LocalizationManager.GetString("lblJanuaryText");
            lblFebruary.Text = LocalizationManager.GetString("lblFebruaryText");
            lblMarch.Text = LocalizationManager.GetString("lblMarchText");
            lblApril.Text = LocalizationManager.GetString("lblAprilText");
            lblMay.Text = LocalizationManager.GetString("lblMayText");
            lblJune.Text = LocalizationManager.GetString("lblJuneText");
            lblJuly.Text = LocalizationManager.GetString("lblJulyText");
            lblAugust.Text = LocalizationManager.GetString("lblAugustText");
            lblSeptember.Text = LocalizationManager.GetString("lblSeptemberText");
            lblOctober.Text = LocalizationManager.GetString("lblOctoberText");
            lblNovember.Text = LocalizationManager.GetString("lblNovemberText");
            lblDecember.Text = LocalizationManager.GetString("lblDecemberText");
        }
    }
}
