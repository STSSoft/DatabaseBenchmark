using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases
{
    public class MySQLTokuDBDatabase : MySQLDatabase
    {
        public MySQLTokuDBDatabase()
            :base(MySQLStorageEngine.TokuDB)
        {
        }

        public override IndexingTechnology IndexingTechnology
        {
            get
            {
                return IndexingTechnology.FractalTree;
            }
        }
    }
}
