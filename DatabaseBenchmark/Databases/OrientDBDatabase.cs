using Orient.Client;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseBenchmark.Databases
{
    public class OrientDBDatabase : Database
    {
        private const int QUERY_CAPACITY = 100000;

        private ODatabase[] databases;
        private int flowCount;
        private long flowRecordCount;

        /// <summary>
        /// Specifies how many records are inserted with every batch.
        /// </summary>
        public int InsertsPerQuery { get; set; }

        public string Server { get; set; }
        public int Port { get; set; }
        public string DatabaseTest { get; set; }
        public string Ussername { get; set; }
        public string Password { get; set; }

        public OrientDBDatabase()
        {
            DatabaseName = "OrientDB";
            DatabaseCollection = "myTestDatabaseAlias";
            Category = "SQL";
            Description = "OrientDB 2.0 + OrientDB-NET.binary";
            Website = "http://www.orientechnologies.com/";
            Color = System.Drawing.Color.Orange;

            Requirements = new string[] 
            { 
                "Orient.Client.dll",
                "nunit.framework.dll", 
                "Orient.NUnit.dll",
                "Problem with more than 10 tasks, because of poolSize. "
            };

            InsertsPerQuery = 10000;
            Server = "127.0.0.1";
            Port = 2424;
            DatabaseTest = "test1";
            Ussername = "admin";
            Password = "admin";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            this.flowCount = flowCount;
            this.flowRecordCount = flowRecordCount;

            databases = new ODatabase[flowCount];
            var connect = OClient.CreateDatabasePool(Server, Port, DatabaseTest, ODatabaseType.Document, Ussername, Password, flowCount, DatabaseCollection);

            for (int i = 0; i < flowCount; i++)
            {
                ODatabase database = new ODatabase(DatabaseCollection);
                databases[i] = database;
            }

            databases[0].Command("DROP CLASS Tick");
            databases[0].Command("CREATE Class Tick");
            databases[0].Command("CREATE PROPERTY Tick.Key long");
            databases[0].Command("CREATE PROPERTY Tick.Symbol string");
            databases[0].Command("CREATE PROPERTY Tick.Timestamp DATETIME");
            databases[0].Command("CREATE PROPERTY Tick.Bid double");
            databases[0].Command("CREATE PROPERTY Tick.Ask double");
            databases[0].Command("CREATE PROPERTY Tick.BidSize integer");
            databases[0].Command("CREATE PROPERTY Tick.AskSize integer");
            databases[0].Command("CREATE PROPERTY Tick.Provider string");
            databases[0].Command("CREATE INDEX Tick.Key Dictionary");
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, STS.General.Generators.Tick>> flow)
        {
            StringBuilder batch = new StringBuilder();
            string SQLInsertQuery = "INSERT INTO Tick (Key,Symbol,Timestamp, Bid, Ask,BidSize, AskSize,Provider) VALUES ";

            batch.Append(SQLInsertQuery);

            int counter = 0;
            string query = string.Empty;

            foreach (var kv in flow)
            {
                query = string.Format("({0}, '{1}', '{2}', {3}, {4}, {5}, {6}, '{7}'),",
                    kv.Key,
                    kv.Value.Symbol,
                    kv.Value.Timestamp.ToString("yyyy-MM-dd hh:MM:ss"),
                    kv.Value.Bid,
                    kv.Value.Ask,
                    kv.Value.BidSize,
                    kv.Value.AskSize,
                    kv.Value.Provider);

                batch.Append(query);
                counter++;

                if (counter == InsertsPerQuery)
                {
                    batch.Remove(batch.Length - 1, 1);
                    databases[flowID].Command(batch.ToString());

                    batch.Clear();
                    counter = 0;

                    batch.Append(SQLInsertQuery);
                }
            }

            if (counter > 0)
            {
                batch.Remove(batch.Length - 1, 1);
                databases[flowID].Command(batch.ToString());
            }
        }

        public override IEnumerable<KeyValuePair<long, STS.General.Generators.Tick>> Read()
        {
            int skip = 0;
            List<ODocument> query = new List<ODocument>(QUERY_CAPACITY);

            while (skip < flowCount * flowRecordCount)
            {
                query.Clear();
                query = databases[0].Command(String.Format("SELECT FROM Tick ORDER BY Key ASC SKIP {0} LIMIT {1}", skip, QUERY_CAPACITY)).ToList();

                foreach (var record in query)
                {
                    long key = record.GetField<long>("Key");

                    Tick tick = new Tick(
                        record.GetField<string>("Symbol"),
                        record.GetField<DateTime>("Timestamp"),
                        record.GetField<double>("Bid"),
                        record.GetField<double>("Ask"),
                        record.GetField<int>("BidSize"),
                        record.GetField<int>("AskSize"),
                        record.GetField<string>("Provider"));

                    yield return new KeyValuePair<long, Tick>(key, tick);
                }
                skip += QUERY_CAPACITY;
            }
        }
        public override long Size
        {
            get
            {
                return databases[0].Size;
            }
        }

        public override void Finish()
        {
            OClient.DropDatabasePool(DatabaseCollection);

            foreach (var database in databases)
                database.Close();
        }
    }
}
