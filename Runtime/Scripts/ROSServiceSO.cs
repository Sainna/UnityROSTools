using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using RosSharp.RosBridgeClient;
using Sainna.Robotics.ROSTools.Logging;
using UnityEngine;

namespace Sainna.Robotics.ROSTools
{
    [CreateAssetMenu(fileName = "ROSServiceCaller", menuName = "ROS Tools/Service caller", order = 1)]
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
                if (string.IsNullOrEmpty(MessageType) || string.IsNullOrEmpty(ServiceName))
                {
                    ROSLogger.LogError($"Service '{ServiceName}' has invalid configuration. MessageType or ServiceName is empty.", ROSLogger.CATEGORY_SERVICES);
                    return null;
                }

                string respType = MessageType;
                var reqIdx = respType.LastIndexOf("Request", StringComparison.Ordinal);
                if (reqIdx == -1)
                {
                    ROSLogger.LogError($"Invalid message type format for service '{ServiceName}'. Expected format to end with 'Request'.", ROSLogger.CATEGORY_SERVICES);
                    return null;
                }
                
                respType = respType.Remove(reqIdx, "Request".Length);
                respType = respType.Insert(reqIdx, "Response");

                // Try to find the types in different assemblies
                Type reqType = FindType(MessageType);
                Type respType_obj = FindType(respType);

                if (reqType == null)
                {
                    ROSLogger.LogError($"Could not find request type: {MessageType}", ROSLogger.CATEGORY_SERVICES);
                    return null;
                }

                if (respType_obj == null)
                {
                    ROSLogger.LogError($"Could not find response type: {respType}", ROSLogger.CATEGORY_SERVICES);
                    return null;
                }

                try
                {
                    return (ROSService)typeof(ROSServiceFactory)
                        .GetMethod("CreateROSService")
                        ?.MakeGenericMethod(reqType, respType_obj)
                        .Invoke(null, new object[] { ServiceName, DefaultRequest });
                }
                catch (Exception ex)
                {
                    ROSLogger.LogError($"Failed to create service '{ServiceName}': {ex.Message}", ROSLogger.CATEGORY_SERVICES);
                    return null;
                }
            }

            private Type FindType(string typeName)
            {
                // Try different assembly combinations
                string[] assemblies = { 
                    "Assembly-CSharp", 
                    "siemens.ros-sharp.Runtime",
                    "moe.sainna.rostools"
                };

                foreach (string assembly in assemblies)
                {
                    Type type = Type.GetType($"{typeName}, {assembly}");
                    if (type != null)
                        return type;
                }

                // If not found, try without assembly qualifier
                return Type.GetType(typeName);
            }
        }

        

        [SerializeField] private List<ROSServiceInfo> _ROSServices = new List<ROSServiceInfo>();

        public List<ROSServiceInfo> ROSServicesInfos => _ROSServices;
    }
}