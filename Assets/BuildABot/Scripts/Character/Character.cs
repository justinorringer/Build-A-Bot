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
        
        [Header("References")]
        
        [Tooltip("The combat controller used by this character.")]
        [SerializeField] private CombatController combatController;

        /** The attribute set used by this character. */
        public CharacterAttributeSet Attributes => attributes;

        /** The character movement component used by this character. */
        public abstract CharacterMovement CharacterMovement { get; }

        /** The combat controller used by this character. */
        public CombatController CombatController => combatController;
        
        /** The collider of this character. */
        public Collider2D Collider { get; private set; }
        
        /** The bounding size of this character. */
        public Vector2 Bounds { get; private set; }
        
        /** The inventory of this character. */
        public Inventory Inventory { get; private set; }
        
        /** This character's sprite renderer. */
        public SpriteRenderer SpriteRenderer { get; private set; }
        
        /** An event triggered when this character dies. */
        public event UnityAction OnDeath
        {
            add => onDeath.AddListener(value);
            remove => onDeath.RemoveListener(value);
        }

        
        /** The cooling/temperature regeneration coroutine used by this character. */
        private IEnumerator _coolingTask;

        protected void StopCooling()
        {
            if (_coolingTask != null)
            {
                StopCoroutine(_coolingTask);
                _coolingTask = null;
            }
        }

        /**
         * Kills this character.
         */
        protected virtual void Kill()
        {
            onDeath.Invoke();
            Destroy(gameObject);
        }

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider2D>();
            Bounds = Collider.bounds.size;
            Inventory = GetComponent<Inventory>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            Attributes.Temperature.OnPostValueChange += HandleTemperatureChange;
        }

        protected virtual void OnEnable()
        {
            _coolingTask = Utility.RepeatFunction(this, () =>
            {
                float currentTemp = Attributes.Temperature.BaseValue;
                float operatingTemp = Attributes.OperatingTemperature.CurrentValue;
                float coolingRate = Attributes.CoolDownRate.CurrentValue;

                if (currentTemp >= Attributes.MaxTemperature.CurrentValue) Kill();
                else if (currentTemp <= Attributes.MinTemperature.CurrentValue) Kill();
                
                if (currentTemp > operatingTemp && coolingRate != 0.0f)
                {
                    // Lower character temperature 
                    Attributes.Temperature.BaseValue = Mathf.Max(currentTemp - coolingRate, operatingTemp);
                }
                else if (currentTemp < operatingTemp && coolingRate != 0.0f)
                {
                    // Raise character temperature 
                    Attributes.Temperature.BaseValue = Mathf.Min(currentTemp + coolingRate, operatingTemp);
                }
            }, 1.0f);
        }

        protected virtual void OnDisable()
        {
            StopCooling();
        }

        protected virtual void OnDestroy()
        {
            Attributes.Temperature.OnPostValueChange -= HandleTemperatureChange;
        }

        private void HandleTemperatureChange(float newTemperature)
        {
            if (newTemperature >= Attributes.MaxTemperature.CurrentValue) Kill();
            else if (newTemperature <= Attributes.MinTemperature.CurrentValue) Kill();
        }
    }
}