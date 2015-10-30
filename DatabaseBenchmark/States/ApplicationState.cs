using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.States
{
    public abstract class ApplicationState
    {
        public MainForm Form;

        public ApplicationState(MainForm form)
        {
            Form = form;
        }

        public abstract void Handle();
    }

    public enum State
    {
        None,
        TestRunning,
        TestStopped
    }
}
