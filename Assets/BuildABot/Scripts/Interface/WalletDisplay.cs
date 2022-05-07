using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class WalletDisplay : MonoBehaviour
    {
        [SerializeField] private HUD hud;
        [SerializeField] private Image background;
        [SerializeField] private Image border;
        [SerializeField] private TMP_Text walletNumber;
        private IEnumerator _displayTask;

        protected void Awake()
        {
            GameManager.OnLevelLoaded += OnNewLevel;
            hud.Player.OnWalletChanged += OnWalletUpdate;
            OnNewLevel();
            OnWalletUpdate(0, 0);
        }

        protected void OnDestroy()
        {
            hud.Player.OnWalletChanged -= OnWalletUpdate;
            if (GameManager.Initialized) GameManager.OnLevelLoaded -= OnNewLevel;
        }

        private void OnNewLevel()
        {
            Color color;
            Color backgroundColor;
            switch (GameManager.GameState.NextLevelType) { //0 for normal, 1 for frozen, 2 for advanced
                case 0:
                    color = new Color(0.7764706f, 0.7607843f, 1.0f);
                    backgroundColor = new Color(0.40f, 0.42f, 0.47f);
                    break;
                case 1:
                    color = new Color(0.4039216f, 0.7490196f, 0.9058824f);
                    backgroundColor = new Color(0.30f, 0.45f, 0.57f);
                    break;
                case 2:
                    color = new Color(0.6705883f, 0.2f, 0.1882353f);
                    backgroundColor = new Color(0.31f, 0.21f, 0.21f);
                    break;
                default:
                    color = new Color( 0.0f, 0.0f, 0.0f);
                    //background = new Color(0.4078431f, 0.4156863f, 0.4745098f);
                    backgroundColor = new Color(0.40f, 0.42f, 0.47f);
                    break;
            }

            //background.color = backgroundColor;
            background.color = color * 0.75f;
            border.color = color;
        }

        private void OnWalletUpdate(int oldValue, int newValue)
        {
            //walletNumber.text = $"$ {newValue}";
            int delta = newValue - oldValue;
            int interval = delta / 20;

            int i = 0;
            if (_displayTask != null)
            {
                walletNumber.text = $"$ {oldValue}";
                StopCoroutine(_displayTask);
            }
            _displayTask = Utility.RepeatFunction(this, () =>
            {
                walletNumber.text = $"$ {oldValue + (i * interval)}";
                i++;
            }, 0.05f, 20, () =>
            {
                _displayTask = null;
                walletNumber.text = $"$ {newValue}";
            });
        }
    }
}