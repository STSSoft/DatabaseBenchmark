using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands.View
{
    public class InterfaceCommand : ViewCommand
    {
        public InterfaceCommand(MainForm form)
            : base(form)
        {
        }

        public override void Execute()
        {
            View.InitializeCharts(View.GetSelectedDatabasesChartValues());

            if (View.TreeFrame.GetSelectedDatabases().Length == 0 && View.TreeFrame.GetSelectedDatabase() == null)
                return;

            View.ClearLogFrame();
        }
    }
}
