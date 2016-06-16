using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using DatabaseBenchmark.Core.Reporting;

namespace DatabaseBenchmark.Core
{
    public abstract class Test : ITest
    {
        public IDatabase Database { get; set; }

        public virtual string Name { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual string Status { get; protected set; }

        [Browsable(false)]
        public List<PerformanceWatch> Reports { get; protected set; }

        [Browsable(false)]
        public PerformanceWatch ActiveReport { get; protected set; }

        public abstract void Start(CancellationToken cancellationToken);
    }
}
