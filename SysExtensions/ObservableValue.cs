#region License
/*********************************************************************************
 * ObservableValue.cs
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
using System.Collections.Generic;

namespace SysExtensions
{
    /// <summary>
    /// Contains a value and provides it with an event to observe updates to it.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of the value to observe.
    /// </typeparam>
    public class ObservableValue<TValue>
    {
        private readonly IEqualityComparer<TValue> equalityComparer;

        private TValue value;

        private event Action<TValue> valueChanged;

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableValue{TValue}"/> with a default initial value and default equality comparer.
        /// </summary>
        public ObservableValue() : this(EqualityComparer<TValue>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableValue{TValue}"/> with an initial value and default equality comparer.
        /// </summary>
        public ObservableValue(TValue initialValue) : this(initialValue, EqualityComparer<TValue>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableValue{TValue}"/> with a default initial value and equality comparer.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="equalityComparer"/> is null.
        /// </exception>
        public ObservableValue(IEqualityComparer<TValue> equalityComparer)
        {
            this.equalityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableValue{TValue}"/> with an initial value and equality comparer.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="equalityComparer"/> is null.
        /// </exception>
        public ObservableValue(TValue initialValue, IEqualityComparer<TValue> equalityComparer) : this(equalityComparer)
        {
            value = initialValue;
        }

        /// <summary>
        /// Gets or sets the observed value.
        /// </summary>
        public TValue Value
        {
            get => value;
            set
            {
                if (!equalityComparer.Equals(this.value, value))
                {
                    OnValueChanging(this.value);
                    this.value = value;
                    OnValueChanged(value);
                }
            }
        }

        /// <summary>
        /// Occurs before a change to <see cref="Value"/>.
        /// </summary>
        public event Action<TValue> ValueChanging;

        /// <summary>
        /// Occurs after a change to <see cref="Value"/>.
        /// When a handler is added for this event, it is called immediately.
        /// </summary>
        public event Action<TValue> ValueChanged
        {
            add
            {
                valueChanged += value;
                value(Value);
            }
            remove
            {
                valueChanged -= value;
            }
        }

        /// <summary>
        /// Raises the <see cref="ValueChanging"/> event.
        /// </summary>
        protected virtual void OnValueChanging(TValue oldValue)
        {
            ValueChanging?.Invoke(oldValue);
        }

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        protected virtual void OnValueChanged(TValue newValue)
        {
            valueChanged?.Invoke(newValue);
        }
    }
}
