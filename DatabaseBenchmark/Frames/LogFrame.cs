using System;
using System.Linq;
using System.Threading;
using log4net;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class LogFrame : DockContent
    {
        private StringAppender TestAppender;

        public LogFrame()
        {
            InitializeComponent();

            TestAppender = (StringAppender)LogManager.GetRepository().GetAppenders().First(appender => appender.Name.Equals("StringLoggerTest"));
            TestAppender.OnAppend += UpdateTextBox;
        }

        private void UpdateTextBox()
        {
            Action clear = textBoxTestLogs.Clear;
            Action<string> update = textBoxTestLogs.AppendText;

            Invoke(clear);
            Invoke(update, TestAppender.GetLogs());
        }
    }
}
