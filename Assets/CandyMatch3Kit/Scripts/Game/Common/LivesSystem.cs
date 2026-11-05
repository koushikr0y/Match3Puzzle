// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class handles the lives system in the game. It is used to add and remove lives and other classes
    /// can subscribe to it in order to receive a notification when the number of lives changes.
    /// </summary>
    public class LivesSystem : MonoBehaviour
    {
        private DateTime oldDate;
        private TimeSpan timeSpan;
        private bool runningCountdown;
        private float accTime;

        public Action<TimeSpan, int> onCountdownUpdated;
        public Action<int> onCountdownFinished;

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            var numLives = PlayerPrefs.GetInt("num_lives");
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var timeToNextLife = PuzzleMatchManager.instance.gameConfig.timeToNextLife;

            if (numLives < maxLives && PlayerPrefs.HasKey("next_life_time"))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString("next_life_time"));
                var prevNextLifeTime = DateTime.FromBinary(temp);
                TimeSpan remainingTime;
                var now = DateTime.Now;
                if (prevNextLifeTime > now)
                {
                    remainingTime = prevNextLifeTime - now;
                    if (numLives < maxLives)
                    {
                        StartCountdown((int)remainingTime.TotalSeconds);
                    }
                }
                else
                {
                    remainingTime = now - prevNextLifeTime;
                    var livesToGive = ((int)remainingTime.TotalSeconds / timeToNextLife) + 1;
                    numLives = numLives + livesToGive;
                    if (numLives > maxLives)
                    {
                        numLives = maxLives;
                    }
                    PlayerPrefs.SetInt("num_lives", numLives);
                    if (numLives < maxLives)
                    {
                        StartCountdown(timeToNextLife - ((int)remainingTime.TotalSeconds % timeToNextLife));
                    }
                }
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        private void Update()
        {
            if (!runningCountdown)
            {
                return;
            }

            accTime += Time.deltaTime;
            if (accTime >= 1.0f)
            {
                accTime = 0.0f;
                timeSpan = timeSpan.Subtract(TimeSpan.FromSeconds(1));
                SetTimeToNextLife((int)timeSpan.TotalSeconds);
                var numLives = PlayerPrefs.GetInt("num_lives");
                if (onCountdownUpdated != null)
                {
                    onCountdownUpdated(timeSpan, numLives);
                }
                if ((int)timeSpan.TotalSeconds == 0)
                {
                    StopCountdown();
                    AddLife();
                }
            }
        }

        /// <summary>
        /// Adds a life to the system.
        /// </summary>
        public void AddLife()
        {
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var numLives = PlayerPrefs.GetInt("num_lives");
            numLives += 1;
            PlayerPrefs.SetInt("num_lives", numLives);
            if (numLives < maxLives)
            {
                if (!runningCountdown)
                {
                    var timeToNextLife = PuzzleMatchManager.instance.gameConfig.timeToNextLife;
                    StartCountdown(timeToNextLife);
                }
            }
            else
            {
                StopCountdown();
            }
        }

        /// <summary>
        /// Removes a life from the system.
        /// </summary>
        public void RemoveLife()
        {
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var numLives = PlayerPrefs.GetInt("num_lives");
            numLives -= 1;
            if (numLives < 0)
            {
                numLives = 0;
            }
            PlayerPrefs.SetInt("num_lives", numLives);
            if (numLives < maxLives && !runningCountdown)
            {
                var timeToNextLife = PuzzleMatchManager.instance.gameConfig.timeToNextLife;
                StartCountdown(timeToNextLife);
            }
        }

        /// <summary>
        /// Sets the number of lives to the maximum number allowed by the game configuration.
        /// </summary>
        public void RefillLives()
        {
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var refillCost = PuzzleMatchManager.instance.gameConfig.livesRefillCost;
            PlayerPrefs.SetInt("num_lives", maxLives);
            PuzzleMatchManager.instance.coinsSystem.SpendCoins(refillCost);
            StopCountdown();
        }

        /// <summary>
        /// Starts the countdown to give a free life to the player.
        /// </summary>
        /// <param name="timeToNextLife">The time in seconds until the next free life is given.</param>
        public void StartCountdown(int timeToNextLife)
        {
            SetTimeToNextLife(timeToNextLife);
            timeSpan = TimeSpan.FromSeconds(timeToNextLife);
            runningCountdown = true;

            if (onCountdownUpdated == null)
            {
                return;
            }
            var numLives = PlayerPrefs.GetInt("num_lives");
            onCountdownUpdated(timeSpan, numLives);
        }

        /// <summary>
        /// Stops the countdown to give a free life to the player.
        /// </summary>
        public void StopCountdown()
        {
            runningCountdown = false;
            var numLives = PlayerPrefs.GetInt("num_lives");
            if (onCountdownFinished != null)
            {
                onCountdownFinished(numLives);
            }
        }

        /// <summary>
        /// Registers the specified callbacks to be called when the amount of lives changes.
        /// </summary>
        /// <param name="updateCallback">The callback to register for when the number of lives changes.</param>
        /// <param name="finishCallback">The callback to register for when the free life is given.</param>
        public void Subscribe(Action<TimeSpan, int> updateCallback, Action<int> finishCallback)
        {
            onCountdownUpdated += updateCallback;
            onCountdownFinished += finishCallback;
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var numLives = PlayerPrefs.GetInt("num_lives");
            if (numLives < maxLives)
            {
                if (onCountdownUpdated != null)
                {
                    onCountdownUpdated(timeSpan, numLives);
                }
            }
            else
            {
                if (onCountdownFinished != null)
                {
                    onCountdownFinished(numLives);
                }
            }
        }

        /// <summary>
        /// Unregisters the specified callbacks to be called when the amount of lives changes.
        /// </summary>
        /// <param name="updateCallback">The callback to unregister for when the number of lives changes.</param>
        /// <param name="finishCallback">The callback to unregister for when the free life is given.</param>
        public void Unsubscribe(Action<TimeSpan, int> updateCallback, Action<int> finishCallback)
        {
            if (onCountdownUpdated != null)
            {
                onCountdownUpdated -= updateCallback;
            }
            if (onCountdownFinished != null)
            {
                onCountdownFinished -= finishCallback;
            }
        }

        /// <summary>
        /// Sets the time until the next free life to the specified number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds until the next free life.</param>
        private void SetTimeToNextLife(int seconds)
        {
            var nextLifeTime = DateTime.Now.Add(TimeSpan.FromSeconds(seconds));
            PlayerPrefs.SetString("next_life_time", nextLifeTime.ToBinary().ToString());
        }
    }
}
