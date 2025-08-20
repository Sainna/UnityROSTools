using UnityEngine;
using System.Runtime.CompilerServices;

namespace Sainna.Robotics.ROSTools.Logging
{
    /// <summary>
    /// Centralized logging utility for ROS Tools package using Unity's built-in Logger
    /// </summary>
    public static class ROSLogger
    {
        private static Logger s_Logger;
        
        // Log categories for better organization
        public const string CATEGORY_CONNECTION = "ROSConnection";
        public const string CATEGORY_SERVICES = "ROSServices";
        public const string CATEGORY_TOPICS = "ROSTopics";
        public const string CATEGORY_MANAGER = "ROSManager";
        public const string CATEGORY_EDITOR = "ROSEditor";
        public const string CATEGORY_EXAMPLES = "ROSExamples";

        static ROSLogger()
        {
            InitializeLogger();
        }

        private static void InitializeLogger()
        {
            // Create a custom Logger using Unity's built-in Logger class
            s_Logger = new Logger(Debug.unityLogger.logHandler);
            s_Logger.logEnabled = true;
            s_Logger.filterLogType = LogType.Log;
        }
        
        public static void SetLogLevel(LogType logType)
        {
            // Set the log level for the logger
            s_Logger.filterLogType = logType;
        }
        
        public static void SetLogEnabled(bool enabled)
        {
            // Enable or disable logging
            s_Logger.logEnabled = enabled;
        }

        #region Public Logging Methods

        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="category">Log category for filtering</param>
        /// <param name="context">Unity object context (optional)</param>
        /// <param name="memberName">Calling method name (auto-filled)</param>
        /// <param name="sourceFilePath">Source file path (auto-filled)</param>
        /// <param name="sourceLineNumber">Source line number (auto-filled)</param>
        public static void LogInfo(string message, string category = "", Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var formattedMessage = FormatMessage(message, category, memberName, sourceFilePath, sourceLineNumber);
            s_Logger.Log(LogType.Log, (object)formattedMessage, context);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="category">Log category for filtering</param>
        /// <param name="context">Unity object context (optional)</param>
        /// <param name="memberName">Calling method name (auto-filled)</param>
        /// <param name="sourceFilePath">Source file path (auto-filled)</param>
        /// <param name="sourceLineNumber">Source line number (auto-filled)</param>
        public static void LogWarning(string message, string category = "", Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var formattedMessage = FormatMessage(message, category, memberName, sourceFilePath, sourceLineNumber);
            s_Logger.Log(LogType.Warning, (object)formattedMessage, context);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="category">Log category for filtering</param>
        /// <param name="context">Unity object context (optional)</param>
        /// <param name="memberName">Calling method name (auto-filled)</param>
        /// <param name="sourceFilePath">Source file path (auto-filled)</param>
        /// <param name="sourceLineNumber">Source line number (auto-filled)</param>
        public static void LogError(string message, string category = "", Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var formattedMessage = FormatMessage(message, category, memberName, sourceFilePath, sourceLineNumber);
            s_Logger.Log(LogType.Error, (object)formattedMessage, context);
        }

        /// <summary>
        /// Logs an exception with context
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">Additional context message</param>
        /// <param name="category">Log category for filtering</param>
        /// <param name="context">Unity object context (optional)</param>
        /// <param name="memberName">Calling method name (auto-filled)</param>
        /// <param name="sourceFilePath">Source file path (auto-filled)</param>
        /// <param name="sourceLineNumber">Source line number (auto-filled)</param>
        public static void LogException(System.Exception exception, string message = "", string category = "", Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var formattedMessage = FormatMessage($"{message} Exception: {exception.Message}", category, memberName, sourceFilePath, sourceLineNumber);
            s_Logger.Log(LogType.Exception, (object)$"{formattedMessage}\nStackTrace: {exception.StackTrace}", context);
        }

        #endregion

        #region Convenience Methods for Specific Categories

        public static void LogConnection(string message, LogLevel level = LogLevel.Info, Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithLevel(message, CATEGORY_CONNECTION, level, context, memberName, sourceFilePath, sourceLineNumber);
        }

        public static void LogService(string message, LogLevel level = LogLevel.Info, Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithLevel(message, CATEGORY_SERVICES, level, context, memberName, sourceFilePath, sourceLineNumber);
        }

        public static void LogTopic(string message, LogLevel level = LogLevel.Info, Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithLevel(message, CATEGORY_TOPICS, level, context, memberName, sourceFilePath, sourceLineNumber);
        }

        public static void LogManager(string message, LogLevel level = LogLevel.Info, Object context = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithLevel(message, CATEGORY_MANAGER, level, context, memberName, sourceFilePath, sourceLineNumber);
        }

        #endregion

        #region Private Helper Methods

        private static void LogWithLevel(string message, string category, LogLevel level, Object context,
            string memberName, string sourceFilePath, int sourceLineNumber)
        {
            switch (level)
            {
                case LogLevel.Info:
                    LogInfo(message, category, context, memberName, sourceFilePath, sourceLineNumber);
                    break;
                case LogLevel.Warning:
                    LogWarning(message, category, context, memberName, sourceFilePath, sourceLineNumber);
                    break;
                case LogLevel.Error:
                    LogError(message, category, context, memberName, sourceFilePath, sourceLineNumber);
                    break;
            }
        }

        private static string FormatMessage(string message, string category, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            var categoryPrefix = !string.IsNullOrEmpty(category) ? $"[{category}] " : "";
            
            return $"[ROS] {categoryPrefix}{message} ({fileName}.{memberName}:{sourceLineNumber})";
        }

        #endregion

        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }
    }
}
