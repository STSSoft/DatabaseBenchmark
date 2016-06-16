//using Perst;
//using STS.General.Generators;
//using System;
//using DatabaseBenchmark.Core;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Xml.Serialization;
//using DatabaseBenchmark.Core.Attributes;

//namespace DatabaseBenchmark.Databases
//{
//    public class PerstDatabase : Core.Database
//    {
//        private Storage perst;
//        private Index index;
//        private string fileName;

//        public override string IndexingTechnology
//        {
//            get { return "BTree"; }
//        }

//        public PerstDatabase()
//        { 
//            Name = "Perst";
//            CollectionName = "table";
//            Category = "NoSQL\\Object Databases";
//            Description = "Perst 4.42";
//            Website = "http://www.mcobject.com/perst";
//            Color = Color.FromArgb(243, 184, 191);

//            Requirements = new String[]
//            { 
//                "PerstNet4.0.dll" 
//            }; 
//        }

//        public override void Open(int flowCount, long flowRecordCount)
//        {
//            fileName = Path.Combine(DataDirectory, string.Format("{0}.perst", CollectionName));

//            perst = StorageFactory.Instance.CreateStorage();
//            perst.SetProperty("perst.multiclient.support", true);
//            perst.Open(fileName);

//            index = perst.CreateIndex(typeof(long), false);
//        }

//        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
//        {
//            perst.BeginThreadTransaction(TransactionMode.ReadWrite);

//            foreach (var kv in flow)
//                index.Set(new Key(kv.Key), kv);

//            perst.Commit();
//            perst.EndThreadTransaction();
//        }

//        public override IEnumerable<KeyValuePair<long, Tick>> Read()
//        {
//            perst.BeginThreadTransaction(TransactionMode.ReadOnly);

//            foreach (var rec in index)
//                yield return (KeyValuePair<long, Tick>)rec;

//            perst.EndThreadTransaction();
//        }

//        public override void Close()
//        {
//            perst.Close();
//        }
//    }
//}