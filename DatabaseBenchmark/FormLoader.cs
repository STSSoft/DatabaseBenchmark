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
        private Thread Worker;

        public FormLoader()
        {
        }

        public void Run(Form form)
        {
            if (Worker != null)
                return;

            Worker = new Thread(new ThreadStart(() => { Application.Run(form); }));
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
            catch { }
            finally
            {
                Worker = null;
            }
        }
    }
}
