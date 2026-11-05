// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The class used for the color bomb + color bomb combo.
    /// </summary>
    public class TwoColorBombCombo : ColorBombCombo
    {
        /// <summary>
        /// Resolves this combo.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="tiles">The tiles destroyed by the combo.</param>
        /// <param name="fxPool">The pool to use for the visual effects.</param>
        public override void Resolve(GameBoard board, List<GameObject> tiles, FxPool fxPool)
        {
            foreach (var tile in tiles)
            {
                if (tile != null && (tile.GetComponent<Candy>() != null || tile.GetComponent<ColorBomb>() != null))
                {
                    board.ExplodeTileNonRecursive(tile);
                }
            }

            SoundManager.instance.PlaySound("ColorBomb");

            board.ApplyGravity();
        }
    }
}
