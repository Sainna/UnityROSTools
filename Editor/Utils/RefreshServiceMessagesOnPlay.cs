using System.IO;
using UnityEngine;
using UnityEditor;

namespace Sainna.Robotics.ROSTools.Editor
{
// ensure class initializer is called whenever scripts recompile
    [InitializeOnLoad]
    public static class RefreshServiceMessagesOnPlay
    {
        // register an event handler when the class is initialized
        static RefreshServiceMessagesOnPlay()
        {
            if (File.Exists(ServiceMessageListAttributeDrawer.TYPE_PATHS))
            {
                ServiceMessageListAttributeDrawer.LoadMessageTypes();
            }
            else
            {
                Debug.Log("[ROSTools] Populating type names...");
                ServiceMessageListAttributeDrawer.PopulateTypeNames();
            }
        }

        [MenuItem("ROS Tools/Refresh enum types")]
        static void Init()
        {
            ServiceMessageListAttributeDrawer.PopulateTypeNames();
        }
    }
}