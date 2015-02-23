using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STS.General.Extensions;

namespace DatabaseBenchmark.Statistics
{
    public interface IStatistic
    {
        int Step { get; set; }

        void Start();
        void Stop();
        void Add();
    }
}
