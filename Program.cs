using System.Text;
using Natural_merge_sort;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        const string inputFilename = "unsorted_data.txt";
        const string outputFilename = "sorted_data.txt";
        const int FILE_SIZE_MB = 10;

        try
        {
            long recordsCount = DataGenerator.GenerateTestFile(inputFilename, FILE_SIZE_MB);


            var sorter1 = new NaturalMergeSorter(inputFilename, outputFilename);
            sorter1.Sort();

            var sorter2 = new ModNaturalMergeSorter(inputFilename, outputFilename);
            sorter2.Sort();

            var sorter3 = new AIModNaturalMergeSorter(inputFilename, outputFilename);
            sorter3.Sort();


            var inputInfo = new FileInfo(inputFilename);
            var outputInfo = new FileInfo(outputFilename);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nПомилка: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}