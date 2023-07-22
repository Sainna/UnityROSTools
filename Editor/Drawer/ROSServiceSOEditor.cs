using System.Reflection;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sainna.Robotics.ROSTools.Editor
{
    [CustomEditor(typeof(ROSServiceSO))]
    public class ROSServiceSOEditor : UnityEditor.Editor
    {
        private ReorderableList list;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("_ROSServices"),
                false, true, true, true);

            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "ROS Services");
            };
            
            // Draw all the element of the referenced property
            list.drawElementCallback = 
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.indentLevel = 1;
                    EditorGUI.PropertyField(rect, element, isActive);
                };
            
            // Get the height of all the properties element for proper display
            list.elementHeightCallback = (index) => {
                Repaint ();
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, element.isExpanded);
            };
            
            
            // When adding a new service, make sure to change the referenced Request
            list.onAddCallback = (ReorderableList l) => {
                var index = l.serializedProperty.arraySize;
                if (index >= 1)
                {
                    l.serializedProperty.GetArrayElementAtIndex(index - 1).DuplicateCommand();
                    var elementDst = l.serializedProperty.GetArrayElementAtIndex(index);
                    elementDst.FindPropertyRelative("DefaultRequest").managedReferenceValue = null;
                }
                else
                {
                    l.serializedProperty.InsertArrayElementAtIndex(0);
                }
            };
        }
    	
        public override void OnInspectorGUI() {
            serializedObject.Update();
            list.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}