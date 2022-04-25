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

        protected void Awake()
        {
            gameObject.SetActive(false);
            _anim = GetComponent<Animator>();
        }

        /**
         * Displays the provided message with token replacement.
         * <param name="message">The message to display.</param>
         */
        public void ShowMessage(string message)
        {
            if (message == null) return;
            gameObject.SetActive(true);
            textArea.text = owner.Player.PerformStandardTokenReplacement(message);
        }

        /**
         * Begins the closing animation for this message.
         */
        public void HideMessage()
        {
            _anim.SetTrigger(HideHash);
        }

        /**
         * Clears and hides this message display. This should not be called manually.
         */
        public void Clear()
        {
            gameObject.SetActive(false);
            textArea.text = "";
        }
    }
}