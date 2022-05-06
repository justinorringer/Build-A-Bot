using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BuildABot
{
    public class GameOverDisplay : MonoBehaviour
    {

        [Tooltip("The text component used to draw the game stats to the screen.")]
        [SerializeField] private TMP_Text statsDisplay;

        [Tooltip("The text component used to draw the game stat labels to the screen.")]
        [SerializeField] private TMP_Text statsLabels;

        [Tooltip("The display used to prompt the player to exit.")]
        [SerializeField] private TokenReplacedText exitPrompt;

        [Tooltip("An event fired when this display finishes.")]
        [SerializeField] private UnityEvent onFinish;

        [Tooltip("The animator used by this display.")]
        [SerializeField] private Animator animator;

        [Tooltip("The sound played as each stat is displayed.")]
        [SerializeField] private AudioClip statSound;

        private static readonly int ShowButtonHash = Animator.StringToHash("ShowButton");
        private static readonly int CloseHash = Animator.StringToHash("Close");

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

        private void ReturnToStart(InputAction.CallbackContext obj)
        {
            GameManager.GetPlayer().PlayerController.InputActions.DialogueUI.Continue.performed -= ReturnToStart;
            animator.SetTrigger(CloseHash);
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
                string[] stats = new string[4];
                stats[0] = i >= 0 ? timeString : "";
                stats[1] = i >= 1 ? $"{GameManager.GameState.CompletedLevelCount}" : "";
                stats[2] = i >= 2 ? $"{GameManager.GameState.KillCount}" : "";
                stats[3] = i >= 3 ? $"{GameManager.GameState.TotalMoneyEarned}" : "";
                string[] labels = new string[4];
                labels[0] = i >= 0 ? "Time Played:" : "<color=\"black\">Time Played:</color>";
                labels[1] = i >= 1 ? "Levels Completed:" : "<color=\"black\">Levels Completed:</color>";
                labels[2] = i >= 2 ? "Bots Defeated:" : "<color=\"black\">Bots Defeated:</color>";
                labels[3] = i >= 3 ? "Money Earned:" : "<color=\"black\">Money Earned:</color>";
                i++;
                statsLabels.text = $"{labels[0]}\n{labels[1]}\n{labels[2]}\n{labels[3]}";
                statsDisplay.GetComponent<MatchFontSize>().Refresh();
                statsDisplay.text = $"{stats[0]}\n{stats[1]}\n{stats[2]}\n{stats[3]}";
                AudioManager.PlayOneShot(statSound);
            }, 1.0f, 4, () =>
            {
                animator.speed = speed;
                exitPrompt.Refresh();
                GameManager.GetPlayer().PlayerController.InputActions.DialogueUI.Continue.performed += ReturnToStart;
                animator.SetTrigger(ShowButtonHash);
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