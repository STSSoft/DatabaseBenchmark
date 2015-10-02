using OxyPlot;

namespace DatabaseBenchmark.Charts
{
    public class ChartSettings
    {
        public string Name { get; set; }
        public bool ShowLegend { get; set; }
        public LegendPossition Possition { get; set; }
        public bool IsLogarithmic { get; set; }

        public ChartSettings(string name, bool showLegend, LegendPossition possition, bool isLogarithmic)
        {
            Name = name;
            ShowLegend = showLegend;
            Possition = possition;
            IsLogarithmic = isLogarithmic;
        }
    }

    public enum LegendPossition
    {
        Top,
        Bottom,
        Left,
        Right
    }
}
