/// <file>Lists.cs</file>
/// <author>David Rossy, Laurent Barraud and Julien Terrapon - SI-CA2a</author>
/// <version>1.2</version>
/// <date>November 11th, 2021</date>


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
