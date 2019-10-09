using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeProManager
{
    class Status
    {
        private string denomination;


        /// <summary>
        /// status of the task
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
    }
}
