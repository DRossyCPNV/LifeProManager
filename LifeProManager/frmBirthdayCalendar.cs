/// <file>frmBirthdayCalendar.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.1</version>
/// <date>February 24th, 2026</date>

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LifeProManager
{
    public partial class frmBirthdayCalendar : Form
    {
        private DBConnection dbConn => Program.DbConn;

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
            // Local copy of the list
            List<Tasks> birthdaysList = listOfBirthdays;

            // Current year used to compute the age
            int currentYear = DateTime.Now.Year;

            foreach (Tasks task in birthdaysList)
            {
                DateTime birthdayDate;

                // Parses the stored date using DateTime to ensure reliability across formats. 
                // Substring-based extraction breaks as soon as the date format or localization
                // settings change.
                if (!DateTime.TryParse(task.Deadline, out birthdayDate))
                {
                    // If the date cannot be parsed, skips this entry safely
                    continue;
                }

                // Extracts day and month in 2-digit format
                string dayOfBirthday = birthdayDate.Day.ToString("00");
                string monthOfBirthday = birthdayDate.Month.ToString("00");

                // Year of birth is stored in the Description field
                int yearOfBirth;
                int.TryParse(task.Description, out yearOfBirth);

                // Computes the age the person will reach this year
                int age = currentYear - yearOfBirth;

                // Appends the birthday to the correct month label
                switch (monthOfBirthday)
                {
                    case "01":
                        lblJanuaryData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "02":
                        lblFebruaryData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "03":
                        lblMarchData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "04":
                        lblAprilData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "05":
                        lblMayData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "06":
                        lblJuneData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "07":
                        lblJulyData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "08":
                        lblAugustData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "09":
                        lblSeptemberData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "10":
                        lblOctoberData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "11":
                        lblNovemberData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
                        break;

                    case "12":
                        lblDecemberData.Text += $"{dayOfBirthday} - {task.Title} ({age})\n";
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
