using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Utils;

namespace DatabaseBenchmark.Factory
{
    public static class TestFactory
    {
        public static void Initialize(MainForm form)
        {
            //Form.History.Clear();

            var databases = form.TreeFrame.GetSelectedDatabases();
            var tests = form.TestSelectionFrame.CheckedTests;

            foreach (var database in databases)
            {
                //// TODO: Fix this.
                //var session = new BenchmarkSession(database, TableCount, RecordCount, Randomness, Cancellation);
                //History.Add(session);

                //Test = new FullWriteReadTest(database, TableCount, RecordCount, Randomness, Cancellation);
                DirectoryUtils.ClearDatabaseDataDirectory(database);
                DirectoryUtils.CreateAndSetDatabaseDirectory(Application.StartupPath, database);
            }

            //Tests = tests.ToArray();
            //Databases = databases;
        }
    }
}
