// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// Match detector that detects L-shaped matches.
    /// </summary>
	public class LshapedMatchDetector : MatchDetector
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
                            board.GetTile(i + 2, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j + 1) != null && board.GetTile(i, j + 1).GetComponent<Candy>() != null &&
                            board.GetTile(i, j + 1).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j + 2) != null && board.GetTile(i, j + 2).GetComponent<Candy>() != null &&
                            board.GetTile(i, j + 2).GetComponent<Candy>().color == color)
                        {
                            var match = new Match();
                            match.type = MatchType.LShaped;
                            match.AddTile(board.GetTile(i, j));
                            match.AddTile(board.GetTile(i + 1, j));
                            match.AddTile(board.GetTile(i + 2, j));
                            match.AddTile(board.GetTile(i, j + 1));
                            match.AddTile(board.GetTile(i, j + 2));
                            matches.Add(match);

                            var k = i + 3;
                            while (k < board.level.width && board.GetTile(k, j) != null &&
                                   board.GetTile(k, j).GetComponent<Candy>() != null &&
                                   board.GetTile(k, j).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(k, j));
                                k += 1;
                            }

                            k = j + 3;
                            while (k < board.level.height && board.GetTile(i, k) != null &&
                                   board.GetTile(i, k).GetComponent<Candy>() != null &&
                                   board.GetTile(i, k).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(i, k));
                                k += 1;
                            }
                        }

                        if (board.GetTile(i + 1, j) != null && board.GetTile(i + 1, j).GetComponent<Candy>() != null &&
                            board.GetTile(i + 1, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i + 2, j) != null && board.GetTile(i + 2, j).GetComponent<Candy>() != null &&
                            board.GetTile(i + 2, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j - 1) != null && board.GetTile(i, j - 1).GetComponent<Candy>() != null &&
                            board.GetTile(i, j - 1).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j - 2) != null && board.GetTile(i, j - 2).GetComponent<Candy>() != null &&
                            board.GetTile(i, j - 2).GetComponent<Candy>().color == color)
                        {
                            var match = new Match();
                            match.type = MatchType.LShaped;
                            match.AddTile(board.GetTile(i, j));
                            match.AddTile(board.GetTile(i + 1, j));
                            match.AddTile(board.GetTile(i + 2, j));
                            match.AddTile(board.GetTile(i, j - 1));
                            match.AddTile(board.GetTile(i, j - 2));
                            matches.Add(match);

                            var k = i + 3;
                            while (k < board.level.width && board.GetTile(k, j) != null &&
                                   board.GetTile(k, j).GetComponent<Candy>() != null &&
                                   board.GetTile(k, j).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(k, j));
                                k += 1;
                            }

                            k = j - 3;
                            while (k >= 0 && board.GetTile(i, k) != null && board.GetTile(i, k).GetComponent<Candy>() != null &&
                                   board.GetTile(i, k).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(i, k));
                                k -= 1;
                            }
                        }

                        if (board.GetTile(i - 1, j) != null && board.GetTile(i - 1, j).GetComponent<Candy>() != null &&
                            board.GetTile(i - 1, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i - 2, j) != null && board.GetTile(i - 2, j).GetComponent<Candy>() != null &&
                            board.GetTile(i - 2, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j + 1) != null && board.GetTile(i, j + 1).GetComponent<Candy>() != null &&
                            board.GetTile(i, j + 1).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j + 2) != null && board.GetTile(i, j + 2).GetComponent<Candy>() != null &&
                            board.GetTile(i, j + 2).GetComponent<Candy>().color == color)
                        {
                            var match = new Match();
                            match.type = MatchType.LShaped;
                            match.AddTile(board.GetTile(i, j));
                            match.AddTile(board.GetTile(i - 1, j));
                            match.AddTile(board.GetTile(i - 2, j));
                            match.AddTile(board.GetTile(i, j + 1));
                            match.AddTile(board.GetTile(i, j + 2));
                            matches.Add(match);

                            var k = i - 3;
                            while (k >= 0 && board.GetTile(k, j) != null && board.GetTile(k, j).GetComponent<Candy>() != null &&
                                   board.GetTile(k, j).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(k, j));
                                k -= 1;
                            }

                            k = j + 3;
                            while (k < board.level.height && board.GetTile(i, k) != null &&
                                   board.GetTile(i, k).GetComponent<Candy>() != null &&
                                   board.GetTile(i, k).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(i, k));
                                k += 1;
                            }
                        }

                        if (board.GetTile(i - 1, j) != null && board.GetTile(i - 1, j).GetComponent<Candy>() != null &&
                            board.GetTile(i - 1, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i - 2, j) != null && board.GetTile(i - 2, j).GetComponent<Candy>() != null &&
                            board.GetTile(i - 2, j).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j - 1) != null && board.GetTile(i, j - 1).GetComponent<Candy>() != null &&
                            board.GetTile(i, j - 1).GetComponent<Candy>().color == color &&
                            board.GetTile(i, j - 2) != null && board.GetTile(i, j - 2).GetComponent<Candy>() != null &&
                            board.GetTile(i, j - 2).GetComponent<Candy>().color == color)
                        {
                            var match = new Match();
                            match.type = MatchType.LShaped;
                            match.AddTile(board.GetTile(i, j));
                            match.AddTile(board.GetTile(i - 1, j));
                            match.AddTile(board.GetTile(i - 2, j));
                            match.AddTile(board.GetTile(i, j - 1));
                            match.AddTile(board.GetTile(i, j - 2));
                            matches.Add(match);

                            var k = i - 3;
                            while (k >= 0 && board.GetTile(k, j) != null && board.GetTile(k, j).GetComponent<Candy>() != null &&
                                   board.GetTile(k, j).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(k, j));
                                k -= 1;
                            }

                            k = j - 3;
                            while (k >= 0 && board.GetTile(i, k) != null && board.GetTile(i, k).GetComponent<Candy>() != null &&
                                   board.GetTile(i, k).GetComponent<Candy>().color == color)
                            {
                                match.AddTile(board.GetTile(i, k));
                                k -= 1;
                            }
                        }
                    }

                    i += 1;
                }
            }

            return matches;
		}
	}
}
