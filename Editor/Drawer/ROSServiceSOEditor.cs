using System.Reflection;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[CustomEditor(typeof(ROSServiceSO))]
public class ROSServiceSOEditor : Editor
{
    private ReorderableList list;
	
    private void OnEnable() {
        list = new ReorderableList(serializedObject, 
            serializedObject.FindProperty("_ROSServices"), 
            false, true, true, true);
        
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "ROS Services");
        };
        
        
        list.drawElementCallback = 
            (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.indentLevel = 1;
                EditorGUI.PropertyField(rect, element, isActive);
            };
        
        list.elementHeightCallback = (index) => {
            Repaint ();
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, element.isExpanded);
        };
        
        
        list.onAddCallback = (ReorderableList l) => {
            var index = l.serializedProperty.arraySize;
            if (index == 0)
            {
                l.serializedProperty.arraySize++;
                return;
            }
                
            // var elementOrg = l.serializedProperty.GetArrayElementAtIndex(index-1);
            l.serializedProperty.GetArrayElementAtIndex(index - 1).DuplicateCommand();
            var elementDst = l.serializedProperty.GetArrayElementAtIndex(index);
            // elementDst.DuplicateCommand();
            // elementDst = elementOrg.Copy();
            elementDst.FindPropertyRelative("DefaultRequest").managedReferenceValue = null;
        };
    }
	
    public override void OnInspectorGUI() {
        serializedObject.Update();
        list.DoLayoutList();
        
        serializedObject.ApplyModifiedProperties();
    }
}
