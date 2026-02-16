/// <file>Program.cs</file>
/// <author>Laurent Barraud, David Rossy and Julien Terrapon for alpha-tests.</author>
/// <version>1.6.2</version>
/// <date>February 16th, 2026</date>

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
