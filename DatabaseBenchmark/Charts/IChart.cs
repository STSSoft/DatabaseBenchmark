using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Charts
{
    public interface IChart
    {
        string Title { get; set; }
        ChartSettings Settings { get; set; }

        int Count { get; set; }
        bool IsLogarithmic { get; set; }

        void CreateSeries(string series, Color color);
        void AddPoint(string series, object x, object y);
        void Clear();

        byte[] ToByteArray();
    }
}
