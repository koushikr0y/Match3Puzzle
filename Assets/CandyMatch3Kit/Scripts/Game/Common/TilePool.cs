// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

using GameVanilla.Core;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class stores the pools of the tiles used in the game.
    /// </summary>
    public class TilePool : MonoBehaviour
    {
        public ObjectPool blueCandyPool;
        public ObjectPool greenCandyPool;
        public ObjectPool orangeCandyPool;
        public ObjectPool purpleCandyPool;
        public ObjectPool redCandyPool;
        public ObjectPool yellowCandyPool;

        public ObjectPool blueHorizontalStripedCandyPool;
        public ObjectPool greenHorizontalStripedCandyPool;
        public ObjectPool orangeHorizontalStripedCandyPool;
        public ObjectPool purpleHorizontalStripedCandyPool;
        public ObjectPool redHorizontalStripedCandyPool;
        public ObjectPool yellowHorizontalStripedCandyPool;

        public ObjectPool blueVerticalStripedCandyPool;
        public ObjectPool greenVerticalStripedCandyPool;
        public ObjectPool orangeVerticalStripedCandyPool;
        public ObjectPool purpleVerticalStripedCandyPool;
        public ObjectPool redVerticalStripedCandyPool;
        public ObjectPool yellowVerticalStripedCandyPool;

        public ObjectPool blueWrappedCandyPool;
        public ObjectPool greenWrappedCandyPool;
        public ObjectPool orangeWrappedCandyPool;
        public ObjectPool purpleWrappedCandyPool;
        public ObjectPool redWrappedCandyPool;
        public ObjectPool yellowWrappedCandyPool;

        public ObjectPool colorBombCandyPool;

        public ObjectPool honeyPool;
        public ObjectPool icePool;
        public ObjectPool syrup1Pool;
        public ObjectPool syrup2Pool;

        public ObjectPool marshmallowPool;
        public ObjectPool chocolatePool;
        public ObjectPool unbreakablePool;

        public ObjectPool cherryPool;
        public ObjectPool watermelonPool;

        public ObjectPool lightBgTilePool;
        public ObjectPool darkBgTilePool;

        private readonly List<ObjectPool> candies = new List<ObjectPool>();
        private readonly List<ObjectPool> horizontalStripedCandies = new List<ObjectPool>();
        private readonly List<ObjectPool> verticalStripedCandies = new List<ObjectPool>();
        private readonly List<ObjectPool> wrappedCandies = new List<ObjectPool>();

        private readonly List<ObjectPool> specialBlocks = new List<ObjectPool>();

        private readonly List<ObjectPool> collectables = new List<ObjectPool>();

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(blueCandyPool);
            Assert.IsNotNull(greenCandyPool);
            Assert.IsNotNull(orangeCandyPool);
            Assert.IsNotNull(purpleCandyPool);
            Assert.IsNotNull(redCandyPool);
            Assert.IsNotNull(yellowCandyPool);

            Assert.IsNotNull(blueHorizontalStripedCandyPool);
            Assert.IsNotNull(greenHorizontalStripedCandyPool);
            Assert.IsNotNull(orangeHorizontalStripedCandyPool);
            Assert.IsNotNull(purpleHorizontalStripedCandyPool);
            Assert.IsNotNull(redHorizontalStripedCandyPool);
            Assert.IsNotNull(yellowHorizontalStripedCandyPool);

            Assert.IsNotNull(blueVerticalStripedCandyPool);
            Assert.IsNotNull(greenVerticalStripedCandyPool);
            Assert.IsNotNull(orangeVerticalStripedCandyPool);
            Assert.IsNotNull(purpleVerticalStripedCandyPool);
            Assert.IsNotNull(redVerticalStripedCandyPool);
            Assert.IsNotNull(yellowVerticalStripedCandyPool);

            Assert.IsNotNull(blueWrappedCandyPool);
            Assert.IsNotNull(greenWrappedCandyPool);
            Assert.IsNotNull(orangeWrappedCandyPool);
            Assert.IsNotNull(purpleWrappedCandyPool);
            Assert.IsNotNull(redWrappedCandyPool);
            Assert.IsNotNull(yellowWrappedCandyPool);

            Assert.IsNotNull(colorBombCandyPool);

            Assert.IsNotNull(honeyPool);
            Assert.IsNotNull(icePool);
            Assert.IsNotNull(syrup1Pool);
            Assert.IsNotNull(syrup2Pool);

            Assert.IsNotNull(marshmallowPool);
            Assert.IsNotNull(chocolatePool);
            Assert.IsNotNull(unbreakablePool);

            Assert.IsNotNull(cherryPool);
            Assert.IsNotNull(watermelonPool);

            Assert.IsNotNull(lightBgTilePool);
            Assert.IsNotNull(darkBgTilePool);

            candies.Add(blueCandyPool);
            candies.Add(greenCandyPool);
            candies.Add(orangeCandyPool);
            candies.Add(purpleCandyPool);
            candies.Add(redCandyPool);
            candies.Add(yellowCandyPool);

            horizontalStripedCandies.Add(blueHorizontalStripedCandyPool);
            horizontalStripedCandies.Add(greenHorizontalStripedCandyPool);
            horizontalStripedCandies.Add(orangeHorizontalStripedCandyPool);
            horizontalStripedCandies.Add(purpleHorizontalStripedCandyPool);
            horizontalStripedCandies.Add(redHorizontalStripedCandyPool);
            horizontalStripedCandies.Add(yellowHorizontalStripedCandyPool);

            verticalStripedCandies.Add(blueVerticalStripedCandyPool);
            verticalStripedCandies.Add(greenVerticalStripedCandyPool);
            verticalStripedCandies.Add(orangeVerticalStripedCandyPool);
            verticalStripedCandies.Add(purpleVerticalStripedCandyPool);
            verticalStripedCandies.Add(redVerticalStripedCandyPool);
            verticalStripedCandies.Add(yellowVerticalStripedCandyPool);

            wrappedCandies.Add(blueWrappedCandyPool);
            wrappedCandies.Add(greenWrappedCandyPool);
            wrappedCandies.Add(orangeWrappedCandyPool);
            wrappedCandies.Add(purpleWrappedCandyPool);
            wrappedCandies.Add(redWrappedCandyPool);
            wrappedCandies.Add(yellowWrappedCandyPool);

            specialBlocks.Add(marshmallowPool);
            specialBlocks.Add(chocolatePool);
            specialBlocks.Add(unbreakablePool);

            collectables.Add(cherryPool);
            collectables.Add(watermelonPool);
        }

        /// <summary>
        /// Returns the pool of the specified candy color.
        /// </summary>
        /// <param name="color">The candy color.</param>
        /// <returns>The pool of the specified candy color.</returns>
        public ObjectPool GetCandyPool(CandyColor color)
        {
            return candies[(int) color];
        }

        /// <summary>
        /// Returns the pool of the specified striped candy.
        /// </summary>
        /// <param name="direction">The direction of the striped candy.</param>
        /// <param name="color">The color of the striped candy.</param>
        /// <returns>The pool of the specified striped candy.</returns>
        public ObjectPool GetStripedCandyPool(StripeDirection direction, CandyColor color)
        {
            switch (direction)
            {
                case StripeDirection.Horizontal:
                    return horizontalStripedCandies[(int) color];

                default:
                    return verticalStripedCandies[(int) color];

            }
        }

        /// <summary>
        /// Returns the pool of the specified wrapped candy.
        /// </summary>
        /// <param name="color">The color of the wrapped candy.</param>
        /// <returns>The pool of the specified wrapped candy.</returns>
        public ObjectPool GetWrappedCandyPool(CandyColor color)
        {
            return wrappedCandies[(int) color];
        }

        /// <summary>
        /// Returns the pool of the specified block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>The pool of the specified block.</returns>
        public ObjectPool GetSpecialBlockPool(SpecialBlockType block)
        {
            return specialBlocks[(int) block];
        }

        /// <summary>
        /// Returns the pool of the specified collectable.
        /// </summary>
        /// <param name="collectable">The collectable.</param>
        /// <returns>The pool of the specified collectable.</returns>
        public ObjectPool GetCollectablePool(CollectableType collectable)
        {
            return collectables[(int) collectable];
        }
    }
}
