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
            Form.MainLayout.InitializeCharts(Form.MainLayout.GetSelectedDatabasesChartValues());

            if (Form.MainLayout.TreeView.GetSelectedDatabases().Length == 0 && Form.MainLayout.TreeView.GetSelectedDatabase() == null)
                return;

            Form.MainLayout.ClearLogFrame();
        }
    }
}
