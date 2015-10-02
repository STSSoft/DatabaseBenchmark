using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows.Forms.DataVisualization.Charting;

namespace DatabaseBenchmark.Charts
{
    public partial class BarChartMono : UserControl
    {
        private double maxValue;

        private PlotModel chart;
        private ColumnSeries cache;

        private CategoryAxis XAxis;
        private LinearAxis YAxis;

        public BarChartMono()
        {
            InitializeComponent();

            plotView1.Dock = DockStyle.Fill;
            plotView1.Model = new PlotModel();

            chart = plotView1.Model;
            chart.IsLegendVisible = false;
            chart.Background = OxyColors.White;

            XAxis = new CategoryAxis();
            XAxis.MajorGridlineThickness = 0;
            XAxis.Position = AxisPosition.Bottom;

            YAxis = new LinearAxis();
            YAxis.MajorGridlineThickness = 0;
            YAxis.TextColor = OxyColors.Transparent;
            YAxis.TickStyle = OxyPlot.Axes.TickStyle.None;

            // Chart area.
            //chartArea.AxisX.MajorGrid.LineWidth = 0;
            //chartArea.AxisY.MajorGrid.LineWidth = 0;

            //chartArea.AxisX.IsLabelAutoFit = true;
            //chartArea.AxisX.Interval = 1;

            //chartArea.AxisY.LabelStyle.Enabled = false;
            //chartArea.AxisY.IsLabelAutoFit = true;

            chart.Axes.Add(XAxis);
            chart.Axes.Add(YAxis);
        }

        public void CreateSeries(string name, string labelFormat = null)
        {
            ColumnSeries series = new ColumnSeries();
            series.Title = name;

            series.LabelFormatString = labelFormat;
            series.LabelPlacement = LabelPlacement.Outside;

            chart.Series.Add(series);
            cache = series;
        }

        public void AddPoint(string label, object y, Color color)
        {
            //if (chart.Series.Count == 0)
            //    return;

            if (y.GetType() == typeof(double))
            {
                if (maxValue < (double)y)
                    maxValue = (double)y;

                chart.Axes[1].Maximum = maxValue * 1.3;
            }

            var col = OxyColor.FromRgb(color.R, color.G, color.B);
            ColumnItem dataItem = new ColumnItem();

            dataItem.Color = col;
            dataItem.Value = DateTimeAxis.ToDouble(y);

            cache.Items.Add(dataItem);

            XAxis.Labels.Add(label);
            plotView1.InvalidatePlot(true);
        }

        /// <summary>
        /// Adds a title to the chart while removing any previous ones.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get
            {
                return chart.Title;
            }

            set
            {
                chart.Title = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChartValueType AxisXValueType
        {
            get
            {
                return ChartValueType.Auto;
            }
            set
            {
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChartValueType AxisYValueType
        {
            get
            {
                return ChartValueType.Auto;
            }
            set
            {
            }
        }

        /// <summary>
        /// Clears all created series.
        /// </summary>
        public void Clear()
        {
            maxValue = 0;

            chart.Series.Clear();
            XAxis.Labels.Clear();
        }

        public byte[] ConvertToByteArray()
        {
            //using (var chartimage = new MemoryStream())
            //{
            //    chart.SaveImage(chartimage, ChartImageFormat.Png);

            //    return chartimage.GetBuffer();
            //}
            return null;
        }
    }
}
