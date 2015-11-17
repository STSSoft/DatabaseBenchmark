using DatabaseBenchmark.Core;
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
            var benchmark = Form.CurrentTest;
            benchmark.ExecuteTests(Form.Cancellation.Token, Tests);
            
            //TODO:
            //Fix this.
            //testSuite.OnTestMethodCompleted += Report;
            //testSuite.OnException += OnException;

            try
            {
                //foreach (var benchmark in History)
                //{
                //    if (Cancellation.IsCancellationRequested)
                //        break;

                    //Current = benchmark;
                    //testSuite.ExecuteInit(benchmark);

                    //// Write.
                    //MainLayout.SetCurrentMethod(TestMethod.Write);
                    //CurrentStatus = TestMethod.Write.ToString();

                    //testSuite.ExecuteWrite(benchmark);

                    //// Read.
                    //MainLayout.SetCurrentMethod(TestMethod.Read);
                    //CurrentStatus = TestMethod.Read.ToString();

                    //testSuite.ExecuteRead(benchmark);

                    //// Secondary Read.
                    //MainLayout.SetCurrentMethod(TestMethod.SecondaryRead);
                    //CurrentStatus = TestMethod.SecondaryRead.ToString();

                    //testSuite.ExecuteSecondaryRead(benchmark);

                    //// Finish.
                    //CurrentStatus = TestMethod.None.ToString();
                    //testSuite.ExecuteFinish(benchmark);
                //}
            }
            finally
            {
            }
        }
    }
}
