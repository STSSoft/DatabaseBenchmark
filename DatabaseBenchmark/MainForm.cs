using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Report;
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

        private volatile Task MainTask = null;
        private CancellationTokenSource Cancellation;

        private BenchmarkTest Current;
        private List<BenchmarkTest> History;

        private int TableCount;
        private long RecordCount;

        private float Randomness = 0.0f;
        private string CurrentStatus;

        private ILog Logger;
        private ProjectManager ApplicationManager;

        public MainForm()
        {
            InitializeComponent();

            History = new List<BenchmarkTest>();

            toolStripMain.Items.Insert(toolStripMain.Items.Count - 2, new ToolStripControlHost(trackBar1));
            this.SuspendLayout();

            // Logger.
            Logger = LogManager.GetLogger(Properties.Settings.Default.ApplicationLogger);

            ApplicationManager = new ProjectManager(dockPanel1, new ToolStripComboBox[] { cbFlowsCount, cbRecordCount }, trackBar1, CONFIGURATION_FOLDER);

            // Load dock and application configuration.
            ApplicationManager.Load(Path.Combine(CONFIGURATION_FOLDER, "Database Benchmark.dbproj"));
            ApplicationManager.LoadDocking();

            ApplicationManager.LayoutManager.SelectFrame(TestMethod.Write);

            View_Click(btnSizeView, EventArgs.Empty);

            openFileDialogProject.InitialDirectory = CONFIGURATION_FOLDER;
            saveFileDialogProject.InitialDirectory = CONFIGURATION_FOLDER;

            this.ResumeLayout();
        }

        #region Execute Benchmark

        private void DoBenchmark()
        {
            BenchmarkSuite testSuite = new BenchmarkSuite();
            testSuite.OnTestFinish += Report;

            try
            {
                foreach (var benchmark in History)
                {
                    Current = benchmark;
                    testSuite.ExecuteInit(benchmark);

                    // Write.
                    ApplicationManager.SetCurrentMethod(TestMethod.Write);
                    CurrentStatus = TestMethod.Write.ToString();

                    testSuite.ExecuteWrite(benchmark);

                    // Read.
                    ApplicationManager.SetCurrentMethod(TestMethod.Read);
                    CurrentStatus = TestMethod.Read.ToString();

                    testSuite.ExecuteRead(benchmark);

                    // Secondary Read.
                    ApplicationManager.SetCurrentMethod(TestMethod.SecondaryRead);
                    CurrentStatus = TestMethod.SecondaryRead.ToString();

                    testSuite.ExecuteSecondaryRead(benchmark);

                    // Finish.
                    CurrentStatus = TestMethod.None.ToString();
                    testSuite.ExecuteFinish(benchmark);
                }
            }
            finally
            {
                Current = null;
            }
        }

        #endregion

        #region Report methods

        private void Report(BenchmarkTest benchmark, TestMethod method)
        {
            try
            {
                Action<string, object, Color> updateChart = null;

                StepFrame ActiveStepFrame = ApplicationManager.GetActiveStepFrame();
                string databaseName = benchmark.Database.Name;
                Color databaseColor = benchmark.Database.Color;

                // Speed chart.
                updateChart = ActiveStepFrame.AddAverageSpeedToBar;
                Report(databaseName, databaseColor, updateChart, benchmark.GetSpeed(method));

                // Size chart.
                updateChart = ActiveStepFrame.AddSizeToBar;
                Report(databaseName, databaseColor, updateChart, benchmark.Database.Size / (1024.0 * 1024.0));

                // Time chart.
                updateChart = ActiveStepFrame.AddTimeToBar;
                Report(databaseName, databaseColor, updateChart, new DateTime(benchmark.GetTime(method).Ticks));

                // CPU chart.
                updateChart = ActiveStepFrame.AddCpuUsageToBar;
                Report(databaseName, databaseColor, updateChart, benchmark.GetAverageProcessorTime(method));

                // Memory chart.
                updateChart = ActiveStepFrame.AddMemoryUsageToBar;
                Report(databaseName, databaseColor, updateChart, benchmark.GetPeakWorkingSet(method) / (1024.0 * 1024.0));

                // I/O chart.
                updateChart = ActiveStepFrame.AddIoUsageToBar;
                Report(databaseName, databaseColor, updateChart, benchmark.GetAverageIOData(method) / (1024.0 * 1024.0));
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

        #endregion

        #region Export

        private void onlineReportResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LoadingForm.Start("Obtaining computer configuration...", Bounds);
                ReportForm form = new ReportForm(History);
                LoadingForm.Stop();

                form.ShowDialog();
            }
            catch (Exception exc)
            {
                Logger.Error("Online report exception occured ...", exc);
            }
        }

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
            dialog.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // Start loading and disable MainForm.
                    LoadingForm.Start(string.Format("Exporting to {0} ....", reportFormat), Bounds);
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
                            BenchmarkTest test = History[0];
                            PdfUtils.Export(saveFileDialogPdf.FileName, ApplicationManager.LayoutManager.StepFrames, test.FlowCount, test.RecordCount, test.Randomness, SystemUtils.GetComputerConfiguration(), reportType);
                            break;
                    }

                    // Stop loading end enable MainForm
                    LoadingForm.Stop();
                    Enabled = true;
                }
                catch (Exception exc)
                {
                    string message = string.Format("Export results to {0} failed...", reportFormat);

                    Logger.Error(message, exc);
                    ReportError(message);
                    Enabled = true;
                    LoadingForm.Stop();
                }
            }
        }

        #endregion

        #region Buttons & Click Events

        private void startButton_Click(object sender, EventArgs e)
        {
            // Parse test parameters.
            TableCount = Int32.Parse(cbFlowsCount.Text.Replace(" ", ""));
            RecordCount = Int64.Parse(cbRecordCount.Text.Replace(" ", ""));
            Randomness = trackBar1.Value / 20.0f;

            var benchmarks = ApplicationManager.SelectedDatabases;
            if (benchmarks.Length == 0)
                return;

            History.Clear();
            Cancellation = new CancellationTokenSource();

            foreach (var benchmark in benchmarks)
            {
                var session = new BenchmarkTest(benchmark, TableCount, RecordCount, Randomness, Cancellation);
                History.Add(session);

                try
                {
                    foreach (var directory in Directory.GetDirectories(benchmark.DataDirectory))
                        Directory.Delete(directory, true);

                    foreach (var files in Directory.GetFiles(benchmark.DataDirectory, "*.*", SearchOption.AllDirectories))
                        File.Delete(files);
                }
                catch (Exception exc)
                {
                    Logger.Error("Application exception occured...", exc);
                }
            }

            ApplicationManager.Prepare();

            // Start the benchmark.
            Logger.Info("Tests started...");
            MainTask = Task.Factory.StartNew(DoBenchmark, Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (MainTask == null)
                return;

            Cancellation.Cancel();
        }

        private void View_Click(object sender, EventArgs e)
        {
            ToolStripButton button = (ToolStripButton)sender;
            int column = Int32.Parse(button.Tag.ToString());

            ApplicationManager.LayoutManager.ShowBarChart(column, button.Checked);
            ((ToolStripMenuItem)showBarChartsToolStripMenuItem.DropDownItems[column]).Checked = button.Checked;
        }

        private void EditBartItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem button = (ToolStripMenuItem)sender;
            int column = Int32.Parse(button.Tag.ToString());

            ApplicationManager.LayoutManager.ShowBarChart(column, button.Checked);
            ((ToolStripButton)toolStripMain.Items[11 + column]).Checked = button.Checked;
        }

        private void axisType_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripButton).Checked;
            ApplicationManager.LayoutManager.GetActiveStepFrame().SelectedChartIsLogarithmic = isChecked;
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
            ApplicationManager.LayoutManager.SelectTreeView();
        }

        #endregion

        #region Docking

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                LoadingForm.Start("Saving project...", Bounds);
                ApplicationManager.Store(saveFileDialogProject.FileName);
                LoadingForm.Stop();
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                LoadingForm.Start("Loading project...", Bounds);
                ApplicationManager.Load(openFileDialogProject.FileName);
                LoadingForm.Stop();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Save current project ?", "Save project", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                saveConfigurationToolStripMenuItem_Click(sender, e);

            LoadingForm.Start("Creating project...", Bounds);
            ApplicationManager.Reset();
            LoadingForm.Stop();
        }

        #endregion

        #region Edit Toolstrip Menu

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool state = ApplicationManager.LayoutManager.IsSelectedTreeViewNode;

            editToolStripMenuItem.DropDownItems[0].Visible = state; // Clone.
            editToolStripMenuItem.DropDownItems[1].Visible = state; // Rename.
            editToolStripMenuItem.DropDownItems[4].Visible = state; // Restore default.
            editToolStripMenuItem.DropDownItems[6].Visible = state; // Separator.
            editToolStripMenuItem.DropDownItems[7].Visible = state; // Properties.
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.CloneNode();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.RenameNade();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.DeleteNode();
        }

        private void restoreDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.RestoreDefault();
        }

        private void restoreDefaultAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.CreateTreeView();
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.ShowProperties();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.TreeView.CollapseAll();
        }

        private void MoveLegend(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            LegendPossition position = (LegendPossition)Enum.Parse(typeof(LegendPossition), item.Text);
            StepFrame selectedFrame = ApplicationManager.LayoutManager.GetActiveStepFrame();

            selectedFrame.SelectedChartPosition = position;

            foreach (ToolStripMenuItem menuItem in legendPossitionToolStripMenuItem.DropDownItems)
                menuItem.Checked = menuItem.Text == item.Text;
        }

        private void showLegendToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripMenuItem).Checked;
            ApplicationManager.LayoutManager.GetActiveStepFrame().SelectedChartLegendIsVisible = isChecked;
        }

        private void logarithmicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripMenuItem).Checked;
            ApplicationManager.LayoutManager.GetActiveStepFrame().SelectedChartIsLogarithmic = isChecked;
        }

        #endregion

        #region View Toolstrip Menu

        private void databasesWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.SelectTreeView();
        }

        private void writeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.SelectFrame(TestMethod.Write);
        }

        private void readWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.SelectFrame(TestMethod.Read);
        }

        private void secondaryReadWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.SelectFrame(TestMethod.SecondaryRead);
        }

        private void logWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationManager.LayoutManager.ShowLogFrame();
        }

        private void resetWindowLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            ApplicationManager.Reset();

            this.ResumeLayout();
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
                if (MainTask != null)
                    btnStop.Enabled = MainTask.Status == TaskStatus.Running ? true : false;

                btnStart.Enabled = !btnStop.Enabled;

                ApplicationManager.LayoutManager.TreeView.TreeViewEnabled = btnStart.Enabled;
                cbFlowsCount.Enabled = btnStart.Enabled;
                cbRecordCount.Enabled = btnStart.Enabled;
                trackBar1.Enabled = btnStart.Enabled;

                cloneToolStripMenuItem.Enabled = btnStart.Enabled;
                renameToolStripMenuItem.Enabled = btnStart.Enabled;
                deleteToolStripMenuItem.Enabled = btnStart.Enabled;
                restoreDefaultToolStripMenuItem.Enabled = btnStart.Enabled;
                restoreDefaultAllToolStripMenuItem.Enabled = btnStart.Enabled;
                propertiesToolStripMenuItem.Enabled = btnStart.Enabled;
                expandAllToolStripMenuItem.Enabled = btnStart.Enabled;
                collapseAllToolStripMenuItem.Enabled = btnStart.Enabled;

                bool isStoped = !(History.Count == 0 || MainTask.Status == TaskStatus.Running);

                btnExportCsv.Enabled = isStoped;
                btnExportJson.Enabled = isStoped;
                toolStripButtonPdfExport.Enabled = isStoped;

                exportResultToPDFToolStripMenuItem.Enabled = isStoped;
                exportToCSVToolStripMenuItem.Enabled = isStoped;
                exportToJSONToolStripMenuItem.Enabled = isStoped;
                onlineReportResultsToolStripMenuItem.Enabled = isStoped;

                legendPossitionToolStripMenuItem.Enabled = showLegendToolStripMenuItem.Checked;

                var activeFrame = ApplicationManager.GetActiveStepFrame();
                var session = Current;

                if (session == null)
                {
                    progressBar.Value = 0;
                    progressStatus.Text = "(None)";
                    percentStatus.Text = "%";
                    elapsedTimeStatus.Text = "Elapsed: ";
                    estimateTimeStatus.Text = "Estimate: ";
                    return;
                }

                var method = session.CurrentMethod;
                if (method == TestMethod.None)
                    return;

                if (autoNavigatetoolStripMenuItem.Checked)
                    ApplicationManager.LayoutManager.StepFrames[method].Activate();

                TimeSpan elapsed = session.GetTime(method);

                long currentRecords = session.GetRecords(method);
                long totalRecords = TableCount * RecordCount;
                double progress = (100.0 * currentRecords) / totalRecords;

                var database = session.Database;

                // Draw charts.
                if (activeFrame.Text != null) // Frame is in write, read or other mode.
                {
                    int averagePossition = activeFrame.lineChartAverageSpeed.GetPointsCount(database.Name);
                    int momentPossition = activeFrame.lineChartMomentSpeed.GetPointsCount(database.Name);

                    var averageSpeedData = session.GetAverageSpeed(method, averagePossition);
                    var momentSpeedData = session.GetMomentSpeed(method, momentPossition);
                    var cpuData = session.GetAverageUserTimeProcessor(method, averagePossition);
                    var memoryData = session.GetAverageWorkingSet(method, averagePossition);
                    var ioData = session.GetAverageDataIO(method, averagePossition);

                    activeFrame.AddAverageSpeed(database.Name, averageSpeedData);
                    activeFrame.AddMomentSpeed(database.Name, momentSpeedData);
                    activeFrame.AddAverageCpuUsage(database.Name, cpuData);
                    activeFrame.AddAverageMemoryUsage(database.Name, memoryData);
                    activeFrame.AddAverageIO(database.Name, ioData);
                }

                if (Math.Abs(progress - 0.0) <= double.Epsilon)
                    estimateTimeStatus.Text = "Estimate: infinity";
                else
                {
                    TimeSpan timeSpan = new TimeSpan((long)((elapsed.Ticks * (100.0 - progress)) / progress));
                    estimateTimeStatus.Text = String.Format("Estimate: {0:dd\\.hh\\:mm\\:ss}", timeSpan);
                }

                elapsedTimeStatus.Text = String.Format("Elapsed: {0:dd\\.hh\\:mm\\:ss} ", elapsed);
                progressBar.Value = (int)progress;
                percentStatus.Text = string.Format("{0:f2}%", progress);
                progressStatus.Text = database.Name + " " + CurrentStatus;
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
            DialogResult result = MessageBox.Show("Save current project?", "Save project", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
                e.Cancel = true;
            else if (result == DialogResult.No)
            {
                stopButton_Click(sender, e);
                ApplicationManager.StoreDocking();
            }
            else if (result == DialogResult.Yes)
            {
                stopButton_Click(sender, e);
                saveConfigurationToolStripMenuItem_Click(sender, e);

                ApplicationManager.StoreDocking();
            }
        }

        private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            StepFrame selectedFrame = ApplicationManager.LayoutManager.GetActiveStepFrame();
            bool state = selectedFrame != null;

            viewToolStripMenuItem.DropDownItems[6].Visible = state;  // Separator.
            viewToolStripMenuItem.DropDownItems[7].Visible = state;  // Show legend.
            viewToolStripMenuItem.DropDownItems[8].Visible = state;  // Legend position.
            viewToolStripMenuItem.DropDownItems[9].Visible = state;  // Logarithmic.

            if (!state)
                return;

            LegendPossition position = selectedFrame.SelectedChartPosition;

            foreach (ToolStripMenuItem menuItem in legendPossitionToolStripMenuItem.DropDownItems)
                menuItem.Checked = menuItem.Text == position.ToString();

            showLegendToolStripMenuItem.Checked = selectedFrame.SelectedChartLegendIsVisible;
            logarithmicToolStripMenuItem.Checked = selectedFrame.SelectedChartIsLogarithmic;
        }
    }
}