#region License
/*********************************************************************************
 * JsonBackgroundSyntaxVisitor.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonBackgroundSyntaxVisitor
    {
        public virtual void DefaultVisit(GreenJsonBackgroundSyntax node) { }
        public virtual void Visit(GreenJsonBackgroundSyntax node) { if (node != null) node.Accept(this); }
        public virtual void VisitCommentSyntax(GreenJsonCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitUnterminatedMultiLineCommentSyntax(GreenJsonUnterminatedMultiLineCommentSyntax node) => DefaultVisit(node);
        public virtual void VisitWhitespaceSyntax(GreenJsonWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonBackgroundSyntaxVisitor<TResult>
    {
        public virtual TResult DefaultVisit(GreenJsonBackgroundSyntax node) => default;
        public virtual TResult Visit(GreenJsonBackgroundSyntax node) => node == null ? default : node.Accept(this);
        public virtual TResult VisitCommentSyntax(GreenJsonCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitUnterminatedMultiLineCommentSyntax(GreenJsonUnterminatedMultiLineCommentSyntax node) => DefaultVisit(node);
        public virtual TResult VisitWhitespaceSyntax(GreenJsonWhitespaceSyntax node) => DefaultVisit(node);
    }

    /// <summary>
    /// Represents a visitor that visits a single <see cref="GreenJsonBackgroundSyntax"/>.
    /// See also: https://en.wikipedia.org/wiki/Visitor_pattern
    /// </summary>
    public abstract class GreenJsonBackgroundSyntaxVisitor<T, TResult>
    {
        public virtual TResult DefaultVisit(GreenJsonBackgroundSyntax node, T arg) => default;
        public virtual TResult Visit(GreenJsonBackgroundSyntax node, T arg) => node == null ? default : node.Accept(this, arg);
        public virtual TResult VisitCommentSyntax(GreenJsonCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitUnterminatedMultiLineCommentSyntax(GreenJsonUnterminatedMultiLineCommentSyntax node, T arg) => DefaultVisit(node, arg);
        public virtual TResult VisitWhitespaceSyntax(GreenJsonWhitespaceSyntax node, T arg) => DefaultVisit(node, arg);
    }
}
