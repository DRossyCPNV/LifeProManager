/// <file>Lists.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon for alpha-tests.</author>
/// <version>1.6.2</version>
/// <date>February 16th, 2026</date>

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
