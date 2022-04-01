using UnityEngine;

namespace BuildABot
{
    public interface IInteractable
    {
        
        public void Interact(InteractionController instigator);

        public string GetMessage();

        public bool CanInteract { get; set; }

    }
}