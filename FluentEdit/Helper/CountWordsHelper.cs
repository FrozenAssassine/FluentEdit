using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FluentEdit.Helper
{
    internal class CountWordsHelper
    {
        public static int CountWords(IEnumerable<string> lines)
        {
            int words = 0;
            IEnumerator enumerator = lines.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object currentItem = enumerator.Current;
                words += currentItem.ToString().Count(x => x == '\n' || x == '\r' || x == ' ') + 1;
            }
            return words;
        }

        public static int CountWordsSpan(IEnumerable<string> lines)
        {
            int wordCount = 0;

            foreach (var line in lines)
            {
                var span = line.AsSpan();
                int index = 0;

                while (index < span.Length)
                {
                    while (index < span.Length && char.IsWhiteSpace(span[index]))
                    {
                        index++;
                    }

                    if (index < span.Length)
                    {
                        wordCount++;
                    }

                    while (index < span.Length && !char.IsWhiteSpace(span[index]))
                    {
                        index++;
                    }
                }
            }

            return wordCount;
        }

        private static void Benchmark(Action action, string output)
        {
            long initialMemory = GC.GetTotalMemory(true);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            action.Invoke();

            stopwatch.Stop();

            long finalMemory = GC.GetTotalMemory(false);

            TimeSpan elapsedTime = stopwatch.Elapsed;
            long memoryUsed = finalMemory - initialMemory;

            Debug.WriteLine($"{output}> Elapsed Time: {elapsedTime.TotalMilliseconds} ms");
            Debug.WriteLine($"{output}> Memory Used: {memoryUsed / 1024.0:F2} KB");
        }
    }
}
