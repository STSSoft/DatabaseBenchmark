using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace DatabaseBenchmark
{
    public class ConsoleOptions
    {
        // Configuration.
        [Option("createConfig", Separator='=', HelpText = "Generate default configuration file.")]
        public string CreateConfigFile { get; set; }

        [Option("configPath", Separator='=', HelpText = "The path to the configuration file.")]
        public string ConfigFilePath { get; set; }

        [Option("csvPath", HelpText = "The path to the CSV report file.")]
        public string CsvPath { get; set; }

        // Report.
        [Option("jsonPath", HelpText = "The path to the JSON report file.")]
        public string JsonPath { get; set; }

        [Option("report", HelpText = "Report type.")]
        public string ReportType { get; set; }

        public ConsoleOptions()
        {
        }
    }
}
