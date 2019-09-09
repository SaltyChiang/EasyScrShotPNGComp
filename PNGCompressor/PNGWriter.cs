using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PNGComp
{
    class PNGWriter
    {
        public static byte[] pngMagic = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private byte[] fileStream;
        public PNGWriter(ChunkList chunkList)
        {
            byte[] fileStream = new byte[8];
            pngMagic.CopyTo(fileStream, 0);
            fileStream = fileStream.Concat(chunkList.PrepareForOutput()).ToArray();
            FileStream file = new FileStream("testout.png", FileMode.OpenOrCreate);
            BinaryWriter binaryWriter = new BinaryWriter(file);
            binaryWriter.Write(fileStream);
            binaryWriter.Close();
            file.Close();
        }
    }
}
