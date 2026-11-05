// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// Utility type to represent a level tile, identified by its x and y coordinates.
    /// </summary>
    public struct TileDef
    {
        public readonly int x;
        public readonly int y;

        public TileDef(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
