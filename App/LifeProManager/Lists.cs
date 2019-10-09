using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeProManager
{
    class Lists
    {
        private string title;

        /// <summary>
        /// topic of the task (lists)
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
               title = value;
            }
        }
    }
}
