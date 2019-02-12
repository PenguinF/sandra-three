#region License
/*********************************************************************************
 * WeakEventInvocationException.cs
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

using System;
using System.Collections;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents the exception thrown from the invocation of a weak event.
    /// It preserves the characteristics of the exception which was unhandled in the weak event handler, in particular the stack trace.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Summaries of this class are copied from <see cref="Exception"/> so Intellisense works correctly on this type of exception too.
    /// </remarks>
    public sealed class WeakEventInvocationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeakEventInvocationException"/> class.
        /// </summary>
        public WeakEventInvocationException(Exception innerException) : base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        /// <returns>
        /// An object that implements the <see cref="IDictionary"/> interface and contains a collection of user-defined key/value pairs.
        /// The default is an empty collection.
        /// </returns>
        public override IDictionary Data => InnerException.Data;

        /// <summary>
        /// When overridden in a derived class, returns the <see cref="Exception"/> that is the root cause of one or more subsequent exceptions.
        /// </summary>
        /// <returns>
        /// The first exception thrown in a chain of exceptions. If the <see cref="Exception.InnerException"/>
        /// property of the current exception is a null reference (Nothing in Visual Basic), this property returns the current exception.
        /// </returns>
        public override Exception GetBaseException() => InnerException.GetBaseException();

        /// <summary>
        /// Gets or sets a link to the help file associated with this exception.
        /// </summary>
        /// <returns>
        /// The Uniform Resource Name (URN) or Uniform Resource Locator (URL).
        /// </returns>
        public override string HelpLink
        {
            get => InnerException.HelpLink;
            set => InnerException.HelpLink = value;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string ("").
        /// </returns>
        public override string Message => InnerException.Message;

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        /// <returns>
        /// The name of the application or the object that causes the error.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The object must be a runtime <see cref="System.Reflection"/> object.
        /// </exception>
        public override string Source
        {
            get => InnerException.Source;
            set => InnerException.Source = value;
        }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        /// <returns>
        /// A string that describes the immediate frames of the call stack.
        /// </returns>

        // Glue stack trace of inner exception and this exception together.
        public override string StackTrace => MergeWithStackTrace(InnerException.StackTrace);

        // Override base.ToString() for the unhandled exception dialog.
        public override string ToString() => MergeWithStackTrace(InnerException.ToString());

        string MergeWithStackTrace(string baseString)
        {
            string baseStackTrace = base.StackTrace;
            if (string.IsNullOrEmpty(baseString)) return baseStackTrace;
            if (string.IsNullOrEmpty(baseStackTrace)) return baseString;
            return string.Concat(baseString, Environment.NewLine, baseStackTrace);
        }
    }
}
