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
            //View.MainLayout.InitializeCharts(View.MainLayout.GetSelectedDatabasesChartValues());

            //if (View.MainLayout.TreeView.GetSelectedDatabases().Length == 0 && View.MainLayout.TreeView.GetSelectedDatabase() == null)
            //    return;

            //View.MainLayout.ClearLogFrame();
        }
    }
}
