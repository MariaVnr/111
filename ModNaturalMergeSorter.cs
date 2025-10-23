using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_merge_sort
{
    internal class ModNaturalMergeSorter
    {
        private readonly string inputFile;
        private readonly string outputFile;
        private readonly string workFile;
        private readonly string tempFile1 = "temp1.txt";
        private readonly string tempFile2 = "temp2.txt";

        
        private const long MAX_CHUNK_MEMORY = 200 * 1024 * 1024; 
        private const int BUFFER_SIZE = 65536;

        private int iterations = 0;
        private long comparisons = 0;
        private int initialRunsCount = 0;

        public ModNaturalMergeSorter(string inputFile, string outputFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.workFile = "work_file.txt";
        }

        
        public void Sort()
        {
            

            var stopwatch = Stopwatch.StartNew();

            CreateInitialSortedRuns();
            while (true)
            {
                iterations++;

                int seriesCount = Distribute();

                if (seriesCount <= 1)
                {
                    break;
                }

                Merge();
            }
            File.Copy(workFile, outputFile, true);
            Cleanup();

            stopwatch.Stop();

            var inputSize = new FileInfo(inputFile).Length / 1024.0 / 1024.0;
            var outputSize = new FileInfo(outputFile).Length / 1024.0 / 1024.0;

            
            Console.WriteLine($"Час виконання Модифікованого Природного злиття: {stopwatch.Elapsed.TotalSeconds:F2} секунд");
        }

        private void CreateInitialSortedRuns()
        {
            var lines = new List<string>();
            long currentMemory = 0;
            int runNumber = 0;

            using (var reader = new StreamReader(inputFile, Encoding.UTF8, true, BUFFER_SIZE))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                    currentMemory += line.Length * 2;
                    if (currentMemory >= MAX_CHUNK_MEMORY)
                    {
                        WriteRunToFile(lines, runNumber == 0);
                        lines.Clear();
                        currentMemory = 0;
                        runNumber++;
                    }
                }
                if (lines.Count > 0)
                {
                    WriteRunToFile(lines, runNumber == 0);
                    runNumber++;
                }
            }

            initialRunsCount = runNumber;
        }

        private void WriteRunToFile(List<string> lines, bool createNew)
        {
            var sorted = lines.OrderBy(line => line[0]).ToList();

            using (var writer = new StreamWriter(workFile, !createNew, Encoding.UTF8, BUFFER_SIZE))
            {
                foreach (var line in sorted)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private char GetKey(string line)
        {
            if (string.IsNullOrEmpty(line))
                return '\0';
            return line[0];
        }

        private int Distribute()
        {
            int seriesCount = 0;

            using (var input = new StreamReader(workFile, Encoding.UTF8, true, BUFFER_SIZE))
            using (var output1 = new StreamWriter(tempFile1, false, Encoding.UTF8, BUFFER_SIZE))
            using (var output2 = new StreamWriter(tempFile2, false, Encoding.UTF8, BUFFER_SIZE))
            {
                StreamWriter currentOutput = output1;
                char previousKey = '\0';
                bool firstLine = true;
                string line;

                while ((line = input.ReadLine()) != null)
                {
                    char currentKey = GetKey(line);
                    if (currentKey == '\0')
                        continue;

                    if (!firstLine)
                    {
                        comparisons++;
                        if (currentKey < previousKey)
                        {
                            currentOutput = (currentOutput == output1) ? output2 : output1;
                            seriesCount++;
                        }
                    }

                    currentOutput.WriteLine(line);
                    previousKey = currentKey;
                    firstLine = false;
                }
            }

            return seriesCount + 1;
        }

        private void Merge()
        {
            using (var input1 = new StreamReader(tempFile1, Encoding.UTF8, true, BUFFER_SIZE))
            using (var input2 = new StreamReader(tempFile2, Encoding.UTF8, true, BUFFER_SIZE))
            using (var output = new StreamWriter(workFile, false, Encoding.UTF8, BUFFER_SIZE))
            {
                string line1 = input1.ReadLine();
                string line2 = input2.ReadLine();

                while (line1 != null && line2 != null)
                {
                    MergeOneSeries(input1, input2, output, ref line1, ref line2);
                }

                if (line1 != null)
                {
                    output.WriteLine(line1);
                    while ((line1 = input1.ReadLine()) != null)
                    {
                        output.WriteLine(line1);
                    }
                }

                if (line2 != null)
                {
                    output.WriteLine(line2);
                    while ((line2 = input2.ReadLine()) != null)
                    {
                        output.WriteLine(line2);
                    }
                }
            }
        }

        private void MergeOneSeries(StreamReader input1, StreamReader input2,
            StreamWriter output, ref string line1, ref string line2)
        {
            char key1 = GetKey(line1);
            char key2 = GetKey(line2);

            bool series1Active = true;
            bool series2Active = true;

            while (series1Active || series2Active)
            {
                if (!series1Active)
                {
                    output.WriteLine(line2);
                    char prevKey2 = key2;
                    line2 = input2.ReadLine();

                    if (line2 == null)
                    {
                        series2Active = false;
                        break;
                    }

                    key2 = GetKey(line2);
                    comparisons++;
                    if (key2 < prevKey2)
                    {
                        series2Active = false;
                    }
                }
                else if (!series2Active)
                {
                    output.WriteLine(line1);
                    char prevKey1 = key1;
                    line1 = input1.ReadLine();

                    if (line1 == null)
                    {
                        series1Active = false;
                        break;
                    }

                    key1 = GetKey(line1);
                    comparisons++;
                    if (key1 < prevKey1)
                    {
                        series1Active = false;
                    }
                }
                else
                {
                    comparisons++;
                    if (key1 <= key2)
                    {
                        output.WriteLine(line1);
                        char prevKey1 = key1;
                        line1 = input1.ReadLine();

                        if (line1 == null)
                        {
                            series1Active = false;
                            continue;
                        }

                        key1 = GetKey(line1);
                        comparisons++;
                        if (key1 < prevKey1)
                        {
                            series1Active = false;
                        }
                    }
                    else
                    {
                        output.WriteLine(line2);
                        char prevKey2 = key2;
                        line2 = input2.ReadLine();

                        if (line2 == null)
                        {
                            series2Active = false;
                            continue;
                        }

                        key2 = GetKey(line2);
                        comparisons++;
                        if (key2 < prevKey2)
                        {
                            series2Active = false;
                        }
                    }
                }
            }
        }

        private void Cleanup()
        {
            try
            {
                if (File.Exists(tempFile1))
                    File.Delete(tempFile1);
                if (File.Exists(tempFile2))
                    File.Delete(tempFile2);
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

