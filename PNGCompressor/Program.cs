
namespace PNGComp
{
    class Program
    {
        static void Main(string[] args)
        {
            PNGComp pngComp = new PNGComp("test.png", "testout.png");
            pngComp.Compress();
        }
    }
}
