using DatabaseBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark
{
    public class TestConfiguration
    {
        public int FlowCount { get; set; }
        public long RecordCount { get; set; }
        public float Randomness { get; set; }
        public string DataDirectory { get; set; }

        public List<Database> Databases { get; set; }

        public TestConfiguration(int flowCount, long recordCount, float randomness, string dataDirectory, params Database[] databases)
        {
            FlowCount = flowCount;
            RecordCount = recordCount;
            Randomness = randomness;

            Databases = new List<Database>(databases);
        }

        public TestConfiguration()
        {
            FlowCount = 0;
            RecordCount = 0;
            Randomness = 0.0f;

            Databases = new List<Database>();
        }
    }
}
