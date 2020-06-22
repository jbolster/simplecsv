using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleCsv
{
    public class CsvParse
    {
        /// <summary>
        /// Parse a CSV stream
        /// </summary>
        /// <param name="stream">The stream to read</param>
        /// <param name="onReadAction">After each record, the action to call with a string array for the row</param>
        public static bool Parse(Stream stream, Action<string[]> onReadAction)
        {
            // Used as a buffer for each column
            byte[] columnBuffer = new byte[1024];

            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                var headers = ReadLine(streamReader, columnBuffer);
                var numColumns = 0;
                while (headers[numColumns] != null)
                {
                    numColumns++;
                }

                onReadAction(headers);

                while (!streamReader.EndOfStream)
                {
                    onReadAction(ReadLine(streamReader, columnBuffer, numColumns));
                }
            }

            return true;
        }
        
        private static string[] ReadLine(StreamReader streamReader, byte[] columnBuffer, int numberOfColumns = 10)
        {
            string[] stringArray = new string[numberOfColumns];

            short tokenIndex = 0;
            bool columnQuoted = false;
            byte columnIndex = 0;
            byte currentByte;
            byte nextByte;

            // Read token
            while (tokenIndex < 1024 && !streamReader.EndOfStream)
            {
                currentByte = (byte) streamReader.Read();

                //if (currentByte >= 'A' && currentByte <= 'Z')
                //{
                //    currentByte |= 0b00100000;
                //}

                if (tokenIndex != 0)
                {
                    if (currentByte == 0x22 /* '"' */)
                    {
                        if (columnQuoted)
                        {
                            nextByte = (byte) streamReader.Read();
                            if (nextByte == '"')
                            {
                                columnBuffer[tokenIndex] = nextByte;
                                tokenIndex++;
                                continue;
                            }
                            else
                            {
                                columnBuffer[tokenIndex] = currentByte;
                                columnQuoted = false;
                                continue;
                            }
                        }

                        throw new Exception("Invalid file - quote not escaped");
                    }

                    if (!columnQuoted)
                    {
                        // If we've reached a comma outside of a quoted string, then
                        // get the value of the buffer for the current column
                        // Progress the column counter and reset the buffer index
                        // for next processing
                        if (currentByte == ',')
                        {
                            stringArray[columnIndex] = Encoding.UTF8.GetString(columnBuffer, 0, tokenIndex);
                            columnIndex++;
                            tokenIndex = 0;
                            continue;
                        }

                        // If column is CR then assume NL after and read that
                        // then set the column value to the current token buffer
                        // and return (we've finished the record)
                        if (currentByte == '\r')
                        {
                            streamReader.Read();
                            stringArray[columnIndex] = Encoding.UTF8.GetString(columnBuffer, 0, tokenIndex);

                            return stringArray;
                        }

                        // If column is NL then set the column value to the
                        // current token buffer and return (we've finished the record)
                        if (currentByte == '\n')
                        {
                            stringArray[columnIndex] = Encoding.UTF8.GetString(columnBuffer, 0, tokenIndex);

                            return stringArray;
                        }
                    }
                }
                else
                {
                    // If the column begins with a quote, then mark as
                    // a quoted column and move to next character
                    if (currentByte == '"')
                    {
                        columnQuoted = true;
                        continue;
                    }

                    // If the first token is a quote, then it means that the
                    // column is empty. Progress to next character
                    if (currentByte == ',')
                    {
                        columnIndex++;
                        continue;
                    }

                    // If the first token is a new CR, then read the next char
                    // (assume it's a NL) and return the array - we've finished the record
                    if (currentByte == '\r')
                    {
                        streamReader.Read();
                        return stringArray;
                    }

                    // If the first token is a NL then return the array
                    // - we've finished the record
                    if (currentByte == '\n')
                    {
                        return stringArray;
                    }
                }

                // In all other cases, it's just a normal character
                // and should be stored
                columnBuffer[tokenIndex] = currentByte;
                tokenIndex++;
            }

            // This handles the case when we've reached the end but
            // the record wasn't finished or didn't end with a new line
            stringArray[columnIndex] = Encoding.UTF8.GetString(columnBuffer, 0, tokenIndex);

            return stringArray;
        }
    }
}