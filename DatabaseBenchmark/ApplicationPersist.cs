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

namespace DatabaseBenchmark
{
    /// <summary>
    /// Persists the state of the application (including: application settings, database settings, window layout).
    /// </summary>
    public class ApplicationPersist
    {
        public static readonly string DOCKING_CONFIGURATION = "Docking.config";
        public static readonly string APPLICATION_CONFIGURATION = "AppConfig.bin";

        private ILog Logger;
        private int Count;

        public string ApplicationConfigPath { get; private set; }
        public string DockConfigPath { get; private set; }

        public DockContainer Container { get; private set; }
        public string ConfigurationFolder { get; private set; }

        public ApplicationPersist(DockContainer dockingContainer, string configFolder)
        {
            Container = dockingContainer;
            ConfigurationFolder = configFolder;

            Logger = LogManager.GetLogger("ApplicationLogger");

            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            ApplicationConfigPath = Path.Combine(configFolder, APPLICATION_CONFIGURATION);
            DockConfigPath = Path.Combine(configFolder, DOCKING_CONFIGURATION);
        }

        public void Store()
        {
            try
            {
                // Docking.
                StoreDocking();

                // Databases and frames.
                using (var stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    Tuple<IDatabase, bool>[] databases = Container.TreeView.GetAllDatabases();

                    StoreDatabases(databases, writer);
                    StoreFrames(writer, Container.Frames.Select(x => x.Value), Container.Frames.Count);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Persist store error ...", exc);
            }
        }

        public void Load()
        {
            try
            {
                // Docking.
                LoadDocking();

                if (!File.Exists(ApplicationConfigPath))
                {
                    Container.TreeView.CreateTreeView();
                    return;
                }

                // Clear TreeView.
                Container.TreeView.treeView.Nodes.Clear();

                using(var stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate))
                {
                    BinaryReader reader = new BinaryReader(stream);

                    LoadDatabases(reader);
                    LoadFrames(reader);

                    Container.TreeView.treeView.ExpandAll();
                }
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

        #region Private Methods

        private void StoreFrames(BinaryWriter writer, IEnumerable<StepFrame> frames, int count)
        {
            writer.Write(Container.Frames.Count);

            foreach (var frame in Container.Frames)
            {
                writer.Write(frame.Value.Text);
                writer.Write(frame.Value.DockState.ToString());
            }
        }

        private void LoadFrames(BinaryReader reader)
        {
            int framesCount = reader.ReadInt32();

            for (int i = 0; i < framesCount; i++)
            {
                string frameText = reader.ReadString();

                DockState state = (DockState)Enum.Parse(typeof(DockState), reader.ReadString());

                Container.Frames[frameText].Show(Container.DockingPanel);
                Container.Frames[frameText].DockState = state;
            }
        }

        private void StoreDatabases(Tuple<IDatabase, bool>[] databases, BinaryWriter writer)
        {
            DatabasePersist persist = new DatabasePersist();
            writer.Write(databases.Length);

            foreach (var database in databases)
            {
                writer.Write(database.Item2);
                persist.Write(writer, database.Item1);
            }
        }

        private Tuple<IDatabase, bool>[] LoadDatabases(BinaryReader reader)
        {
            int length = reader.ReadInt32();

            Tuple<IDatabase, bool>[] databases = new Tuple<IDatabase, bool>[length];
            DatabasePersist persist = new DatabasePersist();

            for (int i = 0; i < databases.Length; i++)
            {
                bool checkedState = reader.ReadBoolean();
                IDatabase database = persist.Read(reader);

                Container.TreeView.CreateTreeViewNode(database, checkedState);
            }

            return databases;
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

            return null;
        }

        #endregion
    }

    public class DatabasePersist : IPersist<IDatabase>
    {
        public void Write(BinaryWriter writer, IDatabase item)
        {
            Type type = item.GetType();

            // Serialize the type.
            writer.Write(type.FullName);

            // Serialize IDatabase members.
            writer.Write(item.DatabaseName);
            writer.Write(item.DatabaseCollection == null ? string.Empty : item.DatabaseCollection);
            writer.Write(item.DataDirectory);
            writer.Write(item.ConnectionString == null ? string.Empty : item.ConnectionString);
            writer.Write(item.Category);
            writer.Write(item.Description);
            writer.Write(item.Website);
            writer.Write(item.Color.ToArgb());

            writer.Write(item.Requirements.Length);

            foreach (var element in item.Requirements)
                writer.Write(element);
        }

        public IDatabase Read(BinaryReader reader)
        {
            string typeName = reader.ReadString();
            Type type = Type.GetType(typeName);

            IDatabase database = (IDatabase)Activator.CreateInstance(type);

            database.DatabaseName = reader.ReadString();
            database.DatabaseCollection = reader.ReadString();
            database.DataDirectory = reader.ReadString();
            database.ConnectionString = reader.ReadString();
            database.Category = reader.ReadString();
            database.Description = reader.ReadString();
            database.Website = reader.ReadString();
            database.Color = Color.FromArgb(reader.ReadInt32());

            int length = reader.ReadInt32();
            string[] requirements = new string[length];

            for (int i = 0; i < length; i++)
                requirements[i] = reader.ReadString();

            return database;
        }

        //private  class SerializeObjects

    }

    public class DockContainer
    {
        public DockPanel DockingPanel { get; set; }
        public TreeViewFrame TreeView { get; set; }
        public Dictionary<string, StepFrame> Frames { get; set; }

        public DockContainer(DockPanel panel, TreeViewFrame treeView, Dictionary<string, StepFrame> frames)
        {
            DockingPanel = panel;
            TreeView = treeView;
            Frames = frames;
        }
    }
}