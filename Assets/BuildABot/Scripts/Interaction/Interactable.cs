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
         * Gets the message tip to display for this object.
         * <returns>The interaction message for this object.</returns>
         */
        public string GetMessage();

        /** Can this object be interacted with? */
        public bool CanInteract { get; set; }

        /** An event called when this interaction is finished. */
        public event UnityAction OnFinishInteraction;

    }
}