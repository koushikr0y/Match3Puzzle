// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;

namespace GameVanilla.Game.Common
{
    public enum MatchType
    {
        Horizontal,
        Vertical,
        TShaped,
        LShaped
    }

    public class Match
    {
        public MatchType type;

        public readonly List<GameObject> tiles = new List<GameObject>();

        public void AddTile(GameObject tile)
        {
            tiles.Add(tile);
        }
    }
}
