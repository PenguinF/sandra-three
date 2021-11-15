#region License
/*********************************************************************************
 * BitUtilities.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
**********************************************************************************/
#endregion

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Numerics
{
    /// <summary>
    /// Contains utility methods to manipulate vectors of bits.
    /// See also <seealso cref="BitOperations"/>.
    /// </summary>
    public static class BitUtilities
    {
        /// <summary>
        /// Tests if a vector has any set bits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Test(this ulong vector) => vector != 0;

        /// <summary>
        /// Tests if another vector has any bits in common with this one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Test(this ulong vector, ulong otherVector) => (vector & otherVector) != 0;

        /// <summary>
        /// Tests if a vector is equal to zero or a power of two, i.e. is true for zero or one bits exactly.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMaxOneBit(this ulong vector) => !vector.Test(vector - 1);

        /// <summary>
        /// Enumerates all indices for which the corresponding bit in the vector is true.
        /// </summary>
        public static IEnumerable<int> Indices(this ulong vector)
        {
            while (vector != 0)
            {
                // Select the least significant 1-bit using a well known trick.
                ulong oneBit = vector & (0U - vector);
                yield return BitOperations.Log2(oneBit);

                // Zero the least significant 1-bit so the index of the next 1-bit can be yielded.
                vector ^= oneBit;
            }
        }

        /// <summary>
        /// Enumerates all bits in a given vector, i.e. for each set bit a vector with the same set bit and all other bits zeroed out.
        /// </summary>
        public static IEnumerable<ulong> SetBits(this ulong vector)
        {
            while (vector != 0)
            {
                // Select the least significant 1-bit using a well known trick.
                ulong oneBit = vector & (0U - vector);
                yield return oneBit;

                // Zero the least significant 1-bit so the index of the next 1-bit can be yielded.
                vector ^= oneBit;
            }
        }
    }

#if NET47
    /// <summary>
    /// Bridges functions to .NET 4.
    /// </summary>
    public static class BitOperations
    {
        /// <summary>
        /// Gets the index of the single bit of the vector, or an undefined value if the number of set bits in the vector is not equal to one.
        /// </summary>
        public static int Log2(this ulong oneBitVector)
        {
            // Constant masks.
            const ulong m1 = 0x5555555555555555;  // 010101010101...
            const ulong m2 = 0x3333333333333333;  // 001100110011...
            const ulong m4 = 0x0f0f0f0f0f0f0f0f;  // 000011110000...
            const ulong m8 = 0x00ff00ff00ff00ff;
            const ulong m16 = 0x0000ffff0000ffff;
            const ulong m32 = 0x00000000ffffffff;

            // Calculate the index of the single set bit by testing it against several predefined constants.
            // This index is built as a binary value.
            int index = ((oneBitVector & m32) == 0 ? 32 : 0) |
                        ((oneBitVector & m16) == 0 ? 16 : 0) |
                        ((oneBitVector & m8) == 0 ? 8 : 0) |
                        ((oneBitVector & m4) == 0 ? 4 : 0) |
                        ((oneBitVector & m2) == 0 ? 2 : 0) |
                        ((oneBitVector & m1) == 0 ? 1 : 0);

            return index;
        }
    }
#endif
}
