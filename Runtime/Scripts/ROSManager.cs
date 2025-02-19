using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NativeWebSocket;
using ROSBridgeLib;
using Sainna.Utils;
using SimpleJSON;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;


public static class RosMsgExtension
{
    
    public static string ToJSON<T>(this T msg) where T : Message
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!first)
                sb.Append(",");
            if(field.FieldType.IsPrimitive)
                sb.AppendLine($"\"{field.Name}\": {field.GetValue(msg)}");
            first = false;
        }
        sb.Append("}");
        return sb.ToString();
    }
    
    public static T MessageFromJSON<T>(this JSONNode msg) where T : Message
    {
        return null;
    }
}

namespace Sainna.Robotics.ROSTools
{
    public class ROSManager : MonoBehaviour
    {
        // Singleton behavior
        protected static ROSManager _instance;

        public static ROSManager GetOrCreateInstance()
        {
            if (_instance == null)
            {
                // Prefer to use the ROSConnection in the scene, if any
                _instance = FindObjectOfType<ROSManager>();
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

            
            RosConnection = GetROSConnection( false);
            Init();
        }

        private ROSBridgeConnectionsManager RosConnectionManager;
        private ROSBridgeWebSocketConnection RosConnection;
        [SerializeField] private string _ROSAdress;
        [SerializeField] private int _ROSPort;

        [SerializeField]
        // [OnSerializedFieldChangedCall("Init")]
        private ROSServiceSO[] Services;

        private Dictionary<string, ROSService> ServiceMap = new Dictionary<string, ROSService>();

        public ROSService GetService(string serviceName) => ServiceMap[serviceName];
        public string[] GetServices() => ServiceMap.Keys.ToArray();


        protected virtual void InitServiceConnection(ROSService service, ROSBridgeWebSocketConnection connection)
        {
            Debug.Log($"Registering {service.ServiceName}");
            service.Init(connection);
        }


        // In case some services need special initialisation processes (i.e. detour service for empty calls)
        protected virtual void ServicePreProcess(ROSServiceSO.ROSServiceInfo serviceInfo, ROSService serviceAbst, ref string serviceName)
        {
            
        }
        
        public delegate void ROSConnectionDelegate();
        public event ROSConnectionDelegate ROSConnected; // event

        // IEnumerator WaitForRosConnection()
        // {
        //     
        //     Debug.Log("[ROSManager] Waiting for ROS");
        //     WaitingOnRosConnection = true;
        //     yield return new WaitWhile(() => RosConnection.HasConnectionError);
        //     Debug.Log("[ROSManager] Connected to ROS");
        //     WaitingOnRosConnection = false;
        //
        //     ROSConnected?.Invoke();
        //
        //     ROSConnected = null;
        // }

        public bool WaitingOnRosConnection { get; private set; } = false;
        // needs to be public to be called by the OnSerializedFieldChanged attribute
        public void Init()
        {
            Debug.Log("[ROSManager] Initialising services");
            
            // Init manager: Get all the services and create a representation as ROSService in memory
            ServiceMap.Clear();

            if (Services == null)
            {
                Debug.Log("[ROS Manager] No services registered");
                return;
            }
                
            
            foreach (var serviceSo in Services)
            {
                foreach (var serviceInfo in serviceSo.ROSServicesInfos)
                {
                    var serviceAbst = serviceInfo.InitService();

                    string serviceName = serviceInfo.ServiceName;
                    ServicePreProcess(serviceInfo, serviceAbst, ref serviceName);

                    ServiceMap.Add(serviceName, serviceAbst);
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }


        // This allows for a centralized control of the connection and connection errors in case of disconnects
        public ROSBridgeWebSocketConnection GetROSConnection(bool reinitPublishers = false)
        {
            if (RosConnection != null)
            {
                if (RosConnection.GetWSState() == WebSocketState.Closed)
                {
                    Debug.Log("Reconnecting to ROS");
                    RosConnection.Connect();
                }
                
                
            }
            else
            {
                var ROSBridgeManager = ROSBridgeConnectionsManager.Instance;
                Debug.Log("Getting instance and connecting to ROS");
                ROSBridgeManager.GetConnection(_ROSAdress, _ROSPort);

                if (reinitPublishers && PublisherMap.Count > 0)
                {
                    Debug.Log("Reinitialising publishers...");
                    ReinitPublishers();
                }
            }

            return RosConnection;
        }


        

        private void RegisterPublisherAction(string publisherName, string publisherType)
        {
            Debug.Log($"Registered {publisherName}");
            RosConnection.AddPublisher(publisherName, publisherType); 
            PublisherMap.Add(publisherName, publisherType);
        }
        
        void ReinitPublishers()
        {
            foreach (var publisher in PublisherMap)
            {
                RosConnection.AddPublisher(publisher.Key, publisher.Value);
            }
        }

        private Dictionary<string, string> PublisherMap = new Dictionary<string, string>();
        public void RegisterPublisher<T>(string publisherName) where T : Message
        {
            if (RosConnection == null)
            {
                RosConnection = GetROSConnection();
                if(RosConnection.GetWSState() != WebSocketState.Open)
                    RosConnection.OnSocketOpen(() => { RegisterPublisherAction(publisherName, 
                        MessageRegistry.GetRosMessageName<T>());  });
                else
                {
                    RegisterPublisherAction(publisherName, MessageRegistry.GetRosMessageName<T>());
                }
            }
            else if (RosConnection.GetWSState() == WebSocketState.Closed)
            {
                if (!WaitingOnRosConnection)
                {
                    RosConnection.OnSocketOpen(() => { ROSConnected?.Invoke(); ROSConnected = null; });
                    RosConnection = GetROSConnection();
                }
            }
            else
            {
                RegisterPublisherAction(publisherName, MessageRegistry.GetRosMessageName<T>());
            }
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
