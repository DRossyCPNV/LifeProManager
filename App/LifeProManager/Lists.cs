/// <file>Lists.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.6.1</version>
/// <date>January 17th, 2025</date>

namespace LifeProManager
{
    /// <class>This class handles the lists of topics</class>
    public class Lists
    {
        private int id;
        private string title;

        public int Id 
        { 
            get => id; 
            set => id = value; 
        }
        public string Title 
        { 
            get => title; 
            set => title = value; 
        }
    }
}
