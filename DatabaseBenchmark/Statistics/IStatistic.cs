
namespace DatabaseBenchmark.Statistics
{
    public interface IStatistic
    {
        int Step { get; set; }

        void Start();
        void Stop();
        void Add();
    }
}
