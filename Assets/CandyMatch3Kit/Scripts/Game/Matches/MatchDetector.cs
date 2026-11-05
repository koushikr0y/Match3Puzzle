// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace GameVanilla.Game.Common
{
	/// <summary>
	/// The base class of match detectors.
	/// </summary>
	public abstract class MatchDetector
	{
		/// <summary>
		/// Returns the list of detected matches.
		/// </summary>
		/// <param name="board">The game board.</param>
		/// <returns>The list of detected matches.</returns>
		public abstract List<Match> DetectMatches(GameBoard board);
	}
}
