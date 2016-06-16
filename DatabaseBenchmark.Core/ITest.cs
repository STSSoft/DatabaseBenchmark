using DatabaseBenchmark.Core.Reporting;
using System.Collections.Generic;
using System.Threading;

namespace DatabaseBenchmark.Core
{
    public interface ITest
    {
        /// <summary>
        /// Represents a database implementation.
        /// </summary>
        IDatabase Database { get; set; }

        /// <summary>
        /// The name of the current test. For example: Write or Random Read.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A small description what the test actually does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The current status of the test. For example: Executing write and etc.
        /// </summary>
        string Status { get; }

        /// <summary>
        /// A set of reports.
        /// </summary>
        List<PerformanceWatch> Reports { get; }

        /// <summary>
        /// The currently active report.
        /// </summary>
        PerformanceWatch ActiveReport { get; }

        /// <summary>
        /// Starts the test.
        /// </summary>
        /// <param name="token">Token to be observed for cancellation request.</param>
        void Start(CancellationToken token);
    }
}
