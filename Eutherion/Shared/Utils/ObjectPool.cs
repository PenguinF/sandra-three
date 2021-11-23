#region License
/*********************************************************************************
 * ObjectPool.cs
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

// Comment this out to disable pooling while e.g. debugging or profiling.
#define POOLING

using System;

namespace Eutherion
{
    /// <summary>
    /// Manages a thread-safe pool of objects.
    /// </summary>
    /// <typeparam name="TPooledObject">
    /// The type of objects to pool.
    /// </typeparam>
    public class ObjectPool<TPooledObject> where TPooledObject : class
    {
        /// <summary>
        /// Holds an object rented from the pool.
        /// Dispose to return to the pool so it can be reused.
        /// </summary>
        public class RentedObject : IDisposable
        {
            /// <summary>
            /// Gets the actual rented object.
            /// </summary>
            public TPooledObject Object { get; }

            internal ObjectPool<TPooledObject> Pool { get; }
            internal int Index { get; }
            internal bool IsRented;

            internal RentedObject(ObjectPool<TPooledObject> pool, TPooledObject @object, int index)
            {
                Pool = pool;
                Object = @object;
                Index = index;

                // No need to finalize.
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Returns the rented object to the pool.
            /// It is strongly recommended to not access the object after it has been disposed.
            /// </summary>
            public void Dispose() => Pool.ReturnRentedObject(this);
        }

        /// <summary>
        /// Gets the function which creates new pooled objects.
        /// </summary>
        public Func<TPooledObject> ObjectConstructor { get; }

#if POOLING
        private readonly object Sentinel = new object();
        private int CreatedObjectCount;
        private int PooledObjectCapacity;
        private int MinimumUnrentedIndex;
        private RentedObject[] PooledObjects;
#endif

        /// <summary>
        /// Initializes a new instance of <see cref="ObjectPool{TPooledObject}"/>.
        /// </summary>
        /// <param name="objectConstructor">
        /// The function which creates new pooled objects.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="objectConstructor"/> is null.
        /// </exception>
        public ObjectPool(Func<TPooledObject> objectConstructor)
        {
            ObjectConstructor = objectConstructor ?? throw new ArgumentNullException(nameof(objectConstructor));

#if POOLING
            PooledObjectCapacity = 64;
            PooledObjects = new RentedObject[PooledObjectCapacity];
#endif
        }

        /// <summary>
        /// Rents an object from the pool. If the pool has no available objects, it creates
        /// a new object by calling <see cref="ObjectConstructor"/>.
        /// </summary>
        /// <returns>
        /// The rented object which must be disposed of to return it to the pool.
        /// </returns>
        public RentedObject Rent()
        {
#if POOLING
            lock (Sentinel)
            {
                // Loop through entries to find an unrented one.
                while (true)
                {
                    if (MinimumUnrentedIndex == CreatedObjectCount)
                    {
                        if (MinimumUnrentedIndex == PooledObjectCapacity)
                        {
                            // Grow the array.
                            int newPooledObjectCapacity = PooledObjectCapacity * 2;
                            RentedObject[] newPooledObjects = new RentedObject[newPooledObjectCapacity];
                            Array.Copy(PooledObjects, newPooledObjects, PooledObjectCapacity);

                            PooledObjectCapacity = newPooledObjectCapacity;
                            PooledObjects = newPooledObjects;
                        }

                        // Create a new object and store at the current index.
                        TPooledObject newObject = ObjectConstructor();
                        RentedObject unrentedObject = new RentedObject(this, newObject, MinimumUnrentedIndex);
                        PooledObjects[MinimumUnrentedIndex] = unrentedObject;
                        MinimumUnrentedIndex++;
                        CreatedObjectCount++;
                        return unrentedObject;
                    }

                    // Reuse pooled object at current index or move onto the next.
                    RentedObject pooledObject = PooledObjects[MinimumUnrentedIndex];

                    if (!pooledObject.IsRented)
                    {
                        pooledObject.IsRented = true;
                        return pooledObject;
                    }

                    MinimumUnrentedIndex++;
                }
            }
#else
            return new RentedObject(this, ObjectConstructor(), 0);
#endif
        }

        private void ReturnRentedObject(RentedObject rentedObject)
        {
#if POOLING
            lock (Sentinel)
            {
                rentedObject.IsRented = false;
                if (rentedObject.Index < MinimumUnrentedIndex)
                {
                    MinimumUnrentedIndex = rentedObject.Index;
                }
            }
#endif
        }
    }
}
