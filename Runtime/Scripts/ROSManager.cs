using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using RosSharp.RosBridgeClient;
using Sainna.Utils;
using UnityEngine;



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

            
            RosConnectionManager = GetROSConnection( false);
            
            Init();
        }

        private RosConnector RosConnectionManager;
        [SerializeField] private string _ROSAdress;
        [SerializeField] private int _ROSPort;

        [SerializeField]
        // [OnSerializedFieldChangedCall("Init")]
        private ROSServiceSO[] Services;

        private Dictionary<string, ROSService> ServiceMap = new Dictionary<string, ROSService>();

        public ROSService GetService(string serviceName) => ServiceMap[serviceName];
        public string[] GetServices() => ServiceMap.Keys.ToArray();


        protected virtual void InitServiceConnection(ROSService service, RosConnector connection)
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
        public RosConnector GetROSConnection(bool reinitPublishers = false)
        {
            if (RosConnectionManager != null)
            {
                if (!RosConnectionManager.IsConnected.WaitOne(0))
                {
                    Debug.LogWarning("NOT IMPLEMENTED Reconnecting to ROS");
                    //RosConnection.Connect();
                }
                
                
            }
            else
            {
                RosConnectionManager = FindFirstObjectByType<RosConnector>();
                Debug.Log("Getting instance and connecting to ROS");
                if (RosConnectionManager == null)
                {
                    RosConnectionManager = gameObject.AddComponent<RosConnector>();
                }

                if (reinitPublishers && PublisherMap.Count > 0)
                {
                    Debug.Log("Reinitialising publishers...");
                    ReinitPublishers();
                }
            }

            return RosConnectionManager;
        }


        

        private void RegisterPublisherAction(string publisherName, string publisherType)
        {
            throw new NotImplementedException("RegisterPublisherAction is not implemented in ROSManager.");
            Debug.Log($"Registered {publisherName}");

            //RosConnectionManager.RosSocket.AddPublisher(publisherName, publisherType); 
            //PublisherMap.Add(publisherName, publisherType);
        }
        
        void ReinitPublishers()
        {
            throw new NotImplementedException("ReinitPublishers is not implemented in ROSManager.");
            foreach (var publisher in PublisherMap)
            {
                //RosConnection.AddPublisher(publisher.Key, publisher.Value);
            }
        }

        private Dictionary<string, Type> PublisherMap = new Dictionary<string, Type>();
        public void RegisterPublisher<T>(string publisherName) where T : Message
        {
            if (!RosConnectionManager.IsConnected.WaitOne(0))
            {
                Debug.LogWarning("ROSManager: Cannot register publisher, not connected to ROS.");
            }
            else
            {
                RosConnectionManager.RosSocket.Advertise<T>(publisherName);
                PublisherMap.Add(publisherName, typeof(T));
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
