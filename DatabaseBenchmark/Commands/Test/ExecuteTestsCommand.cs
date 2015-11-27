using DatabaseBenchmark.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Commands
{
    /// <summary>
    /// The main command that handles the execution of the tests.
    /// </summary>
    public class ExecuteTestsCommand : TestCommand
    {
        public ExecuteTestsCommand(MainForm form, Database[] databases, ITest[] tests)
            : base(form, databases, tests)
        {
        }

        public override void Start()
        {
            // Prepare GUI.
            Form.Cancellation = new CancellationTokenSource();

            // Start the benchmark.
            Form.MainTask = Task.Factory.StartNew(DoBenchmark, Form.Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public override void Stop()
        {
            if (Form.MainTask == null)
                return;

            Form.Cancellation.Cancel();
        }

        private void DoBenchmark()
        {
            var benchmark = Form.CurrentBenchmark;

            try
            {
                benchmark.ExecuteTests(Form.Cancellation.Token, Tests);
            }
            catch (Exception exc)
            {
                Logger.Error("Test execution error...", exc);
            }
        }
    }
}
