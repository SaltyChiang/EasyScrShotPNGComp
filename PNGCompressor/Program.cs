
using System;
using System.IO;

namespace PNGComp
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] fileList = Directory.GetFiles(".", "*.png");
            int threadsMaxCount = Environment.ProcessorCount;

            PNGCompMT.CompressMT(fileList, threadsMaxCount);

            Console.WriteLine("\nAll compressed. \nPress Enter to quit.");
            Console.ReadLine();

        }
    }
}
