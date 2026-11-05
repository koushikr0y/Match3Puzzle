// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace GameVanilla.Game.Common
{
    public abstract class Goal
    {
        public abstract bool IsComplete(GameState state);

#if UNITY_EDITOR

        public abstract void Draw();

#endif
    }

    public class ReachScoreGoal : Goal
    {
        public int score;

        public override bool IsComplete(GameState state)
        {
            return state.score >= score;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Score");
            score = EditorGUILayout.IntField(score, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

#endif

        public override string ToString()
        {
            return "Reach " + score + " points";
        }
    }

    public class CollectCandyGoal : Goal
    {
        public CandyColor candyType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedCandies[candyType] >= amount;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            candyType = (CandyColor)EditorGUILayout.EnumPopup(candyType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount");
            amount = EditorGUILayout.IntField(amount, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

#endif

        public override string ToString()
        {
            return "Collect " + amount + " " + candyType;
        }
    }

    public class CollectElementGoal : Goal
    {
        public ElementType elementType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedElements[elementType] >= amount;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            elementType = (ElementType) EditorGUILayout.EnumPopup(elementType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount");
            amount = EditorGUILayout.IntField(amount, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

#endif

        public override string ToString()
        {
            return "Collect " + amount + " " + elementType;
        }
    }

    public class CollectSpecialBlockGoal : Goal
    {
        public SpecialBlockType specialBlockType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedSpecialBlocks[specialBlockType] >= amount;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            specialBlockType = (SpecialBlockType)EditorGUILayout.EnumPopup(specialBlockType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount");
            amount = EditorGUILayout.IntField(amount, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

#endif

        public override string ToString()
        {
            return "Collect " + amount + " " + specialBlockType;
        }
    }

    public class CollectCollectableGoal : Goal
    {
        public CollectableType collectableType;
        public int amount;

        public override bool IsComplete(GameState state)
        {
            return state.collectedCollectables[collectableType] >= amount;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Type");
            collectableType = (CollectableType)EditorGUILayout.EnumPopup(collectableType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Amount");
            amount = EditorGUILayout.IntField(amount, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

#endif

        public override string ToString()
        {
            return "Collect " + amount + " " + collectableType;
        }
    }

    public class DestroyAllChocolateGoal : Goal
    {
        public bool completed;

        public override bool IsComplete(GameState state)
        {
            return completed;
        }

#if UNITY_EDITOR

        public override void Draw()
        {
        }

#endif

        public override string ToString()
        {
            return "Destroy all chocolate";
        }
    }
}
