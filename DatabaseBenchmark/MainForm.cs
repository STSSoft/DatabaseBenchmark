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
using DatabaseBenchmark.Databases;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Validation;
using log4net;
using STS.General.GUI.Extensions;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark
{
    public partial class MainForm : Form
    {
        public static readonly string DATABASES_DIRECTORY = Path.Combine(Application.StartupPath + "\\Databases");
        public static readonly string DOCKING_CONFIGURATION = "Docking.config";
        private static int count = 0;

        private volatile Task MainTask = null;
        private CancellationTokenSource Cancellation;

        private BenchmarkTest Current;
        private List<BenchmarkTest> History;

        private int TableCount;
        private long RecordCount;

        private float Randomness = 0.0f;
        private KeysType KeysType;

        private string CurrentStatus;

        private volatile StepFrame ActiveStepFrame;
        private TreeViewFrame TreeViewFrame;
        private Dictionary<string, StepFrame> TestFrames;

        private ILog Logger;

        public MainForm()
        {
            InitializeComponent();

            History = new List<BenchmarkTest>();

            ActiveStepFrame = new StepFrame();
            TreeViewFrame = new TreeViewFrame();
            TestFrames = new Dictionary<string, StepFrame>();

            btnSpeedView.Tag = 0;
            btnTimeView.Tag = 1;
            btnSizeView.Tag = 2;
            btnIOView.Tag = 3;
            buttonCpuView.Tag = 4;
            buttonMemoryView.Tag = 5;

            cbFlowsCount.SelectedIndex = 0;
            cbRecordCount.SelectedIndex = 5;

            // Trackbar.
            trackBar1.Value = 20; // Sets the randomness to 100%.
            toolStripMain.Items.Insert(toolStripMain.Items.Count - 2, new ToolStripControlHost(trackBar1));

            this.SuspendLayout();

            // Test Frames.
            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
            {
                StepFrame stepFrame = CreateStepFrame(method);
                TestFrames.Add(method.ToString(), stepFrame);
            }

            View_Click(btnTimeView, EventArgs.Empty);

            TreeViewFrame.CreateTreeView();
            
            // Logger.
            Logger = LogManager.GetLogger("ApplicationLogger");

            // Docking.
            if (File.Exists(DOCKING_CONFIGURATION))
                LoadDockingConfiguration(DOCKING_CONFIGURATION);
            else
                InitializeDockingConfiguration();

            TestFrames[TestMethod.Write.ToString()].Select();

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
                MainTask = null;
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
                updateChart = ActiveStepFrame.barChartSpeed.AddPoint;
                ReportSpeed(databaseName, databaseColor, updateChart, benchmark.GetSpeed(method));

                // Size chart.
                updateChart = ActiveStepFrame.barChartSize.AddPoint;
                ReportSize(databaseName, databaseColor, updateChart, benchmark.Database.Size);

                // Time chart.
                updateChart = ActiveStepFrame.barChartTime.AddPoint;
                ReportTime(databaseName, databaseColor, updateChart, benchmark.GetTime(method));

                // CPU chart.
                updateChart = ActiveStepFrame.barChartCPU.AddPoint;
                ReportCPU(databaseName, databaseColor, updateChart, benchmark.GetAverageProcessorTime(method));

                // Memory chart.
                updateChart = ActiveStepFrame.barChartMemory.AddPoint;
                ReportMemory(databaseName, databaseColor, updateChart, benchmark.GetPeakWorkingSet(method));

                // I/O chart.
                updateChart = ActiveStepFrame.barChartIO.AddPoint;
                ReportIO(databaseName, databaseColor, updateChart, benchmark.GetAverageIOData(method));
            }
            catch (Exception exc)
            {
                Logger.Error("Report results failed...", exc);
            }
        }

        private void ReportSpeed(string databaseName, Color databaseColor, Action<string, object, Color> addPoint, double speed)
        {
            try
            {
                Invoke(addPoint, databaseName, speed, databaseColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report speed exception occured...", exc);
            }
        }

        private void ReportSize(string databaseName, Color databaseColor, Action<string, object, Color> addPoint, long size)
        {
            try
            {
                double databaseSize = size / (1024.0 * 1024.0);

                Invoke(addPoint, databaseName, databaseSize, databaseColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report size exception occured...", exc);
            }
        }

        private void ReportTime(string databaseName, Color databaseColor, Action<string, object, Color> addPoint, TimeSpan time)
        {
            try
            {
                DateTime elapsed = new DateTime(time.Ticks);

                Invoke(addPoint, databaseName, elapsed, databaseColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report time exception occured...", exc);
            }
        }

        private void ReportCPU(string databaseName, Color databaseColor, Action<string, object, Color> addPoint, float cpu)
        {
            try
            {
                Invoke(addPoint, databaseName, cpu, databaseColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report CPU exception occured...", exc);
            }
        }

        private void ReportMemory(string databaseName, Color databaseColor, Action<string, object, Color> addPoint, float memory)
        {
            try
            {
                float memoryMBytes = memory / (1024.0f * 1024.0f);

                Invoke(addPoint, databaseName, memoryMBytes, databaseColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report memory exception occured...", exc);
            }
        }

        private void ReportIO(string databaseName, Color databaseColor, Action<string, object, Color> addPoint, float io)
        {
            try
            {
                float ioMBytes = io / (1024.0f * 1024.0f);

                Invoke(addPoint, databaseName, ioMBytes, databaseColor);
            }
            catch (Exception exc)
            {
                Logger.Error("Report I/O exception occured...", exc);
            }
        }

        #endregion

        #region Charts

        private void InitializeCharts(Database[] benchmarks)
        {
            StepFrame stepFrame = null;

            // Clear and prepare charts.
            foreach (var item in TestFrames)
            {
                stepFrame = item.Value;
                stepFrame.ClearCharts();

                stepFrame.barChartSpeed.CreateSeries("Series1", "{#,#}");
                stepFrame.barChartSize.CreateSeries("Series1", "{0:0.#}");
                stepFrame.barChartTime.CreateSeries("Series1", "HH:mm:ss");
                stepFrame.barChartTime.AxisYValueType = ChartValueType.DateTime;

                stepFrame.barChartCPU.CreateSeries("Series1", "{0:0.#}");
                stepFrame.barChartMemory.CreateSeries("Series1", "{0:0.#}");
                stepFrame.barChartIO.CreateSeries("Series1", "{0:0.#}");
                
                stepFrame.barChartSpeed.Title = "Speed (rec/sec)";
                stepFrame.barChartSize.Title = "Size (MB)";
                stepFrame.barChartTime.Title = "Time (hh:mm:ss)";
                stepFrame.barChartCPU.Title = "CPU usage (%)";
                stepFrame.barChartMemory.Title = "Memory usage (MB)";
                stepFrame.barChartIO.Title = "IO Data (MB/sec)";

                foreach (var benchmark in benchmarks)
                {
                    stepFrame.lineChartAverageSpeed.CreateSeries(benchmark.DatabaseName, benchmark.Color);
                    stepFrame.lineChartMomentSpeed.CreateSeries(benchmark.DatabaseName, benchmark.Color);
                    stepFrame.lineChartAverageCPU.CreateSeries(benchmark.DatabaseName, benchmark.Color);
                    stepFrame.lineChartAverageMemory.CreateSeries(benchmark.DatabaseName, benchmark.Color);
                    stepFrame.lineChartAverageIO.CreateSeries(benchmark.DatabaseName, benchmark.Color);
                }
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

            Logger.Info("Tests started...");

            // Parse test parameters.
            TableCount = Int32.Parse(cbFlowsCount.Text.Replace(" ", ""));
            RecordCount = Int64.Parse(cbRecordCount.Text.Replace(" ", ""));
            Randomness = trackBar1.Value / 20.0f;
            KeysType = Randomness == 0f ? KeysType.Sequential : KeysType.Random;

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
            MainTask = Task.Factory.StartNew(DoBenchmark, Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (MainTask == null)
                    return;

                Cancellation.Cancel();
            }
            finally
            {
                MainTask = null;
            }
        }

        #region Export & Report

        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExportCsv_Click(sender, e);
        }

        private void exportToJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnExportJson_Click(sender, e);
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
            saveFileDialogCsv.Title = "Export Results";
            saveFileDialogCsv.Filter = "Microsoft Excel (*.csv)|*.csv";
            saveFileDialogCsv.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);
            saveFileDialogCsv.DefaultExt = "csv";

            saveFileDialogCsv.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fileName = saveFileDialogCsv.FileName;

            try
            {
                BenchmarkTest.ExportSessions(History, fileName);
            }
            catch (Exception exc)
            {
                Logger.Error("Export results to CSV failed...", exc);
            }
        }

        private void btnExportJson_Click(object sender, EventArgs e)
        {
            saveFileDialogJson.Title = "Export Results";
            saveFileDialogJson.Filter = "JSON (*.json)|*.json";
            saveFileDialogJson.FileName = String.Format("Database Benchmark {0:yyyy-MM-dd HH.mm}", DateTime.Now);
            saveFileDialogJson.DefaultExt = "json";

            saveFileDialogJson.ShowDialog();
        }

        private void saveFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string fileName = saveFileDialogJson.FileName;

            try
            {
                ComputerConfiguration configuration = ComputerUtils.GetComputerConfiguration();
                JsonUtils.ExportToJson(fileName, configuration, History);
            }
            catch (Exception exc)
            {
                Logger.Error("Export results to JSON failed...", exc);
            }
        }

        #endregion

        private void View_Click(object sender, EventArgs e)
        {
            foreach (var control in this.Controls.Iterate())
            {
                StepFrame step = control as StepFrame;
                if (step == null)
                    continue;

                ToolStripButton button = (ToolStripButton)sender;
                int column = (int)button.Tag;

                if (button.Checked)
                    step.LayoutPanel.ColumnStyles[column] = new ColumnStyle(SizeType.Percent, 18);
                else
                    step.LayoutPanel.ColumnStyles[column] = new ColumnStyle(SizeType.Absolute, 0);
            }
        }

        private void axisType_Click(object sender, EventArgs e)
        {
            bool isChecked = (sender as ToolStripButton).Checked;
            StepFrame step = null;

            foreach (var item in TestFrames)
            {
                step = item.Value;

                if (isChecked)
                {
                    step.lineChartAverageSpeed.IsLogarithmic = true;
                    step.lineChartMomentSpeed.IsLogarithmic = true;
                    step.lineChartAverageIO.IsLogarithmic = true;
                }
                else
                {
                    step.lineChartAverageSpeed.IsLogarithmic = false;
                    step.lineChartMomentSpeed.IsLogarithmic = false;
                    step.lineChartAverageIO.IsLogarithmic = false;
                }
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
            SaveDockingConfiguration(DOCKING_CONFIGURATION);
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDockingConfiguration(DOCKING_CONFIGURATION);
        }

        private void SaveDockingConfiguration(string fileName)
        {
            dockPanel1.SaveAsXml(fileName);
        }

        private void LoadDockingConfiguration(string fileName)
        {
            count = 0;

            try
            {
                if (File.Exists(DOCKING_CONFIGURATION))
                    dockPanel1.LoadFromXml(fileName, new DeserializeDockContent(GetContentFromPersistString));
                else
                    InitializeDockingConfiguration();
            }
            catch (Exception exc)
            {
                Logger.Error("Load docking configuration error...", exc);
                InitializeDockingConfiguration();
            }
            finally
            {
                TreeViewFrame.Text = "Databases";
            }
        }

        private void InitializeDockingConfiguration()
        {
            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(dockPanel1);
            TreeViewFrame.DockState = DockState.DockLeft;

            foreach (var item in TestFrames)
            {
                item.Value.Show(dockPanel1);
                item.Value.DockState = DockState.Document;
            } 
        }

        private void ResetDockingConfiguration()
        {
            this.SuspendLayout();

            // Dispose existing windows.
            TreeViewFrame.Dispose();

            foreach (var frame in TestFrames)
                frame.Value.Dispose();

            TestFrames.Clear();

            // Create new window layout.
            TreeViewFrame = new TreeViewFrame();

            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(dockPanel1);
            TreeViewFrame.DockState = DockState.DockLeft;

            TreeViewFrame.CreateTreeView();
            
            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
            {
                StepFrame frame = CreateStepFrame(method);
                TestFrames[method.ToString()] = frame;
            }

            foreach (var item in TestFrames)
            {
                item.Value.Show(dockPanel1);
                item.Value.DockState = DockState.Document;
            }

            TestFrames[TestMethod.Write.ToString()].Select();

            this.ResumeLayout();
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return TreeViewFrame;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if(count == 0)
                    frame = TestFrames[TestMethod.Write.ToString()];
                else if(count == 1)
                    frame = TestFrames[TestMethod.Read.ToString()];
                else if (count == 2)
                    frame = TestFrames[TestMethod.SecondaryRead.ToString()];

                count++;
            }

            return frame;
        }

        #endregion

        #region Main Menu Strip View

        private void databasesWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TreeViewFrame.IsDisposed)
            {
                TreeViewFrame.Select();
                return;
            }

            this.SuspendLayout();

            TreeViewFrame = new TreeViewFrame();

            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(dockPanel1);
            TreeViewFrame.DockState = DockState.DockLeft;

            TreeViewFrame.CreateTreeView();

            this.ResumeLayout();
        }

        private void writeWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StepFrame frame =  TestFrames[TestMethod.Write.ToString()];

            if (!frame.IsDisposed)
            {
                frame.Select();
                return;
            }

            StepFrame writeFrame = CreateStepFrame(TestMethod.Write);
            frame = writeFrame;

            writeFrame.Show(dockPanel1);
            writeFrame.DockState = DockState.Document;
        }

        private void readWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StepFrame frame = TestFrames[TestMethod.Read.ToString()];

            if (!frame.IsDisposed)
            {
                frame.Select();
                return;
            }

            StepFrame readFrame = CreateStepFrame(TestMethod.Read);
            frame = readFrame;

            readFrame.Show(dockPanel1);
            readFrame.DockState = DockState.Document;
        }

        private void secondaryReadWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StepFrame frame = TestFrames[TestMethod.SecondaryRead.ToString()];

            if (!frame.IsDisposed)
            {
                frame.Select();
                return;
            }

            StepFrame secondaryReadFrame = CreateStepFrame(TestMethod.SecondaryRead);
            frame = secondaryReadFrame;

            secondaryReadFrame.Show(dockPanel1);
            secondaryReadFrame.DockState = DockState.Document;
        }

        private void resetWindowLayoutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (File.Exists(DOCKING_CONFIGURATION))
                File.Delete(DOCKING_CONFIGURATION);

            ResetDockingConfiguration();
        }

        private StepFrame CreateStepFrame(TestMethod method)
        {
            StepFrame stepFrame = new StepFrame();
            stepFrame.Text = method.ToString();
            stepFrame.Dock = DockStyle.Fill;

            // Hide time, CPU, memory and I/O view from the layout.
            stepFrame.LayoutPanel.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 0);
            stepFrame.LayoutPanel.ColumnStyles[3] = new ColumnStyle(SizeType.Absolute, 0);
            stepFrame.LayoutPanel.ColumnStyles[4] = new ColumnStyle(SizeType.Absolute, 0);
            stepFrame.LayoutPanel.ColumnStyles[5] = new ColumnStyle(SizeType.Absolute, 0);

            return stepFrame;
        }

        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                btnStop.Enabled = MainTask != null;
                btnStart.Enabled = !btnStop.Enabled;
                btnExportCsv.Enabled = !btnStop.Enabled;

                TreeViewFrame.treeView.Enabled = btnStart.Enabled;
                cbFlowsCount.Enabled = btnStart.Enabled;
                cbRecordCount.Enabled = btnStart.Enabled;
                trackBar1.Enabled = btnStart.Enabled;

                if (History.Count == 0)
                {
                    btnExportCsv.Enabled = false;
                    btnExportJson.Enabled = false;
                }
                else
                {
                    btnExportCsv.Enabled = true;
                    btnExportJson.Enabled = true;
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
                    var averagePossition = activeFrame.lineChartAverageSpeed.GetPointsCount(database.DatabaseName);
                    var momentPossition = activeFrame.lineChartMomentSpeed.GetPointsCount(database.DatabaseName);

                    // Average speed.
                    foreach (var item in session.GetAverageSpeed(method, averagePossition))
                        activeFrame.lineChartAverageSpeed.AddPoint(database.DatabaseName, item.Key, item.Value);

                    // Moment speed.
                    foreach (var item in session.GetMomentSpeed(method, momentPossition))
                        activeFrame.lineChartMomentSpeed.AddPoint(database.DatabaseName, item.Key, item.Value);

                    // Average CPU usage.
                    foreach (var item in session.GetAverageUserTimeProcessor(method, momentPossition))
                        activeFrame.lineChartAverageCPU.AddPoint(database.DatabaseName, item.Key, item.Value);

                    // Average memory usage.
                    foreach (var item in session.GetAverageWorkingSet(method, averagePossition))
                        activeFrame.lineChartAverageMemory.AddPoint(database.DatabaseName, item.Key, item.Value / (1024.0 * 1024.0));

                    // Average Data IO.
                    foreach (var item in session.GetAverageDataIO(method, averagePossition))
                        activeFrame.lineChartAverageIO.AddPoint(database.DatabaseName, item.Key, item.Value / (1024.0 * 1024.0));
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
            SaveDockingConfiguration(DOCKING_CONFIGURATION);
            stopButton_Click(sender, e);

            Application.Exit();
        }
    }
}