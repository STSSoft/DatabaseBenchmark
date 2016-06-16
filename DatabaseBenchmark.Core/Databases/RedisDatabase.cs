//using ServiceStack.Redis;
//using ServiceStack.Redis.Generic;
//using STS.General.Generators;
//using System;
//using DatabaseBenchmark.Core;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Xml.Serialization;
//using DatabaseBenchmark.Core.Attributes;

//namespace DatabaseBenchmark.Databases
//{
//    public class RedisDatabase : Database
//    {
//        private RedisTypedClient<KeyValuePair<long, TickRecord>>[] clients;
//        private RedisSortedSet sortedSet;

//        [Category("Connection Settings")]
//        public string Server { get; set; }

//        [Category("Connection Settings")]
//        public int Port { get; set; }

//        public override string IndexingTechnology
//        {
//            get { return "HashTable"; }
//        }
//        public RedisDatabase()
//        {
//            Name = "Redis";
//            CollectionName = "0";
//            Category = @"NoSQL\Key-Value Store\RAM";
//            Description = "Redis 2.8.19 .Net Client 3.9.71";
//            Website = "http://redis.io/";
//            Color = Color.PeachPuff;

//            Requirements = new string[]
//            {
//                "ServiceStack.dll",
//                "ServiceStack.Common.dll",
//                "ServiceStack.Interfaces.dll",
//                "ServiceStack.OrmLite.dll",
//                "ServiceStack.OrmLite.SqlServer.dll",
//                "ServiceStack.Redis.dll",
//                "ServiceStack.ServiceInterface.dll",
//                "ServiceStack.Text.dll"
//            };

//            Server = "10.11.11.91";
//            Port = 6379;
//        }

//        public override void Open(int flowCount, long flowRecordCount)
//        {
//            clients = new RedisTypedClient<KeyValuePair<long, TickRecord>>[flowCount];
//            sortedSet = new RedisSortedSet();

//            for (int i = 0; i < flowCount; i++)
//                clients[i] = new RedisTypedClient<KeyValuePair<long, TickRecord>>(new RedisClient(Server, Port));

//            clients[0].FlushAll();// Remove all keys from all databases
//        }

//        public override void Write(int flowID, IEnumerable<KeyValuePair<long, STS.General.Generators.Tick>> flow)
//        {
//            RedisTypedClient<KeyValuePair<long, TickRecord>> client = clients[flowID];

//            foreach (var kv in flow)
//            {
//                var data = kv.Value;
//                TickRecord tick = new TickRecord(
//                    data.Symbol,
//                    TickRecord.DateTimeToUnixTimestamp(data.Timestamp),
//                    data.Bid, 
//                    data.Ask,
//                    data.BidSize,
//                    data.AskSize,
//                    data.Provider);

//                client.AddItemToSortedSet(sortedSet, new KeyValuePair<long, TickRecord>(kv.Key, tick));
//            }
//        }

//        public override IEnumerable<KeyValuePair<long, STS.General.Generators.Tick>> Read()
//        {
//            foreach (var kv in clients[0].GetAllItemsFromSortedSet(sortedSet))
//            {
//                TickRecord data = kv.Value;
//                Tick tick = new Tick(
//                    data.Symbol, 
//                    TickRecord.UnixTimestampToDateTime(data.UnixTime),
//                    data.Bid,
//                    data.Ask,
//                    data.BidSize,
//                    data.AskSize, 
//                    data.Provider);

//                yield return new KeyValuePair<long, Tick>(kv.Key, tick);
//            }
//        }

//        public override void Close()
//        {
//        }

//        public override long Size
//        {
//            get { return long.Parse(clients[0].NativeClient.Info["used_memory"]); }
//        }

//        public class TickRecord
//        {
//            public string Symbol { get; set; }
//            public double UnixTime { get; set; }
//            public double Bid { get; set; }
//            public double Ask { get; set; }
//            public int BidSize { get; set; }
//            public int AskSize { get; set; }
//            public string Provider { get; set; }

//            public TickRecord()
//            {
//            }

//            public TickRecord(string symbol, double time, double bid, double ask, int bidSize, int askSize, string provider)
//            {
//                Symbol = symbol;
//                UnixTime = time;
//                Bid = bid;
//                Ask = ask;
//                BidSize = bidSize;
//                AskSize = askSize;
//                Provider = provider;
//            }

