using ComponentAce.Zlib;
using CompressSharper.Zopfli;
using System.IO;
using System.IO.Compression;
using System.Linq;

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
            Compress(false);
        }

        public void Compress(bool useZopfli)
        {
            Read();
            Compressor compressor = new Compressor(chunkList);
            compressor.CompressIDAT(useZopfli);
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
            file.Close();
        }

        private void Write()
        {
            fileStream = fileStream.Concat(chunkList.PrepareForOutput()).ToArray();
            FileStream file = new FileStream(outputPath, FileMode.Create);
            BinaryWriter binaryWriter = new BinaryWriter(file);
            binaryWriter.Write(fileStream);
            binaryWriter.Close();
            file.Close();
        }
    }

    internal class Compressor
    {
        public byte[] zlibNoCompressionFlag = new byte[2] { 0x78, 0x01 };
        public byte[] zlibLowCompressionFlag = new byte[2] { 0x78, 0x5E };
        public byte[] zlibDefaultCompressionFlag = new byte[2] { 0x78, 0x9C };
        public byte[] zlibBestCompressionFlag = new byte[2] { 0x78, 0xDA };
        private Chunk idat;
        internal Compressor(ChunkList chunkList)
        {
            idat = chunkList.CombineIDAT();
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            int len;
            while ((len = input.Read(buffer, 0, 4096)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        internal void CompressIDAT(bool useZopfli)
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
                        if (useZopfli)
                        {
                            outputStream.Write(zlibBestCompressionFlag, 0, 2);
                            byte[] dataRaw = new byte[tempStream.Length];
                            tempStream.Read(dataRaw, 0, dataRaw.Length);
                            ZopfliDeflater zopfliDeflater = new ZopfliDeflater(outputStream);
                            zopfliDeflater.Deflate(dataRaw, false);
                        }
                        else
                        {
                            ZOutputStream zOutputStream = new ZOutputStream(outputStream, zlibConst.Z_BEST_COMPRESSION);
                            CopyStream(tempStream, zOutputStream);
                            zOutputStream.Flush();
                        }

                        byte[] dataNew = new byte[outputStream.Length];
                        outputStream.Position = 0;
                        outputStream.Read(dataNew, 0, dataNew.Length);
                        idat.SetChunkData(dataNew);
                        idat.Refresh();
                    }
                }
            }
        }

        internal void UpdateChunkList(ChunkList chunkList)
        {
            chunkList.SetIDAT(idat);
        }
    }
}
