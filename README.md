# ROS Tools for Unity

Welcome to ROS Tools for Unity! This package provides a way to reduce boilerplate code for ROS service and topic management, allowing for easy use of Unity's ROS capabilities with a plug-and-play philosophy.

## Features

- **Service Management**: Easy-to-use ROS service calling with Scriptable Object configuration  
- **Topic Management**: Dynamic publisher and subscriber creation without complex setup
- **Auto-Reconnect**: Robust automatic reconnection with configurable retry attempts and timeouts
- **Centralized Logging**: Professional logging system with configurable levels and categories
- **Plug-and-Play**: Minimal configuration required, automatic connection management
- **Unity Integration**: Seamlessly integrates with Unity's component system
- **Error Handling**: Comprehensive error handling and graceful degradation

## Supported ROS Operations

- ✅ **Services**: Call ROS services with custom requests and responses
- ✅ **Topics**: Publish and subscribe to ROS topics dynamically  
- ✅ **Auto-Reconnect**: Automatic connection recovery with configurable parameters
- ✅ **Connection Management**: Robust connection handling with events and monitoring
- ✅ **Message Types**: Support for all standard and custom ROS message types

## How to install

In the Unity Package manager, add a package from git URL and use the following:

```text
https://github.com/Sainna/UnityROSTools.git
``

## Logging Configuration

The package includes a centralized logging system that can be configured for your development and production needs:

### Enable/Disable Logging

```csharp
using Sainna.Robotics.ROSTools.Logging;

// Disable all logging
ROSLogger.SetLogEnabled(false);

// Re-enable logging
ROSLogger.SetLogEnabled(true);
```

### Set Log Levels

```csharp
using UnityEngine;
using Sainna.Robotics.ROSTools.Logging;

// Show only warnings and errors
ROSLogger.SetLogLevel(LogType.Warning);

// Show all logs (default)
ROSLogger.SetLogLevel(LogType.Log);

// Show only errors
ROSLogger.SetLogLevel(LogType.Error);
```

### Log Categories

The logging system includes categorized messages for easy filtering:

- `ROSConnection`: Connection/reconnection events
- `ROSServices`: Service registration and calling
- `ROSTopics`: Topic creation, publishing, subscribing  
- `ROSManager`: Manager initialization and cleanup
- `ROSEditor`: Editor-specific operations
- `ROSExamples`: Example script activities

All logs are prefixed with `[ROS]` and include detailed context information.


## Documentation

Documentation is available here: [https://sainna.github.io/UnityROSTools](https://sainna.github.io/UnityROSTools)

## Disclaimer
Part of this code and documentation was written with the help of AI Tools. It has been reviewed by human developpers.
