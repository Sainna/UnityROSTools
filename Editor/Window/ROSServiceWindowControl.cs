using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Sainna.Robotics.ROSTools.Editor
{
    public class ROSServiceWindowControl : EditorWindow
    {
        Vector2 scrollPosition;
        
        private bool ShowCustomRequest = false;

        private int SelectedService = 0;
        private string[] ServiceList;

        private ROSConnection ROS;
        private SerializedObject ThisSerialized;


        [SerializeReference] public Message Request;

        // Add menu item named "My Window" to the Window menu
        private void Awake()
        {
            ThisSerialized = new SerializedObject(this);
        }

        [MenuItem("ROS Tools/Service caller")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ROSServiceWindowControl), false, "ROS Service caller");
        }

        protected virtual void DrawAddons()
        {
            
        }


        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (ROS == null)
                ROS = ROSConnection.GetOrCreateInstance();


            // if (Application.isPlaying)
            // {
                GUILayout.Label("Connection status", EditorStyles.boldLabel);
                if ((ROS.HasConnectionError || !ROS.HasConnectionThread))
                {
                    EditorStyles.largeLabel.normal.textColor = Color.red;
                    GUILayout.Label("Disconnected", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.normal.textColor = Color.white;
                    if (!ROS.HasConnectionThread && GUILayout.Button("Connect"))
                    {
                        ROS.Connect();
                    }
                }
                else
                {
                    EditorStyles.largeLabel.normal.textColor = Color.green;
                    GUILayout.Label("Connected", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.normal.textColor = Color.white;
                    if (GUILayout.Button("Disconnect"))
                    {
                        ROS.Disconnect();
                    }

                    EditorGUILayout.Space(12.0f);

                    DrawAddons();

                    EditorGUILayout.Separator();
                    ShowCustomRequest =
                        EditorGUILayout.BeginFoldoutHeaderGroup(ShowCustomRequest, "Custom service request");
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    if (ShowCustomRequest)
                    {
                        DrawServiceControl();
                    }

                }
            // }
            // else
            // {
            //     GUILayout.Label("Application not in Play mode", EditorStyles.boldLabel);
            // }

            EditorGUILayout.EndScrollView();

        }
        
        private ROSServiceManager ServiceManager;

        ROSServiceManager GetServiceManager()
        {
            if (ServiceManager == null)
            {
                ServiceManager = ROSServiceManager.GetOrCreateInstance();
            }

            return ServiceManager;
        }


        void DrawMessageInspector(bool regenRequest, int serviceID)
        {
            ThisSerialized ??= new SerializedObject(this);
            if (regenRequest || Request == null)
            {
                var req = GetServiceManager().GetService(ServiceList[serviceID]).GetDefaultRequest();
                Request = req;
                ThisSerialized.Update();
            }

            SerializedProperty prop = ThisSerialized.FindProperty("Request");
            if (prop != null)
            {
                EditorGUILayout.PropertyField(prop, prop.isExpanded);
                ThisSerialized.ApplyModifiedProperties();
            }

        }

        List<string> DisplayServiceList = new List<string>();

        void DrawServiceControl()
        {
            int newSelected = 0;
            EditorGUILayout.BeginHorizontal();
            if (ServiceList != null && ServiceList.Length > 0 && DisplayServiceList.Count == ServiceList.Length)
            {
                newSelected = EditorGUILayout.Popup("Service name", SelectedService, DisplayServiceList.ToArray());
            }

            if (ServiceList == null || GUILayout.Button("Refresh services"))
            {
                ServiceList = GetServiceManager().GetServices();
                DisplayServiceList.Clear();
                DisplayServiceList.AddRange(ServiceList);
                for (var index = 0; index < DisplayServiceList.Count; index++)
                {
                    if (DisplayServiceList[index].StartsWith('/'))
                    {
                        DisplayServiceList[index] = DisplayServiceList[index].Remove(0, 1);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            if (ServiceList.Length > 0)
            {
                DrawMessageInspector(newSelected != SelectedService, newSelected);
                SelectedService = newSelected;

                if (GUILayout.Button("Send"))
                {
                    Debug.Log(Request);
                    GetServiceManager().GetService(ServiceList[SelectedService]).Call(Request);
                }
            }
            
            EditorGUILayout.Separator();
            if (GUILayout.Button("Reinitialize services"))
            {
                GetServiceManager().Init();
                ServiceList = null;
            }

        }

    }
}