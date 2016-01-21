using DatabaseBenchmark.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands.Test
{
    /// <summary>
    /// The main command that handles the execution of the tests.
    /// </summary>
    public class TestExecutionCommand : TestCommand
    {
        public TestExecutionCommand(MainForm form, Database[] databases, ITest[] tests)
            : base(form, databases, tests)
        {
        }

        public override void Start()
        {
            // Prepare GUI.
            Form.Cancellation = new CancellationTokenSource();

            // Start the benchmark.
            Form.MainTask = Task.Factory.StartNew(DoBenchmark, Form.Cancellation, Form.Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public override void Stop()
        {
            if (Form.MainTask == null)
                return;

            Form.Cancellation.Cancel();
            Task.WaitAll(new Task[] { Form.MainTask }, 500);
        }

        private void DoBenchmark(object state)
        {
            var token = ((CancellationTokenSource)state).Token;

            token.ThrowIfCancellationRequested();

            var benchmark = Form.History[0];

            try
            {
                Form.CurrentBenchmark = benchmark;
                benchmark.ExecuteTests(token, Tests);
            }
            catch (Exception exc)
            {
                Logger.Error("Test execution error...", exc);
            }
        }
    }
}
