using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeProManager
{
    class Priorities
    {
        private string denomination;
        private int priorityLevel;

        /// <summary>
        /// denomination of the priority of the task
        /// </summary>
        public string Denomination
        {
            get
            {
                return denomination;
            }
            set
            {
                denomination = value;
            }
        }


        /// <summary>
        /// priority of the task
        /// </summary>
        public int PriorityLevel
        {
            get
            {
                return priorityLevel;
            }
            set
            {
                priorityLevel = value;
            }
        }
    }

   
}
