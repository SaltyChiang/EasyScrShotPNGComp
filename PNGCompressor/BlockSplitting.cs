﻿/**************************************************************************
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

namespace CompressSharper
{
    /// <summary>
    /// Determines how to split the blocks
    /// </summary>
    public enum BlockSplitting
    {
        /// <summary>
        /// No block splitting
        /// </summary>
        None,

        /// <summary>
        /// Chooses the blocksplit points first, then does iterative LZ77 on each individual block
        /// </summary>
        First,

        /// <summary>
        /// Chooses the blocksplit points last
        /// </summary>
        Last
    }
}