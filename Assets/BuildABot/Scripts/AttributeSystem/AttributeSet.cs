using System;
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
         * Initializes a new AttributeSet object.
         */
        protected AttributeSet()
        {
            // Get all AttributeData<T> derived fields of this set
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Check each field
            for (int i = 0; i < fields.Length; i++)
            {
                // Bind the change events to the field if it is attribute data
                switch (fields[i].GetValue(this))
                {
                    case AttributeData<float> attribute:
                        BindAttributeChangeEvents(attribute);
                        break;
                    case AttributeData<int> attribute:
                        BindAttributeChangeEvents(attribute);
                        break;
                    default:
                        Debug.LogWarningFormat("AttributeSet types should not contain fields that are not derived from AttributeData<T>: {0}", fields[i].Name);
                        break;
                }
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
    }
}