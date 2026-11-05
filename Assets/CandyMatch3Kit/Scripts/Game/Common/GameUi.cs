// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Game.UI;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class is the entry point to the game's user interface.
    /// </summary>
    public class GameUi : MonoBehaviour
    {
        public GameObject goalGroup;

        [SerializeField] private Text limitTitleText;

        [SerializeField] private Text limitText;

        [SerializeField] private Text scoreText;

        [SerializeField] private ProgressBar progressBar;

        [SerializeField] private GameObject goalPrefab;

        [SerializeField] private GameObject scoreGoalItem;

        [SerializeField] private Text scoreGoalItemText;

        [SerializeField] private GameObject scoreGoalOnlyItem;

        [SerializeField] private Text scoreGoalOnlyItemText;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(limitTitleText);
            Assert.IsNotNull(limitText);
            Assert.IsNotNull(scoreText);
            Assert.IsNotNull(progressBar);
            Assert.IsNotNull(goalPrefab);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(scoreGoalItem);
            Assert.IsNotNull(scoreGoalItemText);
            Assert.IsNotNull(scoreGoalOnlyItem);
            Assert.IsNotNull(scoreGoalOnlyItemText);
        }

        /// <summary>
        /// Sets the specified limit type in the UI.
        /// </summary>
        /// <param name="type">The limit type.</param>
        public void SetLimitType(LimitType type)
        {
            limitTitleText.text = type == LimitType.Moves ? "Moves" : "Time";
            if (type == LimitType.Time)
            {
                limitText.fontSize = 200;
            }
        }

        /// <summary>
        /// Sets the specified limit in the UI.
        /// </summary>
        /// <param name="amount">The limit amount.</param>
        public void SetLimit(int amount)
        {
            limitText.text = amount.ToString();
        }

        /// <summary>
        /// Sets the specified limit in the UI.
        /// </summary>
        /// <param name="amount">The limit amount.</param>
        public void SetLimit(string amount)
        {
            limitText.text = amount;
        }

        /// <summary>
        /// Sets the specified score in the UI.
        /// </summary>
        /// <param name="score">The score.</param>
        public void SetScore(int score)
        {
            scoreText.text = score.ToString();
        }

        /// <summary>
        /// Sets the specified goals in the UI.
        /// </summary>
        /// <param name="goals">The goals.</param>
        /// <param name="scaleImages">True if the goal images should be scaled; false otherwise.</param>
        public void SetGoals(List<Goal> goals, bool scaleImages = false)
        {
            var childrenToRemove = goalGroup.GetComponentsInChildren<GoalUiElement>().ToList();
            foreach (var child in childrenToRemove)
            {
                Destroy(child.gameObject);
            }

            var reachScoreGoal = goals.Find(x => x is ReachScoreGoal);
            if (reachScoreGoal != null)
            {
                if (goals.Count == 1)
                {
                    scoreGoalItem.SetActive(false);
                    scoreGoalOnlyItemText.text = ((ReachScoreGoal) reachScoreGoal).score.ToString();
                }
                else
                {
                    scoreGoalOnlyItem.SetActive(false);
                    scoreGoalItemText.text = ((ReachScoreGoal) reachScoreGoal).score.ToString();
                }
            }
            else
            {
                scoreGoalItem.SetActive(false);
                scoreGoalOnlyItem.SetActive(false);
            }

            foreach (var goal in goals)
            {
                if (!(goal is ReachScoreGoal))
                {
                    var goalObject = Instantiate(goalPrefab);
                    goalObject.transform.SetParent(goalGroup.transform, false);
                    goalObject.GetComponent<GoalUiElement>().Fill(goal);
                }
            }

            if (scaleImages)
            {
                var goalsWithoutScore = goals.FindAll(x => !(x is ReachScoreGoal));
                if (goalsWithoutScore.Count >= 3)
                {
                    foreach (var child in goalGroup.GetComponentsInChildren<GoalUiElement>().ToList())
                    {
                        child.GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 1.0f);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the UI's progress bar with the specified scores.
        /// </summary>
        /// <param name="score1">The score needed to reach the first star.</param>
        /// <param name="score2">The score needed to reach the second star.</param>
        /// <param name="score3">The score needed to reach the third star.</param>
        public void InitializeProgressBar(int score1, int score2, int score3)
        {
            progressBar.Fill(score1, score2, score3);
        }

        /// <summary>
        /// Sets the score of the UI's progress bar.
        /// </summary>
        /// <param name="score">The score.</param>
        public void SetProgressBar(int score)
        {
            progressBar.UpdateProgressBar(score);
        }

        /// <summary>
        /// Updates the UI's goals with the specified game state.
        /// </summary>
        /// <param name="state">The game state.</param>
        public void UpdateGoals(GameState state)
        {
            foreach (var element in goalGroup.GetComponentsInChildren<GoalUiElement>())
            {
                element.UpdateGoal(state);
            }
        }
    }
}
