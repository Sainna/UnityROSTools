using System;
using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Sainna.Robotics.ROSTools.Editor
{
    public class ROSWindowControl : EditorWindow
    {
        Vector2 scrollPosition;
        
        private bool ShowCustomRequest = false;

        private int SelectedService = 0;
        private string[] ServiceList;

        private ROSManager ROS;
        private SerializedObject ThisSerialized;


        [SerializeReference] public Message Request;

        // Add menu item named "My Window" to the Window menu
        protected virtual void Awake()
        {
            ThisSerialized = new SerializedObject(this);
        }

        [MenuItem("ROS Tools/Service caller")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ROSWindowControl), false, "ROS Service caller");
        }

        protected virtual void DrawAddons()
        {
            
        }


        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (ROS == null)
                ROS = ROSManager.GetOrCreateInstance();


            // if (Application.isPlaying)
            // {
                GUILayout.Label("Connection status", EditorStyles.boldLabel);
                if (!(ROS.GetROSConnection().IsConnected.WaitOne(0)))
                {
                    EditorStyles.largeLabel.normal.textColor = Color.red;
                    GUILayout.Label("Disconnected", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.normal.textColor = Color.white;
                    if (GUILayout.Button("Connect"))
                    {
                        Debug.LogWarning("CONNECTION VIA EDITOR WINDOW NOT IMPLEMENTED");
                        //ROS.Connect();
                    }
                }
                else
                {
                    EditorStyles.largeLabel.normal.textColor = Color.green;
                    GUILayout.Label("Connected", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.normal.textColor = Color.white;
                    if (GUILayout.Button("Disconnect"))
                    {
                        Debug.LogWarning("DISCONNECTION VIA EDITOR WINDOW NOT IMPLEMENTED");
                        //ROS.Disconnect();
                    }

                    EditorGUILayout.Space(12.0f);

                    DrawAddons();

                    EditorGUILayout.Separator();
                    ShowCustomRequest =
                        EditorGUILayout.BeginFoldoutHeaderGroup(ShowCustomRequest, "Custom service request");
                    
                    if (ShowCustomRequest)
                    {
                        DrawServiceControl();
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();

                }
            // }
            // else
            // {
            //     GUILayout.Label("Application not in Play mode", EditorStyles.boldLabel);
            // }

            EditorGUILayout.EndScrollView();

        }
        
        private ROSManager _manager;

        protected ROSManager GetServiceManager()
        {
            if (_manager == null)
            {
                _manager = ROSManager.GetOrCreateInstance();
            }

            return _manager;
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
            bool closeHorizontal = false;
            
            if (ServiceList != null && ServiceList.Length > 0 && DisplayServiceList.Count == ServiceList.Length)
            {
                EditorGUILayout.BeginHorizontal();
                closeHorizontal = true;
                newSelected = EditorGUILayout.Popup("Service name", SelectedService, DisplayServiceList.ToArray());
            }

            var refreshButton = GUILayout.Button("Refresh services");
            
            if(closeHorizontal)
                EditorGUILayout.EndHorizontal();

            if (ServiceList == null || refreshButton)
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