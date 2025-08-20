using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using RosSharp.RosBridgeClient;
using Sainna.Utils;
using Sainna.Robotics.ROSTools.Logging;
using UnityEngine;



namespace Sainna.Robotics.ROSTools
{
    /// <summary>
    /// Manages ROS services and topics in Unity. Provides a centralized way to handle ROS communication.
    /// This is a singleton MonoBehaviour that should exist only once per scene.
    /// </summary>
    public class ROSManager : MonoBehaviour
    {
        // Singleton behavior
        protected static ROSManager _instance;

        /// <summary>
        /// Gets the existing ROSManager instance or creates a new one if none exists.
        /// </summary>
        /// <returns>The ROSManager instance</returns>
        public static ROSManager GetOrCreateInstance()
        {
            if (_instance == null)
            {
                // Prefer to use the ROSConnection in the scene, if any
                _instance = FindFirstObjectByType<ROSManager>();
                if (_instance != null)
                    return _instance;

                GameObject instance = new GameObject("ROSManager");
                _instance = instance.AddComponent<ROSManager>();
                // _instance.Init();

            }
            return _instance;
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;

            RosConnectionManager = GetROSConnection(false);
            
            Init();
        }

        void Start()
        {
            // Start connection monitoring after initialization
            StartCoroutine(MonitorConnection());
        }


        private RosConnector RosConnectionManager;
        
        [Header("ROS Connection Settings")]
        [SerializeField] private string _ROSAddress = "localhost";
        [SerializeField] private int _ROSPort = 9090;

        [Header("Auto-Reconnect Settings")]
        [SerializeField] 
        [Tooltip("Enable automatic reconnection when connection is lost")]
        private bool enableAutoReconnect = true;
        
        [SerializeField] 
        [Tooltip("Maximum number of reconnection attempts (0 = infinite)")]
        private int maxReconnectAttempts = 5;
        
        [SerializeField] 
        [Tooltip("Time to wait between reconnection attempts (seconds)")]
        private float reconnectInterval = 3.0f;
        
        [SerializeField] 
        [Tooltip("Timeout for each connection attempt (seconds)")]
        private float connectionTimeout = 10.0f;

        [Header("Service Configuration")]
        [SerializeField]
        [Tooltip("Array of ROS Service Scriptable Objects to register on startup")]
        // [OnSerializedFieldChangedCall("Init")]
        private ROSServiceSO[] Services;

        // Auto-reconnect state variables
        private bool isReconnecting = false;
        private int currentReconnectAttempt = 0;
        private Coroutine reconnectCoroutine;

        private Dictionary<string, ROSService> ServiceMap = new Dictionary<string, ROSService>();
        private Dictionary<string, ROSTopic> TopicMap = new Dictionary<string, ROSTopic>();

        public ROSService GetService(string serviceName) 
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                ROSLogger.LogError("Service name cannot be null or empty", ROSLogger.CATEGORY_SERVICES, this);
                return null;
            }
            
            if (!ServiceMap.ContainsKey(serviceName))
            {
                ROSLogger.LogWarning($"Service '{serviceName}' not found. Available services: {string.Join(", ", ServiceMap.Keys)}", 
                    ROSLogger.CATEGORY_SERVICES, this);
                return null;
            }
            
