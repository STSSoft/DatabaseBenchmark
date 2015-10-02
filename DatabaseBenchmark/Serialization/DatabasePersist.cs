using DatabaseBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Serialization
{
    public class DatabasePersist : IXmlSerializable
    {
        // Database -> Checked state in TreeView.
        public Dictionary<IDatabase, bool> Databases { get; private set; }

        public DatabasePersist(Dictionary<IDatabase, bool> databases)
        {
            Databases = databases;
        }

        public DatabasePersist()
            : this(new Dictionary<IDatabase, bool>())
        {
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            // Deserialize databases.
            while (reader.IsStartElement("IDatabase"))
            {
                reader.ReadStartElement("IDatabase");

                reader.ReadStartElement("AssemblyQualifiedName");
                string typeName = reader.ReadContentAsString();
                reader.ReadEndElement();

                reader.ReadStartElement("CheckedState");
                string state = reader.ReadContentAsString();
                reader.ReadEndElement();

                Type databaseType = Type.GetType(typeName);

                //TODO: find new way 
                // when database type is deleted
                if (databaseType == null) 
                {
                    reader.Skip();
                    reader.Skip();

                    reader.ReadStartElement("Color");
                    reader.Skip();
                    reader.ReadEndElement();
                }
                else
                {
                    bool checkedState = Boolean.Parse(state);

                    XmlSerializer databaseDeserializer = new XmlSerializer(databaseType);
                    IDatabase database = (IDatabase)databaseDeserializer.Deserialize(reader);

                    reader.ReadStartElement("Color");
                    database.Color = ColorTranslator.FromHtml(reader.ReadContentAsString());
                    reader.ReadEndElement();

                    Databases.Add(database, checkedState);
                }

                reader.ReadEndElement(); // IDatabase.
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var database in Databases)
            {
                // Get database type.
                var databaseType = database.Key.GetType();

                // Create xml element and attributes.
                writer.WriteStartElement("IDatabase");

                writer.WriteStartElement("AssemblyQualifiedName");
                writer.WriteValue(databaseType.AssemblyQualifiedName);
                writer.WriteEndElement();

                writer.WriteStartElement("CheckedState");
                writer.WriteValue(database.Value.ToString());
                writer.WriteEndElement();

                // Serialize database.
                XmlSerializer serializer = new XmlSerializer(databaseType);
                serializer.Serialize(writer, database.Key);

                writer.WriteStartElement("Color");
                writer.WriteValue(ColorTranslator.ToHtml(database.Key.Color));
                writer.WriteEndElement();

                // IDatabase.
                writer.WriteEndElement();
            }
        }
    }
}