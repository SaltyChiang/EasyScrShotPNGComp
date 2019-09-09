using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNGComp
{
    class PNGReader
    {
        public ChunkList chunkList;
        private string inputPath;
        private int offset;

        public PNGReader(string inputPath)
        {
            offset = 0;
            this.inputPath = inputPath;
        }

        public void Read()
        {
            Stream file = new FileStream(inputPath, FileMode.Open);
            byte[] pngId = new byte[8];
            file.Read(pngId, 0, 8);
            offset += pngId.Length;
            chunkList = new ChunkList(file, offset);
            chunkList.CombineIDAT();
        }
    }
}
