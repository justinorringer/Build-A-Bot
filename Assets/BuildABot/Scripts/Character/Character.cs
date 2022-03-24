using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{
    /**
     * The base class used by all character types.
     */
    public abstract class Character : MonoBehaviour
    {

        [Tooltip("The attributes available to this character.")]
        [SerializeField] private CharacterAttributeSet attributes;

        [Header("Events")]
        
        [Tooltip("A dispatcher called whenever this character is killed.")]
        [SerializeField] private UnityEvent onDeath;

        /** The attribute set used by this character. */
        public CharacterAttributeSet Attributes => attributes;

        /** The character movement component used by this character. */
        public abstract CharacterMovement CharacterMovement { get; }
        
        /** The bounding size of this character. */
        public Collider2D Collider { get; private set; }
        
        /** The bounding size of this character. */
        public Vector2 Bounds { get; private set; }

        public virtual void Kill()
        {
            onDeath.Invoke();
            Destroy(gameObject);
        }

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider2D>();
            Bounds = Collider.bounds.size;
        }

        protected virtual void OnEnable()
        {
            Attributes.Temperature.AddPostValueChangeListener(HandleTemperatureChange);
        }

        protected virtual void OnDisable()
        {
            Attributes.Temperature.RemovePostValueChangeListener(HandleTemperatureChange);
        }

        private void HandleTemperatureChange(float newTemperature)
        {
            if (newTemperature >= Attributes.MaxTemperature.CurrentValue) Kill();
        }
    }
}