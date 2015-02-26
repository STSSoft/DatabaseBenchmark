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

    public class XmlAppSettingsPersist : IXmlSerializable
    {
        // Key - database -> Value - database state in TreeView
        public Dictionary<IDatabase, bool> Databases { get; set; }

        // Key - ComboBox name -> Value - selected value
        public Dictionary<string, string> ComboBoxItems { get; set; }
        public int TrackBarValue { get; set; }

        public XmlAppSettingsPersist()
            : this(new Dictionary<IDatabase, bool>(), new Dictionary<string, string>(), default(int))
        {
        }

        public XmlAppSettingsPersist(Dictionary<IDatabase, bool> databases, Dictionary<string, string> comboBoxItems, int trackBarValue)
        {
            Databases = databases;
            ComboBoxItems = comboBoxItems;
            TrackBarValue = trackBarValue;
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("XmlAppSettingsPersist");
            reader.ReadStartElement("Databases");

            // Deserialize databases.
            while (reader.IsStartElement("IDatabase"))
            {
                Type dbType = Type.GetType(reader.GetAttribute("AssemblyQualifiedName"));
                bool state = bool.Parse(reader.GetAttribute("CheckedState"));

                reader.ReadStartElement("IDatabase");

                XmlSerializer serial = new XmlSerializer(dbType);

                IDatabase db = (IDatabase)serial.Deserialize(reader);
                db.Color = ColorTranslator.FromHtml(reader.ReadContentAsString());

                Databases.Add(db, state);

                reader.ReadEndElement(); // IDatabase.
            }

            reader.ReadEndElement(); // Databases.

            // Deserialize ComboBoxes.
            reader.ReadStartElement("ComboBoxes");

            while (reader.IsStartElement("ComboBox"))
            {
                string name = reader.GetAttribute("Name");

                reader.ReadStartElement("ComboBox");
                ComboBoxItems.Add(name, reader.ReadContentAsString());

                reader.ReadEndElement(); // ComboBox.
            }

            reader.ReadEndElement(); // ComboBoxes.

            reader.ReadStartElement("TrackBar");
            TrackBarValue = reader.ReadContentAsInt();

            reader.ReadEndElement(); // TrackBar;
            reader.ReadEndElement(); // DatabaseXmlPersist.
        }

        public void WriteXml(XmlWriter writer)
        {
            // Serialize Databases.
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

                writer.WriteValue(ColorTranslator.ToHtml(db.Key.Color));

                // IDatabase.
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            // Serialize ComboBoxes.
            writer.WriteStartElement("ComboBoxes");

            foreach (var item in ComboBoxItems)
            {
                writer.WriteStartElement("ComboBox");

                writer.WriteAttributeString("Name", item.Key);
                writer.WriteValue(item.Value);

                writer.WriteEndElement(); // ComboBox.
            }

            writer.WriteEndElement(); // ComboBoxes.

            // Serialize TrackBar.
            writer.WriteStartElement("TrackBar");
            writer.WriteValue(TrackBarValue);

            writer.WriteEndElement(); // TrackBar.
        }
    }

    public class AppSettings
    {
        public DockPanel DockingPanel { get; set; }
        public TreeViewFrame TreeView { get; set; }
        public Dictionary<string, StepFrame> Frames { get; set; }
        public ToolStripComboBox[] ComboBoxes { get; set; }
        public TrackBar TrackBar { get; set; }

        public AppSettings(DockPanel panel, TreeViewFrame treeView, Dictionary<string, StepFrame> frames, ToolStripComboBox[] comboBoxes, TrackBar trackBar)
        {
            DockingPanel = panel;
            TreeView = treeView;
            Frames = frames;
            ComboBoxes = comboBoxes;
            TrackBar = trackBar;
        }

        public Dictionary<string, string> GetComboBoxSelectedItems()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var cb in ComboBoxes)
                result.Add(cb.Name, cb.Text);

            return result;
        }
    }
}