using System.Linq;
using UnityEngine;
using System.Reflection;

namespace Sainna.Utils
{
    public class OnSerializedFieldChangedCallAttribute : PropertyAttribute
    {
        public string methodName;
        public OnSerializedFieldChangedCallAttribute(string methodNameNoArguments)
        {
            methodName = methodNameNoArguments;
        }
    }

#if UNITY_EDITOR

    [UnityEditor.CustomPropertyDrawer(typeof(OnSerializedFieldChangedCallAttribute))]
    public class OnChangedCallAttributePropertyDrawer : UnityEditor.PropertyDrawer
    {
        
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();
            UnityEditor.EditorGUI.PropertyField(position, property, label);
            if(UnityEditor.EditorGUI.EndChangeCheck())
            {
                OnSerializedFieldChangedCallAttribute at = attribute as OnSerializedFieldChangedCallAttribute;
                MethodInfo method = property.serializedObject.targetObject.GetType().GetMethods().Where(m => m.Name == at.methodName).First();
                property.serializedObject.ApplyModifiedProperties();
                if (method != null && method.GetParameters().Count() == 0)// Only instantiate methods with 0 parameters
                    method.Invoke(property.serializedObject.targetObject, null);
            }
        }
    }

#endif


}
