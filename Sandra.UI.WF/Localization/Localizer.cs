﻿#region License
/*********************************************************************************
 * Localizer.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
#endregion

using System;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Defines an abstract method to generate a localized string given a <see cref="LocalizedStringKey"/>.
    /// </summary>
    public abstract class Localizer
    {
        /// <summary>
        /// Generates a localized string given a <see cref="LocalizedStringKey"/>.
        /// </summary>
        public abstract string Localize(LocalizedStringKey localizedStringKey);

        private sealed class DefaultLocalizer : Localizer
        {
            public override string Localize(LocalizedStringKey localizedStringKey)
            {
                if (localizedStringKey == null) return null;
                if (localizedStringKey.Key == null) return localizedStringKey.DisplayText;
                return "{" + localizedStringKey.Key + "}";
            }
        }

        private static Localizer current;

        /// <summary>
        /// Gets or sets the current <see cref="Localizer"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The provided new value for <see cref="Current"/> is null.
        /// </exception>
        public static Localizer Current
        {
            get => current;
            set
            {
                if (current != value)
                {
                    current = value ?? throw new ArgumentNullException(nameof(value));
                    event_CurrentChanged.Raise(null, EventArgs.Empty);
                }
            }
        }

        internal static readonly WeakEvent<object, EventArgs> event_CurrentChanged = new WeakEvent<object, EventArgs>();

        internal static event Action<object, EventArgs> CurrentChanged
        {
            add => event_CurrentChanged.AddListener(value);
            remove => event_CurrentChanged.RemoveListener(value);
        }

        /// <summary>
        /// Gets a reference to the default <see cref="Localizer"/>,
        /// which provides a default localized string for each <see cref="LocalizedStringKey"/>.
        /// </summary>
        public static readonly Localizer Default = new DefaultLocalizer();

        static Localizer()
        {
            current = Default;
        }
    }
}
