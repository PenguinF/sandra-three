#region License
/*********************************************************************************
 * CircularBufferTests.cs
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
using System.Collections.Generic;
using System.Collections.Specialized;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class CircularBufferTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void MaxCapacityZeroOrLessThrows(int maxCapacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(maxCapacity));

            // This should not throw.
            var buffer = new CircularBuffer<int>(1);

            // But this should.
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer.MaximumCapacity = maxCapacity);
        }

        [Fact]
        public void UnchangedElements()
        {
            var buffer = new CircularBuffer<Box<Box<int>>>(3);
            var box1 = new Box<int>(0);
            var box2 = new Box<int>(0);

            // Add in opposite order. See also OrderOfAddedElements.
            buffer.Add(new Box<Box<int>>(box2));
            buffer.Add(new Box<Box<int>>(box1));
            Assert.Collection(buffer,
                element1 => Assert.Same(element1.Value, box1),
                element2 => Assert.Same(element2.Value, box2));
        }

        public static IEnumerable<object[]> MaxCapacityItemsToAddCombinations()
        {
            yield return new object[] { 1, 0 };
            yield return new object[] { 1, 1 };
            yield return new object[] { 1, 2 };
            yield return new object[] { 3, 0 };
            yield return new object[] { 3, 1 };
            yield return new object[] { 3, 2 };
            yield return new object[] { 3, 3 };
            yield return new object[] { 3, 4 };
            yield return new object[] { 99, 5 };
            yield return new object[] { 5, 99 };
        }

        [Theory]
        [MemberData(nameof(MaxCapacityItemsToAddCombinations))]
        public void BufferBehavesLikeReadOnlyList(int maxCapacity, int itemsToAdd)
        {
            // Set up buffer.
            var buffer = new CircularBuffer<int>(maxCapacity);
            var expectedCount = itemsToAdd;
            if (expectedCount > maxCapacity) expectedCount = maxCapacity;
            int counter = 0;
            itemsToAdd.Times(() => buffer.Add(counter++));

            // Do not use Assert.Collection, which may make assumptions based on the data type of
            // its argument. Instead, test IReadOnlyList members explicitly.
            Assert.Equal(expectedCount, buffer.Count);

            // Assert that elements are enumerated from 0 to Count - 1.
            int i = 0;
            foreach (var bufferValue in buffer)
            {
                Assert.Equal(buffer[i], bufferValue);
                i++;
            }
            Assert.Equal(expectedCount, i);
        }

        [Theory]
        [MemberData(nameof(MaxCapacityItemsToAddCombinations))]
        public void OrderOfElements(int maxCapacity, int itemsToAdd)
        {
            // Tests that items are indexed in order of newest to oldest,
            // i.e. buffer[0] always returns the last added item.
            var buffer = new CircularBuffer<int>(maxCapacity);
            var expectedCount = itemsToAdd;
            if (expectedCount > maxCapacity) expectedCount = maxCapacity;
            int counter = 0;
            itemsToAdd.Times(() => buffer.Add(counter++));

            // Do not use Assert.Collection, which may make assumptions based on the data type of
            // its argument. Instead, enumerate explicitly.
            int i = itemsToAdd - 1;
            foreach (var bufferValue in buffer)
            {
                Assert.Equal(i, bufferValue);
                i--;
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        public void DecreaseMaxCapacity(int itemsToAdd)
        {
            const int initialMaxCapacity = 7;
            const int decreasedMaxCapacity = 3;

            // Set up buffer.
            var buffer = new CircularBuffer<int>(initialMaxCapacity);
            int counter = 0;
            itemsToAdd.Times(() => buffer.Add(counter++));

            // Decrease capacity.
            buffer.MaximumCapacity = decreasedMaxCapacity;
            var expectedCount = itemsToAdd;
            if (expectedCount > decreasedMaxCapacity) expectedCount = decreasedMaxCapacity;

            // Do not use Assert.Collection, which may make assumptions based on the data type of
            // its argument. Instead, enumerate explicitly.
            Assert.Equal(expectedCount, buffer.Count);

            int i = itemsToAdd - 1;
            foreach (var bufferValue in buffer)
            {
                Assert.Equal(i, bufferValue);
                i--;
            }
        }
    }
}