            return ServiceMap[serviceName];
        }
        
        public string[] GetServices() => ServiceMap.Keys.ToArray();

        // Topic management methods
        public ROSTopic GetTopic(string topicName)
        {
            if (string.IsNullOrEmpty(topicName))
            {
                ROSLogger.LogError("Topic name cannot be null or empty", ROSLogger.CATEGORY_TOPICS, this);
                return null;
            }
            
            if (!TopicMap.ContainsKey(topicName))
            {
                ROSLogger.LogWarning($"Topic '{topicName}' not found. Available topics: {string.Join(", ", TopicMap.Keys)}", 
                    ROSLogger.CATEGORY_TOPICS, this);
                return null;
            }
            
            return TopicMap[topicName];
        }
        public string[] GetTopics() => TopicMap.Keys.ToArray();

        /// <summary>
        /// Creates and registers a ROS Publisher
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <param name="topicName">The ROS topic name</param>
        /// <returns>The created publisher</returns>
        public ROSPublisher<T> CreatePublisher<T>(string topicName) where T : Message
        {
            if (string.IsNullOrEmpty(topicName))
            {
                ROSLogger.LogError("Topic name cannot be null or empty", ROSLogger.CATEGORY_TOPICS, this);
                return null;
            }
            
            if (TopicMap.ContainsKey(topicName))
            {
                ROSLogger.LogWarning($"Topic '{topicName}' already exists. Returning existing publisher.", ROSLogger.CATEGORY_TOPICS, this);
                var existingTopic = TopicMap[topicName] as ROSPublisher<T>;
                if (existingTopic == null)
                {
                    ROSLogger.LogError($"Topic '{topicName}' exists but is not a publisher of type {typeof(T).Name}", ROSLogger.CATEGORY_TOPICS, this);
                    return null;
                }
                return existingTopic;
            }

            var publisher = ROSTopicFactory.CreatePublisher<T>(topicName, RosConnectionManager);
            publisher.Init();
            TopicMap.Add(topicName, publisher);
            
            ROSLogger.LogInfo($"Created publisher for topic: '{topicName}'", ROSLogger.CATEGORY_TOPICS, this);
            return publisher;
        }

        /// <summary>
        /// Creates and registers a ROS Subscriber
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <param name="topicName">The ROS topic name</param>
        /// <param name="callback">Callback function for received messages</param>
        /// <returns>The created subscriber</returns>
        public ROSSubscriber<T> CreateSubscriber<T>(string topicName, SubscriptionHandler<T> callback) where T : Message
        {
            if (string.IsNullOrEmpty(topicName))
            {
                ROSLogger.LogError("Topic name cannot be null or empty", ROSLogger.CATEGORY_TOPICS, this);
                return null;
            }
            
            if (callback == null)
            {
                ROSLogger.LogError("Callback cannot be null for subscriber", ROSLogger.CATEGORY_TOPICS, this);
                return null;
            }
            
            if (TopicMap.ContainsKey(topicName))
            {
                ROSLogger.LogWarning($"Topic '{topicName}' already exists. Returning existing subscriber.", ROSLogger.CATEGORY_TOPICS, this);
                var existingTopic = TopicMap[topicName] as ROSSubscriber<T>;
                if (existingTopic == null)
                {
                    ROSLogger.LogError($"Topic '{topicName}' exists but is not a subscriber of type {typeof(T).Name}", ROSLogger.CATEGORY_TOPICS, this);
                    return null;
                }
                return existingTopic;
            }

            var subscriber = ROSTopicFactory.CreateSubscriber<T>(topicName, callback, RosConnectionManager);
            subscriber.Init();
            TopicMap.Add(topicName, subscriber);
            
            ROSLogger.LogInfo($"Created subscriber for topic: '{topicName}'", ROSLogger.CATEGORY_TOPICS, this);
            return subscriber;
        }

        /// <summary>
        /// Removes and cleans up a topic
        /// </summary>
        /// <param name="topicName">The topic name to remove</param>
        public void RemoveTopic(string topicName)
        {
            if (TopicMap.ContainsKey(topicName))
            {
                TopicMap[topicName].Cleanup();
                TopicMap.Remove(topicName);
                ROSLogger.LogInfo($"Removed topic: '{topicName}'", ROSLogger.CATEGORY_TOPICS, this);
            }
        }


        protected virtual void InitServiceConnection(ROSService service, RosConnector connection)
        {
            ROSLogger.LogInfo($"Registering service '{service.ServiceName}'", ROSLogger.CATEGORY_SERVICES, this);
            service.Init(connection);
        }


        // In case some services need special initialisation processes (i.e. detour service for empty calls)
        protected virtual void ServicePreProcess(ROSServiceSO.ROSServiceInfo serviceInfo, ROSService serviceAbst, ref string serviceName)
        {
            
        }
        
        public delegate void ROSConnectionDelegate();
        public event ROSConnectionDelegate ROSConnected; // event
        public event ROSConnectionDelegate ROSDisconnected; // event
        
        public delegate void ROSReconnectionDelegate(int attemptNumber, int maxAttempts);
        public event ROSReconnectionDelegate ROSReconnectionAttempt; // event
        
        public delegate void ROSReconnectionFailedDelegate(int totalAttempts);
        public event ROSReconnectionFailedDelegate ROSReconnectionFailed; // event

        public bool WaitingOnRosConnection { get; private set; } = false;
        public bool IsConnected => RosConnectionManager != null && RosConnectionManager.IsConnected.WaitOne(0);
        public bool IsReconnecting => isReconnecting;
        // needs to be public to be called by the OnSerializedFieldChanged attribute
        public void Init()
        {
            ROSLogger.LogInfo("Initialising services", ROSLogger.CATEGORY_MANAGER, this);
            
            // Init manager: Get all the services and create a representation as ROSService in memory
            ServiceMap.Clear();

            if (Services == null)
            {
                ROSLogger.LogInfo("No services registered", ROSLogger.CATEGORY_MANAGER, this);
                return;
            }
                
            
            foreach (var serviceSo in Services)
            {
                foreach (var serviceInfo in serviceSo.ROSServicesInfos)
                {
                    var serviceAbst = serviceInfo.InitService();
                    if (serviceAbst == null)
                    {
                        ROSLogger.LogError($"Failed to initialize service: {serviceInfo.ServiceName}", ROSLogger.CATEGORY_SERVICES, this);
                        continue;
                    }

                    string serviceName = serviceInfo.ServiceName;
                    ServicePreProcess(serviceInfo, serviceAbst, ref serviceName);

                    if (ServiceMap.ContainsKey(serviceName))
                    {
                        ROSLogger.LogWarning($"Service '{serviceName}' already exists. Skipping duplicate.", ROSLogger.CATEGORY_SERVICES, this);
                        continue;
                    }

                    ServiceMap.Add(serviceName, serviceAbst);
                    
                    // Initialize service connection if we have a connection
                    if (RosConnectionManager != null)
                    {
                        InitServiceConnection(serviceAbst, RosConnectionManager);
                    }
                }
            }
            
            // Trigger connection event if we're connected
            if (RosConnectionManager != null && RosConnectionManager.IsConnected.WaitOne(0))
            {
                ROSConnected?.Invoke();
            }
        }

        /// <summary>
        /// Monitors the ROS connection and triggers auto-reconnect if enabled
        /// </summary>
        private IEnumerator MonitorConnection()
        {
            bool wasConnected = false;
            
            while (this != null)
            {
                bool currentlyConnected = IsConnected;
                
                // Check if we just lost connection
                if (wasConnected && !currentlyConnected && !isReconnecting)
                {
                    ROSLogger.LogWarning("Connection lost!", ROSLogger.CATEGORY_CONNECTION, this);
                    ROSDisconnected?.Invoke();
                    
                    if (enableAutoReconnect)
                    {
                        StartReconnection();
                    }
                }
                // Check if we just gained connection
                else if (!wasConnected && currentlyConnected && !isReconnecting)
                {
                    ROSLogger.LogInfo("Connection established!", ROSLogger.CATEGORY_CONNECTION, this);
                    ROSConnected?.Invoke();
                    currentReconnectAttempt = 0; // Reset attempt counter on successful connection
                }
                
                wasConnected = currentlyConnected;
                yield return new WaitForSeconds(1.0f); // Check every second
            }
        }

        /// <summary>
        /// Starts the auto-reconnection process
        /// </summary>
        private void StartReconnection()
        {
            if (isReconnecting)
                return;
                
            if (reconnectCoroutine != null)
            {
                StopCoroutine(reconnectCoroutine);
            }
            
            reconnectCoroutine = StartCoroutine(AutoReconnect());
        }

        /// <summary>
        /// Auto-reconnection coroutine
        /// </summary>
        private IEnumerator AutoReconnect()
        {
            isReconnecting = true;
            currentReconnectAttempt = 0;
            
            ROSLogger.LogInfo($"Starting auto-reconnection (max attempts: {(maxReconnectAttempts == 0 ? "infinite" : maxReconnectAttempts.ToString())})", 
                ROSLogger.CATEGORY_CONNECTION, this);
            
            while (isReconnecting && (maxReconnectAttempts == 0 || currentReconnectAttempt < maxReconnectAttempts))
            {
                currentReconnectAttempt++;
                ROSLogger.LogInfo($"Reconnection attempt {currentReconnectAttempt}/{(maxReconnectAttempts == 0 ? "∞" : maxReconnectAttempts.ToString())}", 
                    ROSLogger.CATEGORY_CONNECTION, this);
                
                ROSReconnectionAttempt?.Invoke(currentReconnectAttempt, maxReconnectAttempts);
                
                // Try to reconnect
                yield return StartCoroutine(AttemptReconnection());
                
                // Check if connection was successful
                if (IsConnected)
                {
                    ROSLogger.LogInfo("Reconnection successful!", ROSLogger.CATEGORY_CONNECTION, this);
                    isReconnecting = false;
                    currentReconnectAttempt = 0;
                    
                    // Reinitialize services and topics
                    ReinitializeAfterReconnection();
                    
                    ROSConnected?.Invoke();
                    yield break;
                }
                
                // Wait before next attempt
                if (currentReconnectAttempt < maxReconnectAttempts || maxReconnectAttempts == 0)
                {
                    ROSLogger.LogInfo($"Waiting {reconnectInterval} seconds before next attempt...", ROSLogger.CATEGORY_CONNECTION, this);
                    yield return new WaitForSeconds(reconnectInterval);
                }
            }
            
            // All attempts failed
            isReconnecting = false;
            ROSLogger.LogError($"Failed to reconnect after {currentReconnectAttempt} attempts. Auto-reconnection stopped.", 
                ROSLogger.CATEGORY_CONNECTION, this);
            ROSReconnectionFailed?.Invoke(currentReconnectAttempt);
        }

        /// <summary>
        /// Attempts a single reconnection with timeout
        /// </summary>
        private IEnumerator AttemptReconnection()
        {
            float startTime = Time.time;


            // Clean up existing connection
            if (RosConnectionManager != null)
            {
                DestroyImmediate(RosConnectionManager);
                RosConnectionManager = null;
            }

            // Create new connection
            RosConnectionManager = gameObject.AddComponent<RosConnector>();

            // Configure connection if we have settings
            if (!string.IsNullOrEmpty(_ROSAddress))
            {
                // Note: RosConnector doesn't expose these settings directly in ros-sharp
                // This would need to be customized based on the specific ros-sharp version
                ROSLogger.LogInfo($"Attempting to connect to {_ROSAddress}:{_ROSPort}", ROSLogger.CATEGORY_CONNECTION, this);
            }

            // Wait for connection with timeout
            while (Time.time - startTime < connectionTimeout)
            {
                try
                {
                    if (RosConnectionManager != null && RosConnectionManager.IsConnected.WaitOne(0))
                    {
                        yield break; // Connection successful
                    }
                }
                catch (Exception ex)
                {
                    ROSLogger.LogError($"Exception during reconnection attempt: {ex.Message}", ROSLogger.CATEGORY_CONNECTION, this);
                }

                yield return new WaitForSeconds(0.1f);
            }

            // Timeout reached
            ROSLogger.LogWarning($"Connection attempt timed out after {connectionTimeout} seconds", ROSLogger.CATEGORY_CONNECTION, this);
        }

        /// <summary>
        /// Reinitializes services and topics after successful reconnection
        /// </summary>
        private void ReinitializeAfterReconnection()
        {
            ROSLogger.LogInfo("Reinitializing services and topics after reconnection...", ROSLogger.CATEGORY_CONNECTION, this);
            
            // Reinitialize services
            foreach (var service in ServiceMap.Values)
            {
                try
                {
                    service.Init(RosConnectionManager);
                    ROSLogger.LogInfo($"Reinitialized service: '{service.ServiceName}'", ROSLogger.CATEGORY_SERVICES, this);
                }
                catch (Exception ex)
                {
                    ROSLogger.LogError($"Failed to reinitialize service '{service.ServiceName}': {ex.Message}", ROSLogger.CATEGORY_SERVICES, this);
                }
            }
            
            // Reinitialize topics
            ReinitTopics();
            
            // Reinitialize publishers
            if (PublisherMap.Count > 0)
            {
                ReinitPublishers();
            }
        }

        /// <summary>
        /// Manually triggers a reconnection attempt
        /// </summary>
        public void ManualReconnect()
        {
            if (isReconnecting)
            {
                ROSLogger.LogWarning("Reconnection already in progress", ROSLogger.CATEGORY_CONNECTION, this);
                return;
            }
            
            ROSLogger.LogInfo("Manual reconnection triggered", ROSLogger.CATEGORY_CONNECTION, this);
            StartReconnection();
        }

        /// <summary>
        /// Stops the auto-reconnection process
        /// </summary>
        public void StopReconnection()
        {
            if (reconnectCoroutine != null)
            {
                StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = null;
            }
            
            isReconnecting = false;
            currentReconnectAttempt = 0;
            ROSLogger.LogInfo("Auto-reconnection stopped", ROSLogger.CATEGORY_CONNECTION, this);
        }

        /// <summary>
        /// Gets or sets the auto-reconnect enabled state
        /// </summary>
        public bool AutoReconnectEnabled
        {
            get => enableAutoReconnect;
            set
            {
                enableAutoReconnect = value;
                if (!value)
                {
                    StopReconnection();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of reconnection attempts
        /// </summary>
        public int MaxReconnectAttempts
        {
            get => maxReconnectAttempts;
            set => maxReconnectAttempts = Mathf.Max(0, value);
        }

        /// <summary>
        /// Gets or sets the reconnection interval in seconds
        /// </summary>
        public float ReconnectInterval
        {
            get => reconnectInterval;
            set => reconnectInterval = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// Gets or sets the connection timeout in seconds
        /// </summary>
        public float ConnectionTimeout
        {
            get => connectionTimeout;
            set => connectionTimeout = Mathf.Max(1.0f, value);
        }


        

        // This allows for a centralized control of the connection and connection errors in case of disconnects
        public RosConnector GetROSConnection(bool reinitPublishers = false)
        {
            // If we're in reconnection process, don't interfere
            if (isReconnecting)
            {
                return RosConnectionManager;
            }
            
            if (RosConnectionManager != null)
            {
                if (!RosConnectionManager.IsConnected.WaitOne(0))
                {
                    // Connection lost - will be handled by the monitoring coroutine
                    if (enableAutoReconnect && !isReconnecting)
                    {
                        ROSLogger.LogWarning("Connection lost detected in GetROSConnection", ROSLogger.CATEGORY_CONNECTION, this);
                        // The monitoring coroutine will handle the reconnection
                    }
                    else
                    {
                        ROSLogger.LogWarning("Connection lost. Auto-reconnect is disabled.", ROSLogger.CATEGORY_CONNECTION, this);
                    }
                }
            }
            else
            {
                RosConnectionManager = FindFirstObjectByType<RosConnector>();
                ROSLogger.LogInfo("Getting RosConnector instance and connecting to ROS", ROSLogger.CATEGORY_CONNECTION, this);
                if (RosConnectionManager == null)
                {
                    ROSLogger.LogInfo("No RosConnector found in scene. Creating new one.", ROSLogger.CATEGORY_CONNECTION, this);
                    RosConnectionManager = gameObject.AddComponent<RosConnector>();
                }

                if (reinitPublishers && PublisherMap.Count > 0)
                {
                    ROSLogger.LogInfo("Reinitialising publishers...", ROSLogger.CATEGORY_TOPICS, this);
                    ReinitPublishers();
                }
                
                // Initialize topics when connection is established
                ReinitTopics();
            }

            return RosConnectionManager;
        }

        /// <summary>
        /// Reinitializes all registered topics after connection
        /// </summary>
        private void ReinitTopics()
        {
            foreach (var topic in TopicMap.Values)
            {
                topic.Init();
            }
        }


        

        private void RegisterPublisherAction(string publisherName, Type publisherType)
        {
            ROSLogger.LogInfo($"Registering publisher '{publisherName}' with type {publisherType.Name}", ROSLogger.CATEGORY_TOPICS, this);

            // Ensure we have a valid connection
            if (RosConnectionManager == null || !RosConnectionManager.IsConnected.WaitOne(0))
            {
                ROSLogger.LogWarning($"Cannot register publisher '{publisherName}', not connected to ROS.", ROSLogger.CATEGORY_TOPICS, this);
                return;
            }

            // Use reflection to call the generic Advertise method
            var advertiseMethod = typeof(RosSocket).GetMethod("Advertise").MakeGenericMethod(publisherType);
            advertiseMethod.Invoke(RosConnectionManager.RosSocket, new object[] { publisherName });
            
            PublisherMap[publisherName] = publisherType;
        }
        
        void ReinitPublishers()
        {
            ROSLogger.LogInfo("Reinitializing publishers...", ROSLogger.CATEGORY_TOPICS, this);
            
            if (RosConnectionManager == null || !RosConnectionManager.IsConnected.WaitOne(0))
            {
                ROSLogger.LogWarning("Cannot reinitialize publishers, not connected to ROS.", ROSLogger.CATEGORY_TOPICS, this);
                return;
            }

            foreach (var publisher in PublisherMap)
            {
                try
                {
                    var advertiseMethod = typeof(RosSocket).GetMethod("Advertise").MakeGenericMethod(publisher.Value);
                    advertiseMethod.Invoke(RosConnectionManager.RosSocket, new object[] { publisher.Key });
                    ROSLogger.LogInfo($"Reinitialized publisher: '{publisher.Key}'", ROSLogger.CATEGORY_TOPICS, this);
                }
                catch (Exception ex)
                {
                    ROSLogger.LogError($"Failed to reinitialize publisher '{publisher.Key}': {ex.Message}", ROSLogger.CATEGORY_TOPICS, this);
                }
            }
        }

        private Dictionary<string, Type> PublisherMap = new Dictionary<string, Type>();
        public void RegisterPublisher<T>(string publisherName) where T : Message
        {
            if (!RosConnectionManager.IsConnected.WaitOne(0))
            {
                ROSLogger.LogWarning("Cannot register publisher, not connected to ROS.", ROSLogger.CATEGORY_TOPICS, this);
            }
            else
            {
                RosConnectionManager.RosSocket.Advertise<T>(publisherName);
                PublisherMap.Add(publisherName, typeof(T));
            }
        }

        protected virtual void OnDestroy()
        {
            // Stop auto-reconnection
            StopReconnection();
            
            // Cleanup all topics when the manager is destroyed
            foreach (var topic in TopicMap.Values)
            {
                topic.Cleanup();
            }
            TopicMap.Clear();
            
            // Clear services
            ServiceMap.Clear();
            
            ROSLogger.LogInfo("Cleaned up all services and topics", ROSLogger.CATEGORY_MANAGER, this);
        }

        // EXAMPLE USAGE OF A SERVICE

        // [SerializeField]
        // Transform target;
        //
        // [ContextMenu("ExampleCall")]
        // void ExampleCall()
        // {
        //
        //     var serv = ServiceMap["xarm_pose_plan"] as ROSService<PlanPoseRequest, PlanPoseResponse>;
        //
        //     var request = new PlanPoseRequest();
        //
        //     // Place Pose
        //     request.target = new PoseMsg
        //     {
        //         position = (target.position).To<FLU>(),
        //         orientation = Quaternion.identity.To<FLU>()
        //     };
        //
        //     serv.Call(request, ExampleCallback);
        // }
        //
        //
        // void ExampleCallback(PlanPoseResponse resp)
        // {
        //     Debug.Log("Callback from plan pose!!");
        //     if (resp.success)
        //     {
        //         var serv = ServiceMap["xarm_exec_plan"];
        //         serv.Call();
        //     }
        // }

    }
}
