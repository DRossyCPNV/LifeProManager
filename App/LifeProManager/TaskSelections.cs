﻿/// <file>TaskSelections.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.2.1</version>
/// <date>December 30th, 2021</date>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeProManager
{
    /// <class>TaskSelections handles the selected tasks</class>
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