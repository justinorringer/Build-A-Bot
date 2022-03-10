using System;
using System.Collections;
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

        /**
         * A specific modifier applied to a set.
         */
        private struct AppliedModifier<T>
        {
            /** The applied modifier. */
            public AttributeModifier<T> Modifier;
            /** The value applied by the modifier. */
            public T Value;
            /** The attribute targeted by the modifier. */
            public AttributeData<T> Target;
        }
        
        /**
         * The data associated with a single applied effect instance.
         */
        private class AppliedEffect
        {
            /** The effect that was applied. */
            public Effect Effect;
            /** The magnitude the effect was applied with. */
            public float AppliedMagnitude;
            /** The timer coroutine handle used if this effect had a duration. */
            public IEnumerator Timer;
            /** The timer context that was used when applying this effect. */
            public MonoBehaviour TimerContext;
            /** The float modifiers associated with this effect. */
            public List<LinkedListNode<AppliedModifier<float>>> FloatModifiers;
            /** The int modifiers associated with this effect. */
            public List<LinkedListNode<AppliedModifier<int>>> IntModifiers;
        }

        /** All currently applied float modifiers mapped to their associated attribute. */
        private Dictionary<AttributeData<float>, LinkedList<AppliedModifier<float>>> _appliedFloatModifiers = new Dictionary<AttributeData<float>, LinkedList<AppliedModifier<float>>>();
        /** All currently applied int modifiers mapped to their associated attribute. */
        private Dictionary<AttributeData<int>, LinkedList<AppliedModifier<int>>> _appliedIntModifiers = new Dictionary<AttributeData<int>, LinkedList<AppliedModifier<int>>>();

        /** Each effect mapped to a list (in order by application time) of the collections of modifiers applied by each effect instance */
        private Dictionary<Effect, List<AppliedEffect>> _appliedEffects = new Dictionary<Effect, List<AppliedEffect>>();

        /** A runtime map of all attribute names to their references in this set. */
        private Dictionary<string, AttributeDataBase> _attributes = new Dictionary<string, AttributeDataBase>();

        /** The names of the attributes held by this set. */
        public IReadOnlyCollection<string> AttributeNames => _attributes.Keys;
        
