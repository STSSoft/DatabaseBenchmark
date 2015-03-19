using System;
using System.Linq;
using System.Threading;
using log4net;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class LogFrame : DockContent
    {
        System.Threading.Timer timer;

        private StringAppender ApplicationAppender;
        private StringAppender TestAppender;

        public LogFrame()
        {
            InitializeComponent();

            TestAppender = (StringAppender)LogManager.GetRepository().GetAppenders().First(appender => appender.Name.Equals("StringLoggerTest"));
            ApplicationAppender = (StringAppender)LogManager.GetRepository().GetAppenders().First(appender => appender.Name.Equals("StringLoggerApplication"));
            
            timer = new System.Threading.Timer(DoWork, null, Timeout.Infinite, 1000);
        }

        public void Start()
        {
            timer.Change(0, 1000);
        }

        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void DoWork(object state)
        {
            Action update = UpdateTextBox;

            try
            {
                Invoke(update);
            }
            catch(Exception exc) { }
        }

        private void UpdateTextBox()
        {
            textBoxApplicationLogs.Clear();
            textBoxTestLogs.Clear();

            textBoxApplicationLogs.AppendText(ApplicationAppender.GetLogs());
            textBoxTestLogs.AppendText(TestAppender.GetLogs());
        }
    }
}
