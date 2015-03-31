using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark
{
    public static class FileAssociations
    {
        public static void Create(string extension, string progID, string description, string iconPath, string applicationPath, params string[] openWith)
        {
            if (progID == null)
                throw new ArgumentException("progID = null");

            if (description == null)
                throw new ArgumentException("description = null");

            if (iconPath == null)
                throw new ArgumentException("iconPath = null");

            if (applicationPath == null)
                throw new ArgumentException("applicationPath = null");

            Registry.ClassesRoot.CreateSubKey(extension).SetValue("", progID);
            RegistryKey key = Registry.ClassesRoot.CreateSubKey(progID, RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue("", description, RegistryValueKind.String);
            key.CreateSubKey("DefaultIcon").SetValue("", iconPath, RegistryValueKind.String);
            key.CreateSubKey(@"Shell\Open\Command").SetValue("", applicationPath + " %1", RegistryValueKind.String);

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
