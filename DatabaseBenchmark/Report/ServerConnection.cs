using DatabaseBenchmark.Report;
using System;
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
            Host = "http://presence.bg/bm/public/api/v1/benchmarks";
        }

        public HttpStatusCode SendData(string jsonData)
        {
            // Remove control characters from string.
            string output = new String(jsonData.Where(c => !char.IsControl(c)).ToArray());

            using (var client = new HttpClient())
            {
                string json = JsonUtils.ConvertJsonToPostQuery(jsonData).ToString();
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = client.PostAsync(Host, content).Result;

                return response.StatusCode;
            }
        }
    }
}
