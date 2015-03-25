using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseBenchmark.Properties;
using log4net;

namespace DatabaseBenchmark
{
    public class FormLoader
    {
        private ILog Logger;
        private Thread Worker;

        public Form UserForm { get; private set; }

        public FormLoader(Form form)
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
            UserForm = form;
        }

        public void Run()
        {
            if (Worker != null)
                return;

            Worker = new Thread(new ThreadStart(() => { Application.Run(UserForm); }));
            Worker.Start();
        }

        public void Stop()
        {
            if (Worker == null)
                return;

            try
            {
                Worker.Abort();
            }
            catch (Exception exc)
            {
                Logger.Error("FormLoader Stop()...", exc);
            }
            finally
            {
                Worker = null;
            }
        }
    }
}
