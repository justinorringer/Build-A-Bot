using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public class GameOverDisplay : MonoBehaviour
    {

        [Tooltip("The text component used to draw the game stats to the screen.")]
        [SerializeField] private TMP_Text statsDisplay;

        [Tooltip("An event fired when this display finishes.")]
        [SerializeField] private UnityEvent onFinish;

        [Tooltip("The animator used by this display.")]
        [SerializeField] private Animator animator;

        [Tooltip("The sound played as each stat is displayed.")]
        [SerializeField] private AudioClip statSound;
        
        public event UnityAction OnFinish
        {
            add => onFinish.AddListener(value);
            remove => onFinish.RemoveListener(value);
        }

        protected void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void ShowStats()
        {
            float speed = animator.speed;
            animator.speed = 0;
            int i = 0;
            double secondsPlayed = GameManager.GameState.StopTime - GameManager.GameState.StartTime;
            TimeSpan timePlayed = TimeSpan.FromSeconds(secondsPlayed);
            string timeString = $"{timePlayed.Hours + (24 * timePlayed.Days)}h:{timePlayed.Minutes}m:{timePlayed.Seconds}s";
            Utility.RepeatFunction(this, () =>
            {
                string[] lines = new string[4];
                lines[0] = i >= 0 ? $"Time Played:<pos=70%>{timeString}" : "";
                lines[1] = i >= 1 ? $"Levels Completed:<pos=85%>{GameManager.GameState.CompletedLevelCount}" : "";
                lines[2] = i >= 2 ? $"Total Kills:<pos=85%>{GameManager.GameState.KillCount}" : "";
                lines[3] = i >= 3 ? $"Money Earned:<pos=85%>{GameManager.GameState.TotalMoneyEarned}" : "";
                i++;
                statsDisplay.text = $"{lines[0]}\n{lines[1]}\n{lines[2]}\n{lines[3]}";
                AudioManager.PlayOneShot(statSound);
            }, 1.0f, 4, () =>
            {
                animator.speed = speed;
            }, true);
        }

        /**
         * Handles finishing the animation for the game over screen.
         */
        private void FinishAnimation()
        {
            onFinish.Invoke();
        }
    }
}