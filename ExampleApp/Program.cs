using System;
using System.IO;
using SimpleCsv;
using Microsoft.VisualBasic.FileIO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var stream = new InfiniteCsvStream(
                headers: new[] {"headerA", "headerB", "headerC", "headerD"},
                recordGenerator: (index) => new string[]
                {
                    Guid.NewGuid().ToString(), index.ToString(), null, index % 5  == 0 ? "hello,there" : "something"
                },
                maxSize: 10))
            {
                CsvParse.Parse(stream, record => Console.WriteLine(string.Join(",", record)));
            }
        }
    }
}