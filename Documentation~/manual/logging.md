# Logging System

The ROS Tools package includes a comprehensive logging system built on Unity's native Logger class. This provides professional-grade logging with categorization, configurable levels, and detailed context information.

## Quick Start

### Basic Configuration

```csharp
using Sainna.Robotics.ROSTools.Logging;
using UnityEngine;

// In your MonoBehaviour Start() or Awake()
void Start()
{
    // For development: Show all logs (default)
    ROSLogger.SetLogLevel(LogType.Log);
    
    // For production: Show only warnings and errors
    ROSLogger.SetLogLevel(LogType.Warning);
    
    // To completely disable logging
    ROSLogger.SetLogEnabled(false);
}
```

## Configuration Options

### Log Levels

Control which types of messages are displayed in the Unity Console:

```csharp
// Show everything (Info, Warning, Error, Exception)
ROSLogger.SetLogLevel(LogType.Log);

// Show warnings, errors, and exceptions (recommended for production)
ROSLogger.SetLogLevel(LogType.Warning);

// Show only errors and exceptions
ROSLogger.SetLogLevel(LogType.Error);

// Show only exceptions (minimal logging)
ROSLogger.SetLogLevel(LogType.Exception);
```

### Enable/Disable Logging

```csharp
// Completely disable all ROS logging (performance optimization)
ROSLogger.SetLogEnabled(false);

// Re-enable logging
ROSLogger.SetLogEnabled(true);
```

## Log Categories

All logs are categorized for easy filtering in the Unity Console:

| Category | Description | Example Messages |
|----------|-------------|------------------|
| `ROSConnection` | Connection events, reconnection attempts | "Connection established!", "Reconnection attempt 1/5" |
| `ROSServices` | Service registration and calling | "Registering service 'move_robot'", "Cannot call service, not connected" |
| `ROSTopics` | Topic management and messaging | "Created publisher for topic '/cmd_vel'", "Subscriber cleaned up" |
| `ROSManager` | Manager lifecycle and initialization | "Initialising services", "Cleaned up all services and topics" |
| `ROSEditor` | Editor tools and operations | "Created publisher for topic via editor", "Refreshing message types" |
| `ROSExamples` | Example script activities | "Published: Hello from Unity!", "Auto-reconnect enabled" |

## Log Format

Every log message follows a consistent format that includes:

```text
[ROS] [Category] Message (FileName.MethodName:LineNumber)
```

**Example:**
```text
[ROS] [ROSTopics] Created publisher for topic: '/unity/hello' (ROSManager.CreatePublisher:156)
```

This format provides:
- **`[ROS]`**: Easy identification of ROS-related logs
- **`[Category]`**: Quick filtering by system component
- **`Message`**: The actual log content
- **`(FileName.MethodName:LineNumber)`**: Exact location in code for debugging

## Using Logging in Your Scripts

### Basic Usage

```csharp
using Sainna.Robotics.ROSTools.Logging;
using UnityEngine;

public class MyRobotController : MonoBehaviour
{
    void Start()
    {
        // Simple info log
        ROSLogger.LogInfo("Robot controller initialized");
        
        // Log with custom category
        ROSLogger.LogInfo("Sensors online", "MyRobot");
        
        // Log with GameObject context (clickable in console)
        ROSLogger.LogInfo("Navigation started", "MyRobot", this);
    }
    
    void OnCollisionDetected()
    {
        // Warning with context
        ROSLogger.LogWarning("Obstacle detected, stopping", "MyRobot", this);
    }
    
    void OnCriticalError()
    {
        // Error logging
        ROSLogger.LogError("Critical system failure", "MyRobot", this);
    }
}
```

### Exception Logging

```csharp
try
{
    // Some risky operation
    CallROSService();
}
catch (System.Exception ex)
{
    // Log exception with context
    ROSLogger.LogException(ex, "Failed to call ROS service", "MyRobot", this);
}
```

### Category-Specific Convenience Methods

```csharp
// Shorthand methods for common categories
ROSLogger.LogConnection("Custom connection event");
ROSLogger.LogService("Custom service event");
ROSLogger.LogTopic("Custom topic event");
ROSLogger.LogManager("Custom manager event");
```

## Production Recommendations

### Development Settings
```csharp
// Show all logs for comprehensive debugging
ROSLogger.SetLogLevel(LogType.Log);
ROSLogger.SetLogEnabled(true);
```

