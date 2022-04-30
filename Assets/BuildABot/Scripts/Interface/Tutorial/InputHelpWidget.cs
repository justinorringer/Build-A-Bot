using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class InputHelpWidget : MonoBehaviour
    {
        [SerializeField] private HUD owner;
        [SerializeField] private TMP_Text textArea;

        private Animator _anim;
        private static readonly int HideHash = Animator.StringToHash("Hide");
        
        /** A request made to this display. */
        [Serializable]
        public struct InputHelpRequest
        {
            public string message;
            public string inputPath;
        }

        /** the ongoing and all pending requests made to this display. */
        private readonly Queue<InputHelpRequest> _requests = new Queue<InputHelpRequest>();

        protected void Awake()
        {
            gameObject.SetActive(false);
            _anim = GetComponent<Animator>();
        }

        /**
         * Displays the provided message with token replacement.
         * <param name="message">The message to display.</param>
         * <param name="inputPath">The path of the input to clear this message on.</param>
         */
        public void ShowMessage(string message, string inputPath = null)
        {
            if (message == null) return; // Invalid message
            
            _requests.Enqueue(new InputHelpRequest
            {
                message = message,
                inputPath = inputPath
            });

            if (!gameObject.activeSelf)
            {
                DisplayNextRequest();
            }
            else Debug.Log("Already enabled");
        }

        /**
         * Attempts to display the next request in the queue. This will fail if a message is currently shown or there are no requests.
         */
        private bool DisplayNextRequest()
        {
            if (_requests.Count == 0 || gameObject.activeSelf) return false;
            InputHelpRequest request = _requests.Peek();
            
            gameObject.SetActive(true);
            textArea.text = owner.Player.PerformStandardTokenReplacement(request.message);

            if (request.inputPath != null && !owner.Player.PlayerController.BindInputEvent(request.inputPath, HideMessage))
            {
                _requests.Dequeue();
                return DisplayNextRequest();
            }

            return true;
        }

        /**
         * Begins the closing animation for this message.
         */
        public void HideMessage()
        {
            InputHelpRequest request = _requests.Peek();
            if (request.inputPath != null)
                owner.Player.PlayerController.UnbindInputEvent(request.inputPath, HideMessage);
            _anim.SetTrigger(HideHash);
        }

        /**
         * Clears and hides this message display. This should not be called manually, instead please rely on the trigger
         * in the close animation.
         */
        public void Clear()
        {
            _requests.Dequeue();
            gameObject.SetActive(false);
            textArea.text = "";
            DisplayNextRequest();
        }
    }
}