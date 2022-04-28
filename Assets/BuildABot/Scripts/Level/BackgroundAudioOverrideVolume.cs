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
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInChildren<AudioListener>() != null)
            {
                AudioManager.CrossFadeToNewTrack(track, fadeLength);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInChildren<AudioListener>() != null)
            {
                AudioManager.CrossFadeToDefaultTrack(fadeLength);
            }
        }
    }
}