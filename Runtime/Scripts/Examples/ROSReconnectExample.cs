using UnityEngine;
using Sainna.Robotics.ROSTools;
using Sainna.Robotics.ROSTools.Logging;

namespace Sainna.Robotics.ROSTools.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the auto-reconnect features of ROSManager
    /// </summary>
    public class ROSReconnectExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Text connectionStatusText;
        [SerializeField] private UnityEngine.UI.Text reconnectStatusText;
        [SerializeField] private UnityEngine.UI.Button manualReconnectButton;
        [SerializeField] private UnityEngine.UI.Button stopReconnectButton;
        [SerializeField] private UnityEngine.UI.Toggle autoReconnectToggle;

        private ROSManager rosManager;

        void Start()
        {
            // Get the ROS Manager instance
            rosManager = ROSManager.GetOrCreateInstance();

            // Subscribe to connection events
            rosManager.ROSConnected += OnROSConnected;
            rosManager.ROSDisconnected += OnROSDisconnected;
            rosManager.ROSReconnectionAttempt += OnReconnectionAttempt;
            rosManager.ROSReconnectionFailed += OnReconnectionFailed;

            // Setup UI event handlers
            if (manualReconnectButton != null)
                manualReconnectButton.onClick.AddListener(OnManualReconnectClicked);
            
            if (stopReconnectButton != null)
                stopReconnectButton.onClick.AddListener(OnStopReconnectClicked);
            
            if (autoReconnectToggle != null)
            {
                autoReconnectToggle.isOn = rosManager.AutoReconnectEnabled;
                autoReconnectToggle.onValueChanged.AddListener(OnAutoReconnectToggled);
            }

            // Initial status update
            UpdateConnectionStatus();
        }

        void Update()
        {
            // Update UI periodically
            UpdateConnectionStatus();
            UpdateReconnectStatus();
        }

        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (rosManager != null)
            {
                rosManager.ROSConnected -= OnROSConnected;
                rosManager.ROSDisconnected -= OnROSDisconnected;
                rosManager.ROSReconnectionAttempt -= OnReconnectionAttempt;
                rosManager.ROSReconnectionFailed -= OnReconnectionFailed;
            }
        }

        #region Event Handlers

        private void OnROSConnected()
        {
            ROSLogger.LogInfo("ROS Connected!", ROSLogger.CATEGORY_EXAMPLES);
            UpdateConnectionStatus();
        }

        private void OnROSDisconnected()
        {
            ROSLogger.LogInfo("ROS Disconnected!", ROSLogger.CATEGORY_EXAMPLES);
            UpdateConnectionStatus();
        }

        private void OnReconnectionAttempt(int attemptNumber, int maxAttempts)
        {
            string maxAttemptsStr = maxAttempts == 0 ? "∞" : maxAttempts.ToString();
            ROSLogger.LogInfo($"Reconnection attempt {attemptNumber}/{maxAttemptsStr}", ROSLogger.CATEGORY_EXAMPLES);
            UpdateReconnectStatus();
        }

        private void OnReconnectionFailed(int totalAttempts)
        {
            ROSLogger.LogError($"Reconnection failed after {totalAttempts} attempts", ROSLogger.CATEGORY_EXAMPLES);
            UpdateReconnectStatus();
        }

        #endregion

        #region UI Event Handlers

        private void OnManualReconnectClicked()
        {
            ROSLogger.LogInfo("Manual reconnect triggered", ROSLogger.CATEGORY_EXAMPLES);
            rosManager.ManualReconnect();
        }

        private void OnStopReconnectClicked()
        {
            ROSLogger.LogInfo("Stop reconnect triggered", ROSLogger.CATEGORY_EXAMPLES);
            rosManager.StopReconnection();
        }

        private void OnAutoReconnectToggled(bool enabled)
        {
            ROSLogger.LogInfo($"Auto-reconnect {(enabled ? "enabled" : "disabled")}", ROSLogger.CATEGORY_EXAMPLES);
            rosManager.AutoReconnectEnabled = enabled;
        }

        #endregion

        #region UI Updates

        private void UpdateConnectionStatus()
        {
            if (connectionStatusText == null || rosManager == null) return;

            string status = rosManager.IsConnected ? "Connected" : "Disconnected";
            Color color = rosManager.IsConnected ? Color.green : Color.red;

            connectionStatusText.text = $"ROS Status: {status}";
            connectionStatusText.color = color;
        }

        private void UpdateReconnectStatus()
        {
            if (reconnectStatusText == null || rosManager == null) return;

            if (rosManager.IsReconnecting)
            {
                reconnectStatusText.text = "Reconnecting...";
                reconnectStatusText.color = Color.yellow;
            }
            else if (rosManager.AutoReconnectEnabled)
            {
                reconnectStatusText.text = "Auto-reconnect: Enabled";
                reconnectStatusText.color = Color.blue;
            }
            else
            {
                reconnectStatusText.text = "Auto-reconnect: Disabled";
                reconnectStatusText.color = Color.gray;
            }
        }

        #endregion

        #region Context Menu Actions (for testing)

        [ContextMenu("Test Manual Reconnect")]
        void TestManualReconnect()
        {
            OnManualReconnectClicked();
        }

        [ContextMenu("Test Stop Reconnect")]
        void TestStopReconnect()
        {
            OnStopReconnectClicked();
        }

        [ContextMenu("Toggle Auto Reconnect")]
        void TestToggleAutoReconnect()
        {
            if (rosManager != null)
            {
                rosManager.AutoReconnectEnabled = !rosManager.AutoReconnectEnabled;
                ROSLogger.LogInfo($"Auto-reconnect is now {(rosManager.AutoReconnectEnabled ? "enabled" : "disabled")}", ROSLogger.CATEGORY_EXAMPLES);
            }
        }

        [ContextMenu("Print Connection Info")]
        void PrintConnectionInfo()
        {
            if (rosManager != null)
            {
                ROSLogger.LogInfo($"Connection Status: {(rosManager.IsConnected ? "Connected" : "Disconnected")}", ROSLogger.CATEGORY_EXAMPLES);
                ROSLogger.LogInfo($"Is Reconnecting: {rosManager.IsReconnecting}", ROSLogger.CATEGORY_EXAMPLES);
                ROSLogger.LogInfo($"Auto-reconnect Enabled: {rosManager.AutoReconnectEnabled}", ROSLogger.CATEGORY_EXAMPLES);
                ROSLogger.LogInfo($"Max Reconnect Attempts: {rosManager.MaxReconnectAttempts}", ROSLogger.CATEGORY_EXAMPLES);
                ROSLogger.LogInfo($"Reconnect Interval: {rosManager.ReconnectInterval}s", ROSLogger.CATEGORY_EXAMPLES);
                ROSLogger.LogInfo($"Connection Timeout: {rosManager.ConnectionTimeout}s", ROSLogger.CATEGORY_EXAMPLES);
            }
        }

        #endregion
    }
}
