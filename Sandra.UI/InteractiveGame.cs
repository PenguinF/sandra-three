#region License
/*********************************************************************************
 * InteractiveGame.cs
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

using Eutherion;
using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess.Pgn;

namespace Sandra.UI
{
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    /// <summary>
    /// Encapsulates a chess game which publishes a set of shared <see cref="Eutherion.UIActions.UIAction"/>s.
    /// </summary>
    public partial class InteractiveGame
    {
        public readonly PgnEditor OwnerPgnEditor;
        public readonly Chess.Game Game;

        public InteractiveGame(PgnEditor ownerPgnEditor, Chess.Position initialPosition)
        {
            OwnerPgnEditor = ownerPgnEditor;
            Game = new Chess.Game(initialPosition);
        }

        public void ActiveMoveTreeUpdated()
        {
            if (chessBoardForm != null) chessBoardForm.GameUpdated();
        }

        public void HandleMouseWheelEvent(int delta)
        {
            if (delta > 0)
            {
                (delta / 120).Times(Game.Backward);
                ActiveMoveTreeUpdated();
            }
            else if (delta < 0)
            {
                (-delta / 120).Times(Game.Forward);
                ActiveMoveTreeUpdated();
            }
        }

        StandardChessBoardForm chessBoardForm;

        private static Chess.Variation GetFirstMove(Chess.Variation variation)
        {
            Chess.Variation firstMoveInVariation = variation;
            while (firstMoveInVariation != null && firstMoveInVariation.VariationIndex == 0)
            {
                firstMoveInVariation = firstMoveInVariation.ParentTree.ParentVariation;
            }
            return firstMoveInVariation;
        }
    }
}
