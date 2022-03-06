using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{

    /**
     * The type of operation performed on an attribute by a modifier.
     */
    public enum EAttributeModifierOperationType
    {
        /** Adds to the value. For subtraction, use a negative value. */
        Add,
        /** Multiplies the value. */
        Multiply,
        /** Divides the value. */
        Divide,
        /** Overwrites the value. */
        Replace
    }

    /**
     * The source used when computing the value applied by a modifier.
     * TODO: Replace with type based selector
     */
    public enum EAttributeModifierValueProviderType
    {
        /** A constant value source. */
        Constant,
        /** Another attribute in the targeted set acts as the source. */
        Attribute,
        /** A custom calculation class is used to determine the value. Currently unused. */
        //CustomCalculation
    }
    
    /**
     * The base class for modifiers that can be used to apply changes to an AttributeData instance.
     */
    [Serializable]
    public abstract class AttributeModifierBase
    {
        [Tooltip("The operation performed by this modifier.")]
        [SerializeField] private EAttributeModifierOperationType operationType;
        [Tooltip("The value provider type by this modifier.")]
        [SerializeField] private EAttributeModifierValueProviderType valueProviderType;

        /** The internally serialized attribute set targeted by this modifier. */
        [HideInInspector]
        [SerializeField] private AttributeSetSelector attributeSet;

        
        /** The attribute set type targeted by this modifier. */
        public Type AttributeSetTarget => attributeSet.SelectedType;

        /** The operation performed by this modifier. */
        public EAttributeModifierOperationType OperationType => operationType;

        /** The value provider type used by this modifier. */
        public EAttributeModifierValueProviderType ValueProviderType => valueProviderType;
        
        /** The type of data impacted by this modifier. Usually float or int. */
        public abstract Type DataType { get; }

        
        /**
         * Initializes this modifier and targets the attribute set specified by the provided selector.
         * <param name="target">The selector that contains the targeted AttributeSet type.</param>
         */
        public void Initialize(AttributeSetSelector target)
        {
            attributeSet = target;
            UpdateAttributeSelector();
            InitializeValueProvider();
        }

        /**
         * Updates the internal attribute selector when the attribute set target is changed.
         */
        protected abstract void UpdateAttributeSelector();

        /**
         * Initializes the value provider in this modifier to the default value.
         */
        protected abstract void InitializeValueProvider();
    }

    /**
     * The typed base class of all AttributeModifiers.
     * <typeparam name="T">The type of data targeted by this modifier.</typeparam>
     */
    [Serializable]
    public abstract class AttributeModifier<T> : AttributeModifierBase
    {

        [Tooltip("The value provider for this modifier.")]
        [SerializeReference] protected AttributeModifierValueProviderBase valueProvider;

        /** The value provider for this modifier. */
        protected AttributeModifierValueProvider<T> ValueProvider => valueProvider as AttributeModifierValueProvider<T>;
        
        /** The attribute targeted by this modifier. */
        public abstract AttributeSelector<T> Attribute { get; }

        public override Type DataType => typeof(T);
        
        /**
         * Gets the value of this modifier.
         * <param name="target">The attribute set targeted by this modifier.</param>
         * <param name="snapshot">The snapshot of current values before applying any modifiers in the set.</param>
         * <returns>The value of this modifier calculated from its value provider.</returns>
         */
        public T GetModifierValue(AttributeSet target, Dictionary<AttributeData<T>, T> snapshot) => ValueProvider.GetModifierValue(target, snapshot);
        
        protected override void UpdateAttributeSelector()
        {
            Attribute.InitializeForAttributeSetType(AttributeSetTarget);
        }
        
    }

    /**
     * An attribute modifier that targets a float attribute.
     */
    [Serializable]
    public sealed class FloatAttributeModifier : AttributeModifier<float>
    {
        [Tooltip("The attribute targeted by this modifier.")]
        [SerializeField] private FloatAttributeSelector targetAttribute = new FloatAttributeSelector();

        public override AttributeSelector<float> Attribute => targetAttribute;

        protected override void InitializeValueProvider()
        {
            switch (ValueProviderType)
            {
                case EAttributeModifierValueProviderType.Constant:
                    valueProvider = new FloatAttributeModifierConstantValueProvider();
                    break;
                case EAttributeModifierValueProviderType.Attribute:
                    var provider = new FloatAttributeModifierAttributeValueProvider();
                    provider.Attribute.InitializeForAttributeSetType(AttributeSetTarget);
                    valueProvider = provider;
                    break;
                default:
                    valueProvider = null;
                    break;
            }
        }
    }

    /**
     * An attribute modifier that targets an int attribute.
     */
    [Serializable]
    public sealed class IntAttributeModifier : AttributeModifier<int>
    {
        [Tooltip("The attribute targeted by this modifier.")]
        [SerializeField] private IntAttributeSelector targetAttribute = new IntAttributeSelector();

        public override AttributeSelector<int> Attribute => targetAttribute;
        
        protected override void InitializeValueProvider()
        {
            switch (ValueProviderType)
            {
                case EAttributeModifierValueProviderType.Constant:
                    valueProvider = new IntAttributeModifierConstantValueProvider();
                    break;
                case EAttributeModifierValueProviderType.Attribute:
                    var provider = new IntAttributeModifierAttributeValueProvider();
                    provider.Attribute.InitializeForAttributeSetType(AttributeSetTarget);
                    valueProvider = provider;
                    break;
                default:
                    valueProvider = null;
                    break;
            }
        }

    }
    
#region Attribute Modifier Value Providers

    /**
     * The base class of all value providers that produce the numeric values used by AttributeModifier types to affect
     * AttributeData instances.
     */
    [Serializable]
    public abstract class AttributeModifierValueProviderBase
    {
        /** The value source type used by this value provider. */
        public abstract EAttributeModifierValueProviderType ValueProviderType { get; }
    }
    
    /**
     * The generic typed version of AttributeModifierValueProviderBase. This class provides the basic interface
     * used to get values of type T.
     * <typeparam name="T">The type of value being provided.</typeparam>
     */
    [Serializable]
    public abstract class AttributeModifierValueProvider<T> : AttributeModifierValueProviderBase
    {
        /**
         * Gets the value to apply to the targeted modifier.
         * <param name="target">The attribute set being targeted.</param>
         * <param name="snapshot">The snapshot of current values before applying any modifiers in the set.</param>
         * <returns>The value to assign to the owning modifier.</returns>
         */
        public abstract T GetModifierValue(AttributeSet target, Dictionary<AttributeData<T>, T> snapshot);
    }
    
    
#region Constant Based Value Provider
    
    /**
     * A value provider that returns a constant value of type T.
     * <typeparam name="T">The type of value being provided.</typeparam>
     */
    [Serializable]
    public abstract class AttributeModifierConstantValueProvider<T> : AttributeModifierValueProvider<T>
    {
        [Tooltip("The constant value provided by this value provider.")]
        [SerializeField] private T value;

        public override EAttributeModifierValueProviderType ValueProviderType => EAttributeModifierValueProviderType.Constant;
        
        public override T GetModifierValue(AttributeSet target, Dictionary<AttributeData<T>, T> snapshot) => value;
    }

    /**
     * A value provider that returns a constant value of type float.
     */
    [Serializable]
    public class FloatAttributeModifierConstantValueProvider : AttributeModifierConstantValueProvider<float> {}

    /**
     * A value provider that returns a constant value of type int.
     */
    [Serializable]
    public class IntAttributeModifierConstantValueProvider : AttributeModifierConstantValueProvider<int> {}
    
#endregion
    
#region Attribute Based Value Provider
    
    /**
     * An Attribute Modifier Value Provider that determines its value from another attribute in the targeted set.
     * <typeparam name="T">The type of attribute being targeted.</typeparam>
     */
    [Serializable]
    public abstract class AttributeModifierAttributeValueProvider<T> : AttributeModifierValueProvider<T>
    {

        [Tooltip("If true, the source attribute will have its value taken before any modifiers are applied. Otherwise, its value at application time will be used.")]
        [SerializeField] private bool shouldSnapshot = true;

        /** The selector used to choose the attribute that this provider draws its value from. */
        public abstract AttributeSelector<T> Attribute { get; }

        public override EAttributeModifierValueProviderType ValueProviderType => EAttributeModifierValueProviderType.Attribute;

        public override T GetModifierValue(AttributeSet target, Dictionary<AttributeData<T>, T> snapshot)
        {
            AttributeData<T> attribute = Attribute.GetSelectedAttribute(target);
            T result = attribute.CurrentValue;
            if (shouldSnapshot) snapshot.TryGetValue(attribute, out result);
            return result;
        }
    }

    /**
     * An Attribute Modifier Value Provider that determines its value from another attribute in the targeted set.
     * This class is the concrete version of AttributeModifierAttributeValueProvider for floats.
     */
    [Serializable]
    public class FloatAttributeModifierAttributeValueProvider : AttributeModifierAttributeValueProvider<float>
    {
        [Tooltip("The attribute to get the value of.")]
        [SerializeField] private FloatAttributeSelector attribute = new FloatAttributeSelector();

        public override AttributeSelector<float> Attribute => attribute;
    }

    /**
     * An Attribute Modifier Value Provider that determines its value from another attribute in the targeted set.
     * This class is the concrete version of AttributeModifierAttributeValueProvider for ints.
     */
    [Serializable]
    public class IntAttributeModifierAttributeValueProvider : AttributeModifierAttributeValueProvider<int>
    {
        [Tooltip("The attribute to get the value of.")]
        [SerializeField] private IntAttributeSelector attribute = new IntAttributeSelector();

        public override AttributeSelector<int> Attribute => attribute;
    }
    
#endregion

#endregion
    
}