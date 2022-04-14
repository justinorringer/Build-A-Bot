using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class StartMenuManager : MonoBehaviour
    {

        [SerializeField] private Button newGameButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private TMP_Text versionDisplay;
        
        protected void Awake()
        {
            Cursor.visible = true;
        }

        protected void Start()
        {
            newGameButton.Select();
#if DEMO_BUILD
            //quitGameButton.interactable = false;
            versionDisplay.gameObject.SetActive(true);
            versionDisplay.text = "Demo - " + Application.version;
#else
            versionDisplay.gameObject.SetActive(true);
            versionDisplay.text = Application.version;
#endif
        }
    }
}