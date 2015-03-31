using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Security.Permissions;
using System.Security.Principal;
using System.Windows.Forms;

namespace DatabaseBenchmark
{
    static class Program
    {
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!FileAssociations.Exists(".dbproj") && IsAdministrator())
                FileAssociations.Create(".dbproj", "DatabaseBenchmark", "Database Benchmark is a powerfull open source tool designed to stress test databases with large data flows.",
                   Path.Combine(Application.StartupPath, "Resources\\logo_01.png"), Path.Combine(Application.StartupPath, "DatabaseBenchmark.exe"), "DatabaseBenchmark");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
