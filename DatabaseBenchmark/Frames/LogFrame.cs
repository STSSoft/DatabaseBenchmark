using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using DatabaseBenchmark.Properties;
using log4net;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class LogFrame : DockContent
    {
        private ILog Logger;
        private StringAppender TestAppender;

        public LogFrame()
        {
            InitializeComponent();

            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);

            TestAppender = (StringAppender)LogManager.GetRepository().GetAppenders().First(appender => appender.Name.Equals("StringLoggerTest"));
            TestAppender.OnAppend += UpdateTextBox;
        }

        private void UpdateTextBox()
        {
            Action<string> addLine = AddLine;

            try
            {
                Invoke(addLine, TestAppender.LastLine);
            }
            catch(Exception exc)
            {
                Logger.Error("LogFrame error...", exc);
            }
        }

        private void AddLine(string line)
        {
            int startIndex = richTextBoxLogs.TextLength;
            richTextBoxLogs.AppendText(line);

            // Highlight error lines.
            if (line.Contains("ERROR"))
            {
                richTextBoxLogs.Select(startIndex, line.Length);
                richTextBoxLogs.SelectionBackColor = Color.Red;
            }
        }
    }
}
