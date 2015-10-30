using DatabaseBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseBenchmark.Commands
{
    public abstract class TestCommand
    {
        public MainForm Form;
        public Database[] TestedDatabases { get; set; }

        public TestCommand(MainForm form, Database[] databases)
        {
            Form = form;
            TestedDatabases = databases;
        }

        public abstract void Start();
        public abstract void Stop();
    }
}
