using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseBenchmark.Frames
{
    public partial class LogFrame : DockContent
    {
        private Task Worker;
        private CancellationTokenSource Cancellation;

        private StringAppender Appender;

        public LogFrame()
        {
            InitializeComponent();

            Cancellation = new CancellationTokenSource();

            var hierarchy = (Hierarchy)LogManager.GetRepository();
            Appender = (StringAppender)hierarchy.Root.GetAppender("BenchmarkTestLogger");
        }

        public void Start()
        {
            Worker = Task.Factory.StartNew(DoWork, Appender, Cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            if (Worker == null)
                return;

            Cancellation.Cancel();
            Worker = null;
        }

        private void DoWork(object state)
        {
            StringAppender appender = (StringAppender)state;

            while (true)
            {
                if (Cancellation.IsCancellationRequested)
                    break;

                textBox1.Text = appender.GetString();
            }
        }
    }
}
