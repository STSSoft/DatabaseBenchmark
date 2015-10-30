using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Report;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Serialization;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Diagnostics;
using DatabaseBenchmark.Properties;
using DatabaseBenchmark.Utils;
using DatabaseBenchmark.Core.Benchmarking;
using OxyPlot;
using DatabaseBenchmark.Reporting;
using DatabaseBenchmark.Core.Benchmarking.Tests;
using DatabaseBenchmark.Commands;
using DatabaseBenchmark.States;

/*
 * Copyright (c) 2010-2015 STS Soft SC
 * 
 * This file is part of Database Benchmark.
 *
 * Database Benchmark is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU General Public License as published by the Free Software Foundation, 
 * either version 2 of the License, or (at your option) any later version.
 *
 * Database Benchmark is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License along with Database Benchmark. 
 * If not, see http://www.gnu.org/licenses/.
 * 
*/

namespace DatabaseBenchmark
{
    public partial class MainForm : Form
    {
        public static readonly string DATABASES_DIRECTORY = Path.Combine(Application.StartupPath + "\\Databases");
        public static readonly string CONFIGURATION_FOLDER = Path.Combine(Application.StartupPath + "\\Config");

        public volatile Task MainTask = null;

        public CancellationTokenSource Cancellation;

        public State MainState;
        public RunningState RunState;
        public StoppedState StopState;

        public PrepareInterfaceCommand PrepareGuiCommand;
        public DatabasesCommand PrepareTestsAndDatabasesCommand;
        public ExecuteTestsCommand TestsCommand;

        public bool TestFailed;

        public BenchmarkSession Current;
        public Benchmark CurrentTest;
        public FullWriteReadTest Test;
        public List<BenchmarkSession> History;

        public int TableCount;
        public long RecordCount;

        public float Randomness = 0.0f;
        public string CurrentStatus;

        public ILog Logger;

        public ProjectManager Manager;
        public MainLayout MainLayout;

        public List<ToolStripButton> ViewButtons;

        public MainForm()
        {
            InitializeComponent();

            History = new List<BenchmarkSession>();
            ViewButtons = new List<ToolStripButton>();

            PrepareGuiCommand = new PrepareInterfaceCommand(this);
            PrepareTestsAndDatabasesCommand = new DatabasesCommand();

            ViewButtons = toolStripMain.Items.OfType<ToolStripButton>().Where(x => x.CheckOnClick).ToList();

            // Loggers and tracers.
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
            Logger.Info(Environment.NewLine);
            Logger.Info("Application started...");

            Trace.Listeners.Add(new Log4NetTraceListener(Settings.Default.TestLogger));
            Trace.Listeners.Add(new Log4NetTraceListener(Settings.Default.TestLogger));

            this.SuspendLayout();

            toolStripMain.Items.Insert(toolStripMain.Items.Count - 2, new ToolStripControlHost(trackBar1));

            // Layout.
            MainLayout = new MainLayout(dockPanel1, new ToolStripComboBox[] { cbFlowsCount, cbRecordCount }.ToList(), ViewButtons, trackBar1, CONFIGURATION_FOLDER);

            MainLayout.Initialize();
            MainLayout.TreeView.CreateTreeView();
            MainLayout.LoadDocking();

            MainLayout.SelectFrame(TestMethod.Write);

            Manager = new ProjectManager(MainLayout);

            View_Click(btnSizeView, EventArgs.Empty);

            openFileDialogProject.InitialDirectory = CONFIGURATION_FOLDER;
            saveFileDialogProject.InitialDirectory = CONFIGURATION_FOLDER;

            WireDragDrop(Controls);

            this.ResumeLayout();
        }

        #region Execute Benchmark

