using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PNGComp
{
    class PNGComp
    {
        public static byte[] pngMagic = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public ChunkList chunkList;
        private byte[] fileStream;
        private string inputPath;
        private string outputPath;
        private int offset;

        public PNGComp(string inputPath, string outputPath)
        {
            fileStream = new byte[8];
            pngMagic.CopyTo(fileStream, 0);
            offset = 0;
            this.inputPath = inputPath;
            this.outputPath = outputPath;
            
        }

        public void Compress()
        {
            Read();
            Compressor compressor = new Compressor(chunkList);
            compressor.CompressIDAT();
            compressor.UpdateChunkList(chunkList);
            Write();
        }
        private void Read()
        {
            Stream file = new FileStream(inputPath, FileMode.Open);
            byte[] pngId = new byte[8];
            file.Read(pngId, 0, 8);
            offset += pngId.Length;
            chunkList = new ChunkList(file, offset);
        }

        private void Write()
        {
            fileStream = fileStream.Concat(chunkList.PrepareForOutput()).ToArray();
            FileStream file = new FileStream(outputPath, FileMode.OpenOrCreate);
            BinaryWriter binaryWriter = new BinaryWriter(file);
            binaryWriter.Write(fileStream);
            binaryWriter.Close();
            file.Close();
        }
    }
}
