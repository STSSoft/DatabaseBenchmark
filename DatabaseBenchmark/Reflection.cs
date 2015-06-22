using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBenchmark
{
    public static class ReflectionUtils
    {
        public static Database[] CreateDatabaseInstances(params Type[] types)
        {
            Database[] databases = new Database[types.Length];

            for (int i = 0; i < databases.Length; i++)
                databases[i] = (Database)Activator.CreateInstance(types[i]);

            return databases;
        }

        public static string GetPublicPropertyValues(object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            StringBuilder builder = new StringBuilder();

            foreach (var property in properties)
                builder.AppendLine(String.Format("{0}:{1}", property.Name, property.GetValue(obj)));

            return builder.ToString();
        }
    }
}
