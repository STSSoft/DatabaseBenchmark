using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Frames;
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
        private int Count;

        public string DockConfigPath { get; private set; }
        public ProjectSettings SettingsContainer { get; private set; }

        private TreeViewFrame TreeView;
        private DockPanel Panel;
        private Dictionary<string, StepFrame> Frames;
        private LogFrame LogFrame;

        public ProjectManager(ProjectSettings settings, string path)
        {
            Logger = LogManager.GetLogger(Properties.Settings.Default.ApplicationLogger);

            TreeView = settings.TreeView;
            Panel = settings.DockingPanel;
            Frames = settings.Frames;
            LogFrame = settings.LogFrame;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DockConfigPath = Path.Combine(path, Properties.Settings.Default.DockingConfigurationPath);

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
                Frames[method.ToString()] = CreateStepFrame(method);
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
                    Dictionary<IDatabase, bool> databases = TreeView.GetAllDatabases();
                    Dictionary<string, string> selectedItmes = GetSelectedFromComboBoxes(SettingsContainer.ComboBoxes);
                    List<KeyValuePair<string, List<ChartSettings>>> chartSettings = new List<KeyValuePair<string, List<ChartSettings>>>();

                    foreach (var frame in SettingsContainer.Frames)
                        chartSettings.Add(new KeyValuePair<string, List<ChartSettings>>(frame.Value.Text, frame.Value.GetLineChartSettings()));

                    XmlProjectPersist persist = new XmlProjectPersist(databases, selectedItmes, chartSettings, SettingsContainer.TrackBar.Value);

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
            try
            {
                if (!File.Exists(path))
                {
                    TreeView.CreateTreeView();
                    return;
                }

                // Clear TreeView.
                TreeView.ClearTreeViewNodes();

                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(XmlProjectPersist));
                    XmlProjectPersist appPersist = (XmlProjectPersist)deserializer.Deserialize(stream);

                    // Add databases in TreeView.
                    foreach (var database in appPersist.Databases)
                        TreeView.CreateTreeViewNode(database.Key, database.Value);

                    foreach (var comboBox in appPersist.ComboBoxItems)
                        SettingsContainer.ComboBoxes.First(x => x.Name == comboBox.Key).Text = comboBox.Value;

                    foreach (var stepFrame in appPersist.ChartSettings)
                        SettingsContainer.Frames[stepFrame.Key].SetSettings(stepFrame.Value);

                    SettingsContainer.TrackBar.Value = appPersist.TrackBarValue;
                }

                TreeView.ExpandAll();
            }
            catch (Exception exc)
            {
                Logger.Error("Persist load error ...", exc);
                TreeView.CreateTreeView();
            }
        }

        public void Reset()
        {
            ResetDockingConfiguration();

            // Clear TreeView.
            TreeView.ClearTreeViewNodes();

            TreeView.CreateTreeView();
            SettingsContainer.TrackBar.Value = 20;
            SettingsContainer.ComboBoxes[0].SelectedIndex = 0;
            SettingsContainer.ComboBoxes[1].SelectedIndex = 5;
        }

        public void StoreDocking()
        {
            Panel.SaveAsXml(DockConfigPath);
        }

        public void LoadDocking()
        {
            try
            {
                if (File.Exists(DockConfigPath))
                    Panel.LoadFromXml(DockConfigPath, new DeserializeDockContent(GetContentFromPersistString));
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
                TreeView.Text = "Databases";
                LogFrame.Text = "Logs";
            }
        }

        public void SelectFrame(TestMethod method)
        {
            StepFrame frame = Frames[method.ToString()];
            frame.Show(Panel);
        }

        public void ResetDockingConfiguration()
        {
            TreeView.Dispose();

            TreeView = new TreeViewFrame();
            TreeView.CreateTreeView();
            TreeView.Name = "Databases";

            TreeView.Show(Panel);
            TreeView.DockState = DockState.DockLeft;

            foreach (var item in Frames)
            {
                item.Value.Show(Panel);
                item.Value.DockState = DockState.Document;
            }

            Frames[TestMethod.Write.ToString()].Activate();
        }

        public void SelectTreeView()
        {
            if (TreeView.IsHidden)
                TreeView.Show(Panel);
            else
                TreeView.Show(Panel);
        }

        #region Private Methods

        private Dictionary<string, string> GetSelectedFromComboBoxes(params ToolStripComboBox[] comboBoxes)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var comboBox in comboBoxes)
                result.Add(comboBox.Name, comboBox.Text);

            return result;
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

        private void InitializeDockingConfiguration()
        {
            TreeView.Show(Panel);
            TreeView.DockState = DockState.DockLeft;

            foreach (var item in Frames)
            {
                item.Value.Show(Panel);
                item.Value.DockState = DockState.Document;
            }

            LogFrame.Show(Panel);
            LogFrame.DockState = DockState.DockBottomAutoHide;
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return TreeView;

            if (persistString == typeof(LogFrame).ToString())
                return LogFrame;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if (Count == 0)
                    frame = Frames[TestMethod.Write.ToString()];
                else if (Count == 1)
                    frame = Frames[TestMethod.Read.ToString()];
                else if (Count == 2)
                    frame = Frames[TestMethod.SecondaryRead.ToString()];

                Count++;
            }

            return frame;
        }

        #endregion
    }
}