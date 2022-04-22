using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public class InteractableCharacter : MonoBehaviour, IInteractable
    {

        [Tooltip("The interaction display object.")]
        [SerializeField] protected InteractionMessage interactionDisplay;
        
        [Tooltip("The dialogue played when speaking to this character.")]
        [SerializeField] protected Dialogue dialogue;
        [Tooltip("The speaker profile used for this character.")]
        [SerializeField] protected DialogueSpeaker speakerProfile;
        
        [Tooltip("The event fired when the interaction is finished..")]
        [SerializeField] protected UnityEvent onFinishInteraction;

        /** The name of this character. */
        public string Name => speakerProfile.CharacterName;
        
        public void Interact(InteractionController instigator)
        {
            if (!CanInteract) return;
            OnInteract(instigator);
        }

        public void SuppressMessage()
        {
            interactionDisplay.Suppress();
        }
        
        public void DisplayMessage(InteractionController instigator)
        {
            interactionDisplay.DisplayMessage(instigator.Player, "{INPUT:Player:Interact} Talk to " + Name);
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