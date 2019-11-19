﻿using ComponentAce.Zlib;
using CompressSharper.Zopfli;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

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
            PNGCompMT.completCount += 1;
            Console.WriteLine("Compressed {0:D}/{1:D}.", PNGCompMT.completCount, PNGCompMT.fileCount);
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

    public static class PNGCompMT
    {
        static private int i, j;
        static public int completCount = 0;
        static public int fileCount;

        static private Task[] tasks;

        static public void CompressMT()
        {
            string[] fileList = Directory.GetFiles(".", "*.png");
            int threadsMaxCount = Environment.ProcessorCount;

            CompressMT(fileList, threadsMaxCount);
        }
        static public void CompressMT(string[] fileList, int threadsMaxCount)
        {
            for (int i = 0; i < fileList.Length; i++)
                fileList[i] = Path.GetFileName(fileList[i]);
            fileCount = fileList.Length;
            if (fileCount < threadsMaxCount)
                threadsMaxCount = fileCount;

            tasks = new Task[threadsMaxCount];
            Action<object> action = (object obj) =>
            {
                string fileNameString = obj.ToString();
                string fileTempNameString = "temp." + fileNameString;
                PNGComp pngComp = new PNGComp(fileNameString, fileTempNameString);
                pngComp.Compress(false);
            };
            Console.WriteLine("Compressing {0:D} .png files......", fileCount);
            for (i = 0; i < fileCount; i++)
            {
                if (i < threadsMaxCount)
                {
                    tasks[i] = new Task(action, fileList[i]);
                    tasks[i].Start();
                }
                else
                {
                    j = Task.WaitAny(tasks);
                    tasks[j] = new Task(action, fileList[i]);
                    tasks[j].Start();
                }
            }
            Task.WaitAll(tasks);
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
