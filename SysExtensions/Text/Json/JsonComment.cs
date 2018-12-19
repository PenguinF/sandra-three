#region License
/*********************************************************************************
 * JsonComment.cs
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
    public class JsonComment : JsonSymbol
    {
        public const char CommentStartFirstCharacter = '/';
        public const char SingleLineCommentStartSecondCharacter = '/';
        public const char MultiLineCommentStartSecondCharacter = '*';

        public static readonly string SingleLineCommentStart
            = new string(new[] { CommentStartFirstCharacter, SingleLineCommentStartSecondCharacter });

        public static readonly JsonComment Value = new JsonComment();

        private JsonComment() { }

        public override bool IsBackground => true;

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitComment(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitComment(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitComment(this, arg);
    }
}
