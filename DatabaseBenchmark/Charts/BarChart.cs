using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using System.IO;

namespace DatabaseBenchmark.Charts
{
    public partial class BarChart : UserControl
    {
        private double maxValue;
        private ChartArea chartArea;

        public BarChart()
        {
            InitializeComponent();

            chartArea = chart1.ChartAreas[0];

            // Chart area.
            chartArea.AxisX.MajorGrid.LineWidth = 0;
            chartArea.AxisY.MajorGrid.LineWidth = 0;

            chartArea.AxisX.IsLabelAutoFit = true;
            chartArea.AxisX.Interval = 1;

            chartArea.AxisY.LabelStyle.Enabled = false;
            chartArea.AxisY.IsLabelAutoFit = true;
        }

        public void CreateSeries(string name, string labelFormat = null)
        {
            Series series = chart1.Series.Add(name);
            series.ChartType = SeriesChartType.Column;

            series.IsVisibleInLegend = false;

            series.BorderWidth = 1;
            series.IsValueShownAsLabel = true;
            
            series.LabelFormat = labelFormat;
        }

        public void AddPoint(string label, object y, Color color)
        {
            if (y.GetType() == typeof(double))
            {
                if (maxValue < (double)y)
                    maxValue = (double)y;

                // Prevent axis from hiding the label.
                chartArea.AxisY.Maximum = maxValue > 10 ? 1.3 * maxValue : 3.7 * maxValue;
            }

            DataPoint point = new DataPoint();

            point.IsValueShownAsLabel = true;

            point.SetValueXY(label, y);
            point.Color = color;

            // Prevent multiple add of an existing point.
            if (chart1.Series[0].Points.Contains(point))
                return;

            chart1.Series[0].Points.Add(point);
        }

        /// <summary>
        /// Adds a title to the chart while removing any previous ones.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get
            {
                if (chart1.Titles.Count > 0)
                    return chart1.Titles[0].Text;

                return string.Empty;
            }

            set
            {
                chart1.Titles.Clear();

                string name = value;

                Title title = chart1.Titles.Add(name);
                title.Text = name;
                title.Font = new Font(title.Font.FontFamily, 16, FontStyle.Bold);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChartValueType AxisXValueType
        {
            get { return chart1.Series[0].XValueType; }
            set { chart1.Series[0].XValueType = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChartValueType AxisYValueType
        {
            get { return chart1.Series[0].YValueType; }
            set { chart1.Series[0].YValueType = value; }
        }

        /// <summary>
        /// Clears all created series.
        /// </summary>
        public void Clear()
        {
            maxValue = 0;
            chart1.Series.Clear();
        }

        public byte[] ConvertToByteArray()
        {
            using (var chartimage = new MemoryStream())
            {
                chart1.SaveImage(chartimage, ChartImageFormat.Png);

                return chartimage.GetBuffer();
            }
        }
    }
}
