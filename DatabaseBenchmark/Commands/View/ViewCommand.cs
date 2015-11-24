using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands
{
    public abstract class ViewCommand
    {
        public MainForm View;

        public ViewCommand(MainForm form)
        {
            View = form;
        }

        public abstract void Execute();
    }
}
