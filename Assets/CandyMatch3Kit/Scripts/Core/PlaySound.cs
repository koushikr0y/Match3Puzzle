// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace GameVanilla.Core
{
    /// <summary>
    /// Utility class to play a sound via the SoundManager.
    /// </summary>
    public class PlaySound : MonoBehaviour
    {
        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void Play(string soundName)
        {
            SoundManager.instance.PlaySound(soundName);
        }

        /// <summary>
        /// Plays the specified sound looped.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlayLooped(string soundName)
        {
            SoundManager.instance.PlaySound(soundName, true);
        }

        /// <summary>
        /// Stops the specified sound.
        /// </summary>
        /// <param name="soundName">The name of the sound to stop.</param>
        public void Stop(string soundName)
        {
            SoundManager.instance.StopSound(soundName);
        }
    }
}
