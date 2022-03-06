using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * An AttributeSet is a collection of related AttributeData derived fields that are all common to whatever object
     * owns this set. A common example is the attributes of the player character or an NPC.
     * This class is meant to be derived from to create a concrete collection of attributes.
     */
    [Serializable]
    public abstract class AttributeSet
    {

        private HashSet<Effect> _activeEffects = new HashSet<Effect>();

        /** A runtime map of all attribute names to their references in this set. */
        private Dictionary<string, AttributeDataBase> _attributes = new Dictionary<string, AttributeDataBase>();

        /** The names of the attributes held by this set. */
        public IReadOnlyCollection<string> AttributeNames => _attributes.Keys;

        /**
         * Initializes a new AttributeSet object.
         */
        protected AttributeSet()
        {
            // Get all AttributeData<T> derived fields of this set
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Check each field
            foreach (FieldInfo field in fields)
            {
                // Bind the change events to the field if it is attribute data
                if (field.FieldType == typeof(FloatAttributeData))
                {
                    FloatAttributeData attribute = field.GetValue(this) as FloatAttributeData;
                    BindAttributeChangeEvents(attribute);
                    _attributes.Add(field.Name, attribute);
                }
                else if (field.FieldType == typeof(IntAttributeData))
                {
                    IntAttributeData attribute = field.GetValue(this) as IntAttributeData;
                    BindAttributeChangeEvents(attribute);
                    _attributes.Add(field.Name, attribute);
                }
                else
                {
                    Debug.LogWarningFormat("AttributeSet types should not contain fields that are not derived from AttributeData<T>: {0}", field.Name);
                }
            }
        }

        /**
         * Initializes this attribute set and its attributes for runtime.
         */
        public void Initialize()
        {
            foreach (var entry in _attributes)
            {
                entry.Value.Initialize(this);
            }
        }

        /**
         * Gets the attribute data associated with the provided name and data type. If an attribute with the
         * provided name is not found or the type is invalid null will be returned.
         * <typeparam name="T">The expected type of data stored in the attribute, generally float or int.</typeparam>
         * <param name="name">The name of the attribute to get.</param>
         * <returns>The attribute data if found, otherwise null.</returns>
         */
        public AttributeData<T> GetAttributeData<T>(string name)
        {
            bool found = _attributes.TryGetValue(name, out AttributeDataBase attribute);
            if (found) return attribute as AttributeData<T>;

            return null;
        }

        /**
         * Binds the change event delegate functions to an attribute data instance.
         * <typeparam name="T">The type of attribute data being bound, generally float or int.</typeparam>
         * <param name="attribute">The attribute to bind to.</param>
         */
        private void BindAttributeChangeEvents<T>(AttributeData<T> attribute)
        {
            if (null == attribute) return;
            attribute.AddPreValueChangeListener((value) => PreAttributeChange(attribute, value));
            attribute.AddPostValueChangeListener((value) => PostAttributeChange(attribute, value));
            attribute.AddPreBaseValueChangeListener((value) => PreAttributeBaseChange(attribute, value));
            attribute.AddPostBaseValueChangeListener((value) => PostAttributeBaseChange(attribute, value));
        }
        
        /**
         * Called before an attribute in this set has its value changed.
         * <param name="attribute">The attribute data being modified.</param>
         * <param name="newValue">The new value being assigned to the attribute.</param>
         */
        protected virtual void PreAttributeChange<T>(AttributeData<T> attribute, T newValue)
        {
            
        }

        /**
         * Called before an attribute in this set has its base value changed.
         * <param name="attribute">The attribute data being modified.</param>
         * <param name="newValue">The new value being assigned to the attribute.</param>
         */
        protected virtual void PreAttributeBaseChange<T>(AttributeData<T> attribute, T newValue)
        {
            
        }
        
        /**
         * Called after an attribute in this set has its value changed.
         * <param name="attribute">The attribute data being modified.</param>
         * <param name="newValue">The new base value being assigned to the attribute.</param>
         */
        protected virtual void PostAttributeChange<T>(AttributeData<T> attribute, T newValue)
        {
            
        }

        /**
         * Called after an attribute in this set has its base value changed.
         * <param name="attribute">The attribute data being modified.</param>
         * <param name="newValue">The new base value being assigned to the attribute.</param>
         */
        protected virtual void PostAttributeBaseChange<T>(AttributeData<T> attribute, T newValue)
        {
            
        }

        /**
         * A callback used to remove a duration based effect after its time has elapsed.
         * <param name="effect">The effect to remove.</param>
         */
        public void RemoveEffect(Effect effect)
        {
            _activeEffects.Remove(effect);
        }

        /**
         * Applies the given effect to this AttributeSet.
         * <param name="effect">The effect to apply.</param>
         * <param name="context">The context used for duration based effect removal.</param>
         */
        public void ApplyEffect(Effect effect, MonoBehaviour context)
        {
            Dictionary<AttributeData<float>, float> floatCurrentSnapshot = new Dictionary<AttributeData<float>, float>();
            Dictionary<AttributeData<int>, int> intCurrentSnapshot = new Dictionary<AttributeData<int>, int>();
            
            foreach (var entry in this._attributes)
            {
                if (entry.Value is AttributeData<float> floatAttribute) floatCurrentSnapshot.Add(floatAttribute, floatAttribute.CurrentValue);
                else if (entry.Value is AttributeData<int> intAttribute) intCurrentSnapshot.Add(intAttribute, intAttribute.CurrentValue);
            }

            if (effect.DurationMode == EEffectDurationMode.ForDuration)
            {
                _activeEffects.Add(effect);
                Utility.DelayedFunction(context, effect.Duration, () => RemoveEffect(effect));
            }
            
            foreach (var entry in this._attributes)
            {
                if (entry.Value is AttributeData<float> floatAttribute) CalculateModifierStack(floatAttribute, effect, floatCurrentSnapshot);
                else if (entry.Value is AttributeData<int> intAttribute) CalculateModifierStack(intAttribute, effect, intCurrentSnapshot);
            }
        }

        /**
         * Calculates and applies the modifier stack to the provided attribute from the given effect.
         * <param name="attribute">The attribute to modify.</param>
         * <param name="effect">The effect to gather the modifiers from.</param>
         * <param name="snapshot">The snapshot of all current values in this set before applying this stack.</param>
         */
        private void CalculateModifierStack<T>(AttributeData<T> attribute, Effect effect, Dictionary<AttributeData<T>, T> snapshot)
        {
            List<AttributeModifier<T>> mods = new List<AttributeModifier<T>>();
            foreach (AttributeModifierBase mod in effect.Modifiers)
            {
                // Gather the mods that apply to the current attribute
                if (mod is AttributeModifier<T> typedMod && typedMod.Attribute.GetSelectedAttribute(this) == attribute)
                {
                    mods.Add(typedMod);
                }
            }
            attribute.ApplyModifiers(mods, effect.DurationMode == EEffectDurationMode.Instant, snapshot);
        }
    }
}