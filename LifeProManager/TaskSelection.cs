/// <file>TaskSelections.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.4</version>
/// <date>March 1st, 2026</date>


using System.Windows.Forms;

namespace LifeProManager
{
    /// <class>This class handles the selected tasks</class>
    public class TaskSelection
    {
        private int taskId;
        private Label taskLabel;
        private string taskInfo;

        public int TaskId 
        {
            get => taskId; 
            set => taskId = value; 
        }

        public Label TaskLabel 
        { 
            get => taskLabel;
            set => taskLabel = value;
        }

        public string TaskInformation 
        { 
            get => taskInfo; 
            set => taskInfo = value; 
        }

        public int Task_priority { get; set; }
    }
}
