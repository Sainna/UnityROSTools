using System.IO;
using UnityEngine;
using UnityEditor;

// ensure class initializer is called whenever scripts recompile
[InitializeOnLoadAttribute]
public static class RefreshServiceMessagesOnPlay
{
    // register an event handler when the class is initialized
    static RefreshServiceMessagesOnPlay()
    {
        // EditorApplication.playModeStateChanged += EnumListAttributeDrawer.PopulateTypeNames;

        if (File.Exists(ServiceMessageListAttributeDrawer.TYPE_PATHS))
        {
            ServiceMessageListAttributeDrawer.LoadMessageTypes();   
        }
        else
        {
            Debug.Log("Populating");
            ServiceMessageListAttributeDrawer.PopulateTypeNames();
        }
    }
    
    [MenuItem("ROS Tools/Refresh enum types")]
    static void Init()
    {
        ServiceMessageListAttributeDrawer.PopulateTypeNames();
    }
    
    

}