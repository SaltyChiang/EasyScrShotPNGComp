using System;

namespace PNGComp
{
    class Program
    {
        static void Main(string[] args)
        {
            PNGReader pngReader = new PNGReader("test.png");
            pngReader.Read();
            Compressor compressor = new Compressor(pngReader.chunkList);
            compressor.CompressIDAT();
            compressor.UpdateChunkList(pngReader.chunkList);
            PNGWriter pngWriter = new PNGWriter("testout.png");
            pngWriter.Write(pngReader.chunkList);
        }
    }
}