#region Startup and Initialization

        /**
         * Initializes a new AttributeSet object.
         */
        protected AttributeSet()
        {
            GatherAttributeData();
        }

        protected void GatherAttributeData()
        {
            // Walk up the inheritance hierarchy
            Type target = GetType();
            do
            {
                // Get all AttributeData<T> derived fields of this set
                FieldInfo[] fields = target.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
                // Check each field
                foreach (FieldInfo field in fields)
                {
                    // Bind the change events to the field if it is attribute data
                    if (field.FieldType == typeof(FloatAttributeData))
                    {
                        FloatAttributeData attribute = field.GetValue(this) as FloatAttributeData;
                        if (attribute == null) continue;
                        BindAttributeChangeEvents(attribute);
                        _attributes.Add(field.Name, attribute);
                        _appliedFloatModifiers.Add(attribute, new LinkedList<AppliedModifier<float>>());
                    }
                    else if (field.FieldType == typeof(IntAttributeData))
                    {
                        IntAttributeData attribute = field.GetValue(this) as IntAttributeData;
                        if (attribute == null) continue;
                        BindAttributeChangeEvents(attribute);
                        _attributes.Add(field.Name, attribute);
                        _appliedIntModifiers.Add(attribute, new LinkedList<AppliedModifier<int>>());
                    }
                    else
                    {
                        Debug.LogWarningFormat("AttributeSet types should not contain fields that are not derived from AttributeData<T>: {0}", field.Name);
                    }
                }
                
                target = target.BaseType;
            } while (target != typeof(AttributeSet) && null != target);
        }

        /**
         * Initializes this attribute set and its attributes for runtime.
         */
        public void Initialize()
        {
            GatherAttributeData();
            foreach (var entry in _attributes)
            {
                entry.Value.Initialize(this);
            }
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
        
#endregion
        
#region Utilities

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

#endregion
        
#region Change Events
        
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
        
#endregion

#region Effect Handling

        /**
         * Removes the oldest applied instance of the provided effect from this set.
         * Returns true if other instances of this effect remain.
         * <param name="effect">The effect to remove.</param>
         * <param name="recalculateModifiers">Should the modifier stacks associate with the removed effect be recalculated?</param>
         * <returns>True if this set still has instances of the provided effect after removing one.</returns>
         */
        public bool RemoveEffect(Effect effect, bool recalculateModifiers = true)
        {
            
            // TODO: Add a way to request that a specific effect instance is removed using some kind of handle
            
            if (_appliedEffects.TryGetValue(effect, out List<AppliedEffect> list))
            {
                if (list.Count == 0) return false; // No active effects

                AppliedEffect target = list[0];
                
                if (target.Timer != null) target.TimerContext.StopCoroutine(target.Timer); // Stop the removal timer if appropriate

                HashSet<AttributeDataBase> dirtyAttributes = new HashSet<AttributeDataBase>();
                
                // Remove all modifiers applied by the oldest effect instance
                foreach (LinkedListNode<AppliedModifier<float>> entry in target.FloatModifiers)
                {
                    if (_appliedFloatModifiers.TryGetValue(entry.Value.Target, out LinkedList<AppliedModifier<float>> modifiers))
                    {
                        dirtyAttributes.Add(entry.Value.Target);
                        modifiers.Remove(entry); // Remove the modifier
                    }
                }
                foreach (LinkedListNode<AppliedModifier<int>> entry in target.IntModifiers)
                {
                    if (_appliedIntModifiers.TryGetValue(entry.Value.Target, out LinkedList<AppliedModifier<int>> modifiers))
                    {
                        dirtyAttributes.Add(entry.Value.Target);
                        modifiers.Remove(entry); // Remove the modifier
                    }
                }
                
                // Remove the first entry
                list.RemoveAt(0);

                if (recalculateModifiers)
                {
                    // Recalculate the dirty attributes stacks
                    foreach (AttributeDataBase attribute in dirtyAttributes)
                    {
                        if (attribute is FloatAttributeData floatAttribute)
                        {
                            RecalculateFloatModifierStack(floatAttribute, out float addValue, out float multiplyValue, out float divideValue, out float replacement);
                            floatAttribute.CurrentValue = ((replacement + addValue) * multiplyValue) / divideValue;
                        }
                        else if (attribute is IntAttributeData intAttribute)
                        {
                            RecalculateIntModifierStack(intAttribute, out int addValue, out int multiplyValue, out int divideValue, out int replacement);
                            intAttribute.CurrentValue = ((replacement + addValue) * multiplyValue) / divideValue;
                        }
                    }
                }
                
                return list.Count > 0; // Check if any more entries remain
            }
            
            return false;
        }

        /**
         * Applies the given effect to this AttributeSet.
         * <param name="effect">The effect to apply.</param>
         * <param name="context">The context used for duration based effect removal. Only used with ForDuration effects.</param>
         * <param name="magnitude">The optional magnitude multiplier to apply to the provided effect.</param>
         */
        public void ApplyEffect(Effect effect, MonoBehaviour context, float magnitude = 1.0f)
        {
            Dictionary<AttributeData<float>, float> floatCurrentSnapshot = new Dictionary<AttributeData<float>, float>();
            Dictionary<AttributeData<int>, int> intCurrentSnapshot = new Dictionary<AttributeData<int>, int>();
            
            // Take a snapshot of all current attribute values
            foreach (var entry in _attributes)
            {
                if (entry.Value is AttributeData<float> floatAttribute) floatCurrentSnapshot.Add(floatAttribute, floatAttribute.CurrentValue);
                else if (entry.Value is AttributeData<int> intAttribute) intCurrentSnapshot.Add(intAttribute, intAttribute.CurrentValue);
            }

            // Prepare the data for this applied attribute
            AppliedEffect appliedEffect = new AppliedEffect()
            {
                Effect = effect, AppliedMagnitude = magnitude, Timer = null, TimerContext = null,
                FloatModifiers = new List<LinkedListNode<AppliedModifier<float>>>(),
                IntModifiers = new List<LinkedListNode<AppliedModifier<int>>>()
            };

            // Ensure that non-instant effects have an entry
            if (effect.DurationMode != EEffectDurationMode.Instant)
            {
                // Store the list of newly applied mods in the lookup table
                if (!_appliedEffects.TryGetValue(effect, out List<AppliedEffect> appliedEffects))
                {
                    appliedEffects = new List<AppliedEffect>();
                    _appliedEffects.Add(effect, appliedEffects);
                }
                
                // Add the effect data for this call to ApplyEffect
                // How this data is added will depend on the stacking behavior
                
                // Handle stacking behaviors
                switch (effect.StackingMode)
                {
                    case EEffectStackingMode.Combine:
                        // Stack the effects
                        appliedEffects.Add(appliedEffect);
                        break;
                    case EEffectStackingMode.Replace:
                        // Clear the list of instances and add the new effect
                        appliedEffects.Clear();
                        appliedEffects.Add(appliedEffect);
                        break;
                    case EEffectStackingMode.Reset:
                        // Reset the timer on the oldest instance of the effect if one exists, otherwise add
                        if (appliedEffects.Count == 0) appliedEffects.Add(appliedEffect);
                        else
                        {
                            AppliedEffect oldest = appliedEffects[0];
                            if (oldest.Effect.DurationMode == EEffectDurationMode.ForDuration)
                            {
                                // Reset the timer if the old effect is ForDuration
                                oldest.TimerContext.StopCoroutine(oldest.Timer);
                                oldest.Timer = Utility.DelayedFunction(oldest.TimerContext, oldest.Effect.Duration,
                                    () => RemoveEffect(oldest.Effect));
                            }
                        }
                        break;
                    case EEffectStackingMode.KeepOriginal:
                        // Only add the new effect if no other instances exist for the same effect
                        if (appliedEffects.Count == 0) appliedEffects.Add(appliedEffect);
                        break;
                    case EEffectStackingMode.KeepHighestMagnitude:
                        // Select the effect instance with the highest magnitude
                        // If equal in magnitude, the newest is chosen
                        if (appliedEffects.Count == 0) appliedEffects.Add(appliedEffect);
                        else
                        {
                            if (appliedEffects[0].AppliedMagnitude <= magnitude)
                            {
                                appliedEffects.Clear();
                                appliedEffects.Add(appliedEffect);
                            }
                        }
                        break;
                }
            }
            
            // Calculate the full modifier stack for each attribute
            foreach (var entry in _attributes)
            {
                if (entry.Value is AttributeData<float> floatAttribute) CalculateFloatModifierStack(floatAttribute, appliedEffect, floatCurrentSnapshot, magnitude);
                else if (entry.Value is AttributeData<int> intAttribute) CalculateIntModifierStack(intAttribute, appliedEffect, intCurrentSnapshot, magnitude);
            }

            // Apply a timer if the effect is ForDuration
            if (effect.DurationMode == EEffectDurationMode.ForDuration)
            {
                appliedEffect.TimerContext = context;
                appliedEffect.Timer = Utility.DelayedFunction(context, effect.Duration, () => RemoveEffect(effect));
                // TODO: remove directly using an exact reference to this effects modifier list to avoid incorrect removal order bug
                // TODO: Return a handle to this specific effect for more exact removal
            }
        }
        
#endregion

#region Modifier Stack Calculations
        
        /**
         * Calculates and applies the modifier stack to the provided float attribute from the given effect.
         * <param name="attribute">The attribute to modify.</param>
         * <param name="appliedEffect">The applied effect data to gather the modifiers from.</param>
         * <param name="snapshot">The snapshot of all current values in this set before applying this stack.</param>
         * <param name="magnitude">The magnitude to apply to the effect.</param>
         */
        private void CalculateFloatModifierStack(AttributeData<float> attribute, AppliedEffect appliedEffect, Dictionary<AttributeData<float>, float> snapshot, float magnitude)
        {
            
            // Calculate the base modifier stack
            RecalculateFloatModifierStack(attribute, out float tempAdd, out float tempMultiply, out float tempDivide, out float latestCurrentReplacement);
            
            // Handle new effect

            bool isPermanent = appliedEffect.Effect.DurationMode == EEffectDurationMode.Instant;
            
            float baseValue = attribute.BaseValue;

            float baseAdd = 0;
            float baseMultiply = 1;
            float baseDivide = 1;

            float latestBaseReplacement = baseValue;

            // Apply modifiers from new effect
            if (isPermanent)
            {
                // Permanent change to base value
                
                foreach (AttributeModifierBase mod in appliedEffect.Effect.Modifiers)
                {
                    // Gather the mods that apply to the current attribute
                    if (mod is AttributeModifier<float> modifier && modifier.Attribute.GetSelectedAttribute(this) == attribute)
                    {
                        float value = modifier.GetModifierValue(this, snapshot) * magnitude;
                
                        switch (modifier.OperationType)
                        {
                            case EAttributeModifierOperationType.Add:
                                baseAdd += value;
                                break;
                            case EAttributeModifierOperationType.Multiply:
                                baseMultiply += value - 1;
                                break;
                            case EAttributeModifierOperationType.Divide:
                                baseDivide += value - 1;
                                break;
                            case EAttributeModifierOperationType.Replace:
                                // Clear any mods to current value already applied
                                baseAdd = 0;
                                baseMultiply = 1;
                                baseDivide = 1;
                                // Update the replacement cache
                                latestBaseReplacement = value;
                                break;
                        }
                    }
                }
            }
            else
            {
                List<LinkedListNode<AppliedModifier<float>>> mods = appliedEffect.FloatModifiers;
                
                foreach (AttributeModifierBase mod in appliedEffect.Effect.Modifiers)
                {
                    // Gather the mods that apply to the current attribute
                    if (mod is AttributeModifier<float> modifier && modifier.Attribute.GetSelectedAttribute(this) == attribute)
                    {
                        float value = modifier.GetModifierValue(this, snapshot) * magnitude;

                        // Cache the mod in the mod list
                        AppliedModifier<float> am = new AppliedModifier<float>
                        {
                            Modifier = modifier, Value = value, Target = attribute
                        };
                        LinkedListNode<AppliedModifier<float>> m = new LinkedListNode<AppliedModifier<float>>(am);
                        _appliedFloatModifiers[attribute].AddLast(m);
                        mods.Add(m);
                
                        switch (modifier.OperationType)
                        {
                            case EAttributeModifierOperationType.Add:
                                tempAdd += value;
                                break;
                            case EAttributeModifierOperationType.Multiply:
                                tempMultiply += value - 1;
                                break;
                            case EAttributeModifierOperationType.Divide:
                                tempDivide += value - 1;
                                break;
                            case EAttributeModifierOperationType.Replace:
                                // Clear any mods to current value already applied
                                tempAdd = 0;
                                tempMultiply = 1;
                                tempDivide = 1;
                                // Update the replacement cache
                                latestCurrentReplacement = value;
                                break;
                        }
                    }
                }
            }
            
            // Update the values

            attribute.BaseValue = ((latestBaseReplacement + baseAdd) * baseMultiply) / baseDivide;
            attribute.CurrentValue = ((latestCurrentReplacement + tempAdd) * tempMultiply) / tempDivide;
        }
        
        /**
         * Calculates and applies the modifier stack to the provided int attribute from the given effect.
         * <param name="attribute">The attribute to modify.</param>
         * <param name="appliedEffect">The applied effect data to gather the modifiers from.</param>
         * <param name="snapshot">The snapshot of all current values in this set before applying this stack.</param>
         * <param name="magnitude">The magnitude to apply to the effect.</param>
         */
        private void CalculateIntModifierStack(AttributeData<int> attribute, AppliedEffect appliedEffect, Dictionary<AttributeData<int>, int> snapshot, float magnitude)
        {
            // Calculate the base modifier stack
            RecalculateIntModifierStack(attribute, out int tempAdd, out int tempMultiply, out int tempDivide, out int latestCurrentReplacement);
            
            // Handle new effect

            bool isPermanent = appliedEffect.Effect.DurationMode == EEffectDurationMode.Instant;
            
            int baseValue = attribute.BaseValue;

            int baseAdd = 0;
            int baseMultiply = 1;
            int baseDivide = 1;

            int latestBaseReplacement = baseValue;

            // Apply modifiers from new effect
            if (isPermanent)
            {
                // Permanent change to base value
                
                foreach (AttributeModifierBase mod in appliedEffect.Effect.Modifiers)
                {
                    // Gather the mods that apply to the current attribute
                    if (mod is AttributeModifier<int> modifier && modifier.Attribute.GetSelectedAttribute(this) == attribute)
                    {
                        int value = (int)(modifier.GetModifierValue(this, snapshot) * magnitude);
                
                        switch (modifier.OperationType)
                        {
                            case EAttributeModifierOperationType.Add:
                                baseAdd += value;
                                break;
                            case EAttributeModifierOperationType.Multiply:
                                baseMultiply += value - 1;
                                break;
                            case EAttributeModifierOperationType.Divide:
                                baseDivide += value - 1;
                                break;
                            case EAttributeModifierOperationType.Replace:
                                // Clear any mods to current value already applied
                                baseAdd = 0;
                                baseMultiply = 1;
                                baseDivide = 1;
                                // Update the replacement cache
                                latestBaseReplacement = value;
                                break;
                        }
                    }
                }
            }
            else
            {
                List<LinkedListNode<AppliedModifier<int>>> mods = appliedEffect.IntModifiers;
                
                foreach (AttributeModifierBase mod in appliedEffect.Effect.Modifiers)
                {
                    // Gather the mods that apply to the current attribute
                    if (mod is AttributeModifier<int> modifier && modifier.Attribute.GetSelectedAttribute(this) == attribute)
                    {
                        int value = (int)(modifier.GetModifierValue(this, snapshot) * magnitude);

                        // Cache the mod in the mod list
                        AppliedModifier<int> am = new AppliedModifier<int>
                        {
                            Modifier = modifier, Value = value, Target = attribute
                        };
                        LinkedListNode<AppliedModifier<int>> m = new LinkedListNode<AppliedModifier<int>>(am);
                        _appliedIntModifiers[attribute].AddLast(m);
                        mods.Add(m);
                
                        switch (modifier.OperationType)
                        {
                            case EAttributeModifierOperationType.Add:
                                tempAdd += value;
                                break;
                            case EAttributeModifierOperationType.Multiply:
                                tempMultiply += value - 1;
                                break;
                            case EAttributeModifierOperationType.Divide:
                                tempDivide += value - 1;
                                break;
                            case EAttributeModifierOperationType.Replace:
                                // Clear any mods to current value already applied
                                tempAdd = 0;
                                tempMultiply = 1;
                                tempDivide = 1;
                                // Update the replacement cache
                                latestCurrentReplacement = value;
                                break;
                        }
                    }
                }
            }
            
            // Update the values

            attribute.BaseValue = ((latestBaseReplacement + baseAdd) * baseMultiply) / baseDivide;
            attribute.CurrentValue = ((latestCurrentReplacement + tempAdd) * tempMultiply) / tempDivide;
        }

        /**
         * Recalculates the modifier stack values for the provided attribute. These changes must still be applied.
         * <param name="attribute">The attribute to recalculate.</param>
         * <param name="addValue">The additive term of the attribute value.</param>
         * <param name="multiplyValue">The multiplier term of the attribute value.</param>
         * <param name="divideValue">The divisor term of the attribute value.</param>
         * <param name="replacement">The replacement term of the attribute value.</param>
         */
        private void RecalculateFloatModifierStack(AttributeData<float> attribute, out float addValue, out float multiplyValue,
            out float divideValue, out float replacement)
        {
            float baseValue = attribute.BaseValue;
            float currentValue = baseValue;

            addValue = 0;
            multiplyValue = 1;
            divideValue = 1;

            replacement = currentValue;
            
            foreach (AppliedModifier<float> appliedMod in _appliedFloatModifiers[attribute])
            {
                float value = appliedMod.Value;
            
                switch (appliedMod.Modifier.OperationType)
                {
                    case EAttributeModifierOperationType.Add:
                        addValue += value;
                        break;
                    case EAttributeModifierOperationType.Multiply:
                        multiplyValue += value - 1;
                        break;
                    case EAttributeModifierOperationType.Divide:
                        divideValue += value - 1;
                        break;
                    case EAttributeModifierOperationType.Replace:
                        // Clear any mods to current value already applied
                        addValue = 0;
                        multiplyValue = 1;
                        divideValue = 1;
                        // Update the replacement cache
                        replacement = value;
                        break;
                }
            }
        }
        
        /**
         * Recalculates the modifier stack values for the provided attribute. These changes must still be applied.
         * <param name="attribute">The attribute to recalculate.</param>
         * <param name="addValue">The additive term of the attribute value.</param>
         * <param name="multiplyValue">The multiplier term of the attribute value.</param>
         * <param name="divideValue">The divisor term of the attribute value.</param>
         * <param name="replacement">The replacement term of the attribute value.</param>
         */
        private void RecalculateIntModifierStack(AttributeData<int> attribute, out int addValue, out int multiplyValue,
            out int divideValue, out int replacement)
        {
            int baseValue = attribute.BaseValue;
            int currentValue = baseValue;

            addValue = 0;
            multiplyValue = 1;
            divideValue = 1;

            replacement = currentValue;
            
            foreach (AppliedModifier<int> appliedMod in _appliedIntModifiers[attribute])
            {
                int value = appliedMod.Value;
            
                switch (appliedMod.Modifier.OperationType)
                {
                    case EAttributeModifierOperationType.Add:
                        addValue += value;
                        break;
                    case EAttributeModifierOperationType.Multiply:
                        multiplyValue += value - 1;
                        break;
                    case EAttributeModifierOperationType.Divide:
                        divideValue += value - 1;
                        break;
                    case EAttributeModifierOperationType.Replace:
                        // Clear any mods to current value already applied
                        addValue = 0;
                        multiplyValue = 1;
                        divideValue = 1;
                        // Update the replacement cache
                        replacement = value;
                        break;
                }
            }
        }
        
#endregion
        
#region Object Overrides

        public override string ToString()
        {
            string entries = "";
            foreach (var entry in _attributes)
            {
                entries += $"{entry.Key}: []\n";
            }
            return $"Attribute Set {GetType().Name}: [\n{entries}]";
        }

#endregion

    }
}