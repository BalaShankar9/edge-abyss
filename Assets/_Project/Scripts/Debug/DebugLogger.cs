using UnityEngine;
using System.Diagnostics;

namespace EdgeAbyss.Debug
{
    /// <summary>
    /// Wrapper around UnityEngine.Debug for project-wide logging.
    /// Provides static methods that match UnityEngine.Debug API.
    /// When 'using EdgeAbyss.Debug;' is added, Debug.Log/LogWarning/LogError
    /// will use these methods instead of UnityEngine.Debug.
    /// </summary>
    public static class Debug
    {
        /// <summary>Enable/disable all logging at runtime.</summary>
        public static bool Enabled = true;

        /// <summary>
        /// Logs a standard message.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("DEBUG")]
        public static void Log(string message)
        {
            if (Enabled)
                UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// Logs a standard message with context object.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("DEBUG")]
        public static void Log(string message, Object context)
        {
            if (Enabled)
                UnityEngine.Debug.Log(message, context);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public static void LogWarning(string message)
        {
            if (Enabled)
                UnityEngine.Debug.LogWarning(message);
        }

        /// <summary>
        /// Logs a warning message with context object.
        /// </summary>
        public static void LogWarning(string message, Object context)
        {
            if (Enabled)
                UnityEngine.Debug.LogWarning(message, context);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        /// <summary>
        /// Logs an error message with context object.
        /// </summary>
        public static void LogError(string message, Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        public static void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        /// <summary>
        /// Logs an exception with context object.
        /// </summary>
        public static void LogException(System.Exception exception, Object context)
        {
            UnityEngine.Debug.LogException(exception, context);
        }
    }

    /// <summary>
    /// Assertion utilities.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void IsTrue(bool condition, string message = "Assertion failed")
        {
            UnityEngine.Debug.Assert(condition, message);
        }

        /// <summary>
        /// Asserts that an object is not null.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void IsNotNull(object obj, string message = "Object was null")
        {
            UnityEngine.Debug.Assert(obj != null, message);
        }
    }
}
