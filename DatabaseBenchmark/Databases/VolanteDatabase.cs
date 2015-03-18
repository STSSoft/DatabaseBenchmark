using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using Volante;

namespace DatabaseBenchmark.Databases
{
    public class VolanteDatabase : Database
    {
        private const int COMMIT_COUNT = 500000;
        private const int CACHE_SIZE_IN_BYTES = 2000 * 1024 * 1024; // 2GB

        private Volante.IDatabase database = DatabaseFactory.CreateDatabase();
        private IIndex<long, TickEntity> table;

        public VolanteDatabase()
        {
            SyncRoot = new object();

            DatabaseName = "Volante";
            DatabaseCollection = "table1";
            Category = "NoSQL\\Object Databases";
            Description = "Volante";
            Website = "https://github.com/kjk/volante";
            Color = System.Drawing.Color.FromArgb(195, 189, 243);

            Requirements = new string[]
            { 
                "Volante.dll"
            };
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            database.CodeGeneration = true;
            database.Open(Path.Combine(DataDirectory, "volante.dbs"), CACHE_SIZE_IN_BYTES);

            database.ObjectCacheInitSize = 4 * 1024;
            database.ExtensionQuantum = 16 * 1024 * 1024;
            database.ObjectCacheInitSize = 4 * 1319;

            table = database.CreateIndex<long, TickEntity>(IndexType.Unique);

            database.Commit();
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            int insertCount = 0;

            lock (SyncRoot)
            {
                foreach (var kv in flow)
                {
                    var tick = kv.Value;
                    TickEntity entity = new TickEntity(kv.Key, tick.Symbol, tick.Timestamp, tick.Bid, tick.Ask, tick.BidSize, tick.AskSize, tick.Provider);

                    table.Set(kv.Key, entity);
                    insertCount++;

                    if (insertCount == COMMIT_COUNT)
                    {
                        table.Store();
                        insertCount = 0;
                    }
                }

                table.Store();
                database.Commit();
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            foreach (var item in table)
                yield return new KeyValuePair<long, Tick>(item.ID, item.GetTick());
        }

        public override void Finish()
        {
            database.Close();
        }

        public override long Size
        {
            get { return database.DatabaseSize; }
        }

        public class TickEntity : Persistent
        {
            public long ID;

            public string Symbol { get; set; }
            public DateTime Timestamp { get; set; }
            public double Bid { get; set; }
            public double Ask { get; set; }
            public int BidSize { get; set; }
            public int AskSize { get; set; }
            public string Provider { get; set; }

            public TickEntity()
            {
            }

            public TickEntity(long id, string symbol, DateTime time, double bid, double ask, int bidSize, int askSize, string provider)
            {
                ID = id;

                Symbol = symbol;
                Timestamp = time;
                Bid = bid;
                Ask = ask;
                BidSize = bidSize;
                AskSize = askSize;
                Provider = provider;
            }

            public Tick GetTick()
            {
                return new Tick(Symbol, Timestamp, Bid, Ask, BidSize, AskSize, Provider);
            }
        }
    }
}
