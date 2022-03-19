using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    
    /**
     * The base implementation of the attribute selector property drawer. To be used in editor, a concrete version with the
     * CustomPropertyDrawer attribute must be used.
     */
    public abstract class AttributeSelectorDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            SerializedProperty optionsProperty = property.FindPropertyRelative("options");

            int optionCount = optionsProperty.arraySize;

            int index = 0;

            string currentValue = valueProperty.stringValue;
            
            string[] options = new string[optionCount];
            string[] optionsNice = new string[optionCount];
            for (int i = 0; i < optionCount; i++)
            {
                options[i] = optionsProperty.GetArrayElementAtIndex(i).stringValue;
                if (options[i] == currentValue) index = i;
                optionsNice[i] = ObjectNames.NicifyVariableName(options[i]);
            }
            
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            index = EditorGUI.Popup(position, label.text, index, optionsNice);
            
            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.stringValue = options[index];
            }
            
            EditorGUI.EndProperty();
        }
    }
    
    /**
     * The attribute selector drawer for float attribute selectors.
     */
    [CustomPropertyDrawer(typeof(FloatAttributeSelector))]
    public class FloatAttributeSelectorDrawer : AttributeSelectorDrawer {}
    
    /**
     * The attribute selector drawer for int attribute selectors.
     */
    [CustomPropertyDrawer(typeof(IntAttributeSelector))]
    public class IntAttributeSelectorDrawer : AttributeSelectorDrawer {}
    
    /**
     * The property drawer for AttributeSetSelector instances.
     */
    [CustomPropertyDrawer(typeof(AttributeSetSelector))]
    public class AttributeSetSelectorDrawer : PropertyDrawer
    {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            string currentValue = valueProperty.stringValue;
            bool isInvalid = string.IsNullOrEmpty(currentValue);

            string[] options = fieldInfo.FieldType.GetProperty("Options")?.GetValue(null) as string[];
            string[] optionsNice = fieldInfo.FieldType.GetProperty("NiceOptions")?.GetValue(null) as string[];
            
            if (null == options) options = new string[0];
            if (null == optionsNice) optionsNice = new string[0];

            if (isInvalid)
            {
                string[] optionsWithInvalid = new string[options.Length + 1];
                optionsWithInvalid[0] = "-";
                string[] optionsNiceWithInvalid = new string[optionsNice.Length + 1];
                optionsNiceWithInvalid[0] = "-";
                for (int i = 0; i < options.Length; i++)
                {
                    optionsWithInvalid[i + 1] = options[i];
                    optionsNiceWithInvalid[i + 1] = optionsNice[i];
                }

                options = optionsWithInvalid;
                optionsNice = optionsNiceWithInvalid;
            }
            
            int optionCount = options.Length;

            int index = 0;
            
            // Get the current value index
            for (int i = 0; i < optionCount; i++)
            {
                if (options[i] == currentValue) index = i;
            }
            
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            index = EditorGUI.Popup(position, label.text, index, optionsNice);
            
            if (EditorGUI.EndChangeCheck())
            {
                valueProperty.stringValue = options[index];
            }
            
            EditorGUI.EndProperty();
        }
    }
}