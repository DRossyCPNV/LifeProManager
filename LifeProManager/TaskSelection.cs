/// <file>TaskSelections.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.1</version>
/// <date>February 24th, 2026</date>


using System.Windows.Forms;

namespace LifeProManager
{
    /// <class>This class handles the selected tasks</class>
    public class TaskSelection
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

        public int Task_priority { get; set; }
    }
}
