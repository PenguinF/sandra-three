#region License
/*********************************************************************************
 * SafeLazyObject.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
using System.Threading;

namespace Eutherion.Utils
{
    /// <summary>
    /// Represents the thread-safe lazy evaluation of a parameterless constructor which creates an object.
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of object to create.
    /// </typeparam>
    public struct SafeLazyObject<TObject> where TObject : class
    {
        private volatile Func<TObject> Constructor;
        private TObject ConstructedObject;

        /// <summary>
        /// Initializes a new instance of <see cref="SafeLazyObject{TValue}"/>.
        /// </summary>
        /// <param name="constructor">
        /// The object constructor.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="constructor"/> is null.
        /// </exception>
        public SafeLazyObject(Func<TObject> constructor)
        {
            Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            ConstructedObject = null;
        }

        /// <summary>
        /// Gets the result of the constructor.
        /// Note that if multiple threads race to construct the object, they will all call the constructor.
        /// Only one of the created objects is kept, and henceforth returned.
        /// </summary>
        public TObject Object
        {
            get
            {
                // Avoid calling the constructor and CompareExchange() if ConstructedObject is already available.
                if (ConstructedObject == null)
                {
                    // Create a local copy of Constructor, so that if another thread succeeds in setting ConstructedObject 
                    // and also resetting the Constructor field before this thread reaches the CompareExchange statement,
                    // it will not generate a NullReferenceException. The object created on this thread is then discarded.
                    Func<TObject> constructor = Constructor;

                    // Perform CompareExchange on the ConstructedObject rather than the constructor, otherwise threads
                    // that lost the race might return a null ConstructedObject while the winning thread is still busy
                    // constructing it.
                    if (constructor != null && Interlocked.CompareExchange(ref ConstructedObject, constructor(), null) == null)
                    {
                        // If Constructor() threw, then it will be re-evaluated by the next caller, and it will likely throw again.
                        // This releases any references the constructor may have held implicitly if it was a closure.
                        Constructor = null;
                    }
                }

                return ConstructedObject;
            }
        }
    }
}
