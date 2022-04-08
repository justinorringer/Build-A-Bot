using UnityEngine;

namespace BuildABot
{
    public class InteractableCharacter : MonoBehaviour, IInteractable
    {

        [SerializeField] protected Dialogue dialogue;
        [SerializeField] protected DialogueSpeaker speakerProfile;

        /** The name of this character. */
        public string Name => speakerProfile.CharacterName;
        
        public void Interact(InteractionController instigator)
        {
            if (!CanInteract) return;
            OnInteract(instigator);
        }

        public string GetMessage()
        {
            return "Talk to " + Name;
        }

        protected virtual void OnInteract(InteractionController instigator)
        {
            if (instigator.Player.HUD.DialogueDisplay.TryStartDialogue(dialogue, speakerProfile))
            {
                instigator.Player.HUD.DialogueDisplay.OnEndDialogue += OnFinishDialogue;
            }
        }

        protected virtual void OnFinishDialogue(Dialogue finished, DialogueSpeaker speaker)
        {
            
        }

        public bool CanInteract { get; set; } = true;
    }
}