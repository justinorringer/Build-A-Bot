using UnityEngine;

namespace BuildABot
{
    public class InteractableCharacter : MonoBehaviour, IInteractable
    {

        [SerializeField] private Dialogue dialogue;
        [SerializeField] private DialogueSpeaker speakerProfile;
        
        public void Interact(InteractionController instigator)
        {
            if (!CanInteract) return;
            if (instigator.Player.HUD.DialogueDisplay.TryStartDialogue(dialogue, speakerProfile))
            {
                instigator.Player.PlayerInput.GameInputEnabled = false;
            }
        }

        public string GetMessage()
        {
            return "Talk";
        }

        public bool CanInteract { get; set; } = true;
    }
}