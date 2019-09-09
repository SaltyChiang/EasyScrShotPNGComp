using System;

namespace PNGComp
{
    class Program
    {
        static void Main(string[] args)
        {
            PNGReader pngReader = new PNGReader("test.png");
            PNGWriter pngWriter = new PNGWriter(pngReader.chunkList);
        }
    }
}
