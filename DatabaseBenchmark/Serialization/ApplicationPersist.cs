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
    public class ApplicationPersist
    {
        public static readonly string DOCKING_CONFIGURATION = "Docking.config";
        public static readonly string APPLICATION_CONFIGURATION = "AppConfig.config";

        private ILog Logger;
        private int Count;

        public string DockConfigPath { get; private set; }
        public string ApplicationConfigPath { get; private set; }

        public AppSettings Container { get; private set; }

        public ApplicationPersist(AppSettings dockingContainer, string configurationFolder, params TestMethod[] methods)
        {
            Container = dockingContainer;

            Logger = LogManager.GetLogger("ApplicationLogger");

            DockConfigPath = Path.Combine(configurationFolder, DOCKING_CONFIGURATION);
            ApplicationConfigPath = Path.Combine(configurationFolder, APPLICATION_CONFIGURATION);

            foreach (var method in methods)
                Container.Frames[method.ToString()] = CreateStepFrame(method);
        }

        public void Store(string directoryPath)
        {
            try
            {
                if(directoryPath != null)
                    ApplicationConfigPath = Path.Combine(directoryPath, APPLICATION_CONFIGURATION);

                // Docking.
                StoreDocking();

                // Remove last configuration.
                if (File.Exists(ApplicationConfigPath))
                    File.Delete(ApplicationConfigPath);

                // Databases and frames.
                using (var stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate))
                {
                    Dictionary<IDatabase, bool> databases = Container.TreeView.GetAllDatabases();
                    XmlAppSettingsPersist persist = new XmlAppSettingsPersist(databases, Container.GetComboBoxSelectedItems(), Container.TrackBar.Value);

                    XmlSerializer serializer = new XmlSerializer(typeof(XmlAppSettingsPersist));
                    serializer.Serialize(stream, persist);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Persist store error ...", exc);
            }
        }

        public void Load(string filePath)
        {
            try
            {
                if (filePath != null)
                    ApplicationConfigPath = filePath;

                // Docking.
                LoadDocking();

                if (!File.Exists(ApplicationConfigPath))
                {
                    Container.TreeView.CreateTreeView();
                    return;
                }

                // Clear TreeView.
                Container.TreeView.treeView.Nodes.Clear();

                using (var stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(XmlAppSettingsPersist));
                    XmlAppSettingsPersist deserializeObj = (XmlAppSettingsPersist)deserializer.Deserialize(stream);

                    // Add databases in TreeView.
                    foreach (var db in deserializeObj.Databases)
                        Container.TreeView.CreateTreeViewNode(db.Key, db.Value);

                    foreach (var cb in deserializeObj.ComboBoxItems)
                        Container.ComboBoxes.First(x => x.Name == cb.Key).Text = cb.Value;

                    Container.TrackBar.Value = deserializeObj.TrackBarValue;
                }

                Container.TreeView.treeView.ExpandAll();
            }
            catch (Exception exc)
            {
                Logger.Error("Persist load error ...", exc);
                Container.TreeView.CreateTreeView();
            }
        }

        public void StoreDocking()
        {
            Container.DockingPanel.SaveAsXml(DockConfigPath);
        }

        public void LoadDocking()
        {
            try
            {
                if (File.Exists(DockConfigPath))
                    Container.DockingPanel.LoadFromXml(DockConfigPath, new DeserializeDockContent(GetContentFromPersistString));
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
                Container.TreeView.Text = "Databases";
            }
        }

        public void SelectStepFrame(TestMethod method)
        {
            StepFrame frame = Container.Frames[method.ToString()];

            if (!frame.IsDisposed)
            {
                frame.Select();
                return;
            }

            StepFrame readFrame = CreateStepFrame(method);
            frame = readFrame;

            readFrame.Show(Container.DockingPanel);
            readFrame.DockState = DockState.Document;
        }

        public void ResetDockingConfiguration()
        {
            // Dispose existing windows.
            Container.TreeView.Dispose();

            foreach (var frame in Container.Frames)
                frame.Value.Dispose();

            Container.Frames.Clear();

            // Create new window layout.
            TreeViewFrame TreeViewFrame = new TreeViewFrame();

            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(Container.DockingPanel);
            TreeViewFrame.DockState = DockState.DockLeft;

            TreeViewFrame.CreateTreeView();

            Container.TreeView = TreeViewFrame;

            foreach (var method in new TestMethod[] { TestMethod.Write, TestMethod.Read, TestMethod.SecondaryRead })
            {
                StepFrame frame = CreateStepFrame(method);
                Container.Frames[method.ToString()] = frame;
            }

            foreach (var item in Container.Frames)
            {
                item.Value.Show(Container.DockingPanel);
                item.Value.DockState = DockState.Document;
            }

            Container.Frames[TestMethod.Write.ToString()].Select();
        }

        public void SelectTreeView()
        {
            if (!Container.TreeView.IsDisposed)
            {
                Container.TreeView.Select();
                return;
            }

            TreeViewFrame TreeViewFrame = new TreeViewFrame();

            TreeViewFrame.Text = "Databases";
            TreeViewFrame.Show(Container.DockingPanel);
            TreeViewFrame.DockState = DockState.DockLeft;

            TreeViewFrame.CreateTreeView();
        }

        #region Private Methods

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
            Container.TreeView.Text = "Databases";
            Container.TreeView.Show(Container.DockingPanel);
            Container.TreeView.DockState = DockState.DockLeft;

            foreach (var item in Container.Frames)
            {
                item.Value.Show(Container.DockingPanel);
                item.Value.DockState = DockState.Document;
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return Container.TreeView;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if (Count == 0)
                    frame = Container.Frames[TestMethod.Write.ToString()];
                else if (Count == 1)
                    frame = Container.Frames[TestMethod.Read.ToString()];
                else if (Count == 2)
                    frame = Container.Frames[TestMethod.SecondaryRead.ToString()];

                Count++;
            }

            return frame;
        }

        #endregion
    }
}