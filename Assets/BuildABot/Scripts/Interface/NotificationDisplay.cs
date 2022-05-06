using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class NotificationDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text sizerText;
        [SerializeField] private TMP_Text displayText;

        [SerializeField] private Animator animator;
        private static readonly int PlayHash = Animator.StringToHash("Play");
        private static readonly int ResetHash = Animator.StringToHash("Reset");

        private readonly Queue<string> _messages = new Queue<string>();

        protected void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (_messages.Count == 0)
            {
                _messages.Enqueue(message);
                sizerText.text = message;
                displayText.text = message;
                gameObject.SetActive(true);
                animator.SetTrigger(PlayHash);
            }
            else
            {
                _messages.Enqueue(message);
            }
        }

        protected void FinishDisplay()
        {
            animator.SetTrigger(ResetHash);
            _messages.Dequeue();
            if (_messages.Count > 0)
            {
                sizerText.text = _messages.Peek();
                displayText.text = _messages.Peek();
                animator.SetTrigger(PlayHash);
            }
            else
            {
                animator.ResetTrigger(PlayHash);
                animator.ResetTrigger(ResetHash);
                gameObject.SetActive(false);
            }
        }
    }
}