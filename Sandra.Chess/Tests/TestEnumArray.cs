/*********************************************************************************
 * TestEnumArray.cs
 * 
 * Copyright (c) 2004-2016 Henk Nicolai
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
 *********************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sandra.Chess.Tests
{
    [TestClass]
    public class TestEnumArray
    {
        Exception ExpectException(Action testAction)
        {
            try
            {
                testAction();
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }


        enum IllegalEnum
        {
            MinusOne = -1,
            Zero = 0,
            One = 1,
        }

        [TestMethod]
        public void TestIllegalEnum()
        {
            // The beauty of this is that the static constructor is only run when already inside the closure.
            Exception exception = ExpectException(() => EnumIndexedArray<IllegalEnum, int>.New());
            Assert.IsInstanceOfType(exception, typeof(TypeInitializationException));
        }

        enum EnumWithDuplicates
        {
            A1 = 0, A2 = 0,
            B2 = 1, B1 = 1,
            C = 2,
        }

        [TestMethod]
        public void TestEnumWithDuplicates()
        {
            var array = EnumIndexedArray<EnumWithDuplicates, int>.New();
            Assert.AreEqual(array.Length, 3);
        }
    }
}
