using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DatabaseBenchmark.Benchmarking;
using System.Xml.Serialization;

namespace DatabaseBenchmark
{
    public abstract class Database : IDatabase
    {
        protected object SyncRoot { get; set; }

        public string DatabaseName { get; set; }
        public string DatabaseCollection { get; set;}
        public string DataDirectory { get; set; }
        public string ConnectionString { get; set; }

        public string Category { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }

        [XmlIgnore]
        public Color Color { get; set; }
        public string[] Requirements { get; set; }

        public abstract void Init(int flowCount, long flowRecordCount);
        public abstract void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow);
        public abstract IEnumerable<KeyValuePair<long, Tick>> Read();
        public abstract void Finish();

        [Browsable(false)]
        public virtual long Size
        {
            get { return Directory.GetFiles(DataDirectory, "*.*", SearchOption.AllDirectories).Sum(x => (new FileInfo(x)).Length); }
        }
    }
}
