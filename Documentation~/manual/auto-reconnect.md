# Auto-Reconnect Feature

The ROS Tools package includes a robust auto-reconnect feature that automatically handles connection failures and attempts to restore the connection without manual intervention.

## Overview

The auto-reconnect feature provides:

- **Automatic connection monitoring**: Continuously monitors the ROS connection status
- **Configurable retry attempts**: Set maximum number of reconnection attempts (or infinite)
- **Timeout handling**: Configurable timeout for each connection attempt
- **Event-driven**: Provides events for connection state changes
- **Manual control**: Ability to manually trigger or stop reconnection

## Configuration

### Inspector Settings

The ROSManager component exposes the following auto-reconnect settings in the Unity Inspector:

- **Enable Auto Reconnect**: Enable/disable automatic reconnection
- **Max Reconnect Attempts**: Maximum number of attempts (0 = infinite)
- **Reconnect Interval**: Time to wait between attempts (seconds)
- **Connection Timeout**: Timeout for each connection attempt (seconds)

### Programmatic Configuration

You can also configure auto-reconnect settings via code:

```csharp
var rosManager = ROSManager.GetOrCreateInstance();

// Enable/disable auto-reconnect
rosManager.AutoReconnectEnabled = true;

// Set maximum attempts (0 = infinite)
rosManager.MaxReconnectAttempts = 5;

// Set interval between attempts
rosManager.ReconnectInterval = 3.0f;

// Set connection timeout
rosManager.ConnectionTimeout = 10.0f;
```

## Events

The auto-reconnect system provides several events to monitor connection status:

### Connection Events

```csharp
var rosManager = ROSManager.GetOrCreateInstance();

// Triggered when connection is established
rosManager.ROSConnected += () => {
    Debug.Log("ROS Connected!");
};

// Triggered when connection is lost
rosManager.ROSDisconnected += () => {
    Debug.Log("ROS Disconnected!");
};
```

### Reconnection Events

```csharp
// Triggered for each reconnection attempt
rosManager.ROSReconnectionAttempt += (attemptNumber, maxAttempts) => {
    Debug.Log($"Reconnection attempt {attemptNumber}/{maxAttempts}");
};

// Triggered when all reconnection attempts fail
rosManager.ROSReconnectionFailed += (totalAttempts) => {
    Debug.LogError($"Failed to reconnect after {totalAttempts} attempts");
};
```

## Manual Control

### Manual Reconnection

You can manually trigger a reconnection attempt:

```csharp
var rosManager = ROSManager.GetOrCreateInstance();

// Manually trigger reconnection
rosManager.ManualReconnect();
```

### Stop Reconnection

You can stop an ongoing reconnection process:

```csharp
// Stop auto-reconnection
rosManager.StopReconnection();
```

## Status Monitoring

### Connection Status

```csharp
var rosManager = ROSManager.GetOrCreateInstance();

// Check if currently connected
bool isConnected = rosManager.IsConnected;

// Check if currently attempting to reconnect
bool isReconnecting = rosManager.IsReconnecting;

// Check if auto-reconnect is enabled
bool autoReconnectEnabled = rosManager.AutoReconnectEnabled;
```

## Complete Example

Here's a complete example showing how to use the auto-reconnect features:

