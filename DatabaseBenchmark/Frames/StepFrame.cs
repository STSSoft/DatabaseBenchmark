using DatabaseBenchmark.Charts;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class StepFrame : DockContent
    {
        public StepFrame()
        {
            InitializeComponent();
        }

        public void InitializeCharts(IEnumerable<KeyValuePair<string, Color>> lineSeries)
        {
            // Bar charts.
            barChartSpeed.CreateSeries("Series1", "{#,#}");
            barChartSize.CreateSeries("Series1", "{0:0.#}");
            barChartTime.CreateSeries("Series1", "HH:mm:ss");
            barChartTime.AxisYValueType = ChartValueType.DateTime;

            barChartCPU.CreateSeries("Series1", "{0:0.#}");
            barChartMemory.CreateSeries("Series1", "{0:0.#}");
            barChartIO.CreateSeries("Series1", "{0:0.#}");

            barChartSpeed.Title = "Speed (rec/sec)";
            barChartSize.Title = "Size (MB)";
            barChartTime.Title = "Time (hh:mm:ss)";
            barChartCPU.Title = "CPU usage (%)";
            barChartMemory.Title = "Memory usage (MB)";
            barChartIO.Title = "IO Data (MB/sec)";

            // Line charts.
            lineChartAverageSpeed.AxisXTitle = "Records";
            lineChartAverageSpeed.AxisYTitle = "Records/Sec";
            lineChartMomentSpeed.AxisXTitle = "Records";
            lineChartMomentSpeed.AxisYTitle = "Records/Sec";
            lineChartAverageCPU.AxisXTitle = "Records";
            lineChartAverageCPU.AxisYTitle = "Percent (%)";
            lineChartAverageMemory.AxisXTitle = "Records";
            lineChartAverageMemory.AxisYTitle = "MB";
            lineChartAverageIO.AxisXTitle = "Records";
            lineChartAverageIO.AxisYTitle = "MB/Sec";

            foreach (var item in lineSeries)
            {
                lineChartAverageSpeed.CreateSeries(item.Key, item.Value);
                lineChartMomentSpeed.CreateSeries(item.Key, item.Value);
                lineChartAverageCPU.CreateSeries(item.Key, item.Value);
                lineChartAverageMemory.CreateSeries(item.Key, item.Value);
                lineChartAverageIO.CreateSeries(item.Key, item.Value);
            }
        }

        public void ClearCharts()
        {
            lineChartAverageSpeed.Clear();
            lineChartMomentSpeed.Clear();
            lineChartAverageCPU.Clear();
            lineChartAverageMemory.Clear();
            lineChartAverageIO.Clear();

            barChartSpeed.Clear();
            barChartTime.Clear();
            barChartSize.Clear();
            barChartCPU.Clear();
            barChartMemory.Clear();
            barChartIO.Clear();
        }

        public void SetLogarithmic(bool isLogarithmic)
        {
            lineChartAverageSpeed.IsLogarithmic = isLogarithmic;
            lineChartMomentSpeed.IsLogarithmic = isLogarithmic;
            lineChartAverageCPU.IsLogarithmic = isLogarithmic;
            lineChartAverageMemory.IsLogarithmic = isLogarithmic;
            lineChartAverageIO.IsLogarithmic = isLogarithmic;
        }

        #region Add points to LineChart

        public void AddAverageSpeed(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageSpeed.AddPoint(series, item.Key, item.Value);
        }

        public void AddMomentSpeed(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartMomentSpeed.AddPoint(series, item.Key, item.Value);
        }

        public void AddAverageCpuUsage(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageCPU.AddPoint(series, item.Key, item.Value);
        }

        public void AddAverageMemoryUsage(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageMemory.AddPoint(series, item.Key, item.Value / (1024.0 * 1024.0));
        }

        public void AddAverageIO(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageIO.AddPoint(series, item.Key, item.Value / (1024.0 * 1024.0));
        }

        #endregion

        #region Add points to BarChart

        public void AddAverageSpeedToBar(string label, object y, Color color)
        {
            barChartSpeed.AddPoint(label, y, color);
        }

        public void AddSizeToBar(string label, object y, Color color)
        {
            barChartSize.AddPoint(label, y, color);
        }

        public void AddTimeToBar(string label, object y, Color color)
        {
            barChartTime.AddPoint(label, y, color);
        }

        public void AddCpuUsageToBar(string label, object y, Color color)
        {
            barChartCPU.AddPoint(label, y, color);
        }

        public void AddMemoryUsageToBar(string label, object y, Color color)
        {
            barChartMemory.AddPoint(label, y, color);
        }

        public void AddIoUsageToBar(string label, object y, Color color)
        {
            barChartIO.AddPoint(label, y, color);
        }

        #endregion

        public List<BarChart> GetSelectedBarCharts()
        {
            List<BarChart> allBarCharts = new List<BarChart>();

            foreach (Control item in LayoutPanel.Controls)
            {
                if (LayoutPanel.ColumnStyles[LayoutPanel.GetColumn(item)].SizeType == SizeType.Percent)
                    allBarCharts.Add(item as BarChart);
            }

            return allBarCharts;
        }

        public List<BarChart> GetAllBarCharts()
        {
            List<BarChart> barCharts = new List<BarChart>();

            foreach (Control item in LayoutPanel.Controls)
                barCharts.Add(item as BarChart);

            return barCharts;
        }
    }
}
