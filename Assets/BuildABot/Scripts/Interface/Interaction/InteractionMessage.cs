using System;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class InteractionMessage : MonoBehaviour
    {
        [Tooltip("The text area used to display the message.")]
        [SerializeField] private TMP_Text textArea;
        [Tooltip("The HUD that owns this display.")]
        [SerializeField] private HUD hud;

        /**
         * Displays the message on screen.
         * <param name="message">The message to display. Token replacement will be performed.</param>
         */
        public void DisplayMessage(string message)
        {
            gameObject.SetActive(true);
            textArea.text = hud.Player.PerformStandardTokenReplacement(message);
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