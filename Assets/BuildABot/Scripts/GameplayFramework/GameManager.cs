using UnityEngine;

namespace BuildABot.GameplayFramework
{
    public sealed class GameManager : MonoBehaviour
    {
        
        // TODO: require a PlayerInputManager component
        
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            Debug.Assert(Instance == null, "Multiple instances of GameManager cannot exist in the same scene.");
            if (Instance == null) Instance = this;
            else
            {
                Destroy(this); // Destroy the violating instance
            }
        }

        private void Start()
        {
            //PlayerInputManager.onPlayerJoined += RegisterPlayer
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            //PlayerInputManager.onPlayerJoined
        }

        private void RegisterPlayer(Player player)
        {
            // TODO: Switch to new input system, bind to on player joined
        }

        public static void GetPlayer(int playerIndex)
        {
            Debug.Assert(Instance != null, "Attempting to use GameManager without properly initializing one in the scene.");
        }
    }
}