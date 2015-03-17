using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DatabaseBenchmark.Validation
{
    /// <summary>
    /// Sends the test data and computer configuration to the dedicated servers of DatabaseBenchmark.
    /// </summary>
    public class ServerConnection
    {
        public string Host { get; set; }

        public ServerConnection()
        {
            Host = "http://stssoft.com/draft2014/benchmarks/public/api/v1/benchmarks";
        }

        public HttpStatusCode SendData(string jsonData)
        {
            return ExecutePostQuery(jsonData);
        }

        private HttpStatusCode ExecutePostQuery(string jsonData)
        {
            // Remove control characters from string.
            string output = new String(jsonData.Where(c => !char.IsControl(c)).ToArray());

            using (var client = new HttpClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("Data", output));

                var jsonString = JsonConvert.SerializeObject(values);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = client.PostAsync(Host, content).Result;
                Uri uri = response.Content.Headers.ContentLocation;

                return response.StatusCode;
            }
        }
    }
}
