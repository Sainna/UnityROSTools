# ROS Tools for Unity

Welcome to ROS Tools for Unity! This package provides a way to reduce boilerplate code for ROS service and topic management, allowing for easy use of Unity's ROS capabilities with a plug-and-play philosophy.

## Features

- **Service Management**: Easy-to-use ROS service calling with Scriptable Object configuration
- **Topic Management**: Dynamic publisher and subscriber creation without complex setup
- **Plug-and-Play**: Minimal configuration required, automatic connection management
- **Unity Integration**: Seamlessly integrates with Unity's component system
- **Error Handling**: Robust connection management with automatic reconnection

## Supported ROS Operations

- ✅ **Services**: Call ROS services with custom requests and responses
- ✅ **Topics**: Publish and subscribe to ROS topics dynamically
- ✅ **Connection Management**: Automatic ROS connection handling
- ✅ **Message Types**: Support for all standard and custom ROS message types

## How to install

In the Unity Package manager, add a package from git URL and use the following:

```text
https://github.com/Sainna/UnityROSTools.git
```

## Quick Start

### Services

1. Create a ROSServiceSO (Scriptable Object) via `Assets → Create → ScriptableObjects → ROS → Service Caller`
2. Add a `ROSManager` component to a GameObject in your scene
3. Reference your Scriptable Object in the ROSManager
4. Call services from your scripts using the simple API

### Topics

1. Get the ROSManager instance: `ROSManager.GetOrCreateInstance()`
2. Create publishers: `rosManager.CreatePublisher<MessageType>("topic_name")`
3. Create subscribers: `rosManager.CreateSubscriber<MessageType>("topic_name", callback)`
4. Everything is handled automatically!
