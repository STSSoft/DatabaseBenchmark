using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases
{
    public class MySQLInnoDBDatabase : MySQLDatabase
    {
        public MySQLInnoDBDatabase()
            :base(MySQLStorageEngine.INNODB)
        {
        }

        public override IndexingTechnology IndexingTechnology
        {
            get
            {
                return IndexingTechnology.BTree;
            }
        }
    }
}
