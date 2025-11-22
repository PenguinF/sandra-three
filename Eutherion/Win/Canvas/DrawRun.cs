#region License
/*********************************************************************************
 * DrawRun.cs
 *
 * Copyright (c) 2004-2025 Henk Nicolai
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Eutherion.Win.Canvas
{
    /// <summary>
    /// Represents a single invocation of a <see cref="Control"/> being drawn.
    /// </summary>
    public sealed class DrawRun : IDisposable
    {
        private readonly Dictionary<Color, SolidBrush> SolidBrushes = new Dictionary<Color, SolidBrush>();
        private readonly DisposableResourceCollection DisposableResources = new DisposableResourceCollection();
        private readonly List<ConstrainedClipScope> ClipScopes = new List<ConstrainedClipScope>();

        /// <summary>
        /// Gets the graphics surface to draw on.
        /// </summary>
        public Graphics Graphics { get; }

        private SmoothingMode _SmoothingMode = SmoothingMode.Invalid;

        /// <summary>
        /// Gets or sets the current smoothing mode.
        /// </summary>
        public SmoothingMode SmoothingMode
        {
            get => _SmoothingMode;
            set
            {
                // Wrap this in an equality check to prevent P/Invoke calls if the value doesn't change.
                if (value != _SmoothingMode)
                {
                    Graphics.SmoothingMode = value;
                    _SmoothingMode = value;
                }
            }
        }

        private TextRenderingHint _TextRenderingHint = (TextRenderingHint)(-1);

        /// <summary>
        /// Gets or sets the current text rendering hint.
        /// </summary>
        public TextRenderingHint TextRenderingHint
        {
            get => _TextRenderingHint;
            set
            {
                // Wrap this in an equality check to prevent P/Invoke calls if the value doesn't change.
                if (value != _TextRenderingHint)
                {
                    Graphics.TextRenderingHint = value;
                    _TextRenderingHint = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="DrawRun"/> with a Windows <see cref="System.Drawing.Graphics"/> object.
        /// </summary>
        /// <param name="graphics">
        /// The Windows <see cref="System.Drawing.Graphics"/> object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="graphics"/> is <see langword="null"/>.
        /// </exception>
        public DrawRun(Graphics graphics)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        }

        public SolidBrush GetSolidBrush(Color color) => SolidBrushes.GetOrAdd(color, key =>
        {
            var brush = new SolidBrush(key);
            DisposableResources.Add(brush);
            return brush;
        });

        public ConstrainedClipScope ExcludeClip(Rectangle rect)
        {
            Graphics.ExcludeClip(rect);
            ConstrainedClipScope excludedClipScope = new ConstrainedClipScope(this);
            ClipScopes.Add(excludedClipScope);
            return excludedClipScope;
        }

        internal void ResetClip(ConstrainedClipScope excludedClipScope)
        {
            for (int i = ClipScopes.Count - 1; i >= 0; i--)
            {
                if (excludedClipScope == ClipScopes[i])
                {
                    ClipScopes.RemoveAt(i);
                    if (ClipScopes.Count == 0)
                    {
                        Graphics.ResetClip();
                    }
                    break;
                }
            }
        }

        public void Dispose()
        {
            DisposableResources.Dispose();
            GC.SuppressFinalize(this);
        }

        ~DrawRun() { }
    }
}
