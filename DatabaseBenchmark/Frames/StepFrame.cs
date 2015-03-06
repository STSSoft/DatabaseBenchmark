using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms.DataVisualization.Charting;

namespace DatabaseBenchmark.Frames
{
    public partial class StepFrame : DockContent
    {
        private string text;

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

        public override string Text
        {
            get { return text; }
            set { text = value; }
        }
    }
}
