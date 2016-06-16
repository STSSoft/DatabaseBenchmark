using System;
using DatabaseBenchmark.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseBenchmark.Core.Attributes;

namespace DatabaseBenchmark.Databases
{
    public class MySQLMemoryDatabase : MySQLDatabase
    {
        public MySQLMemoryDatabase()
            : base(MySQLStorageEngine.MEMORY)
        {
        }

        protected override string CreateTableQuery()
        {
            return String.Format("CREATE TABLE `{0}` (", CollectionName) +
                       "`ID` bigint(20), INDEX USING BTREE (ID), NOT NULL," +
                       "`Symbol` varchar(255) NOT NULL," +
                       "`Time` datetime NOT NULL," +
                       "`Bid` double NOT NULL," +
                       "`Ask` double NOT NULL," +
                       "`BidSize` int(20) NOT NULL," +
                       "`AskSize` int(20) NOT NULL," +
                       "`Provider` varchar(255) NOT NULL," +
                     String.Format(") ENGINE={0};", StorageEngine);
        }

        public override string IndexingTechnology
        {
            get { return "BTree"; }
        }
    }
}
