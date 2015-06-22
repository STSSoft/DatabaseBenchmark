using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases
{
    public class MySQLMyIsamDatabase : MySQLDatabase
    {
        public MySQLMyIsamDatabase()
            :base(MySQLStorageEngine.MyISAM)
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
