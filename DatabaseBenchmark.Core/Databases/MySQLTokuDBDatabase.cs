using System;
using DatabaseBenchmark.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Core.Attributes;

namespace DatabaseBenchmark.Databases
{
    public class MySQLTokuDBDatabase : MySQLDatabase
    {
        public MySQLTokuDBDatabase()
            :base(MySQLStorageEngine.TokuDB)
        {
        }

        public override string IndexingTechnology
        {
            get { return "FractalTree"; }
        }

    }
}
