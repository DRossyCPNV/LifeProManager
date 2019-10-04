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
        List<Label> activeLabelsList = new List<Label>();
        List<Label> topicsLabelsList = new List<Label>();
        List<Label> finishedLabelsList = new List<Label>();
        List<Label> finishedLabelsSelectedList = new List<Label>();


        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            calMonth.ShowToday = false;
            calMonth.MaxSelectionCount = 1;
            cmdToday_Click(sender, e);

            activeLabelsList.Add(lblActiveTask1);
            activeLabelsList.Add(lblActiveTask2);
            activeLabelsList.Add(lblActiveTask3);
            activeLabelsList.Add(lblActiveTask4);
            activeLabelsList.Add(lblActiveTask5);
            activeLabelsList.Add(lblWeekTask1);
            activeLabelsList.Add(lblWeekTask2);
            activeLabelsList.Add(lblWeekTask3);
            activeLabelsList.Add(lblWeekTask4);
            activeLabelsList.Add(lblWeekTask5);

            topicsLabelsList.Add(lblTopicTask1);
            topicsLabelsList.Add(lblTopicTask2);
            topicsLabelsList.Add(lblTopicTask3);
            topicsLabelsList.Add(lblTopicTask4);
            topicsLabelsList.Add(lblTopicTask5);
            topicsLabelsList.Add(lblTopicTask6);
            topicsLabelsList.Add(lblTopicTask7);
            topicsLabelsList.Add(lblTopicTask8);
            topicsLabelsList.Add(lblTopicTask9);
            topicsLabelsList.Add(lblTopicTask10);

            finishedLabelsList.Add(lblFinishedTask1);
            finishedLabelsList.Add(lblDateTimeFinishedTask1);
            finishedLabelsList.Add(lblFinishedTask2);
            finishedLabelsList.Add(lblDateTimeFinishedTask2);
            finishedLabelsList.Add(lblFinishedTask3);
            finishedLabelsList.Add(lblDateTimeFinishedTask3);
            finishedLabelsList.Add(lblFinishedTask4);
            finishedLabelsList.Add(lblDateTimeFinishedTask4);
            finishedLabelsList.Add(lblFinishedTask5);
            finishedLabelsList.Add(lblDateTimeFinishedTask5);
            finishedLabelsList.Add(lblFinishedTask6);
            finishedLabelsList.Add(lblDateTimeFinishedTask6);
            finishedLabelsList.Add(lblFinishedTask7);
            finishedLabelsList.Add(lblDateTimeFinishedTask7);
            finishedLabelsList.Add(lblFinishedTask8);
            finishedLabelsList.Add(lblDateTimeFinishedTask8);
            finishedLabelsList.Add(lblFinishedTask9);
            finishedLabelsList.Add(lblDateTimeFinishedTask9);
            finishedLabelsList.Add(lblFinishedTask10);
            finishedLabelsList.Add(lblDateTimeFinishedTask10);
        }

      

        private void ActiveLabelToSelect_Click(object sender, EventArgs e)
        {
            Label labelToSelect = sender as Label;

            if (labelToSelect.BackColor == Color.Transparent)
            {

                foreach (Label item in activeLabelsList)
                {
                    item.BackColor = Color.Transparent;
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

                foreach (Label item in topicsLabelsList)
                {
                    item.BackColor = Color.Transparent;
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
                finishedLabelsSelectedList.Add(labelToSelect);
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

        private void CalMonth_DateChanged(object sender, DateRangeEventArgs e)
        {
            string daySelected = calMonth.SelectionRange.Start.ToString("dd-MMM-yyyy");
            grpToday.Text = daySelected;

            DBConnection dbConn = new DBConnection();
            dbConn.CreateTable();
            dbConn.InsertData();

            List<string> taskList = new List<string>();
            // Copie le contenu de la liste de string retourné par la méthode dbConn.ReadData() dans taskList
            taskList = dbConn.ReadData();

            // Copie les 5 premiers éléments de la liste dans les labels de l'onglet "Actives", pour autant que la liste ne soit pas vide.
            if (taskList.Count() != 0)
            {
                
            }
            else
            {
                lblActiveTask1.Text = "";
                lblActiveTask2.Text = "";
                lblActiveTask3.Text = "";
                lblActiveTask4.Text = "";
                lblActiveTask5.Text = "";
            }

        }

    }
}
