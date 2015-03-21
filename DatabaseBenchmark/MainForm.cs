using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Report;
using DatabaseBenchmark.Serialization;
using DatabaseBenchmark.Validation;
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
            BenchmarkSuite benchmarkExecutor = new BenchmarkSuite();
            benchmarkExecutor.OnTestFinish += Report;

            try
            {
                foreach (var benchmark in History)
                {
                    Current = benchmark;
                    benchmarkExecutor.ExecuteInit(benchmark);

                    // Write.
                    ApplicationManager.SetCurrentMethod(TestMethod.Write);
                    CurrentStatus = TestMethod.Write.ToString();

                    benchmarkExecutor.ExecuteWrite(benchmark);

                    // Read.
                    ApplicationManager.SetCurrentMethod(TestMethod.Read);
                    CurrentStatus = TestMethod.Read.ToString();

                    benchmarkExecutor.ExecuteRead(benchmark);

                    // Secondary Read.
                    ApplicationManager.SetCurrentMethod(TestMethod.SecondaryRead);
                    CurrentStatus = TestMethod.SecondaryRead.ToString();

                    benchmarkExecutor.ExecuteSecondaryRead(benchmark);

                    // Finish.
                    CurrentStatus = TestMethod.None.ToString();
                    benchmarkExecutor.ExecuteFinish(benchmark);
                }
            }
            finally
            {
                Current = null;
            }
        }

        #endregion

        #region Report methods

        public void Report(BenchmarkTest benchmark, TestMethod method)
        {
            try
            {
                Action<string, object, Color> updateChart = null;

                StepFrame ActiveStepFrame = ApplicationManager.GetActiveStepFrame();
                string databaseName = benchmark.Database.DatabaseName;
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

        #region Buttons & Click Events

        private void startButton_Click(object sender, EventArgs e)
        {
            if (ApplicationManager.IsDisposedStepFrame)
            {
                MessageBox.Show("Please, restore the test windows from the View menu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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

            ApplicationManager.InitializeCharts();

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
        }

        private void axisType_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripButton).Checked;
            ApplicationManager.LayoutManager.SetLogarithmic(isChecked);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        #region Export

        private void onlineReportResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (History.Count == 0)
            {
                MessageBox.Show("Please, do a test first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ReportForm form = new ReportForm(History);
            form.ShowDialog();
        }

        // CSV.
        private void summaryReportToolStripMenuItemCsv_Click(object sender, EventArgs e)
        {
            ExportToCsv(ReportType.Summary);
        }

        private void detailedReportToolStripMenuItemCsv_Click(object sender, EventArgs e)
        {
            ExportToCsv(ReportType.Detailed);
        }

        // JSON.
        private void summaryReportToolStripMenuItemJson_Click(object sender, EventArgs e)
        {
            ExportToJson(ReportType.Summary);
        }

        private void detailedReportToolStripMenuItemJson_Click(object sender, EventArgs e)
        {
            ExportToJson(ReportType.Detailed);
        }

        // PDF.
        private void summaryReportToolStripMenuItemPdf_Click(object sender, EventArgs e)
        {
            ExportToPdf(ReportType.Summary);
        }

        private void detailedReportToolStripMenuItemPdf_Click(object sender, EventArgs e)
        {
            ExportToPdf(ReportType.Detailed);
        }

        private void ExportToCsv(ReportType reportType)
        {
            saveFileDialogCsv.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);

            if (saveFileDialogCsv.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Loading.Start("Waiting export to CSV...", Bounds);

                    Enabled = false;
                    CsvUtils.ExportResults(History, saveFileDialogCsv.FileName, reportType);
                    Enabled = true;

                    Loading.Stop();
                }
                catch (Exception exc)
                {
                    Logger.Error("Export results to CSV failed...", exc);
                    Enabled = true;
                }
            }
        }

        private void ExportToJson(ReportType type)
        {
            saveFileDialogJson.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);

            if (saveFileDialogJson.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Loading.Start("Waiting export to JSON...", Bounds);
                    Enabled = false;

                    ComputerConfiguration configuration = SystemUtils.GetComputerConfiguration();
                    JsonUtils.ExportToJson(saveFileDialogJson.FileName, configuration, History, type);

                    Enabled = true;
                    Loading.Stop();
                }
                catch (Exception exc)
                {
                    Logger.Error("Export results to JSON failed...", exc);
                    Enabled = true;
                }
            }
        }

        private void ExportToPdf(ReportType reportType)
        {
            saveFileDialogPdf.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);

            if (saveFileDialogPdf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Loading.Start("Waiting export to PDF...", Bounds);

                    Enabled = false;
                    BenchmarkTest test = History[0];
                    PdfUtils.Export(saveFileDialogPdf.FileName, ApplicationManager.LayoutManager.Frames, test.FlowCount, test.RecordCount, test.Randomness, SystemUtils.GetComputerConfiguration(), reportType);
                    Enabled = true;

                    Loading.Stop();
                }
                catch (Exception exc)
                {
                    Logger.Error("Export results to PDF failed...", exc);
                    Enabled = true;
                }
            }
        }

        #endregion

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
                Loading.Start("Saving project...", Bounds);
                ApplicationManager.Store(saveFileDialogProject.FileName);
                Loading.Stop();
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                Loading.Start("Loading project...", Bounds);
                ApplicationManager.Load(openFileDialogProject.FileName);
                Loading.Stop();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Save current project ?", "Save project", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                saveConfigurationToolStripMenuItem_Click(sender, e);

            Loading.Start("Creating project...", Bounds);
            ApplicationManager.Reset();
            Loading.Stop();
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

        private void resetWindowLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            ApplicationManager.Reset();

            this.ResumeLayout();
        }

        #endregion

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

                if (History.Count == 0 || MainTask.Status == TaskStatus.Running)
                {
                    btnExportCsv.Enabled = false;
                    btnExportJson.Enabled = false;
                    toolStripButtonPdfExport.Enabled = false;

                    exportResultToPDFToolStripMenuItem.Enabled = false;
                    exportToCSVToolStripMenuItem.Enabled = false;
                    exportToJSONToolStripMenuItem.Enabled = false;
                }
                else
                {
                    btnExportCsv.Enabled = true;
                    btnExportJson.Enabled = true;
                    toolStripButtonPdfExport.Enabled = true;

                    exportResultToPDFToolStripMenuItem.Enabled = true;
                    exportToCSVToolStripMenuItem.Enabled = true;
                    exportToJSONToolStripMenuItem.Enabled = true;
                }

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

                if (btnAutoNavigate.Checked)
                    ApplicationManager.LayoutManager.Frames[method].Activate();

                TimeSpan elapsed = session.GetTime(method);

                long currentRecords = session.GetRecords(method);
                long totalRecords = TableCount * RecordCount;
                double progress = (100.0 * currentRecords) / totalRecords;

                var database = session.Database;

                // Draw charts.
                if (activeFrame.Text != null) // Frame is in write, read or other mode.
                {
                    int averagePossition = activeFrame.lineChartAverageSpeed.GetPointsCount(database.DatabaseName);
                    int momentPossition = activeFrame.lineChartMomentSpeed.GetPointsCount(database.DatabaseName);

                    var averageSpeedData = session.GetAverageSpeed(method, averagePossition);
                    var momentSpeedData = session.GetMomentSpeed(method, momentPossition);
                    var cpuData = session.GetAverageUserTimeProcessor(method, averagePossition);
                    var memoryData = session.GetAverageWorkingSet(method, averagePossition);
                    var ioData = session.GetAverageDataIO(method, averagePossition);

                    activeFrame.AddAverageSpeed(database.DatabaseName, averageSpeedData);
                    activeFrame.AddMomentSpeed(database.DatabaseName, momentSpeedData);
                    activeFrame.AddAverageCpuUsage(database.DatabaseName, cpuData);
                    activeFrame.AddAverageMemoryUsage(database.DatabaseName, memoryData);
                    activeFrame.AddAverageIO(database.DatabaseName, ioData);
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
                progressStatus.Text = database.DatabaseName + " " + CurrentStatus;
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
    }
}