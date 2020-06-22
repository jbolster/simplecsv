using BenchmarkDotNet.Running;

namespace SimpleCsv.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CsvReadTests>();
        }
    }
}