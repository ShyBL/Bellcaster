//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Settings;
using UnityEngine;

namespace Esper.FeelSpeak
{
    /// <summary>
    /// Feel Speak logger.
    /// </summary>
    public static class FeelSpeakLogger
    {
        /// <summary>
        /// Logs a message to the Unity Console.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void Log(object message)
        {
            if (FeelSpeak.Settings.debugLogMode == FeelSpeakSettings.DebugLogMode.Normal)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// Logs a warning message to the Unity Console.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogWarning(object message)
        {
            if (FeelSpeak.Settings.debugLogMode == FeelSpeakSettings.DebugLogMode.Normal)
            {
                Debug.LogWarning(message);
            }
        }

        /// <summary>
        /// Logs an error message to the Unity console.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        public static void LogError(object message)
        {
            if (FeelSpeak.Settings.debugLogMode == FeelSpeakSettings.DebugLogMode.Normal)
            {
                Debug.LogError(message);
            }
        }
    }
}