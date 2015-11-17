using DatabaseBenchmark.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseBenchmark.Commands
{
    public class PrepareBenchmark : Command
    {
        public MainForm Form { get; private set; }

        public PrepareBenchmark()
        {
        }

        public PrepareBenchmark(MainForm form)
        {
            Form = form;
        }

        public override void Execute()
        {
            //Form.History.Clear();

            var databases = Form.MainLayout.TreeView.GetSelectedDatabases();
            var tests = Form.TestsWindows.CheckedTests;

            foreach (var database in databases)
            {
                //// TODO: Fix this.
                //var session = new BenchmarkSession(database, TableCount, RecordCount, Randomness, Cancellation);
                //History.Add(session);

                //Test = new FullWriteReadTest(database, TableCount, RecordCount, Randomness, Cancellation);
                DirectoryUtils.ClearDatabaseDataDirectory(database);
                DirectoryUtils.CreateAndSetDatabaseDirectory(Application.StartupPath, database);
            }

            Tests = tests.ToArray();
            Databases = databases;
        }
    }
}
