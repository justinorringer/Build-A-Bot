using System;
using UnityEngine;

namespace BuildABot
{
    public class BackgroundAudioOverrideVolume : MonoBehaviour
    {

        [Min(0f)]
        [SerializeField] private float fadeLength = 3f;
        [Tooltip("The dto play when this volume is entered.")]
        [SerializeField] private AudioClip track;

        private bool _active;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInChildren<AudioListener>() != null)
            {
                AudioManager.CrossFadeToNewTrack(track, fadeLength);
                _active = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_active && other.GetComponentInChildren<AudioListener>() != null)
            {
                AudioManager.CrossFadeToDefaultTrack(fadeLength);
                _active = false;
            }
        }

        private void OnDisable()
        {
            if (_active)
            {
                AudioManager.CrossFadeToDefaultTrack(fadeLength);
                _active = false;
            }
        }
    }
}