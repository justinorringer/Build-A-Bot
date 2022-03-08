using System;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{

    /**
     * The base class used by all attributes with no contained data. This exists to facilitate storage
     * of attributes in generic lists and to provide a base implementation and interface for non-datatype-specific
     * behaviors.
     */
    [Serializable]
    public abstract class AttributeDataBase
    {
        /** The data type stored by this attribute. */
        public abstract Type DataType { get; }

        public AttributeSet Owner { get; private set; }

        /**
         * Initializes this attribute for runtime.
         * <param name="owner">The attribute set that owns this attribute.</param>
         */
        public virtual void Initialize(AttributeSet owner)
        {
            Owner = owner;
        }
    }

    /**
     * The type specific container used to hold attribute data.
     * <typeparam name="T">The type of dat stored in this attribute.</typeparam>
     */
    [Serializable]
    public abstract class AttributeData<T> : AttributeDataBase
    {
        [SerializeField] private T defaultValue;

        private T _baseValue;
        private T _currentValue;
        
        [Tooltip("An event triggered before modifying the base value of this attribute. Provides the new base value.")]
        [SerializeField] private UnityEvent<T> onPreBaseValueChange;
        [Tooltip("An event triggered before modifying the base value of this attribute. Provides the new base value.")]
        [SerializeField] private UnityEvent<T> onPostBaseValueChange;
        
        [Tooltip("An event triggered before modifying the current value of this attribute. Provides the new current value.")]
        [SerializeField] private UnityEvent<T> onPreValueChange;
        [Tooltip("An event triggered after modifying the current value of this attribute. Provides the new current value.")]
        [SerializeField] private UnityEvent<T> onPostValueChange;

        public override Type DataType => typeof(T);

        /** The default value assigned to this attribute. */
        public T DefaultValue => defaultValue;

        /** The current base value of this attribute without temporary modifiers. */
        public T BaseValue
        {
            get => _baseValue;
            set
            {
                onPreBaseValueChange.Invoke(value);
                _baseValue = value;
                onPostBaseValueChange.Invoke(value);
            }
        }
        /** The current value of this attribute including temporary modifiers. */
        public T CurrentValue
        {
            get => _currentValue;
            set
            {
                onPreValueChange.Invoke(value);
                _currentValue = value;
                onPostValueChange.Invoke(value);
            }
        }

        /**
         * Constructs a new AttributeData attribute container.
         * <param name="defaultValue">The default value for this attribute.</param>
         */
        protected AttributeData(T defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        public override void Initialize(AttributeSet owner)
        {
            base.Initialize(owner);
            _baseValue = defaultValue;
            _currentValue = defaultValue;
        }

        /**
         * Adds a new listener for the OnPreValueChange event.
         * <param name="listener">The listener to add.</param>
         */
        public void AddPreValueChangeListener(UnityAction<T> listener) => onPreValueChange.AddListener(listener);
        /**
         * Removes a listener from the OnPreValueChange event.
         * <param name="listener">The listener to remove.</param>
         */
        public void RemovePreValueChangeListener(UnityAction<T> listener) => onPreValueChange.RemoveListener(listener);

        /**
         * Adds a new listener for the OnPostValueChange event.
         * <param name="listener">The listener to add.</param>
         */
        public void AddPostValueChangeListener(UnityAction<T> listener) => onPostValueChange.AddListener(listener);
        /**
         * Removes a listener from the OnPostValueChange event.
         * <param name="listener">The listener to remove.</param>
         */
        public void RemovePostValueChangeListener(UnityAction<T> listener) => onPostValueChange.RemoveListener(listener);

        /**
         * Adds a new listener for the OnPreBaseValueChange event.
         * <param name="listener">The listener to add.</param>
         */
        public void AddPreBaseValueChangeListener(UnityAction<T> listener) => onPreBaseValueChange.AddListener(listener);
        /**
         * Removes a listener from the OnPreBaseValueChange event.
         * <param name="listener">The listener to remove.</param>
         */
        public void RemovePreBaseValueChangeListener(UnityAction<T> listener) => onPreBaseValueChange.RemoveListener(listener);

        /**
         * Adds a new listener for the OnPostBaseValueChange event.
         * <param name="listener">The listener to add.</param>
         */
        public void AddPostBaseValueChangeListener(UnityAction<T> listener) => onPostBaseValueChange.AddListener(listener);
        /**
         * Removes a listener from the OnPostBaseValueChange event.
         * <param name="listener">The listener to remove.</param>
         */
        public void RemovePostBaseValueChangeListener(UnityAction<T> listener) => onPostBaseValueChange.RemoveListener(listener);
    }

    /**
     * An attribute that uses a float value.
     */
    [Serializable]
    public sealed class FloatAttributeData : AttributeData<float>
    {
        /**
         * Constructs a new FloatAttributeData with a default value of 0.
         */
        public FloatAttributeData() : base(0)
        {
            
        }
        
        /**
         * Constructs a new FloatAttributeData with the provided default value.
         */
        public FloatAttributeData(float defaultValue) : base(defaultValue)
        {
            
        }
    }

    /**
     * An attribute that uses an integer value.
     */
    [Serializable]
    public sealed class IntAttributeData : AttributeData<int>
    {
        /**
         * Constructs a new IntAttributeData with a default value of 0.
         */
        public IntAttributeData() : base(0)
        {
            
        }
        
        /**
         * Constructs a new IntAttributeData with the provided default value.
         */
        public IntAttributeData(int defaultValue) : base(defaultValue)
        {
            
        }
    }
}