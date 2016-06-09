using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestGit
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            TestaFhu.getCommitsByTree("master");
            // TestaFhu.WalkCommits();

            if (false)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }

        }
    }
}
