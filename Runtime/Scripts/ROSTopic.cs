using System;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using Sainna.Robotics.ROSTools.Logging;
using UnityEngine;

namespace Sainna.Robotics.ROSTools
{
    /// <summary>
    /// Abstract class representing a ROS Topic (Publisher or Subscriber).
    /// Used to store ROS topics regardless of message type
    /// </summary>
    public abstract class ROSTopic
    {
        /// <summary>
        /// The topic name, needs to be the same as would be used in ROS
        /// </summary>
        public string TopicName { get; protected set; }
        
        /// <summary>
        /// Reference to the current ROSConnection
        /// </summary>
        public RosConnector Connection { get; set; }

        /// <summary>
        /// Whether this topic is active (connected)
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Constructor for the <see cref="ROSTopic"/> abstract class
        /// </summary>
        protected ROSTopic()
        {
        }

        /// <summary>
        /// Initializes the topic with a connection
        /// </summary>
        public abstract void Init();
        
        /// <summary>
        /// Assigns a <see cref="Connection"/> before calling <see cref="Init()"/> on it.
        /// </summary>
        /// <param name="connection">The <see cref="RosConnector"/> currently in use</param>
        public void Init(RosConnector connection)
        {
            Connection = connection;
            Init();
        }

        /// <summary>
        /// Cleanup and disconnect the topic
        /// </summary>
        public abstract void Cleanup();
    }

    /// <summary>
    /// Represents a ROS Publisher for publishing messages to a topic
    /// </summary>
    /// <typeparam name="T">The Message type (needs to be a child of type Message)</typeparam>
    public class ROSPublisher<T> : ROSTopic where T : Message
    {
        private string publisherId;

        /// <summary>
        /// Constructor for ROSPublisher
        /// </summary>
        /// <param name="topicName">The ROS topic name</param>
        /// <param name="connection">Optional ROS connection</param>
        public ROSPublisher(string topicName, RosConnector connection = null)
        {
            TopicName = topicName;
            Connection = connection;
        }

        public override void Init()
        {
            if (Connection == null)
            {
                var manager = ROSManager.GetOrCreateInstance();
                if (manager)
                {
                    Connection = manager.GetROSConnection();
                }
            }

            if (Connection != null && Connection.IsConnected.WaitOne(0))
            {
                publisherId = Connection.RosSocket.Advertise<T>(TopicName);
                IsActive = true;
                ROSLogger.LogInfo($"Publisher initialized for topic: '{TopicName}'", ROSLogger.CATEGORY_TOPICS);
            }
            else
            {
                ROSLogger.LogWarning($"Cannot initialize publisher for topic '{TopicName}', not connected to ROS.", ROSLogger.CATEGORY_TOPICS);
                IsActive = false;
            }
        }

        /// <summary>
        /// Publishes a message to the topic
        /// </summary>
        /// <param name="message">The message to publish</param>
        public void Publish(T message)
        {
            if (!IsActive)
            {
                ROSLogger.LogWarning($"Cannot publish to topic '{TopicName}', publisher is not active.", ROSLogger.CATEGORY_TOPICS);
                return;
            }

            if (Connection != null && Connection.IsConnected.WaitOne(0))
            {
                Connection.RosSocket.Publish(publisherId, message);
            }
            else
            {
                ROSLogger.LogWarning($"Cannot publish to topic '{TopicName}', connection lost.", ROSLogger.CATEGORY_TOPICS);
                IsActive = false;
            }
        }

        public override void Cleanup()
        {
            if (IsActive && Connection != null && !string.IsNullOrEmpty(publisherId))
            {
                IsActive = false;
                try
                {
                    Connection.RosSocket.Unadvertise(publisherId);
                }
                catch (KeyNotFoundException e)
                {
                    // Key was already removed, this is fine
                    ROSLogger.LogWarning($"Publisher for topic '{TopicName}' was already cleaned up: {e.Message}", ROSLogger.CATEGORY_TOPICS);
                }
                ROSLogger.LogInfo($"Publisher cleaned up for topic: '{TopicName}'", ROSLogger.CATEGORY_TOPICS);
            }
        }
    }

