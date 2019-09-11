/**************************************************************************
 * Adapted and ported to C# in 2014 by
 * Steven Giacomelli (stevegiacomelli@gmail.com)
 **************************************************************************
 * Orginal Version
 * Copyright 2011 Google Inc. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Author: lode.vandevenne@gmail.com (Lode Vandevenne)
 * Author: jyrki.alakuijala@gmail.com (Jyrki Alakuijala)
************************************************************************** */

using System;
using System.IO;

namespace CompressSharper.Zopfli
{
    /// <summary>
    /// Used by the Deflater to write bit encoded to an output stream
    /// </summary>
    sealed internal class BitWriter
    {
        #region Constructor

        /// <summary>
        /// Constructs a new BitWritter
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public BitWriter(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanWrite)
                throw new ArgumentException("stream must be writable", "stream");

            _stream = stream;
        }

        #endregion Constructor

        #region Properties/Fields

        private int _bitBuffer = 0;
        private int _bitCount = 0;
        private Stream _stream = null;
        #endregion Properties/Fields

        #region Public Methods

        /// <summary>
        /// Writes the stored bits to the stream
        /// </summary>
        public void FlushBits()
        {
            //do a sanity check on the stream
            if (_stream == null ||
                !_stream.CanWrite)
                return;

            if (_bitCount > 0)
            {
                _stream.Write(new byte[] { (byte)_bitBuffer }, 0, 1);
                _bitCount = 0;
                _bitBuffer = 0;
            }
        }

        /// <summary>
        /// Write a single bit
        /// </summary>
        /// <param name="value">The bit to write</param>
        public void Write(byte value)
        {
            Write(value, 1);
        }

        /// <summary>
        /// Writes the bits
        /// </summary>
        /// <param name="value">The packed bits</param>
        /// <param name="numberOfBits">The number if bits to write</param>
        public void Write(uint value, int numberOfBits)
        {
            //sanity check the stream
            if (_stream == null ||
                !_stream.CanWrite)
                return;

            PrivateWrite(value, numberOfBits);
        }

        /// <summary>
        /// Writes a number of bytes to the stream
        /// </summary>
        /// <param name="buffer">The bytes to write</param>
        public void Write(byte[] buffer)
        {
            if (buffer == null)
                return;

            Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a number of bytes to the stream
        /// </summary>
        /// <param name="buffer">The bytes to write</param>
        /// <param name="offset">The offset to blockStart</param>
        /// <param name="count">The total number of bytes to write</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            //sanity check the stream
            if (_stream == null ||
                !_stream.CanWrite)
            {
                return;
            }

            if (_bitCount == 0)
                _stream.Write(buffer, offset, count);
            else
                WriteUnaligned(buffer, offset, count);
        }

        /// <summary>
        /// Writes the bits
        /// </summary>
        /// <param name="value">The packed bits</param>
        /// <param name="numberOfBits">The number if bits to write</param>
        public void WriteHuffman(uint value, int numberOfBits)
        {
            //sanity check the stream
            if (_stream == null ||
                !_stream.CanWrite)
                return;

            PrivateWriteHuffman(value, numberOfBits);
        }
        #endregion Public Methods

        #region Private Methods

        private void PrivateWrite(uint value, int numberOfBits)
        {
            for (int i = 0; i < numberOfBits; i++)
            {
                _bitBuffer |= (byte)(((value >> i) & 1) << _bitCount++);

                if (_bitCount >= 8)
                {
                    _stream.Write(new byte[] { (byte)_bitBuffer }, 0, 1);
                    _bitCount -= 8;
                    _bitBuffer >>= 8;
                }
            }
        }

        private void PrivateWriteHuffman(uint value, int numberOfBits)
        {
            for (int i = 0; i < numberOfBits; i++)
            {
                _bitBuffer |= (byte)(((value >> (numberOfBits - i - 1)) & 1) << _bitCount++);

                if (_bitCount >= 8)
                {
                    _stream.Write(new byte[] { (byte)_bitBuffer }, 0, 1);
                    _bitCount -= 8;
                    _bitBuffer >>= 8;
                }
            }
        }

        private void WriteUnaligned(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < (offset + count); i++)
                PrivateWrite(buffer[i], 8);
        }
        #endregion Private Methods
    }
}