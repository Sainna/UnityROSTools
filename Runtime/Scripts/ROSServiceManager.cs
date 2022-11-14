using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class ROSServiceManager : MonoBehaviour
{
    private ROSConnection RosConnection;

    [SerializeField] private ROSServiceSO[] Services;

    private Dictionary<string, ROSService> ServiceMap = new Dictionary<string, ROSService>();

    public ROSService GetService(string serviceName) => ServiceMap[serviceName];
    public string[] GetServices() => ServiceMap.Keys.ToArray();


        [SerializeField]
    bool _DontDestroyGameobjectOnLoad = false;

    private static ROSServiceManager instance;
    public static ROSServiceManager Instance
    {
        get{
            if (instance == null) {
                Type t = typeof(ROSServiceManager);

                instance = (ROSServiceManager)FindObjectOfType (t);
                if (instance == null) {
                    Debug.LogWarning ($"No GameObject of type {t}");
                }
            }

            return instance;
        }
    }

    virtual protected void Awake(){
        CheckInstance();
    }

    protected bool CheckInstance(){
        if (instance == null) {
            instance = this;
            if(_DontDestroyGameobjectOnLoad)
                DontDestroyOnLoad(gameObject);
            return true;
        } else if (Instance == this) {
            return true;
        }
        if(_DontDestroyGameobjectOnLoad)
        {
            Destroy (gameObject);
        }
        else
        {
            Destroy (this);   
        }
        return false;
    }


    private void InitServiceConnection(ROSService service, ROSConnection connection)
    {
        service.Init(connection);
    }

    void InitAllServices()
    {
        foreach (var service in ServiceMap)
        {
            InitServiceConnection(service.Value, RosConnection);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RosConnection = GetROSConnection();

        foreach (var serviceSo in Services)
        {
            foreach (var serviceInfo in serviceSo.ROSServicesInfos)
            {
                var serviceAbst = serviceInfo.InitService();
                
                string serviceName = serviceInfo.ServiceName;

                ServiceMap.Add(serviceName, serviceAbst);
            }
        }

        InitAllServices();
    }


    public ROSConnection GetROSConnection()
    {
        if (RosConnection && RosConnection.HasConnectionThread)
        {
            if(RosConnection.HasConnectionError)
                RosConnection.Connect();
        }
        else
        {
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
