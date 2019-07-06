# SimpleCsv

```
    /// <summary>
    /// Parse a CSV stream
    /// </summary>
    /// <param name="stream">The stream to read</param>
    /// <param name="onReadAction">After each record, the action to call with a string array for the row</param>
    public static void Parse(Stream stream, Action<string[]> onReadAction);
```

Example:
```
    CsvParse.Parse(stream, record => Console.WriteLine(string.Join(",", record)));
```

# InfiniteCsvStream

```

    /// <summary>
    /// Creates a continuous stream of CSV records up to a given number
    /// </summary>
    /// <param name="headers">An object array for the first row</param>
    /// <param name="recordGenerator">A function to return an object array for the current record i</param>
    /// <param name="maxSize">The maximum number of rows to generate (default: 1,000,000)</param>
    /// <param name="encoding">The text encoding (default: UTF8)</param>
    InfiniteCsvStream(object[] headers, Func<int,object[]) => recordGenerator, int maxSize, Encoding encoding)
```

Example:
```
    using (var stream = new InfiniteCsvStream(
        headers: new[] {"headerA", "headerB", "headerC", "headerD"},
        recordGenerator: (index) => new string[]
        {
            Guid.NewGuid().ToString(), index.ToString(), null, index % 5  == 0 ? "hello,there" : "something"
        },
        maxSize: 1000000))
    {
        // Use stream
    }
```