#region License
/*********************************************************************************
 * GlyphTabControl.TabPageCollection.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Eutherion.Win.Controls
{
    public partial class GlyphTabControl
    {
        /// <summary>
        /// Represents a collection of tab pages in a <see cref="GlyphTabControl"/>.
        /// </summary>
        public sealed class TabPageCollection : IList<TabPage>
        {
            private readonly List<TabPage> tabs = new List<TabPage>();

            private readonly GlyphTabControl OwnerTabControl;

            internal TabPageCollection(GlyphTabControl ownerTabControl) => OwnerTabControl = ownerTabControl;

            private void InsertTab(int index, TabPage tab)
            {
                tabs.Insert(index, tab);
                OwnerTabControl.TabInserted(tab, index);
            }

            private void RemoveTab(int index) => RemoveTab(index, disposeClientControl: false);

            internal void RemoveTab(int index, bool disposeClientControl)
            {
                TabPage tab = tabs[index];
                tabs.RemoveAt(index);
                OwnerTabControl.TabRemoved(tab, index, disposeClientControl);
            }

            /// <summary>
            /// Gets or sets the tab page at the specified index.
            /// </summary>
            /// <param name="index">
            /// The zero-based index of the tab page to get or set.
            /// </param>
            /// <returns>
            /// The tab page at the specified index.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            /// The new value to set is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is not a valid index in the collection.
            /// </exception>
            public TabPage this[int index]
            {
                get => tabs[index];
                set
                {
                    if (value == null) throw new ArgumentNullException(nameof(value));

                    if (tabs[index] != value)
                    {
                        // Can use the unsafe core methods, tabs[index] will have thrown already.
                        RemoveTab(index);
                        InsertTab(index, value);
                    }
                }
            }

            /// <summary>
            /// Gets the number of tab pages in this collection.
            /// </summary>
            public int Count => tabs.Count;

            /// <summary>
            /// Adds the tab page at the end of the collection.
            /// </summary>
            /// <param name="tab">
            /// The tab page to add.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="tab"/> is null.
            /// </exception>
            public void Add(TabPage tab) => Insert(tabs.Count, tab);

            /// <summary>
            /// Removes all tab pages from the collection. Controls on the tab pages are not disposed.
            /// </summary>
            public void Clear()
            {
                for (int i = tabs.Count; i >= 0; i--) RemoveTab(i);
            }

            /// <summary>
            /// Returns if the tab page is a member of the collection.
            /// </summary>
            /// <param name="tab">
            /// The tab page to locate in the collection.
            /// </param>
            /// <returns>
            /// True if found in the collection; otherwise false.
            /// </returns>
            public bool Contains(TabPage tab) => tabs.Contains(tab);

            /// <summary>
            /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
            /// </summary>
            /// <param name="array">
            /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from this collection.
            /// The array must have zero-based indexing.
            /// </param>
            /// <param name="arrayIndex">
            /// The zero-based index in array at which copying begins.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="array"/> is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="arrayIndex"/> is less than 0.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// The number of elements in this collection is greater than the available space from arrayIndex to the end of the destination array.
            /// </exception>
            public void CopyTo(TabPage[] array, int arrayIndex) => tabs.CopyTo(array, arrayIndex);

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            public IEnumerator<TabPage> GetEnumerator() => tabs.GetEnumerator();

            /// <summary>
            /// Determines the index of a specific tab page in the collection.
            /// </summary>
            /// <param name="tab">
            /// The tab page to locate in the collection.
            /// </param>
            /// <returns>
            /// The index of <paramref name="tab"/> if found in the collection; otherwise, -1.
            /// </returns>
            public int IndexOf(TabPage tab) => tab != null ? tabs.IndexOf(tab) : -1;

            /// <summary>
            /// Inserts a tab page in the collection at the specified index.
            /// </summary>
            /// <param name="index">
            /// The zero-based index at which <paramref name="tab"/> should be inserted.
            /// </param>
            /// <param name="tab">
            /// The tab page to insert into the collection.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="tab"/> is null.
            /// </exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is not a valid index in the collection.
            /// </exception>
            public void Insert(int index, TabPage tab)
            {
                if (tab == null) throw new ArgumentNullException(nameof(tab));
                if (index < 0 || index > tabs.Count) throw new ArgumentOutOfRangeException(nameof(index));
                InsertTab(index, tab);
            }

            /// <summary>
            /// Removes a specific tab page from the collection. The client control on the tab page is not disposed.
            /// </summary>
            /// <param name="tab">
            /// The tab page to remove from the collection.
            /// </param>
            /// <returns>
            /// true if <paramref name="tab"/> was successfully removed from the collection; otherwise, false.
            /// This method also returns false if <paramref name="tab"/> is not found in the original collection.
            /// </returns>
            public bool Remove(TabPage tab)
            {
                int index = IndexOf(tab);
                if (index >= 0)
                {
                    RemoveTab(index);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Removes the tab page at the specified index. The client control on the tab page is not disposed.
            /// </summary>
            /// <param name="index">
            /// The zero-based index of the tab page to remove.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="index"/> is not a valid index in the collection.
            /// </exception>
            public void RemoveAt(int index)
            {
                if (index < 0 || index >= tabs.Count) throw new ArgumentOutOfRangeException(nameof(index));
                RemoveTab(index);
            }

            bool ICollection<TabPage>.IsReadOnly => false;

            IEnumerator IEnumerable.GetEnumerator() => tabs.GetEnumerator();
        }
    }
}
