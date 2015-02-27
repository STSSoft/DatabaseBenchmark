using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Serialization
{
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
}
