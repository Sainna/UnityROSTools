using System;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEditor;
using UnityEngine;
using Sainna.Utils.Extensions;


namespace Sainna.Robotics.ROSTools.Editor
{
    [CustomPropertyDrawer(typeof(ROSServiceSO.ROSServiceInfo))]
    public class ROSServiceInfoPropertyDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect

        private const int DEFAULT_LINES = 6;

        private int GetRequestChildrenCount(SerializedProperty property)
        {
            var defaultRequestProperty = property.FindPropertyRelative("DefaultRequest");
            return defaultRequestProperty.CountInProperty();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x,
                EditorGUIUtility.singleLineHeight);

            var serviceNameProperty = property.FindPropertyRelative("ServiceName");
            var messageTypeProperty = property.FindPropertyRelative("MessageType");
            var commentProperty = property.FindPropertyRelative("Comment");
            var defaultRequestProperty = property.FindPropertyRelative("DefaultRequest");

            label.text = serviceNameProperty.stringValue;

            property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);
            if (property.isExpanded)
            {
                Rect rectProp = new Rect(position.min.x, position.min.y + EditorGUIUtility.singleLineHeight,
                    position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(rectProp, serviceNameProperty);
                rectProp = rectProp.AddYPositionLine();
                EditorGUI.PropertyField(rectProp, messageTypeProperty);

                Type reqType = Type.GetType(messageTypeProperty.stringValue + ", Assembly-CSharp") ??
                               Type.GetType(messageTypeProperty.stringValue +
                                            ", Unity.Robotics.ROSTCPConnector.Messages");
                if (reqType != null)
                {
                    if (defaultRequestProperty.managedReferenceValue == null ||
                        (reqType.FullName != null &&
                         !defaultRequestProperty.managedReferenceFullTypename.EndsWith(reqType.FullName)))
                    {
                        var rawOb = Activator.CreateInstance(reqType) as Message;
                        defaultRequestProperty.managedReferenceValue = rawOb;
                    }

                    EditorGUI.indentLevel = 2;
                    rectProp = rectProp.AddYPositionLine();
                    EditorGUI.PropertyField(rectProp, defaultRequestProperty, true);
                    if (defaultRequestProperty.isExpanded)
                    {
                        int typeDepth = defaultRequestProperty.CountInProperty();
                        rectProp = rectProp.AddYPositionLine(typeDepth - 1);
                    }
                }

                EditorGUI.indentLevel = 1;
                rectProp = rectProp.AddYPositionLine(3).AddLinesHeight(2);
                commentProperty.stringValue =
                    EditorGUI.TextArea(rectProp, commentProperty.stringValue, EditorStyles.helpBox);

            }



            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int totalLines = 1;

            if (property.isExpanded)
            {
                totalLines += DEFAULT_LINES; // for type field


                totalLines += GetRequestChildrenCount(property);
            }

            return EditorGUIUtility.singleLineHeight * totalLines +
                   EditorGUIUtility.standardVerticalSpacing * (totalLines - 1);
        }
    }
}