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

namespace DatabaseBenchmark
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

        public string ApplicationConfigPath { get; private set; }
        public string DockConfigPath { get; private set; }

        public AppSettings Container { get; private set; }
        public string ConfigurationFolder { get; private set; }

        public ApplicationPersist(AppSettings dockingContainer, string configFolder)
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

                // Remove last configuration.
                if (File.Exists(ApplicationConfigPath))
                    File.Delete(ApplicationConfigPath);

                // Databases and frames.
                using (var stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate))
                {
                    Dictionary<IDatabase, bool> databases = Container.TreeView.GetAllDatabases();
                    DatabaseXmlPersist persist = new DatabaseXmlPersist(databases);

                    XmlSerializer serializer = new XmlSerializer(typeof(DatabaseXmlPersist));
                    serializer.Serialize(stream, persist);
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

                using (var stream = new FileStream(ApplicationConfigPath, FileMode.OpenOrCreate))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(DatabaseXmlPersist));
                    DatabaseXmlPersist container = (DatabaseXmlPersist)deserializer.Deserialize(stream);

                    // Add databases in TreeView.
                    foreach (var db in container.Databases)
                        Container.TreeView.CreateTreeViewNode(db.Key, db.Value);
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

        #region Private Methods

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

    public class DatabaseXmlPersist : IXmlSerializable
    {
        // Key - database -> Value - database state in TreeView
        public Dictionary<IDatabase, bool> Databases { get; set; }

        public DatabaseXmlPersist()
            : this(new Dictionary<IDatabase, bool>())
        {
        }

        public DatabaseXmlPersist(Dictionary<IDatabase, bool> databases)
        {
            Databases = databases;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("DatabasePersist");
            reader.ReadStartElement("Databases");

            while (reader.IsStartElement("IDatabase"))
            {
                Type dbType = Type.GetType(reader.GetAttribute("AssemblyQualifiedName"));
                bool state = bool.Parse(reader.GetAttribute("CheckedState"));

                reader.ReadStartElement("IDatabase");

                XmlSerializer serial = new XmlSerializer(dbType);

                Databases.Add((IDatabase)serial.Deserialize(reader), state);

                reader.ReadEndElement(); // IDatabase.
            }

            reader.ReadEndElement(); // Databases.
            reader.ReadEndElement(); // DatabasePersist.
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.Flush();
            writer.WriteStartElement("Databases");

            foreach (var db in Databases)
            {
                // Get database type.
                var dbType = db.Key.GetType();

                // Create xml element and attributes.
                writer.WriteStartElement("IDatabase");
                writer.WriteAttributeString("AssemblyQualifiedName", dbType.AssemblyQualifiedName);
                writer.WriteAttributeString("CheckedState", db.Value.ToString());

                // Serialize database
                XmlSerializer serializer = new XmlSerializer(dbType);
                serializer.Serialize(writer, db.Key);

                // IDatabase.
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }

    public class AppSettings
    {
        public DockPanel DockingPanel { get; set; }
        public TreeViewFrame TreeView { get; set; }
        public Dictionary<string, StepFrame> Frames { get; set; }

        public AppSettings(DockPanel panel, TreeViewFrame treeView, Dictionary<string, StepFrame> frames)
        {
            DockingPanel = panel;
            TreeView = treeView;
            Frames = frames;
        }
    }
}