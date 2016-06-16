using System;
using DatabaseBenchmark.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Core.Attributes;

namespace DatabaseBenchmark.Databases
{
    public class MySQLArchiveDatabase : MySQLDatabase
    {
        public MySQLArchiveDatabase()
            : base(MySQLStorageEngine.ARCHIVE)
        {
        }

        public override string IndexingTechnology
        {
            get { return "None"; }
        }
    }
}
