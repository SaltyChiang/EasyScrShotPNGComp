using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PNGComp
{
    class ChunkList
    {
        private int chunkCount;
        private List<Chunk> chunkList;
        private int indexOfIDAT;

        internal ChunkList(Stream inputStream, int offset)
        {
            chunkCount = 0;
            Append(inputStream, offset);
        }

        internal ChunkList(ChunkList chunkList)
        {
            this.chunkCount = chunkList.chunkCount;
            this.chunkList = chunkList.chunkList;
            this.indexOfIDAT = chunkList.indexOfIDAT;
        }

        internal Chunk CombineIDAT()
        {
            int chunkCountNew = 0;
            List<Chunk> chunkListNew = new List<Chunk> { chunkList[0] };
            chunkCountNew++;
            for (int i = 1; i < chunkCount; i++)
            {
                if (chunkList[i - 1].GetChunkType().Equals("IDAT") && chunkList[i].GetChunkType().Equals("IDAT"))
                {
                    chunkListNew[chunkCountNew - 1].Combine(chunkList[i]);
                }
                else if (chunkList[i - 1].GetChunkType().Equals("IDAT") && !chunkList[i].GetChunkType().Equals("IDAT"))
                {
                    chunkListNew[chunkCountNew - 1].Refresh();
                    indexOfIDAT = chunkCountNew - 1;
                    chunkListNew.Add(chunkList[i]);
                    chunkCountNew++;
                }
                else
                {
                    chunkListNew.Add(chunkList[i]);
                    chunkCountNew++;
                }
            }
            chunkCount = chunkCountNew;
            chunkList = chunkListNew;

            return chunkList[indexOfIDAT];
        }

        internal void SetIDAT(Chunk chunk)
        {
            chunkList[indexOfIDAT] = chunk;
        }

        private void Append(Stream inputStream, int offset)
        {
            while (offset < inputStream.Length)
            {
                Chunk chunk = new Chunk(inputStream);
                if (chunkList == null)
                    chunkList = new List<Chunk> { chunk };
                else
                    chunkList.Add(chunk);
                offset += chunkList[chunkCount].GetChunkLength();
                chunkCount++;
            }
        }

        internal byte[] PrepareForOutput()
        {
            byte[] chunkOut = new byte[] { };
            for (int i = 0; i < chunkCount; i++)
            {
                chunkOut = chunkOut.Concat(chunkList[i].PrepareForOutputChunk()).ToArray();
            }
            return chunkOut;
        }
    }
    internal class CRC32
    { // based on http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net

        private const UInt32 defaultPolynomial = 0xedb88320;
        private const UInt32 defaultSeed = 0xffffffff;
        private static UInt32[] defaultTable;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;

        public CRC32()
            : this(defaultPolynomial, defaultSeed)
        {
        }

        public CRC32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            this.hash = seed;
        }

        public void Update(byte[] buffer)
        {
            Update(buffer, 0, buffer.Length);
        }

        public void Update(byte[] buffer, int start, int length)
        {
            for (int i = 0, j = start; i < length; i++, j++)
            {
                unchecked
                {
                    hash = (hash >> 8) ^ table[buffer[j] ^ hash & 0xff];
                }
            }
        }

        public UInt32 GetValue()
        {
            return ~hash;
        }

        public void Reset()
        {
            this.hash = seed;
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == defaultPolynomial && defaultTable != null)
                return defaultTable;
            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }
            if (polynomial == defaultPolynomial)
                defaultTable = createTable;
            return createTable;
        }
    }

    internal class Chunk
    {
        private int Length;
        private byte[] LengthByte;
        private string Type;
        private byte[] TypeByte;
        private byte[] Data;
        private byte[] CRC;

        internal Chunk(Stream inputStream)
        {
            this.LengthByte = new byte[4];
            this.TypeByte = new byte[4];
            this.CRC = new byte[4];

            inputStream.Read(this.LengthByte, 0, 4);

            Array.Reverse(this.LengthByte);
            this.Length = BitConverter.ToInt32(this.LengthByte, 0);
            Array.Reverse(this.LengthByte);
            this.Data = new byte[this.Length];

            inputStream.Read(this.TypeByte, 0, 4);
            inputStream.Read(this.Data, 0, this.Length);
            inputStream.Read(this.CRC, 0, 4);
            this.Type = Encoding.ASCII.GetString(this.TypeByte);
        }

        internal int GetChunkLength()
        {
            return this.Length + 12;
        }

        internal string GetChunkType()
        {
            return this.Type;
        }

        internal byte[] GetChunkData()
        {
            return this.Data;
        }

        internal void SetChunkData(byte[] data)
        {
            this.Data = data;
        }

        internal void Combine(Chunk chunk)
        {
            this.Length += chunk.Length;
            this.Data = this.Data.Concat(chunk.Data).ToArray();
        }

        internal void Refresh()
        {
            this.Length = this.Data.Length;
            this.LengthByte = BitConverter.GetBytes(this.Length);
            Array.Reverse(this.LengthByte);
            this.UpdateCRC32();
        }

        private void UpdateCRC32()
        {
            CRC32 crc32 = new CRC32();
            crc32.Update(this.TypeByte);
            crc32.Update(this.Data);
            byte[] hash = BitConverter.GetBytes(crc32.GetValue());
            Array.Reverse(hash);
            this.CRC = hash;
            //Console.WriteLine(BitConverter.ToString(hash));
        }
        internal byte[] PrepareForOutputChunk()
        {
            byte[] chunkOut = new byte[] { };
            chunkOut = chunkOut.Concat(this.LengthByte).ToArray();
            chunkOut = chunkOut.Concat(this.TypeByte).ToArray();
            chunkOut = chunkOut.Concat(this.Data).ToArray();
            chunkOut = chunkOut.Concat(this.CRC).ToArray();
            return chunkOut;
        }
    }
}
