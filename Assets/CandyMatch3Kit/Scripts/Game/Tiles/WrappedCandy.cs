// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The class used for wrapped candies.
    /// </summary>
    public class WrappedCandy : Candy
    {
        /// <summary>
        /// Returns a list containing all the tiles destroyed when this tile explodes.
        /// </summary>
        /// <returns>A list containing all the tiles destroyed when this tile explodes.</returns>
        public override List<GameObject> Explode()
        {
            var tiles = new List<GameObject>();
            tiles.Add(board.GetTile(x - 1, y - 1));
            tiles.Add(board.GetTile(x, y - 1));
            tiles.Add(board.GetTile(x + 1, y - 1));
            tiles.Add(board.GetTile(x - 1, y));
            tiles.Add(gameObject);
            tiles.Add(board.GetTile(x + 1, y));
            tiles.Add(board.GetTile(x - 1, y + 1));
            tiles.Add(board.GetTile(x, y + 1));
            tiles.Add(board.GetTile(x + 1, y + 1));
            return tiles;
        }

        /// <summary>
        /// Shows the visual effects associated to the explosion of this tile.
        /// </summary>
        /// <param name="pool">The pool to use for the visual effects.</param>
        public override void ShowExplosionFx(FxPool pool)
        {
            base.ShowExplosionFx(pool);

            var explosion = pool.wrappedCandyExplosion.GetObject();
            explosion.transform.position = transform.position;

            SoundManager.instance.PlaySound("CandyWrap");
        }
    }
}
