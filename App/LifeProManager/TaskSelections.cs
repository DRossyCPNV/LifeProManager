/// <file>TaskSelections.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6.1</version>
/// <date>January 17th, 2025</date>


using System.Windows.Forms;

namespace LifeProManager
{
    /// <class>This class handles the selected tasks</class>
    class TaskSelections
    {
        private int task_id;
        private Label task_label;
        private string task_information;

        public int Task_id 
        {
            get => task_id; 
            set => task_id = value; 
        }

        public Label Task_label 
        { 
            get => task_label;
            set => task_label = value;
        }

        public string Task_information 
        { 
            get => task_information; 
            set => task_information = value; 
        }
    }
}
