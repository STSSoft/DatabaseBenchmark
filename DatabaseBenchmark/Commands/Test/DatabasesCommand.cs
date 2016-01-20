using DatabaseBenchmark.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseBenchmark.Core;

namespace DatabaseBenchmark.Commands
{
    public class BenchmarkCommand : Command
    {
        public MainForm Form { get; private set; }

        public BenchmarkCommand()
        {
        }

        public BenchmarkCommand(MainForm form)
        {
            Form = form;
        }

        public override void Execute()
        {
            Form.History.Clear();

            Databases = Form.TreeFrame.GetSelectedDatabases();
            Tests = Form.TestSelectionFrame.CheckedTests.ToArray();

            foreach (var database in Databases)
            {
                //// TODO: Fix this.
                //var session = new BenchmarkSession(database, TableCount, RecordCount, Randomness, Cancellation);
                //History.Add(session);

                foreach (var test in Tests)
                {
                    test.Database = database;
                }

                var benchmark = new Benchmark(database);
                Form.History.Add(benchmark);

                //Test = new FullWriteReadTest(database, TableCount, RecordCount, Randomness, Cancellation);
                DirectoryUtils.ClearDatabaseDataDirectory(database);
                DirectoryUtils.CreateAndSetDatabaseDirectory(Application.StartupPath, database);
            }
        }
    }
}
