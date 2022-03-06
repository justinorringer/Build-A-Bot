using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BuildABot
{
    [CustomEditor(typeof(Effect))]
    public class EffectEditor : Editor
    {

        /** The list used to display the effect modifiers. */
        private ReorderableList _list;

        /** The effect being displayed by this editor. */
        private Effect _targetEffect;

        private void OnEnable()
        {

            _targetEffect = target as Effect;
            
            _list = new ReorderableList(serializedObject, serializedObject.FindProperty("modifiers"), true, true, true, true);

            _list.onAddDropdownCallback += (rect, reorderableList) =>
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Float Attribute Modifier"), false, HandleAddClick, new FloatAttributeModifier());
                menu.AddItem(new GUIContent("Int Attribute Modifier"), false, HandleAddClick, new IntAttributeModifier());
                menu.ShowAsContext();
            };

            _list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(rect, element, new GUIContent("Modifier " + index), true);
                EditorGUI.indentLevel--;
            };
            
            _list.drawHeaderCallback = (rect) => {  
                EditorGUI.LabelField(rect, "Modifiers");
            };

            _list.elementHeightCallback = index =>
            {
                SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), true);

            serializedObject.ApplyModifiedProperties();
            
            serializedObject.Update();
            
            SerializedProperty durationModeProperty = serializedObject.FindProperty("durationMode");

            EditorGUILayout.PropertyField(durationModeProperty, true);
            if (IsShowingDuration(durationModeProperty)) 
                EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"), true);
            
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.Space();

            SerializedProperty targetProperty = serializedObject.FindProperty("target");
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(targetProperty, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (EditorUtility.DisplayDialog("Change Targeted Attribute Set?",
                    $"Are you sure that you want to change the AttributeSet type targeted by {_targetEffect.DisplayName}? This will remove all currently applied modifiers.",
                    "Change Target", "Keep Current Target"))
                {
                    // Apply the change and clear the modifiers list
                    
                    _list.serializedProperty.ClearArray();
                    
                    serializedObject.ApplyModifiedProperties();
                }
                else serializedObject.Update(); // Keep current value, revert change
            }
            
            EditorGUILayout.Space();
            
            serializedObject.Update();
            
            _list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        /**
         * Should this drawer show the duration field?
         * <param name="durationModeProperty">The duration mode property.</param>
         */
        private bool IsShowingDuration(SerializedProperty durationModeProperty)
        {
            Enum.TryParse(durationModeProperty.enumNames[durationModeProperty.enumValueIndex],
                out EEffectDurationMode durationType);
            return durationType == EEffectDurationMode.ForDuration;
        }

        /**
         * Handles adding data to the modifier list when clicking add.
         * <param name="data">The data being added.</param>
         */
        private void HandleAddClick(object data)
        {
            if (!(data is AttributeModifierBase modifier))
            {
                Debug.LogErrorFormat("Failed to add modifier to Effect '{0}': Null data", _targetEffect.DisplayName);
                return;
            }
            modifier.Initialize(_targetEffect.Target);
            int index = _list.serializedProperty.arraySize++;
            _list.index = index;
            SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = modifier;
            serializedObject.ApplyModifiedProperties();
        }
    }
}