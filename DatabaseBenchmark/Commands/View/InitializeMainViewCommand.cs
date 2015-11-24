using DatabaseBenchmark.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands.View
{
    public class InitializeMainViewCommand : ViewCommand
    {
        public InitializeMainViewCommand(MainForm form)
            : base(form)
        {
        }

        public override void Execute()
        {
        }
    }
}
