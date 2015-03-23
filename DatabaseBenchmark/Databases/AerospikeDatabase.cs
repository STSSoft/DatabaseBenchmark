using Aerospike.Client;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DatabaseBenchmark.Databases
{
    public class AerospikeDatabase : Database
    {
        private AerospikeClient client;
        private LargeList[] indexes;

        public AerospikeDatabase()
        {
            Name = "Aerospike";
            CollectionName = "table1";
            Category = @"NoSQL\Key-Value Store\RAM";
            Description = "Aerospike 3.4.1 .NET Client 3.0.13";
            Website = "http://www.aerospike.com/";
            Color = Color.DarkOrchid;

            Requirements = new string[] 
            { 
                "AerospikeClient.dll",
                "lua51.dll",
                "LuaInterface.dll"
            };

            ConnectionString = "Server:10.11.11.91;Port:3000;Namespace:test;Set:demoset;";
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            string[] str = ConnectionString.Split(';');
            string server = str[0].Split(':')[1];
            int port = Int32.Parse(str[1].Split(':')[1]);
            string ns = str[2].Split(':')[1];
            string set = str[3].Split(':')[1];

            client = new AerospikeClient(server, port);
            indexes = new LargeList[flowCount];

            Key listKey = new Key(ns, set, CollectionName);
            client.Delete(null, listKey);

            WritePolicy policy = new WritePolicy();
            policy.recordExistsAction = RecordExistsAction.REPLACE;

            for (int i = 0; i < flowCount; i++)
                indexes[i] = client.GetLargeList(policy, listKey, CollectionName, null);
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, STS.General.Generators.Tick>> flow)
        {
            LargeList index = indexes[flowID];

            foreach (var kv in flow)
                index.Update(Value.Get(kv.ToString()));
        }

        public override IEnumerable<KeyValuePair<long, STS.General.Generators.Tick>> Read()
        {
            LargeList index = indexes[0];

            foreach (string value in index.Scan())
            {
                string[] str = value.Remove(0, 1).Remove(value.Length - 2, 1).Split(','); // remove '[' ']'

                long key = long.Parse(str[0]);
                str = str[1].Split(';');

                string symbol = str[0];
                DateTime time = DateTime.Parse(str[1]);
                double bid = double.Parse(str[2]);
                double ask = double.Parse(str[3]);
                int bidSize = int.Parse(str[4]);
                int askSize = int.Parse(str[5]);
                string provider = str[6];

                Tick tick = new Tick(symbol, time, bid, ask, bidSize, askSize, provider);

                yield return new KeyValuePair<long, Tick>(key, tick);
            }
        }

        public override void Finish()
        {
            client.Close();
        }

        public override long Size 
        { 
            get 
            {
                return 0; 
            } 
        }
    }
}
