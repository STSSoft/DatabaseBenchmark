using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using STS.General.Collections;
using STS.General.Comparers;
using STS.General.Extensions;
using STS.General.Generators;
using DatabaseBenchmark.Benchmarking;

namespace DatabaseBenchmark
{
    /// <summary>
    /// Represents a single database instance.
    /// This interface is implemented by all databases which participate in the benchmark.
	/// Test
    /// </summary>
    public interface IDatabase
    {
        #region Database Description

        /// <summary>
        /// Name of the database. DatabaseName must be a valid directory Name (see DataDirectory for details).
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// Name of the database collection (table, document and etc.).
        /// </summary>
        string DatabaseCollection { get; set; }

        /// <summary>
        /// Each database has it's own data directory. This is the place where the database stores its data. 
        /// This property is initialized with Application.StartupPath\Databases\IBenchmark.DatabaseName value.
        /// </summary>
        string DataDirectory { get; set; }

        /// <summary>
        /// The category of the database (SQL, NoSQL, NoSQL\\Key-Value and etc.)
        /// </summary>
        string Category { get; set; }

        /// <summary>
        /// A description of the database. Usually the name and specific version.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The official website of the database.
        /// </summary>
        string Website { get; set; }

        /// <summary>
        /// The color of the database used for the charts.
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Different requirements - for example used libraries, implementation details and etc.
        /// </summary>
        string[] Requirements { get; set; }

        /// <summary>
        /// A connection string if the database requires a remote connection.
        /// </summary>
        string ConnectionString { get; set; }

        #endregion

        #region Benchmark Methods

        /// <summary>
        /// Initialize and create the database - create configuration files, engines and etc.
        /// </summary>
        void Init(int flowCount, long flowRecordCount);

        /// <summary>
        /// Begin writing records into the database. Multiple threads invoke this method (one for each flow).
        /// </summary>
        void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow);

        /// <summary>
        /// Begin reading the records from the database in a separate thread. The tick flow must be returned in ascending by key order.
        /// </summary>
        IEnumerable<KeyValuePair<long, Tick>> Read();

        /// <summary>
        /// Close the database.
        /// </summary>
        void Finish();

        /// <summary>
        /// Returns the size of the database in bytes. 
        /// The size property is calculated by the total amount of bytes of all files contained in the working directory.
        /// In case that data files cannot be stored in DataDirectory, you should override the Size property (see MongoDB case)
        /// </summary>
        long Size { get; }

        #endregion
    }
}
