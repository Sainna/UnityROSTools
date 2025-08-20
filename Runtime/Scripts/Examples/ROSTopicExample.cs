using RosSharp.RosBridgeClient.MessageTypes.Std;
using Sainna.Robotics.ROSTools.Logging;
using UnityEngine;

namespace Sainna.Robotics.ROSTools.Examples
{
    /// <summary>
    /// Example script demonstrating how to use ROS Topics (Publishers and Subscribers)
    /// This script shows the plug-and-play philosophy for topic management
    /// </summary>
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

            ROSLogger.LogInfo($"Created publisher for topic: '{publisherTopicName}'", ROSLogger.CATEGORY_EXAMPLES);
            ROSLogger.LogInfo($"Created subscriber for topic: '{subscriberTopicName}'", ROSLogger.CATEGORY_EXAMPLES);
        }

        void Update()
        {
            // Publish a message every publishInterval seconds
            if (UnityEngine.Time.time - lastPublishTime > publishInterval)
            {
                PublishMessage();
                lastPublishTime = UnityEngine.Time.time;
            }
        }

        void PublishMessage()
        {
            if (stringPublisher != null && stringPublisher.IsActive)
            {
                var message = new String();
                message.data = $"Hello from Unity! Message #{messageCounter++}";
                
                stringPublisher.Publish(message);
                ROSLogger.LogInfo($"Published: {message.data}", ROSLogger.CATEGORY_EXAMPLES);
            }
        }

        void OnStringMessageReceived(String message)
        {
            ROSLogger.LogInfo($"Received message: {message.data}", ROSLogger.CATEGORY_EXAMPLES);
        }

        void OnDestroy()
        {
            // Topics are automatically cleaned up by ROSManager, but you can manually remove them
            if (rosManager != null)
            {
                rosManager.RemoveTopic(publisherTopicName);
                rosManager.RemoveTopic(subscriberTopicName);
            }
        }

        // Example of how to create topics dynamically
        [ContextMenu("Create Dynamic Topics")]
        void CreateDynamicTopics()
        {
            // Publishers and subscribers can be created at runtime
            var dynamicPublisher = rosManager.CreatePublisher<String>("/dynamic/publisher");
            var dynamicSubscriber = rosManager.CreateSubscriber<String>("/dynamic/subscriber", (msg) => {
                ROSLogger.LogInfo($"Dynamic subscriber received: {msg.data}", ROSLogger.CATEGORY_EXAMPLES);
            });

            // Publish a test message
            var testMessage = new String { data = "Dynamic topic test!" };
            dynamicPublisher.Publish(testMessage);
        }
    }
}
