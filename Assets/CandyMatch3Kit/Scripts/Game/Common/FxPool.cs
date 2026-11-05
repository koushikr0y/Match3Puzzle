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
    /// This class stores the pools of the visual effects used in the game.
    /// </summary>
    public class FxPool : MonoBehaviour
    {
        public ObjectPool spawnParticles;

        public ObjectPool blueCandyExplosion;
        public ObjectPool greenCandyExplosion;
        public ObjectPool orangeCandyExplosion;
        public ObjectPool purpleCandyExplosion;
        public ObjectPool redCandyExplosion;
        public ObjectPool yellowCandyExplosion;

        public ObjectPool horizontalStripedCandyExplosion;
        public ObjectPool verticalStripedCandyExplosion;

        public ObjectPool wrappedCandyExplosion;

        public ObjectPool colorBombExplosion;

        public ObjectPool honeyExplosion;
        public ObjectPool iceExplosion;
        public ObjectPool syrupExplosion;

        public ObjectPool marshmallowExplosion;
        public ObjectPool chocolateExplosion;

        public ObjectPool collectableExplosion;

        public ObjectPool complimentTextPool;

        private readonly List<ObjectPool> candyExplosions = new List<ObjectPool>();
        private readonly List<ObjectPool> elementExplosions = new List<ObjectPool>();
        private readonly List<ObjectPool> specialBlockExplosions = new List<ObjectPool>();

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(spawnParticles);

            Assert.IsNotNull(blueCandyExplosion);
            Assert.IsNotNull(greenCandyExplosion);
            Assert.IsNotNull(orangeCandyExplosion);
            Assert.IsNotNull(purpleCandyExplosion);
            Assert.IsNotNull(redCandyExplosion);
            Assert.IsNotNull(yellowCandyExplosion);
            Assert.IsNotNull(horizontalStripedCandyExplosion);
            Assert.IsNotNull(verticalStripedCandyExplosion);
            Assert.IsNotNull(wrappedCandyExplosion);
            Assert.IsNotNull(colorBombExplosion);
            Assert.IsNotNull(honeyExplosion);
            Assert.IsNotNull(iceExplosion);
            Assert.IsNotNull(marshmallowExplosion);
            Assert.IsNotNull(chocolateExplosion);
            Assert.IsNotNull(collectableExplosion);

            Assert.IsNotNull(complimentTextPool);

            candyExplosions.Add(blueCandyExplosion);
            candyExplosions.Add(greenCandyExplosion);
            candyExplosions.Add(orangeCandyExplosion);
            candyExplosions.Add(purpleCandyExplosion);
            candyExplosions.Add(redCandyExplosion);
            candyExplosions.Add(yellowCandyExplosion);

            elementExplosions.Add(null);
            elementExplosions.Add(honeyExplosion);
            elementExplosions.Add(iceExplosion);
            elementExplosions.Add(syrupExplosion);
            elementExplosions.Add(syrupExplosion);

            specialBlockExplosions.Add(marshmallowExplosion);
            specialBlockExplosions.Add(chocolateExplosion);
            specialBlockExplosions.Add(null);
        }

        /// <summary>
        /// Returns the explosion pool of the specified candy color.
        /// </summary>
        /// <param name="color">The candy color.</param>
        /// <returns>The explosion pool of the specified candy color.</returns>
        public ObjectPool GetCandyExplosionPool(CandyColor color)
        {
            return candyExplosions[(int) color];
        }

        /// <summary>
        /// Returns the explosion pool of the specified striped candy.
        /// </summary>
        /// <param name="direction">The direction of the striped candy.</param>
        /// <returns>The explosion pool of the specified striped candy.</returns>
        public ObjectPool GetStripedCandyExplosionPool(StripeDirection direction)
        {
            switch (direction)
            {
                case StripeDirection.Horizontal:
                    return horizontalStripedCandyExplosion;

                default:
                    return verticalStripedCandyExplosion;
            }
        }

        /// <summary>
        /// Returns the explosion pool of the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The explosion pool of the specified element.</returns>
        public ObjectPool GetElementExplosion(ElementType element)
        {
            return elementExplosions[(int) element];
        }

        /// <summary>
        /// Returns the explosion pool of the specified block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <returns>The explosion pool of the specified block.</returns>
        public ObjectPool GetSpecialBlockExplosion(SpecialBlockType block)
        {
            return specialBlockExplosions[(int) block];
        }
    }
}
