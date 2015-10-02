using DatabaseBenchmark.Utils;
using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace DatabaseBenchmark
{
    static class Program
    {
        const string PROJECT_EXTENSION = ".dbproj";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (RegistryUtils.Exists(PROJECT_EXTENSION) && IsAdministrator())
                    RegistryUtils.Delete(PROJECT_EXTENSION);

                if (!RegistryUtils.Exists(PROJECT_EXTENSION) && IsAdministrator())
                    RegistryUtils.Create(PROJECT_EXTENSION, "DatabaseBenchmark");
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
