using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BuildABot
{
    public class MainMenuLandingPage : MonoBehaviour
    {
        [SerializeField] private MainMenu mainMenu;
        
        [SerializeField] private List<Button> buttons;

        [SerializeField] private Button quitToDesktopButton;

        protected void OnEnable()
        {
            
#if DEMO_BUILD
            //quitToDesktopButton.interactable = false;
#endif
            
            // Select the first found button
            foreach (Button button in buttons)
            {
                if (button.enabled && button.interactable)
                {
                    button.Select();
                    break;
                }
            }

            mainMenu.Player.PlayerController.InputActions.UI.Back.performed += Input_Back;
        }

        protected void OnDisable()
        {
            mainMenu.Player.PlayerController.InputActions.UI.Back.performed -= Input_Back;
        }

        private void Input_Back(InputAction.CallbackContext context)
        {
            mainMenu.Player.CloseMenu(); // Close main menu
        }
    }
}