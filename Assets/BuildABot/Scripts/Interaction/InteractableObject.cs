using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {

        [Tooltip("The interaction display object.")]
        [SerializeField] protected InteractionMessage interactionDisplay;
        
        [Tooltip("The sound played when speaking to this character.")]
        [SerializeField] protected AudioClip sound;

        [Tooltip("The event fired when the interaction is finished..")]
        [SerializeField] protected UnityEvent onFinishInteraction;

        /** The name of this character. */
        public string Name;
        
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
            Debug.Log("HERE");
            interactionDisplay.DisplayMessage(instigator.Player, "{INPUT:Player:Interact} Enter the " + Name);
        }

        protected virtual void OnInteract(InteractionController instigator)
        {
            CanInteract = false;
            onFinishInteraction.Invoke();
        }

        public bool CanInteract { get; set; } = true;
        
        public event UnityAction OnFinishInteraction
        {
            add => onFinishInteraction.AddListener(value);
            remove => onFinishInteraction.RemoveListener(value);
        }
    }
}