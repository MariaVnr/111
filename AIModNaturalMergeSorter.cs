using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_merge_sort
{
    internal class AIModNaturalMergeSorter
    {
       
        private const long MaxSortBufferBytes = (long)(300 * 1024 * 1024 * 0.85);
        private const int IO_BUFFER_SIZE = 4 * 1024 * 1024;

        private readonly string inputFile;
        private readonly string outputFile;
        private readonly string workFile;
        
        private readonly string[] tempFiles = { "temp_a.txt", "temp_b.txt", "temp_c.txt", "temp_d.txt" };

        private int iterations = 0;
        private long comparisons = 0;

        public AIModNaturalMergeSorter(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.workFile = "work_file.txt"; 
        }

        public void Sort()
        {
            var stopwatch = Stopwatch.StartNew();

            int initialSeriesCount = InitialSortAndDistribute();

            if (initialSeriesCount <= 1)
            {
                File.Copy(workFile, outputFile, true);
            }
            else
            {
                string currentSourceFile = workFile;
                string currentDestFile = tempFiles[0]; 

                while (true)
                {
                    iterations++;
                    Console.WriteLine($"\nІтерація {iterations}:");

                    int seriesCount = Distribute(currentSourceFile);
                    Console.WriteLine($"  Серій після розподілу: {seriesCount}");

                    if (seriesCount <= 1)
                    {
                        currentDestFile = currentSourceFile;
                        break;
                    }

                    Merge4To1();

                    currentSourceFile = workFile;
                }
                File.Copy(currentDestFile, outputFile, true);
            }

            Cleanup();
            stopwatch.Stop();
            Console.WriteLine($"Час виконання Модифікованого чат-ботом: {stopwatch.Elapsed.TotalSeconds:F2} секунд");
        }

        
        private int InitialSortAndDistribute()
        {
            var records = new List<string>();
            long currentMemoryUsage = 0;
            int currentFileIndex = 0;
            int totalRuns = 0;

            var writers = tempFiles.Select(f => new StreamWriter(f, false, Encoding.UTF8, IO_BUFFER_SIZE)).ToList();

            using (var reader = new StreamReader(inputFile, Encoding.UTF8, true, IO_BUFFER_SIZE))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    int lineSize = line.Length * 2 + 4;

                    if (currentMemoryUsage + lineSize > MaxSortBufferBytes)
                    {
                        records.Sort(StringComparer.Ordinal);
                        foreach (var rec in records)
                        {
                            writers[currentFileIndex].WriteLine(rec);
                        }

                        records.Clear();
                        currentMemoryUsage = 0;
                        totalRuns++;
                        currentFileIndex = (currentFileIndex + 1) % writers.Count;
                    }

                    records.Add(line);
                    currentMemoryUsage += lineSize;
                }

                if (records.Any())
                {
                    records.Sort(StringComparer.Ordinal);
                    foreach (var rec in records)
                    {
                        writers[currentFileIndex].WriteLine(rec);
                    }
                    totalRuns++;
                }
            }

            writers.ForEach(w => w.Dispose());
            Merge4To1();

            return totalRuns;
        }

        private int Distribute(string sourceFile)
        {
            int seriesCount = 0;
            int currentFileIndex = 0;

            var writers = tempFiles.Select(f => new StreamWriter(f, false, Encoding.UTF8, IO_BUFFER_SIZE)).ToList();

            using (var input = new StreamReader(sourceFile, Encoding.UTF8, true, IO_BUFFER_SIZE))
            {
                string previousLine = null;
                string line;

                while ((line = input.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (previousLine != null)
                    {
                        comparisons++;
                        if (string.CompareOrdinal(line, previousLine) < 0)
                        {
                            // Кінець серії: перемикаємо файл
                            currentFileIndex = (currentFileIndex + 1) % writers.Count;
                            seriesCount++;
                        }
                    }

                    writers[currentFileIndex].WriteLine(line);
                    previousLine = line;
                }
            }

            writers.ForEach(w => w.Dispose());
            return seriesCount + 1;
        }

        private void Merge4To1()
        {
            using (var output = new StreamWriter(workFile, false, Encoding.UTF8, IO_BUFFER_SIZE))
            {
                MergeTwoFiles(tempFiles[0], tempFiles[1], output);

                MergeTwoFiles(tempFiles[2], tempFiles[3], output);
            }
        }

        private void MergeTwoFiles(string file1, string file2, StreamWriter output)
        {
            using (var input1 = new StreamReader(file1, Encoding.UTF8, true, IO_BUFFER_SIZE))
            using (var input2 = new StreamReader(file2, Encoding.UTF8, true, IO_BUFFER_SIZE))
            {
                string line1 = input1.ReadLine();
                string line2 = input2.ReadLine();

                while (line1 != null && line2 != null)
                {
                    MergeOneSeries(input1, input2, output, ref line1, ref line2);
                }

                AppendRemaining(input1, output, ref line1);
                AppendRemaining(input2, output, ref line2);
            }
        }

        private void AppendRemaining(StreamReader input, StreamWriter output, ref string line)
        {
            if (line != null)
            {
                output.WriteLine(line);
                while ((line = input.ReadLine()) != null)
                {
                    output.WriteLine(line);
                }
            }
        }

        private void MergeOneSeries(StreamReader input1, StreamReader input2,
            StreamWriter output, ref string line1, ref string line2)
        {
            bool series1Active = line1 != null;
            bool series2Active = line2 != null;

            while (series1Active || series2Active)
            {
                if (!series1Active) 
                {
                    WriteAndAdvance(input2, output, ref line2, ref series2Active, true);
                }
                else if (!series2Active)
                {
                    WriteAndAdvance(input1, output, ref line1, ref series1Active, true);
                }
                else 
                {
                    comparisons++;
                    if (string.CompareOrdinal(line1, line2) <= 0)
                    {
                        WriteAndAdvance(input1, output, ref line1, ref series1Active, false);
                    }
                    else
                    {
                        WriteAndAdvance(input2, output, ref line2, ref series2Active, false);
                    }
                }
            }
        }

        private void WriteAndAdvance(StreamReader input, StreamWriter output, ref string currentLine,
            ref bool seriesActive, bool isTail)
        {
            output.WriteLine(currentLine);
            string prevLine = currentLine;
            currentLine = input.ReadLine();

            if (currentLine == null)
            {
                seriesActive = false;
                return;
            }

            if (!isTail)
            {
                comparisons++;
                if (string.CompareOrdinal(currentLine, prevLine) < 0)
                {
                    seriesActive = false;
                }
            }
        }


        private void Cleanup()
        {
            try
            {
                foreach (var file in tempFiles)
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                if (File.Exists(workFile))
                    File.Delete(workFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при видаленні тимчасових файлів: {ex.Message}");
            }
        }
    }
}
