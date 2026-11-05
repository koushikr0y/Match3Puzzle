// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class manages a single goal element within the in-game user interface for goals.
    /// </summary>
    public class GoalUiElement : MonoBehaviour
    {
        public Image image;
        public Text amountText;
        public Image tickImage;
        public Image crossImage;

        public bool isCompleted { get; private set; }

        private Goal currentGoal;
        private int targetAmount;
        private int currentAmount;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            tickImage.gameObject.SetActive(false);
            crossImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fills this element with the information of the specified goal.
        /// </summary>
        /// <param name="goal">The associated goal.</param>
        public virtual void Fill(Goal goal)
        {
            currentGoal = goal;
            if (goal is CollectCandyGoal)
            {
                var candyGoal = (CollectCandyGoal)goal;
                var texture = Resources.Load("Game/" + candyGoal.candyType + "Candy") as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                targetAmount = candyGoal.amount;
                amountText.text = targetAmount.ToString();
            }
            else if (goal is CollectElementGoal)
            {
                var elementGoal = (CollectElementGoal)goal;
                var texture = Resources.Load("Game/" + elementGoal.elementType) as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                targetAmount = elementGoal.amount;
                amountText.text = targetAmount.ToString();
            }
            else if (goal is CollectSpecialBlockGoal)
            {
                var specialGoal = (CollectSpecialBlockGoal)goal;
                var texture = Resources.Load("Game/" + specialGoal.specialBlockType) as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                targetAmount = specialGoal.amount;
                amountText.text = targetAmount.ToString();
            }
            else if (goal is CollectCollectableGoal)
            {
                var collectableGoal = (CollectCollectableGoal)goal;
                var texture = Resources.Load("Game/" + collectableGoal.collectableType) as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                targetAmount = collectableGoal.amount;
                amountText.text = targetAmount.ToString();
            }
            else if (goal is DestroyAllChocolateGoal)
            {
                var texture = Resources.Load("Game/Chocolate") as Texture2D;
                if (texture != null)
                {
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100);
                }
                amountText.text = "All";
            }
        }

        /// <summary>
        /// Updates this element based on the current state of the game.
        /// </summary>
        /// <param name="state">The current game state.</param>
        public virtual void UpdateGoal(GameState state)
        {
            var chocolateGoal = currentGoal as DestroyAllChocolateGoal;
            if (chocolateGoal != null)
            {
                if (state.destroyedAllChocolates && !chocolateGoal.completed)
                {
                    chocolateGoal.completed = true;
                    SetCompletedTick(true);
                    SoundManager.instance.PlaySound("ReachedGoal");
                    return;
                }
            }

            if (currentAmount == targetAmount)
            {
                return;
            }

            var newAmount = 0;
            var candyGoal = currentGoal as CollectCandyGoal;
            if (candyGoal != null)
            {
                newAmount = state.collectedCandies[candyGoal.candyType];
            }
            else
            {
                var elementGoal = currentGoal as CollectElementGoal;
                if (elementGoal != null)
                {
                    newAmount = state.collectedElements[elementGoal.elementType];
                }
                else
                {
                    var specialGoal = currentGoal as CollectSpecialBlockGoal;
                    if (specialGoal != null)
                    {
                        newAmount = state.collectedSpecialBlocks[specialGoal.specialBlockType];
                    }
                    else
                    {
                        var collectableGoal = currentGoal as CollectCollectableGoal;
                        if (collectableGoal != null)
                        {
                            newAmount = state.collectedCollectables[collectableGoal.collectableType];
                        }
                    }
                }
            }

            if (newAmount == currentAmount)
            {
                return;
            }

            currentAmount = newAmount;
            if (currentAmount >= targetAmount)
            {
                currentAmount = targetAmount;
                SetCompletedTick(true);
                SoundManager.instance.PlaySound("ReachedGoal");
            }
            amountText.text = (targetAmount - currentAmount).ToString();
            if (amountText.gameObject.activeSelf)
            {
                amountText.GetComponent<Animator>().SetTrigger("GoalAchieved");
            }
        }

        /// <summary>
        /// Sets the goal tick as completed/not completed.
        /// </summary>
        /// <param name="completed">True if the completion tick should be shown; false otherwise.</param>
        public void SetCompletedTick(bool completed)
        {
            isCompleted = completed;
            amountText.gameObject.SetActive(false);
            if (completed)
            {
                tickImage.gameObject.SetActive(true);
                image.GetComponent<Animator>().SetTrigger("GoalAchieved");
                tickImage.GetComponent<Animator>().SetTrigger("GoalAchieved");
            }
            else
            {
                crossImage.gameObject.SetActive(true);
            }
        }
    }
}
