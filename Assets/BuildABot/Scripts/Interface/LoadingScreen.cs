using System;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class LoadingScreen : MonoBehaviour
    {

        [Tooltip("The text component drawn to the screen to show the loading text.")]
        [SerializeField] private TMP_Text loadingText;

        [Tooltip("The animator used for this loading screen.")]
        [SerializeField] private Animator animator;

        private bool _playing;
        private Func<float> _getProgress;
        private Action _onFinishOpen;
        private Action _onFinishClose;
        private static readonly int CloseHash = Animator.StringToHash("Close");

        public void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Begin(Func<float> getProgress, Action onFinish = null)
        {
            _onFinishOpen = onFinish;
            _getProgress = getProgress;
            gameObject.SetActive(true);
        }

        private void FinishOpening()
        {
            _onFinishOpen?.Invoke();
            _onFinishOpen = null;
            Play();
        }

        private void Play()
        {
            _playing = true;
            int count = 0;
            Utility.RepeatFunctionUntil(this, () =>
            {
                string dots = "";
                for (int i = 0; i < count; i++)
                {
                    dots += ".";
                }

                loadingText.text = _getProgress == null ? $"Loading{dots}" : $"Loading{dots} {(_getProgress.Invoke() * 100):00}%";
                count = (count + 1) % 4;
            }, 0.5f, () => _playing);
        }

        public void End(Action onFinish = null)
        {
            _playing = false;
            _onFinishClose = onFinish;
            animator.SetTrigger(CloseHash);
        }

        private void FinishClosing()
        {
            gameObject.SetActive(false);
            _onFinishClose?.Invoke();
            _onFinishClose = null;
            _getProgress = null;
        }
        
    }
}