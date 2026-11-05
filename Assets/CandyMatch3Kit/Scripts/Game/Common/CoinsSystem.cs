// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class handles the coins system in the game. It is used to buy and spend coins and other classes
    /// can subscribe to it in order to receive a notification when the number of coins changes.
    /// </summary>
    public class CoinsSystem : MonoBehaviour
    {
        private Action<int> onCoinsUpdated;

        /// <summary>
        /// Buys the specified amount of coins.
        /// </summary>
        /// <param name="amount">The amount of coins to buy.</param>
        public void BuyCoins(int amount)
        {
            var numCoins = PlayerPrefs.GetInt("num_coins");
            numCoins += amount;
            PlayerPrefs.SetInt("num_coins", numCoins);
            if (onCoinsUpdated != null)
            {
                onCoinsUpdated(numCoins);
            }
        }

        /// <summary>
        /// Spends the specified amount of coins.
        /// </summary>
        /// <param name="amount">The amount of coins to spend.</param>
        public void SpendCoins(int amount)
        {
            var numCoins = PlayerPrefs.GetInt("num_coins");
            numCoins -= amount;
            if (numCoins < 0)
            {
                numCoins = 0;
            }
            PlayerPrefs.SetInt("num_coins", numCoins);
            if (onCoinsUpdated != null)
            {
                onCoinsUpdated(numCoins);
            }
        }

        /// <summary>
        /// Registers the specified callback to be called when the amount of coins changes.
        /// </summary>
        /// <param name="callback">The callback to register.</param>
        public void Subscribe(Action<int> callback)
        {
            onCoinsUpdated += callback;
        }

        /// <summary>
        /// Unregisters the specified callback to be called when the amount of coins changes.
        /// </summary>
        /// <param name="callback">The callback to unregister.</param>
        public void Unsubscribe(Action<int> callback)
        {
            if (onCoinsUpdated != null)
            {
                onCoinsUpdated -= callback;
            }
        }
    }
}
