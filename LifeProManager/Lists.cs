/// <file>Lists.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon</author>
/// <version>1.7.3</version>
/// <date>February 28th, 2026</date>

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
