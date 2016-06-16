using STS.General.Generators;
using System.Collections.Generic;
using System.Drawing;

namespace DatabaseBenchmark.Core
{
    /// <summary>
    /// Represents a single database instance.
    /// This interface is implemented by all databases which participate in the benchmark.
    /// </summary>
    public interface IDatabase
    {
        #region Database Description

        /// <summary>
        /// Name of the database. It must be a valid directory Name (see DataDirectory for details).
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The category of the database (SQL, NoSQL, NoSQL\\Key-Value and etc.)
        /// </summary>
        string Category { get; set; }

        /// <summary>
        /// The indexing technology of the database (B-tree, LSM Tree etc.)
        /// </summary>
        string IndexingTechnology { get; }

        /// <summary>
        /// A description of the database. Usually the name and version.
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
        /// Name of the database collection (table, document and etc.).
        /// </summary>
        string CollectionName { get; set; }

        /// <summary>
        /// Each database has it's own data directory. This is the place where the database stores its data. 
        /// This property is initialized with Application.StartupPath\Databases\IDatabase.Name value.
        /// </summary>
        string DataDirectory { get; set; }

        /// <summary>
        /// A connection string if the database requires a remote connection.
        /// </summary>
        string ConnectionString { get; set; }

        #endregion

        ITable<long, Tick>[] Tables { get; }

        #region Database Methods

        /// <summary>
        /// Initialize and create the database - create configuration files, engines and etc.
        /// </summary>
        void Open();

        ITable<long, Tick> OpenOrCreateTable(string name);

        void DeleteTable(string name);

        /// <summary>
        /// Close the database.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns the size of the database in bytes. 
        /// The size property is calculated by the total amount of bytes of all files contained in the working directory.
        /// In case that data files cannot be stored in DataDirectory, you should override the Size property (see MongoDB case)
        /// </summary>
        long Size { get; }

        #endregion
    }
}
