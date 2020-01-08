#region License
/*********************************************************************************
 * ConstantImageProvider.cs
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
using System.Drawing;

namespace Eutherion.Win
{
    /// <summary>
    /// <see cref="IImageProvider"/> which always provides the same image to a UI element.
    /// </summary>
    public class ConstantImageProvider : IImageProvider
    {
        /// <summary>
        /// Gets the <see cref="ConstantImageProvider"/> which returns a null <see cref="System.Drawing.Image"/>.
        /// </summary>
        public static readonly ConstantImageProvider Empty = new ConstantImageProvider();

        /// <summary>
        /// Gets the image from this image provider.
        /// </summary>
        public Image Image { get; }

        private ConstantImageProvider() { }

        /// <summary>
        /// Initializes a new instance of <see cref="ConstantImageProvider"/> with a specified image.
        /// </summary>
        /// <param name="image">
        /// The image for the <see cref="ConstantImageProvider"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="image"/> is null.
        /// </exception>
        public ConstantImageProvider(Image image)
            => Image = image ?? throw new ArgumentNullException(nameof(image));

        Image IImageProvider.GetImage() => Image;
    }
}
