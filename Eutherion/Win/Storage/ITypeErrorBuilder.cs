#region License
/*********************************************************************************
 * ITypeErrorBuilder.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

using Eutherion.Localization;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Contains information to build an error message caused by a value being of a different type than expected.
    /// </summary>

    // The idea of this design is that both ITypeErrorBuilders and actual PTypeErrors both allocate little memory,
    // because:
    //
    // a) There are only very few ITypeErrorBuilder instances, one for each PType, plus a few indexed type error
    //    builders, which in turn are small too.
    // b) PTypeErrors contain only what's necessary to display the error somewhere, i.e. a source position and range,
    //    and most of the time a property key and a value display string.
    // c) No references are kept to any syntax structures, they are only given as arguments to methods of this
    //    interface to transform an error into a meaningful message in various stages.
    //
    // The ITypeErrorBuilder is generated from a typecheck of a single value, regardless of where it occurs in
    // the source. The builder is then used to generate a proper (unlocalized) PTypeError in the context of the source.

    public interface ITypeErrorBuilder
    {
        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        string GetLocalizedTypeErrorMessage(Localizer localizer, string actualValueString);

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <param name="propertyKey">
        /// The property key for which the error occurred.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        string GetLocalizedTypeErrorAtPropertyKeyMessage(Localizer localizer, string actualValueString, string propertyKey);

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <param name="itemIndex">
        /// The index of the array where the error occurred.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        string GetLocalizedTypeErrorAtItemIndexMessage(Localizer localizer, string actualValueString, int itemIndex);
    }
}
