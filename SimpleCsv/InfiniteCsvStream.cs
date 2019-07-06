using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleCsv
{
    public class InfiniteCsvStream : Stream
    {
        private readonly int _maxSize;
        private readonly Func<int, object[]> _recordGenerator;
        private readonly Encoding _encoding;
        
        // Contains any remainder of the last CSV item that was
        // created which doesn't fit in the buffer
        private readonly byte[] _lastCsvItem = new byte[4096];

        // The number of bytes in the buffer that the remainder uses
        private int _usedBuffer;

        // The current number
        private int _itemIndex;
        private long _position;
        
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly object[] _headers;

        /// <summary>
        /// Creates a continuous stream of CSV records up to a given number
        /// </summary>
        /// <param name="headers">An object array for the first row</param>
        /// <param name="recordGenerator">A function to return an object array for the current record i</param>
        /// <param name="maxSize">The maximum number of rows to generate (default: 1,000,000)</param>
        /// <param name="encoding">The text encoding (default: UTF8)</param>
        public InfiniteCsvStream(object[] headers, Func<int, object[]> recordGenerator, int maxSize = 1000000,
            Encoding encoding = null)
        {
            _maxSize = maxSize;
            _recordGenerator = recordGenerator;
            _encoding = encoding ?? Encoding.UTF8;
            _headers = headers;
        }

        private byte[] GenerateCsvRecordByteArray(object[] items)
        {
            _sb.Clear();
            var str = string.Join(",", items.Select(x =>
            {
                var itemString = x?.ToString();
                if (itemString == null) return null;

                var a = new []{'\\', ',','\r','\n' };
                if (itemString.IndexOfAny(a) >= 0)
                {
                    return $"\"{itemString}\"";
                }

                return itemString;
            })) + "\r\n";

            return _encoding.GetBytes(str);
        }

        private int WriteHeadersToBuffer(byte[] buffer)
        {
            var bytes = GenerateCsvRecordByteArray(_headers);
            bytes.CopyTo(buffer, 0);

            _position = bytes.Length;
            return bytes.Length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            byte[] bytes = new byte[4096];
            var bufferIndex = 0;

            if (_itemIndex == 0)
            {
                bufferIndex = WriteHeadersToBuffer(buffer);
            }

            if (_itemIndex < _maxSize)
            {
                while (bufferIndex < count && _itemIndex < _maxSize)
                {
                    if (_usedBuffer > 0)
                    {
                        _lastCsvItem
                            .Take(_usedBuffer)
                            .ToArray()
                            .CopyTo(buffer, 0);
                        bufferIndex = _usedBuffer;
                        _position += _usedBuffer;
                        _usedBuffer = 0;
                    }

                    var i = 0;

                    while (_itemIndex < _maxSize && bufferIndex < count)
                    {
                        bytes = GenerateCsvRecordByteArray(_recordGenerator(_itemIndex));
                        i = 0;

                        while (bufferIndex < count && i < bytes.Length)
                        {
                            buffer[bufferIndex++] = bytes[i++];

                            _position++;
                        }

                        _itemIndex++;
                    }

                    // Copy remainder to used buffer
                    _usedBuffer = bytes.Length - i;
                    if (_usedBuffer > 0)
                    {
                        bytes.Skip(i).ToArray().CopyTo(_lastCsvItem, 0);
                    }
                }
            }
            else if (_usedBuffer > 0 && bufferIndex < count)
            {
                _lastCsvItem
                    .Take(_usedBuffer)
                    .ToArray().CopyTo(buffer, 0);
                _position += _usedBuffer;
                bufferIndex += _usedBuffer;
                _usedBuffer = 0;
            }

            return bufferIndex;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();

        public override bool CanRead => _itemIndex < _maxSize;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _position;

        public override long Position
        {
            get => _position;
            set => throw new NotImplementedException();
        }
    }

}
