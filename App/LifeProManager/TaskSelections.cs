﻿/// <file>TaskSelections.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.0</version>
/// <date>November 7th, 2019</date>


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

        public int Task_id { get => task_id; set => task_id = value; }
        public Label Task_label { get => task_label; set => task_label = value; }
        public string Task_information { get => task_information; set => task_information = value; }
    }
}