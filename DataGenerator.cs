using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_merge_sort
{
    internal class DataGenerator
    {
        private static readonly Random random = new Random();
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";

        public static string GeneratePhone()
        {
            return $"+380{random.Next(100000000, 999999999)}";
        }

        public static string GenerateText(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(Chars[random.Next(Chars.Length)]);
            }
            return sb.ToString();
        }

        public static long GenerateTestFile(string filename, int sizeMb = 10)
        {
        
            long targetSize = sizeMb * 1024L * 1024L; 
            long currentSize = 0;
            long recordsCount = 0;

            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                while (currentSize < targetSize)
                {
                    char key = Letters[random.Next(Letters.Length)];
                    string text = GenerateText(random.Next(20, 46));
                    string phone = GeneratePhone();

                    string record = $"{key}|{text}|{phone}";
                    writer.WriteLine(record);

                    currentSize += Encoding.UTF8.GetByteCount(record) + 2; 
                    recordsCount++;

                    
                }
            }

            var fileInfo = new FileInfo(filename);
            Console.WriteLine($"Файл створено: {recordsCount} записів, розмір: {fileInfo.Length / 1024.0 / 1024.0:F2} МБ");
            return recordsCount;
        }
    }
}
