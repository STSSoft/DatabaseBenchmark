using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseBenchmark.Benchmarking
{
    public enum TestMethod
    {
        None = -1,
        Write = 0,
        Read = 1,
        SecondaryRead = 2
    }
}
