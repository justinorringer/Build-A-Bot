using System;
using UnityEngine;

namespace BuildABot
{
    public sealed class AudioManager : GameSingleton<AudioManager>
    {
        [Tooltip("The default background track to play when this manager loads.")]
        [SerializeField] private AudioClip defaultBackgroundTrack;
        [Tooltip("The default volume to play this background track at.")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultBackgroundVolume = 0.3f;
        
        [Tooltip("The primary audio source used to play background music.")]
        [SerializeField] private AudioSource primaryBackgroundPlayer;
        [Tooltip("The secondary audio source used to fade in new background tracks.")]
        [SerializeField] private AudioSource secondaryBackgroundPlayer;

        private float _currentVolume;

        public static float CurrentVolume => Initialized ? Instance._currentVolume : 0;

        public static float DefaultVolume => Initialized ? Instance.defaultBackgroundVolume : 0;
        public static AudioClip CurrentBackgroundTrack => Initialized ? Instance.primaryBackgroundPlayer.clip : null;

        public static AudioClip DefaultBackgroundTrack => Initialized ? Instance.defaultBackgroundTrack : null;

        protected override void Awake()
        {
            base.Awake();
            _currentVolume = defaultBackgroundVolume;
            secondaryBackgroundPlayer.clip = null;
            secondaryBackgroundPlayer.volume = 0;
            primaryBackgroundPlayer.clip = defaultBackgroundTrack;
            primaryBackgroundPlayer.volume = defaultBackgroundVolume;
        }

        private void Start()
        {
            primaryBackgroundPlayer.Play();
        }

        public static void SetBackgroundVolume(float volume)
        {
            void Implementation()
            {
                Instance._currentVolume = Mathf.Clamp01(volume);
                Instance.primaryBackgroundPlayer.volume = Instance._currentVolume;
            }

            void LatentImplementation()
            {
                OnInitialized -= LatentImplementation;
                Implementation();
            }

            if (Initialized) Implementation();
            else OnInitialized += LatentImplementation;
        }

        private static void FadePlayerToTarget(AudioSource player, float seconds, float targetVolume, Action onFinish = null)
        {
            const float fadeInterval = 0.01f;

            if (targetVolume < 0f || targetVolume > 1f)
            {
                Debug.LogWarningFormat("Attempting to fade to invalid volume {0}, forcing 0-1 range.", targetVolume);
                targetVolume = Mathf.Clamp01(targetVolume);
            }
            
            if (seconds <= fadeInterval)
            {
                player.volume = targetVolume;
                return;
            }
            bool fading = true;
            
            float progress = 0f;
            float progressInterval = fadeInterval / seconds;
            float startingVolume = player.volume;
            float interval = startingVolume - targetVolume;
            
            Utility.DelayedFunction(Instance, seconds, () => fading = false, true);
            
            Utility.RepeatFunctionUntil(Instance, () =>
            {
                player.volume = startingVolume - (progress * interval);
                progress += progressInterval;
            }, fadeInterval, () => fading, () =>
            {
                onFinish?.Invoke();
            }, true);
        }

        public static void EaseBackgroundTrackVolume(float seconds, float targetVolume, Action onFinish = null)
        {
            if (Initialized) FadePlayerToTarget(Instance.primaryBackgroundPlayer, seconds, targetVolume, onFinish);
        }

        public static void FadeOutBackgroundTrack(float seconds, Action onFinish = null)
        {
            if (Initialized) FadePlayerToTarget(Instance.primaryBackgroundPlayer, seconds, 0f, onFinish);
        }

        public static void PauseBackgroundTrack()
        {
            if (Initialized) Instance.primaryBackgroundPlayer.Pause();
        }

        public static void ResumeBackgroundTrack()
        {
            if (Initialized) Instance.primaryBackgroundPlayer.UnPause();
        }

        public static void RestartBackgroundTrack()
        {
            if (Initialized) Instance.primaryBackgroundPlayer.time = 0;
        }

        public static void SwapBackgroundTrack(AudioClip track)
        {
            if (Initialized)
            {
                Instance.primaryBackgroundPlayer.Stop();
                Instance.primaryBackgroundPlayer.clip = track;
                Instance.primaryBackgroundPlayer.Play();
            }
        }

        public static void CrossFadeToNewTrack(AudioClip track, float seconds, Action onFinish = null)
        {
            if (!Initialized) return;
            FadePlayerToTarget(Instance.primaryBackgroundPlayer, seconds, 0f);
            Instance.secondaryBackgroundPlayer.clip = track;
            Instance.secondaryBackgroundPlayer.volume = 0f;
            Instance.secondaryBackgroundPlayer.Play();
            FadePlayerToTarget(Instance.secondaryBackgroundPlayer, seconds, CurrentVolume, () =>
            {
                float currentTime = Instance.secondaryBackgroundPlayer.time;
                Instance.primaryBackgroundPlayer.clip = track;
                Instance.primaryBackgroundPlayer.volume = CurrentVolume;
                Instance.primaryBackgroundPlayer.Play();
                Instance.primaryBackgroundPlayer.time = currentTime;
                Instance.secondaryBackgroundPlayer.Stop();
                Instance.secondaryBackgroundPlayer.clip = null;
                Instance.secondaryBackgroundPlayer.volume = 0f;
                onFinish?.Invoke();
            });
        }

        public static void CrossFadeToDefaultTrack(float seconds, Action onFinish = null)
        {
            if (Initialized) CrossFadeToNewTrack(Instance.defaultBackgroundTrack, seconds, onFinish);
        }

        public static void PlayOneShot(AudioClip track, float volume = 1.0f)
        {
            Instance.primaryBackgroundPlayer.PlayOneShot(track, volume);
        }
        
    }
}