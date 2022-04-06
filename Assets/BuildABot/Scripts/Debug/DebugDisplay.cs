using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuildABot
{
    /**
     * A component used to control the debug displays presented to the player.
     */
    public class DebugDisplay : MonoBehaviour
    {

        [Header("References")]
        
        [Tooltip("The player controlling this display.")]
        [SerializeField] private Player player;
        
        [Header("Command Console")]
        
        [Tooltip("Does this debug display allow the user to summon the command console?")]
        [SerializeField] private bool allowCommandConsole = true;

        [Tooltip("A reference to the child command console of this debug display.")]
        [SerializeField] private CommandConsole console;
        
        [Header("Data Displays")]
        
        [Tooltip("A reference to the debug display for FPS.")]
        [SerializeField] private TMP_Text fpsDisplay;
        
        protected void OnEnable()
        {
            if (allowCommandConsole)
            {
                player.PlayerController.InputActions.Player.OpenConsole.performed += Input_OpenConsole;
                player.PlayerController.InputActions.ConsoleUI.Close.performed += Input_CloseConsole;
            }
        }

        protected void OnDisable()
        {
            if (allowCommandConsole)
            {
                player.PlayerController.InputActions.Player.OpenConsole.performed -= Input_OpenConsole;
                player.PlayerController.InputActions.ConsoleUI.Close.performed -= Input_CloseConsole;
            }
        }

        protected void Update()
        {
            if (fpsDisplay.enabled)
            {
                fpsDisplay.text = $"FPS: {1.0f / Time.unscaledDeltaTime}";
            }
        }

        private void SetShowConsole(bool show)
        {
            // Toggle the command console
            console.gameObject.SetActive(show);
            console.ClearInput();
            Cursor.visible = show;
            if (show)
            {
                console.Focus();
            }
        }

        /**
         * Toggles the FPS display on screen.
         */
        public void ToggleFPS()
        {
            bool show = !fpsDisplay.enabled;
            fpsDisplay.enabled = show;
            Debug.LogFormat("FPS display {0}", show ? "enabled" : "disabled");
        }

        private void Input_OpenConsole(InputAction.CallbackContext context)
        {
            SetShowConsole(true);
        }
        
        private void Input_CloseConsole(InputAction.CallbackContext context)
        {
            SetShowConsole(false);
        }
    }
}