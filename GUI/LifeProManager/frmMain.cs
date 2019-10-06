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
    public partial class frmMain : Form
    {
        Tasks activeTask1 = new Tasks();
        Tasks activeTask2 = new Tasks();
        Tasks activeTask3 = new Tasks();
        Tasks activeTask4 = new Tasks();
        Tasks activeTask5 = new Tasks();

        List<Label> topicsLabelsList = new List<Label>();
        List<Label> finishedLabelsList = new List<Label>();
        List<Label> finishedLabelsSelectedList = new List<Label>();


        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            DBConnection dbConn = new DBConnection();
            dbConn.CreateTable();
            dbConn.InsertData();
            dbConn.Close();

            calMonth.ShowToday = false;
            calMonth.MaxSelectionCount = 1;

        }

      

        private void ActiveLabelToSelect_Click(object sender, EventArgs e)
        {
            Label labelToSelect = sender as Label;

            if (labelToSelect.BackColor == Color.Transparent)
            {
                Label[] activeLabelsList = { lblActiveTask1, lblActiveTask2, lblActiveTask3, lblActiveTask4, lblActiveTask5 };
                foreach (Label label in activeLabelsList)
                {
                    label.BackColor = Color.Transparent;
                }

                labelToSelect.BackColor = Color.LightSteelBlue;

            } else
            {
               labelToSelect.BackColor = Color.Transparent;
            }
            
        }

        private void LblActiveTask1_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblActiveTask1, e);
        }

        private void LblActiveTask2_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblActiveTask2, e);
        }

        private void LblActiveTask3_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblActiveTask3, e);
        }

        private void LblActiveTask4_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblActiveTask4, e);
        }

        private void LblActiveTask5_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblActiveTask5, e);
        }

        private void LblWeekTask1_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblWeekTask1, e);
        }

        private void LblWeekTask2_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblWeekTask2, e);
        }

        private void LblWeekTask3_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblWeekTask3, e);
        }

        private void LblWeekTask4_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblWeekTask4, e);
        }

        private void LblWeekTask5_Click(object sender, EventArgs e)
        {
            ActiveLabelToSelect_Click(lblWeekTask5, e);
        }

        private void TopicsLabelToSelect_Click(object sender, EventArgs e)
        {
            Label labelToSelect = sender as Label;

            if (labelToSelect.BackColor == Color.Transparent)
            {
                Label[] topicsLabelsList = { lblTopicTask1, lblTopicTask2, lblTopicTask3, lblTopicTask4, lblTopicTask5, lblTopicTask6,
                lblTopicTask7, lblTopicTask8, lblTopicTask9, lblTopicTask10};

                foreach (Label label in topicsLabelsList)
                {
                    label.BackColor = Color.Transparent;
                }

                labelToSelect.BackColor = Color.LightSteelBlue;

            } else
            {
                labelToSelect.BackColor = Color.Transparent;
            }

        }

        private void LblTopicTask1_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask1, e);
        }

        private void LblTopicTask2_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask2, e);
        }

        private void LblTopicTask3_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask3, e);
        }

        private void LblTopicTask4_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask4, e);
        }

        private void LblTopicTask5_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask5, e);
        }

        private void LblTopicTask6_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask6, e);
        }

        private void LblTopicTask7_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask7, e);
        }

        private void LblTopicTask8_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask8, e);
        }

        private void LblTopicTask9_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask9, e);
        }

        private void LblTopicTask10_Click(object sender, EventArgs e)
        {
            TopicsLabelToSelect_Click(lblTopicTask10, e);
        }

        private void FinishedLabelToSelect_Click(object sender, EventArgs e)
        {
            Label labelToSelect = sender as Label;

            if (labelToSelect.BackColor == Color.Transparent)
            {
                labelToSelect.BackColor = Color.LightSteelBlue;
                
            }
            else
            {
                labelToSelect.BackColor = Color.Transparent;
            }
        }

        private void LblFinishedTask1_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask1, e);
        }

        private void LblFinishedTask2_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask2, e);
        }

        private void LblFinishedTask3_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask3, e);
        }

        private void LblFinishedTask4_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask4, e);
        }

        private void LblFinishedTask5_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask5, e);
        }

        private void LblFinishedTask6_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask6, e);
        }

        private void LblFinishedTask7_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask7, e);
        }

        private void LblFinishedTask8_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask8, e);
        }

        private void LblFinishedTask9_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask9, e);
        }

        private void LblFinishedTask10_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask10, e);
        }

        private void LblFinishedTask11_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask11, e);
        }

        private void LblFinishedTask12_Click(object sender, EventArgs e)
        {
            FinishedLabelToSelect_Click(lblFinishedTask12, e);
        }

        private void cmdToday_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(DateTime.Today);
        }

        private void CmdPreviousDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(-1));
        }

        private void CmdNextDay_Click(object sender, EventArgs e)
        {
            calMonth.SetDate(calMonth.SelectionStart.AddDays(1));
        }

        private void CalMonth_DateSelected(object sender, DateRangeEventArgs e)
        {
            string daySelected = calMonth.SelectionStart.ToString("dd-MM-yyyy");
            grpToday.Text = calMonth.SelectionStart.ToString("dd-MMM-yyyy");

            List<string> taskList = new List<string>();
            // Copy the content of the list of string returned by the method dbConnReadData into the list of string taskList
            DBConnection dbConn = new DBConnection();
            taskList = dbConn.ReadDataForADay(daySelected);
            dbConn.Close();
            int nbTasksInList = taskList.Count();

            // Fills in the Title property of each activeTask object, then the corresponding labels in the actives tab
            switch (nbTasksInList)
            {
                case 0:
                    activeTask1.Title = "";
                    activeTask2.Title = "";
                    activeTask3.Title = "";
                    activeTask4.Title = "";
                    activeTask5.Title = "";
                    lblActiveTask1.Text = "";
                    lblActiveTask2.Text = "";
                    lblActiveTask3.Text = "";
                    lblActiveTask4.Text = "";
                    lblActiveTask5.Text = "";
                    break;

                case 1:
                    activeTask1.Title = taskList[0];
                    activeTask2.Title = "";
                    activeTask3.Title = "";
                    activeTask4.Title = "";
                    activeTask5.Title = "";
                    lblActiveTask1.Text = activeTask1.Title;
                    lblActiveTask2.Text = "";
                    lblActiveTask3.Text = "";
                    lblActiveTask4.Text = "";
                    lblActiveTask5.Text = "";
                    break;

                case 2:
                    activeTask1.Title = taskList[0];
                    activeTask2.Title = taskList[1];
                    activeTask3.Title = "";
                    activeTask4.Title = "";
                    activeTask5.Title = "";
                    lblActiveTask1.Text = activeTask1.Title;
                    lblActiveTask2.Text = activeTask2.Title;
                    lblActiveTask3.Text = "";
                    lblActiveTask4.Text = "";
                    lblActiveTask5.Text = "";
                    break;

                case 3:
                    activeTask1.Title = taskList[0];
                    activeTask2.Title = taskList[1];
                    activeTask3.Title = taskList[2];
                    activeTask4.Title = "";
                    activeTask5.Title = "";
                    lblActiveTask1.Text = activeTask1.Title;
                    lblActiveTask2.Text = activeTask2.Title;
                    lblActiveTask3.Text = activeTask3.Title;
                    lblActiveTask4.Text = "";
                    lblActiveTask5.Text = "";
                    break;

                case 4:
                    activeTask1.Title = taskList[0];
                    activeTask2.Title = taskList[1];
                    activeTask3.Title = taskList[2];
                    activeTask4.Title = taskList[3];
                    activeTask5.Title = "";
                    lblActiveTask1.Text = activeTask1.Title;
                    lblActiveTask2.Text = activeTask2.Title;
                    lblActiveTask3.Text = activeTask3.Title;
                    lblActiveTask4.Text = activeTask4.Title;
                    lblActiveTask5.Text = "";
                    break;

                case 5:
                    activeTask1.Title = taskList[0];
                    activeTask2.Title = taskList[1];
                    activeTask3.Title = taskList[2];
                    activeTask4.Title = taskList[3];
                    activeTask5.Title = taskList[4];
                    lblActiveTask1.Text = activeTask1.Title;
                    lblActiveTask2.Text = activeTask2.Title;
                    lblActiveTask3.Text = activeTask3.Title;
                    lblActiveTask4.Text = activeTask4.Title;
                    lblActiveTask5.Text = activeTask5.Title;
                    break;

                // if there are more than 5 tasks in tasklist, only the first five are loaded
                default:
                    activeTask1.Title = taskList[0];
                    activeTask2.Title = taskList[1];
                    activeTask3.Title = taskList[2];
                    activeTask4.Title = taskList[3];
                    activeTask5.Title = taskList[4];
                    lblActiveTask1.Text = activeTask1.Title;
                    lblActiveTask2.Text = activeTask2.Title;
                    lblActiveTask3.Text = activeTask3.Title;
                    lblActiveTask4.Text = activeTask4.Title;
                    lblActiveTask5.Text = activeTask5.Title;
                    break;
            }
        }
    }
}
