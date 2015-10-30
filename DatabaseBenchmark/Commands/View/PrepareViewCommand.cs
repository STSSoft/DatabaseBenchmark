using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands
{
    public class PrepareInterfaceCommand : ViewCommand
    {
        public PrepareInterfaceCommand(MainForm form)
            : base(form)
        {
        }

        public override void Execute()
        {
            //MainLayout.InitializeCharts(MainLayout.GetSelectedDatabasesChartValues());

            //if (MainLayout.TreeView.GetSelectedDatabases().Length == 0 && MainLayout.TreeView.GetSelectedDatabase() == null)
            //    return;

            //MainLayout.ClearLogFrame();
        }
    }
}
