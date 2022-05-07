using System;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class TokenReplacedText : MonoBehaviour
    {
        private TMP_Text _tmpText;
        private string _source;

        protected void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
            _source = _tmpText.text;
        }

        protected void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            _tmpText.text = GameManager.GetPlayer().PerformStandardTokenReplacement(_source);
        }
    }
}