// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;
using GameVanilla.Game.Popups;
using GameVanilla.Game.UI;

namespace GameVanilla.Game.Scenes
{
    /// <summary>
    /// This class contains the logic associated to the game scene.
    /// </summary>
	public class GameScene : BaseScene
	{
		public GameBoard gameBoard;

		public GameUi gameUi;

		public Level level;

		public FxPool fxPool;

		[SerializeField]
		private Image ingameBoosterPanel;

		[SerializeField]
		private Text ingameBoosterText;

		private bool gameStarted;
		private bool gameFinished;

		private bool boosterMode;
		private BuyBoosterButton currentBoosterButton;
		private int ingameBoosterBgTweenId;

	    /// <summary>
	    /// Unity's Awake method.
	    /// </summary>
		private void Awake()
		{
			Assert.IsNotNull(gameBoard);
			Assert.IsNotNull(gameUi);
			Assert.IsNotNull(fxPool);
			Assert.IsNotNull(ingameBoosterPanel);
			Assert.IsNotNull(ingameBoosterText);
		}

	    /// <summary>
	    /// Unity's Start method.
	    /// </summary>
		private void Start()
		{
			gameBoard.LoadLevel();

			level = gameBoard.level;
            OpenPopup<LevelGoalsPopup>("Popups/LevelGoalsPopup", popup => popup.SetGoals(level.goals));
		}

	    /// <summary>
	    /// Unity's Update method.
	    /// </summary>
		private void Update()
		{
			if (!gameStarted || gameFinished)
			{
				return;
			}

            if (currentPopups.Count > 0)
            {
                return;
            }

			if (boosterMode)
			{
				if (currentBoosterButton.boosterType == BoosterType.Switch)
				{
					gameBoard.HandleSwitchBoosterInput(currentBoosterButton);
				}
				else
				{
					gameBoard.HandleBoosterInput(currentBoosterButton);
				}
			}
			else
			{
				gameBoard.HandleInput();
			}
		}

	    /// <summary>
	    /// Starts the game.
	    /// </summary>
		public void StartGame()
		{
			gameStarted = true;
		    gameBoard.StartGame();
		}

	    /// <summary>
	    /// Ends the game.
	    /// </summary>
		public void EndGame()
		{
			gameFinished = true;
		    gameBoard.EndGame();
		}

	    /// <summary>
	    /// Restarts the game.
	    /// </summary>
		public void RestartGame()
		{
		    gameStarted = false;
		    gameFinished = false;
		    gameBoard.ResetLevelData();
			level = gameBoard.level;
            OpenPopup<LevelGoalsPopup>("Popups/LevelGoalsPopup", popup => popup.SetGoals(level.goals));
		}

        /// <summary>
        /// Continues the current game with additional moves/time.
        /// </summary>
        public void Continue()
        {
            gameFinished = false;
            gameBoard.Continue();
        }

        /// <summary>
        /// Checks if the game has finished.
        /// </summary>
        public void CheckEndGame()
        {
            if (gameFinished)
            {
                return;
            }

            var goalsComplete = true;
            foreach (var goal in level.goals)
            {
                if (!goal.IsComplete(gameBoard.gameState))
                {
                    goalsComplete = false;
                    break;
                }
            }

            if (gameBoard.currentLimit == 0)
            {
                EndGame();
            }

            if (goalsComplete)
            {
                EndGame();

                var nextLevel = PlayerPrefs.GetInt("next_level");
                if (nextLevel == 0)
                {
                    nextLevel = 1;
                }
                if (level.id == nextLevel)
                {
                    PlayerPrefs.SetInt("next_level", level.id + 1);
                    PuzzleMatchManager.instance.unlockedNextLevel = true;
                }
                else
                {
                    PuzzleMatchManager.instance.unlockedNextLevel = false;
                }

                if (level.limitType == LimitType.Moves && level.awardSpecialCandies && gameBoard.currentLimit > 0)
                {
                    gameBoard.AwardSpecialCandies();
                }
                else
                {
                    StartCoroutine(OpenWinPopupAsync());
                }
            }
            else
            {
                if (gameFinished)
                {
                    StartCoroutine(OpenNoMovesOrTimePopupAsync());
                }
            }
        }

        /// <summary>
        /// Opens the win popup.
        /// </summary>
        public void OpenWinPopup()
        {
            OpenPopup<WinPopup>("Popups/WinPopup", popup =>
            {
                var levelStars = PlayerPrefs.GetInt("level_stars_" + level.id);
                var gameState = gameBoard.gameState;
                if (gameState.score >= level.score3)
                {
                    popup.SetStars(3);
                    PlayerPrefs.SetInt("level_stars_" + level.id, 3);
                }
                else if (gameState.score >= level.score2)
                {
                    popup.SetStars(2);
                    if (levelStars < 3)
                    {
                        PlayerPrefs.SetInt("level_stars_" + level.id, 2);
                    }
                }
                else if (gameState.score >= level.score1)
                {
                    popup.SetStars(1);
                    if (levelStars < 2)
                    {
                        PlayerPrefs.SetInt("level_stars_" + level.id, 1);
                    }
                }
                else
                {
                    popup.SetStars(0);
                }

                popup.SetLevel(level.id);

                var levelScore = PlayerPrefs.GetInt("level_score_" + level.id);
                if (levelScore < gameState.score)
                {
                    PlayerPrefs.SetInt("level_score_" + level.id, gameState.score);
                }

                popup.SetScore(gameState.score);
                popup.SetGoals(gameUi.goalGroup);
            });
        }

