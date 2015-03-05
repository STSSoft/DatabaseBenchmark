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

namespace DatabaseBenchmark.Frames
{
    public partial class StepFrame : DockContent
    {
        private string text;

        public StepFrame()
        {
            InitializeComponent();
        }

        public void DrawAverageSpeed(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageSpeed.AddPoint(series, item.Key, item.Value);
        }

        public void DrawMomentSpeed(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartMomentSpeed.AddPoint(series, item.Key, item.Value);
        }

        public void DrawAverageCpuUsage(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageCPU.AddPoint(series, item.Key, item.Value);
        }

        public void DrawAverageMemoryUsage(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageMemory.AddPoint(series, item.Key, item.Value / (1024.0 * 1024.0));
        }

        public void DrawAverageIO(string series, IEnumerable<KeyValuePair<long, double>> data)
        {
            foreach (var item in data)
                lineChartAverageIO.AddPoint(series, item.Key, item.Value / (1024.0 * 1024.0));
        }

        public void ClearCharts()
        {
            lineChartAverageSpeed.Clear();
            lineChartMomentSpeed.Clear();
            lineChartAverageCPU.Clear();
            lineChartAverageMemory.Clear();
            lineChartAverageIO.Clear();

            barChartSpeed.Clear();
            barChartSize.Clear();
            barChartTime.Clear();
            barChartCPU.Clear();
            barChartMemory.Clear();
            barChartIO.Clear();
        }

        public override string Text
        {
            get { return text; }
            set { text = value; }
        }
    }
}
