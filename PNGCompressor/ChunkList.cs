using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNGComp
{
    class ChunkList
    {
        private int chunkCount;
        private List<Chunk> chunkList;
        private int indexOfIDAT;

        public ChunkList(Stream inputStream, int offset)
        {
            chunkCount = 0;
            Append(inputStream, offset);
        }

        public ChunkList(ChunkList chunkList)
        {
            this.chunkCount = chunkList.chunkCount;
            this.chunkList = chunkList.chunkList;
            this.indexOfIDAT = chunkList.indexOfIDAT;
        }

        public Chunk CombineIDAT()
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

        public void SetIDAT(Chunk chunk)
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

        public byte[] PrepareForOutput()
        {
            byte[] chunkOut = new byte[] { };
            for (int i = 0; i < chunkCount; i++)
            {
                chunkOut = chunkOut.Concat(chunkList[i].PrepareForOutputChunk()).ToArray();
            }
            return chunkOut;
        }
    }

    class Chunk
    {
        private int Length;
        private byte[] LengthByte;
        private string Type;
        private byte[] TypeByte;
        private byte[] Data;
        private byte[] CRC;

        public Chunk(Stream inputStream)
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

        public int GetChunkLength()
        {
            return this.Length + 12;
        }

        public string GetChunkType()
        {
            return this.Type;
        }

        public byte[] GetChunkData()
        {
            return this.Data;
        }

        public void SetChunkData(byte[] data)
        {
            this.Data = data;
        }

        public void Combine(Chunk chunk)
        {
            this.Length += chunk.Length;
            this.Data = this.Data.Concat(chunk.Data).ToArray();
        }

        public void Refresh()
        {
            this.Length = this.Data.Length;
            this.LengthByte = BitConverter.GetBytes(this.Length);
            Array.Reverse(this.LengthByte);
            this.UpdateCRC32();
        }
        public void UpdateIDAT(byte[] dataNew)
        {

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
        public byte[] PrepareForOutputChunk()
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
