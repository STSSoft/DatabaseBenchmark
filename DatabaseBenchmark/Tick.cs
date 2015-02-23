using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseBenchmark
{
    public class Tick
    {
        public string Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int BidSize { get; set; }
        public int AskSize { get; set; }
        public string Provider { get; set; }

        public Tick()
        {
        }

        public Tick(string symbol, DateTime time, double bid, double ask, int bidSize, int askSize, string provider)
        {
            Symbol = symbol;
            Timestamp = time;
            Bid = bid;
            Ask = ask;
            BidSize = bidSize;
            AskSize = askSize;
            Provider = provider;
        }

        public override string ToString()
        {
            return String.Format("{0};{1:yyyy-MM-dd HH:mm:ss};{2};{3};{4};{5};{6}", Symbol, Timestamp, Bid, Ask, BidSize, AskSize, Provider);
        }
    }
}
