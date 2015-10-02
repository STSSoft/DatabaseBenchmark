using DatabaseBenchmark.Core;
using DatabaseBenchmark.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Utils
{
    public static class DirectoryUtils
    {
        private static ILog Logger;

        static DirectoryUtils()
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
        }

        public static void CreateAndSetDatabasesDataDirectory(string mainDirectory, IEnumerable<Database> databases)
        {
            // Create directories.
            if (!Directory.Exists(mainDirectory))
                Directory.CreateDirectory(mainDirectory);

            foreach (var database in databases)
               CreateAndSetDatabaseDirectory(mainDirectory, database);
        }

        public static void CreateAndSetDatabaseDirectory(string mainDirectory, Database database)
        {
            try
            {
                string dataDir = Path.Combine(mainDirectory, database.Name);

                if (Directory.Exists(dataDir))
                    ClearDatabaseDataDirectory(database);
                
                Directory.CreateDirectory(dataDir);
                database.DataDirectory = dataDir;
            }
            catch (Exception exc)
            {
                Logger.Error("Create database directory error...", exc);
            }
        }

        public static void ClearDatabaseDataDirectory(Database database)
        {
            try
            {
                if (database.DataDirectory != null)
                {
                    foreach (var directory in Directory.GetDirectories(database.DataDirectory))
                        Directory.Delete(directory, true);

                    foreach (var files in Directory.GetFiles(database.DataDirectory, "*.*", SearchOption.AllDirectories))
                        File.Delete(files);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Clear database directory error...", exc);
            }
        }
    }
}
