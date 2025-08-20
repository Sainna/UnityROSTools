# ROS Tools for Unity - Developer Guide

## Overview

This Unity package provides a streamlined way to work with ROS (Robot Operating System) in Unity projects. It follows a plug-and-play philosophy, making ROS integration as simple as possible.

## Key Components

### ROSManager
The central singleton that manages all ROS connections, services, and topics.

**Key Features:**
- Singleton pattern ensuring only one instance per scene
- Automatic connection management
- Service and topic lifecycle management
- Error handling and reconnection logic

### Service Management (ROSService)
Services use Scriptable Objects for configuration:

1. **ROSServiceSO**: Scriptable Object containing service definitions
2. **ROSService**: Runtime representation of a service
3. **ROSServiceFactory**: Factory for creating typed services

**Usage:**
```csharp
// Get service and call it
var service = ROSManager.GetOrCreateInstance().GetService("my_service") as ROSService<MyRequest, MyResponse>;
service.Call(new MyRequest(), (response) => Debug.Log("Response received"));
```

### Topic Management (ROSTopic)
Topics are created dynamically without Scriptable Objects:

1. **ROSPublisher<T>**: For publishing messages
2. **ROSSubscriber<T>**: For receiving messages
3. **ROSTopicFactory**: Factory for creating publishers/subscribers

**Usage:**
```csharp
var manager = ROSManager.GetOrCreateInstance();

// Create publisher
var publisher = manager.CreatePublisher<String>("/my_topic");
publisher.Publish(new String { data = "Hello ROS!" });

// Create subscriber
var subscriber = manager.CreateSubscriber<String>("/other_topic", (msg) => {
    Debug.Log($"Received: {msg.data}");
});
```

## Unity Best Practices Implemented

### 1. Singleton Pattern
- Thread-safe singleton implementation
- Automatic instance creation when needed
- Proper cleanup on destroy

### 2. Editor Integration
- Custom Property Drawers for service configuration
- Editor window for runtime testing
- Automatic type discovery and refresh

### 3. Error Handling
- Comprehensive null checks
- Graceful degradation when ROS is unavailable
- Clear error messages and warnings

### 4. Lifecycle Management
- Automatic cleanup of resources
- Connection state monitoring
- Graceful handling of connection loss

### 5. Inspector Friendly
- Serialized fields with tooltips
- Header groups for organization
- Validation in property setters

## Architecture Benefits

### Plug-and-Play Philosophy
- **Services**: Configure once in ScriptableObjects, use anywhere
- **Topics**: Create dynamically with one line of code
- **Connection**: Automatic management, no manual setup required

### Type Safety
- Generic type system ensures compile-time safety
- Automatic type discovery from assemblies
- Clear error messages for type mismatches

### Extensibility
- Virtual methods for customization
- Event system for connection state changes
- Factory pattern for easy extension

## Common Usage Patterns

### 1. Simple Publisher
```csharp
public class SimplePublisher : MonoBehaviour
{
    private ROSPublisher<String> publisher;
    
    void Start()
    {
        var manager = ROSManager.GetOrCreateInstance();
        publisher = manager.CreatePublisher<String>("/unity/messages");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            publisher.Publish(new String { data = "Hello from Unity!" });
        }
    }
}
```

### 2. Service Caller
```csharp
public class ServiceCaller : MonoBehaviour
{
    [SerializeField] private ROSServiceSO serviceConfig;
    
    void Start()
    {
        var manager = ROSManager.GetOrCreateInstance();
        // Services are automatically registered from serviceConfig
    }
    
    public void CallMyService()
    {
        var manager = ROSManager.GetOrCreateInstance();
        var service = manager.GetService("my_service");
        service.Call(); // Uses default request
    }
}
```

### 3. Dynamic Topic Management
```csharp
public class DynamicTopicManager : MonoBehaviour
{
    private ROSManager manager;
    private Dictionary<string, ROSPublisher<String>> publishers = new();
    
    void Start()
    {
        manager = ROSManager.GetOrCreateInstance();
    }
    
    public void CreateTopic(string topicName)
    {
        if (!publishers.ContainsKey(topicName))
        {
            publishers[topicName] = manager.CreatePublisher<String>(topicName);
        }
    }
    
    public void RemoveTopic(string topicName)
    {
        if (publishers.ContainsKey(topicName))
        {
            manager.RemoveTopic(topicName);
            publishers.Remove(topicName);
        }
    }
}
```

## Dependencies

- **ros-sharp**: Provides the underlying ROS communication layer
- **Unity 2022.3+**: Required Unity version
- **Newtonsoft.Json**: For message serialization (included with ros-sharp)

