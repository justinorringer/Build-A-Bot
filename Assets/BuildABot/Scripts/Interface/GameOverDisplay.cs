using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class GameOverDisplay : MonoBehaviour
    {
        [Tooltip("The player that was killed.")]
        [SerializeField] private Player player;

        [Tooltip("the text component drawn to the screen.")]
        [SerializeField] private TMP_Text textDisplay;

        private string _message = "Game Over";

        /** The message shown by this display. */
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                textDisplay.text = _message ?? "";
            }
        }
        
        public bool IsFinished { get; private set; } = false;

        /**
         * Handles finishing the animation for the game over screen.
         */
        public void OnFinishAnimation()
        {
            IsFinished = true;
        }
    }
}