using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using RosSharp.RosBridgeClient;
using UnityEngine;

namespace Sainna.Robotics.ROSTools
{
    [CreateAssetMenu(fileName = "ROSServiceCaller", menuName = "ScriptableObjects/ROS/Service caller", order = 1)]
    public class ROSServiceSO : ScriptableObject
    {


        //todo: custom editor
        // Custom editor should refresh service list
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
                var reqIdx = respType.LastIndexOf("Request", StringComparison.Ordinal);
                respType = respType.Remove(reqIdx, "Request".Length);
                respType = respType.Insert(reqIdx, "Response");
                
                


                // TODO: add assembly to type name
                return (ROSService)typeof(ROSServiceFactory)
                    .GetMethod("CreateROSService")
                    ?.MakeGenericMethod(Type.GetType(MessageType), Type.GetType(respType))
                    .Invoke(null, new object[] { ServiceName, DefaultRequest });
            }
        }

        

        [SerializeField] private List<ROSServiceInfo> _ROSServices = new List<ROSServiceInfo>();

        public List<ROSServiceInfo> ROSServicesInfos => _ROSServices;
    }
}