using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class ChangeColorOnDisable : MonoBehaviour
    {

        [SerializeField] private Color enabledFontColor = Color.white;
        [SerializeField] private Color disabledFontColor = new Color(0.4f, 0.4f, 0.4f);
        
        protected void OnEnable()
        {
            Selectable parent = GetComponentInParent<Selectable>();
            if (parent != null && !parent.interactable)
            {
                TMP_Text text = GetComponent<TMP_Text>();
                if (text != null) text.color = disabledFontColor;
            }
            else
            {
                TMP_Text text = GetComponent<TMP_Text>();
                if (text != null) text.color = enabledFontColor;
            }
        }
    }
}