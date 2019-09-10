using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using CompressSharper;

namespace PNGComp
{
    class Compressor
    {
        //public ChunkList chunkList;
        private Chunk idat;
        public Compressor(ChunkList chunkList)
        {
            // this.chunkList = new ChunkList(chunkList);
            chunkList.CombineIDAT();
            idat = chunkList.GetIDAT();
        }

        public void CompressIDAT()
        {
            byte[] data = idat.GetChunkData();
            using (MemoryStream inputStream = new MemoryStream(data, 2, data.Length - 2))
            {
                using (MemoryStream tempStream = new MemoryStream())
                {
                    using (DeflateStream inflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        inflateStream.CopyTo(tempStream);
                    }
                    tempStream.Position = 0;
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        outputStream.WriteByte(0x78);
                        outputStream.WriteByte(0xDA);
                        //using (DeflateStream deflateStream = new DeflateStream(outputStream, CompressionLevel.Optimal, true))
                        //{
                        //    tempStream.CopyTo(deflateStream);
                        //}
                        byte[] dataRaw = new byte[tempStream.Length];
                        tempStream.Read(dataRaw, 0, dataRaw.Length);
                        ZopfliDeflater zopfliDeflater = new ZopfliDeflater(outputStream);
                        zopfliDeflater.Deflate(dataRaw, false);

                        byte[] dataNew = new byte[outputStream.Length];
                        outputStream.Position = 0;
                        outputStream.Read(dataNew, 0, dataNew.Length);
                        idat.SetChunkData(dataNew);
                        idat.Refresh();
                    }
                }
            }
        }

        public void UpdateChunkList(ChunkList chunkList)
        {
            chunkList.SetIDAT(idat);
        }
    }
}
