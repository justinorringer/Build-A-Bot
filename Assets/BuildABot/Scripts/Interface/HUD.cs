using UnityEngine;

namespace BuildABot
{
    public class HUD : MonoBehaviour
    {
        [Tooltip("The player instance using this HUD.")]
        [SerializeField] private Player player;

        [Tooltip("The dialogue display object.")]
        [SerializeField] private DialogueDisplay dialogueDisplay;

        [Tooltip("The input help widget used by this HUD.")]
        [SerializeField] private InputHelpWidget inputHelp;

        [Tooltip("The alert widget used to give notifications to the player.")]
        [SerializeField] private NotificationDisplay notificationDisplay;

        /** The player instance using this HUD. */
        public Player Player => player;

        /** The dialogue display used by this HUD. */
        public DialogueDisplay DialogueDisplay => dialogueDisplay;

        /** The input help widget used by this HUD. */
        public InputHelpWidget InputHelpWidget => inputHelp;

        /** The alert widget used to give notifications to the player. */
        public NotificationDisplay NotificationDisplay => notificationDisplay;
    }
}
