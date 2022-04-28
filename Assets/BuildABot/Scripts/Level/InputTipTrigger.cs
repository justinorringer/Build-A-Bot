using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildABot
{
    public class InputTipTrigger : MonoBehaviour
    {
        [Tooltip("The message to display when the player enters this volume.")]
        [TextArea]
        [SerializeField] private string message;

        [Tooltip("The input action expected to be performed to clear the summoned display. If not specified, a manual dismissal is required.")]
        [SerializeField] private InputActionReference action;
        
        [Tooltip("Should this message only be triggered once?")]
        [SerializeField] private bool playOnce;

        /** Has this tip been triggered? */
        private bool _hasTriggered;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_hasTriggered || !playOnce)
            {
                Player player = other.transform.GetComponent<Player>();
                if (player != null)
                {
                    string inputPath = null;
                    if (action != null)
                    {
                        InputAction inputAction = action.ToInputAction();
                        inputPath = $"{{INPUT:{inputAction.actionMap.name}:{inputAction.name}}}";
                    }
                    player.ShowInputHelp(message, inputPath);
                    _hasTriggered = true;
                }
            }
        }
    }
}