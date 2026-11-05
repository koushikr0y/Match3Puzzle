// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The class used for the wrapped candy + wrapped candy combo.
    /// </summary>
    public class TwoWrappedCandyCombo : Combo
    {
        /// <summary>
        /// Resolves this combo.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="tiles">The tiles destroyed by the combo.</param>
        /// <param name="fxPool">The pool to use for the visual effects.</param>
        public override void Resolve(GameBoard board, List<GameObject> tiles, FxPool fxPool)
        {
            var x = tileB.x;
            var y = tileB.y;
            var tilesToExplode = new List<GameObject>();

            tilesToExplode.Add(board.GetTile(x - 2, y - 2));
            tilesToExplode.Add(board.GetTile(x - 1, y - 2));
            tilesToExplode.Add(board.GetTile(x, y - 2));
            tilesToExplode.Add(board.GetTile(x + 1, y - 2));
            tilesToExplode.Add(board.GetTile(x + 2, y - 2));

            tilesToExplode.Add(board.GetTile(x - 2, y - 1));
            tilesToExplode.Add(board.GetTile(x - 1, y - 1));
            tilesToExplode.Add(board.GetTile(x, y - 1));
            tilesToExplode.Add(board.GetTile(x + 1, y - 1));
            tilesToExplode.Add(board.GetTile(x + 2, y - 1));

            tilesToExplode.Add(board.GetTile(x - 2, y));
            tilesToExplode.Add(board.GetTile(x - 1, y));
            tilesToExplode.Add(board.GetTile(x, y));
            tilesToExplode.Add(board.GetTile(x + 1, y));
            tilesToExplode.Add(board.GetTile(x + 2, y));

            tilesToExplode.Add(board.GetTile(x - 2, y + 1));
            tilesToExplode.Add(board.GetTile(x - 1, y + 1));
            tilesToExplode.Add(board.GetTile(x, y + 1));
            tilesToExplode.Add(board.GetTile(x + 1, y + 1));
            tilesToExplode.Add(board.GetTile(x + 2, y + 1));

            tilesToExplode.Add(board.GetTile(x - 2, y + 2));
            tilesToExplode.Add(board.GetTile(x - 1, y + 2));
            tilesToExplode.Add(board.GetTile(x, y + 2));
            tilesToExplode.Add(board.GetTile(x + 1, y + 2));
            tilesToExplode.Add(board.GetTile(x + 2, y + 2));

            foreach (var tile in tilesToExplode)
            {
                board.ExplodeTileNonRecursive(tile);
            }

            var explosion = fxPool.wrappedCandyExplosion.GetObject();
            explosion.transform.position = tileB.transform.position;

            SoundManager.instance.PlaySound("CandyWrap");

            board.ApplyGravity();
        }
    }
}
