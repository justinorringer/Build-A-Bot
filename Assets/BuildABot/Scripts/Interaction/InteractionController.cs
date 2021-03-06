using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildABot
{
    public class InteractionController : MonoBehaviour
    {

        [SerializeField] private Player player;

        [SerializeField] private LayerMask targetLayer;

        [SerializeField] private bool allowInteraction = true;

        private IInteractable _target;

        private IEnumerator _checkTask;

        public Player Player => player;

        public bool AllowInteraction
        {
            get => allowInteraction;
            set
            {
                allowInteraction = value;
                if (allowInteraction)
                {
                    if (_checkTask != null) StopCoroutine(_checkTask);
                    _checkTask = Utility.RepeatFunction(this, CheckForTargets, 0.25f);
                }
                else
                {
                    if (_checkTask != null) StopCoroutine(_checkTask);
                    _checkTask = null;
                }
            }
        }
        
        protected void Start()
        {
            // Repeat check until disabled
            AllowInteraction = allowInteraction;
        }

        protected void OnEnable()
        {
            player.PlayerController.InputActions.Player.Interact.performed += Input_OnInteract;
        }

        protected void OnDisable()
        {
            player.PlayerController.InputActions.Player.Interact.performed -= Input_OnInteract;
        }

        private void Input_OnInteract(InputAction.CallbackContext context)
        {
            if (_target != null)
            {
                _target.SuppressMessage();
                _target.Interact(this); // TODO: Subscribe to OnInteractionFinished event
                _target = null;
            }
        }

        private void CheckForTargets()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, player.CharacterMovement.Facing,
                player.Bounds.x * 2, targetLayer);
            if (hit.transform == null)
            {
                if (_target != null) _target.SuppressMessage();
                _target = null;
                return;
            }
            
            IInteractable interactable = hit.transform.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract)
            {
                _target = interactable;
                interactable.DisplayMessage(this);
            }
            else
            {
                if (_target != null) _target.SuppressMessage();
                _target = null;
            }
            
        }
        
    }
}