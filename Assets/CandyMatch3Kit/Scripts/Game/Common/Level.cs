// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The available limit types.
    /// </summary>
    public enum LimitType
    {
        Moves,
        Time
    }

    /// <summary>
    /// This class stores the settings of a game level.
    /// </summary>
    public class Level
    {
        public int id;

        public int width;
        public int height;
        public List<LevelTile> tiles = new List<LevelTile>();

        public LimitType limitType;
        public int limit;

        public List<Goal> goals = new List<Goal>();
        public List<CandyColor> availableColors = new List<CandyColor>();

        public int score1;
        public int score2;
        public int score3;

        public bool awardSpecialCandies;
        public AwardedSpecialCandyType awardedSpecialCandyType;

        public int collectableChance;

        public Dictionary<BoosterType, bool> availableBoosters = new Dictionary<BoosterType, bool>();
    }
}
