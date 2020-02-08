#region License
/*********************************************************************************
 * IJsonForegroundSymbol.cs
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
    /// Denotes any terminal json symbol that is not treated as background, such as comments or whitespace.
    /// </summary>
    public interface IJsonForegroundSymbol : IGreenJsonSymbol
    {
        bool IsValueStartSymbol { get; }
        bool HasErrors { get; }

        void Accept(JsonForegroundSymbolVisitor visitor);
        TResult Accept<TResult>(JsonForegroundSymbolVisitor<TResult> visitor);
        TResult Accept<T, TResult>(JsonForegroundSymbolVisitor<T, TResult> visitor, T arg);
    }
}
