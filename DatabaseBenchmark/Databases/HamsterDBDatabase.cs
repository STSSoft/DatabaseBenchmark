using Hamster;
using STS.General.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DatabaseBenchmark.Databases
{
    public class HamsterDBDatabase : Database
    {
        private const long CACHE_SIZE = 2000 * 1024 * 1024; // 2GB 

        private Hamster.Environment enviroment;
        private Hamster.Database database;

        public HamsterDBDatabase()
        {
            SyncRoot = new object();

            DatabaseName = "HamsterDB";
            DatabaseCollection = "database";
            Category = "NoSQL\\Key-Value Store";
            Description = "HamsterDB 2.1.9 + HamsterDb-dotnet.dll";
            Website = "http://hamsterdb.com/";
            Color = Color.FromArgb(119, 206, 27);

            Requirements = new string[] 
            { 
                "hamsterdb-2.1.9.dll", 
                "HamsterDb-dotnet.dll" 
            };
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            string path = Path.Combine(DataDirectory, String.Format("{0}..hamster", DatabaseCollection));

            enviroment = new Hamster.Environment();
            enviroment.Create(path, 0, 0664, new Parameter[] { GetCacheParam(CACHE_SIZE) });
            database = enviroment.CreateDatabase((short)1, 0, new Parameter[] { GetKeySizeParam(8) });
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            lock (SyncRoot)
            {
                foreach (var kv in flow)
                    database.Insert(Direct(kv.Key), FromTick(kv.Value), HamConst.HAM_OVERWRITE);
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            Cursor cursor = new Cursor(database);

            try
            {
                foreach (var item in cursor.Forward())
                    yield return new KeyValuePair<long, Tick>(Reverse(item.Key), ToTick(item.Value));
            }
            finally
            {
                cursor.Close();
            }
        }

        public override void Finish()
        {
            database.Close();
            enviroment.Close();
        }

        #region HelpMethods

        private byte[] Direct(Int64 key)
        {
            ulong val = (UInt64)(key + Int64.MaxValue + 1);
            var index = BitConverter.GetBytes(val);

            byte[] buf = new byte[8];
            buf[0] = index[7];
            buf[1] = index[6];
            buf[2] = index[5];
            buf[3] = index[4];
            buf[4] = index[3];
            buf[5] = index[2];
            buf[6] = index[1];
            buf[7] = index[0];

            return buf;
        }

        private Int64 Reverse(byte[] index)
        {
            byte[] buf = new byte[8];
            buf[0] = index[7];
            buf[1] = index[6];
            buf[2] = index[5];
            buf[3] = index[4];
            buf[4] = index[3];
            buf[5] = index[2];
            buf[6] = index[1];
            buf[7] = index[0];

            UInt64 val = BitConverter.ToUInt64(buf, 0);

            return (Int64)(val - (UInt64)Int64.MaxValue - 1);
        }

        private static Parameter GetCacheParam(long cacheSize)
        {
            Parameter param = new Parameter();
            param.name = HamConst.HAM_PARAM_CACHESIZE;
            param.value = cacheSize;

            return param;
        }

        private static Parameter GetKeySizeParam(long keySize)
        {
            Parameter param = new Parameter();
            param.name = HamConst.HAM_PARAM_KEYSIZE;
            param.value = keySize;

            return param;
        }

        private byte[] FromTick(Tick tick)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(tick.Symbol);
                writer.Write(tick.Timestamp.Ticks);
                writer.Write(tick.Bid);
                writer.Write(tick.Ask);
                writer.Write(tick.BidSize);
                writer.Write(tick.AskSize);
                writer.Write(tick.Provider);

                return stream.ToArray();
            }
        }

        private Tick ToTick(byte[] value)
        {
            Tick tick = new Tick();

            using (MemoryStream stream = new MemoryStream(value))
            {
                BinaryReader reader = new BinaryReader(stream);

                tick.Symbol = reader.ReadString();
                tick.Timestamp = new DateTime(reader.ReadInt64());
                tick.Bid = reader.ReadDouble();
                tick.Ask = reader.ReadDouble();
                tick.BidSize = reader.ReadInt32();
                tick.AskSize = reader.ReadInt32();
                tick.Provider = reader.ReadString();
            }

            return tick;
        }

        #endregion
    }

    public static class HamsterExtensions
    {
        public static IEnumerable<KeyValuePair<byte[], byte[]>> Forward(this Cursor cursor)
        {
            try
            {
                cursor.MoveFirst();
            }
            catch (DatabaseException)
            {
                yield break;
            }

            while (true)
            {
                byte[] key = cursor.GetKey();
                byte[] rec = cursor.GetRecord();

                yield return new KeyValuePair<byte[], byte[]>(key, rec);

                try
                {
                    cursor.MoveNext();
                }
                catch (DatabaseException)
                {
                    break;
                }
            }
        }

        public static IEnumerable<KeyValuePair<byte[], byte[]>> Backward(this Cursor cursor)
        {
            try
            {
                cursor.MoveLast();
            }
            catch (DatabaseException)
            {
                yield break;
            }

            while (true)
            {
                byte[] key = cursor.GetKey();
                byte[] rec = cursor.GetRecord();

                yield return new KeyValuePair<byte[], byte[]>(key, rec);

                try
                {
                    cursor.MovePrevious();
                }
                catch (DatabaseException)
                {
                    break;
                }
            }
        }
    }
}
