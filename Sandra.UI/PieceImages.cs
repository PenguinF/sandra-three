#region License
/*********************************************************************************
 * PieceImages.cs
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

using Eutherion.Utils;
using Eutherion.Win.AppTemplate;
using Sandra.Chess;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Sandra.UI
{
    internal static class PieceImages
    {
        public static EnumIndexedArray<ColoredPiece, Image> ImageArray { get; private set; }

        private static string RuntimePath(string imageFileKey)
            => Path.Combine(Session.ExecutableFolder, "Images", imageFileKey + ".png");

        private static Bitmap DefaultResourceImage(string imageFileKey)
            => (Bitmap)Properties.Resources.ResourceManager.GetObject(imageFileKey, Properties.Resources.Culture);

        private static Image LoadChessPieceImage(string imageFileKey)
        {
            try
            {
                return Image.FromFile(RuntimePath(imageFileKey));
            }
            catch
            {
                return DefaultResourceImage(imageFileKey);
            }
        }

        public static void LoadChessPieceImages()
        {
            var array = EnumIndexedArray<ColoredPiece, Image>.New();
            array[ColoredPiece.BlackPawn] = LoadChessPieceImage("bp");
            array[ColoredPiece.BlackKnight] = LoadChessPieceImage("bn");
            array[ColoredPiece.BlackBishop] = LoadChessPieceImage("bb");
            array[ColoredPiece.BlackRook] = LoadChessPieceImage("br");
            array[ColoredPiece.BlackQueen] = LoadChessPieceImage("bq");
            array[ColoredPiece.BlackKing] = LoadChessPieceImage("bk");
            array[ColoredPiece.WhitePawn] = LoadChessPieceImage("wp");
            array[ColoredPiece.WhiteKnight] = LoadChessPieceImage("wn");
            array[ColoredPiece.WhiteBishop] = LoadChessPieceImage("wb");
            array[ColoredPiece.WhiteRook] = LoadChessPieceImage("wr");
            array[ColoredPiece.WhiteQueen] = LoadChessPieceImage("wq");
            array[ColoredPiece.WhiteKing] = LoadChessPieceImage("wk");
            ImageArray = array;
        }

#if DEBUG
        private static void DeployRuntimePieceImage(string imageFileKey)
        {
            var runtimePath = RuntimePath(imageFileKey);
            Directory.CreateDirectory(Path.GetDirectoryName(runtimePath));

            try
            {
                DefaultResourceImage(imageFileKey).Save(runtimePath);
            }
            catch
            {
                // Likely the image file has been locked.
            }
        }

        /// <summary>
        /// Deploys piece images to the Images folder.
        /// </summary>
        public static void DeployRuntimePieceImageFiles()
        {
            new[] { "bp", "bn", "bb", "br", "bq", "bk",
                    "wp", "wn", "wb", "wr", "wq", "wk",
            }.ForEach(DeployRuntimePieceImage);
        }
#endif
    }
}
