using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    public abstract class GameSingleton<TDerived> : MonoBehaviour where TDerived : GameSingleton<TDerived>
    {

        [Tooltip("An event triggered whenever this object is finished initializing.")]
        [SerializeField] private UnityEvent onInitialized;
        
        /** The singleton instance in the scene. */
        protected static TDerived Instance { get; private set; }

        /** Checks if this manager has already been initialized. */
        public static bool Initialized => Instance != null;
        
        #region Events
        
        /** An event triggered whenever this singleton is finished initializing. */
        public static event UnityAction OnInitialized
        {
            add => Instance.onInitialized.AddListener(value);
            remove => Instance.onInitialized.RemoveListener(value);
        }
        
        #endregion

        protected virtual void Awake()
        {
            Debug.Assert(Instance == null, "Multiple instances of a singleton cannot exist in the same scene.");
            if (Instance == null)
            {
                Instance = this as TDerived;
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