        /// <summary>
        /// Opens the lose popup.
        /// </summary>
        public void OpenLosePopup()
        {
            PuzzleMatchManager.instance.livesSystem.RemoveLife();
            OpenPopup<LosePopup>("Popups/LosePopup", popup =>
            {
                popup.SetLevel(level.id);
                popup.SetScore(gameBoard.gameState.score);
                popup.SetGoals(gameUi.goalGroup);
            });
        }

        /// <summary>
        /// Opens the popup for buying additional moves or time.
        /// </summary>
        private void OpenNoMovesOrTimePopup()
        {
            OpenPopup<NoMovesOrTimePopup>("Popups/NoMovesOrTimePopup",
                popup => { popup.SetGameScene(this); });
        }

        /// <summary>
        /// Called when the pause button is pressed.
        /// </summary>
        public void OnPauseButtonPressed()
        {
            if (currentPopups.Count == 0)
            {
                OpenPopup<InGameSettingsPopup>("Popups/InGameSettingsPopup");
            }
            else
            {
                CloseCurrentPopup();
            }
        }

        /// <summary>
        /// Opens the win popup.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator OpenWinPopupAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.EndGamePopupDelay);
            OpenWinPopup();
        }

        /// <summary>
        /// Opens the popup for buying additional moves or time.
        /// </summary>
        /// <returns>The coroutine.</returns>
        private IEnumerator OpenNoMovesOrTimePopupAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.EndGamePopupDelay);
            OpenNoMovesOrTimePopup();
        }

        /// <summary>
        /// Shows the compliment text.
        /// </summary>
		/// <param name="type">The compliment type.</param>
        public void ShowComplimentText(ComplimentType type)
        {
	        if (gameFinished)
	        {
		        return;
	        }

	        var text = fxPool.complimentTextPool.GetObject();
	        text.transform.SetParent(canvas.transform, false);
	        text.GetComponent<ComplimentText>().SetComplimentType(type);
        }

		/// <summary>
		/// Enables the booster mode in the game.
		/// </summary>
		/// <param name="button">The used booster button.</param>
		public void EnableBoosterMode(BuyBoosterButton button)
		{
			boosterMode = true;
			currentBoosterButton = button;
			FadeInInGameBoosterOverlay();
			gameBoard.OnBoosterModeEnabled();

			switch (button.boosterType)
			{
				case BoosterType.Lollipop:
					ingameBoosterText.text = "Select a tile for the lollipop:";
					break;

				case BoosterType.Bomb:
					ingameBoosterText.text = "Select a tile for the bomb:";
					break;

				case BoosterType.Switch:
					ingameBoosterText.text = "Swap two tiles:";
					break;

				case BoosterType.ColorBomb:
					ingameBoosterText.text = "Select a tile for the color bomb:";
					break;
			}
		}

		/// <summary>
		/// Disables the booster mode in the game.
		/// </summary>
		public void DisableBoosterMode()
		{
			boosterMode = false;
			FadeOutInGameBoosterOverlay();
			gameBoard.OnBoosterModeDisabled();
		}

        /// <summary>
        /// Fades in the in-game booster overlay.
        /// </summary>
        private void FadeInInGameBoosterOverlay()
        {
            var tween = LeanTween.value(ingameBoosterPanel.gameObject, 0.0f, 1.0f, 0.4f).setOnUpdate(value =>
            {
                ingameBoosterPanel.GetComponent<CanvasGroup>().alpha = value;
                ingameBoosterText.GetComponent<CanvasGroup>().alpha = value;

            });
            tween.setOnComplete(() => ingameBoosterPanel.GetComponent<CanvasGroup>().blocksRaycasts = true);
            ingameBoosterBgTweenId = tween.id;
        }

        /// <summary>
        /// Fades out the in-game booster overlay.
        /// </summary>
        private void FadeOutInGameBoosterOverlay()
        {
            LeanTween.cancel(ingameBoosterBgTweenId, false);
            var tween = LeanTween.value(ingameBoosterPanel.gameObject, 1.0f, 0.0f, 0.2f).setOnUpdate(value =>
            {
                ingameBoosterPanel.GetComponent<CanvasGroup>().alpha = value;
                ingameBoosterText.GetComponent<CanvasGroup>().alpha = value;

            });
            tween.setOnComplete(() => ingameBoosterPanel.GetComponent<CanvasGroup>().blocksRaycasts = false);
        }
	}
}
