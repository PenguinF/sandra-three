﻿#region License
/*********************************************************************************
 * EnumArrayTests.cs
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

using System;
using Xunit;

namespace Sandra.Chess.Tests
{
    public class EnumArrayTests
    {
        private void AssertEnumIsIllegal<T>() where T : Enum
        {
            // The beauty of this is that the static constructor is only run when already inside the closure.
            Assert.Throws<TypeInitializationException>(() => EnumIndexedArray<T, int>.New());
        }

        enum _EmptyEnum
        {
        }

        [Fact]
        public void EmptyEnum()
        {
            var array = EnumIndexedArray<_EmptyEnum, int>.New();
            Assert.Equal(0, array.Length);
        }

        enum IllegalEnum2
        {
            MinusOne = -1,
            Zero = 0,
            One = 1,
        }

        [Fact]
        public void EnumWithNegativeValue()
        {
            AssertEnumIsIllegal<IllegalEnum2>();
        }

        enum IllegalEnum3
        {
            MinusOne = -1,
            Zero = 0,
            // To test the specific edge case where the highest value is the number of elements minus one.
            Two = 2,
        }

        [Fact]
        public void EnumWithGaps()
        {
            AssertEnumIsIllegal<IllegalEnum3>();
        }

        enum _EnumWithDuplicates
        {
            A1 = 0, A2 = 0,
            B2 = 1, B1 = 1,
            C = 2,
        }

        [Fact]
        public void EnumWithDuplicates()
        {
            var array = EnumIndexedArray<_EnumWithDuplicates, int>.New();
            Assert.Equal(3, array.Length);
        }
    }
}
