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