## Logging System

The package includes a centralized logging system using Unity's built-in Logger class. This provides professional-grade logging with categorization and configurable levels.

### Logging Configuration

#### Enable/Disable Logging

```csharp
using Sainna.Robotics.ROSTools.Logging;

// Disable all ROS logging
ROSLogger.SetLogEnabled(false);

// Re-enable logging (useful for debugging)
ROSLogger.SetLogEnabled(true);
```

#### Set Log Levels

Control which types of messages are displayed:

```csharp
using UnityEngine;
using Sainna.Robotics.ROSTools.Logging;

// Show only warnings and errors (production)
ROSLogger.SetLogLevel(LogType.Warning);

// Show all logs including info (development - default)
ROSLogger.SetLogLevel(LogType.Log);

// Show only errors (minimal logging)
ROSLogger.SetLogLevel(LogType.Error);

// Show only exceptions (critical only)
ROSLogger.SetLogLevel(LogType.Exception);
```

### Log Categories

All logging is categorized for easy filtering in the Unity Console:

- **`ROSConnection`**: Connection establishment, loss, and reconnection events
- **`ROSServices`**: Service registration, initialization, and calling
- **`ROSTopics`**: Topic creation, publishing, subscribing, and cleanup
- **`ROSManager`**: Manager lifecycle, initialization, and cleanup
- **`ROSEditor`**: Editor-specific operations and tools
- **`ROSExamples`**: Example script activities and demonstrations

### Log Format

All logs follow a consistent format:

```text
[ROS] [Category] Message (FileName.MethodName:LineNumber)
```

Example:

```text
[ROS] [ROSTopics] Created publisher for topic: '/unity/hello' (ROSManager.CreatePublisher:156)
```

### Using Logging in Custom Scripts

You can use the ROSLogger in your own scripts for consistency:

```csharp
using Sainna.Robotics.ROSTools.Logging;

public class MyRobotController : MonoBehaviour
{
    void Start()
    {
        // Log with a custom category
        ROSLogger.LogInfo("Robot controller initialized", "MyRobot");
        
        // Log with context object for click-to-select in console
        ROSLogger.LogWarning("Sensor data missing", "MyRobot", this);
    }
    
    void OnError()
    {
        // Log errors with full context
        ROSLogger.LogError("Critical robot failure detected", "MyRobot", this);
    }
}
```

### Production Recommendations

For production builds, consider:

1. **Disable Info Logs**: Use `ROSLogger.SetLogLevel(LogType.Warning)` to reduce console noise
2. **Disable All Logging**: Use `ROSLogger.SetLogEnabled(false)` for performance-critical applications
3. **Keep Error Logs**: Always keep error and warning logs enabled for debugging

### Debugging Tips

1. **Use Categories**: Filter Unity Console by `[ROSConnection]`, `[ROSTopics]`, etc.
2. **Context Objects**: Click on log entries with context to select the GameObject in the scene
3. **Call Stack**: Each log includes the calling method and line number for easy navigation
4. **Enable All Logs**: Use `ROSLogger.SetLogLevel(LogType.Log)` when debugging issues

## Editor Tools

### ROS Service Caller Window
Access via `ROS Tools → Service Caller`
- Test services without writing code
- Dynamic topic creation and management
- Connection status monitoring
- Real-time service and topic lists

### Message Type Refresh
Access via `ROS Tools → Refresh enum types`
- Refreshes available message types
- Automatically discovers custom message types
- Updates service configuration dropdowns

## Performance Considerations

1. **Connection Pooling**: Single connection shared across all services and topics
2. **Lazy Initialization**: Services and topics created only when needed
3. **Automatic Cleanup**: Resources cleaned up when GameObjects are destroyed
4. **Error Recovery**: Automatic reconnection and re-initialization

## Troubleshooting

### Common Issues

1. **"Service not found"**: Check service name spelling and ensure ROSServiceSO is configured
2. **"Connection lost"**: ROS bridge may be down, check ROS_IP and network connection
3. **"Type not found"**: Run `ROS Tools → Refresh enum types` to update available types
4. **"Multiple ROSManagers"**: Only one ROSManager should exist per scene

### Debug Tools

1. Unity Console shows detailed connection and operation logs
2. ROS Service Caller window shows real-time status
3. ROSManager inspector shows configured services and active topics

## Migration Guide

### From Previous Versions
- Service calling API remains the same
- Topic management is new - replace manual publisher/subscriber creation
- ROSServiceManager renamed to ROSManager

### Integration with Existing Projects
- Add ROSManager to scene
- Configure existing ROSServiceSO assets
- Replace manual topic management with ROSManager methods
