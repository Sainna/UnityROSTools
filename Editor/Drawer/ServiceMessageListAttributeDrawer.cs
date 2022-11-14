using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Reflection;
using System.Threading.Tasks.Sources;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ServiceMessageListAttribute))]
public class ServiceMessageListAttributeDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public static string[] TypeNames = null;
    
    public static readonly string TYPE_PATHS = "Assets/ROSTools/Editor/Resources/ServiceMessages.json";


    public static void PopulateTypeNames()
    {

        // foreach (var VARIABLE in CompilationPipeline.GetAssemblies())
        // {
        //     Debug.Log(VARIABLE.outputPath);
        //     Debug.Log(VARIABLE.name);
        // }
        //
        var query = Assembly.Load("Assembly-CSharp")
            .GetTypes()
            .Where(t => t.Namespace?.Contains("RosMessageTypes") == true)
            .Where(t => t.Name.Contains("Request"))
            .Select(typeinfo => typeinfo.FullName );
        var customTypes = query.ToArray();
        
        var stdQuery = Assembly.Load("Unity.Robotics.ROSTCPConnector.Messages")
            .GetTypes()
            .Where(t => t.Namespace?.Contains("RosMessageTypes") == true)
            .Where(t => t.Name.Contains("Request"))
            .Select(typeinfo => typeinfo.FullName );
        var stdTypes = stdQuery.ToArray();
        
        TypeNames = customTypes.Concat(stdTypes).ToArray();
        SaveEnumTypes();
    }
    
    public static void SaveEnumTypes()
    {
        string str = JsonHelper.ToJson(TypeNames, true);
        Directory.CreateDirectory(Path.GetDirectoryName(TYPE_PATHS));
        using (FileStream fs = new FileStream(TYPE_PATHS, FileMode.Create)){
            using (StreamWriter writer = new StreamWriter(fs)){
                writer.Write(str);
            }
        }
        AssetDatabase.Refresh ();
    }
    
    public static void LoadMessageTypes(){
        TypeNames = JsonHelper.FromJson<string>(File.ReadAllText(TYPE_PATHS));
    }
    
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        string curr = property.stringValue;
        if (TypeNames != null && TypeNames.Length > 0)
        {
            int currIndex = Array.FindIndex(TypeNames, s => s.Equals(curr));
            if (currIndex == -1) currIndex = 0;
            property.stringValue = TypeNames[EditorGUI.Popup(position, "Message type", currIndex, TypeNames)];
        }
        EditorGUI.EndProperty();
    }
}