    /// <summary>
    /// Represents a ROS Subscriber for receiving messages from a topic
    /// </summary>
    /// <typeparam name="T">The Message type (needs to be a child of type Message)</typeparam>
    public class ROSSubscriber<T> : ROSTopic where T : Message
    {
        private string subscriberId;
        private SubscriptionHandler<T> messageCallback;

        /// <summary>
        /// Constructor for ROSSubscriber
        /// </summary>
        /// <param name="topicName">The ROS topic name</param>
        /// <param name="callback">Callback function to handle received messages</param>
        /// <param name="connection">Optional ROS connection</param>
        public ROSSubscriber(string topicName, SubscriptionHandler<T> callback, RosConnector connection = null)
        {
            TopicName = topicName;
            messageCallback = callback;
            Connection = connection;
        }

        public override void Init()
        {
            if (Connection == null)
            {
                var manager = ROSManager.GetOrCreateInstance();
                if (manager)
                {
                    Connection = manager.GetROSConnection();
                }
            }

            if (Connection != null && Connection.IsConnected.WaitOne(0))
            {
                subscriberId = Connection.RosSocket.Subscribe<T>(TopicName, messageCallback);
                IsActive = true;
                ROSLogger.LogInfo($"Subscriber initialized for topic: '{TopicName}'", ROSLogger.CATEGORY_TOPICS);
            }
            else
            {
                ROSLogger.LogWarning($"Cannot initialize subscriber for topic '{TopicName}', not connected to ROS.", ROSLogger.CATEGORY_TOPICS);
                IsActive = false;
            }
        }

        /// <summary>
        /// Changes the callback function for this subscriber
        /// </summary>
        /// <param name="newCallback">The new callback function</param>
        public void ChangeCallback(SubscriptionHandler<T> newCallback)
        {
            messageCallback = newCallback;
            if (IsActive)
            {
                // Resubscribe with new callback
                Cleanup();
                Init();
            }
        }

        public override void Cleanup()
        {
            if (IsActive && Connection != null && !string.IsNullOrEmpty(subscriberId))
            {
                try
                {
                    Connection.RosSocket.Unsubscribe(subscriberId);
                }
                catch (KeyNotFoundException e)
                {
                    // Key was already removed, this is fine
                    ROSLogger.LogWarning($"Subscriber for topic '{TopicName}' was already cleaned up: {e.Message}", ROSLogger.CATEGORY_TOPICS);
                }
                IsActive = false;
                ROSLogger.LogInfo($"Subscriber cleaned up for topic: '{TopicName}'", ROSLogger.CATEGORY_TOPICS);
            }
        }
    }

    /// <summary>
    /// Factory class for creating ROS topics
    /// </summary>
    public static class ROSTopicFactory
    {
        /// <summary>
        /// Creates a ROS Publisher
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <param name="topicName">The ROS topic name</param>
        /// <param name="connection">Optional ROS connection</param>
        /// <returns>A new ROSPublisher instance</returns>
        public static ROSPublisher<T> CreatePublisher<T>(string topicName, RosConnector connection = null) where T : Message
        {
            return new ROSPublisher<T>(topicName, connection);
        }

        /// <summary>
        /// Creates a ROS Subscriber
        /// </summary>
        /// <typeparam name="T">The Message type</typeparam>
        /// <param name="topicName">The ROS topic name</param>
        /// <param name="callback">Callback function for received messages</param>
        /// <param name="connection">Optional ROS connection</param>
        /// <returns>A new ROSSubscriber instance</returns>
        public static ROSSubscriber<T> CreateSubscriber<T>(string topicName, SubscriptionHandler<T> callback, RosConnector connection = null) where T : Message
        {
            return new ROSSubscriber<T>(topicName, callback, connection);
        }
    }
}
