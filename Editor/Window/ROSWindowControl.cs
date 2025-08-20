using System;
using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using Sainna.Robotics.ROSTools.Logging;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Sainna.Robotics.ROSTools.Editor
{
    public class ROSWindowControl : EditorWindow
    {
        Vector2 scrollPosition;

        private bool ShowCustomRequest = false;
        private bool ShowTopicManagement = false;
        private bool ShowReconnectSettings = false;

        private int SelectedService = 0;
        private string[] ServiceList;

        // Topic management variables
        private string newTopicName = "";
        private int selectedMessageType = 0;
        private string[] availableMessageTypes;

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


            if (Application.isPlaying)
            {
            GUILayout.Label("Connection status", EditorStyles.boldLabel);

            // Connection status display
            if (ROS == null || !ROS.IsConnected)
            {
                EditorStyles.largeLabel.normal.textColor = Color.red;
                string statusText = ROS.IsReconnecting ? "Reconnecting..." : "Disconnected";
                GUILayout.Label(statusText, EditorStyles.largeLabel);
                EditorStyles.largeLabel.normal.textColor = Color.white;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Manual Reconnect") && !ROS.IsReconnecting)
                {
                    ROS.ManualReconnect();
                }

                if (GUILayout.Button("Stop Reconnect") && ROS.IsReconnecting)
                {
                    ROS.StopReconnection();
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorStyles.largeLabel.normal.textColor = Color.green;
                GUILayout.Label("Connected", EditorStyles.largeLabel);
                EditorStyles.largeLabel.normal.textColor = Color.white;
            }

            EditorGUILayout.Space(12.0f);

            // Auto-reconnect settings
            ShowReconnectSettings =
                EditorGUILayout.BeginFoldoutHeaderGroup(ShowReconnectSettings, "Auto-Reconnect Settings");

            if (ShowReconnectSettings)
            {
                DrawReconnectSettings();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            DrawAddons();

            EditorGUILayout.Separator();
            ShowTopicManagement =
                EditorGUILayout.BeginFoldoutHeaderGroup(ShowTopicManagement, "Topic Management");

            if (ShowTopicManagement)
            {
                DrawTopicManagement();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Separator();
            ShowCustomRequest =
                EditorGUILayout.BeginFoldoutHeaderGroup(ShowCustomRequest, "Custom service request");

            if (ShowCustomRequest)
            {
                DrawServiceControl();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            }
            else
            {
                 GUILayout.Label("Application not in Play mode", EditorStyles.boldLabel);
            }
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

        void DrawReconnectSettings()
        {
            EditorGUILayout.BeginVertical("box");

            // Auto-reconnect enabled toggle
            bool autoReconnectEnabled = ROS.AutoReconnectEnabled;
            bool newAutoReconnectEnabled = EditorGUILayout.Toggle("Enable Auto-Reconnect", autoReconnectEnabled);
            if (newAutoReconnectEnabled != autoReconnectEnabled)
            {
                ROS.AutoReconnectEnabled = newAutoReconnectEnabled;
            }

            EditorGUI.BeginDisabledGroup(!ROS.AutoReconnectEnabled);

            // Max reconnect attempts
            int maxAttempts = ROS.MaxReconnectAttempts;
            int newMaxAttempts = EditorGUILayout.IntField("Max Reconnect Attempts (0 = infinite)", maxAttempts);
            if (newMaxAttempts != maxAttempts)
            {
                ROS.MaxReconnectAttempts = newMaxAttempts;
            }

            // Reconnect interval
            float reconnectInterval = ROS.ReconnectInterval;
            float newReconnectInterval = EditorGUILayout.FloatField("Reconnect Interval (seconds)", reconnectInterval);
            if (newReconnectInterval != reconnectInterval)
            {
                ROS.ReconnectInterval = newReconnectInterval;
            }

            // Connection timeout
            float connectionTimeout = ROS.ConnectionTimeout;
            float newConnectionTimeout = EditorGUILayout.FloatField("Connection Timeout (seconds)", connectionTimeout);
            if (newConnectionTimeout != connectionTimeout)
            {
                ROS.ConnectionTimeout = newConnectionTimeout;
            }

            EditorGUI.EndDisabledGroup();

            // Status information
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status Information", EditorStyles.boldLabel);

            string reconnectStatus = ROS.IsReconnecting ? "Currently reconnecting..." : "Idle";
            EditorGUILayout.LabelField("Reconnect Status:", reconnectStatus);

            string connectionStatus = ROS.IsConnected ? "Connected" : "Disconnected";
            EditorGUILayout.LabelField("Connection Status:", connectionStatus);

            EditorGUILayout.EndVertical();
        }

        void DrawTopicManagement()
        {
            GUILayout.Label("Active Topics", EditorStyles.boldLabel);

            var topics = GetServiceManager().GetTopics();
            if (topics.Length > 0)
            {
                EditorGUILayout.BeginVertical("box");
                foreach (var topicName in topics)
                {
                    EditorGUILayout.BeginHorizontal();
                    var topic = GetServiceManager().GetTopic(topicName);

                    // Topic name and status
                    GUILayout.Label(topicName, GUILayout.Width(200));

                    // Status indicator
                    var statusColor = topic.IsActive ? Color.green : Color.red;
                    var originalColor = GUI.color;
                    GUI.color = statusColor;
                    GUILayout.Label(topic.IsActive ? "●" : "●", GUILayout.Width(20));
                    GUI.color = originalColor;

                    // Topic type
                    var topicType = topic is ROSPublisher<RosSharp.RosBridgeClient.Message>
                        ? "Publisher"
                        : "Subscriber";
                    GUILayout.Label(topicType, GUILayout.Width(80));

                    // Remove button
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        GetServiceManager().RemoveTopic(topicName);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No active topics", MessageType.Info);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Create New Publisher", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            newTopicName = EditorGUILayout.TextField("Topic Name", newTopicName);

            // Initialize message types if needed
            if (availableMessageTypes == null)
            {
                RefreshMessageTypes();
            }

            if (availableMessageTypes != null && availableMessageTypes.Length > 0)
            {
                selectedMessageType = EditorGUILayout.Popup("Message Type", selectedMessageType, availableMessageTypes);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Publisher"))
                {
                    CreatePublisherFromEditor();
                }

                if (GUILayout.Button("Refresh Types"))
                {
                    RefreshMessageTypes();
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No message types available. Make sure you have ROS message types in your project.",
                    MessageType.Warning);
                if (GUILayout.Button("Refresh Types"))
                {
                    RefreshMessageTypes();
                }
            }

            EditorGUILayout.EndVertical();
        }

        void RefreshMessageTypes()
        {
            // Get available message types from the ServiceMessageListAttributeDrawer
            if (ServiceMessageListAttributeDrawer.TypeNames != null &&
                ServiceMessageListAttributeDrawer.TypeNames.Length > 0)
            {
                // Convert request types to general message types
                var messageTypes = new List<string>();
                foreach (var typeName in ServiceMessageListAttributeDrawer.TypeNames)
                {
                    var shortName = typeName;
                    if (shortName.Contains(","))
                        shortName = shortName.Substring(0, shortName.IndexOf(","));

                    // Convert Request types to base message types for topics
                    if (shortName.EndsWith("Request"))
                    {
                        var baseType = shortName.Replace("Request", "");
                        if (!messageTypes.Contains(baseType))
                            messageTypes.Add(baseType);
                    }
                    else if (!shortName.EndsWith("Response") && !messageTypes.Contains(shortName))
                    {
                        messageTypes.Add(shortName);
                    }
                }

                availableMessageTypes = messageTypes.ToArray();
            }
            else
            {
                // Fallback to some common types
                availableMessageTypes = new string[]
                {
                    "RosSharp.RosBridgeClient.MessageTypes.Std.String",
                    "RosSharp.RosBridgeClient.MessageTypes.Std.Int32",
                    "RosSharp.RosBridgeClient.MessageTypes.Std.Float64"
                };
            }
        }

        void CreatePublisherFromEditor()
        {
            if (string.IsNullOrEmpty(newTopicName))
            {
                EditorUtility.DisplayDialog("Error", "Topic name cannot be empty", "OK");
                return;
            }

            if (availableMessageTypes == null || availableMessageTypes.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No message types available", "OK");
                return;
            }

            try
            {
                var selectedTypeName = availableMessageTypes[selectedMessageType];
                var messageType = Type.GetType(selectedTypeName + ", Assembly-CSharp") ??
                                  Type.GetType(selectedTypeName + ", siemens.ros-sharp.Runtime");

                if (messageType == null)
                {
                    EditorUtility.DisplayDialog("Error", $"Could not find message type: {selectedTypeName}", "OK");
                    return;
                }

                // Use reflection to call CreatePublisher with the correct type
                var method = typeof(ROSManager).GetMethod("CreatePublisher").MakeGenericMethod(messageType);
                var result = method.Invoke(GetServiceManager(), new object[] { newTopicName });

                if (result != null)
                {
                    ROSLogger.LogInfo($"Created publisher for topic: '{newTopicName}' with type: {messageType.Name}", ROSLogger.CATEGORY_EDITOR);
                    newTopicName = "";
                }
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to create publisher: {ex.Message}", "OK");
                ROSLogger.LogError($"Failed to create publisher: {ex}", ROSLogger.CATEGORY_EDITOR);
            }
        }

        void DrawServiceControl()
        {
            int newSelected = 0;
            bool closeHorizontal = false;
            
            EditorGUILayout.BeginHorizontal();

            if (ServiceList != null && ServiceList.Length > 0 && DisplayServiceList.Count == ServiceList.Length)
            {
                
                closeHorizontal = true;
                newSelected = EditorGUILayout.Popup("Service name", SelectedService, DisplayServiceList.ToArray());
            }

            var refreshButton = GUILayout.Button("Refresh services");


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
                    ROSLogger.LogInfo($"Sending service request: {Request}", ROSLogger.CATEGORY_EDITOR);
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