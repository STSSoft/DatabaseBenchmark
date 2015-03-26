using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DatabaseBenchmark.Charts
{
    public partial class LineChart : UserControl
    {
        private ChartArea chartArea;
        private Series cache;
        private ElementPosition lastPossition;

        public LineChart()
        {
            InitializeComponent();

            chartArea = chart1.ChartAreas[0];

            // Chart area
            chartArea.BackColor = Color.Black;

            // Axes
            chartArea.AxisX.MajorGrid.LineColor = Color.DimGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.DimGray;

            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;


            leftToolStripMenuItem.Tag = LegendPossition.Left;
            rightToolStripMenuItem.Tag = LegendPossition.Right;
            topToolStripMenuItem.Tag = LegendPossition.Top;
            bottomToolStripMenuItem.Tag = LegendPossition.Bottom;

            MoveLegend(topToolStripMenuItem, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a series in the chart with the specified name and color.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        public void CreateSeries(string name, Color color)
        {
            cache = chart1.Series.Add(name);

            cache.Color = color;
            cache.BorderWidth = 2;
            cache.ChartType = SeriesChartType.Line;

            Legend legend = new Legend(name);
            legend.Position = chart1.ChartAreas["ChartAreaLegend"].Position;
            legend.Position.Auto = false;
            legend.DockedToChartArea = "ChartAreaLegend";
            legend.Name = name;

            chart1.Legends.Add(legend);
        }

        public void AddPoint(string series, long x, double y)
        {
            if (cache.Name != series)
                cache = chart1.Series.FindByName(series);

            // If you switch to logarithmic scale and the chart contains 0's, it will throw an exception.
            if (y == 0)
                y = double.NaN;

            cache.Points.AddXY(x, y);
        }

        /// <summary>
        /// Gets or sets a flag that indicates if the Y axis is logarithmic.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsLogarithmic
        {
            get { return chartArea.AxisY.IsLogarithmic; }
            set { chartArea.AxisY.IsLogarithmic = value; }
        }

        public string AxisXTitle
        {
            get { return chartArea.AxisX.Title; }
            set { chartArea.AxisX.Title = value; }
        }

        public string AxisYTitle
        {
            get { return chartArea.AxisY.Title; }
            set { chartArea.AxisY.Title = value; }
        }

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
                title.Visible = true;
            }
        }

        /// <summary>
        /// Returns the number of points for the specified series.
        /// </summary>
        public int GetPointsCount(string series)
        {
            return chart1.Series.Count > 0 ? chart1.Series[series].Points.Count : 0;
        }

        /// <summary>
        /// Clears all created series.
        /// </summary>
        public void Clear()
        {
            chart1.Series.Clear();
            chart1.Legends.Clear();
            cache = null;
        }

        public byte[] ConvertToByteArray()
        {
            using (var chartImage = new MemoryStream())
            {
                chart1.SaveImage(chartImage, ChartImageFormat.Bmp);

                return chartImage.GetBuffer();
            }
        }

        public LegendPossition GetLegendPosition()
        {
            return (LegendPossition)chart1.ChartAreas["ChartAreaLegend"].Tag;
        }

        public void SetLegenedPosition(LegendPossition position)
        {
            foreach (ToolStripMenuItem menuItem in legendPossitionToolStripMenuItem.DropDownItems)
                menuItem.Checked = (LegendPossition)menuItem.Tag == position;

            ChartArea legendArea = chart1.ChartAreas["ChartAreaLegend"];

            switch (position)
            {
                case LegendPossition.Left:
                    legendArea.Position = new ElementPosition(0, 0, 9, 100);
                    legendArea.Tag = LegendPossition.Left;
                    chartArea.Position = new ElementPosition(10, 0, 90, 100);

                    break;

                case LegendPossition.Right:
                    legendArea.Position = new ElementPosition(91, 0, 9, 100);
                    legendArea.Tag = LegendPossition.Right;
                    chartArea.Position = new ElementPosition(0, 0, 90, 100);

                    break;

                case LegendPossition.Top:
                    legendArea.Position = new ElementPosition(0, 0, 100, 9);
                    legendArea.Tag = LegendPossition.Top;
                    chartArea.Position = new ElementPosition(0, 10, 100, 90);

                    break;

                case LegendPossition.Bottom:
                    legendArea.Position = new ElementPosition(0, 91, 100, 9);
                    legendArea.Tag = LegendPossition.Bottom;
                    chartArea.Position = new ElementPosition(0, 0, 100, 90);

                    break;
            }

            foreach (Legend legend in chart1.Legends)
                legend.Position = legendArea.Position;

            legendToolStripMenuItem.Visible = true;
            lastPossition = chartArea.Position;
        }

        private void legendToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            LegendVisible = legendToolStripMenuItem.Checked;
        }

        private void MoveLegend(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            SetLegenedPosition((LegendPossition)item.Tag);
        }

        private void logarithmicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsLogarithmic = !IsLogarithmic;
            logarithmicToolStripMenuItem.Checked = IsLogarithmic;
        }

        public bool LegendVisible
        {
            get { return chart1.ChartAreas["ChartAreaLegend"].Visible; }

            set
            {
                chart1.ChartAreas["ChartAreaLegend"].Visible = value;

                if (value)
                    chart1.ChartAreas["ChartAreaChart"].Position = lastPossition;
                else
                    chart1.ChartAreas["ChartAreaChart"].Position = new ElementPosition(0, 0, 100, 100);
            }
        }

        public ChartSettings Settings
        {
            get
            {
                LegendPossition possition = (LegendPossition)chart1.ChartAreas["ChartAreaLegend"].Tag;

                return new ChartSettings(Title, legendToolStripMenuItem.Checked, possition, IsLogarithmic);
            }

            set
            {
                if (value == null)
                    return;

                Title = value.Name;

                switch (value.Possition)
                {
                    case LegendPossition.Left:
                        MoveLegend(leftToolStripMenuItem, EventArgs.Empty);
                        break;

                    case LegendPossition.Right:
                        MoveLegend(rightToolStripMenuItem, EventArgs.Empty);
                        break;

                    case LegendPossition.Top:
                        MoveLegend(topToolStripMenuItem, EventArgs.Empty);
                        break;

                    case LegendPossition.Bottom:
                        MoveLegend(bottomToolStripMenuItem, EventArgs.Empty);
                        break;
                }

                legendToolStripMenuItem.Checked = value.ShowLegend;
                IsLogarithmic = value.IsLogarithmic;
            }
        }
    }
}