### Production Settings
```csharp
// Reduce console noise while keeping important messages
ROSLogger.SetLogLevel(LogType.Warning);

// For performance-critical applications
ROSLogger.SetLogEnabled(false);
```

### Build-Specific Configuration

```csharp
void ConfigureLogging()
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Development: Show all logs
    ROSLogger.SetLogLevel(LogType.Log);
    ROSLogger.SetLogEnabled(true);
#elif UNITY_STANDALONE
    // Production: Warnings and errors only
    ROSLogger.SetLogLevel(LogType.Warning);
    ROSLogger.SetLogEnabled(true);
#else
    // Mobile/other platforms: Disable for performance
    ROSLogger.SetLogEnabled(false);
#endif
}
```

## Console Filtering Tips

### Unity Console Filtering

1. **Filter by Category**: Type `[ROSTopics]` in the console search to see only topic-related logs
2. **Filter by Log Type**: Use console buttons (Info, Warning, Error) to filter by severity
3. **Filter by Component**: Search for specific script names like `ROSManager`

### Common Filter Examples

- `[ROS]` - All ROS Tools logs
- `[ROSConnection]` - Only connection-related events
- `[ROS] Created` - All creation events
- `[ROS] Error` - All error messages
- `ROSManager.CreatePublisher` - Specific method calls

## Debugging Workflow

### Connection Issues
1. Enable all logging: `ROSLogger.SetLogLevel(LogType.Log)`
2. Filter by `[ROSConnection]` in console
3. Look for connection attempt and failure messages

### Service Problems
1. Filter by `[ROSServices]` in console
2. Check for service registration messages
3. Verify service calling attempts and responses

### Topic Issues
1. Filter by `[ROSTopics]` in console  
2. Verify publisher/subscriber creation
3. Check for cleanup messages when topics are removed

### Performance Debugging
1. Temporarily disable logging: `ROSLogger.SetLogEnabled(false)`
2. Measure performance difference
3. Re-enable with appropriate level for production

## Advanced Configuration

### Runtime Log Level Changes

```csharp
public class LoggingController : MonoBehaviour
{
    [Header("Logging Controls")]
    public KeyCode toggleLoggingKey = KeyCode.L;
    public KeyCode cycleLevelKey = KeyCode.Semicolon;
    
    private LogType[] logLevels = { LogType.Log, LogType.Warning, LogType.Error };
    private int currentLevelIndex = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(toggleLoggingKey))
        {
            ToggleLogging();
        }
        
        if (Input.GetKeyDown(cycleLevelKey))
        {
            CycleLogLevel();
        }
    }
    
    void ToggleLogging()
    {
        // Toggle logging on/off
        bool currentState = /* you'd need to track this */;
        ROSLogger.SetLogEnabled(!currentState);
        Debug.Log($"ROS Logging: {(!currentState ? "Enabled" : "Disabled")}");
    }
    
    void CycleLogLevel()
    {
        currentLevelIndex = (currentLevelIndex + 1) % logLevels.Length;
        ROSLogger.SetLogLevel(logLevels[currentLevelIndex]);
        Debug.Log($"ROS Log Level: {logLevels[currentLevelIndex]}");
    }
}
```

### Custom Categories

```csharp
// Define your own categories for project-specific logging
public static class MyProjectCategories
{
    public const string NAVIGATION = "Navigation";
    public const string SENSORS = "Sensors";
    public const string AI = "AI";
}

// Use custom categories
ROSLogger.LogInfo("Path planning complete", MyProjectCategories.NAVIGATION);
ROSLogger.LogWarning("Sensor data stale", MyProjectCategories.SENSORS);
```

## Troubleshooting

### Common Issues

**Q: I don't see any ROS logs in the console**
- Check if logging is enabled: `ROSLogger.SetLogEnabled(true)`
- Verify log level allows your message type
- Ensure you're using the correct namespace: `using Sainna.Robotics.ROSTools.Logging;`

**Q: Too many logs are cluttering the console**
- Adjust log level: `ROSLogger.SetLogLevel(LogType.Warning)`
- Use console filtering to focus on specific categories
- Consider disabling logging in production builds

**Q: I can't find where a log message is coming from**
- Look at the `(FileName.MethodName:LineNumber)` part of the log
- Click on logs with context objects to select the GameObject
- Use IDE "Go to Definition" with the file and line information

**Q: Performance impact from logging**
- Disable logging entirely: `ROSLogger.SetLogEnabled(false)`
- Increase log level to reduce message volume
- Use conditional compilation for debug-only logging
