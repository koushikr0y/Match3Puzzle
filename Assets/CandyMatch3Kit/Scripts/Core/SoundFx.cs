// Copyright (C) 2017-2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

namespace GameVanilla.Core
{
    /// <summary>
    /// Wrapper around Unity's AudioSource that disables the game object after the sound clip
    /// has been played (allowing it to be reused in the context of a pool of sound effects; see
    /// the SoundManager class).
    /// </summary>
    public class SoundFx : MonoBehaviour
    {
        private AudioSource audioSource;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            Assert.IsTrue(audioSource != null);
        }

        /// <summary>
        /// Plays the specified audio clip.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="loop">True if the clip should be looped; false otherwise.</param>
        public void Play(AudioClip clip, bool loop = false)
        {
            if (clip == null)
            {
                return;
            }
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
            Invoke("DisableSoundFx", clip.length + 0.1f);
        }

        /// <summary>
        /// Returns the sound effect to the sound effects pool.
        /// </summary>
        private void DisableSoundFx()
        {
            GetComponent<PooledObject>().pool.ReturnObject(gameObject);
        }
    }
}
