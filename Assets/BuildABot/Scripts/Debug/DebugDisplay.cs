using TMPro;
using UnityEngine;

namespace BuildABot
{
    /**
     * A component used to control the debug displays presented to the player.
     */
    public class DebugDisplay : MonoBehaviour
    {
        [Header("Command Console")]
        
        [Tooltip("Does this debug display allow the user to summon the command console?")]
        [SerializeField] private bool allowCommandConsole = true;

        [Tooltip("The key used to open the command prompt.")]
        [SerializeField] private KeyCode commandConsoleKey = KeyCode.BackQuote;

        [Tooltip("A reference to the child command console of this debug display.")]
        [SerializeField] private CommandConsole console;
        
        /** should the console be shown? */
        private bool _showConsole;

        [Header("Data Displays")]
        
        [Tooltip("A reference to the debug display for FPS.")]
        [SerializeField] private TMP_Text fpsDisplay;

        protected void Update()
        {
            if (allowCommandConsole && Input.GetKeyDown(commandConsoleKey))
            {
                // Toggle the command console
                _showConsole = !_showConsole;
                console.gameObject.SetActive(_showConsole);
                console.ClearInput();
                Cursor.visible = _showConsole;
                if (_showConsole)
                {
                    console.Focus();
                }
            }

            if (fpsDisplay.enabled)
            {
                fpsDisplay.text = $"FPS: {1.0f / Time.unscaledDeltaTime}";
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
    }
}