#region License
/*********************************************************************************
 * PType.List.cs
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

using Eutherion.Utils;

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        private static readonly PTypeErrorBuilder TupleItemTypeMismatchError
            = new PTypeErrorBuilder(PTypeErrorBuilder.TupleItemTypeMismatchError);

        public sealed class TupleType<T1, T2> : PType<(T1, T2)>
        {
            public (PType<T1>, PType<T2>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, (T1, T2)> TryGetValidValue(PValue value)
            {
                if (value is PList list
                    && list.Count == 5
                    && ItemTypes.Item1.TryGetValidValue(list[0]).IsOption2(out T1 value1)
                    && ItemTypes.Item2.TryGetValidValue(list[1]).IsOption2(out T2 value2))
                {
                    return (value1, value2);
                }

                return InvalidValue(TupleItemTypeMismatchError);
            }

            public override Maybe<(T1, T2)> TryConvert(PValue value)
            {
                if (value is PList list
                    && list.Count == 5
                    && ItemTypes.Item1.TryConvert(list[0]).IsJust(out T1 value1)
                    && ItemTypes.Item2.TryConvert(list[1]).IsJust(out T2 value2))
                {
                    return (value1, value2);
                }

                return Maybe<(T1, T2)>.Nothing;
            }

            public override PValue GetPValue((T1, T2) value)
            {
                var (value1, value2) = value;
                return new PList(new[]
                {
                    ItemTypes.Item1.GetPValue(value1),
                    ItemTypes.Item2.GetPValue(value2),
                });
            }
        }

        public sealed class TupleType<T1, T2, T3, T4, T5> : PType<(T1, T2, T3, T4, T5)>
        {
            public (PType<T1>, PType<T2>, PType<T3>, PType<T4>, PType<T5>) ItemTypes { get; }

            public TupleType((PType<T1>, PType<T2>, PType<T3>, PType<T4>, PType<T5>) itemTypes)
                => ItemTypes = itemTypes;

            internal override Union<ITypeErrorBuilder, (T1, T2, T3, T4, T5)> TryGetValidValue(PValue value)
            {
                if (value is PList list
                    && list.Count == 5
                    && ItemTypes.Item1.TryGetValidValue(list[0]).IsOption2(out T1 value1)
                    && ItemTypes.Item2.TryGetValidValue(list[1]).IsOption2(out T2 value2)
                    && ItemTypes.Item3.TryGetValidValue(list[2]).IsOption2(out T3 value3)
                    && ItemTypes.Item4.TryGetValidValue(list[3]).IsOption2(out T4 value4)
                    && ItemTypes.Item5.TryGetValidValue(list[4]).IsOption2(out T5 value5))
                {
                    return (value1, value2, value3, value4, value5);
                }

                return InvalidValue(TupleItemTypeMismatchError);
            }

            public override Maybe<(T1, T2, T3, T4, T5)> TryConvert(PValue value)
            {
                if (value is PList list
                    && list.Count == 5
                    && ItemTypes.Item1.TryConvert(list[0]).IsJust(out T1 value1)
                    && ItemTypes.Item2.TryConvert(list[1]).IsJust(out T2 value2)
                    && ItemTypes.Item3.TryConvert(list[2]).IsJust(out T3 value3)
                    && ItemTypes.Item4.TryConvert(list[3]).IsJust(out T4 value4)
                    && ItemTypes.Item5.TryConvert(list[4]).IsJust(out T5 value5))
                {
                    return (value1, value2, value3, value4, value5);
                }

                return Maybe<(T1, T2, T3, T4, T5)>.Nothing;
            }

            public override PValue GetPValue((T1, T2, T3, T4, T5) value)
            {
                var (value1, value2, value3, value4, value5) = value;
                return new PList(new[]
                {
                    ItemTypes.Item1.GetPValue(value1),
                    ItemTypes.Item2.GetPValue(value2),
                    ItemTypes.Item3.GetPValue(value3),
                    ItemTypes.Item4.GetPValue(value4),
                    ItemTypes.Item5.GetPValue(value5),
                });
            }
        }
    }
}
