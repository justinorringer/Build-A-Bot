using System.Collections;
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
        [SerializeField] protected UnityEvent onDeath;

        /** The attribute set used by this character. */
        public CharacterAttributeSet Attributes => attributes;

        /** The character movement component used by this character. */
        public abstract CharacterMovement CharacterMovement { get; }
        
        /** The bounding size of this character. */
        public Collider2D Collider { get; private set; }
        
        /** The bounding size of this character. */
        public Vector2 Bounds { get; private set; }
        
        /** The inventory of this character. */
        public Inventory Inventory { get; private set; }

        /** The cooling/temperature regeneration coroutine used by this character. */
        private IEnumerator _coolingTask;

        public virtual void Kill()
        {
            onDeath.Invoke();
            Destroy(gameObject);
        }

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider2D>();
            Bounds = Collider.bounds.size;
            Inventory = GetComponent<Inventory>();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void OnEnable()
        {
            Attributes.Temperature.OnPostValueChange += HandleTemperatureChange;
            _coolingTask = Utility.RepeatFunction(this, () =>
            {
                float currentTemp = Attributes.Temperature.BaseValue;
                float minTemp = Attributes.BaseTemperature.CurrentValue;
                float coolingRate = Attributes.CoolDownRate.CurrentValue;
                
                if (currentTemp > minTemp && coolingRate != 0.0f)
                {
                    // Lower character temperature 
                    Attributes.Temperature.BaseValue = Mathf.Max(currentTemp - coolingRate, minTemp);
                }
            }, 1.0f);
        }

        protected virtual void OnDisable()
        {
            Attributes.Temperature.OnPostValueChange -= HandleTemperatureChange;
            StopCoroutine(_coolingTask);
            _coolingTask = null;
        }

        private void HandleTemperatureChange(float newTemperature)
        {
            if (newTemperature >= Attributes.MaxTemperature.CurrentValue) Kill();
        }
    }
}