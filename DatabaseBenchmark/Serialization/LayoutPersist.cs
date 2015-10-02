using DatabaseBenchmark.Charts;
using DatabaseBenchmark.Core.Benchmarking;
using DatabaseBenchmark.Frames;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Serialization
{
    public class LayoutPersist : IXmlSerializable
    {
        private MainLayout Layout;

        public LayoutPersist(MainLayout layout)
        {
            Layout = layout;
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            // <TreeView Order>
            reader.ReadStartElement("TreeView");

            reader.ReadStartElement("TreeViewOrder");

            Layout.TreeView.treeViewOrder = (TreeViewOrder)Enum.Parse(typeof(TreeViewOrder), reader.ReadContentAsString());

            reader.ReadEndElement();

            reader.ReadEndElement();
            // <//TreeViewOrder>

            // <ComboBoxes>
            reader.ReadStartElement("ComboBoxes");

            int count = 0;
            while (reader.IsStartElement("ComboBox"))
            {
                // <ComboBox>
                reader.ReadStartElement("ComboBox");

                reader.ReadStartElement("Name");
                string name = reader.ReadContentAsString();
                reader.ReadEndElement();

                reader.ReadStartElement("Text");
                string value = reader.ReadContentAsString();
                reader.ReadEndElement();

                // Set combo boxes.
                Layout.ComboBoxes.First(x => x.Name == name).Text = value;

                reader.ReadEndElement();
                // </ComboBox>

                count++;
            }

            reader.ReadEndElement();
            // </ComboBoxes>

            // <ToolStripButtons>
            reader.ReadStartElement("ToolStripButtons");

            count = 0;
            while (reader.IsStartElement("Button"))
            {
                // <Button>
                reader.ReadStartElement("Button");

                reader.ReadStartElement("Name");
                string name = reader.ReadContentAsString();
                reader.ReadEndElement();

                reader.ReadStartElement("Checked");
                bool isChecked = Boolean.Parse(reader.ReadContentAsString());
                reader.ReadEndElement();

                // Set buttons.
                var currentButton = Layout.Buttons.First(x => x.Name == name);
                currentButton.Checked = !isChecked;
                currentButton.PerformClick();

                reader.ReadEndElement();
                // </Button>

                count++;
            }

            reader.ReadEndElement();
            // </ToolStripButtons>

            // <TrackBar>
            reader.ReadStartElement("TrackBar");
            Layout.TrackBar.Value = Int32.Parse(reader.ReadContentAsString());
            reader.ReadEndElement();
            // </TrackBar>

            // <StepFrameSettings>
            reader.ReadStartElement("StepFrameSettings");

            while (reader.IsStartElement("Frame"))
            {
                // <Frame>
                reader.ReadStartElement("Frame");

                reader.ReadStartElement("Name");
                string name = reader.ReadContentAsString();
                reader.ReadEndElement();

                TestMethod method = (TestMethod)Enum.Parse(typeof(TestMethod), name);
                List<ChartSettings> chartSettings = new List<ChartSettings>();

                while (reader.IsStartElement("Chart"))
                {
                    // <Chart>
                    reader.ReadStartElement("Chart");

                    string chartName = null;

                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement("Name");
                        chartName = reader.ReadContentAsString();
                    }

                    reader.Read();

                    reader.ReadStartElement("ShowLegend");
                    bool showLegend = Boolean.Parse(reader.ReadContentAsString());
                    reader.ReadEndElement();

                    reader.ReadStartElement("IsLogarithmic");
                    bool isLogarithmic = Boolean.Parse(reader.ReadContentAsString());
                    reader.ReadEndElement();

                    reader.ReadStartElement("Position");

                    string position = reader.ReadContentAsString();
                    LegendPossition possition = (LegendPossition)Enum.Parse(typeof(LegendPossition), position);

                    reader.ReadEndElement();

                    chartSettings.Add(new ChartSettings(chartName, showLegend, possition, isLogarithmic));

                    reader.ReadEndElement();
                    // </Chart>
                }

                reader.ReadEndElement();
                // </Frame>

                Layout.SelectFrame(method);

                Layout.StepFrames[method].SetSettings(chartSettings);
            }

            reader.ReadEndElement();
            // <StepFrameSettings>
        }

        public void WriteXml(XmlWriter writer)
        {
            List<KeyValuePair<TestMethod, List<ChartSettings>>> chartSettings = new List<KeyValuePair<TestMethod, List<ChartSettings>>>();

            foreach (var frame in Layout.StepFrames)
                chartSettings.Add(new KeyValuePair<TestMethod, List<ChartSettings>>(frame.Key, frame.Value.GetLineChartSettings()));

            // <TreeView>
            writer.WriteStartElement("TreeView");

            // <Order>
            writer.WriteStartElement("TreeViewOrder");
            writer.WriteValue(Layout.TreeView.treeViewOrder.ToString());
            writer.WriteEndElement();
            // </Order>

            writer.WriteEndElement();
            // </TreeViewOrder>

            // <ComboBoxes>
            writer.WriteStartElement("ComboBoxes");

            foreach (var item in Layout.ComboBoxes)
            {
                // <ComboBox>
                writer.WriteStartElement("ComboBox");

                writer.WriteStartElement("Name");
                writer.WriteValue(item.Name);
                writer.WriteEndElement();

                writer.WriteStartElement("Text");
                writer.WriteValue(item.Text);
                writer.WriteEndElement();

                writer.WriteEndElement();
                // </ComboBox>
            }

            writer.WriteEndElement();
            // <//ComboBoxes>

            // <ToolStripButtons>
            writer.WriteStartElement("ToolStripButtons");

            foreach (var button in Layout.Buttons)
            {
                // <Button>
                writer.WriteStartElement("Button");

                writer.WriteStartElement("Name");
                writer.WriteValue(button.Name);
                writer.WriteEndElement();

                writer.WriteStartElement("Checked");
                writer.WriteValue(button.Checked.ToString());
                writer.WriteEndElement();

                writer.WriteEndElement();
                // </Button>
            }

            writer.WriteEndElement();
            // </ToolStripButtons>

            // <TrackBar>
            writer.WriteStartElement("TrackBar");
            writer.WriteValue(Layout.TrackBar.Value);
            writer.WriteEndElement();
            // </TrackBar>

            // <StepFrameSettings>
            writer.WriteStartElement("StepFrameSettings");

            foreach (var item in chartSettings)
            {
                // <Frame>
                writer.WriteStartElement("Frame");

                writer.WriteStartElement("Name");
                writer.WriteValue(item.Key.ToString());
                writer.WriteEndElement();

                foreach (ChartSettings settings in item.Value)
                {
                    // <Chart>
                    writer.WriteStartElement("Chart");

                    writer.WriteStartElement("Name");
                    writer.WriteValue(settings.Name);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ShowLegend");
                    writer.WriteValue(settings.ShowLegend.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("IsLogarithmic");
                    writer.WriteValue(settings.IsLogarithmic.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("Position");
                    writer.WriteValue(settings.Possition.ToString());
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    // </Chart>
                }

                writer.WriteEndElement();
                // </Frame>
            }

            writer.WriteEndElement();
            // </StepFrameSettings>
        }
    }
}
