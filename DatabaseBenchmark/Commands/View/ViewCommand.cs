using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands
{
    public abstract class ViewCommand
    {
        public MainForm Form;

        public ViewCommand(MainForm form)
        {
            Form = form;
        }

        public abstract void Execute();
    }
}
