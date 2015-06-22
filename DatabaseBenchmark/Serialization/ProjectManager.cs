using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
using DatabaseBenchmark.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Serialization
{
    /// <summary>
    /// Persists the state of the application (including: application settings, database settings, window layout).
    /// </summary>
    public class ProjectManager
    {
        private ILog Logger;
        private LayoutManager LayoutManager;
        private volatile TestMethod CurrentMethod;

        public string DockConfigPath { get; private set; }

        public ProjectManager(DockPanel panel, ToolStripComboBox[] comboBoxes, List<ToolStripButton> buttons,  TrackBar trackBar, string path)
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
            LayoutManager = new LayoutManager(panel, comboBoxes, buttons, trackBar);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DockConfigPath = Path.Combine(path, Settings.Default.DockingConfigurationPath);
        }

        public void Store(string path)
        {
            try
            {
                // Docking.
                StoreDocking();

                // Remove last configuration.
                if (File.Exists(path))
                    File.Delete(path);

                // Databases and frames.
                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    Dictionary<IDatabase, bool> databases = LayoutManager.TreeView.GetAllDatabases();
                    Dictionary<string, string> selectedItmes = LayoutManager.GetSelectedFromComboBoxes();
                    Dictionary<string, bool> toolStripButtons = LayoutManager.GetToolStripCheckedButtons();

                    List<KeyValuePair<TestMethod, List<ChartSettings>>> chartSettings = new List<KeyValuePair<TestMethod, List<ChartSettings>>>();

                    foreach (var frame in LayoutManager.StepFrames)
                        chartSettings.Add(new KeyValuePair<TestMethod, List<ChartSettings>>(frame.Key, frame.Value.GetLineChartSettings()));

                    XmlProjectPersist persist = new XmlProjectPersist(databases,LayoutManager.TreeView.IsCategoryOrder(), selectedItmes, toolStripButtons, chartSettings, LayoutManager.TrackBar.Value);

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlProjectPersist));
                    serializer.Serialize(stream, persist);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Persist store error ...", exc);
            }
        }

        public void Load(string path)
        {
            TreeViewFrame treeView = LayoutManager.TreeView;

            try
            {
                if (!File.Exists(path))
                    treeView.CreateTreeView();

                // Clear TreeView.
                treeView.ClearTreeViewNodes();

                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(XmlProjectPersist));
                    XmlProjectPersist appPersist = (XmlProjectPersist)deserializer.Deserialize(stream);

                    // Add databases in TreeView.
                    foreach (var database in appPersist.Databases)
                        treeView.CreateTreeViewNode(database.Key, database.Value, appPersist.IsCategoryOrder);

                    foreach (var comboBox in appPersist.ComboBoxItems)
                        LayoutManager.ComboBoxes.First(x => x.Name == comboBox.Key).Text = comboBox.Value;

                    foreach (var btn in appPersist.Buttons)
                    {
                        var currentBtn = LayoutManager.Buttons.First(x => x.Name == btn.Key);
                        currentBtn.Checked = !btn.Value;
                        currentBtn.PerformClick();
                    }

                    foreach (var stepFrame in appPersist.ChartSettings)
                        LayoutManager.StepFrames[stepFrame.Key].SetSettings(stepFrame.Value);

                    LayoutManager.TrackBar.Value = appPersist.TrackBarValue;
                }

                treeView.ExpandAll();
                treeView.SelectFirstNode();

                LayoutManager.RefreshPropertyFrame();
            }
            catch (Exception exc)
            {
                Logger.Error("Persist load error ...", exc);
                treeView.CreateTreeView();
            }
        }

        public void Reset()
        {
            LayoutManager.Reset();
            LayoutManager.ResetDocking();
        }

        public void StoreDocking()
        {
            LayoutManager.StoreDocking(DockConfigPath);
        }

        public void LoadDocking()
        {
            try
            {
                if (File.Exists(DockConfigPath))
                    LayoutManager.LoadDocking(DockConfigPath);
                else
                    LayoutManager.Reset();
            }
            catch (Exception exc)
            {
                Logger.Error("Load docking configuration error...", exc);
                LayoutManager.Reset();
            }
        }

        public void ResetDocking()
        {
            LayoutManager.ResetDocking();
        }

        public void SetCurrentMethod(TestMethod method)
        {
            CurrentMethod = method;
        }

        public StepFrame GetCurrentStepFrame()
        {
            return LayoutManager.StepFrames[CurrentMethod];
        }

        public StepFrame GetSelectedStepFrame()
        {
            return LayoutManager.GetActiveStepFrame();
        }

        public void ShowStepFrame(TestMethod method)
        {
            LayoutManager.SelectFrame(method);
        }

        public void ShowBarChart(int column, bool visible)
        {
            LayoutManager.ShowBarChart(column, visible);
        }

        public void SelectTreeView(bool visible)
        {
            LayoutManager.SelectTreeView(visible);
        }

        public void ShowLogFrame()
        {
            LayoutManager.ShowLogFrame();
        }
        public void ShowProperties()
        {
            LayoutManager.ShowProperties();
        }

        public void EnablePropertyFrame(bool state)
        {
            LayoutManager.EnablePropertyFrame(state);
        }

        public void Prepare()
        {
            LayoutManager.InitializeCharts();
            LayoutManager.ClearLog();
        }

        public void ClearCharts()
        {
            LayoutManager.ClearCharts();
        }

        public Database[] SelectedDatabases
        {
            get { return LayoutManager.TreeView.GetSelectedBenchmarks(); } // TODO: fix to GetSelectedDatabases()
        }

        public TreeViewFrame TreeView
        {
            get { return LayoutManager.TreeView; }
        }

        public Dictionary<TestMethod, StepFrame> StepFrames
        {
            get { return LayoutManager.StepFrames; }
        }
    }
}