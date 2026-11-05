// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// The available coin icons.
    /// </summary>
    public enum CoinIcon
    {
        VerySmall,
        Small,
        MediumSmall,
        MediumLarge,
        Large,
        VeryLarge
    }

    /// <summary>
    /// An in-app purchasable item.
    /// </summary>
    public class IapItem
    {
        public string storeId;
        public int numCoins;
        public int discount;
        public bool mostPopular;
        public bool bestValue;
        public CoinIcon coinIcon;

#if UNITY_EDITOR
        /// <summary>
        /// Draws the IAP item.
        /// </summary>
        public void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Store id");
            storeId = EditorGUILayout.TextField(storeId, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Coins");
            numCoins = EditorGUILayout.IntField(numCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Discount");
            discount = EditorGUILayout.IntField(discount, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Most popular");
            mostPopular = EditorGUILayout.Toggle(mostPopular);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Best value");
            bestValue = EditorGUILayout.Toggle(bestValue);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Coin icon");
            coinIcon = (CoinIcon)EditorGUILayout.EnumPopup(coinIcon, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
#endif
    }
}
