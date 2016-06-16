using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Core
{
    public interface ITable<TKey, TRecord> : IEnumerable<KeyValuePair<TKey, TRecord>>
    {
        /// <summary>
        /// The name of the table.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The database implementation that holds the current table instance.
        /// </summary>
        IDatabase Database { get; }

        /// <summary>
        /// 
        /// </summary>
        void Write(IEnumerable<KeyValuePair<TKey, TRecord>> records);

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<KeyValuePair<TKey, TRecord>> Read(TKey from, TKey to);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<TKey, TRecord>> Read();

        /// <summary>
        /// 
        /// </summary>
        void Delete(TKey key);

        /// <summary>
        /// 
        /// </summary>
        void Delete(TKey from, TKey to);

        TRecord this[TKey key] { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        void Close();
    }
}
