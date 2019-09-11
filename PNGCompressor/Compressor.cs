using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using CompressSharper.Zopfli;
using ComponentAce.Zlib;

namespace PNGComp
{
    class Compressor
    {
        private Chunk idat;
        public Compressor(ChunkList chunkList)
        {
            idat = chunkList.CombineIDAT();
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
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
                        //byte[] dataRaw = new byte[tempStream.Length];
                        //tempStream.Read(dataRaw, 0, dataRaw.Length);
                        //ZopfliDeflater zopfliDeflater = new ZopfliDeflater(outputStream);
                        //zopfliDeflater.Deflate(dataRaw, false);

                        ZOutputStream zOutputStream = new ZOutputStream(outputStream, zlibConst.Z_BEST_COMPRESSION);
                        CopyStream(tempStream, zOutputStream);
                        zOutputStream.Flush();

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
