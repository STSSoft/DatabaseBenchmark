# Database Benchmark

Database Benchmark is a benchmark application designed to test different databases with heavy data flows. The application performs two main test scenarios:

- Insertion of large amount of randomly generated records with sequential or random keys;
- Read of the inserted records, ordered by their keys.

Database Benchmark is a stress test tool, it is not a complex performance measuring tool. It measures:

- Insert speed – the speed of insertion of all generated records (with sequential or random keys);
- Read speed – the speed of reading of all inserted records ordered by their key;
- Size – the size of the database after insert and read complete.

Every tested database must be capable of performing this simple test - insert the generated records and read them, ordered by their keys.

# Included DBMS products
All of the databases included in the benchmark are implemented by our team. We are doing our best to make sure that the implementations are the best possible. If you can propose a better implementation or if you want to make a change, we encourage you to do it, following the Contributing guide: https://github.com/STSSoft/DatabaseBenchmark/wiki/Contributing guide.

The currently included databases are:

* Access
* Aerospike
* BrightstarDB
* CassandraDB
* Couchbase
* Db4objects
* Firebird
* HamsterDB
* LevelDB
* MongoDB
* MS SQL Server
* MS SQL Server Compact
* MySQL
* Oracle BerkeleyDB
* OrientDB
* Perst
* PostgreSQL
* RavenDB
* Redis
* ScimoreDB
* SQLite
* STSdb 4.0
* STSdb 3.5
* TokuMX
* VelocityDB
* Volante

# Features
- Advanced Data Generators - special algorithms that provide close to real-life data streams.
- Graphic Vsualization and Export - chart visualization and export of the test results.
- Easy to Use - Intuitive and easy to use interface.

# Use case
This tool can be used as additional viewpoint when the research engineers or software architects assesses the appropriate background storage engine for their mission critical systems.


