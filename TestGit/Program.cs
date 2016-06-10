
namespace TestGit
{


    static class Program
    {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [System.STAThread]
        static void Main()
        {
            // DynamicSqlFormatter.Test();
            // Test.ListAllBranches();
            Test.GetCommitsByBranch("master");
            // Test.WalkCommits();

            if (false)
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new Form1());
            }

        } // End Sub Main



    } // End Class Program


} // End Namespace TestGit 