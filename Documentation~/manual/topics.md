# Topic management

## Overview

Topic management in ROS Tools follows the same plug-and-play philosophy as services but without requiring Scriptable Objects. Topics can be created dynamically at runtime and are automatically managed by the `ROSManager`.

## Creating Publishers

Publishers are used to send messages to ROS topics. Creating a publisher is very simple:

```csharp
void Start()
{
    // Get the ROS Manager
    ROSManager rosManager = ROSManager.GetOrCreateInstance();
    
    // Create a publisher for String messages
    ROSPublisher<String> publisher = rosManager.CreatePublisher<String>("/my_topic");
    
    // Publish a message
    var message = new String();
    message.data = "Hello ROS!";
    publisher.Publish(message);
}
```

## Creating Subscribers

Subscribers are used to receive messages from ROS topics:

```csharp
void Start()
{
    // Get the ROS Manager
    ROSManager rosManager = ROSManager.GetOrCreateInstance();
    
    // Create a subscriber with a callback function
    ROSSubscriber<String> subscriber = rosManager.CreateSubscriber<String>("/incoming_topic", OnMessageReceived);
}

void OnMessageReceived(String message)
{
    Debug.Log($"Received: {message.data}");
}
```

## Dynamic Topic Management

Topics can be created and destroyed at runtime:

```csharp
public class TopicController : MonoBehaviour
{
    private ROSManager rosManager;
    private ROSPublisher<String> dynamicPublisher;
    
    void Start()
    {
        rosManager = ROSManager.GetOrCreateInstance();
    }
    
    [ContextMenu("Create Publisher")]
    void CreatePublisher()
    {
        dynamicPublisher = rosManager.CreatePublisher<String>("/dynamic_topic");
    }
    
    [ContextMenu("Remove Publisher")]
    void RemovePublisher()
    {
        if (dynamicPublisher != null)
        {
            rosManager.RemoveTopic("/dynamic_topic");
            dynamicPublisher = null;
        }
    }
    
    [ContextMenu("Publish Message")]
    void PublishMessage()
    {
        if (dynamicPublisher != null && dynamicPublisher.IsActive)
        {
            var message = new String { data = "Dynamic message!" };
            dynamicPublisher.Publish(message);
        }
    }
}
```

## Complete Example

Here's a complete example showing both publisher and subscriber usage:

```csharp
using RosSharp.RosBridgeClient.MessageTypes.Std;
using UnityEngine;

public class ROSTopicExample : MonoBehaviour
{
    [SerializeField] private string publisherTopicName = "/unity/hello";
    [SerializeField] private string subscriberTopicName = "/ros/world";
    [SerializeField] private float publishInterval = 1.0f;
    
    private ROSManager rosManager;
    private ROSPublisher<String> stringPublisher;
    private ROSSubscriber<String> stringSubscriber;
    private float lastPublishTime;
    private int messageCounter = 0;

    void Start()
    {
        // Get or create the ROS Manager
        rosManager = ROSManager.GetOrCreateInstance();

        // Create a publisher - very simple, no ScriptableObject needed
        stringPublisher = rosManager.CreatePublisher<String>(publisherTopicName);

        // Create a subscriber with callback - also very simple
        stringSubscriber = rosManager.CreateSubscriber<String>(subscriberTopicName, OnStringMessageReceived);
    }

    void Update()
    {
        // Publish a message every publishInterval seconds
        if (Time.time - lastPublishTime > publishInterval)
        {
            PublishMessage();
            lastPublishTime = Time.time;
        }
    }

    void PublishMessage()
    {
        if (stringPublisher != null && stringPublisher.IsActive)
        {
            var message = new String();
            message.data = $"Hello from Unity! Message #{messageCounter++}";
            
            stringPublisher.Publish(message);
            Debug.Log($"Published: {message.data}");
        }
    }

    void OnStringMessageReceived(String message)
    {
        Debug.Log($"Received message: {message.data}");
    }
}
```

## Best Practices

### Lifecycle Management

- Topics are automatically cleaned up when the `ROSManager` is destroyed
- You can manually remove topics using `rosManager.RemoveTopic(topicName)`
- Check `IsActive` property before publishing to ensure the topic is connected

### Error Handling

- Publishers and subscribers will automatically handle connection issues
- Topics will be reinitialized when the ROS connection is restored
- Check the Unity console for connection status and error messages

### Performance Considerations

- Reuse publishers and subscribers when possible instead of creating new ones frequently
- Use appropriate publish intervals to avoid overwhelming the ROS network
- Consider message size when designing your communication patterns

## Available Message Types

You can use any ROS message type that is available in your project:

- Standard ROS messages (e.g., `String`, `Int32`, `Float64`)
- Geometry messages (e.g., `Vector3`, `Quaternion`, `Pose`)
- Custom message types generated by ros-sharp

The type system is the same as used for services, and message types are automatically discovered from your assemblies.
