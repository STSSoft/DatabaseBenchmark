using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases
{
    public class MySQLBlackholeDatabase : MySQLDatabase
    {
        public MySQLBlackholeDatabase()
            : base(MySQLStorageEngine.BLACKHOLE)
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
