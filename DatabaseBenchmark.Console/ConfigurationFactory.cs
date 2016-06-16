using DatabaseBenchmark.Properties;
using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DatabaseBenchmark
{
    public static class ConfigurationFactory
    {
        private static ILog Logger = LogManager.GetLogger(Settings.Default.Logger);

        public static void CreateConfigFile(string filePath)
        {
            XDocument document = new XDocument();
            document.Declaration = new XDeclaration("1.0", "utf-8", "true");

            XElement configuration = new XElement("Configuration");

            // <TestParameters>
            XElement testParams = new XElement("TestParameters");
            testParams.Add
                (
                    new XElement("FlowCount", "1"),
                    new XElement("RecordsCount", "1000000"),
                    new XElement("Randomness", "1.0"),
                    new XElement("DataDirectory", @"C:\DatabaseBenchmark\")
                );

            configuration.Add(testParams);

            // <Databases>
            XElement databases = new XElement("Databases");

            foreach (var db in ReflectionUtils.CreateDatabaseInstances())
            {
                XElement database = new XElement("Database");
                database.Add(new XAttribute("Name", db.GetType().AssemblyQualifiedName));

                database.Add(new XElement("Enabled", 1));
                database.Add(new XElement("DataDirectory", String.Empty));

                foreach (var property in ReflectionUtils.GetPublicProperties(db.GetType()))
                {
                    var name = property.Name;
                    var value = property.GetValue(db);

                    database.Add(new XElement(name, value));
                }

                databases.Add(database);
            }

            configuration.Add(databases);
            document.Add(configuration);

            var stream = new FileStream(filePath, FileMode.Create);
            document.Save(stream);
            stream.Close();
        }

        public static TestConfiguration ParseConfigFile(Stream stream)
        {
            try
            {
                TestConfiguration configuration = new TestConfiguration();
                XDocument document = XDocument.Load(stream);

                var flowCount = document.Element("Configuration").Element("TestParameters").Element("FlowCount").Value;
                var recordsCount = document.Element("Configuration").Element("TestParameters").Element("RecordsCount").Value;
                var randomness = document.Element("Configuration").Element("TestParameters").Element("Randomness").Value;
                var dataDirectory = document.Element("Configuration").Element("TestParameters").Element("DataDirectory").Value;

                configuration.FlowCount = Int32.Parse(flowCount);
                configuration.RecordCount = Int64.Parse(recordsCount);
                configuration.Randomness = Single.Parse(randomness);
                configuration.DataDirectory = dataDirectory;

                var databases = document.Element("Configuration").Element("Databases").Elements("Database").Where(x => x.Element("Enabled").Value == "1");

                foreach (var database in databases)
                {
                    var stringType = database.Attribute("Name").Value;
                    var databaseType = Type.GetType(stringType);

                    var databaseInstance = ReflectionUtils.CreateDatabaseInstance(databaseType);

                    foreach (var property in typeof(Database).GetProperties().Where(x => x.SetMethod != null))
                    {
                        var stringValue = database.Element(property.Name).Value;
                        var value = ParseValue(stringValue, property.PropertyType);

                        if(property.SetMethod != null)
                            property.SetValue(databaseInstance, value);
                    }

                    configuration.Databases.Add(databaseInstance);
                }

                return configuration;
            }
            catch (Exception exc)
            {
                Logger.Error("Parsing error...", exc);
            }

            return null;
        }

        public static object ParseValue(string value, Type type)
        {
            object parseValue = null;

            if (type == typeof(int))
                parseValue = Int32.Parse(value);
            else if (type == typeof(uint))
                parseValue = UInt32.Parse(value);
            else if (type == typeof(long))
                parseValue = Int64.Parse(value);
            else if (type == typeof(float))
                parseValue = Single.Parse(value);
            else if (type == typeof(double))
                parseValue = Double.Parse(value);
            else if (type == typeof(string))
                return value;

            return parseValue;
        }
    }
}
