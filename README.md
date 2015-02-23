# Database Benchmark

Database Benchmark is a benchmark application designed to test different databases with heavy data flows. The application performs two main test scenarios:

- Insertion of large amount of randomly generated records with sequential or random keys;
- Read of the inserted records, ordered by their keys.

Database Benchmark is a stress test tool, it is not a complex performance measuring tool. It measures:

- Insert speed – the speed of insertion of all generated records (with sequential or random keys);
- Read speed – the speed of reading of all inserted records ordered by their key;
- Size – the size of the database after insert and read complete.

Every tested database must be capable of performing this simple test - insert the generated records and read them, ordered by their keys.
