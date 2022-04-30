using UnityEngine.Events;

namespace BuildABot
{
    public interface IInteractable
    {
        /**
         * Interacts with this object.
         * <param name="instigator">The instigator of the interaction.</param>
         */
        public void Interact(InteractionController instigator);

        /**
         * Hides the message display of this object if it is active.
         */
        public void SuppressMessage();
        
        /**
         * Shows the message tip for this object.
         * <param name="instigator">The instigator of the interaction.</param>
         */
        public void DisplayMessage(InteractionController instigator);

        /** Can this object be interacted with? */
        public bool CanInteract { get; set; }

        /** An event called when this interaction is finished. */
        public event UnityAction OnFinishInteraction;

    }
}