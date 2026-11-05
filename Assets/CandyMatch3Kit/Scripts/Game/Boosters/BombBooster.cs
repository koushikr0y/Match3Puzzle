// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

namespace GameVanilla.Game.Common
{
	/// <summary>
	/// The class that represents the bomb booster.
	/// </summary>
	public class BombBooster : Booster
	{
        /// <summary>
        /// Resolves this booster.
        /// </summary>
        /// <param name="board">The game board.</param>
        /// <param name="tile">The tile in which to apply the booster.</param>
		public override void Resolve(GameBoard board, GameObject tile)
		{
            var tiles = new List<GameObject>();
			var x = tile.GetComponent<Tile>().x;
			var y = tile.GetComponent<Tile>().y;
            tiles.Add(board.GetTile(x - 1, y - 1));
            tiles.Add(board.GetTile(x, y - 1));
            tiles.Add(board.GetTile(x + 1, y - 1));
            tiles.Add(board.GetTile(x - 1, y));
            tiles.Add(tile);
            tiles.Add(board.GetTile(x + 1, y));
            tiles.Add(board.GetTile(x - 1, y + 1));
            tiles.Add(board.GetTile(x, y + 1));
            tiles.Add(board.GetTile(x + 1, y + 1));

			foreach (var t in tiles)
			{
				if (t != null && t.GetComponent<Tile>().destructable)
				{
					board.ExplodeTile(t);
				}
			}
		}
	}
}
