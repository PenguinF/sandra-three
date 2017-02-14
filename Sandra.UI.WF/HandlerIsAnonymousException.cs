/*********************************************************************************
 * HandlerIsAnonymousException.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Exception thrown when an event handler is registered on a weak event with a target that is anonymous or otherwise compiler generated.
    /// Such a handler will be garbage collected and never be called. Consider using a named method or not using a weak event.
    /// </summary>
    public class HandlerIsAnonymousException : Exception
    {
        public const string HandlerIsAnonymousExceptionMessage
            = "An event handler is registered on a weak event with a target that is anonymous or otherwise compiler generated. "
            + "Such a handler will be garbage collected and never be called. Consider using a named method or not using a weak event.";
        public const string MethodNameLabel = "Method name: ";
        public const string AnonymousTypeNameLabel = "Anonymous type name: ";
        public const string DeclaringTypeNameLabel = "Declaring type name: ";

        static string getNullableTypeName(Type type) => type != null ? type.FullName : "<null>";

        static IEnumerable<string> getExceptionText(MethodInfo methodInfo)
        {
            yield return HandlerIsAnonymousExceptionMessage;
            yield return string.Empty;
            yield return MethodNameLabel + methodInfo.Name;
            yield return AnonymousTypeNameLabel + methodInfo.DeclaringType.FullName;
            yield return DeclaringTypeNameLabel + getNullableTypeName(methodInfo.DeclaringType.DeclaringType);
        }

        public HandlerIsAnonymousException(MethodInfo methodInfo) : base(string.Join(Environment.NewLine, getExceptionText(methodInfo)))
        {
        }

        public static void ThrowIfCompilerGenerated(MethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any())
            {
                throw new HandlerIsAnonymousException(methodInfo);
            }
        }
    }
}
