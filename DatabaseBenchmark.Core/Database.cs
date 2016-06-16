using STS.General.Generators;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DatabaseBenchmark.Core
{
    public abstract class Database : IDatabase
    {
        protected object SyncRoot { get; set; }

        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string[] Requirements { get; set; }
        public string CollectionName { get; set; }
        public string DataDirectory { get; set; }
        public string ConnectionString { get; set; }

        [XmlIgnore]
        public Color Color { get; set; }

        public ITable<long, Tick>[] Tables { get; set; }

        public abstract void Open();
        public abstract ITable<long, Tick> OpenOrCreateTable(string name);
        public abstract void DeleteTable(string name);
        public abstract void Close();
        
        public virtual string IndexingTechnology
        {
            get { return "None"; }
        }

        [Browsable(false)]
        public virtual long Size
        {
            get { return Directory.GetFiles(DataDirectory, "*.*", SearchOption.AllDirectories).Sum(x => (new FileInfo(x)).Length); }
        }
    }
}
