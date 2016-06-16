using DatabaseBenchmark.Core;
using DatabaseBenchmark.Core.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark.Core.Utils
{
    // TODO: Clean up this mess.
    public static class ReflectionUtils
    {
        private static ILog Logger;

        static ReflectionUtils()
        {
            Logger = LogManager.GetLogger(Settings.Default.ApplicationLogger);
        }

        public static Database[] CreateDatabaseInstances()
        {
            Type[] databaseTypes = GetDatabaseTypes();
            Database[] databases = new Database[databaseTypes.Length];

            for (int i = 0; i < databases.Length; i++)
                databases[i] = CreateDatabaseInstance(databaseTypes[i]);

            return databases;
        }

        public static Database CreateDatabaseInstance(Type databaseType)
        {
            Database database = null;

            try
            {
                database = (Database)Activator.CreateInstance(databaseType);
            }
            catch (Exception exc)
            {
                Logger.Error("Database create instance error...", exc);
            }

            return database;
        }

        public static Type[] GetDatabaseTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Database)) && t.GetConstructor(new Type[] { }) != null).ToArray();
        }

        public static void ChangePropertyValue(string name, int value, Database database)
        {
            var property = database.GetType().GetProperty(name);
            property.SetValue(database, value);
        }

        public static List<PropertyInfo> GetPublicProperties(Type type)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if(property.GetCustomAttribute(typeof(BrowsableAttribute)) == null)
                    result.Add(property);
            }

            return result;
        }

        public static void ChangePropertyValues(Database source, Database target)
        {
            foreach (var item in GetPublicProperties(source))
                ChangePropertyValue(item.Name, (int)item.GetValue(source), target);
        }

        public static List<PropertyInfo> GetPublicProperties(Database database)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            foreach (var property in database.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double))
                    result.Add(property);
            }
            return result;
        }

        public static List<Tuple<string, object>> GetPublicPropertiesAndValues(Database database)
        {
            List<Tuple<string, object>> result = new List<Tuple<string, object>>();

            foreach (var property in database.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double))
                    result.Add(new Tuple<string, object>(property.Name, property.GetValue(database)));
            }

            return result;
        }

        public static ITest[] GetTests()
        {
            var testTypes = GetTestTypes();
            ITest[] tests = new ITest[testTypes.Length];

            for (int i = 0; i < tests.Length; i++)
                tests[i] = (ITest)Activator.CreateInstance(testTypes[i]);

            return tests;
        }

        public static Type[] GetTestTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterface("ITest") == typeof(ITest) && t.GetConstructor(new Type[] { }) != null).ToArray();
        }
    }
}
