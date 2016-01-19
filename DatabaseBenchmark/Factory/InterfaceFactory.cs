using DatabaseBenchmark.Commands;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Properties;
using DatabaseBenchmark.Serialization;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseBenchmark.Commands.View;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Factory
{
    public static class InterfaceFactory
    {
        /// <summary>
        /// Initialize all GUI objects and states as necessary.
        /// </summary>
        public static void Initialize(MainForm form)
        {
            form.GuiCommand = new InterfaceCommand(form);
            form.BenchmarkCommand = new BenchmarkCommand(form);

            form.History = new List<Benchmark>();

            // Loggers and tracers.
            form.Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
            form.Logger.Info(Environment.NewLine);
            form.Logger.Info("Application started...");

            Trace.Listeners.Add(new Log4NetTraceListener(Settings.Default.TestLogger));
            Trace.Listeners.Add(new Log4NetTraceListener(Settings.Default.TestLogger));

            form.Manager = new ProjectManager();

            // Database tree.
            form.TreeFrame = new TreeViewFrame();
            form.TreeFrame.CreateTreeView();
            form.TreeFrame.DatabaseClick += form.ShowDatabaseProperties;
            form.TreeFrame.DefaultRestored += form.PropertiesDefaultRestored;

            form.TreeFrame.Show(form.dockPanel1);
            form.TreeFrame.DockState = DockState.DockLeft;

            // Logging frame.
            form.LogFrame = new LogFrame();
            form.LogFrame.Show(form.dockPanel1);
            form.LogFrame.DockState = DockState.DockBottomAutoHide;

            // Properties frame.
            form.PropertiesFrame = new PropertiesFrame();
            form.PropertiesFrame.DatabaseNameChanged += form.DatabaseNameChanged;
            form.PropertiesFrame.Caller = form.TreeFrame;

            form.PropertiesFrame.Show(form.dockPanel1);
            form.PropertiesFrame.DockState = DockState.DockRight;

            // Tests frame.
            form.TestSelectionFrame = new TestsFrame();
            form.TestSelectionFrame.TestClick += form.ShowTestProperties;
            form.TestSelectionFrame.Initialize();

            form.TestSelectionFrame.Show(form.dockPanel1);
            form.TestSelectionFrame.DockState = DockState.DockLeft;

            // Step frames.
            form.StepFrames = new List<StepFrame>();

            // View buttons.
            form.ViewButtons = new List<ToolStripButton>();
            form.ViewButtons = form.toolStripMain.Items.OfType<ToolStripButton>().Where(x => x.CheckOnClick).ToList();
            form.View_Click(form.btnSizeView, EventArgs.Empty);

            // Drag 'n' Drop.
            form.WireDragDrop(form.Controls);
        }
    }
}
