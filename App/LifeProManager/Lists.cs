﻿/// <file>Lists.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.2.1</version>
/// <date>December 30th, 2021</date>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeProManager
{
    /// <class>Lists handles the lists of topics</class>
    public class Lists
    {
        private int id;
        private string title;

        public int Id { get => id; set => id = value; }
        public string Title { get => title; set => title = value; }
    }
}