using System;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{

    /**
     * The base container type used to hold attribute data.
     * <typeparam name="T">The type of dat stored in this attribute.</typeparam>
     */
    [Serializable]
    public abstract class AttributeData<T>
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
    public class FloatAttributeData : AttributeData<float>
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
    public class IntAttributeData : AttributeData<int>
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