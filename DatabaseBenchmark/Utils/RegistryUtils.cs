using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseBenchmark.Utils
{
    public static class RegistryUtils
    {
        public static void Create(string extension, params string[] openWith)
        {
            string progID = "DatabaseBenchmark";
            string description = "Database Benchmark is a powerfull open source tool designed to stress test databases with large data flows.";
            string iconPath = Path.Combine(Application.StartupPath, "Resources", "Benchmark48x48.ico");
            string applicationPath = Path.Combine(Application.StartupPath, "DatabaseBenchmark.exe");

            RegistryKey progKey = Registry.ClassesRoot.CreateSubKey(extension);
            progKey.SetValue("", progID);
            progKey.CreateSubKey("DefaultIcon").SetValue("", iconPath, RegistryValueKind.String);
            RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID, RegistryKeyPermissionCheck.ReadWriteSubTree);

            key.SetValue("", description, RegistryValueKind.String);
            key.CreateSubKey("DefaultIcon").SetValue("", iconPath, RegistryValueKind.String);
            key.CreateSubKey(@"Shell\Open\Command").SetValue("", applicationPath + " \"%1\"", RegistryValueKind.String);

            if (openWith != null)
            {
                key = key.CreateSubKey("OpenWithList", RegistryKeyPermissionCheck.ReadWriteSubTree);

                foreach (string file in openWith)
                    key.CreateSubKey(file);
            }

            key.Flush();
            key.Close();
        }

        public static void Delete(string extension)
        {
            if (!Exists(extension))
                return;

            if (Registry.ClassesRoot.OpenSubKey(extension, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl) == null)
                return;

            Registry.ClassesRoot.DeleteSubKeyTree(GetProgID(extension));
            Registry.ClassesRoot.DeleteSubKeyTree(extension);
        }

        /// <summary>
        /// Gets a value indicating whether the association keys exist.
        /// </summary>
        public static bool Exists(string extension)
        {
            if (Registry.ClassesRoot.OpenSubKey(extension) != null)
            {
                string id = GetProgID(extension);

                if (id != null)
                {
                    if (Registry.ClassesRoot.OpenSubKey(id) != null)
                        return true;
                }
            }

            return false;
        }

        public static string GetProgID(string extension)
        {
            string toReturn = string.Empty;

            if (Registry.ClassesRoot.OpenSubKey(extension, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl) != null)
            {
                if (Registry.ClassesRoot.OpenSubKey(extension, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl).GetValue("") != null)
                    toReturn = Registry.ClassesRoot.OpenSubKey(extension, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl).GetValue("").ToString();
            }

            return toReturn;
        }
    }
}
