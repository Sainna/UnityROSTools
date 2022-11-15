using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEditor;
using UnityEngine;

namespace Sainna.Robotics.ROSTools.Editor
{
    public class UnityCoordToROSCoordWindow : EditorWindow
    {
        Vector2 scrollPosition;


        bool fold = true;
        Vector4 rotationComponents;
        Vector3 eulerComponents;
        Transform selectedTransform;


        private Quaternion customRot;
        private Vector3 customPos;
        private Vector3 customEuler;
        private Vector3 customScale;
        private Vector4 customRotComp;


        private bool ShowSelectionConvert;
        private bool ShowCustomConvert;

        [MenuItem("ROS Tools/Convert coordinates")]
        static void Init()
        {
            var window = GetWindow(typeof(UnityCoordToROSCoordWindow), false, "ROS Coordinate converter");
            window.Show();
        }


        private bool xArmAPICoord = false;

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            xArmAPICoord = EditorGUILayout.Toggle("Use xArm API coordinate", xArmAPICoord);
            ShowSelectionConvert = EditorGUILayout.BeginFoldoutHeaderGroup(ShowSelectionConvert, "Selected transform");
            if (ShowSelectionConvert && Selection.activeGameObject)
            {
                selectedTransform = Selection.activeGameObject.transform;

                bool eulerChange = false;
                bool quatChange = false;
                fold = EditorGUILayout.InspectorTitlebar(fold, selectedTransform);
                if (fold)
                {
                    selectedTransform.position =
                        EditorGUILayout.Vector3Field("Position", selectedTransform.position);
                    EditorGUILayout.Space();
                    EditorGUI.BeginChangeCheck();
                    eulerComponents =
                        EditorGUILayout.Vector3Field("Euler Rotation",
                            selectedTransform.eulerAngles);
                    eulerChange = EditorGUI.EndChangeCheck();
                    EditorGUILayout.Space();
                    EditorGUI.BeginChangeCheck();
                    rotationComponents =
                        EditorGUILayout.Vector4Field("Detailed Rotation",
                            QuaternionToVector4(selectedTransform.localRotation));
                    quatChange = EditorGUI.EndChangeCheck();
                    EditorGUILayout.Space();
                    selectedTransform.localScale =
                        EditorGUILayout.Vector3Field("Scale", selectedTransform.localScale);
                }

                if (eulerChange)
                {
                    selectedTransform.localRotation = Quaternion.Euler(eulerComponents);
                }
                else if (quatChange)
                {
                    selectedTransform.localRotation = ConvertToQuaternion(rotationComponents);
                }

                EditorGUILayout.Space(12.0f, true);

                var localRotation = selectedTransform.localRotation;
                var rosPos = selectedTransform.position.To<FLU>();
                var rosEuler = localRotation.eulerAngles.To<FLU>();
                var rosRot = localRotation.To<FLU>();
                if (xArmAPICoord)
                {
                    rosPos *= 1000;
                    rosEuler *= Mathf.Deg2Rad;
                }

                EditorGUILayout.Vector3Field("ROS Position", new Vector3(rosPos.x, rosPos.y, rosPos.z));
                EditorGUILayout.Vector3Field("ROS Rotation (Euler)", new Vector3(rosEuler.x, rosEuler.y, rosEuler.z));

                if (!xArmAPICoord)
                    EditorGUILayout.Vector4Field("ROS Rotation (Quaternion)",
                        new Vector4(rosRot.x, rosRot.y, rosRot.z, rosRot.w));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(18.0f, true);
            ShowCustomConvert = EditorGUILayout.BeginFoldoutHeaderGroup(ShowCustomConvert, "Custom transform");
            if (ShowCustomConvert)
            {
                bool eulerChange;
                bool quatChange;

                customPos =
                    EditorGUILayout.Vector3Field("Position", customPos);
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                customEuler =
                    EditorGUILayout.Vector3Field("Euler Rotation",
                        customEuler);
                eulerChange = EditorGUI.EndChangeCheck();
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                customRotComp =
                    EditorGUILayout.Vector4Field("Detailed Rotation",
                        QuaternionToVector4(customRot));
                quatChange = EditorGUI.EndChangeCheck();
                EditorGUILayout.Space();
                // customScale =
                //     EditorGUILayout.Vector3Field("Scale", customScale);


                if (eulerChange)
                {
                    customRot = Quaternion.Euler(customEuler);
                }
                else if (quatChange)
                {
                    customRot = ConvertToQuaternion(customRotComp);
                }

                EditorGUILayout.Space(12.0f, true);

                var rosPos = customPos.To<FLU>();
                var rosEuler = customRot.eulerAngles.To<FLU>();
                var rosRot = customRot.To<FLU>();

                if (xArmAPICoord)
                {
                    rosPos *= 1000;
                    rosEuler *= Mathf.Deg2Rad;
                }

                EditorGUILayout.Vector3Field("ROS Position", new Vector3(rosPos.x, rosPos.y, rosPos.z));
                EditorGUILayout.Vector3Field("ROS Rotation (Euler)", new Vector3(rosEuler.x, rosEuler.y, rosEuler.z));

                if (!xArmAPICoord)
                    EditorGUILayout.Vector4Field("ROS Rotation (Quaternion)",
                        new Vector4(rosRot.x, rosRot.y, rosRot.z, rosRot.w));

                customEuler = customRot.eulerAngles;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndScrollView();
        }

        Quaternion ConvertToQuaternion(Vector4 v4)
        {
            return new Quaternion(v4.x, v4.y, v4.z, v4.w);
        }

        Vector4 QuaternionToVector4(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        void OnInspectorUpdate()
        {
            this.Repaint();
        }
    }
}
