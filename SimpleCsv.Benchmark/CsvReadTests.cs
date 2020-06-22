using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CsvHelper;
using Microsoft.VisualBasic.FileIO;

namespace SimpleCsv.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net472)]
    [MarkdownExporterAttribute.GitHub]
    [MemoryDiagnoser]
    public class CsvReadTests
    {
        private Stream _stream;

        [Params(1000, 10000, 1000000)]
        public int N;

        [IterationSetup]
        public void IterationSetup()
        {
            _stream = new InfiniteCsvStream(
                headers: new[] {"headerA", "headerB", "headerC", "headerD"},
                recordGenerator: (index) => new string[]
                {
                    "0000-0000-0000-0000-0000", index.ToString(), null, "another string"
                },
                maxSize: N);
        }
        
        [Benchmark]
        public bool CsvHelper()
        {
            using (var reader = new StreamReader(_stream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                }
            }
            
            return true;
        }
        
        [Benchmark]
        public bool NoParsing()
        {
            using (var reader = new StreamReader(_stream, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    reader.Read();
                }
            }

            return true;
        }

        [Benchmark]
        public bool SimpleCsv() => CsvParse.Parse(_stream, _ => { });
        
        [Benchmark(Baseline = true)]
        public bool TextFieldParse()
        {
            using (var reader = new StreamReader(_stream, Encoding.UTF8))
            using (var parser = new TextFieldParser(reader))
            {
                parser.Delimiters = new string[] { "," };
                while (true)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null)
                    {
                        break;
                    }
                }
            }
            
            return true;
        }
    }
}