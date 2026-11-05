// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Editor
{
    /// <summary>
    /// The "Game settings" tab in the editor.
    /// </summary>
    public class GameSettingsTab : EditorTab
    {
        private ReorderableList tileScoreOverridesList;
        private ScoreOverride currentScoreOverride;

        private ReorderableList iapItemsList;
        private IapItem currentIapItem;

        private const string editorPrefsName = "cmk_game_settings_path";

        private int selectedTabIndex;

        private Vector2 scrollPos;

        private int newLevel;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editor">The parent editor.</param>
        public GameSettingsTab(PuzzleMatchKitEditor editor) : base(editor)
        {
            if (EditorPrefs.HasKey(editorPrefsName))
            {
                var path = EditorPrefs.GetString(editorPrefsName);
                parentEditor.gameConfig = LoadJsonFile<GameConfiguration>(path);
                if (parentEditor.gameConfig != null)
                {
                    CreateTileScoreOverridesList();
                    CreateIapItemsList();
                }
            }

            newLevel = PlayerPrefs.GetInt("next_level");
        }

        /// <summary>
        /// Called when this tab is drawn.
        /// </summary>
        public override void Draw()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.Space(15);

            DrawMenu();

            GUILayout.Space(15);

            var prevSelectedIndex = selectedTabIndex;
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
                new[] {"Game", "Monetization", "Player preferences"}, GUILayout.Width(500));

            if (selectedTabIndex != prevSelectedIndex)
            {
                GUI.FocusControl(null);
            }

            if (selectedTabIndex == 0)
            {
                DrawGameTab();
            }
            else if (selectedTabIndex == 1)
            {
                DrawMonetizationTab();
            }
            else
            {
                DrawPreferencesTab();
            }


            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the game tab.
        /// </summary>
        private void DrawGameTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                DrawScoreSettings();

                GUILayout.Space(15);

                DrawLivesSettings();

                GUILayout.Space(15);

                DrawCoinsSettings();

                GUILayout.Space(15);

                DrawPurchasableBoosterSettings();

                GUILayout.Space(15);

                DrawContinueGameSettings();
            }
        }

        /// <summary>
        /// Draws the monetization tab.
        /// </summary>
        private void DrawMonetizationTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                DrawRewardedAdSettings();

                GUILayout.Space(15);

                DrawIapSettings();
            }
        }

        /// <summary>
        /// Draws the preferences tab.
        /// </summary>
        private void DrawPreferencesTab()
        {
            if (parentEditor.gameConfig != null)
            {
                GUILayout.Space(15);

                DrawPreferencesSettings();
            }
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        private void DrawMenu()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New", GUILayout.Width(100), GUILayout.Height(50)))
            {
                parentEditor.gameConfig = new GameConfiguration();
                CreateTileScoreOverridesList();
                CreateIapItemsList();
            }

            if (GUILayout.Button("Open", GUILayout.Width(100), GUILayout.Height(50)))
            {
                var path = EditorUtility.OpenFilePanel("Open game configuration", Application.dataPath + "/CandyMatch3Kit/Resources",
                    "json");
                if (!string.IsNullOrEmpty(path))
                {
                    parentEditor.gameConfig = LoadJsonFile<GameConfiguration>(path);
                    if (parentEditor.gameConfig != null)
                    {
                        CreateTileScoreOverridesList();
                        CreateIapItemsList();
                        EditorPrefs.SetString(editorPrefsName, path);
                    }
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(50)))
            {
                SaveGameConfiguration(Application.dataPath + "/CandyMatch3Kit/Resources");
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the score settings.
        /// </summary>
        private void DrawScoreSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Score", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The default score given to the player when a tile explodes. You can change this value for specific tile types in the override list.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default score", "The default score of tiles."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.defaultTileScore =
                EditorGUILayout.IntField(gameConfig.defaultTileScore, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(250));
            if (tileScoreOverridesList != null)
            {
                tileScoreOverridesList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentScoreOverride != null)
            {
                DrawScoreOverride(currentScoreOverride);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the lives settings.
        /// </summary>
        private void DrawLivesSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Lives", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the lives system.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Max lives",
                    "The maximum number of lives that the player can have."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.maxLives = EditorGUILayout.IntField(gameConfig.maxLives, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Time to next life",
                    "The number of seconds that need to pass before the player is given a free life."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.timeToNextLife = EditorGUILayout.IntField(gameConfig.timeToNextLife, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Refill cost",
                    "The cost in coins of refilling the lives of the player up to its maximum number."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.livesRefillCost = EditorGUILayout.IntField(gameConfig.livesRefillCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the coins settings.
        /// </summary>
        private void DrawCoinsSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Coins", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the coins system.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Initial coins",
                    "The initial number of coins given to the player."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.initialCoins = EditorGUILayout.IntField(gameConfig.initialCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the purchasable booster settings.
        /// </summary>
        private void DrawPurchasableBoosterSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            var oldLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.LabelField("Purchasable boosters", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the boosters that can be purchased by the player in-game.", MessageType.Info);
            GUILayout.EndHorizontal();

            foreach (var booster in Enum.GetValues(typeof(BoosterType)))
            {
                EditorGUIUtility.labelWidth = 150;

                var boosterText = StringUtils.DisplayCamelCaseString(booster.ToString());
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(string.Format("{0} amount", boosterText));
                var amount = gameConfig.ingameBoosterAmount;
                amount[(BoosterType)booster] =
                    EditorGUILayout.IntField(amount[(BoosterType)booster], GUILayout.Width(30));

                GUILayout.Space(15);

                EditorGUIUtility.labelWidth = 130;

                EditorGUILayout.PrefixLabel(string.Format("{0} cost", boosterText));
                var cost = gameConfig.ingameBoosterCost;
                cost[(BoosterType)booster] =
                    EditorGUILayout.IntField(cost[(BoosterType)booster], GUILayout.Width(70));
                GUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Draws the continue game settings.
        /// </summary>
        private void DrawContinueGameSettings()
        {
            var gameConfig = parentEditor.gameConfig;

            EditorGUILayout.LabelField("Continue game", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the options given to the player when losing a game.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Extra moves",
                    "The number of extra moves that can be purchased by the player when a move-limited game is lost."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.numExtraMoves = EditorGUILayout.IntField(gameConfig.numExtraMoves, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Moves cost",
                    "The cost in coins of the extra moves."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.extraMovesCost = EditorGUILayout.IntField(gameConfig.extraMovesCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Extra time",
                    "The number of extra seconds that can be purchased by the player when a time-limited game is lost."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.numExtraTime = EditorGUILayout.IntField(gameConfig.numExtraTime, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Time cost",
                    "The cost in coins of the extra time."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.extraTimeCost = EditorGUILayout.IntField(gameConfig.extraTimeCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the rewarded ad settings.
        /// </summary>
		private void DrawRewardedAdSettings()
		{
            EditorGUILayout.LabelField("Rewarded ad", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The number of coins given to the player after watching a rewarded ad.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Coins", "The number of coins awarded to the player after watching a rewarded ad."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            parentEditor.gameConfig.rewardedAdCoins =
                EditorGUILayout.IntField(parentEditor.gameConfig.rewardedAdCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();
		}

        /// <summary>
        /// Draws the in-app purchases settings.
        /// </summary>
        private void DrawIapSettings()
        {
            EditorGUILayout.LabelField("In-app purchases", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The settings of your in-game purchasable items.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(350));
            if (iapItemsList != null)
            {
                iapItemsList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentIapItem != null)
            {
                DrawIapItem(currentIapItem);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the preferences settings.
        /// </summary>
		private void DrawPreferencesSettings()
		{
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
		    GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Level", "The current level number."),
                GUILayout.Width(50));
            newLevel =
                EditorGUILayout.IntField(newLevel, GUILayout.Width(50));
		    GUILayout.EndHorizontal();

		    if (GUILayout.Button("Set progress", GUILayout.Width(120), GUILayout.Height(30)))
		    {
		        PlayerPrefs.SetInt("next_level", newLevel);
		    }

		    GUILayout.Space(15);

            EditorGUILayout.LabelField("PlayerPrefs", EditorStyles.boldLabel);
		    if (GUILayout.Button("Delete PlayerPrefs", GUILayout.Width(120), GUILayout.Height(30)))
		    {
		        PlayerPrefs.DeleteAll();
		    }

		    GUILayout.Space(15);

            EditorGUILayout.LabelField("EditorPrefs", EditorStyles.boldLabel);
		    if (GUILayout.Button("Delete EditorPrefs", GUILayout.Width(120), GUILayout.Height(30)))
		    {
		        EditorPrefs.DeleteAll();
		    }
		}

        /// <summary>
        /// Creates the list of tile score overrides.
        /// </summary>
        private void CreateTileScoreOverridesList()
        {
            tileScoreOverridesList = SetupReorderableList("Score overrides", parentEditor.gameConfig.scoreOverrides,
                ref currentScoreOverride, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.ToString());
                },
                (x) =>
                {
                    currentScoreOverride = x;
                },
                () =>
                {
                    var menu = new GenericMenu();
                    var goalTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScoreOverride));
                    foreach (var type in goalTypes)
                    {
                        menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.Name)), false,
                            CreateTileScoreOverrideCallback, type);
                    }
                    menu.ShowAsContext();
                },
                (x) =>
                {
                    currentScoreOverride = null;
                });
        }

        /// <summary>
        /// Creates the list of in-app purchase items.
        /// </summary>
        private void CreateIapItemsList()
        {
            iapItemsList = SetupReorderableList("In-app purchase items", parentEditor.gameConfig.iapItems,
                ref currentIapItem, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 350, EditorGUIUtility.singleLineHeight), x.storeId);
                },
                (x) =>
                {
                    currentIapItem = x;
                },
                () =>
                {
                    var newItem = new IapItem();
                    parentEditor.gameConfig.iapItems.Add(newItem);
                },
                (x) =>
                {
                    currentIapItem = null;
                });
        }

        /// <summary>
        /// Callback to call when a new tile score override is created.
        /// </summary>
        /// <param name="obj">The type of object to create.</param>
        private void CreateTileScoreOverrideCallback(object obj)
        {
            var goal = Activator.CreateInstance((Type) obj) as ScoreOverride;
            parentEditor.gameConfig.scoreOverrides.Add(goal);
        }

        /// <summary>
        /// Draws the specified score override item.
        /// </summary>
        /// <param name="tileScoreOverride">The score override item to draw.</param>
        private void DrawScoreOverride(ScoreOverride scoreOverride)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;

            scoreOverride.Draw();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Draws the specified in-app purchase item.
        /// </summary>
        /// <param name="item">The in-app purchase item to draw.</param>
        private void DrawIapItem(IapItem item)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            item.Draw();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Saves the current game configuration to the specified path.
        /// </summary>
        /// <param name="path">The path to which to save the current game configuration.</param>
        public void SaveGameConfiguration(string path)
        {
#if UNITY_EDITOR
            var fullPath = path + "/game_configuration.json";
            SaveJsonFile(fullPath, parentEditor.gameConfig);
            EditorPrefs.SetString(editorPrefsName, fullPath);
            AssetDatabase.Refresh();
#endif
        }
    }
}
