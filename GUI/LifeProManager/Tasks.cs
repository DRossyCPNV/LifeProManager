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
       private int properties_id;
       private int lists_id;
       private int status_id;

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

        /// <summary>
        /// priority of the task
        /// </summary>
        public int Properties_id
        {
            get
            {
                return properties_id;
            }
            set
            {
                properties_id = value;
            }
        }

        /// <summary>
        /// topic of the task (lists)
        /// </summary>
        public int Lists_id
        {
            get
            {
                return lists_id;
            }
            set
            {
                lists_id = value;
            }
        }

        /// <summary>
        /// status of the task
        /// </summary>
        public int Status_id
        {
            get
            {
                return status_id;
            }
            set
            {
                status_id = value;
            }
        }

    }
}