//            public static double DateTimeToUnixTimestamp(DateTime dateTime)
//            {
//                DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//                long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;

//                return (double)unixTimeStampInTicks / TimeSpan.TicksPerSecond;
//            }

//            public static DateTime UnixTimestampToDateTime(double unixTime)
//            {
//                DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//                long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);

//                return new DateTime(unixStart.Ticks + unixTimeStampInTicks);
//            }
//        }

//        public class RedisSortedSet : IRedisSortedSet<KeyValuePair<long, TickRecord>>
//        {
//            # region Not implemented

//            public List<KeyValuePair<long, TickRecord>> GetAll()
//            {
//                throw new NotImplementedException();
//            }

//            public List<KeyValuePair<long, TickRecord>> GetAllDescending()
//            {
//                throw new NotImplementedException();
//            }

//            public double GetItemScore(KeyValuePair<long, TickRecord> item)
//            {
//                throw new NotImplementedException();
//            }

//            public List<KeyValuePair<long, TickRecord>> GetRange(int fromRank, int toRank)
//            {
//                throw new NotImplementedException();
//            }

//            public List<KeyValuePair<long, TickRecord>> GetRangeByHighestScore(double fromScore, double toScore, int? skip, int? take)
//            {
//                throw new NotImplementedException();
//            }

//            public List<KeyValuePair<long, TickRecord>> GetRangeByHighestScore(double fromScore, double toScore)
//            {
//                throw new NotImplementedException();
//            }

//            public List<KeyValuePair<long, TickRecord>> GetRangeByLowestScore(double fromScore, double toScore, int? skip, int? take)
//            {
//                throw new NotImplementedException();
//            }

//            public List<KeyValuePair<long, TickRecord>> GetRangeByLowestScore(double fromScore, double toScore)
//            {
//                throw new NotImplementedException();
//            }

//            public double IncrementItem(KeyValuePair<long, TickRecord> item, double incrementBy)
//            {
//                throw new NotImplementedException();
//            }

//            public int IndexOf(KeyValuePair<long, TickRecord> item)
//            {
//                throw new NotImplementedException();
//            }

//            public int IndexOfDescending(KeyValuePair<long, TickRecord> item)
//            {
//                throw new NotImplementedException();
//            }

//            public KeyValuePair<long, TickRecord> PopItemWithHighestScore()
//            {
//                throw new NotImplementedException();
//            }

//            public KeyValuePair<long, TickRecord> PopItemWithLowestScore()
//            {
//                throw new NotImplementedException();
//            }

//            public int PopulateWithIntersectOf(params IRedisSortedSet<KeyValuePair<long, TickRecord>>[] setIds)
//            {
//                throw new NotImplementedException();
//            }

//            public int PopulateWithUnionOf(params IRedisSortedSet<KeyValuePair<long, TickRecord>>[] setIds)
//            {
//                throw new NotImplementedException();
//            }

//            public int RemoveRange(int minRank, int maxRank)
//            {
//                throw new NotImplementedException();
//            }

//            public int RemoveRangeByScore(double fromScore, double toScore)
//            {
//                throw new NotImplementedException();
//            }

//            public void Add(KeyValuePair<long, TickRecord> item)
//            {
//                throw new NotImplementedException();
//            }

//            public void Clear()
//            {
//                throw new NotImplementedException();
//            }

//            public bool Contains(KeyValuePair<long, TickRecord> item)
//            {
//                throw new NotImplementedException();
//            }

//            public void CopyTo(KeyValuePair<long, TickRecord>[] array, int arrayIndex)
//            {
//                throw new NotImplementedException();
//            }

//            public int Count
//            {
//                get { throw new NotImplementedException(); }
//            }

//            public bool IsReadOnly
//            {
//                get { throw new NotImplementedException(); }
//            }

//            public bool Remove(KeyValuePair<long, TickRecord> item)
//            {
//                throw new NotImplementedException();
//            }

//            public IEnumerator<KeyValuePair<long, TickRecord>> GetEnumerator()
//            {
//                throw new NotImplementedException();
//            }

//            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//            {
//                throw new NotImplementedException();
//            }

//            #endregion

//            public string Id
//            {
//                get { return "Test"; }
//            }
//        }
//    }
//}
