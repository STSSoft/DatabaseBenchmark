using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Security.Permissions;
using System.Security.Principal;
using System.Windows.Forms;
using System.Linq;

namespace DatabaseBenchmark
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string extension = ".dbproj";

            try
            {
                if (FileAssociations.Exists(extension) && IsAdministrator())
                    FileAssociations.Delete(extension);

                if (!FileAssociations.Exists(extension) && IsAdministrator())
                    FileAssociations.Create(extension, "DatabaseBenchmark");
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
