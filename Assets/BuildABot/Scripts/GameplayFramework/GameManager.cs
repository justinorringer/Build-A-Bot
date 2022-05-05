using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    public sealed class GameManager : GameSingleton<GameManager>
    {
        private readonly Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private bool _paused;
        private LoadingScreen _loadingScreenInstance;

        private readonly GameState _gameState = new GameState();

        [Tooltip("The player prefab to spawn for new player instances.")]
        [SerializeField] private Player playerTemplate;

        [Tooltip("The screen to display when a game over occurs.")]
        [SerializeField] private GameOverDisplay gameOverDisplay;

        [Tooltip("The screen to display when waiting on a level to load.")]
        [SerializeField] private LoadingScreen loadingScreen;

        [Tooltip("An event triggered whenever the game is paused.")]
        [SerializeField] private UnityEvent onPause;
        
        [Tooltip("An event triggered whenever the game is unpaused.")]
        [SerializeField] private UnityEvent onUnpause;
        
        [Tooltip("An event triggered whenever the game pause state of the game changes.")]
        [SerializeField] private UnityEvent<bool> onSetPaused;
        
        [Tooltip("An event triggered whenever a level begins loading.")]
        [SerializeField] private UnityEvent onLevelBeginLoad;
        
        [Tooltip("An event triggered whenever a level finishes loading.")]
        [SerializeField] private UnityEvent onLevelLoaded;
        
        #region Public Properties

        /** Is the game currently paused? */
        public static bool Paused => Instance != null ? Instance._paused : Time.timeScale == 0.0f;

        /** The state of the current play session. */
        public static GameState GameState => Instance != null ? Instance._gameState : null; // TODO: Make a per-player GameState
        
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
        
        /** An event triggered whenever a level begins loading. */
        public static event UnityAction OnLevelBeginLoad
        {
            add => Instance.onLevelBeginLoad.AddListener(value);
            remove => Instance.onLevelBeginLoad.RemoveListener(value);
        }
        
        /** An event triggered whenever a level finishes loading. */
        public static event UnityAction OnLevelLoaded
        {
            add => Instance.onLevelLoaded.AddListener(value);
            remove => Instance.onLevelLoaded.RemoveListener(value);
        }
        
        #endregion

        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeInitialization()
        {
            SceneManager.LoadScene("PersistentGame", LoadSceneMode.Additive);
        }

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
            if (!Initialized) return null;
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

        /**
         * Opens the specified level. If using a single load scene mode, the current scene will be unloaded.
         * <param name="level">The level to load.</param>
         * <param name="mode">The mode to use when loading the level.</param>
         */
        public static void OpenLevel(string level, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!Initialized)
            {
                Debug.LogWarning("The game manager has not been initialized. Falling back to built in scene loading.");
                SceneManager.LoadScene(level, mode);
                return;
            }

            if (Instance._loadingScreenInstance != null) return;
            
            Pause();
            Player player = GetPlayer();
            // Force spawn a player so that a camera will be in the scene
            if (player != null)
            {
                player.DisableHUD();
                player.CloseMenu();
                player.PlayerController.enabled = false;
                player.CharacterMovement.ClearMovement();
            }

            AsyncOperation loadTask = null;
            Instance._loadingScreenInstance = Instantiate(Instance.loadingScreen, Instance.transform);
            Debug.Log("Showing loading screen");
            Instance._loadingScreenInstance.Begin(() => Mathf.Clamp01((loadTask?.progress ?? 0) / 0.9f),
                () => {
                    if (player == null)
                    {
                        player = Instantiate(Instance.playerTemplate, Vector3.zero, Quaternion.identity);
                        player.DisableHUD();
                        player.CloseMenu();
                        player.PlayerController.enabled = false;
                        player.CharacterMovement.ClearMovement();
                    }
                    Debug.Log("Beginning scene load");
                    Instance.onLevelBeginLoad.Invoke();
                    loadTask = SceneManager.LoadSceneAsync(level, mode);
                    loadTask.completed += asyncOperation =>
                    {
                        Debug.Log("Finished loading level");
                        Instance.onLevelLoaded.Invoke();
                        Instance._loadingScreenInstance.End(() =>
                        {
                            Debug.Log("Finished closing load screen");
                            Destroy(Instance._loadingScreenInstance.gameObject);
                            Instance._loadingScreenInstance = null;
                            Debug.Log("Resuming gameplay");
                            Unpause();
                            if (player != null)
                            {
                                player.EnableHUD();
                                player.PlayerController.enabled = true;
                            }
                        });
                    };
                });
        }

        public static void GameOver(Player player)
        {
            if (Initialized)
            {
                GameState.StopTime = Time.realtimeSinceStartupAsDouble;
                player.DisableHUD();
                player.CloseMenu();
                player.PlayerController.InputActions.Player.Disable();
                player.PlayerController.InputActions.DialogueUI.Enable();
                Pause();
                AudioManager.FadeOutBackgroundTrack(5f);
                GameOverDisplay displayInstance = Instantiate(Instance.gameOverDisplay, Instance.transform);

                void HandleDisplayFinished()
                {
                    displayInstance.OnFinish -= HandleDisplayFinished;
                    GameState.GameStage = 0;
                    GameState.NextLevelType = 0;
                    GameState.CompletedLevelCount = 0;
                    AsyncOperation loadingTask = SceneManager.LoadSceneAsync("BuildABot/Scenes/StartMenuScene", LoadSceneMode.Single);
                    loadingTask.completed += operation =>
                    {
                        Destroy(displayInstance.gameObject);
                    };
                }
                
                displayInstance.OnFinish += HandleDisplayFinished;
                displayInstance.Show();
            }
            else
            {
                Debug.LogWarning("Game over occured when GameManager was not initialized.");
                SceneManager.LoadScene("BuildABot/Scenes/StartMenuScene", LoadSceneMode.Single);
            }
        }
    }
}