using System;
using System.Collections.Generic;
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
            protected set
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
            protected set
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

        /**
         * Applies the provided list of modifiers to this attribute. This will recalculate the current value of this
         * attribute using any temporary modifiers and recalculate the base value from instant modifiers.
         * <param name="modifiers">The modifiers to apply.</param>
         * <param name="targetBase">Should the base value be targeted by these modifiers?</param>
         * <param name="snapshot">The snapshot of current values for all attributes in the owning set.</param>
         */
        public abstract void ApplyModifiers(List<AttributeModifier<T>> modifiers, bool targetBase, Dictionary<AttributeData<T>, T> snapshot);
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

        public override void ApplyModifiers(List<AttributeModifier<float>> modifiers, bool targetBase, Dictionary<AttributeData<float>, float> snapshot)
        {
            
            float baseValue = BaseValue;

            float baseAdd = 0;
            float baseMultiply = 1.0f;
            float baseDivide = 1.0f;
            
            float tempAdd = 0;
            float tempMultiply = 1.0f;
            float tempDivide = 1.0f;
        
            foreach (AttributeModifier<float> modifier in modifiers)
            {
                float value = modifier.GetModifierValue(Owner, snapshot); // TODO: Use the snapshot version for attribute value providers
            
                switch (modifier.OperationType)
                {
                    case EAttributeModifierOperationType.Add:
                        if (targetBase) baseAdd += value;
                        else tempAdd += value;
                        break;
                    case EAttributeModifierOperationType.Multiply:
                        if (targetBase) baseMultiply += value - 1;
                        else tempMultiply += value;
                        break;
                    case EAttributeModifierOperationType.Divide:
                        if (targetBase) baseDivide += value - 1;
                        else tempDivide += value;
                        break;
                    case EAttributeModifierOperationType.Replace:
                        if (targetBase) BaseValue = value;
                        else CurrentValue = value;
                        return;
                }
            }

            baseValue = ((baseValue + baseAdd) * baseMultiply) / baseDivide;
            float currentValue = ((baseValue + tempAdd) * tempMultiply) / tempDivide;

            BaseValue = baseValue;
            CurrentValue = currentValue;
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

        public override void ApplyModifiers(List<AttributeModifier<int>> modifiers, bool targetBase, Dictionary<AttributeData<int>, int> snapshot)
        {
            
            int baseValue = BaseValue;

            int baseAdd = 0;
            int baseMultiply = 1;
            int baseDivide = 1;
            
            int tempAdd = 0;
            int tempMultiply = 1;
            int tempDivide = 1;
        
            foreach (AttributeModifier<int> modifier in modifiers)
            {
                int value = modifier.GetModifierValue(Owner, snapshot);
            
                switch (modifier.OperationType)
                {
                    case EAttributeModifierOperationType.Add:
                        if (targetBase) baseAdd += value;
                        else tempAdd += value;
                        break;
                    case EAttributeModifierOperationType.Multiply:
                        if (targetBase) baseMultiply += value - 1;
                        else tempMultiply += value;
                        break;
                    case EAttributeModifierOperationType.Divide:
                        if (targetBase) baseDivide += value - 1;
                        else tempDivide += value;
                        break;
                    case EAttributeModifierOperationType.Replace:
                        if (targetBase) BaseValue = value;
                        else CurrentValue = value;
                        return;
                }
            }

            baseValue = ((baseValue + baseAdd) * baseMultiply) / baseDivide;
            int currentValue = ((baseValue + tempAdd) * tempMultiply) / tempDivide;

            BaseValue = baseValue;
            CurrentValue = currentValue;
        }
    }
}