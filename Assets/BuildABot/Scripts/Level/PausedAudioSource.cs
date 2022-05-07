using UnityEngine;

namespace BuildABot
{
    public class PausedAudioSource : MonoBehaviour
    {
        private AudioSource _audioSource;

        protected void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        
        protected void OnEnable()
        {
            GameManager.OnSetPaused += HandlePauseState;
        }
        
        protected void OnDisable()
        {
            GameManager.OnSetPaused -= HandlePauseState;
        }

        private void HandlePauseState(bool paused)
        {
            if (paused) _audioSource.Pause();
            else _audioSource.UnPause();
        }
    }
}