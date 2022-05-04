using System;
using UnityEngine;

namespace BuildABot
{
    public class DisableOnLoad : MonoBehaviour
    {

        private bool _shouldEnable;
        
        protected void Awake()
        {
            _shouldEnable = enabled;
            enabled = false;
            GameManager.OnInitialized += HandleGameManagerInitialized;
        }

        private void HandleGameManagerInitialized()
        {
            GameManager.OnInitialized -= HandleGameManagerInitialized;
            enabled = _shouldEnable;
        }

        protected void OnEnable()
        {
            if (GameManager.Initialized) GameManager.OnLevelBeginLoad += HandleBeginLoad;
        }

        protected void OnDisable()
        {
            if (GameManager.Initialized) GameManager.OnLevelBeginLoad -= HandleBeginLoad;
        }

        private void HandleBeginLoad()
        {
            gameObject.SetActive(false);
        }
    }
}