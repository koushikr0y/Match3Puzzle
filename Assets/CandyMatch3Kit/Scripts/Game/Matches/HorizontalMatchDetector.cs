// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// Match detector that detects horizontal matches.
    /// </summary>
	public class HorizontalMatchDetector : MatchDetector
	{
		/// <summary>
		/// Returns the list of detected matches.
		/// </summary>
		/// <param name="board">The game board.</param>
		/// <returns>The list of detected matches.</returns>
		public override List<Match> DetectMatches(GameBoard board)
		{
            var matches = new List<Match>();

            for (var j = 0; j < board.level.height; j++)
            {
                for (var i = 0; i < board.level.width - 2;)
                {
                    var tile = board.GetTile(i, j);
                    if (tile != null && tile.GetComponent<Candy>() != null)
                    {
                        var color = tile.GetComponent<Candy>().color;
                        if (board.GetTile(i + 1, j) != null && board.GetTile(i + 1, j).GetComponent<Candy>() != null &&
                            board.GetTile(i + 1, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i + 2, j) != null && board.GetTile(i + 2, j).GetComponent<Candy>() != null &&
                            board.GetTile(i + 2, j).GetComponent<Candy>().color == color)
                        {
                            var match = new Match();
                            match.type = MatchType.Horizontal;
                            do
                            {
                                match.AddTile(board.GetTile(i, j));
                                i += 1;
                            } while (i < board.level.width && board.GetTile(i, j) != null &&
                                     board.GetTile(i, j).GetComponent<Candy>() != null &&
                                     board.GetTile(i, j).GetComponent<Candy>().color == color);

                            matches.Add(match);
                            continue;
                        }
                    }

                    i += 1;
                }
            }

            return matches;
		}
	}
}
