//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.FeelSpeak.Settings
{
    /// <summary>
    /// Feel Speak's settings.
    /// </summary>
    public class FeelSpeakSettings : ScriptableObject
    {
        /// <summary>
        /// Controls the debug messages logged in the console.
        /// </summary>
        public DebugLogMode debugLogMode = DebugLogMode.Normal;

        /// <summary>
        /// The type of database to use. SQLite is generally recommended as it's more memory efficient. However, SQLite doesn't work on
        /// WebGL. Therefore, WebGL projects should use MDB.
        /// </summary>
        public DatabaseType databaseType;

        /// <summary>
        /// The normal dialogue text speed.
        /// </summary>
        public float textSpeed = 1f;

        /// <summary>
        /// If the player should be able to fast forward dialogue text.
        /// </summary>
        public bool enableFastForward = true;

        /// <summary>
        /// The pause length between sentences. This is only valid for '.', '!', and '?'.
        /// </summary>
        public float sentenceEndPause = 0.15f;

        /// <summary>
        /// The speed multiplier for ellipsis.
        /// </summary>
        public float ellipsisSpeedMultiplier = 0.1f;

        /// <summary>
        /// If enabled, character and emotion text colors will be applied to the dialogue text.
        /// </summary>
        public bool enableObjectTextColors = true;

        /// <summary>
        /// If enabled, any configured text sounds will play during dialogue.
        /// </summary>
        public bool enableTextSounds = true;

        /// <summary>
        /// The default text sound used when no other text sound is found.
        /// </summary>
        public AudioClip defaultTextSound;

        /// <summary>
        /// The minimum delay between text sounds. The delay may be greater than this if text speed is slower or text is
        /// paused.
        /// </summary>
        public float textSoundMinDelay = 0.08f;

        /// <summary>
        /// If enabled, any configured voice lines will play during dialogue.
        /// </summary>
        public bool enableVoiceLines = true;

        /// <summary>
        /// The default color set for the active speaker sprite.
        /// </summary>
        public Color activeSpeakerSpriteColor = Color.white;

        /// <summary>
        /// The default color set for the inactive speaker sprite.
        /// </summary>
        public Color inactiveSpeakerSpriteColor = Color.gray;

        /// <summary>
        /// The length of time that the choice made is visible to the player. This is essentially a delay between making a
        /// choice and the next node.
        /// </summary>
        public float choiceDisplayLength = 0.5f;

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string databaseName = "FeelSpeak";

        /// <summary>
        /// The default automatic next on complete value for all new dialogue nodes.
        /// </summary>
        public bool defaultAutomaticNextOnComplete;

        /// <summary>
        /// The default automatic next delay length for all new dialogue nodes.
        /// </summary>
        public float defaultAutomaticNextDelay;

        /// <summary>
        /// If timeout should be automatically set to enabled for all new choice nodes.
        /// </summary>
        public bool timeoutEnabledByDefault;

        /// <summary>
        /// The default amount of time the player has to make a choice.
        /// </summary>
        public float defaultTimeoutLength = 30f;

        /// <summary>
        /// The default delay value of a delay node.
        /// </summary>
        public float defaultDelay = 5f;

        /// <summary>
        /// The default hide dialogue box value of a delay node.
        /// </summary>
        public bool defaultHideDialogueBox;

        /// <summary>
        /// Supported log modes.
        /// </summary>
        public enum DebugLogMode
        {
            /// <summary>
            /// No logs.
            /// </summary>
            None,

            /// <summary>
            /// Normal logs.
            /// </summary>
            Normal
        }

        /// <summary>
        /// Supported database types.
        /// </summary>
        public enum DatabaseType
        {
            /// <summary>
            /// The recommended database solution. It works on all platforms except WebGL. If you're planning on using WebGL opt for
            /// MDB instead.
            /// </summary>
            SQLite,

            /// <summary>
            /// MDB works on all platforms at the cost of more memory. However, this is usually nothing to worry about.
            /// </summary>
            MDB
        }
    }
}