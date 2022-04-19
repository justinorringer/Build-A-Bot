using System;
using UnityEngine;

namespace BuildABot
{
    public class GameTipTrigger : MonoBehaviour
    {
        [Tooltip("The title of the tip to display when the player enters this volume.")]
        [SerializeField] private string title = HelpWidget.DefaultTitle;

        [Tooltip("The message to display when the player enters this volume.")]
        [TextArea]
        [SerializeField] private string message;

        [Tooltip("The acknowledgement message to use to confirm the tool tip.")]
        [SerializeField] private string acknowledgeMessage = HelpWidget.DefaultAcknowledgeMessage;
        
        [Tooltip("Should this message only be triggered once?")]
        [SerializeField] private bool playOnce;

        /** Has this tip been triggered? */
        private bool _hasTriggered;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_hasTriggered || !playOnce)
            {
                Player player = other.transform.GetComponent<Player>();
                if (player != null)
                {
                    player.ShowHelpMenu(message, title, acknowledgeMessage);
                    _hasTriggered = true;
                }
            }
        }
    }
}