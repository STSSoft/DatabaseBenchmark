using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Frames;
using log4net;
using STS.General.Persist;
using WeifenLuo.WinFormsUI.Docking;
using System.Xml.Serialization;
using System.Xml;
using System.IO.IsolatedStorage;
using System.Windows.Forms;

namespace DatabaseBenchmark.Serialization
{
    /// <summary>
    /// Persists the state of the application (including: application settings, database settings, window layout).
    /// </summary>
    public class ProjectPersist
    {
        public const string DOCKING_CONFIGURATION = "Docking.config";

        private ILog Logger;
        private int Count;

        public string DockConfigPath { get; private set; }
        public ProjectSettings SettingsContainer { get; private set; }

        public ProjectPersist(ProjectSettings settings, string path)
        {
            Logger = LogManager.GetLogger("ApplicationLogger");

            SettingsContainer = settings;
            DockConfigPath = Path.Combine(path, DOCKING_CONFIGURATION);

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
                SettingsContainer.Frames[method.ToString()] = CreateStepFrame(method);
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
                    Dictionary<IDatabase, bool> databases = SettingsContainer.TreeView.GetAllDatabases();
                    Dictionary<string, string> selectedItmes = GetSelectedFromComboBoxes(SettingsContainer.ComboBoxes);

                    XmlProjectPersist persist = new XmlProjectPersist(databases, selectedItmes, SettingsContainer.TrackBar.Value);

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
                    SettingsContainer.TreeView.CreateTreeView();
                    return;
                }

                // Clear TreeView.
                SettingsContainer.TreeView.ClearTreeViewNodes();

                using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(XmlProjectPersist));
                    XmlProjectPersist appPersist = (XmlProjectPersist)deserializer.Deserialize(stream);

                    // Add databases in TreeView.
                    foreach (var database in appPersist.Databases)
                        SettingsContainer.TreeView.CreateTreeViewNode(database.Key, database.Value);

                    foreach (var comboBox in appPersist.ComboBoxItems)
                        SettingsContainer.ComboBoxes.First(x => x.Name == comboBox.Key).Text = comboBox.Value;

                    SettingsContainer.TrackBar.Value = appPersist.TrackBarValue;
                }

                SettingsContainer.TreeView.ExpandAll();
            }
            catch (Exception exc)
            {
                Logger.Error("Persist load error ...", exc);
                SettingsContainer.TreeView.CreateTreeView();
            }
        }

        public void Reset()
        {
            ResetDockingConfiguration();

            // Clear TreeView.
            SettingsContainer.TreeView.ClearTreeViewNodes();

            SettingsContainer.TreeView.CreateTreeView();
            SettingsContainer.TrackBar.Value = 20;
            SettingsContainer.ComboBoxes[0].SelectedIndex = 0;
            SettingsContainer.ComboBoxes[1].SelectedIndex = 5;
        }

        public void StoreDocking()
        {
            SettingsContainer.DockingPanel.SaveAsXml(DockConfigPath);
        }

        public void LoadDocking()
        {
            try
            {
                if (File.Exists(DockConfigPath))
                    SettingsContainer.DockingPanel.LoadFromXml(DockConfigPath, new DeserializeDockContent(GetContentFromPersistString));
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
                SettingsContainer.TreeView.Text = "Databases";
            }
        }

        public void SelectStepFrame(TestMethod method)
        {
            StepFrame frame = SettingsContainer.Frames[method.ToString()];

            if (!frame.IsDisposed)
            {
                frame.Show(SettingsContainer.DockingPanel);
                return;
            }

            StepFrame newFrame = CreateStepFrame(method);
            frame = newFrame;

            newFrame.Show(SettingsContainer.DockingPanel);
            newFrame.DockState = DockState.Document;

            SettingsContainer.Frames[method.ToString()] = newFrame;
        }

        public void ResetDockingConfiguration()
        {
            // Dispose existing windows.
            SettingsContainer.TreeView.Dispose();

            foreach (var frame in SettingsContainer.Frames)
                frame.Value.Dispose();

            SettingsContainer.Frames.Clear();

            // Create new window layout.
            TreeViewFrame TreeViewFrame = new TreeViewFrame();

            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(SettingsContainer.DockingPanel);
            TreeViewFrame.DockState = DockState.DockLeft;

            TreeViewFrame.CreateTreeView();

            SettingsContainer.TreeView = TreeViewFrame;

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
            {
                StepFrame frame = CreateStepFrame(method);
                SettingsContainer.Frames[method.ToString()] = frame;
            }

            foreach (var item in SettingsContainer.Frames)
            {
                item.Value.Show(SettingsContainer.DockingPanel);
                item.Value.DockState = DockState.Document;
            }

            SettingsContainer.Frames[TestMethod.Write.ToString()].Select();
        }

        public void SelectTreeView()
        {
            if (!SettingsContainer.TreeView.IsDisposed)
            {
                SettingsContainer.TreeView.Select();
                return;
            }

            TreeViewFrame TreeViewFrame = new TreeViewFrame();

            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(SettingsContainer.DockingPanel);
            TreeViewFrame.DockState = DockState.DockLeft;

            TreeViewFrame.CreateTreeView();

            SettingsContainer.TreeView = TreeViewFrame;
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
            SettingsContainer.TreeView.Text = "Databases";
            SettingsContainer.TreeView.Show(SettingsContainer.DockingPanel);
            SettingsContainer.TreeView.DockState = DockState.DockLeft;

            foreach (var item in SettingsContainer.Frames)
            {
                item.Value.Show(SettingsContainer.DockingPanel);
                item.Value.DockState = DockState.Document;
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return SettingsContainer.TreeView;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if (Count == 0)
                    frame = SettingsContainer.Frames[TestMethod.Write.ToString()];
                else if (Count == 1)
                    frame = SettingsContainer.Frames[TestMethod.Read.ToString()];
                else if (Count == 2)
                    frame = SettingsContainer.Frames[TestMethod.SecondaryRead.ToString()];

                Count++;
            }

            return frame;
        }

        #endregion
    }
}