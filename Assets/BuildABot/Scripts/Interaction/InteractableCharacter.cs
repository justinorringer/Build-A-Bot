using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public class InteractableCharacter : MonoBehaviour, IInteractable
    {

        [SerializeField] protected Dialogue dialogue;
        [SerializeField] protected DialogueSpeaker speakerProfile;
        [SerializeField] protected UnityEvent onFinishInteraction;

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
                CanInteract = false;
                instigator.Player.HUD.DialogueDisplay.OnEndDialogue += OnFinishDialogue; // TODO: Unsubscribe
            }
        }

        protected virtual void OnFinishDialogue(Dialogue finished, DialogueSpeaker speaker)
        {
            onFinishInteraction.Invoke();
            CanInteract = true;
        }

        public bool CanInteract { get; set; } = true;
        
        public event UnityAction OnFinishInteraction
        {
            add => onFinishInteraction.AddListener(value);
            remove => onFinishInteraction.RemoveListener(value);
        }
    }
}