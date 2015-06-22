using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Databases
{
    public class MySQLArchiveDatabase : MySQLDatabase
    {
        public MySQLArchiveDatabase()
            : base(MySQLStorageEngine.ARCHIVE)
        {
        }

        public override IndexingTechnology IndexingTechnology
        {
            get
            {
                return IndexingTechnology.None;
            }
        }
    }
}
