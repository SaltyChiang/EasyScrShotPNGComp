
namespace PNGComp
{
    class Program
    {
        static void Main(string[] args)
        {
            PNGComp pngComp = new PNGComp("testoutout.png", "testout.png");
            pngComp.Compress(false);
            
        }
    }
}
