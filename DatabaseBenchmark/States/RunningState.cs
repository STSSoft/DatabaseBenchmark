using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.States
{
    public class RunningState : ApplicationState
    {
        public RunningState(MainForm form)
            : base(form)
        {
        }

        public override void Handle()
        {
            var task = Form.MainTask;

            var startButton = Form.btnStart;
            var stopButton = Form.btnStop;
            var showTreeButton = Form.btnTreeView;

            if (Form.MainTask != null)
                stopButton.Enabled = task.Status == TaskStatus.Running ? true : false;

            //btnTreeView.Checked = MainLayout.TreeView.Visible;
            showTreeButton.Checked = true;

            startButton.Enabled = !stopButton.Enabled;

            //MainLayout.TreeView.TreeViewEnabled = btnStart.Enabled;
            //MainLayout.EnablePropertiesFrame(btnStart.Enabled);

            var state = startButton.Enabled;

            //Form.cloneToolStripMenuItem.Enabled = state;
            //Form.renameToolStripMenuItem.Enabled = state;
            //Form.deleteToolStripMenuItem.Enabled = state;
            //Form.restoreDefaultAllToolStripMenuItem.Enabled = state;
            //Form.expandAllToolStripMenuItem.Enabled = state;
            //Form.collapseAllToolStripMenuItem.Enabled = state;
       
            //Form.btnExports.Enabled = true;

            //Form.exportResultToPDFToolStripMenuItem.Enabled = true;
            //Form.exportToCSVToolStripMenuItem.Enabled = true;
            //Form.exportToJSONToolStripMenuItem.Enabled = true;
            //Form.onlineReportResultsToolStripMenuItem.Enabled = true;

            //Form.legendPossitionToolStripMenuItem.Enabled = showLegendToolStripMenuItem.Checked;

            //var activeFrame = MainLayout.GetCurrentFrame();
            //var session = Current;

            //if (session == null)
            //{
            //    progressBar.Value = 0;
            //    progressStatus.Text = "(None)";
            //    percentStatus.Text = "%";
            //    elapsedTimeStatus.Text = "Elapsed: ";
            //    estimateTimeStatus.Text = "Estimate: ";
            //    return;
            //}
        }
    }
}
