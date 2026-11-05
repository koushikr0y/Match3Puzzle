// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Editor
{
    /// <summary>
    /// The "Level editor" tab in the editor.
    /// </summary>
    public class LevelEditorTab : EditorTab
    {
        private int prevWidth = -1;
        private int prevHeight = -1;

        private enum BrushType
        {
            Candy,
            Element,
            SpecialCandy,
            SpecialBlock,
            Collectable,
            Empty,
            Hole
        }

        private BrushType currentBrushType;
        private CandyType currentCandyType;
        private SpecialCandyType currentSpecialCandyType;
        private SpecialBlockType currentSpecialBlockType;
        private ElementType currentElementType;
        private CollectableType currentCollectableType;

        private enum BrushMode
        {
            Tile,
            Row,
            Column,
            Fill
        }

        private BrushMode currentBrushMode = BrushMode.Tile;

        private readonly Dictionary<string, Texture> tileTextures = new Dictionary<string, Texture>();

        private Level currentLevel;

        private ReorderableList goalList;
        private Goal currentGoal;

        private ReorderableList availableCandyColorsList;
        private CandyColor currentCandyColor;

        private Vector2 scrollPos;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="editor">The parent editor.</param>
        public LevelEditorTab(PuzzleMatchKitEditor editor) : base(editor)
        {
            var editorImagesPath = new DirectoryInfo(Application.dataPath + "/CandyMatch3Kit/Editor/Resources");
            var fileInfo = editorImagesPath.GetFiles("*.png", SearchOption.TopDirectoryOnly);
            foreach (var file in fileInfo)
            {
                var filename = Path.GetFileNameWithoutExtension(file.Name);
                tileTextures[filename] = Resources.Load(filename) as Texture;
            }
        }

        /// <summary>
        /// Called when this tab is drawn.
        /// </summary>
        public override void Draw()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 90;

            GUILayout.Space(15);

            DrawMenu();

            if (currentLevel != null)
            {
                var level = currentLevel;
                prevWidth = level.width;

                GUILayout.Space(15);

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Space(15);

                GUILayout.BeginVertical();
                DrawGeneralSettings();
                GUILayout.Space(15);
                DrawInGameBoosterSettings();
                GUILayout.EndVertical();

                GUILayout.Space(300);

                GUILayout.BeginVertical();
                DrawGoalSettings();
                GUILayout.Space(15);
                DrawAvailableColorBlockSettings();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.Space(15);

                DrawLevelEditor();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        private void DrawMenu()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New", GUILayout.Width(100), GUILayout.Height(50)))
            {
                currentLevel = new Level();
                currentGoal = null;
                InitializeNewLevel();
                CreateGoalsList();
                CreateAvailableColorBlocksList();
            }

            if (GUILayout.Button("Open", GUILayout.Width(100), GUILayout.Height(50)))
            {
                var path = EditorUtility.OpenFilePanel("Open level", Application.dataPath + "/CandyMatch3Kit/Resources/Levels",
                    "json");
                if (!string.IsNullOrEmpty(path))
                {
                    currentLevel = LoadJsonFile<Level>(path);
                    CreateGoalsList();
                    CreateAvailableColorBlocksList();
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(50)))
            {
                SaveLevel(Application.dataPath + "/CandyMatch3Kit/Resources");
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the general settings.
        /// </summary>
        private void DrawGeneralSettings()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The general settings of this level.",
                MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Level number", "The number of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.id = EditorGUILayout.IntField(currentLevel.id, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Limit type", "The limit type of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.limitType = (LimitType) EditorGUILayout.EnumPopup(currentLevel.limitType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (currentLevel.limitType == LimitType.Moves)
            {
                EditorGUILayout.LabelField(new GUIContent("Moves", "The maximum number of moves of this level."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
            }
            else if (currentLevel.limitType == LimitType.Time)
            {
                EditorGUILayout.LabelField(new GUIContent("Time", "The maximum number of seconds of this level."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
            }
            currentLevel.limit = EditorGUILayout.IntField(currentLevel.limit, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star 1 score", "The score needed to reach the first star."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.score1 = EditorGUILayout.IntField(currentLevel.score1, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star 2 score", "The score needed to reach the second star."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.score2 = EditorGUILayout.IntField(currentLevel.score2, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star 3 score", "The score needed to reach the third star."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.score3 = EditorGUILayout.IntField(currentLevel.score3, GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the in-game booster settings.
        /// </summary>
        private void DrawInGameBoosterSettings()
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 110;

            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("In-game boosters", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The in-game booster settings of this level.",
                MessageType.Info);
            GUILayout.EndHorizontal();

            foreach (var booster in Enum.GetValues(typeof(BoosterType)))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(StringUtils.DisplayCamelCaseString(booster.ToString()));
                var availableBoosters = currentLevel.availableBoosters;
                availableBoosters[(BoosterType)booster] =
                    EditorGUILayout.Toggle(availableBoosters[(BoosterType)booster], GUILayout.Width(30));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Draws the goal settings.
        /// </summary>
        private void DrawGoalSettings()
        {
            EditorGUILayout.LabelField("Goals", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "This list defines the goals needed to be achieved by the player in order to complete this level.",
                MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(250));
            if (goalList != null)
            {
                goalList.DoLayoutList();
            }
            GUILayout.EndVertical();

            if (currentGoal != null)
            {
                DrawGoal(currentGoal);
            }

            GUILayout.EndHorizontal();

            if (currentLevel.limitType == LimitType.Moves)
            {
                EditorGUIUtility.labelWidth = 140;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Award special candies",
                        "Enable this if you want special candies equal to the number of remaining moves to be awarded to the player at the end of the game."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentLevel.awardSpecialCandies =
                    EditorGUILayout.Toggle(currentLevel.awardSpecialCandies);
                GUILayout.EndHorizontal();

                if (currentLevel.awardSpecialCandies)
                {
                    GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Type", "The type of special candy to award."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                    currentLevel.awardedSpecialCandyType =
                        (AwardedSpecialCandyType)EditorGUILayout.EnumPopup(currentLevel.awardedSpecialCandyType, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }
                EditorGUIUtility.labelWidth = 90;
            }
        }

        /// <summary>
        /// Draws the available color block settings.
        /// </summary>
        private void DrawAvailableColorBlockSettings()
        {
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Available colors", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "This list defines the available colors when a new random candy is created.",
                MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(250));
            if (availableCandyColorsList != null)
            {
                availableCandyColorsList.DoLayoutList();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 120;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Collectable chance",
                    "The random chance of a collectable block to be created."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.collectableChance = EditorGUILayout.IntField(currentLevel.collectableChance, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 90;

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the level editor.
        /// </summary>
        private void DrawLevelEditor()
        {
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The layout settings of this level.",
                MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Width", "The width of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.width = EditorGUILayout.IntField(currentLevel.width, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            prevHeight = currentLevel.height;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Height", "The height of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevel.height = EditorGUILayout.IntField(currentLevel.height, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Brush type", "The current type of brush."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentBrushType = (BrushType) EditorGUILayout.EnumPopup(currentBrushType, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            if (currentBrushType == BrushType.Candy)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Candy", "The current type of candy."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentCandyType = (CandyType) EditorGUILayout.EnumPopup(currentCandyType, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            else if (currentBrushType == BrushType.Element)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Element", "The current type of element."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentElementType =
                    (ElementType) EditorGUILayout.EnumPopup(currentElementType, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            else if (currentBrushType == BrushType.SpecialCandy)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Special candy", "The current type of special candy."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentSpecialCandyType =
                    (SpecialCandyType) EditorGUILayout.EnumPopup(currentSpecialCandyType, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            else if (currentBrushType == BrushType.SpecialBlock)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Special block", "The current type of special block."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentSpecialBlockType =
                    (SpecialBlockType) EditorGUILayout.EnumPopup(currentSpecialBlockType, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            else if (currentBrushType == BrushType.Collectable)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Collectable", "The current type of collectable."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                currentCollectableType =
                    (CollectableType) EditorGUILayout.EnumPopup(currentCollectableType, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Brush mode", "The current brush mode."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentBrushMode = (BrushMode) EditorGUILayout.EnumPopup(currentBrushMode, GUILayout.Width(100));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (prevWidth != currentLevel.width || prevHeight != currentLevel.height)
            {
                currentLevel.tiles = new List<LevelTile>(currentLevel.width * currentLevel.height);
                for (var i = 0; i < currentLevel.width; i++)
                {
                    for (var j = 0; j < currentLevel.height; j++)
                    {
                        currentLevel.tiles.Add(new CandyTile() {type = CandyType.RandomCandy});
                    }
                }
            }

            for (var i = 0; i < currentLevel.height; i++)
            {
                GUILayout.BeginHorizontal();
                for (var j = 0; j < currentLevel.width; j++)
                {
                    var tileIndex = (currentLevel.width * i) + j;
                    CreateButton(tileIndex);
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Initializes a newly-created level.
        /// </summary>
        private void InitializeNewLevel()
        {
            foreach (var type in Enum.GetValues(typeof(CandyColor)))
            {
                currentLevel.availableColors.Add((CandyColor)type);
            }

            foreach (var type in Enum.GetValues(typeof(BoosterType)))
            {
                currentLevel.availableBoosters.Add((BoosterType)type, true);
            }
        }

        /// <summary>
        /// Creates the list of goals of this level.
        /// </summary>
        private void CreateGoalsList()
        {
            goalList = SetupReorderableList("Goals", currentLevel.goals, ref currentGoal, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        x.ToString());
                },
                (x) =>
                {
                    currentGoal = x;
                },
                () =>
                {
                    var menu = new GenericMenu();
                    var goalTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Goal));
                    foreach (var type in goalTypes)
                    {
                        menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.Name)), false,
                            CreateGoalCallback, type);
                    }
                    menu.ShowAsContext();
                },
                (x) =>
                {
                    currentGoal = null;
                });
        }

        /// <summary>
        /// Callback to call when a new goal is created.
        /// </summary>
        /// <param name="obj">The type of object to create.</param>
        private void CreateGoalCallback(object obj)
        {
            var goal = Activator.CreateInstance((Type)obj) as Goal;
            currentLevel.goals.Add(goal);
        }

        /// <summary>
        /// Creates the list of available color blocks of this level.
        /// </summary>
        private void CreateAvailableColorBlocksList()
        {
            availableCandyColorsList = SetupReorderableList("Available colors", currentLevel.availableColors, ref currentCandyColor, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                        x.ToString());
                },
                (x) =>
                {
                    currentCandyColor = x;
                },
                () =>
                {
                    var menu = new GenericMenu();
                    foreach (var type in Enum.GetValues(typeof(CandyColor)))
                    {
                        menu.AddItem(new GUIContent(StringUtils.DisplayCamelCaseString(type.ToString())), false,
                            CreateColorBlockTypeCallback, type);
                    }
                    menu.ShowAsContext();
                },
                (x) =>
                {
                    currentCandyColor = CandyColor.Blue;
                });
            availableCandyColorsList.onRemoveCallback = l =>
            {
                if (currentLevel.availableColors.Count == 1)
                {
                    EditorUtility.DisplayDialog("Warning", "You need at least one color block type.", "Ok");
                }
                else
                {
                    if (!EditorUtility.DisplayDialog("Warning!",
                        "Are you sure you want to delete this item?", "Yes", "No"))
                    {
                        return;
                    }
                    currentCandyColor = CandyColor.Blue;
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                }
            };
        }

        /// <summary>
        /// Callback to call when a new color block type is created.
        /// </summary>
        /// <param name="obj">The type of object to create.</param>
        private void CreateColorBlockTypeCallback(object obj)
        {
            var color = (CandyColor)obj;
            if (currentLevel.availableColors.Contains(color))
            {
                EditorUtility.DisplayDialog("Warning", "This color is already present in the list.", "Ok");
            }
            else
            {
                currentLevel.availableColors.Add(color);
            }
        }

        /// <summary>
        /// Creates a new tile button.
        /// </summary>
        /// <param name="tileIndex">The tile index.</param>
        private void CreateButton(int tileIndex)
        {
            var tileTypeName = string.Empty;
            if (currentLevel.tiles[tileIndex] != null)
            {
                if (currentLevel.tiles[tileIndex] is CandyTile)
                {
                    var blockTile = (CandyTile)currentLevel.tiles[tileIndex];
                    tileTypeName = blockTile.type.ToString();
                }
                else if (currentLevel.tiles[tileIndex] is SpecialCandyTile)
                {
                    var specialCandyTile = (SpecialCandyTile)currentLevel.tiles[tileIndex];
                    tileTypeName = specialCandyTile.type.ToString();
                }
                else if (currentLevel.tiles[tileIndex] is SpecialBlockTile)
                {
                    var specialBlockTile = (SpecialBlockTile)currentLevel.tiles[tileIndex];
                    tileTypeName = specialBlockTile.type.ToString();
                }
                else if (currentLevel.tiles[tileIndex] is CollectableTile)
                {
                    var collectableTile = (CollectableTile)currentLevel.tiles[tileIndex];
                    tileTypeName = collectableTile.type.ToString();
                }
                else if (currentLevel.tiles[tileIndex] is HoleTile)
                {
                    tileTypeName = "Hole";
                }

                if (currentLevel.tiles[tileIndex].elementType == ElementType.Honey)
                {
                    tileTypeName += "_Honey";
                }
                else if (currentLevel.tiles[tileIndex].elementType == ElementType.Ice)
                {
                    tileTypeName += "_Ice";
                }
                else if (currentLevel.tiles[tileIndex].elementType == ElementType.Syrup1)
                {
                    tileTypeName += "_Syrup1";
                }
                else if (currentLevel.tiles[tileIndex].elementType == ElementType.Syrup2)
                {
                    tileTypeName += "_Syrup2";
                }
            }

            if (tileTextures.ContainsKey(tileTypeName))
            {
                if (GUILayout.Button(tileTextures[tileTypeName], GUILayout.Width(60), GUILayout.Height(60)))
                {
                    DrawTile(tileIndex);
                }
            }
            else
            {
                if (GUILayout.Button("", GUILayout.Width(60), GUILayout.Height(60)))
                {
                    DrawTile(tileIndex);
                }
            }
        }

        /// <summary>
        /// Draws the tile at the specified index.
        /// </summary>
        /// <param name="tileIndex">The tile index.</param>
        private void DrawTile(int tileIndex)
        {
            var x = tileIndex % currentLevel.width;
            var y = tileIndex / currentLevel.width;
            if (currentBrushType == BrushType.Candy)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        currentLevel.tiles[tileIndex] = new CandyTile {type = currentCandyType};
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            currentLevel.tiles[idx] = new CandyTile {type = currentCandyType};
                        }
                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new CandyTile {type = currentCandyType};
                        }
                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                currentLevel.tiles[idx] = new CandyTile {type = currentCandyType};
                            }
                        }
                        break;
                }
            }
            else if (currentBrushType == BrushType.SpecialCandy)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        currentLevel.tiles[tileIndex] = new SpecialCandyTile {type = currentSpecialCandyType};
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            currentLevel.tiles[idx] = new SpecialCandyTile {type = currentSpecialCandyType};
                        }
                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new SpecialCandyTile {type = currentSpecialCandyType};
                        }
                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                currentLevel.tiles[idx] = new SpecialCandyTile {type = currentSpecialCandyType};
                            }
                        }
                        break;
                }
            }
            else if (currentBrushType == BrushType.Element)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        if (currentLevel.tiles[tileIndex] != null)
                        {
                            currentLevel.tiles[tileIndex].elementType = currentElementType;
                        }
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            if (currentLevel.tiles[idx] != null)
                            {
                                currentLevel.tiles[idx].elementType = currentElementType;
                            }
                        }
                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            if (currentLevel.tiles[idx] != null)
                            {
                                currentLevel.tiles[idx].elementType = currentElementType;
                            }
                        }
                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                if (currentLevel.tiles[idx] != null)
                                {
                                    currentLevel.tiles[idx].elementType = currentElementType;
                                }
                            }
                        }
                        break;
                }
            }
            else if (currentBrushType == BrushType.SpecialBlock)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        currentLevel.tiles[tileIndex] = new SpecialBlockTile {type = currentSpecialBlockType};
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            currentLevel.tiles[idx] = new SpecialBlockTile {type = currentSpecialBlockType};
                        }
                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new SpecialBlockTile {type = currentSpecialBlockType};
                        }
                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                currentLevel.tiles[idx] = new SpecialBlockTile {type = currentSpecialBlockType};
                            }
                        }
                        break;
                }
            }
            else if (currentBrushType == BrushType.Collectable)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        currentLevel.tiles[tileIndex] = new CollectableTile {type = currentCollectableType};
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            currentLevel.tiles[idx] = new CollectableTile {type = currentCollectableType};
                        }
                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new CollectableTile {type = currentCollectableType};
                        }
                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                currentLevel.tiles[idx] = new CollectableTile {type = currentCollectableType};
                            }
                        }
                        break;
                }
            }
            else if (currentBrushType == BrushType.Empty)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        currentLevel.tiles[tileIndex] = null;
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            currentLevel.tiles[idx] = null;
                        }

                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            currentLevel.tiles[idx] = null;
                        }

                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                currentLevel.tiles[idx] = null;
                            }
                        }

                        break;
                }
            }
            else if (currentBrushType == BrushType.Hole)
            {
                switch (currentBrushMode)
                {
                    case BrushMode.Tile:
                        currentLevel.tiles[tileIndex] = new HoleTile();
                        break;

                    case BrushMode.Row:
                        for (var i = 0; i < currentLevel.width; i++)
                        {
                            var idx = i + (y * currentLevel.width);
                            currentLevel.tiles[idx] = new HoleTile();
                        }
                        break;

                    case BrushMode.Column:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            var idx = x + (j * currentLevel.width);
                            currentLevel.tiles[idx] = new HoleTile();
                        }
                        break;

                    case BrushMode.Fill:
                        for (var j = 0; j < currentLevel.height; j++)
                        {
                            for (var i = 0; i < currentLevel.width; i++)
                            {
                                var idx = i + (j * currentLevel.width);
                                currentLevel.tiles[idx] = new HoleTile();
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Draws the specified goal item.
        /// </summary>
        /// <param name="goal">The goal item to draw.</param>
        private void DrawGoal(Goal goal)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;

            goal.Draw();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        /// <summary>
        /// Saves the current level to the specified path.
        /// </summary>
        /// <param name="path">The path to which to save the current level.</param>
        public void SaveLevel(string path)
        {
#if UNITY_EDITOR
            SaveJsonFile(path + "/Levels/" + currentLevel.id + ".json", currentLevel);
            AssetDatabase.Refresh();
#endif
        }
    }
}