        private void DoBenchmark()
        {
            //CurrentTest = new BenchmarkSuite();
            //CurrentTest.ExecuteTests(TableCount, RecordCount, Randomness, Cancellation, Test);

            // TODO: Fix this.
            //testSuite.OnTestMethodCompleted += Report;
            //testSuite.OnException += OnException;

            try
            {
                foreach (var benchmark in History)
                {
                    if (Cancellation.IsCancellationRequested)
                        break;

                    //Current = benchmark;
                    //testSuite.ExecuteInit(benchmark);

                    //// Write.
                    //MainLayout.SetCurrentMethod(TestMethod.Write);
                    //CurrentStatus = TestMethod.Write.ToString();

                    //testSuite.ExecuteWrite(benchmark);

                    //// Read.
                    //MainLayout.SetCurrentMethod(TestMethod.Read);
                    //CurrentStatus = TestMethod.Read.ToString();

                    //testSuite.ExecuteRead(benchmark);

                    //// Secondary Read.
                    //MainLayout.SetCurrentMethod(TestMethod.SecondaryRead);
                    //CurrentStatus = TestMethod.SecondaryRead.ToString();

                    //testSuite.ExecuteSecondaryRead(benchmark);

                    //// Finish.
                    //CurrentStatus = TestMethod.None.ToString();
                    //testSuite.ExecuteFinish(benchmark);
                }
            }
            finally
            {
                Current = null;

                if (Cancellation.IsCancellationRequested)
                    History.Clear();
                else
                {
                    if (!TestFailed)
                    {
                        if (!Settings.Default.HideReportForm)
                            OnlineReport();
                    }
                }
            }
        }

        private void OnException(Exception exception, BenchmarkSession test)
        {
            TestFailed = true;
        }

        #endregion

        #region Report methods

        private void Report(BenchmarkSession benchmark, TestMethod method)
        {
            try
            {
                // TODO: Fix this.
                //Action<string, object, Color> updateChart = null;

                //StepFrame ActiveStepFrame = MainLayout.GetCurrentFrame();
                //string databaseName = benchmark.Database.Name;
                //Color databaseColor = benchmark.Database.Color;

                //// Speed chart.
                //updateChart = ActiveStepFrame.AddAverageSpeedToBar;
                //Report(databaseName, databaseColor, updateChart, benchmark.GetAverageSpeed(method));

                //// Size chart.
                //updateChart = ActiveStepFrame.AddSizeToBar;
                //Report(databaseName, databaseColor, updateChart, benchmark.DatabaseSize / (1024.0 * 1024.0));

                //// Time chart.
                //updateChart = ActiveStepFrame.AddTimeToBar;
                //Report(databaseName, databaseColor, updateChart, new DateTime(benchmark.GetElapsedTime(method).Ticks));

                //// Memory chart.
                //updateChart = ActiveStepFrame.AddMemoryUsageToBar;
                //Report(databaseName, databaseColor, updateChart, benchmark.GetPeakWorkingSet(method) / (1024.0 * 1024.0));
            }
            catch (Exception exc)
            {
                Logger.Error("Report results failed...", exc);
            }
        }

        private void Report(string series, Color seriesColor, Action<string, object, Color> addPoint, object data)
        {
            try
            {
                Invoke(addPoint, series, data, seriesColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report exception occured...", exc);
            }
        }

        private void onlineReportResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnlineReport();
        }

        #endregion

        #region Export

        // CSV.
        private void summaryReportToolStripMenuItemCsv_Click(object sender, EventArgs e)
        {
            Export(ReportFormat.CSV, ReportType.Summary, saveFileDialogCsv);
        }

        private void detailedReportToolStripMenuItemCsv_Click(object sender, EventArgs e)
        {
            Export(ReportFormat.CSV, ReportType.Detailed, saveFileDialogCsv);
        }

        // JSON.
        private void summaryReportToolStripMenuItemJson_Click(object sender, EventArgs e)
        {
            Export(ReportFormat.JSON, ReportType.Summary, saveFileDialogJson);
        }

        private void detailedReportToolStripMenuItemJson_Click(object sender, EventArgs e)
        {
            Export(ReportFormat.JSON, ReportType.Detailed, saveFileDialogJson);
        }

        // PDF.
        private void summaryReportToolStripMenuItemPdf_Click(object sender, EventArgs e)
        {
            Export(ReportFormat.PDF, ReportType.Summary, saveFileDialogPdf);
        }

