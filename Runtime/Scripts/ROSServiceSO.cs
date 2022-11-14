using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEngine;


[CreateAssetMenu(fileName = "ROSServiceCaller", menuName = "ScriptableObjects/ROS/Service caller", order = 1)]
public class ROSServiceSO : ScriptableObject
{
    //todo: custom editor
    [Serializable]
    public class ROSServiceInfo
    {
        [SerializeField] public string ServiceName;
        [SerializeField, ServiceMessageList] private string MessageType = null;
        [UsedImplicitly] public string Comment = "";
        [SerializeReference] private Message DefaultRequest = null;

        public ROSService InitService()
        {
            string respType = MessageType;
            respType = respType.Remove(respType.LastIndexOf("Request", StringComparison.Ordinal));
            respType += "Response";
        
            return (ROSService)typeof(ROSServiceFactory)
                .GetMethod("CreateROSService")
                ?.MakeGenericMethod(Type.GetType(MessageType), Type.GetType(respType))
                .Invoke(null, new object[] { ServiceName, DefaultRequest });
        }
    }


    [SerializeField] private List<ROSServiceInfo> _ROSServices = new List<ROSServiceInfo>();

    public List<ROSServiceInfo> ROSServicesInfos => _ROSServices;
}
