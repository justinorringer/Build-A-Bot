using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public sealed class GameManager : GameSingleton<GameManager>
    {
        private readonly Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private bool _paused;

        private readonly GameState _gameState = new GameState();

        [Tooltip("An event triggered whenever the game is paused.")]
        [SerializeField] private UnityEvent onPause;
        
        [Tooltip("An event triggered whenever the game is unpaused.")]
        [SerializeField] private UnityEvent onUnpause;
        
        [Tooltip("An event triggered whenever the game pause state of the game changes.")]
        [SerializeField] private UnityEvent<bool> onSetPaused;
        
        #region Public Properties

        /** Is the game currently paused? */
        public static bool Paused => Instance != null ? Instance._paused : Time.timeScale == 0.0f;

        /** The state of the current play session. */
        public static GameState GameState => Instance != null ? Instance._gameState : null;
        
        #endregion
        
        #region Events
        
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

        protected override void Awake()
        {
            _paused = false;
            base.Awake();
        }

        protected override void OnDestroy()
        {
            onPause.RemoveAllListeners();
            onUnpause.RemoveAllListeners();
            onSetPaused.RemoveAllListeners();
            base.OnDestroy();
        }

        internal static void RegisterPlayer(Player player, Action<int> onComplete)
        {
            if (onComplete == null) throw new ArgumentNullException();
            if (player == null || player.PlayerIndex != -1) onComplete.Invoke(-1);

            void RegisterImplementation()
            {
                int index = Instance._players.Count;
                Instance._players.Add(index, player);
                player.OnPlayerDestroyed += UnregisterPlayer;
                Debug.Log($"Registered player {index}");
                onComplete.Invoke(index);
            }

            void LatentRegister()
            {
                OnInitialized -= LatentRegister;
                RegisterImplementation();
            }

            if (Initialized) RegisterImplementation();
            else OnInitialized += LatentRegister;
        }

        private static void UnregisterPlayer(Player player)
        {
            player.OnPlayerDestroyed -= UnregisterPlayer;
            if (Initialized)
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
            if (Initialized)
            {
                if (paused == Paused) return; // Do nothing, no change
                Time.timeScale = paused ? 0.0f : 1; // Update time scale
                Instance._paused = paused;
            
                // Fire events
                Instance.onSetPaused.Invoke(paused);
                if (paused) Instance.onPause.Invoke();
                else Instance.onUnpause.Invoke();
            }
            else
            {
                Time.timeScale = paused ? 0.0f : 1; // Update time scale
            }
        }
    }
}