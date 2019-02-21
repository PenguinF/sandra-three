#region License
/*********************************************************************************
 * KeyUIActionMapping.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

using Eutherion.UIActions;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Represents a mapping between a shortcut key and a <see cref="UIAction"/>.
    /// </summary>
    public struct KeyUIActionMapping
    {
        public KeyUIActionMapping(ShortcutKeys shortcut, UIAction action)
        {
            Shortcut = shortcut;
            Action = action;
        }

        /// <summary>
        /// Gets the shortcut key for this mapping.
        /// </summary>
        public ShortcutKeys Shortcut { get; }

        /// <summary>
        /// Gets the <see cref="UIAction"/> for this mapping.
        /// </summary>
        public UIAction Action { get; }
    }
}
