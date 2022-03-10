using System;
using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * The base class for custom property drawers for attribute modifiers.
     */
    public abstract class AttributeModifierDrawer : PropertyDrawer
    {
        
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty targetAttributeProperty = property.FindPropertyRelative("targetAttribute");
            SerializedProperty operationTypeProperty = property.FindPropertyRelative("operationType");
            SerializedProperty valueProviderTypeProperty = property.FindPropertyRelative("valueProviderType");
            SerializedProperty valueProviderProperty = property.FindPropertyRelative("valueProvider");

            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            int lines = 1;
            int spaces = 0;
            
            if (property.isExpanded)
            {

                Rect pos = CalculatePosition(position, ref lines, spaces);
                EditorGUI.PropertyField(pos, targetAttributeProperty, true);

                spaces++;
            
                pos = CalculatePosition(position, ref lines, spaces);
                EditorGUI.PropertyField(pos, operationTypeProperty, true);
            
                EditorGUI.BeginChangeCheck();

                pos = CalculatePosition(position, ref lines, spaces);
                EditorGUI.PropertyField(pos, valueProviderTypeProperty, new GUIContent(" "), true);
                if (EditorGUI.EndChangeCheck())
                {
                    // Update the value provider to the newly selected type
                    Enum.TryParse(valueProviderTypeProperty.enumNames[valueProviderTypeProperty.enumValueIndex],
                        out EAttributeModifierValueProviderType providerType);

                    SerializedProperty attributeSetValueProperty = property.FindPropertyRelative("attributeSet").FindPropertyRelative("value");

                    AttributeSetSelector selector = AttributeSetSelector.FromValue(attributeSetValueProperty.stringValue);

                    valueProviderProperty.managedReferenceValue = CreateValueProvider(providerType, selector.SelectedType);
                }
                
                EditorGUI.PropertyField(pos, valueProviderProperty, true);
            }
            
            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3 +
                                         EditorGUI.GetPropertyHeight(property.FindPropertyRelative("valueProvider")) +
                                         (EditorGUIUtility.singleLineHeight / 2) * 2 :
                EditorGUIUtility.singleLineHeight;
        }

        /**
         * Calculates the position rect of a single line field given the root position, line number, and current spaces.
         * <param name="position">The root position.</param>
         * <param name="lines">The line number to get the rect for.</param>
         * <param name="spaces">The number of spaces currently added.</param>
         * <returns>The position rect to use for the next property field.</returns>
         */
        private Rect CalculatePosition(Rect position, ref int lines, int spaces)
        {
            return new Rect(position.x, position.y +
                lines++ * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + spaces * (EditorGUIUtility.singleLineHeight / 2),
                position.width, EditorGUIUtility.singleLineHeight);
        }

        /**
         * Creates a new value provider based on the provided type and target.
         * <param name="providerType">The type of value provider to create.</param>
         * <param name="attributeSetTarget">The type of attribute set to target for attribute based value providers.</param>
         */
        protected abstract AttributeModifierValueProviderBase CreateValueProvider(EAttributeModifierValueProviderType providerType, Type attributeSetTarget);
    }

    /**
     * The custom property drawer for float based attribute modifiers.
     */
    [CustomPropertyDrawer(typeof(FloatAttributeModifier))]
    public class FloatAttributeModifierDrawer : AttributeModifierDrawer
    {
        protected override AttributeModifierValueProviderBase CreateValueProvider(
            EAttributeModifierValueProviderType providerType, Type attributeSetTarget)
        {
            switch (providerType)
            {
                case EAttributeModifierValueProviderType.Constant:
                    return new FloatAttributeModifierConstantValueProvider();
                case EAttributeModifierValueProviderType.Attribute:
                    var provider = new FloatAttributeModifierAttributeValueProvider();
                    provider.Attribute.InitializeForAttributeSetType(attributeSetTarget);
                    return provider;
                default:
                    return null;
            }
        }
    }

    /**
     * The custom property drawer for int based attribute modifiers.
     */
    [CustomPropertyDrawer(typeof(IntAttributeModifier))]
    public class IntAttributeModifierDrawer : AttributeModifierDrawer
    {
        protected override AttributeModifierValueProviderBase CreateValueProvider(
            EAttributeModifierValueProviderType providerType, Type attributeSetTarget)
        {
            switch (providerType)
            {
                case EAttributeModifierValueProviderType.Constant:
                    return new IntAttributeModifierConstantValueProvider();
                case EAttributeModifierValueProviderType.Attribute:
                    var provider = new IntAttributeModifierAttributeValueProvider();
                    provider.Attribute.InitializeForAttributeSetType(attributeSetTarget);
                    return provider;
                default:
                    return null;
            }
        }
    }
}