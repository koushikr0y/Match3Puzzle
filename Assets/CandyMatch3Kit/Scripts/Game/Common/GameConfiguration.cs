// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameVanilla.Game.Common
{
    public abstract class ScoreOverride
    {
        public int score;

#if UNITY_EDITOR
        public virtual void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Score");
            score = EditorGUILayout.IntField(score, GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }
#endif
    }

    public class TileScore : ScoreOverride
    {
        public TileScoreType type;

#if UNITY_EDITOR
        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            type = (TileScoreType)EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Draw();

            GUILayout.EndVertical();
        }
#endif

        public override string ToString()
        {
            return string.Format("{0}: {1}", type, score);
        }
    }

    public class ElementScore : ScoreOverride
    {
        public ElementType type;

#if UNITY_EDITOR
        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            type = (ElementType)EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            base.Draw();

            GUILayout.EndVertical();
        }
#endif

        public override string ToString()
        {
            return string.Format("{0}: {1}", type, score);
        }
    }

    /// <summary>
    /// This class stores the general game settings.
    /// </summary>
    [Serializable]
    public class GameConfiguration
    {
        public int maxLives;
        public int timeToNextLife;
        public int livesRefillCost;

        public int initialCoins;

        public int numExtraMoves;
        public int extraMovesCost;
        public int numExtraTime;
        public int extraTimeCost;

        public int defaultTileScore;
        public List<ScoreOverride> scoreOverrides = new List<ScoreOverride>();

        public Dictionary<BoosterType, int> ingameBoosterAmount = new Dictionary<BoosterType, int>();
        public Dictionary<BoosterType, int> ingameBoosterCost = new Dictionary<BoosterType, int>();

        public int rewardedAdCoins;
        public List<IapItem> iapItems = new List<IapItem>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameConfiguration()
        {
            ingameBoosterAmount.Add(BoosterType.Lollipop, 1);
            ingameBoosterAmount.Add(BoosterType.Bomb, 1);
            ingameBoosterAmount.Add(BoosterType.Switch, 1);
            ingameBoosterAmount.Add(BoosterType.ColorBomb, 1);

            ingameBoosterCost.Add(BoosterType.Lollipop, 100);
            ingameBoosterCost.Add(BoosterType.Bomb, 100);
            ingameBoosterCost.Add(BoosterType.Switch, 100);
            ingameBoosterCost.Add(BoosterType.ColorBomb, 100);
        }

        /// <summary>
        /// Returns the score of the specified tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <returns>The score of the specified tile.</returns>
        public int GetTileScore(Tile tile)
        {
            if (tile.GetComponent<StripedCandy>() != null ||
                tile.GetComponent<WrappedCandy>() != null ||
                tile.GetComponent<ColorBomb>() != null)
            {
                var score = scoreOverrides.Find(x => x is TileScore && ((TileScore)x).type == TileScoreType.SpecialCandy);
                if (score != null)
                {
                    return score.score;
                }
            }
            else if (tile.GetComponent<SpecialBlock>() != null)
            {
                var score = scoreOverrides.Find(x => x is TileScore && ((TileScore)x).type == TileScoreType.SpecialBlock);
                if (score != null)
                {
                    return score.score;
                }
            }
            else if (tile.GetComponent<Collectable>() != null)
            {
                var score = scoreOverrides.Find(x => x is TileScore && ((TileScore)x).type == TileScoreType.Collectable);
                if (score != null)
                {
                    return score.score;
                }
            }
            else if (tile.GetComponent<Candy>() != null)
            {
                var score = scoreOverrides.Find(x => x is TileScore && ((TileScore)x).type == TileScoreType.NormalCandy);
                if (score != null)
                {
                    return score.score;
                }
            }
            return defaultTileScore;
        }

        /// <summary>
        /// Returns the score of the specified element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <returns>The score of the specified element type.</returns>
        public int GetElementScore(ElementType type)
        {
            var scores = scoreOverrides.FindAll(x => x is ElementScore);
            foreach (var score in scores)
            {
                var elementScore = score as ElementScore;
                if (elementScore != null && elementScore.type == type)
                {
                    return elementScore.score;
                }
            }
            return defaultTileScore;
        }
    }
}
