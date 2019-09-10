/*
 * Ported to C# in 2014 by Steven Giacomelli (stevegiacomelli@gmail.com)
 *
 * xxHash - Fast Hash algorithm
 * Copyright (C) 2012-2014, Yann Collet.
 * BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *
 * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * You can contact the author at :
 * - xxHash source repository : http://code.google.com/p/xxhash/
*/

using System.Runtime.CompilerServices;

namespace CompressSharper
{
    public class XXHash
    {
        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;

        private XXHash() { }

#if UNSAFE
        public static uint ComputeHash(byte[] buffer, uint seed)
        {
            unsafe
            {
                unchecked
                {
                    int bufferPosition = 0;
                    uint hashValue = 0;

                    fixed (void* bufferPtr = &buffer[0])
                    {
                        uint* intPtr = (uint*)bufferPtr;

                        if (buffer.Length >= 16)
                        {
                            uint v1 = seed + Prime1 + Prime2;
                            uint v2 = seed + Prime2;
                            uint v3 = seed + 0;
                            uint v4 = seed - Prime1;

                            for (bufferPosition = 0; bufferPosition <= (buffer.Length - 16); bufferPosition += 16)
                            {
                                v1 += *intPtr * Prime2; intPtr++; v1 = RotateLeft(v1, 13); v1 *= Prime1;
                                v2 += *intPtr * Prime2; intPtr++; v2 = RotateLeft(v2, 13); v2 *= Prime1;
                                v3 += *intPtr * Prime2; intPtr++; v3 = RotateLeft(v3, 13); v3 *= Prime1;
                                v4 += *intPtr * Prime2; intPtr++; v4 = RotateLeft(v4, 13); v4 *= Prime1;
                            }

                            hashValue = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
                        }
                        else
                        {
                            hashValue = seed + Prime5;
                        }

                        hashValue += (uint)buffer.Length;

                        for (; bufferPosition <= (buffer.Length - 4); bufferPosition += 4)
                        {
                            hashValue += *intPtr * Prime3; intPtr++;
                            hashValue = RotateLeft(hashValue, 17) * Prime4;
                        }
                    }

                    for (; bufferPosition < buffer.Length; bufferPosition++)
                    {
                        hashValue += ((uint)buffer[bufferPosition]) * Prime5;
                        hashValue = RotateLeft(hashValue, 11) * Prime1;
                    }

                    hashValue ^= hashValue >> 15;
                    hashValue *= Prime2;
                    hashValue ^= hashValue >> 13;
                    hashValue *= Prime3;
                    hashValue ^= hashValue >> 16;

                    return hashValue;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint x, int r)
        {
            unchecked
            {
                return ((x << r) | (x >> (32 - r)));
            }
        }
#else
        public static uint ComputeHash(byte[] buffer, uint seed)
        {
            unchecked
            {
                int bufferPosition = 0;
                uint hashValue = 0;

                if (buffer.Length >= 16)
                {
                    uint v1 = seed + Prime1 + Prime2;
                    uint v2 = seed + Prime2;
                    uint v3 = seed + 0;
                    uint v4 = seed - Prime1;

                    for (bufferPosition = 0; bufferPosition <= (buffer.Length - 16); bufferPosition += 16)
                    {
                        v1 += ReadInt(buffer, bufferPosition) * Prime2; v1 = RotateLeft(v1, 13); v1 *= Prime1;
                        v2 += ReadInt(buffer, bufferPosition + 4) * Prime2; v2 = RotateLeft(v2, 13); v2 *= Prime1;
                        v3 += ReadInt(buffer, bufferPosition + 8) * Prime2; v3 = RotateLeft(v3, 13); v3 *= Prime1;
                        v4 += ReadInt(buffer, bufferPosition + 12) * Prime2; v4 = RotateLeft(v4, 13); v4 *= Prime1;
                    }

                    hashValue = RotateLeft(v1, 1) + RotateLeft(v2, 7) + RotateLeft(v3, 12) + RotateLeft(v4, 18);
                }
                else
                {
                    hashValue = seed + Prime5;
                }

                hashValue += (uint)buffer.Length;

                for (; bufferPosition <= (buffer.Length - 4); bufferPosition += 4)
                {
                    hashValue += ReadInt(buffer, bufferPosition) * Prime3;
                    hashValue = RotateLeft(hashValue, 17) * Prime4;
                }

                for (; bufferPosition < buffer.Length; bufferPosition++)
                {
                    hashValue += ((uint)buffer[bufferPosition]) * Prime5;
                    hashValue = RotateLeft(hashValue, 11) * Prime1;
                }

                hashValue ^= hashValue >> 15;
                hashValue *= Prime2;
                hashValue ^= hashValue >> 13;
                hashValue *= Prime3;
                hashValue ^= hashValue >> 16;

                return hashValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReadInt(byte[] buffer, int position)
        {
            unchecked
            {
                return
                    (((uint)buffer[position + 3]) << 24) +
                    (((uint)buffer[position + 2]) << 16) +
                    (((uint)buffer[position + 1]) << 8) +
                    ((uint) buffer[position + 0]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint x, int r)
        {
            unchecked
            {
                return ((x << r) | (x >> (32 - r)));
            }
        }
#endif
    }
}