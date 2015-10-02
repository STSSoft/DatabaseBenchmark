using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseBenchmark.Charts
{
    public partial class LineChartMono : UserControl
    {
        private PlotModel chart;
        private LineSeries cache;

        private LinearAxis XAxis;
        private LinearAxis YAxis;
        private LogarithmicAxis YAxisLogarithmic;

        private LegendPosition lastPossition;
        private Dictionary<string, LineSeries> Series;

        private bool isLogarithmic;

        public LineChartMono()
        {
            InitializeComponent();

            plotView1.Dock = DockStyle.Fill;

            plotView1.Model = new PlotModel();
            plotView1.Model.PlotType = PlotType.XY;
            plotView1.Model.Background = OxyColors.Black;
            plotView1.Model.TextColor = OxyColors.White;
            plotView1.Model.PlotAreaBorderColor = OxyColors.DimGray;

            chart = plotView1.Model;

            // X Axis.
            XAxis = new LinearAxis();
            XAxis.MajorGridlineColor = OxyColors.DimGray;
            XAxis.MajorGridlineStyle = LineStyle.Solid;
            XAxis.MinorGridlineStyle = LineStyle.Dot;
            XAxis.Position = AxisPosition.Bottom;

            // Y axis.
            YAxis = new LinearAxis();
            YAxis.Key = "Linear";
            YAxis.MajorGridlineColor = OxyColors.DimGray;
            YAxis.MajorGridlineStyle = LineStyle.Solid;
            YAxis.MinorGridlineStyle = LineStyle.Dot;
            YAxis.Position = AxisPosition.Left;

            // Y axis logarithmic.
            YAxisLogarithmic = new LogarithmicAxis();
            YAxisLogarithmic.Key = "Linear";
            YAxisLogarithmic.IsAxisVisible = false;
            YAxisLogarithmic.MajorGridlineColor = OxyColors.DimGray;
            YAxisLogarithmic.MajorGridlineStyle = LineStyle.Solid;
            YAxisLogarithmic.MinorGridlineStyle = LineStyle.Dot;
            YAxisLogarithmic.Position = AxisPosition.Left;

            chart.Axes.Add(XAxis);
            chart.Axes.Add(YAxis);
            chart.Axes.Add(YAxisLogarithmic);

            chart.LegendPosition = LegendPosition.TopCenter;
            chart.LegendPlacement = LegendPlacement.Outside;

            Series = new Dictionary<string, LineSeries>();

            leftToolStripMenuItem.Tag = LegendPosition.LeftMiddle;
            rightToolStripMenuItem.Tag = LegendPosition.RightMiddle;
            topToolStripMenuItem.Tag = LegendPosition.TopCenter;
            bottomToolStripMenuItem.Tag = LegendPosition.BottomCenter;

            MoveLegend(topToolStripMenuItem, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a series in the chart with the specified name and color.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        public void CreateSeries(string name, Color color)
        {
            cache = new LineSeries();
            cache.Title = name;
            cache.Color = OxyColor.FromRgb(color.R, color.G, color.B);

            chart.Series.Add(cache);
            Series.Add(name, cache);
        }

        public void AddPoint(string series, long x, double y)
        {
            if (cache.Title != series)
                cache = GetSeriesByTitle(series);

            // If the scale is logarithmic, it cannot contain zeroes.
            if (y == 0)
                y = Double.NaN;

            cache.Points.Add(new OxyPlot.DataPoint(x, y));
            plotView1.InvalidatePlot(true);
        }

        /// <summary>
        /// Gets or sets a flag that indicates if the Y axis is logarithmic.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsLogarithmic
        {
            get { return isLogarithmic; }
            set
            {
                isLogarithmic = value;

                plotView1.InvalidatePlot(true);
            }
        }

        public string AxisXTitle
        {
            get { return XAxis.Title; }
            set { XAxis.Title = value; }
        }

        public string AxisYTitle
        {
            get { return YAxis.Title; }
            set { YAxis.Title = value; }
        }

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

        /// <summary>
        /// Returns the number of points for the specified series.
        /// </summary>
        public int GetPointsCount(string series)
        {
            return chart.Series.Count > 0 ? Series[series].Points.Count : 0;
        }

        /// <summary>
        /// Clears all created series.
        /// </summary>
        public void Clear()
        {
            chart.Series.Clear();
            Series.Clear();

            plotView1.InvalidatePlot(true);
            cache = null;
        }

        // TODO: Try the OxyPlot reporting method.
        public byte[] ConvertToByteArray()
        {
            return null;
        }

        public LegendPosition GetLegendPosition()
        {
            return chart.LegendPosition;
        }

        public void SetLegenedPosition(LegendPosition position)
        {
            foreach (ToolStripMenuItem menuItem in legendPossitionToolStripMenuItem.DropDownItems)
                menuItem.Checked = (LegendPosition)menuItem.Tag == position;

            switch (position)
            {
                case LegendPosition.LeftMiddle:
                    chart.LegendPosition = LegendPosition.LeftMiddle;

                    break;

                case LegendPosition.RightMiddle:
                    chart.LegendPosition = LegendPosition.RightMiddle;
                    break;

                case LegendPosition.TopCenter:
                    chart.LegendPosition = LegendPosition.TopCenter;
                    break;

                case LegendPosition.BottomCenter:
                    chart.LegendPosition = LegendPosition.BottomCenter;
                    break;
            }

            legendToolStripMenuItem.Visible = true;
            lastPossition = chart.LegendPosition;
        }

        private void legendToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            LegendVisible = legendToolStripMenuItem.Checked;
        }

        private void MoveLegend(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            SetLegenedPosition((LegendPosition)item.Tag);

            plotView1.InvalidatePlot(true);
        }

        private void logarithmicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsLogarithmic = !IsLogarithmic;
            logarithmicToolStripMenuItem.Checked = IsLogarithmic;
        }

        public bool LegendVisible
        {
            get;
            set;

            //get { return chart1.ChartAreas["ChartAreaLegend"].Visible; }

            //set
            //{
            //    chart1.ChartAreas["ChartAreaLegend"].Visible = value;

            //    if (value)
            //        chart1.ChartAreas["ChartAreaChart"].Position = lastPossition;
            //    else
            //        chart1.ChartAreas["ChartAreaChart"].Position = new ElementPosition(0, 0, 100, 100);
            //}
        }

        // TODO: ChartSettings.
        public ChartSettings Settings
        {
            get
            {
                //LegendPossition possition = (LegendPossition)chart1.ChartAreas["ChartAreaLegend"].Tag;

                //return new ChartSettings(Title, legendToolStripMenuItem.Checked, possition, IsLogarithmic);
                return null;
            }

            set
            {
                //if (value == null)
                //    return;

                //Title = value.Name;

                //switch (value.Possition)
                //{
                //    case LegendPosition.LeftMiddle:
                //        MoveLegend(leftToolStripMenuItem, EventArgs.Empty);
                //        break;

                //    case LegendPosition.RightMiddle:
                //        MoveLegend(rightToolStripMenuItem, EventArgs.Empty);
                //        break;

                //    case LegendPosition.TopCenter:
                //        MoveLegend(topToolStripMenuItem, EventArgs.Empty);
                //        break;

                //    case LegendPosition.BottomCenter:
                //        MoveLegend(bottomToolStripMenuItem, EventArgs.Empty);
                //        break;
                //}

                //legendToolStripMenuItem.Checked = value.ShowLegend;
                //IsLogarithmic = value.IsLogarithmic;
            }
        }

        private LineSeries GetSeriesByTitle(string title)
        {
            return Series[title];
        }
    }
}