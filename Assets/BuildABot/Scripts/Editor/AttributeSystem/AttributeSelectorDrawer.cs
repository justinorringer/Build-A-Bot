using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * The generic version of the attribute selector property drawer. To be used in editor, a concrete version with the
     * CustomPropertyDrawer attribute must be used.
     * <typeparam name="T">The type of attribute being selected.</typeparam>
     */
    public abstract class AttributeSelectorDrawer<T> : PropertyDrawer
    {

        /** The index used to track the current value. */
        private int _index;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            SerializedProperty optionsProperty = property.FindPropertyRelative("options");

            int optionCount = optionsProperty.arraySize;

            string currentValue = valueProperty.stringValue;
            
            string[] options = new string[optionCount];
            string[] optionsNice = new string[optionCount];
            for (int i = 0; i < optionCount; i++)
            {
                if (options[i] == currentValue) _index = i;
                options[i] = optionsProperty.GetArrayElementAtIndex(i).stringValue;
                optionsNice[i] = ObjectNames.NicifyVariableName(options[i]);
            }
            
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            _index = EditorGUI.Popup(position, label.text, _index, optionsNice);
            
            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.stringValue = options[_index];
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
    
    /**
     * The attribute selector drawer for float attribute selectors.
     */
    [CustomPropertyDrawer(typeof(FloatAttributeSelector))]
    public class FloatAttributeSelectorDrawer : AttributeSelectorDrawer<float> {}
    
    /**
     * The attribute selector drawer for int attribute selectors.
     */
    [CustomPropertyDrawer(typeof(IntAttributeSelector))]
    public class IntAttributeSelectorDrawer : AttributeSelectorDrawer<int> {}
    
    /**
     * The property drawer for AttributeSetSelector instances.
     */
    [CustomPropertyDrawer(typeof(AttributeSetSelector))]
    public class AttributeSetSelectorDrawer : PropertyDrawer
    {

        /** The index used to track the current value. */
        private int _index;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            if (!(fieldInfo.FieldType.GetProperty("Options")?.GetValue(null) is string[] options))
                options = new string [] { "-" };
            if (!(fieldInfo.FieldType.GetProperty("NiceOptions")?.GetValue(null) is string[] optionsNice))
                optionsNice = new string [] { "-" };
            
            int optionCount = options.Length;
            string currentValue = valueProperty.stringValue;
            
            // Get the current value index
            for (int i = 0; i < optionCount; i++)
            {
                if (options[i] == currentValue) _index = i;
            }
            
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            _index = EditorGUI.Popup(position, label.text, _index, optionsNice);
            
            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.stringValue = options[_index];
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}