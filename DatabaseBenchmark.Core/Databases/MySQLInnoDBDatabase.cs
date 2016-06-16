using System;
using DatabaseBenchmark.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Core.Attributes;

namespace DatabaseBenchmark.Databases
{
    public class MySQLInnoDBDatabase : MySQLDatabase
    {
        public MySQLInnoDBDatabase()
            :base(MySQLStorageEngine.INNODB)
        {
        }

        public override string IndexingTechnology
        {
            get { return "BTree"; }
        }
    }
}
