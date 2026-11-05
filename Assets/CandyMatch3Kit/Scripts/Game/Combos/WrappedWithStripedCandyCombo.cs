// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The class used for the wrapped candy + striped candy combo.
    /// </summary>
    public class WrappedWithStripedCandyCombo : Combo
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

            ExplodeRow(board, fxPool, tilesToExplode, y - 1);
            ExplodeRow(board, fxPool, tilesToExplode, y);
            ExplodeRow(board, fxPool, tilesToExplode, y + 1);
            ExplodeColumn(board, fxPool, tilesToExplode, x - 1);
            ExplodeColumn(board, fxPool, tilesToExplode, x);
            ExplodeColumn(board, fxPool, tilesToExplode, x + 1);

            foreach (var tile in tilesToExplode)
            {
                board.ExplodeTileNonRecursive(tile);
            }

            SoundManager.instance.PlaySound("LineVerticalHorizontal");

            board.ApplyGravity();
        }

        /// <summary>
        /// Explodes the specified row.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="fxPool">The pool to use for the visual effects.</param>
        /// <param name="tilesToExplode">The tiles exploded.</param>
        /// <param name="y">The y coordinate.</param>
        private void ExplodeRow(GameBoard board, FxPool fxPool, List<GameObject> tilesToExplode, int y)
        {
            for (var i = 0; i < board.level.width; i++)
            {
                var tile = board.GetTile(i, y);
                if (tile != null)
                {
                    tilesToExplode.Add(tile);

                    var stripes = fxPool.GetStripedCandyExplosionPool(StripeDirection.Horizontal).GetObject();
                    stripes.transform.position = tile.transform.position;
                }
            }
        }

        /// <summary>
        /// Explodes the specified column.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="fxPool">The pool to use for the visual effects.</param>
        /// <param name="tilesToExplode">The tiles exploded.</param>
        /// <param name="x">The x coordinate.</param>
        private void ExplodeColumn(GameBoard board, FxPool fxPool, List<GameObject> tilesToExplode, int x)
        {
            for (var j = 0; j < board.level.height; j++)
            {
                var tile = board.GetTile(x, j);
                if (tile != null)
                {
                    if (!tilesToExplode.Contains(tile))
                    {
                        tilesToExplode.Add(tile);
                    }

                    var stripes = fxPool.GetStripedCandyExplosionPool(StripeDirection.Vertical).GetObject();
                    stripes.transform.position = tile.transform.position;
                }
            }
        }
    }
}
