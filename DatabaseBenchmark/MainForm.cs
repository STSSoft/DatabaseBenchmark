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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Report;
using DatabaseBenchmark.Serialization;
using DatabaseBenchmark.Validation;
using log4net;
using STS.General.GUI.Extensions;
using WeifenLuo.WinFormsUI.Docking;

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

        private volatile StepFrame ActiveStepFrame;
        private TreeViewFrame TreeViewFrame;
        private Dictionary<string, StepFrame> TestFrames;

        private ILog Logger;

        private ApplicationPersist ApplicationPersist;

        public MainForm()
        {
            InitializeComponent();

            History = new List<BenchmarkTest>();

            ActiveStepFrame = new StepFrame();
            TreeViewFrame = new TreeViewFrame();
            TestFrames = new Dictionary<string, StepFrame>();

            cbFlowsCount.SelectedIndex = 0;
            cbRecordCount.SelectedIndex = 5;

            // Trackbar.
            trackBar1.Value = 20; // Sets the randomness to 100%.
            toolStripMain.Items.Insert(toolStripMain.Items.Count - 2, new ToolStripControlHost(trackBar1));

            this.SuspendLayout();

            View_Click(btnSizeView, EventArgs.Empty);

            // Logger.
            Logger = LogManager.GetLogger("ApplicationLogger");

            AppSettings containerSettings = new AppSettings(dockPanel1, TreeViewFrame, new ToolStripComboBox[] { cbFlowsCount, cbRecordCount }, trackBar1);
            ApplicationPersist = new ApplicationPersist(containerSettings, CONFIGURATION_FOLDER);

            TestFrames = ApplicationPersist.SettingsContainer.Frames;

            // Load dock and application configuration.
            ApplicationPersist.Load(Path.Combine(CONFIGURATION_FOLDER, "AppConfig.config"));
            ApplicationPersist.LoadDocking();

            TestFrames[TestMethod.Write.ToString()].Select();

            openFileDialogAppConfig.InitialDirectory = CONFIGURATION_FOLDER;
            saveFileDialogPersist.InitialDirectory = CONFIGURATION_FOLDER;

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
                    CurrentStatus = TestMethod.Write.ToString();
                    ActiveStepFrame = TestFrames[CurrentStatus];

                    benchmarkExecutor.ExecuteWrite(benchmark);

                    // Read.
                    CurrentStatus = TestMethod.Read.ToString();
                    ActiveStepFrame = TestFrames[CurrentStatus];

                    benchmarkExecutor.ExecuteRead(benchmark);

                    // Secondary Read.
                    CurrentStatus = TestMethod.SecondaryRead.ToString();
                    ActiveStepFrame = TestFrames[CurrentStatus];

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

        #region Charts

        private void InitializeCharts(Database[] databases)
        {
            StepFrame stepFrame = null;

            // Clear and prepare charts.
            foreach (var item in TestFrames)
            {
                stepFrame = item.Value;

                stepFrame.ClearCharts();
                stepFrame.InitializeCharts(databases.Select(x => new KeyValuePair<string, Color>(x.DatabaseName, x.Color)));
            }
        }

        #endregion

        #region Buttons & Click Events

        private void startButton_Click(object sender, EventArgs e)
        {
            if (TestFrames.Any(frame => frame.Value.IsDisposed))
            {
                MessageBox.Show("Please, restore the test windows from the View menu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse test parameters.
            TableCount = Int32.Parse(cbFlowsCount.Text.Replace(" ", ""));
            RecordCount = Int64.Parse(cbRecordCount.Text.Replace(" ", ""));
            Randomness = trackBar1.Value / 20.0f;

            var benchmarks = TreeViewFrame.GetSelectedBenchmarks();
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

            InitializeCharts(benchmarks);

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
            foreach (var item in TestFrames)
            {
                StepFrame frame = item.Value;

                ToolStripButton button = (ToolStripButton)sender;
                int column = Int32.Parse(button.Tag.ToString());

                if (button.Checked)
                    frame.LayoutPanel.ColumnStyles[column] = new ColumnStyle(SizeType.Percent, 18);
                else
                    frame.LayoutPanel.ColumnStyles[column] = new ColumnStyle(SizeType.Absolute, 0);
            }
        }

        private void axisType_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripButton).Checked;
            StepFrame step = null;

            foreach (var item in TestFrames)
            {
                step = item.Value;
                step.SetLogarithmic(isChecked);
            }
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

        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExportCsv_Click(sender, e);
        }

        private void exportToJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExportJson_Click(sender, e);
        }

        private void exportResultToPDFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripButtonPdfExport_Click(sender, e);
        }

        private void reportResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (History.Count == 0)
            {
                MessageBox.Show("Please, do a test first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ReportForm form = new ReportForm(History);
            form.Show();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            saveFileDialogCsv.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);
            saveFileDialogCsv.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fileName = saveFileDialogCsv.FileName;

            try
            {
                CsvUtils.ExportTestResults(History, fileName);
            }
            catch (Exception exc)
            {
                Logger.Error("Export results to CSV failed...", exc);
            }
        }

        private void btnExportJson_Click(object sender, EventArgs e)
        {
            saveFileDialogJson.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);
            saveFileDialogJson.ShowDialog();
        }

        private void saveFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fileName = saveFileDialogJson.FileName;

            try
            {
                ComputerConfiguration configuration = SystemUtils.GetComputerConfiguration();
                JsonUtils.ExportToJson(fileName, configuration, History);
            }
            catch (Exception exc)
            {
                Logger.Error("Export results to JSON failed...", exc);
            }
        }

        private void toolStripButtonPdfExport_Click(object sender, EventArgs e)
        {
            saveFileDialogPdf.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);
            saveFileDialogPdf.ShowDialog();
        }

        private void saveFileDialogPdf_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                PdfUtils.Export(saveFileDialogPdf.FileName, TestFrames, SystemUtils.GetComputerConfiguration());
            }
            catch (Exception exc)
            {
                Logger.Error("Export results to PDF failed...", exc);
            }
        }

        #endregion

        #endregion

        #region TreeView

        private void buttonTreeView_Click(object sender, EventArgs e)
        {
            if (TreeViewFrame.IsDisposed)
                databasesWindowToolStripMenuItem_Click(null, EventArgs.Empty);

            if (btnTreeView.Checked)
                TreeViewFrame.DockState = DockState.DockLeft;
            else
                TreeViewFrame.DockState = DockState.DockLeftAutoHide;
        }

        #endregion

        #region Docking

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialogPersist.ShowDialog() == DialogResult.OK)
                ApplicationPersist.Store(saveFileDialogPersist.FileName);
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogAppConfig.ShowDialog() == DialogResult.OK)
                ApplicationPersist.Load(openFileDialogAppConfig.FileName);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationPersist.Reset();
        }

        #endregion

        #region Main Menu Strip View

        private void databasesWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            ApplicationPersist.SelectTreeView();
            TreeViewFrame = ApplicationPersist.SettingsContainer.TreeView;

            this.ResumeLayout();
        }

        private void writeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationPersist.SelectStepFrame(TestMethod.Write);
        }

        private void readWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationPersist.SelectStepFrame(TestMethod.Read);
        }

        private void secondaryReadWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplicationPersist.SelectStepFrame(TestMethod.SecondaryRead);
        }

        private void resetWindowLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            ApplicationPersist.ResetDockingConfiguration();
            TreeViewFrame = ApplicationPersist.SettingsContainer.TreeView;
            TestFrames = ApplicationPersist.SettingsContainer.Frames;

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

                TreeViewFrame.TreeViewEnabled = btnStart.Enabled;
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

                var activeFrame = ActiveStepFrame;
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
                    TestFrames[method.ToString()].Activate();

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
            DialogResult result = MessageBox.Show("Save database settings?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                saveConfigurationToolStripMenuItem_Click(sender, e);

            stopButton_Click(sender, e);
        }
    }
}