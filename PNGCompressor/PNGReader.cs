using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PNGComp
{
    class PNGReader
    {
        public ChunkList chunkList;

        public PNGReader(string fileName)
        {
            int offset = 0;
            Stream file = new FileStream(fileName, FileMode.Open);
            byte[] pngId = new byte[8];
            file.Read(pngId, 0, 8);
            offset += pngId.Length;
            chunkList = new ChunkList(file, offset);
            file.Close();
            chunkList.CombineIDAT();
            //chunkList.PrintListInfo();
        }
    }
}
