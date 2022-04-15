using UnityEngine;

namespace BuildABot
{
    public class HUD : MonoBehaviour
    {
        [Tooltip("The player instance using this HUD.")]
        [SerializeField] private Player player;

        [Tooltip("The dialogue display object.")]
        [SerializeField] private DialogueDisplay dialogueDisplay;

        [Tooltip("The interaction display object.")]
        [SerializeField] private InteractionMessage interactionMessage;

        /** The player instance using this HUD. */
        public Player Player => player;

        /** The dialogue display used by this HUD. */
        public DialogueDisplay DialogueDisplay => dialogueDisplay;

        /** The interaction display object used by this HUD. */
        public InteractionMessage InteractionMessage => interactionMessage;
    }
}
