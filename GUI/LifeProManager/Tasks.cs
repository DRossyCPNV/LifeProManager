using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeProManager
{
    public class Tasks
    {
       private int id;
       private string title;
       private string description;
       private string deadline;
       
        /// <summary>
        /// id of the task
        /// </summary>
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// title of the task
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


        /// <summary>
        /// description of the task
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// day for which the task is to be completed
        /// </summary>
        public string Deadline
        {
            get
            {
                return deadline;
            }
            set
            {
                deadline = value;
            }
        }
    }
}
