using DatabaseBenchmark.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DatabaseBenchmark.Report
{
    /// <summary>
    /// Sends the test data and computer configuration to the dedicated servers of Database Benchmark.
    /// </summary>
    public class ServerConnection
    {
        public string Host { get; set; }

        public ServerConnection()
        {
            Host = "http://reporting.stssoft.com/api/dbm/benchmarks/public/v1"; 
        }

        public HttpWebResponse SendDataAsPost(string jsonData)
        {
            // Remove control characters from string.
            string output = new String(jsonData.Where(character => !char.IsControl(character)).ToArray());

            string postData = "Data=" + output;
            byte[] rawdata = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Host);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = rawdata.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(rawdata, 0, rawdata.Length);
            }

            return (HttpWebResponse)request.GetResponse();
        }
    }
}
