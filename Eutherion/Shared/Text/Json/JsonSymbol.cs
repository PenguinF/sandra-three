#region License
/*********************************************************************************
 * JsonSymbol.cs
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

using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Text.Json
{
    public abstract class JsonSymbol
    {
        public virtual bool IsBackground => false;
        public virtual bool IsValueStartSymbol => false;
        public virtual IEnumerable<JsonErrorInfo> Errors => Enumerable.Empty<JsonErrorInfo>();

        public abstract void Accept(JsonSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg);
    }
}
