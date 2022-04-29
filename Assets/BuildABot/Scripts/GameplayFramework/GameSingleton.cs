using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public abstract class GameSingleton<TDerived> : MonoBehaviour where TDerived : GameSingleton<TDerived>
    {

        [Tooltip("An event triggered whenever this object is finished initializing.")]
        [SerializeField] private UnityEvent onInitialized;

        private static readonly HashSet<UnityAction> _pendingInitializedActions = new HashSet<UnityAction>();
        
        /** The singleton instance in the scene. */
        protected static TDerived Instance { get; private set; }

        /** Checks if this manager has already been initialized. */
        public static bool Initialized => Instance != null;
        
        #region Events
        
        /** An event triggered whenever this singleton is finished initializing. */
        public static event UnityAction OnInitialized
        {
            add
            {
                if (Instance != null) Instance.onInitialized.AddListener(value);
                else _pendingInitializedActions.Add(value);
            }
            remove
            {
                if (Instance != null) Instance.onInitialized.RemoveListener(value);
                else _pendingInitializedActions.Remove(value);
            }
        }
        
        #endregion

        protected virtual void Awake()
        {
            Debug.Assert(Instance == null, "Multiple instances of a singleton cannot exist in the same scene.");
            if (Instance == null)
            {
                Instance = this as TDerived;
                foreach (UnityAction action in _pendingInitializedActions)
                {
                    onInitialized.AddListener(action);
                }
                _pendingInitializedActions.Clear();
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this); // Destroy the violating instance
                return;
            }
            onInitialized.Invoke();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}