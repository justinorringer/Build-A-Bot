using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class HelpWidget : MonoBehaviour
    {

        [SerializeField] private TMP_Text titleComponent;
        [SerializeField] private TMP_Text content;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Button button;

        private Player _player;

        public void Initialize(Player player, string message, string title = "Alert", string acknowledgeMessage = "OK")
        {
            Debug.Assert(player != null, "A help message can only be displayed if a valid player reference exists.");
            _player = player;
            _player.PlayerController.InputActions.Player.Disable();
            _player.PlayerController.InputActions.UI.Enable();
            titleComponent.text = title;
            content.text = player.PerformStandardTokenReplacement(message);
            buttonText.text = acknowledgeMessage;
            button.interactable = false;
            Utility.DelayedFunction(this, 2.0f, () =>
            {
                button.interactable = true;
                button.Select();
            }, true);
        }

        protected void OnDestroy()
        {
            if (_player != null)
            {
                _player.SetPaused(false);
                _player.EnableHUD();
                Cursor.visible = false;
                _player.PlayerController.InputActions.Player.Enable();
                _player.PlayerController.InputActions.UI.Disable();
            }
        }
    }
}