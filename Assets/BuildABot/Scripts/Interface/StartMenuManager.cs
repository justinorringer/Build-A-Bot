using System;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class StartMenuManager : MonoBehaviour
    {

        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        
        protected void Awake()
        {
            Cursor.visible = true;
        }

        protected void Start()
        {
            // TODO: Check for save data, if it exists enable the continue button, otherwise focus on new game
            continueButton.interactable = false;
            newGameButton.Select();
        }
    }
}