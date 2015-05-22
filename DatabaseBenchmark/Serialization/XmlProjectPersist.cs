using DatabaseBenchmark.Benchmarking;
using DatabaseBenchmark.Charts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Serialization
{
    public class XmlProjectPersist : IXmlSerializable
    {
        // Database -> Checked state in TreeView.
        public Dictionary<IDatabase, bool> Databases { get; private set; }

        // ComboBox name -> Selected value.
        public Dictionary<string, string> ComboBoxItems { get; private set; }

        // ToolStripButton name -> IsChecked.
        public Dictionary<string, bool> Buttons { get; private set; }

        public List<KeyValuePair<TestMethod, List<ChartSettings>>> ChartSettings { get; private set; }

        public int TrackBarValue { get; private set; }

        public XmlProjectPersist()
            : this(new Dictionary<IDatabase, bool>(), new Dictionary<string, string>(),new  Dictionary<string, bool>(), new List<KeyValuePair<TestMethod, List<ChartSettings>>>(), default(int))
        {
        }

        public XmlProjectPersist(Dictionary<IDatabase, bool> databases, Dictionary<string, string> comboBoxItems, Dictionary<string, bool> toolStripButtons, List<KeyValuePair<TestMethod, List<ChartSettings>>> chartSettings, int trackBarValue)
        {
            Databases = databases;
            ComboBoxItems = comboBoxItems;
            Buttons  = toolStripButtons;
            TrackBarValue = trackBarValue;
            ChartSettings = chartSettings;
        }

        public void WriteXml(XmlWriter writer)
        {
            // Serialize databases.
            writer.WriteStartElement("Databases");

            foreach (var database in Databases)
            {
                // Get database type.
                var databaseType = database.Key.GetType();

                // Create xml element and attributes.
                writer.WriteStartElement("IDatabase");
                writer.WriteAttributeString("AssemblyQualifiedName", databaseType.AssemblyQualifiedName);
                writer.WriteAttributeString("CheckedState", database.Value.ToString());

                // Serialize database.
                XmlSerializer serializer = new XmlSerializer(databaseType);
                serializer.Serialize(writer, database.Key);

                writer.WriteValue(ColorTranslator.ToHtml(database.Key.Color));

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

            //Serialize Buttons.
            writer.WriteStartElement("ToolStripButtons");

            foreach (var btn in Buttons)
            {   
                writer.WriteStartElement("Button");

                writer.WriteAttributeString("Name", btn.Key);
                writer.WriteAttributeString("Checked", btn.Value.ToString());

                writer.WriteEndElement(); // Button.
            }

            writer.WriteEndElement(); // Buttons.

            // Serialize TrackBar.
            writer.WriteStartElement("TrackBar");
            writer.WriteValue(TrackBarValue);

            writer.WriteEndElement(); // TrackBar.

            writer.WriteStartElement("StepFrameSettings");

            foreach (var item in ChartSettings)
            {
                writer.WriteStartElement("Frame");

                writer.WriteAttributeString("Name", item.Key.ToString());

                foreach (ChartSettings settings in item.Value)
                {
                    writer.WriteStartElement("Chart");

                    writer.WriteAttributeString("Name", settings.Name);
                    writer.WriteAttributeString("ShowLegend", settings.ShowLegend.ToString());
                    writer.WriteAttributeString("IsLogarithmic", settings.IsLogarithmic.ToString());
                    writer.WriteValue(settings.Possition.ToString());

                    writer.WriteEndElement(); // Chart.
                }

                writer.WriteEndElement(); // Frame.
            }

            writer.WriteEndElement(); // StepFrameSettings.
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("XmlProjectPersist");
            reader.ReadStartElement("Databases");

            // Deserialize databases.
            while (reader.IsStartElement("IDatabase"))
            {
                Type dbType = Type.GetType(reader.GetAttribute("AssemblyQualifiedName"));
                bool state = Boolean.Parse(reader.GetAttribute("CheckedState"));

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

            // Deserialize Buttons.
            reader.ReadStartElement("ToolStripButtons");

            while(reader.IsStartElement("Button"))
            {
                string name = reader.GetAttribute("Name");
                bool state = bool.Parse(reader.GetAttribute("Checked"));
  
                reader.ReadStartElement("Button");
                Buttons.Add(name, state);
            }

             reader.ReadEndElement(); // Buttons.

            reader.ReadStartElement("TrackBar");
            TrackBarValue = reader.ReadContentAsInt();

            reader.ReadEndElement(); // TrackBar.

            reader.ReadStartElement("StepFrameSettings");

            while (reader.IsStartElement("Frame"))
            {
                TestMethod method = (TestMethod)Enum.Parse(typeof(TestMethod), reader.GetAttribute("Name"));

                List<ChartSettings> chartSettings = new List<ChartSettings>();

                reader.ReadStartElement("Frame");

                while (reader.IsStartElement("Chart"))
                {
                    string chartName = reader.GetAttribute("Name");
                    bool showLegend = Boolean.Parse(reader.GetAttribute("ShowLegend"));
                    bool isLogarithmic = Boolean.Parse(reader.GetAttribute("IsLogarithmic"));

                    reader.ReadStartElement();
                    LegendPossition possition = (LegendPossition)Enum.Parse(typeof(LegendPossition), reader.ReadContentAsString());

                    chartSettings.Add(new Charts.ChartSettings(chartName, showLegend, possition, isLogarithmic));

                    reader.ReadEndElement(); // Chart.
                }

                reader.ReadEndElement(); // Frame.

                ChartSettings.Add(new KeyValuePair<TestMethod, List<ChartSettings>>(method, chartSettings));
            }

            reader.ReadEndElement(); // StepFrameSettings.
            reader.ReadEndElement(); // DatabaseXmlPersist.
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
