using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BuildABot
{
    public class MainMenuLandingPage : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        
        [SerializeField] private List<Button> buttons;

        protected void OnEnable()
        {
            if (buttons.Count > 0)
            {
                buttons[0].Select();
            }

            mainMenu.Player.PlayerController.InputActions.UI.Back.performed += Input_Back;
        }

        protected void OnDisable()
        {
            mainMenu.Player.PlayerController.InputActions.UI.Back.performed -= Input_Back;
        }

        private void Input_Back(InputAction.CallbackContext context)
        {
            mainMenu.Player.ToggleMenu(); // Close main menu
        }
    }
}