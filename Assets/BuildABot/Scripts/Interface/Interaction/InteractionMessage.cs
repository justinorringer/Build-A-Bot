using System;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class InteractionMessage : MonoBehaviour
    {
        [Tooltip("The text area used to display the message.")]
        [SerializeField] private TMP_Text textArea;

        /**
         * Displays the message on screen.
         * <param name="player">The player requesting the display.</param>
         * <param name="message">The message to display. Token replacement will be performed.</param>
         */
        public void DisplayMessage(Player player, string message)
        {
            gameObject.SetActive(true);
            textArea.text = player.PerformStandardTokenReplacement(message);
        }

        /**
         * Suppresses this display.
         */
        public void Suppress()
        {
            textArea.text = "";
            gameObject.SetActive(false);
        }
    }
}