        private void detailedReportToolStripMenuItemPdf_Click(object sender, EventArgs e)
        {
            Export(ReportFormat.PDF, ReportType.Detailed, saveFileDialogPdf);
        }

        private void Export(ReportFormat reportFormat, ReportType reportType, SaveFileDialog dialog)
        {
            string postfix = reportType == ReportType.Detailed ? "detailed" : "summary";
            dialog.FileName = String.Format("DatabaseBenchmark-{0:yyyy-MM-dd HH.mm}-{1}", DateTime.Now, postfix);

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // Start loading and disable MainForm.
                    LoadingFrame.Start(string.Format("Exporting to {0}...", reportFormat), Bounds);
                    Enabled = false;

                    switch (reportFormat)
                    {
                        case ReportFormat.CSV:
                            CsvUtils.ExportResults(History, saveFileDialogCsv.FileName, reportType);
                            break;

                        case ReportFormat.JSON:
                            ComputerConfiguration configuration = SystemUtils.GetComputerConfiguration();
                            JsonUtils.ExportToJson(saveFileDialogJson.FileName, configuration, History, reportType);
                            break;

                        case ReportFormat.PDF:
                            // TODO: Fix this.
                            //BenchmarkSession test = History[0];
                            //PdfUtils.Export(saveFileDialogPdf.FileName, MainLayout.StepFrames, test.FlowCount, test.RecordCount, test.Randomness, SystemUtils.GetComputerConfiguration(), reportType);
                            break;
                    }

                    // Stop loading end enable MainForm
                    LoadingFrame.Stop();
                    Enabled = true;
                }
                catch (Exception exc)
                {
                    string message = string.Format("Export results to {0} failed...", reportFormat);

                    Logger.Error(message, exc);
                    ReportError(message);
                    Enabled = true;
                    LoadingFrame.Stop();
                }
            }
        }

        #endregion

        #region Buttons & Click Events

        private void startButton_Click(object sender, EventArgs e)
        {
            PrepareGuiCommand.Execute();
            PrepareTestsAndDatabasesCommand.Execute();

            TestsCommand = new ExecuteTestsCommand(this, PrepareTestsAndDatabasesCommand.Databases);
            TestsCommand.Start();  
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            TestsCommand.Stop();
        }

        private void StartTest(Database[] databases, List<KeyValuePair<string, Color>> chartValues)
        {
        }

        private void Tuning_TuningButtonClicked(List<Database> obj)
        {
            throw new NotImplementedException();
        }

        private void View_Click(object sender, EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;
            int column = Int32.Parse(button.Tag.ToString());

            MainLayout.ShowBarChart(column, button.Checked);
            ((ToolStripMenuItem)showBarChartsToolStripMenuItem.DropDownItems[column]).Checked = button.Checked;
        }

        private void EditBartItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem button = (ToolStripMenuItem)sender;
            int column = Int32.Parse(button.Tag.ToString());

            MainLayout.ShowBarChart(column, button.Checked);
            ((ToolStripButton)toolStripMain.Items[9 + column]).Checked = button.Checked;
        }

        private void axisType_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripButton).Checked;

            foreach (var frame in MainLayout.StepFrames)
                frame.Value.SelectedChartIsLogarithmic = isChecked;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        #endregion

        #region TreeView

        private void buttonTreeView_Click(object sender, EventArgs e)
        {
            MainLayout.SelectTreeView(btnTreeView.Checked);
        }

        #endregion

        #region Docking

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadingFrame.Start("Saving project...", Bounds);
            Manager.Store(saveFileDialogProject.FileName);
            LoadingFrame.Stop();
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                LoadingFrame.Start("Loading project...", Bounds);

                Manager.Load(openFileDialogProject.FileName);
                saveFileDialogProject.FileName = openFileDialogProject.FileName;
                saveConfigurationToolStripMenuItem.Enabled = true;
                Text = String.Format("{0} - Database Benchmark", Path.GetFileName(saveFileDialogProject.FileName));

                LoadingFrame.Stop();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Save current project ?", "Save project", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
                return;

            if (result == DialogResult.Yes)
            {
                if (saveConfigurationToolStripMenuItem.Enabled)
                    saveConfigurationToolStripMenuItem_Click(sender, e);
                else
                    saveAsToolStripMenuItem_Click(sender, e);
            }

            LoadingFrame.Start("Creating project...", Bounds);

            MainLayout.Reset();
            saveConfigurationToolStripMenuItem.Enabled = false;
            Text = "NewProject.dbproj - Database Benchmark";

            LoadingFrame.Stop();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                LoadingFrame.Start("Saving project...", Bounds);
                Manager.Store(saveFileDialogProject.FileName);
                LoadingFrame.Stop();

                saveConfigurationToolStripMenuItem.Enabled = true;
                Text = String.Format("{0} - Database Benchmark", Path.GetFileName(saveFileDialogProject.FileName));
            }
        }

        #endregion

        #region Edit Toolstrip Menu

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool state = MainLayout.TreeView.IsSelectedNodeDatabase;

            editToolStripMenuItem.DropDownItems[0].Visible = state; // Clone.
            editToolStripMenuItem.DropDownItems[1].Visible = state; // Rename.
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.CloneNode();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.RenameNode();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.DeleteNode();
        }

        private void restoreDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.RestoreDefault();
        }

        private void restoreDefaultAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.ClearTreeViewNodes();
            MainLayout.TreeView.CreateTreeView();
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.ShowPropertiesFrame();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.CollapseAll();
        }

        #endregion

        #region View Toolstrip Menu

        private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            StepFrame selectedFrame = MainLayout.GetActiveFrame();
            bool state = selectedFrame != null;

            viewToolStripMenuItem.DropDownItems[6].Visible = state;  // Separator.
            viewToolStripMenuItem.DropDownItems[8].Visible = state;  // Show legend.
            viewToolStripMenuItem.DropDownItems[9].Visible = state;  // Legend position.
            viewToolStripMenuItem.DropDownItems[10].Visible = state;  // Logarithmic.

            if (!state)
                return;

            LegendPossition position = selectedFrame.SelectedChartPosition;

            foreach (ToolStripMenuItem menuItem in legendPossitionToolStripMenuItem.DropDownItems)
                menuItem.Checked = menuItem.Text == position.ToString();

            showLegendToolStripMenuItem.Checked = selectedFrame.SelectedChartLegendIsVisible;
            logarithmicToolStripMenuItem.Checked = selectedFrame.SelectedChartIsLogarithmic;
        }

        private void databasesWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.SelectTreeView(true);
            btnTreeView.Checked = true;
        }

        private void writeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.SelectFrame(TestMethod.Write);
        }

        private void readWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.SelectFrame(TestMethod.Read);
        }

        private void secondaryReadWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.SelectFrame(TestMethod.SecondaryRead);
        }

        private void logWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.ShowLogFrame();
        }

        private void MoveLegend(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            LegendPossition position = (LegendPossition)Enum.Parse(typeof(LegendPossition), item.Text);
            StepFrame selectedFrame = MainLayout.GetActiveFrame();

            selectedFrame.SelectedChartPosition = position;

            foreach (ToolStripMenuItem menuItem in legendPossitionToolStripMenuItem.DropDownItems)
                menuItem.Checked = menuItem.Text == item.Text;
        }

        private void showLegendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripMenuItem).Checked;
            MainLayout.GetActiveFrame().SelectedChartLegendIsVisible = isChecked;
        }

        private void logarithmicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripMenuItem).Checked;
            MainLayout.GetActiveFrame().SelectedChartIsLogarithmic = isChecked;
        }

        private void resetWindowLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            MainLayout.RefreshDocking();

            this.ResumeLayout();
        }

        #endregion

        #region Drag Drop Events

        private void WireDragDrop(Control.ControlCollection ctls)
        {
            foreach (Control ctl in ctls)
            {
                ctl.AllowDrop = true;
                ctl.DragEnter += MainForm_DragEnter;
                ctl.DragDrop += MainForm_DragDrop;
                WireDragDrop(ctl.Controls);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;

                foreach (string item in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    if (!item.Contains(".dbproj"))
                    {
                        MessageBox.Show(item);
                        e.Effect = DragDropEffects.None;
                        break;
                    }
                }
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFromFile(files[0]);
        }

        private void LoadFromFile(string path)
        {
            LoadingFrame.Start("Loading project...", Bounds);

            Manager.Load(path);
            Text = String.Format("{0} - Database Benchmark", Path.GetFileName(path));
            saveConfigurationToolStripMenuItem.Enabled = true;

            LoadingFrame.Stop();
        }

        #endregion

        private void ReportError(string error)
        {
            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (MainState == State.TestRunning)
                {
                    RunState.Handle();
                }

                if (MainState == State.TestStopped)
                {
                    StopState.Handle();
                }

                // TODO: Fix this.
                //var method = session.CurrentMethod;
                //if (method == TestMethod.None)
                //    return;

                //if (autoNavigatetoolStripMenuItem.Checked)
                //    MainLayout.StepFrames[method].Activate();

                //TimeSpan elapsed = session.GetElapsedTime(method);

                //long currentRecords = session.GetRecords(method);
                //long totalRecords = TableCount * RecordCount;
                //double progress = (100.0 * currentRecords) / totalRecords;

                //var database = session.Database;

                //// Draw charts.
                //if (activeFrame.Text != null) // Frame is in write, read or other mode.
                //{
                //    int averagePossition = activeFrame.lineChartAverageSpeed.GetPointsCount(database.Name);
                //    int momentPossition = activeFrame.lineChartMomentSpeed.GetPointsCount(database.Name);

                //    var averageSpeedData = session.GetAverageSpeeds(method, averagePossition);
                //    var momentSpeedData = session.GetMomentSpeeds(method, momentPossition);
                //    var memoryData = session.GetMomentWorkingSets(method, averagePossition);

                //    activeFrame.AddAverageSpeed(database.Name, averageSpeedData);
                //    activeFrame.AddMomentSpeed(database.Name, momentSpeedData);
                //    activeFrame.AddPeakMemoryUsage(database.Name, memoryData);
                //}

                //if (Math.Abs(progress - 0.0) <= double.Epsilon)
                //    estimateTimeStatus.Text = "Estimate: infinity";
                //else
                //{
                //    TimeSpan timeSpan = new TimeSpan((long)((elapsed.Ticks * (100.0 - progress)) / progress));
                //    estimateTimeStatus.Text = String.Format("Estimate: {0:dd\\.hh\\:mm\\:ss}", timeSpan);
                //}

                //elapsedTimeStatus.Text = String.Format("Elapsed: {0:dd\\.hh\\:mm\\:ss} ", elapsed);
                //progressBar.Value = (int)progress;
                //percentStatus.Text = string.Format("{0:f2}%", progress);
                //progressStatus.Text = database.Name + " " + CurrentStatus;
            }
            catch (Exception exc)
            {
                Logger.Error("Application exception occured...", exc);
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            toolStripLabel2.Text = (trackBar1.Value * 5).ToString() + " %";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopButton_Click(sender, e);
            MainLayout.StoreDocking();

            Application.Exit();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:

                    if (saveConfigurationToolStripMenuItem.Enabled)
                        saveConfigurationToolStripMenuItem_Click(this, EventArgs.Empty);
                    else
                        saveAsToolStripMenuItem_Click(this, EventArgs.Empty);

                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                if (arg.EndsWith(".dbproj"))
                {
                    LoadFromFile(arg);
                    break;
                }
            }
        }

        private void OnlineReport()
        {
            try
            {
                LoadingFrame.Start("Obtaining computer configuration...", Bounds);
                ReportForm form = new ReportForm(History);
                LoadingFrame.Stop();

                form.ShowDialog();
            }
            catch (Exception exc)
            {
                LoadingFrame.Stop();
                Logger.Error("Online report exception occured ...", exc);
            }
        }

        private void categoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.treeViewOrder = TreeViewOrder.Category;
            MainLayout.TreeView.SetTreeViewOrder();
        }

        private void indexTechnologyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainLayout.TreeView.treeViewOrder = TreeViewOrder.IndexTechnology;
            MainLayout.TreeView.SetTreeViewOrder();
        }
    }
}