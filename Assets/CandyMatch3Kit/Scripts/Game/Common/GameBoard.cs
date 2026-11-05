// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

using FullSerializer;

using GameVanilla.Core;
using GameVanilla.Game.Popups;
using GameVanilla.Game.Scenes;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class is responsible for managing the match-3 gameplay of the kit.
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [SerializeField]
        private GameScene gameScene;

        [SerializeField]
        private GameUi gameUi;

        [SerializeField]
        private BoosterBar boosterBar;

        [SerializeField]
        private TilePool tilePool;

        [SerializeField]
        private FxPool fxPool;

        [SerializeField]
        private Transform boardCenter;

        [SerializeField]
        private List<AudioClip> gameSounds;

        [HideInInspector]
        public Level level;

        [HideInInspector]
        public GameState gameState = new GameState();

        [HideInInspector]
        public int currentLimit;

        private List<GameObject> tiles;
        private List<GameObject> honeys;
        private List<GameObject> ices;
        private List<GameObject> syrups1;
        private List<GameObject> syrups2;

        private readonly List<Vector3> tilePositions = new List<Vector3>();

        private bool drag;
        private GameObject selectedTile;

        private List<Swap> possibleSwaps = new List<Swap>();

        private float tileW;
        private float tileH;

        private GameObject lastSelectedTile;
        private int lastSelectedTileX;
        private int lastSelectedTileY;
        private CandyColor lastSelectedCandyColor;
        
        private GameObject lastOtherSelectedTile;
        private int lastOtherSelectedTileX;
        private int lastOtherSelectedTileY;
        private CandyColor lastOtherSelectedCandyColor;

        private GameConfiguration gameConfig;

        private readonly ComboDetector comboDetector = new ComboDetector();

        private readonly List<GameObject> suggestedMatch = new List<GameObject>();
        private Coroutine suggestedMatchCoroutine;

        private Coroutine countdownCoroutine;

        private bool currentlyAwarding;
        public bool CurrentlyAwarding
        {
            get { return currentlyAwarding; }
        }

        private bool inputLocked;

        private readonly List<CollectableType> eligibleCollectables = new List<CollectableType>();

        private bool explodedChocolate;

        private class Swap
        {
            public GameObject tileA;
            public GameObject tileB;
        }

        private enum SwapDirection
        {
            Horizontal,
            Vertical
        }

        private SwapDirection swapDirection;

        private bool currentlySwapping;

        private readonly MatchDetector horizontalMatchDetector = new HorizontalMatchDetector();
        private readonly MatchDetector verticalMatchDetector = new VerticalMatchDetector();
        private readonly MatchDetector tShapedMatchDetector = new TshapedMatchDetector();
        private readonly MatchDetector lShapedMatchDetector = new LshapedMatchDetector();

        private int consecutiveCascades;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(gameScene);
            Assert.IsNotNull(gameUi);
            Assert.IsNotNull(boosterBar);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            SoundManager.instance.AddSounds(gameSounds);
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>
        protected void OnDestroy()
        {
            SoundManager.instance.RemoveSounds(gameSounds);
        }

        /// <summary>
        /// Loads the current level.
        /// </summary>
        public void LoadLevel()
        {
            var serializer = new fsSerializer();
            gameConfig = FileUtils.LoadJsonFile<GameConfiguration>(serializer, "game_configuration");

            ResetLevelData();
        }

        /// <summary>
        /// Updates the score of the current game.
        /// </summary>
        /// <param name="score">The score.</param>
        private void UpdateScore(int score)
        {
            gameState.score += score;
            gameUi.SetScore(gameState.score);
            gameUi.SetProgressBar(gameState.score);
        }

        /// <summary>
        /// Resets the current level data.
        /// </summary>
        public void ResetLevelData()
        {
            var serializer = new fsSerializer();
            level = FileUtils.LoadJsonFile<Level>(serializer,
                "Levels/" + PuzzleMatchManager.instance.lastSelectedLevel);

            boosterBar.SetData(level);

            if (suggestedMatchCoroutine != null)
            {
                StopCoroutine(suggestedMatchCoroutine);
                suggestedMatchCoroutine = null;
            }

            ClearSuggestedMatch();

            tiles = new List<GameObject>(level.width * level.height);
            honeys = new List<GameObject>(level.width * level.height);
            ices = new List<GameObject>(level.width * level.height);
            syrups1 = new List<GameObject>(level.width * level.height);
            syrups2 = new List<GameObject>(level.width * level.height);

            eligibleCollectables.Clear();
            foreach (var goal in level.goals)
            {
                if (goal is CollectCollectableGoal)
                {
                    var collectableGoal = goal as CollectCollectableGoal;
                    for (var i = 0; i < collectableGoal.amount; i++)
                    {
                        eligibleCollectables.Add(collectableGoal.collectableType);
                    }
                }
            }

            currentLimit = level.limit;
            currentlyAwarding = false;

            consecutiveCascades = 0;

            explodedChocolate = false;

            gameState.Reset();

            gameUi.SetLimitType(level.limitType);
            gameUi.SetLimit(level.limit);
            gameUi.SetGoals(level.goals, true);
            gameUi.InitializeProgressBar(level.score1, level.score2, level.score3);
            UpdateScore(0);

            foreach (var pool in tilePool.GetComponentsInChildren<ObjectPool>())
            {
                pool.Reset();
            }

            foreach (var pool in fxPool.GetComponentsInChildren<ObjectPool>())
            {
                pool.Reset();
            }

            tilePositions.Clear();
            possibleSwaps.Clear();

            const float horizontalSpacing = 0.0f;
            const float verticalSpacing = 0.0f;

            for (var j = 0; j < level.height; j++)
            {
                for (var i = 0; i < level.width; i++)
                {
                    var levelTile = level.tiles[i + (j * level.width)];
                    var tile = CreateTileFromLevel(levelTile, i, j);
                    if (tile != null)
                    {
                        var spriteRenderer = tile.GetComponent<SpriteRenderer>();
                        tileW = spriteRenderer.bounds.size.x;
                        tileH = spriteRenderer.bounds.size.y;
                        tile.transform.position =
                            new Vector2(i * (tileW + horizontalSpacing), -j * (tileH + verticalSpacing));

                        var collectable = tile.GetComponent<Collectable>();
                        if (collectable != null)
                        {
                            var cidx = eligibleCollectables.FindIndex(x => x == collectable.type);
                            if (cidx != -1)
                            {
                                eligibleCollectables.RemoveAt(cidx);
                            }

                        }
                    }

                    tiles.Add(tile);
                }
            }

            var totalWidth = (level.width - 1) * (tileW + horizontalSpacing);
            var totalHeight = (level.height - 1) * (tileH + verticalSpacing);
            for (var j = 0; j < level.height; j++)
            {
                for (var i = 0; i < level.width; i++)
                {
                    var tilePos = new Vector2(i * (tileW + horizontalSpacing), -j * (tileH + verticalSpacing));
                    var newPos = tilePos;
                    newPos.x -= totalWidth / 2;
                    newPos.y += totalHeight / 2;
                    newPos.y += boardCenter.position.y;
                    var tile = tiles[i + (j * level.width)];
                    if (tile != null)
                    {
                        tile.transform.position = newPos;
                    }

                    tilePositions.Add(newPos);

                    var levelTile = level.tiles[i + (j * level.width)];
                    if (!(levelTile is HoleTile))
                    {
                        GameObject bgTile;
                        if (j % 2 == 0)
                        {
                            bgTile = i % 2 == 0
                                ? tilePool.darkBgTilePool.GetObject()
                                : tilePool.lightBgTilePool.GetObject();
                        }
                        else
                        {
                            bgTile = i % 2 == 0
                                ? tilePool.lightBgTilePool.GetObject()
                                : tilePool.darkBgTilePool.GetObject();
                        }

                        bgTile.transform.position = newPos;
                    }
                }
            }

            for (var j = 0; j < level.height; j++)
            {
                for (var i = 0; i < level.width; i++)
                {
                    var levelTile = level.tiles[i + (j * level.width)];
                    if (levelTile != null && levelTile.elementType == ElementType.Honey)
                    {
                        var honey = tilePool.honeyPool.GetObject();
                        honey.transform.position = tilePositions[i + (j * level.width)];
                        honey.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        honeys.Add(honey);
                        ices.Add(null);
                        syrups1.Add(null);
                        syrups2.Add(null);
                    }
                    else if (levelTile != null && levelTile.elementType == ElementType.Ice)
                    {
                        var ice = tilePool.icePool.GetObject();
                        ice.transform.position = tilePositions[i + (j * level.width)];
                        ice.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        ices.Add(ice);
                        honeys.Add(null);
                        syrups1.Add(null);
                        syrups2.Add(null);
                    }
                    else if (levelTile != null && levelTile.elementType == ElementType.Syrup1)
                    {
                        var syrup = tilePool.syrup1Pool.GetObject();
                        syrup.transform.position = tilePositions[i + (j * level.width)];
                        syrup.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        ices.Add(null);
                        honeys.Add(null);
                        syrups1.Add(syrup);
                        syrups2.Add(null);
                    }
                    else if (levelTile != null && levelTile.elementType == ElementType.Syrup2)
                    {
                        var syrup = tilePool.syrup2Pool.GetObject();
                        syrup.transform.position = tilePositions[i + (j * level.width)];
                        syrup.GetComponent<SpriteRenderer>().sortingOrder = -1;
                        ices.Add(null);
                        honeys.Add(null);
                        syrups1.Add(null);
                        syrups2.Add(syrup);
                    }
                    else
                    {
                        honeys.Add(null);
                        ices.Add(null);
                        syrups1.Add(null);
                        syrups2.Add(null);
                    }
                }
            }

            possibleSwaps = DetectPossibleSwaps();
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public void StartGame()
        {
            if (level.limitType == LimitType.Time)
            {
                countdownCoroutine = StartCoroutine(StartCountdown());
            }

            suggestedMatchCoroutine = StartCoroutine(HighlightRandomMatchAsync());
        }

        /// <summary>
        /// Ends the current game.
        /// </summary>
        public void EndGame()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
        }

        /// <summary>
        /// Continues the current game.
        /// </summary>
        public void Continue()
        {
            if (level.limitType == LimitType.Moves)
            {
                currentLimit = gameConfig.numExtraMoves;
                gameUi.SetLimit(currentLimit);
            }
            else if (level.limitType == LimitType.Time)
            {
                currentLimit = gameConfig.numExtraTime;
                gameUi.SetLimit(currentLimit);
                countdownCoroutine = StartCoroutine(StartCountdown());
            }
        }

        /// <summary>
        /// Starts the level countdown (used only in time-limited levels).
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator StartCountdown()
        {
            while (currentLimit > 0)
            {
                --currentLimit;
                UpdateLimitText();
                yield return new WaitForSeconds(1.0f);
            }

            gameScene.CheckEndGame();
        }

        /// <summary>
        /// Updates the limit text.
        /// </summary>
        private void UpdateLimitText()
        {
            if (level.limitType == LimitType.Moves)
            {
                gameUi.SetLimit(currentLimit);
            }
            else if (level.limitType == LimitType.Time)
            {
                var timeSpan = TimeSpan.FromSeconds(currentLimit);
                gameUi.SetLimit(string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds));
            }
        }

        /// <summary>
        /// Handles the player's input.
        /// </summary>
        public void HandleInput()
        {
            if (inputLocked)
            {
                return;
            }

            if (currentlySwapping)
            {
                return;
            }

            if (currentlyAwarding)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                drag = true;
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Tile"))
                {
                    var idx = tiles.FindIndex(x => x == hit.collider.gameObject);
                    if (level.tiles[idx] != null && level.tiles[idx].elementType == ElementType.Ice)
                    {
                        return;
                    }

                    if (hit.collider.GetComponent<SpecialBlock>() != null)
                    {
                        return;
                    }

                    selectedTile = hit.collider.gameObject;
                    selectedTile.GetComponent<Animator>().SetTrigger("Pressed");
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                drag = false;
                if (selectedTile != null && selectedTile.GetComponent<Animator>() != null && selectedTile.gameObject.activeSelf)
                {
                    selectedTile.GetComponent<Animator>().SetTrigger("Unpressed");
                }
            }

            if (drag && selectedTile != null)
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject != selectedTile)
                {
                    if (hit.collider.GetComponent<SpecialBlock>() != null)
                    {
                        return;
                    }

                    if (selectedTile.GetComponent<Animator>() != null && selectedTile.gameObject.activeSelf)
                    {
                        selectedTile.GetComponent<Animator>().SetTrigger("Unpressed");
                    }

                    var idx = tiles.FindIndex(x => x == hit.collider.gameObject);
                    if (level.tiles[idx] != null && level.tiles[idx].elementType == ElementType.Ice)
                    {
                        return;
                    }

                    var idxSelected = tiles.FindIndex(x => x == selectedTile);
                    var xSelected = idxSelected % level.width;
                    var ySelected = idxSelected / level.width;
                    var idxNew = tiles.FindIndex(x => x == hit.collider.gameObject);
                    var xNew = idxNew % level.width;
                    var yNew = idxNew / level.width;
                    if (Math.Abs(xSelected - xNew) > 1 || Math.Abs(ySelected - yNew) > 1)
                    {
                        return;
                    }

                    var combo = comboDetector.GetCombo(hit.collider.gameObject.GetComponent<Tile>(),
                        selectedTile.GetComponent<Tile>());
                    if (combo != null)
                    {
                        var selectedTileCopy = selectedTile;
                        selectedTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        currentlySwapping = true;
                        LeanTween.move(selectedTile, hit.collider.gameObject.transform.position, 0.25f).setOnComplete(
                            () =>
                            {
                                currentlySwapping = false;
                                selectedTileCopy.GetComponent<SpriteRenderer>().sortingOrder = 0;
                                combo.Resolve(this, tiles, fxPool);
                            });
                        LeanTween.move(hit.collider.gameObject, selectedTile.transform.position, 0.25f);

                        var tileA = hit.collider.gameObject;
                        var tileB = selectedTile;
                        var idxA = tiles.FindIndex(x => x == tileA);
                        var idxB = tiles.FindIndex(x => x == tileB);
                        tiles[idxA] = tileB;
                        tiles[idxB] = tileA;

                        tileA.GetComponent<Tile>().x = idxB % level.width;
                        tileA.GetComponent<Tile>().y = idxB / level.width;
                        tileB.GetComponent<Tile>().x = idxA % level.width;
                        tileB.GetComponent<Tile>().y = idxA / level.width;

                        lastSelectedTile = selectedTile;
                        lastSelectedTileX = idxA % level.width;
                        lastSelectedTileY = idxA / level.width;
                        
                        lastOtherSelectedTile = hit.collider.gameObject;
                        lastOtherSelectedTileX = idxB % level.width;
                        lastOtherSelectedTileY = idxB / level.width;

                        selectedTile = null;

                        PerformMove();
                    }
                    else if (possibleSwaps.Find(x => x.tileA == hit.collider.gameObject && x.tileB == selectedTile) !=
                             null ||
                             possibleSwaps.Find(x => x.tileB == hit.collider.gameObject && x.tileA == selectedTile) !=
                             null)
                    {
                        var selectedTileCopy = selectedTile;
                        selectedTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        currentlySwapping = true;
                        LeanTween.move(selectedTile, hit.collider.gameObject.transform.position, 0.25f).setOnComplete(
                            () =>
                            {
                                currentlySwapping = false;
                                selectedTileCopy.GetComponent<SpriteRenderer>().sortingOrder = 0;
                                HandleMatches(true);
                            });
                        LeanTween.move(hit.collider.gameObject, selectedTile.transform.position, 0.25f);

                        var tileA = hit.collider.gameObject;
                        var tileB = selectedTile;
                        var idxA = tiles.FindIndex(x => x == tileA);
                        var idxB = tiles.FindIndex(x => x == tileB);
                        tiles[idxA] = tileB;
                        tiles[idxB] = tileA;

                        if (tileA.GetComponent<Tile>().x != tileB.GetComponent<Tile>().x)
                        {
                            swapDirection = SwapDirection.Horizontal;
                        }
                        else
                        {
                            swapDirection = SwapDirection.Vertical;
                        }

                        tileA.GetComponent<Tile>().x = idxB % level.width;
                        tileA.GetComponent<Tile>().y = idxB / level.width;
                        tileB.GetComponent<Tile>().x = idxA % level.width;
                        tileB.GetComponent<Tile>().y = idxA / level.width;

                        lastSelectedTile = selectedTile;
                        lastSelectedTileX = idxA % level.width;
                        lastSelectedTileY = idxA / level.width;
                        if (selectedTileCopy.GetComponent<Candy>() != null)
                        {
                            lastSelectedCandyColor = selectedTile.GetComponent<Candy>().color;
                        }
                        
                        lastOtherSelectedTile = hit.collider.gameObject;
                        lastOtherSelectedTileX = idxB % level.width;
                        lastOtherSelectedTileY = idxB / level.width;
                        if (hit.collider.gameObject.GetComponent<Candy>() != null)
                        {
                            lastOtherSelectedCandyColor = hit.collider.gameObject.GetComponent<Candy>().color;
                        }

                        selectedTile = null;

                        possibleSwaps = DetectPossibleSwaps();

                        PerformMove();
                    }
                    else
                    {
                        var selectedTileCopy = selectedTile;
                        var hitTileCopy = hit.collider.gameObject;
                        selectedTile.GetComponent<SpriteRenderer>().sortingOrder = 1;

                        var selectedTilePos = selectedTile.transform.position;
                        var hitTilePos = hit.collider.gameObject.transform.position;

                        var tileA = hit.collider.gameObject;
                        var tileB = selectedTile;
                        if (!(tileA.GetComponent<Tile>().x != tileB.GetComponent<Tile>().x &&
                              tileA.GetComponent<Tile>().y != tileB.GetComponent<Tile>().y))
                        {
                            currentlySwapping = true;
                            LeanTween.move(selectedTile, hitTilePos, 0.2f);
                            LeanTween.move(hit.collider.gameObject, selectedTilePos, 0.2f).setOnComplete(() =>
                            {
                                LeanTween.move(selectedTileCopy, selectedTilePos, 0.2f).setOnComplete(() =>
                                {
                                    currentlySwapping = false;
                                    selectedTileCopy.GetComponent<SpriteRenderer>().sortingOrder = 0;
                                });
                                LeanTween.move(hitTileCopy, hitTilePos, 0.2f);
                            });
                        }

                        selectedTile = null;

                        SoundManager.instance.PlaySound("Error");
                    }
                }
            }
        }

        /// <summary>
        /// Handles the player's input when the game is in booster mode.
        /// </summary>
        public void HandleBoosterInput(BuyBoosterButton button)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Tile"))
                {
                    if (hit.collider.GetComponent<Unbreakable>() != null ||
                        hit.collider.GetComponent<Collectable>() != null)
                    {
                        return;
                    }

                    var tile = hit.collider.GetComponent<Tile>();
                    Booster booster = null;
                    switch (button.boosterType)
                    {
                        case BoosterType.Lollipop:
                            booster = new LollipopBooster();
                            break;

                        case BoosterType.Bomb:
                            booster = new BombBooster();
                            break;

                        case BoosterType.ColorBomb:
                            booster = new ColorBombBooster();
                            break;
                    }

                    if (booster != null)
                    {
                        booster.Resolve(this, tile.gameObject);
                        ConsumeBooster(button);
                        ApplyGravity();
                    }

                    gameScene.DisableBoosterMode();

                    selectedTile = hit.collider.gameObject;
                }
                else
                {
                    gameScene.DisableBoosterMode();
                }
            }
        }

        /// <summary>
        /// Handles the player's input when the game is in booster mode and the booster used is the switch.
        /// </summary>
        public void HandleSwitchBoosterInput(BuyBoosterButton button)
        {
            if (Input.GetMouseButtonDown(0))
            {
                drag = true;
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Tile"))
                {
                    selectedTile = hit.collider.gameObject;
                }
                else
                {
                    gameScene.DisableBoosterMode();
                    return;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                drag = false;
            }

            if (drag && selectedTile != null)
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider.gameObject != selectedTile)
                {
                    var selectedTileCopy = selectedTile;
                    selectedTile.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    LeanTween.move(selectedTile, hit.collider.gameObject.transform.position, 0.25f).setOnComplete(
                        () =>
                        {
                            selectedTileCopy.GetComponent<SpriteRenderer>().sortingOrder = 0;
                            gameScene.DisableBoosterMode();
                            HandleMatches(true);
                            ConsumeBooster(button);
                        });
                    LeanTween.move(hit.collider.gameObject, selectedTile.transform.position, 0.25f);

                    var tileA = hit.collider.gameObject;
                    var tileB = selectedTile;
                    var idxA = tiles.FindIndex(x => x == tileA);
                    var idxB = tiles.FindIndex(x => x == tileB);
                    tiles[idxA] = tileB;
                    tiles[idxB] = tileA;

                    tileA.GetComponent<Tile>().x = idxB % level.width;
                    tileA.GetComponent<Tile>().y = idxB / level.width;
                    tileB.GetComponent<Tile>().x = idxA % level.width;
                    tileB.GetComponent<Tile>().y = idxA / level.width;

                    selectedTile = null;

                    possibleSwaps = DetectPossibleSwaps();
                }
            }
        }

        /// <summary>
        /// Performs a move.
        /// </summary>
        private void PerformMove()
        {
            ClearSuggestedMatch();

            if (level.limitType == LimitType.Moves)
            {
                currentLimit -= 1;
                if (currentLimit < 0)
                {
                    currentLimit = 0;
                }

                gameUi.SetLimit(currentLimit);
            }
        }

        /// <summary>
        /// Applies the gravity to the level tiles.
        /// </summary>
        public void ApplyGravity()
        {
            StartCoroutine(ApplyGravityAsync());
        }

        /// <summary>
        /// Explodes the specified generated tiles.
        /// </summary>
        /// <param name="genTiles">The list of generated tiles.</param>
        public void ExplodeGeneratedTiles(List<GameObject> genTiles)
        {
            StartCoroutine(ExplodeGeneratedTilesAsync(genTiles));
        }

        /// <summary>
        /// Explodes the specified generated tiles.
        /// </summary>
        /// <param name="genTiles">The list of generated tiles.</param>
        /// <returns>The coroutine.</returns>
        private IEnumerator ExplodeGeneratedTilesAsync(List<GameObject> genTiles)
        {
            yield return new WaitForSeconds(1.5f);

            foreach (var tile in genTiles)
            {
                ExplodeTile(tile);
            }

            StartCoroutine(ApplyGravityAsync(0.5f));
        }

        /// <summary>
        /// Creates a new tile from the specified level data.
        /// </summary>
        /// <param name="levelTile">The level tile.</param>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <returns>The new tile created from the specified level data.</returns>
        private GameObject CreateTileFromLevel(LevelTile levelTile, int x, int y)
        {
            if (levelTile is CandyTile)
            {
                var candyTile = (CandyTile) levelTile;
                if (candyTile.type == CandyType.RandomCandy)
                {
                    return CreateTile(x, y, false);
                }
                else
                {
                    var tile = tilePool.GetCandyPool((CandyColor) ((int) candyTile.type)).GetObject();
                    tile.GetComponent<Tile>().board = this;
                    tile.GetComponent<Tile>().x = x;
                    tile.GetComponent<Tile>().y = y;
                    return tile;
                }
            }

            if (levelTile is SpecialCandyTile)
            {
                GameObject tile;

                var specialCandyTile = (SpecialCandyTile) levelTile;
                var specialCandyType = (int) specialCandyTile.type;
                if (specialCandyType >= 0 &&
                    specialCandyType <= (int) SpecialCandyType.YellowCandyHorizontalStriped)
                {
                    tile = tilePool.GetStripedCandyPool(StripeDirection.Horizontal, (CandyColor) (specialCandyType % 6))
                        .GetObject();
                }
                else if (specialCandyType <= (int) SpecialCandyType.YellowCandyVerticalStriped)
                {
                    tile = tilePool.GetStripedCandyPool(StripeDirection.Vertical, (CandyColor) (specialCandyType % 6))
                        .GetObject();
                }
                else if (specialCandyType <= (int) SpecialCandyType.YellowCandyWrapped)
                {
                    tile = tilePool.GetWrappedCandyPool((CandyColor) (specialCandyType % 6)).GetObject();
                }
                else
                {
                    tile = tilePool.colorBombCandyPool.GetObject();
                }

                tile.GetComponent<Tile>().board = this;
                tile.GetComponent<Tile>().x = x;
                tile.GetComponent<Tile>().y = y;
                return tile;
            }

            if (levelTile is SpecialBlockTile)
            {
                var specialBlockTile = (SpecialBlockTile) levelTile;
                var block = tilePool.GetSpecialBlockPool(specialBlockTile.type).GetObject();
                block.GetComponent<Tile>().board = this;
                block.GetComponent<Tile>().x = x;
                block.GetComponent<Tile>().y = y;
                return block;
            }

            if (levelTile is CollectableTile)
            {
                var collectableTile = (CollectableTile) levelTile;
                var tile = tilePool.GetCollectablePool(collectableTile.type).GetObject();
                tile.GetComponent<Tile>().board = this;
                tile.GetComponent<Tile>().x = x;
                tile.GetComponent<Tile>().y = y;
                return tile;
            }

            return null;
        }

        /// <summary>
        /// Creates a new, random tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <param name="runtime">True if this tile is created during a game; false otherwise.</param>
        /// <returns>The newly created tile.</returns>
        private GameObject CreateTile(int x, int y, bool runtime)
        {
            var eligibleTiles = new List<CandyColor>();
            eligibleTiles.AddRange(level.availableColors);

            var leftTile1 = GetTile(x - 1, y);
            var leftTile2 = GetTile(x - 2, y);
            if (leftTile1 != null && leftTile2 != null &&
                leftTile1.GetComponent<Candy>() != null && leftTile2.GetComponent<Candy>() != null &&
                leftTile1.GetComponent<Candy>().color == leftTile2.GetComponent<Candy>().color)
            {
                var tileToRemove = eligibleTiles.Find(t =>
                    t == leftTile1.GetComponent<Candy>().color);
                eligibleTiles.Remove(tileToRemove);
            }

            var topTile1 = GetTile(x, y - 1);
            var topTile2 = GetTile(x, y - 2);
            if (topTile1 != null && topTile2 != null &&
                topTile1.GetComponent<Candy>() != null && topTile2.GetComponent<Candy>() != null &&
                topTile1.GetComponent<Candy>().color == topTile2.GetComponent<Candy>().color)
            {
                var tileToRemove = eligibleTiles.Find(t =>
                    t == topTile1.GetComponent<Candy>().color);
                eligibleTiles.Remove(tileToRemove);
            }

            GameObject tile;
            if (runtime && eligibleCollectables.Count > 0)
            {
                var tileChance = UnityEngine.Random.Range(0, 100);
                if (tileChance <= level.collectableChance)
                {
                    var idx = UnityEngine.Random.Range(0, eligibleCollectables.Count);
                    var collectable = eligibleCollectables[idx];
                    tile = tilePool.GetCollectablePool(collectable).GetObject();
                    eligibleCollectables.RemoveAt(idx);
                }
                else
                {
                    tile = tilePool.GetCandyPool(eligibleTiles[UnityEngine.Random.Range(0, eligibleTiles.Count)])
                        .GetObject();
                }
            }
            else
            {
                tile = tilePool.GetCandyPool(eligibleTiles[UnityEngine.Random.Range(0, eligibleTiles.Count)])
                    .GetObject();
            }

            tile.GetComponent<Tile>().board = this;
            tile.GetComponent<Tile>().x = x;
            tile.GetComponent<Tile>().y = y;
            return tile;
        }

        /// <summary>
        /// Creates a new horizontally striped tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <param name="color">The color of the tile.</param>
        /// <returns>The newly created tile.</returns>
        public GameObject CreateHorizontalStripedTile(int x, int y, CandyColor color)
        {
            var tileIdx = x + (y * level.width);
            var tile = tilePool.GetStripedCandyPool(StripeDirection.Horizontal, color).GetObject();
            tile.GetComponent<Tile>().board = this;
            tile.GetComponent<Tile>().x = x;
            tile.GetComponent<Tile>().y = y;
            tile.transform.position = tilePositions[tileIdx];
            tiles[tileIdx] = tile;
            CreateSpawnParticles(tile.transform.position);
            return tile;
        }

        /// <summary>
        /// Creates a new vertically striped tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <param name="color">The color of the tile.</param>
        /// <returns>The newly created tile.</returns>
        public GameObject CreateVerticalStripedTile(int x, int y, CandyColor color)
        {
            var tileIdx = x + (y * level.width);
            var tile = tilePool.GetStripedCandyPool(StripeDirection.Vertical, color).GetObject();
            tile.GetComponent<Tile>().board = this;
            tile.GetComponent<Tile>().x = x;
            tile.GetComponent<Tile>().y = y;
            tile.transform.position = tilePositions[tileIdx];
            tiles[tileIdx] = tile;
            CreateSpawnParticles(tile.transform.position);
            return tile;
        }

        /// <summary>
        /// Creates a new wrapped tile.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <param name="color">The color of the tile.</param>
        /// <returns>The newly created tile.</returns>
        public GameObject CreateWrappedTile(int x, int y, CandyColor color)
        {
            var tileIdx = x + (y * level.width);
            var tile = tilePool.GetWrappedCandyPool(color).GetObject();
            tile.GetComponent<Tile>().board = this;
            tile.GetComponent<Tile>().x = x;
            tile.GetComponent<Tile>().y = y;
            tile.transform.position = tilePositions[tileIdx];
            tiles[tileIdx] = tile;
            CreateSpawnParticles(tile.transform.position);
            return tile;
        }

        /// <summary>
        /// Creates a new color bomb.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <returns>The newly created tile.</returns>
        public GameObject CreateColorBomb(int x, int y)
        {
            var tileIdx = x + (y * level.width);
            var tile = tilePool.colorBombCandyPool.GetObject();
            tile.GetComponent<Tile>().board = this;
            tile.GetComponent<Tile>().x = x;
            tile.GetComponent<Tile>().y = y;
            tile.transform.position = tilePositions[tileIdx];
            tiles[tileIdx] = tile;
            CreateSpawnParticles(tile.transform.position);
            return tile;
        }

        /// <summary>
        /// Creates a new chocolate.
        /// </summary>
        /// <param name="x">The x-coordinate of the tile.</param>
        /// <param name="y">The y-coordinate of the tile.</param>
        /// <returns>The newly created tile.</returns>
        public GameObject CreateChocolate(int x, int y)
        {
            var tileIdx = x + (y * level.width);
            var tile = tilePool.chocolatePool.GetObject();
            tile.GetComponent<Tile>().board = this;
            tile.GetComponent<Tile>().x = x;
            tile.GetComponent<Tile>().y = y;
            tile.transform.position = tilePositions[tileIdx];
            tiles[tileIdx] = tile;
            CreateSpawnParticles(tile.transform.position);
            return tile;
        }

        /// <summary>
        /// Creates the spawn particles at the specified position.
        /// </summary>
        /// <param name="position">The position of the particles.</param>
        private void CreateSpawnParticles(Vector2 position)
        {
            var particles = fxPool.spawnParticles.GetObject();
            particles.transform.position = position;
            foreach (var child in particles.GetComponentsInChildren<ParticleSystem>())
            {
                child.Play();
            }
        }

        /// <summary>
        /// Returns the tile at coordinates (x, y).
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>The tile at coordinates (x, y).</returns>
        public GameObject GetTile(int x, int y)
        {
            if (x >= 0 && x < level.width && y >= 0 && y < level.height)
            {
                return tiles[x + (y * level.width)];
            }

            return null;
        }

        /// <summary>
        /// Replaces the tile at coordinates (x, y).
        /// </summary>
        /// <param name="tile">The new tile.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        private void SetTile(GameObject tile, int x, int y)
        {
            if (x >= 0 && x < level.width && y >= 0 && y < level.height)
            {
                tiles[x + (y * level.width)] = tile;
            }
        }

        /// <summary>
        /// Explodes the specified tile.
        /// </summary>
        /// <param name="tile">The tile to explode.</param>
        /// <param name="didAnySpecialCandyExplode">True if any special candy exploded; false otherwise.</param>
        public void ExplodeTile(GameObject tile, bool didAnySpecialCandyExplode = false)
        {
            var explodedTiles = new List<GameObject>();
            ExplodeTileRecursive(tile, explodedTiles);
            var score = 0;
            foreach (var explodedTile in explodedTiles)
            {
                var idx = tiles.FindIndex(x => x == explodedTile);
                if (idx != -1)
                {
                    explodedTile.GetComponent<Tile>().ShowExplosionFx(fxPool);
                    explodedTile.GetComponent<Tile>().UpdateGameState(gameState);
                    score += gameConfig.GetTileScore(explodedTile.GetComponent<Tile>());
                    DestroyElements(explodedTile);
                    DestroySpecialBlocks(explodedTile, didAnySpecialCandyExplode);
                    explodedTile.GetComponent<PooledObject>().pool.ReturnObject(explodedTile);
                    tiles[idx] = null;
                }

                SoundManager.instance.PlaySound("CandyMatch");
            }

            UpdateScore(score);
            gameUi.UpdateGoals(gameState);
        }

        /// <summary>
        /// Explodes the specified tile non-recursively.
        /// </summary>
        /// <param name="tile">The tile to explode.</param>
        public void ExplodeTileNonRecursive(GameObject tile)
        {
            if (tile != null)
            {
                if (tile.GetComponent<Collectable>() != null)
                {
                    return;
                }

                if (tile.GetComponent<Tile>() != null && !tile.GetComponent<Tile>().destructable)
                {
                    return;
                }

                var idx = tiles.FindIndex(x => x == tile);
                if (idx != -1)
                {
                    tile.GetComponent<Tile>().ShowExplosionFx(fxPool);
                    tile.GetComponent<Tile>().UpdateGameState(gameState);
                    UpdateScore(gameConfig.GetTileScore(tile.GetComponent<Tile>()));
                    DestroyElements(tile);
                    
                    tile.GetComponent<PooledObject>().pool.ReturnObject(tile);
                    tiles[idx] = null;

                    var chocolates = tiles.FindAll(t => t != null && t.GetComponent<Chocolate>() != null);
                    if (chocolates.Count == 0)
                    {
                        gameState.destroyedAllChocolates = true;
                    }
                    
                    gameUi.UpdateGoals(gameState);
                    
                    SoundManager.instance.PlaySound("CandyMatch");
                }
            }
        }

        /// <summary>
        /// Explodes the specified tile recursively.
        /// </summary>
        /// <param name="tile">The tile to explode.</param>
        /// <param name="explodedTiles">The list of the exploded tiles so far.</param>
        private void ExplodeTileRecursive(GameObject tile, List<GameObject> explodedTiles)
        {
            if (tile != null && tile.GetComponent<Tile>() != null)
            {
                var newTilesToExplode = tile.GetComponent<Tile>().Explode();

                explodedTiles.Add(tile);

                foreach (var t in newTilesToExplode)
                {
                    if (t != null && t.GetComponent<Tile>() != null && t.GetComponent<Tile>().destructable &&
                        !explodedTiles.Contains(t))
                    {
                        explodedTiles.Add(t);
                        ExplodeTileRecursive(t, explodedTiles);
                    }
                }

                foreach (var t in newTilesToExplode)
                {
                    if (!newTilesToExplode.Contains(t))
                    {
                        newTilesToExplode.Add(t);
                    }
                }
            }
        }

        /// <summary>
        /// Destroys the elements at the cell occupied by the specified tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        private void DestroyElements(GameObject tile)
        {
            var idx = tile.GetComponent<Tile>().x + (tile.GetComponent<Tile>().y * level.width);
            // Check for honey.
            if (idx != -1 && level.tiles[idx] != null && level.tiles[idx].elementType == ElementType.Honey)
            {
                honeys[idx].GetComponent<PooledObject>().pool.ReturnObject(honeys[idx]);
                level.tiles[idx].elementType = ElementType.None;
                honeys[idx] = null;
                gameState.AddElement(ElementType.Honey);
                UpdateScore(gameConfig.GetElementScore(ElementType.Honey));

                var fx = fxPool.GetElementExplosion(ElementType.Honey).GetObject();
                fx.transform.position = tilePositions[idx];

                SoundManager.instance.PlaySound("Honey");
            }

            // Check for syrup x1.
            if (idx != -1 && level.tiles[idx] != null && level.tiles[idx].elementType == ElementType.Syrup1)
            {
                syrups1[idx].GetComponent<PooledObject>().pool.ReturnObject(syrups1[idx]);
                level.tiles[idx].elementType = ElementType.None;
                syrups1[idx] = null;
                gameState.AddElement(ElementType.Syrup1);
                UpdateScore(gameConfig.GetElementScore(ElementType.Syrup1));

                var fx = fxPool.GetElementExplosion(ElementType.Syrup1).GetObject();
                fx.transform.position = tilePositions[idx];

                SoundManager.instance.PlaySound("Syrup");
            }

            // Check for syrup x2.
            if (idx != -1 && level.tiles[idx] != null && level.tiles[idx].elementType == ElementType.Syrup2)
            {
                var syrup = tilePool.syrup1Pool.GetObject();
                syrup.transform.position = syrups2[idx].transform.position;
                syrup.GetComponent<SpriteRenderer>().sortingOrder = -1;

                syrups2[idx].GetComponent<PooledObject>().pool.ReturnObject(syrups2[idx]);
                level.tiles[idx].elementType = ElementType.Syrup1;
                syrups2[idx] = null;
                syrups1[idx] = syrup;

                gameState.AddElement(ElementType.Syrup2);
                UpdateScore(gameConfig.GetElementScore(ElementType.Syrup2));

                var fx = fxPool.GetElementExplosion(ElementType.Syrup2).GetObject();
                fx.transform.position = tilePositions[idx];

                SoundManager.instance.PlaySound("Syrup");
            }

            // Check for ices.
            if (idx != -1 && level.tiles[idx] != null && level.tiles[idx].elementType == ElementType.Ice)
            {
                ices[idx].GetComponent<PooledObject>().pool.ReturnObject(ices[idx]);
                level.tiles[idx].elementType = ElementType.None;
                ices[idx] = null;
                gameState.AddElement(ElementType.Ice);
                UpdateScore(gameConfig.GetElementScore(ElementType.Ice));

                var fx = fxPool.GetElementExplosion(ElementType.Ice).GetObject();
                fx.transform.position = tilePositions[idx];

                SoundManager.instance.PlaySound("Ice");
            }

        }

        /// <summary>
        /// Destroys the special blocks at the cell occupied by the specified tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <param name="didAnySpecialCandyExplode">True if any special candy exploded; false otherwise.</param>
        private void DestroySpecialBlocks(GameObject tile, bool didAnySpecialCandyExplode)
        {
            if (!didAnySpecialCandyExplode)
            {
                var x = tile.GetComponent<Tile>().x;
                var y = tile.GetComponent<Tile>().y;
                var leftTile = GetTile(x - 1, y);
                var rightTile = GetTile(x + 1, y);
                var topTile = GetTile(x, y + 1);
                var bottomTile = GetTile(x, y - 1);
                var neighbourTiles = new List<GameObject> {leftTile, rightTile, topTile, bottomTile};
                foreach (var neighbour in neighbourTiles)
                {
                    DestroySpecialBlocksInternal(neighbour);
                }
                    
                DestroySpecialBlocksInternal(tile);

                var chocolates = tiles.FindAll(t => t != null && t.GetComponent<Chocolate>() != null);
                if (chocolates.Count == 0)
                {
                    gameState.destroyedAllChocolates = true;
                }
            }
        }

        /// <summary>
        /// Destroys the special blocks at the cell occupied by the specified tile.
        /// </summary>
        /// <param name="tile">The tile.</param>
        private void DestroySpecialBlocksInternal(GameObject tile)
        {
            if (tile != null && tile.GetComponent<SpecialBlock>() != null &&
                tile.GetComponent<SpecialBlock>().destructable)
            {
                var blockIdx = tiles.FindIndex(t => t == tile);
                if (blockIdx != -1)
                {
                    gameState.AddSpecialBlock(tile.GetComponent<SpecialBlock>().type);
                    UpdateScore(gameConfig.GetTileScore(tile.GetComponent<SpecialBlock>()));

                    var fx = fxPool.GetSpecialBlockExplosion(tile.GetComponent<SpecialBlock>().type).GetObject();
                    fx.transform.position = tile.transform.position;

                    tile.GetComponent<PooledObject>().pool.ReturnObject(tile);
                    tiles[blockIdx] = null;
                }

                if (tile.GetComponent<Chocolate>() != null)
                {
                    explodedChocolate = true;
                    SoundManager.instance.PlaySound("Chocolate");
                }
                else if (tile.GetComponent<Marshmallow>() != null)
                {
                    SoundManager.instance.PlaySound("Marshmallow");
                }
            }
        }

        /// <summary>
        /// Returns true if the tile at (x, y) has a match and false otherwise.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True if the tile at (x, y) has a match; false otherwise.</returns>
        private bool HasMatch(int x, int y)
        {
            return HasHorizontalMatch(x, y) || HasVerticalMatch(x, y);
        }

        /// <summary>
        /// Returns true if the tile at (x, y) has a horizontal match and false otherwise.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True if the tile at (x, y) has a horizontal match; false otherwise.</returns>
        private bool HasHorizontalMatch(int x, int y)
        {
            var tile = GetTile(x, y);
            if (tile.GetComponent<Candy>() != null)
            {
                var horzLen = 1;
                for (var i = x - 1;
                    i >= 0 && GetTile(i, y) != null && GetTile(i, y).GetComponent<Candy>() != null &&
                    GetTile(i, y).GetComponent<Candy>().color == tile.GetComponent<Candy>().color;
                    i--, horzLen++) ;
                for (var i = x + 1;
                    i < level.width && GetTile(i, y) != null && GetTile(i, y).GetComponent<Candy>() != null &&
                    GetTile(i, y).GetComponent<Candy>().color == tile.GetComponent<Candy>().color;
                    i++, horzLen++) ;
                if (horzLen >= 3) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the tile at (x, y) has a vertical match and false otherwise.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>True if the tile at (x, y) has a vertical match; false otherwise.</returns>
        private bool HasVerticalMatch(int x, int y)
        {
            var tile = GetTile(x, y);
            if (tile.GetComponent<Candy>() != null)
            {
                var vertLen = 1;
                for (var j = y - 1;
                    j >= 0 && GetTile(x, j) != null && GetTile(x, j).GetComponent<Candy>() != null &&
                    GetTile(x, j).GetComponent<Candy>().color == tile.GetComponent<Candy>().color;
                    j--, vertLen++) ;
                for (var j = y + 1;
                    j < level.height && GetTile(x, j) != null && GetTile(x, j).GetComponent<Candy>() != null &&
                    GetTile(x, j).GetComponent<Candy>().color == tile.GetComponent<Candy>().color;
                    j++, vertLen++) ;
                if (vertLen >= 3) return true;
            }

            return false;
        }

        /// <summary>
        /// Detects all the possible tile swaps in the current level.
        /// </summary>
        /// <returns>A list containing all the possible tile swaps in the level.</returns>
        private List<Swap> DetectPossibleSwaps()
        {
            var swaps = new List<Swap>();

            for (var j = 0; j < level.height; j++)
            {
                for (var i = 0; i < level.width; i++)
                {
                    var tile = GetTile(i, j);
                    if (tile != null)
                    {
                        if (i < level.width - 1)
                        {
                            var other = GetTile(i + 1, j);
                            if (other != null)
                            {
                                SetTile(other, i, j);
                                SetTile(tile, i + 1, j);

                                if (HasMatch(i, j) || HasMatch(i + 1, j))
                                {
                                    var swap = new Swap {tileA = tile, tileB = other};
                                    swaps.Add(swap);
                                }
                            }

                            SetTile(tile, i, j);
                            SetTile(other, i + 1, j);
                        }

                        if (j < level.height - 1)
                        {
                            var other = GetTile(i, j + 1);
                            if (other != null)
                            {
                                SetTile(other, i, j);
                                SetTile(tile, i, j + 1);

                                if (HasMatch(i, j) || HasMatch(i, j + 1))
                                {
                                    var swap = new Swap {tileA = tile, tileB = other};
                                    swaps.Add(swap);
                                }
                            }

                            SetTile(tile, i, j);
                            SetTile(other, i, j + 1);
                        }
                    }
                }
            }

            return swaps;
        }

        /// <summary>
        /// Resolves all the matches in the current level.
        /// </summary>
        /// <param name="isPlayerMatch">True if the match was caused by a player and false otherwise.</param>
        /// <returns>True if there were any matches; false otherwise.</returns>
        private bool HandleMatches(bool isPlayerMatch)
        {
            var matches = new List<Match>();
            var tShapedMatches = tShapedMatchDetector.DetectMatches(this);
            var lShapedMatches = lShapedMatchDetector.DetectMatches(this);
            var horizontalMatches = horizontalMatchDetector.DetectMatches(this);
            var verticalMatches = verticalMatchDetector.DetectMatches(this);

            if (tShapedMatches.Count > 0)
            {
                matches.AddRange(tShapedMatches);
                foreach (var match in horizontalMatches)
                {
                    var found = false;
                    foreach (var match2 in tShapedMatches)
                    {
                        if (match.tiles.Find(x => match2.tiles.Contains(x)) != null)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        matches.Add(match);
                    }
                }

                foreach (var match in verticalMatches)
                {
                    var found = false;
                    foreach (var match2 in tShapedMatches)
                    {
                        if (match.tiles.Find(x => match2.tiles.Contains(x)) != null)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        matches.Add(match);
                    }
                }
            }
            else if (lShapedMatches.Count > 0)
            {
                matches.AddRange(lShapedMatches);
                foreach (var match in horizontalMatches)
                {
                    var found = false;
                    foreach (var match2 in lShapedMatches)
                    {
                        if (match.tiles.Find(x => match2.tiles.Contains(x)) != null)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        matches.Add(match);
                    }
                }

                foreach (var match in verticalMatches)
                {
                    var found = false;
                    foreach (var match2 in lShapedMatches)
                    {
                        if (match.tiles.Find(x => match2.tiles.Contains(x)) != null)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        matches.Add(match);
                    }
                }
            }
            else if (horizontalMatches.Count > 0)
            {
                matches.AddRange(horizontalMatches);
                foreach (var match in verticalMatches)
                {
                    var found = false;
                    foreach (var match2 in horizontalMatches)
                    {
                        if (match.tiles.Find(x => match2.tiles.Contains(x)) != null)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        matches.Add(match);
                    }
                }
            }
            else
            {
                matches.AddRange(horizontalMatches);
                matches.AddRange(verticalMatches);
            }

            if (matches.Count > 0)
            {
                var didAnySpecialCandyExplode = false;
                var numSpecialCandiesGenerated = 0;

                foreach (var match in matches)
                {
                    var randomTile = match.tiles[UnityEngine.Random.Range(0, match.tiles.Count)];
                    var randomIdx = tiles.FindIndex(x => x == randomTile);
                    var randomColor = randomTile.GetComponent<Candy>().color;

                    if (match.tiles.Find(x =>
                        x.GetComponent<StripedCandy>() != null || x.GetComponent<WrappedCandy>() != null))
                    {
                        didAnySpecialCandyExplode = true;
                    }

                    foreach (var tile in match.tiles)
                    {
                        ExplodeTile(tile, didAnySpecialCandyExplode);
                    }

                    if (!didAnySpecialCandyExplode && numSpecialCandiesGenerated == 0)
                    {
                        if (match.tiles.Count >= 5 && match.type != MatchType.TShaped &&
                            match.type != MatchType.LShaped)
                        {
                            if (isPlayerMatch)
                            {
                                if (match.tiles.Contains(lastSelectedTile))
                                {
                                    CreateColorBomb(lastSelectedTileX, lastSelectedTileY);
                                }
                                else if (match.tiles.Contains(lastOtherSelectedTile))
                                {
                                    CreateColorBomb(lastOtherSelectedTileX, lastOtherSelectedTileY);
                                }
                            }
                            else if (randomIdx != -1)
                            {
                                var i = randomIdx % level.width;
                                var j = randomIdx / level.width;
                                CreateColorBomb(i, j);
                            }

                            ++numSpecialCandiesGenerated;
                        }
                        else if (match.tiles.Count >= 5)
                        {
                            if (isPlayerMatch) 
                            {
                                if (match.tiles.Contains(lastSelectedTile))
                                {
                                    CreateWrappedTile(lastSelectedTileX, lastSelectedTileY,
                                        lastSelectedCandyColor);
                                }
                                else if (match.tiles.Contains(lastOtherSelectedTile))
                                {
                                    CreateWrappedTile(lastOtherSelectedTileX, lastOtherSelectedTileY,
                                        lastOtherSelectedCandyColor);
                                }
                            }
                            else if (randomIdx != -1)
                            {
                                var i = randomIdx % level.width;
                                var j = randomIdx / level.width;
                                CreateWrappedTile(i, j, randomColor);
                            }
                            
                            ++numSpecialCandiesGenerated;
                        }
                        else if (match.tiles.Count >= 4)
                        {
                            if (swapDirection == SwapDirection.Horizontal)
                            {
                                if (isPlayerMatch)
                                {
                                    if (match.tiles.Contains(lastSelectedTile))
                                    {
                                        CreateHorizontalStripedTile(lastSelectedTileX, lastSelectedTileY,
                                            lastSelectedCandyColor);
                                    }
                                    else if (match.tiles.Contains(lastOtherSelectedTile))
                                    {
                                        CreateHorizontalStripedTile(lastOtherSelectedTileX, lastOtherSelectedTileY,
                                            lastOtherSelectedCandyColor);
                                    }
                                }
                                else if (randomIdx != -1)
                                {
                                    var i = randomIdx % level.width;
                                    var j = randomIdx / level.width;
                                    CreateHorizontalStripedTile(i, j, randomColor);
                                }
                            }
                            else
                            {
                                if (isPlayerMatch)
                                {
                                    if (match.tiles.Contains(lastSelectedTile))
                                    {
                                        CreateVerticalStripedTile(lastSelectedTileX, lastSelectedTileY,
                                            lastSelectedCandyColor);
                                    }
                                    else if (match.tiles.Contains(lastOtherSelectedTile))
                                    {
                                        CreateVerticalStripedTile(lastOtherSelectedTileX, lastOtherSelectedTileY,
                                            lastOtherSelectedCandyColor);
                                    }
                                }
                                else if (randomIdx != -1)
                                {
                                    var i = randomIdx % level.width;
                                    var j = randomIdx / level.width;
                                    CreateVerticalStripedTile(i, j, randomColor);
                                }
                            }
                            
                            ++numSpecialCandiesGenerated;
                        }
                    }
                }

                if (isPlayerMatch)
                {
                    consecutiveCascades = 0;
                }
                else
                {
                    consecutiveCascades += 1;
                    if (consecutiveCascades == 2)
                    {
                        gameScene.ShowComplimentText(ComplimentType.Good);
                    }
                    else if (consecutiveCascades == 4)
                    {
                        gameScene.ShowComplimentText(ComplimentType.Super);
                    }
                    else if (consecutiveCascades == 6)
                    {
                        gameScene.ShowComplimentText(ComplimentType.Yummy);
                    }
                }

                StartCoroutine(ApplyGravityAsync(didAnySpecialCandyExplode ? 0.5f : 0.0f));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The coroutine that applies the gravity to the current level.
        /// </summary>
        /// <param name="delay">The delay.</param>
        /// <returns>The coroutine.</returns>
        private IEnumerator ApplyGravityAsync(float delay = 0.0f)
        {
            ClearSuggestedMatch();
            if (suggestedMatchCoroutine != null)
            {
                StopCoroutine(suggestedMatchCoroutine);
                suggestedMatchCoroutine = null;
            }

            inputLocked = true;
            yield return new WaitForSeconds(delay);
            ApplyGravityInternal();
            possibleSwaps = DetectPossibleSwaps();
            yield return new WaitForSeconds(1.0f);
            if (currentlyAwarding)
            {
                gameScene.CheckEndGame();
            }
            else
            {
                if (!HandleMatches(false))
                {
                    if (suggestedMatchCoroutine != null)
                    {
                        StopCoroutine(suggestedMatchCoroutine);
                        suggestedMatchCoroutine = null;
                    }
                    ExpandChocolate();
                    inputLocked = false;
                    explodedChocolate = false;
                    suggestedMatchCoroutine = StartCoroutine(HighlightRandomMatchAsync());
                }
            }

            if (CheckCollectables())
            {
                ApplyGravity();
            }
            gameScene.CheckEndGame();
        }

        /// <summary>
        /// Checks the current level for collectables that have been collected by the player.
        /// </summary>
        /// <returns>True if there were collected collectables; false otherwise.</returns>
        private bool CheckCollectables()
        {
            var collectablesToDestroy = new List<Tile>();
            for (var i = 0; i < level.width; i++)
            {
                Tile bottom = null;
                var tileIndex = 0;
                for (var j = level.height - 1; j >= 0; j--)
                {
                    tileIndex = i + (j * level.width);
                    if (tiles[tileIndex] == null)
                    {
                        continue;
                    }

                    var tile = tiles[tileIndex].GetComponent<Tile>();
                    if (tile != null)
                    {
                        if (tile.GetComponent<Unbreakable>() != null)
                        {
                            continue;
                        }

                        bottom = tile;
                    }

                    break;
                }

                if (bottom != null && bottom.GetComponent<Collectable>() != null)
                {
                    collectablesToDestroy.Add(bottom);
                    tiles[tileIndex] = null;
                }
            }

            if (collectablesToDestroy.Count > 0)
            {
                foreach (var tile in collectablesToDestroy)
                {
                    gameState.AddCollectable(tile.GetComponent<Collectable>().type);
                    UpdateScore(gameConfig.GetTileScore(tile.GetComponent<Tile>()));

                    var fx = fxPool.collectableExplosion.GetObject();
                    fx.transform.position = tile.transform.position;

                    SoundManager.instance.PlaySound("Collectable");

                    tile.Explode();
                    tile.GetComponent<PooledObject>().pool.ReturnObject(tile.gameObject);
                }

                gameUi.UpdateGoals(gameState);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Internal method that actually applies the gravity to the current level.
        /// </summary>
        private void ApplyGravityInternal()
        {
            var fallingSoundPlayed = false;
            for (var i = 0; i < level.width; i++)
            {
                for (var j = level.height - 1; j >= 0; j--)
                {
                    var tileIndex = i + (j * level.width);
                    if (GetTile(i, j) == null ||
                        GetTile(i, j).GetComponent<SpecialBlock>() != null)
                    {
                        continue;
                    }

                    // Find bottom.
                    var bottom = -1;
                    for (var k = j; k < level.height; k++)
                    {
                        var idx = i + (k * level.width);
                        if (tiles[idx] == null && !(level.tiles[idx] is HoleTile))
                        {
                            bottom = k;
                        }
                        else if (tiles[idx] != null && tiles[idx].GetComponent<SpecialBlock>() != null)
                        {
                            break;
                        }
                    }

                    if (bottom != -1)
                    {
                        var tile = GetTile(i, j);
                        if (tile != null)
                        {
                            var numTilesToFall = bottom - j;
                            tiles[tileIndex + (numTilesToFall * level.width)] = tiles[tileIndex];
                            var tween = LeanTween.move(tile,
                                tilePositions[tileIndex + level.width * numTilesToFall],
                                0.5f);
                            tween.setEase(LeanTweenType.easeInQuad);
                            tween.setOnComplete(() =>
                            {
                                if (tile.GetComponent<Tile>() != null)
                                {
                                    tile.GetComponent<Tile>().y += numTilesToFall;
                                    if (tile.activeSelf && tile.GetComponent<Animator>() != null)
                                    {
                                        tile.GetComponent<Animator>().SetTrigger("Falling");
                                        if (!fallingSoundPlayed)
                                        {
                                            fallingSoundPlayed = true;
                                            SoundManager.instance.PlaySound("CandyFalling");
                                        }
                                    }
                                }
                            });
                            tiles[tileIndex] = null;
                        }
                    }
                }
            }

            for (var i = 0; i < level.width; i++)
            {
                var numEmpties = 0;
                for (var j = 0; j < level.height; j++)
                {
                    var idx = i + (j * level.width);
                    if (tiles[idx] == null && !(level.tiles[idx] is HoleTile))
                    {
                        numEmpties += 1;
                    }
                    else if (tiles[idx] != null && tiles[idx].GetComponent<SpecialBlock>() != null)
                    {
                        break;
                    }
                }

                if (numEmpties > 0)
                {
                    for (var j = 0; j < level.height; j++)
                    {
                        var tileIndex = i + (j * level.width);
                        var isHole = level.tiles[tileIndex] is HoleTile;
                        var isBiscuit = tiles[tileIndex] != null &&
                                        tiles[tileIndex].GetComponent<SpecialBlock>() != null;
                        if (isBiscuit)
                        {
                            break;
                        }

                        if (tiles[tileIndex] == null && !isHole)
                        {
                            var tile = CreateTile(i, j, true);
                            var sourcePos = tilePositions[i];
                            var targetPos = tilePositions[tileIndex];
                            var pos = sourcePos;
                            pos.y = tilePositions[i].y + (numEmpties * (tileH));
                            --numEmpties;
                            tile.transform.position = pos;
                            var tween = LeanTween.move(tile,
                                targetPos,
                                0.5f);
                            tween.setEase(LeanTweenType.easeInQuad);
                            tween.setOnComplete(() =>
                            {
                                if (tile.activeSelf && tile.GetComponent<Animator>() != null)
                                {
                                    tile.GetComponent<Animator>().SetTrigger("Falling");
                                }
                            });
                            tiles[tileIndex] = tile;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Expands the chocolate in the current level.
        /// </summary>
        private void ExpandChocolate()
        {
            if (explodedChocolate)
            {
                return;
            }

            var chocolates = tiles.FindAll(x => x != null && x.GetComponent<Chocolate>() != null);
            if (chocolates.Count > 0)
            {
                chocolates.Shuffle();

                var foundSpot = false;
                foreach (var chocolate in chocolates)
                {
                    var x = chocolate.GetComponent<Tile>().x;
                    var y = chocolate.GetComponent<Tile>().y;
                    var leftTile = GetTile(x - 1, y);
                    var rightTile = GetTile(x + 1, y);
                    var topTile = GetTile(x, y + 1);
                    var bottomTile = GetTile(x, y - 1);
                    var neighbourTiles = new List<GameObject> {leftTile, rightTile, topTile, bottomTile};
                    foreach (var neighbour in neighbourTiles)
                    {
                        if (neighbour != null &&
                            neighbour.GetComponent<SpecialBlock>() == null)
                        {
                            CreateChocolate(neighbour.GetComponent<Tile>().x, neighbour.GetComponent<Tile>().y);
                            neighbour.GetComponent<PooledObject>().pool.ReturnObject(neighbour);
                            foundSpot = true;

                            SoundManager.instance.PlaySound("ChocolateExpand");

                            break;
                        }
                    }

                    if (foundSpot)
                    {
                        possibleSwaps = DetectPossibleSwaps();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Highlights a random match as a suggestion to the player when he is idle for some time.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator HighlightRandomMatchAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.TimeBetweenRandomMatchSuggestions);
            HighlightRandomMatch();
        }

        /// <summary>
        /// Highlights a random match as a suggestion to the player when he is idle for some time.
        /// </summary>
        public void HighlightRandomMatch()
        {
            if (currentlyAwarding)
            {
                return;
            }

            ClearSuggestedMatch();

            var swapsCopy = new List<Swap>();
            swapsCopy.AddRange(possibleSwaps);
            swapsCopy.RemoveAll(x =>
            {
                var x1 = x.tileA.GetComponent<Tile>().x;
                var y1 = x.tileA.GetComponent<Tile>().y;
                var x2 = x.tileB.GetComponent<Tile>().x;
                var y2 = x.tileB.GetComponent<Tile>().y;
                var idx1 = x1 + (y1 * level.width);
                var idx2 = x2 + (y2 * level.width);
                return (level.tiles[idx1] != null && level.tiles[idx1].elementType == ElementType.Ice) ||
                       (level.tiles[idx2] != null && level.tiles[idx2].elementType == ElementType.Ice) ||
                       x.tileA.GetComponent<SpecialBlock>() != null ||
                       x.tileB.GetComponent<SpecialBlock>() != null;
            });
            if (swapsCopy.Count > 0)
            {
                var idx = UnityEngine.Random.Range(0, swapsCopy.Count);
                var swap = swapsCopy[idx];
                if (!swap.tileA || !swap.tileB)
                {
                    return;
                }

                var x1 = swap.tileA.GetComponent<Tile>().x;
                var y1 = swap.tileA.GetComponent<Tile>().y;
                var x2 = swap.tileB.GetComponent<Tile>().x;
                var y2 = swap.tileB.GetComponent<Tile>().y;
                SetTile(swap.tileA, x2, y2);
                SetTile(swap.tileB, x1, y1);

                if (HasMatch(x2, y2))
                {
                    suggestedMatch.AddRange(GetTilesToHighlight(swap.tileA, x2, y2));
                }
                else if (HasMatch(x1, y1))
                {
                    suggestedMatch.AddRange(GetTilesToHighlight(swap.tileB, x1, y1));
                }

                foreach (var tile in suggestedMatch)
                {
                    if (tile.gameObject.activeSelf && tile.GetComponent<Animator>() != null)
                    {
                        tile.GetComponent<Animator>().SetTrigger("SuggestedMatch");
                    }
                }

                SetTile(swap.tileA, x1, y1);
                SetTile(swap.tileB, x2, y2);
            }
            else
            {
                var hasPlayableColorBomb = false;
                GameObject playableColorBomb = null;
                GameObject playableNeighbour = null;
                for (var i = 0; i < level.width; i++)
                {
                    for (var j = 0; j < level.height; j++)
                    {
                        var idx = i + (j * level.width);
                        var tile = tiles[idx];
                        if (tile != null && tile.GetComponent<ColorBomb>() != null)
                        {
                            playableColorBomb = tile;
                            var left = GetTile(i - 1, j);
                            var right = GetTile(i + 1, j);
                            var top = GetTile(i, j - 1);
                            var bottom = GetTile(i, j + 1);
                            if (left != null && left.GetComponent<Candy>() != null)
                            {
                                hasPlayableColorBomb = true;
                                playableNeighbour = left;
                                break;
                            }
                            if (right != null && right.GetComponent<Candy>() != null)
                            {
                                hasPlayableColorBomb = true;
                                playableNeighbour = right;
                                break;
                            }
                            if (top != null && top.GetComponent<Candy>() != null)
                            {
                                hasPlayableColorBomb = true;
                                playableNeighbour = top;
                                break;
                            }
                            if (bottom != null && bottom.GetComponent<Candy>() != null)
                            {
                                hasPlayableColorBomb = true;
                                playableNeighbour = bottom;
                                break;
                            }
                        }
                    }

                    if (hasPlayableColorBomb)
                    {
                        break;
                    }
                }

                if (hasPlayableColorBomb)
                {
                    suggestedMatch.Add(playableColorBomb);
                    suggestedMatch.Add(playableNeighbour);
                    foreach (var tile in suggestedMatch)
                    {
                        if (tile.gameObject.activeSelf && tile.GetComponent<Animator>() != null)
                        {
                            tile.GetComponent<Animator>().SetTrigger("SuggestedMatch");
                        }
                    }
                }
                else
                {
                    gameScene.OpenPopup<RegenLevelPopup>("Popups/RegenLevelPopup");
                    StartCoroutine(RegenerateLevel());
                }
            }
        }

        /// <summary>
        /// Regenerates the level when no matches are possible.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator RegenerateLevel()
        {
            yield return new WaitForSeconds(2.0f);
            for (var i = 0; i < level.width; i++)
            {
                for (var j = 0; j < level.height; j++)
                {
                    var idx = i + (j * level.width);
                    var tile = tiles[idx];
                    if (tile != null && tile.GetComponent<Candy>() != null)
                    {
                        var newTile = CreateTile(i, j, false);
                        newTile.transform.position = tile.transform.position;
                        tile.GetComponent<PooledObject>().pool.ReturnObject(tile);
                        SetTile(newTile, i, j);
                    }
                }
            }

            possibleSwaps = DetectPossibleSwaps();
            suggestedMatchCoroutine = StartCoroutine(HighlightRandomMatchAsync());
        }

        /// <summary>
        /// Returns the tiles that should be highlighted for the randomly suggested match.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>A list containing all the tiles that should be highlighted for the randomly suggested match.</returns>
        private List<GameObject> GetTilesToHighlight(GameObject tile, int x, int y)
        {
            var tilesToHighlight = new List<GameObject>();

            tilesToHighlight.Add(tile);
            if (HasHorizontalMatch(x, y))
            {
                var i = x - 1;
                while (i >= 0 && GetTile(i, y) != null && GetTile(i, y).GetComponent<Candy>() != null &&
                       GetTile(i, y).GetComponent<Candy>().color == tile.GetComponent<Candy>().color)
                {
                    tilesToHighlight.Add(GetTile(i, y));
                    --i;
                }

                i = x + 1;
                while (i < level.width && GetTile(i, y) != null && GetTile(i, y).GetComponent<Candy>() != null &&
                       GetTile(i, y).GetComponent<Candy>().color == tile.GetComponent<Candy>().color)
                {
                    tilesToHighlight.Add(GetTile(i, y));
                    ++i;
                }
            }
            else if (HasVerticalMatch(x, y))
            {
                var j = y - 1;
                while (j >= 0 && GetTile(x, j) != null && GetTile(x, j).GetComponent<Candy>() != null &&
                       GetTile(x, j).GetComponent<Candy>().color == tile.GetComponent<Candy>().color)
                {
                    tilesToHighlight.Add(GetTile(x, j));
                    --j;
                }

                j = y + 1;
                while (j < level.height && GetTile(x, j) != null && GetTile(x, j).GetComponent<Candy>() != null &&
                       GetTile(x, j).GetComponent<Candy>().color == tile.GetComponent<Candy>().color)
                {
                    tilesToHighlight.Add(GetTile(x, j));
                    ++j;
                }
            }

            return tilesToHighlight;
        }

        /// <summary>
        /// Clears the randomly suggested match.
        /// </summary>
        private void ClearSuggestedMatch()
        {
            foreach (var tile in suggestedMatch)
            {
                if (tile.gameObject.activeSelf)
                {
                    tile.GetComponent<Animator>().SetTrigger("Reset");
                }
            }

            suggestedMatch.Clear();
        }

        /// <summary>
        /// Awards the special candies at the end of the level.
        /// </summary>
        public void AwardSpecialCandies()
        {
            StartCoroutine(AwardSpecialCandiesAsync());
        }

        /// <summary>
        /// Awards the special candies at the end of the level.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator AwardSpecialCandiesAsync()
        {
            currentlyAwarding = true;
            yield return new WaitForSeconds(1.0f);
            gameScene.OpenPopup<SpecialCandiesAwardPopup>("Popups/SpecialCandiesAwardPopup");
            yield return new WaitForSeconds(1.5f);
            while (currentLimit > 0)
            {
                int randomIdx;
                do
                {
                    randomIdx = UnityEngine.Random.Range(0, tiles.Count);
                } while (tiles[randomIdx] == null || (tiles[randomIdx] != null && !IsNormalCandy(tiles[randomIdx])));

                var tile = tiles[randomIdx];
                tiles[randomIdx] = null;
                if (tile != null)
                {
                    tile.GetComponent<PooledObject>().pool.ReturnObject(tile.gameObject);
                }

                if (level.awardedSpecialCandyType == AwardedSpecialCandyType.Striped)
                {
                    if (UnityEngine.Random.Range(0, 2) % 2 == 0)
                    {
                        CreateHorizontalStripedTile(randomIdx % level.width, randomIdx / level.width,
                            GetRandomCandyColor());
                    }
                    else
                    {
                        CreateVerticalStripedTile(randomIdx % level.width, randomIdx / level.width,
                            GetRandomCandyColor());
                    }
                }
                else
                {
                    CreateWrappedTile(randomIdx % level.width, randomIdx / level.width, GetRandomCandyColor());
                }

                SoundManager.instance.PlaySound("BoosterAward");

                currentLimit -= 1;
                UpdateLimitText();
                yield return new WaitForSeconds(GameplayConstants.TimeBetweenRewardedCandiesCreation);
            }

            //foreach (var tile in tiles)
            //{
            //    if (tile != null && IsSpecialCandy(tile))
            //    {
            //        ExplodeTile(tile);
            //        ApplyGravity();
            //        yield return new WaitForSeconds(GameplayConstants.TimeBetweenRewardedCandiesExplosion);
            //    }
            //}

            //gameScene.OpenWinPopup();
            // Snapshot, so changes to 'tiles' don't break the enumeration.
            var specialTiles = tiles.FindAll(t => t != null && IsSpecialCandy(t));

            foreach (var tile in specialTiles)
            {
                // Skip candies already destroyed by a previous explosion or returned to the pool.
                if (tile == null || !tile.activeSelf || !tiles.Contains(tile))
                {
                    continue;
                }

                ExplodeTile(tile);
                ApplyGravity();
                yield return new WaitForSeconds(GameplayConstants.TimeBetweenRewardedCandiesExplosion);
            }

            gameScene.OpenWinPopup();
        }

        /// <summary>
        /// Checks if the specified tile is a regular candy.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <returns>True if the specified tile is a regular candy; false otherwise.</returns>
        private bool IsNormalCandy(GameObject tile)
        {
            return tile.GetComponent<Candy>() != null &&
                   tile.GetComponent<StripedCandy>() == null &&
                   tile.GetComponent<WrappedCandy>() == null;
        }

        /// <summary>
        /// Checks if the specified tile is a special candy.
        /// </summary>
        /// <param name="tile">The tile.</param>
        /// <returns>True if the specified tile is a special candy; false otherwise.</returns>
        private bool IsSpecialCandy(GameObject tile)
        {
            return tile.GetComponent<StripedCandy>() != null ||
                   tile.GetComponent<WrappedCandy>() != null ||
                   tile.GetComponent<ColorBomb>() != null;
        }

        /// <summary>
        /// Returns a random color from the available colors in the current level.
        /// </summary>
        /// <returns>A random color from the available colors in the current level.</returns>
        private CandyColor GetRandomCandyColor()
        {
            var eligibleColors = new List<CandyColor>();
            eligibleColors.AddRange(level.availableColors);
            var idx = UnityEngine.Random.Range(0, eligibleColors.Count);
            return eligibleColors[idx];
        }

        /// <summary>
        /// Called when the booster mode is enabled.
        /// </summary>
        public void OnBoosterModeEnabled()
        {
            if (suggestedMatchCoroutine != null)
            {
                StopCoroutine(suggestedMatchCoroutine);
            }
            ClearSuggestedMatch();
        }

        /// <summary>
        /// Called when the booster mode is disabled.
        /// </summary>
        public void OnBoosterModeDisabled()
        {
        }

        /// <summary>
        /// Consumes the specified booster.
        /// </summary>
        /// <param name="button">The used booster button.</param>
        private void ConsumeBooster(BuyBoosterButton button)
        {
            var playerPrefsKey = string.Format("num_boosters_{0}", (int)button.boosterType);
            var numBoosters = PlayerPrefs.GetInt(playerPrefsKey);
            numBoosters -= 1;
            PlayerPrefs.SetInt(playerPrefsKey, numBoosters);
            button.UpdateAmount(numBoosters);
        }
    }
}
