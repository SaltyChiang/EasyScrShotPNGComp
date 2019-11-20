
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PNGComp
{
    class Program
    {
        static void Main(string[] args)
        {
            //string[] fileList = Directory.GetFiles(".", "*.png");
            //int threadsMaxCount = Environment.ProcessorCount;

            //PNGCompMT.CompressMT(fileList, threadsMaxCount);

            //Console.WriteLine("\nAll compressed. \nPress Enter to quit.");
            //Console.ReadLine();

            //Image image = Image.FromFile("test.png");
            //image.Save("test1.png", ImageFormat.Png);
            PNGComp pngComp = new PNGComp("main.vpy - 13576.png", "backup/main.vpy - 13576.png");
            pngComp.Compress(false);

        }
    }
}
