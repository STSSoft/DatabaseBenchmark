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

namespace DatabaseBenchmark
{
    /// <summary>
    /// Persists the state of the application (including: database settings, window layout.)
    /// </summary>
    public class ApplicationPersist
    {
        public static readonly string DOCKING_CONFIGURATION = "Docking.config";
        public static readonly string APPLICATION_PERSIST_CONFIGURATION = "Persist.config";

        private ILog Logger;
        private int Count;
        private string ApplicationConfigPath;
        private string DockConfigPath;
        private Stream stream;

        public DockContainer Container { get; private set; }

        public ApplicationPersist(ILog logger, DockContainer dockingContainer, string folderPath)
        {
            Container = dockingContainer;
            Logger = logger;

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            ApplicationConfigPath = Path.Combine(folderPath, APPLICATION_PERSIST_CONFIGURATION);
            DockConfigPath = Path.Combine(folderPath, DOCKING_CONFIGURATION);
        }

        public void Store()
        {
            try
            {
                StoreDocking();

                stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate);
                Tuple<IDatabase, bool>[] databases = Container.TreeView.GetAllBenchmarks();

                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(databases.Length);

                StoreDatabases(databases, writer);

                writer.Write(Container.Frames.Count);

                foreach (var frame in Container.Frames)
                {
                    writer.Write(frame.Key);
                    writer.Write(frame.Value.Text);
                    writer.Write(frame.Value.DockState.ToString());
                }

                writer.Close();
            }
            catch (Exception exc)
            {
                Logger.Error("Persist store error ...", exc);
                stream.Close();
            }
        }

        public void Load()
        {
            try
            {
                LoadDocking();

                if (!File.Exists(ApplicationConfigPath))
                {
                    Container.TreeView.CreateTreeView();

                    return;
                }

                //Clear TreeView
                Container.TreeView.treeView.Nodes.Clear();

                stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate);
                BinaryReader reader = new BinaryReader(stream);

                int dbCount = reader.ReadInt32();

                LoadDatabases(reader);

                Container.TreeView.treeView.ExpandAll();

                int countFrames = reader.ReadInt32();

                for (int i = 0; i < countFrames; i++)
                {
                    string frameName = reader.ReadString();
                    string frameText = reader.ReadString();
                    DockState state = (DockState)Enum.Parse(typeof(DockState), reader.ReadString());

                    Container.Frames[frameText].Show(Container.DockingPanel);
                    Container.Frames[frameText].DockState = state;
                }

                reader.Close();
            }
            catch (Exception exc)
            {
                Logger.Error("Persist load error ...", exc);

                //Restore TreeView
                Container.TreeView.CreateTreeView();
                stream.Close();
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
    }

    public class DockContainer
    {
        public DockPanel DockingPanel { get; private set; }
        public TreeViewFrame TreeView { get; private set; }
        public Dictionary<string, StepFrame> Frames { get; private set; }

        public DockContainer()
        {
        }

        public DockContainer(DockPanel panel, TreeViewFrame treeView, Dictionary<string, StepFrame> frames)
        {
            DockingPanel = panel;
            TreeView = treeView;
            Frames = frames;
        }
    }
}