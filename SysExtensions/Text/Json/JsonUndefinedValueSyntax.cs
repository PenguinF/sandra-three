﻿#region License
/*********************************************************************************
 * JsonUndefinedValueSyntax.cs
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

namespace SysExtensions.Text.Json
{
    /// <summary>
    /// Represents a literal syntax node with an undefined or unsupported value.
    /// </summary>
    public sealed class JsonUndefinedValueSyntax : JsonSyntaxNode
    {
        public JsonUndefinedValueSyntax()
        {
        }

        public override void Accept(JsonSyntaxNodeVisitor visitor) => visitor.VisitUndefinedValueSyntax(this);
        public override TResult Accept<TResult>(JsonSyntaxNodeVisitor<TResult> visitor) => visitor.VisitUndefinedValueSyntax(this);
        public override TResult Accept<T, TResult>(JsonSyntaxNodeVisitor<T, TResult> visitor, T arg) => visitor.VisitUndefinedValueSyntax(this, arg);
    }
}
