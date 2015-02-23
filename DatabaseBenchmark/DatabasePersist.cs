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
        private ILog Logger;
        private int Count;

        public DockContainer DockingContainer { get; private set; }

        public ApplicationPersist(DockContainer dockingContainer)
        {
            DockingContainer = dockingContainer;
        }

        public void StoreDocking(DockPanel panel, Stream stream)
        {
            panel.SaveAsXml(stream, Encoding.Default);
        }

        public void LoadDocking(DockPanel panel, Stream stream)
        {
            Count = 0;

            panel.LoadFromXml(stream, GetContentFromPersistString, true);
        }

        public void StoreDatabases(IDatabase[] databases, Stream stream)
        {
            DatabasePersist persist = new DatabasePersist();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(databases.Length);

            foreach (var database in databases)
                persist.Write(writer, database);
        }

        public IDatabase[] LoadDatabases(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            int length = reader.ReadInt32();

            IDatabase[] databases = new IDatabase[length];
            DatabasePersist persist = new DatabasePersist();

            for (int i = 0; i < databases.Length; i++)
                databases[i] = persist.Read(reader);

            return databases;
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(TreeViewFrame).ToString())
                return DockingContainer.TreeView;

            StepFrame frame = null;
            if (persistString == typeof(StepFrame).ToString())
            {
                if (Count == 0)
                    frame = DockingContainer.Frames[TestMethod.Write.ToString()];
                else if (Count == 1)
                    frame = DockingContainer.Frames[TestMethod.Read.ToString()];
                else if (Count == 2)
                    frame = DockingContainer.Frames[TestMethod.SecondaryRead.ToString()];

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

            // Serialize type.
            writer.Write(type.FullName);

            // Serialize IDatabase members.
            writer.Write(item.DatabaseName);
            writer.Write(item.DatabaseCollection);
            writer.Write(item.DataDirectory);
            writer.Write(item.ConnectionString);
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