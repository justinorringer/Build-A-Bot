using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public sealed class GameManager : MonoBehaviour
    {
        private readonly Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private bool _paused;

        [Tooltip("An event triggered whenever the game manager is finished initializing.")]
        [SerializeField] private UnityEvent onInitialized;

        [Tooltip("An event triggered whenever the game is paused.")]
        [SerializeField] private UnityEvent onPause;
        
        [Tooltip("An event triggered whenever the game is unpaused.")]
        [SerializeField] private UnityEvent onUnpause;
        
        [Tooltip("An event triggered whenever the game pause state of the game changes.")]
        [SerializeField] private UnityEvent<bool> onSetPaused;
        
        /** The singleton instance of this game manager. */
        private static GameManager Instance { get; set; }
        
        #region Public Properties

        /** Checks if this manager has already been initialized. */
        public static bool Initialized => Instance != null;

        /** Is the game currently paused? */
        public static bool Paused => Instance._paused;
        
        #endregion
        
        #region Events
        
        /** An event triggered whenever the game manager is finished initializing. */
        public static event UnityAction OnInitialized
        {
            add => Instance.onInitialized.AddListener(value);
            remove => Instance.onInitialized.RemoveListener(value);
        }
        
        /** An event triggered whenever the game is paused. */
        public static event UnityAction OnPause
        {
            add => Instance.onPause.AddListener(value);
            remove => Instance.onPause.RemoveListener(value);
        }
        
        /** An event triggered whenever the game is unpaused. */
        public static event UnityAction OnUnpause
        {
            add => Instance.onUnpause.AddListener(value);
            remove => Instance.onUnpause.RemoveListener(value);
        }
        
        /** An event triggered whenever the game pause state of the game changes. */
        public static event UnityAction<bool> OnSetPaused
        {
            add => Instance.onSetPaused.AddListener(value);
            remove => Instance.onSetPaused.RemoveListener(value);
        }
        
        #endregion

        private void Awake()
        {
            Debug.Assert(Instance == null, "Multiple instances of GameManager cannot exist in the same scene.");
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this); // Destroy the violating instance
                return;
            }

            _paused = false;
            onInitialized.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            onPause.RemoveAllListeners();
            onUnpause.RemoveAllListeners();
            onSetPaused.RemoveAllListeners();
        }

        internal static int RegisterPlayer(Player player)
        {
            if (player == null || player.PlayerIndex != -1) return -1;
            
            int index = Instance._players.Count;
            Instance._players.Add(index, player);
            player.OnPlayerDestroyed += UnregisterPlayer;
            
            Debug.Log($"Registered player {index}");
            return index;
        }

        private static void UnregisterPlayer(Player player)
        {
            player.OnPlayerDestroyed -= UnregisterPlayer;
            if (Instance != null)
            {
                Instance._players.Remove(player.PlayerIndex);
                Debug.Log($"Unregistered player {player.PlayerIndex}");
            }
        }

        /**
         * Gets the player specified by the provided player index. In singleplayer, the only valid index will be 0.
         * the player index can be retrieved from a player's PlayerInput component.
         * <param name="playerIndex">The player index to search for. Defaults to 0.</param>
         * <returns>The player with the provided index if found, otherwise null.</returns>
         */
        public static Player GetPlayer(int playerIndex = 0)
        {
            Debug.Assert(Instance != null, "Attempting to use GameManager without properly initializing one in the scene.");
            return Instance._players.TryGetValue(playerIndex, out Player player) ? player : null;
        }

        /**
         * Gets all players in the current session.
         * <returns>All players active in the current session.</returns>
         */
        public static List<Player> GetAllPlayers()
        {
            List<Player> result = new List<Player>();
            foreach (var entry in Instance._players)
            {
                result.Add(entry.Value);
            }
            return result;
        }

        /**
         * Pauses the game if it is currently playing.
         */
        public static void Pause()
        {
            SetPaused(true);
        }

        /**
         * Resumes the game if it is currently paused.
         */
        public static void Unpause()
        {
            SetPaused(false);
        }

        /**
         * Explicitly sets the paused state of the game.
         * <param name="paused">The new paused state of the game.</param>
         */
        public static void SetPaused(bool paused)
        {
            if (paused == Paused) return; // Do nothing, no change
            Time.timeScale = paused ? 0.0f : 1; // Update time scale
            Instance._paused = paused;
            
            // Fire events
            Instance.onSetPaused.Invoke(paused);
            if (paused) Instance.onPause.Invoke();
            else Instance.onUnpause.Invoke();
        }
    }
}