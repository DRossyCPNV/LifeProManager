/// <file>Program.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon - SI-CA2a</author>
/// <version>1.3</version>
/// <date>February 13th, 2022</date>

using System;
using System.Windows.Forms;

namespace LifeProManager
{
    static class Program
    {  
        /// <summary>
        /// Main entry point of the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