```csharp
using UnityEngine;
using Sainna.Robotics.ROSTools;

public class ROSConnectionManager : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private bool enableAutoReconnect = true;
    [SerializeField] private int maxRetries = 5;
    [SerializeField] private float retryInterval = 3.0f;
    [SerializeField] private float connectionTimeout = 10.0f;

    private ROSManager rosManager;

    void Start()
    {
        // Get ROS Manager instance
        rosManager = ROSManager.GetOrCreateInstance();

        // Configure auto-reconnect settings
        rosManager.AutoReconnectEnabled = enableAutoReconnect;
        rosManager.MaxReconnectAttempts = maxRetries;
        rosManager.ReconnectInterval = retryInterval;
        rosManager.ConnectionTimeout = connectionTimeout;

        // Subscribe to connection events
        rosManager.ROSConnected += OnConnected;
        rosManager.ROSDisconnected += OnDisconnected;
        rosManager.ROSReconnectionAttempt += OnReconnectionAttempt;
        rosManager.ROSReconnectionFailed += OnReconnectionFailed;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (rosManager != null)
        {
            rosManager.ROSConnected -= OnConnected;
            rosManager.ROSDisconnected -= OnDisconnected;
            rosManager.ROSReconnectionAttempt -= OnReconnectionAttempt;
            rosManager.ROSReconnectionFailed -= OnReconnectionFailed;
        }
    }

    private void OnConnected()
    {
        Debug.Log("Successfully connected to ROS!");
        // Your connection logic here
    }

    private void OnDisconnected()
    {
        Debug.LogWarning("Lost connection to ROS!");
        // Your disconnection logic here
    }

    private void OnReconnectionAttempt(int attempt, int maxAttempts)
    {
        string maxStr = maxAttempts == 0 ? "∞" : maxAttempts.ToString();
        Debug.Log($"Reconnection attempt {attempt}/{maxStr}");
        // Your reconnection attempt logic here
    }

    private void OnReconnectionFailed(int totalAttempts)
    {
        Debug.LogError($"Failed to reconnect after {totalAttempts} attempts!");
        // Your failure handling logic here
    }

    // Context menu actions for testing
    [ContextMenu("Manual Reconnect")]
    void TriggerManualReconnect()
    {
        rosManager.ManualReconnect();
    }

    [ContextMenu("Stop Reconnection")]
    void StopReconnection()
    {
        rosManager.StopReconnection();
    }

    [ContextMenu("Toggle Auto-Reconnect")]
    void ToggleAutoReconnect()
    {
        rosManager.AutoReconnectEnabled = !rosManager.AutoReconnectEnabled;
        Debug.Log($"Auto-reconnect: {(rosManager.AutoReconnectEnabled ? "Enabled" : "Disabled")}");
    }
}
```

## Editor Tools

### ROS Service Caller Window

The ROS Service Caller window (`ROS Tools → Service Caller`) includes auto-reconnect controls:

- **Connection Status**: Shows current connection and reconnection status
- **Manual Controls**: Buttons to manually trigger reconnection or stop it
- **Settings Panel**: Configure auto-reconnect parameters in real-time

### Inspector Integration

The ROSManager component in the Inspector provides:

- **Auto-Reconnect Settings**: Configure all parameters directly in the Inspector
- **Real-time Status**: Visual indicators for connection and reconnection status
- **Context Menu Actions**: Right-click actions for testing reconnection

## Best Practices

### Configuration Recommendations

- **Max Attempts**: Set to 3-5 for production, 0 (infinite) for development
- **Retry Interval**: 3-5 seconds is usually sufficient
- **Connection Timeout**: 10-15 seconds for typical networks

### Error Handling

```csharp
// Always check connection status before critical operations
if (rosManager.IsConnected)
{
    // Perform ROS operations
    publisher.Publish(message);
}
else
{
    // Queue operations or handle gracefully
    Debug.LogWarning("ROS not connected, queuing message");
}
```

### Performance Considerations

- The connection monitor runs every second by default
- Reconnection attempts use coroutines for non-blocking operation
- Services and topics are automatically reinitialized after successful reconnection

## Troubleshooting

### Common Issues

1. **Rapid Reconnection Loops**: Increase reconnect interval if experiencing rapid connection/disconnection cycles
2. **Timeout Too Short**: Increase connection timeout for slow networks
3. **Infinite Reconnection**: Set max attempts to prevent infinite loops in production

### Debug Information

Enable verbose logging to monitor reconnection behavior:

```csharp
// Monitor reconnection status
Debug.Log($"Connection: {rosManager.IsConnected}");
Debug.Log($"Reconnecting: {rosManager.IsReconnecting}");
Debug.Log($"Auto-reconnect: {rosManager.AutoReconnectEnabled}");
```

## Integration with Services and Topics

The auto-reconnect feature seamlessly integrates with the existing service and topic management:

- **Services**: Automatically reinitialized after reconnection
- **Topics**: Publishers and subscribers are restored automatically
- **State Preservation**: All registered services and topics maintain their configuration

This ensures that your ROS communication continues seamlessly after connection restoration without requiring manual intervention.
