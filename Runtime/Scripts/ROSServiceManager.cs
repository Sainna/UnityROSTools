using System.Collections.Generic;
using System.Linq;
using Sainna.Utils;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace Sainna.Robotics.ROSTools
{
    public class ROSServiceManager : MonoBehaviour
    {
        // Singleton behavior
        static ROSServiceManager _instance;

        public static ROSServiceManager GetOrCreateInstance()
        {
            if (_instance == null)
            {
                // Prefer to use the ROSConnection in the scene, if any
                _instance = FindObjectOfType<ROSServiceManager>();
                if (_instance != null)
                    return _instance;

                GameObject instance = new GameObject("ROSServiceManager");
                _instance = instance.AddComponent<ROSServiceManager>();
                _instance.Init();

            }
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
        }

        private ROSConnection RosConnection;

        [SerializeField]
        [OnSerializedFieldChangedCall("Init")]
        private ROSServiceSO[] Services;

        private Dictionary<string, ROSService> ServiceMap = new Dictionary<string, ROSService>();

        public ROSService GetService(string serviceName) => ServiceMap[serviceName];
        public string[] GetServices() => ServiceMap.Keys.ToArray();


        protected virtual void InitServiceConnection(ROSService service, ROSConnection connection)
        {
            service.Init(connection);
        }

        protected virtual void InitAllServices()
        {
            foreach (var service in ServiceMap)
            {
                InitServiceConnection(service.Value, RosConnection);
            }
        }


        // In case some services need special initialisation processes (i.e. detour service for empty calls)
        protected virtual void ServicePreProcess(ROSServiceSO.ROSServiceInfo serviceInfo, ROSService serviceAbst)
        {

        }

        
        // needs to be public to be called by the OnSerializedFieldChanged attribute
        public void Init()
        {
            // Init manager: Get all the services and create a representation as ROSService in memory
            RosConnection = GetROSConnection();
            
            ServiceMap.Clear();

            if (Services == null)
                return;
            
            foreach (var serviceSo in Services)
            {
                foreach (var serviceInfo in serviceSo.ROSServicesInfos)
                {
                    var serviceAbst = serviceInfo.InitService();

                    string serviceName = serviceInfo.ServiceName;

                    ServicePreProcess(serviceInfo, serviceAbst);

                    ServiceMap.Add(serviceName, serviceAbst);
                }
            }

            InitAllServices();
        }

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }


        // This allows for a centralized gestion of the connection and connection errors in case of disconnects
        public ROSConnection GetROSConnection()
        {
            if (RosConnection && RosConnection.HasConnectionThread)
            {
                if (RosConnection.HasConnectionError)
                    RosConnection.Connect();
            }
            else
            {
                // Re-init services if connection lost
                RosConnection = ROSConnection.GetOrCreateInstance();
                if (ServiceMap.Count > 0)
                {
                    InitAllServices();
                }

            }

            return RosConnection;
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
