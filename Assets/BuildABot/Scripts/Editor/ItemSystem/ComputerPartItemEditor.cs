using System;
using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    [CustomEditor(typeof(ComputerPartItem))]
    public class ComputerPartItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            
            SerializedProperty partTypeProperty = serializedObject.FindProperty("partType");
            Enum.TryParse(partTypeProperty.enumNames[partTypeProperty.enumValueIndex],
                out EComputerPartSlot partType);

            SerializedProperty attackProperty = serializedObject.FindProperty("attack");
            if (partType == EComputerPartSlot.Mouse || partType == EComputerPartSlot.Keyboard)
            {
                attackProperty.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Attack"), attackProperty.objectReferenceValue, typeof(MeleeAttackData), false);
            }
            else if (partType == EComputerPartSlot.WirelessCard)
            {
                attackProperty.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Attack"), attackProperty.objectReferenceValue, typeof(AoeAttackData), false);
            }
            else if (partType == EComputerPartSlot.DiskDrive)
            {
                attackProperty.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Attack"), attackProperty.objectReferenceValue, typeof(ProjectileAttackData), false);
            }
            else
            {
                attackProperty.objectReferenceValue = null;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}