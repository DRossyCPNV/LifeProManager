/// <file>TaskSelections.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.2</version>
/// <date>February 26th, 2026</date>


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
