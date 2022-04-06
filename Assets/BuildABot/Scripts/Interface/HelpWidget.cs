using System;
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
            titleComponent.text = title;
            content.text = message;
            buttonText.text = acknowledgeMessage;
            button.Select();
        }

        protected void OnDestroy()
        {
            if (_player != null)
            {
                _player.SetPaused(false);
                _player.EnableHUD();
                Cursor.visible = false;
            }
        }
    }
}