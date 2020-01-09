#region License
/*********************************************************************************
 * WeakEvent.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eutherion.Utils
{
    /// <summary>
    /// Defines an event on objects that have a longer lifetime than the controls that subscribe on them.
    /// A <see cref="WeakEvent"/> keeps a weak instead of a strong reference to any event handler that is registered to it with an underlying target.
    /// Event handlers that are closures are not supported, since they go out of scope when the method in which they are declared exits,
    /// and therefore are eligible for garbage collection. A <see cref="HandlerIsAnonymousException"/> is thrown at runtime when such a handler is registered.
    /// If an event handler is static, it is still strongly referenced.
    /// </summary>
    /// <typeparam name="TSender">
    /// The type of the sender of the <see cref="WeakEvent"/>.
    /// </typeparam>
    /// <typeparam name="TEventArgs">
    /// The type of the event arguments of the <see cref="WeakEvent"/>.
    /// </typeparam>
    public sealed class WeakEvent<TSender, TEventArgs> where TEventArgs : EventArgs
    {
        /// <summary>
        /// Wrapper around an event handler that maintains a weak reference to a target instance, or just a strong reference to a static method if there is no target.
        /// </summary>
        private sealed class HandlerRef
        {
            // Null if no target (so a static event handler).
            readonly WeakReference wr;
            internal readonly MethodInfo MethodInfo;
            internal bool Invalid;

            internal HandlerRef(object target, MethodInfo methodInfo)
            {
                if (target != null) wr = new WeakReference(target);
                MethodInfo = methodInfo;
            }

            bool IsInvalid(object target)
                // Null or disposed controls are invalid.
                => target == null
                || target is IWeakEventTarget weakEventTarget && weakEventTarget.IsDisposed;

            internal void IfValidTarget(Action<object> targetAction)
            {
                if (Invalid) return;

                object target;
                if (wr == null)
                {
                    // The handler is a static method, which is valid, but there is no target object to bind to.
                    target = null;
                }
                else
                {
                    // Instance method, check if the target is still alive.
                    target = wr.Target;
                    if (IsInvalid(target))
                    {
                        Invalid = true;
                        return;
                    }
                }

                // 'target' is now strongly referenced, so will not be garbage collected until this method exits.
                targetAction(target);
            }
        }

        List<HandlerRef> wrappers = new List<HandlerRef>();

        void Purge()
        {
            // Purge by copying all valid wrappers to a new list and forget the old wrapper list.
            wrappers = new List<HandlerRef>(wrappers.Where(x => !x.Invalid));
        }

        /// <summary>
        /// Adds a handler to the event. This is equivalent to the statement:
        /// <code>myEvent += <paramref name="handler"/>;</code>
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler"/> is null.
        /// </exception>
        public void AddListener(Action<TSender, TEventArgs> handler) => AddListenerInner(handler);

        private void AddListenerInner(Delegate handler)
        {
            if (null == handler) throw new ArgumentNullException(nameof(handler));

            // Distinguish between static and instance methods by examining the target of the handler.
            object target = handler.Target;

            if (null != target)
            {
                HandlerIsAnonymousException.ThrowIfCompilerGenerated(handler.Method);
            }

            wrappers.Add(new HandlerRef(target, handler.Method));
        }

        /// <summary>
        /// Removes a handler from the event. This is equivalent to the statement:
        /// <code>myEvent -= <param name="handler">handler</param>;</code>
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="handler"/> is null.
        /// </exception>
        public void RemoveListener(Action<TSender, TEventArgs> handler) => RemoveListenerInner(handler);

        private void RemoveListenerInner(Delegate handler)
        {
            if (null == handler) throw new ArgumentNullException(nameof(handler));

            bool purgeNeeded = false;

            foreach (HandlerRef wrapper in wrappers)
            {
                wrapper.IfValidTarget(target =>
                {
                    // Use ReferenceEquals, because if the targets are different objects but are still equal to each other, the handler must not be removed.
                    if (ReferenceEquals(target, handler.Target) && wrapper.MethodInfo == handler.Method)
                    {
                        wrapper.Invalid = true;
                    }
                });

                purgeNeeded |= wrapper.Invalid;
            }

            if (purgeNeeded) Purge();
        }

        /// <summary>
        /// Raises the event. This is equivalent to the statement:
        /// <code>myEvent?.Invoke(<param name="sender">sender</param>, <param name="e">e</param>);</code>
        /// </summary>
        public void Raise(TSender sender, TEventArgs e)
        {
            // Build invocation list, and purge when needed.
            Action<TSender, TEventArgs> invocationList = null;
            bool purgeNeeded = false;
            foreach (HandlerRef wrapper in wrappers)
            {
                wrapper.IfValidTarget(target =>
                {
                    // Here 'target' is null or a strong reference, until invocationList goes out of scope.
                    invocationList += (__sender, __e) =>
                    {
                        // Bind the method info to the target at runtime and invoke it.
                        try
                        {
                            wrapper.MethodInfo.Invoke(target, new object[] { __sender, __e });
                        }
                        catch (TargetInvocationException exc)
                        {
                            // Preserve stack trace of the inner exception.
                            if (exc.InnerException != null) throw new WeakEventInvocationException(exc.InnerException);
                            throw;
                        }
                    };
                });

                purgeNeeded |= wrapper.Invalid;
            }

            // Purge the list.
            if (purgeNeeded) Purge();

            // Fire the event.
            invocationList?.Invoke(sender, e);
        }
    }

    /// <summary>
    /// Provides a means for a <see cref="WeakEvent{TSender, TEventArgs}"/>
    /// to detect if a target can be purged before it is garbage collected.
    /// </summary>
    public interface IWeakEventTarget
    {
        bool IsDisposed { get; }
    }
}
