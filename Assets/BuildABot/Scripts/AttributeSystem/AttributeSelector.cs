using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * A utility class used to select an attribute from an AttributeSet by name.
     */
    [Serializable]
    public abstract class AttributeSelector<T>
    {

        /** The underlying value of this selector. */
        [SerializeField] private string value = "";
        /** The options available to this selector. */
        [SerializeField] private string[] options;
        /** Is this selector valid? */
        [SerializeField] private bool valid;

        /** The underlying data type of the attributes targeted by this selector. */
        public Type DataType => typeof(T);

        /** The name of the attribute targeted by this selector. May be empty. */
        public string SelectedAttributeName => value;

        /** The friendly name of the selected attribute. */
        public string SelectedAttributeNameFriendly => Utility.NicifyVariableName(SelectedAttributeName);
        
        protected AttributeSelector()
        {
            RefreshOptions(null);
        }

        /**
         * Initializes this selector to target the provided attribute set.
         * <param name="target">The attribute set to target.</param>
         */
        public void InitializeForAttributeSet(AttributeSet target)
        {
            InitializeForAttributeSetType(target.GetType());
        }

        /**
         * Initializes this selector to target the provided attribute set type.
         * <param name="targetType">The attribute set type to target.</param>
         */
        public void InitializeForAttributeSetType(Type targetType)
        {
            RefreshOptions(targetType);
        }

        /**
         * Gets the attribute value selected by this object given the owning attribute set to select from.
         * <param name="source">The set to get the attribute from based on this selector.</param>
         * <returns>The attribute chosen by this selector from the provided attribute set.</returns>
         */
        public AttributeData<T> GetSelectedAttribute(AttributeSet source)
        {
            return valid ? source.GetAttributeData<T>(value) : null;
        }

        /**
         * Refreshes the options available to this selector.
         * <param name="attributeSetType">The new attribute set type to target.</param>
         */
        private void RefreshOptions(Type attributeSetType)
        {
            if (null != attributeSetType && attributeSetType.IsSubclassOf(typeof(AttributeSet)) && attributeSetType != typeof(AttributeSet))
            {

                List<string> choices = new List<string>();

                Type target = attributeSetType;
                do
                {
                    
                    // Get all AttributeDataBase derived fields of the attribute set type
                    FieldInfo[] fields = target.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .OrderBy(field => field.Name).ToArray(); // Sort by name to ensure order
            
                    // Check each field
                    foreach (FieldInfo field in fields)
                    {
                        if (field.FieldType.IsSubclassOf(typeof(AttributeData<T>)))
                        {
                            choices.Add(field.Name);
                        }
                    }
                
                    target = target.BaseType;
                } while (target != typeof(AttributeSet) && null != target);

                options = choices.ToArray();
                valid = true;
            }
            else
            {
                options = new [] { "-" };
                valid = false;
            }
        }
    }
    
    /**
     * Used to select a float based attribute from an AttributeSet.
     */
    [Serializable]
    public sealed class FloatAttributeSelector : AttributeSelector<float> {}
    
    /**
     * Used to select an int based attribute from an AttributeSet.
     */
    [Serializable]
    public sealed class IntAttributeSelector : AttributeSelector<int> {}

    
    /**
     * A utility class used to select an AttributeSet type.
     */
    [Serializable]
    public class AttributeSetSelector
    {
        /** The underlying value of this selector. */
        [SerializeField] private string value = "";

        /** Gets the type selected by this object. */
        public Type SelectedType => Type.GetType(value);

        public AttributeSetSelector()
        {
            
        }

        /**
         * Constructs a new AttributeSetSelector set to target the provided AttributeSet type.
         * <param name="attributeSetType">The type of the AttributeSet class to target.</param>
         */
        public AttributeSetSelector(Type attributeSetType)
        {
            if (!attributeSetType.IsSubclassOf(typeof(AttributeSet)))
                throw new ArgumentException(
                    "AttributeSetSelector objects can only contain references to AttributeSet types.",
                    nameof(attributeSetType));
            value = attributeSetType.AssemblyQualifiedName;
        }

        /**
         * Constructs a new AttributeSetSelector from a given value.
         * <param name="value">The underlying value to generate from. This should be an Assembly-Qualified type name.</param>
         * <returns>The newly created selector.</returns>
         */
        public static AttributeSetSelector FromValue(string value)
        {
            AttributeSetSelector result = new AttributeSetSelector {value = value};
            return result;
        }

        /**
         * Constructs a new AttributeSetSelector from a compile time type.
         * <typeparam name="T">The AttributeSet derived type to target.</typeparam>
         * <returns>The newly created selector.</returns>
         */
        public static AttributeSetSelector FromType<T>() where T : AttributeSet
        {
            return new AttributeSetSelector(typeof(T));
        }

    #if UNITY_EDITOR
        
        public static string[] Options { get; private set; }
        public static string[] NiceOptions { get; private set; }
        
        [InitializeOnLoadMethod]
        public static void FindAllOptions()
        {
            // Get all types derived from AttributeSet
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(AttributeSet)))
                .ToList();
            
            // Sort by name
            types.Sort((a, b) => string.CompareOrdinal(a.FullName, b.FullName));

            NiceOptions = new string[types.Count];
            Options = new string[types.Count];

            // Populate options
            for (int i = 0; i < types.Count; i++)
            {
                NiceOptions[i] = ObjectNames.NicifyVariableName(types[i].Name);
                Options[i] = types[i].AssemblyQualifiedName;
            }
        }
    #endif
    }
}