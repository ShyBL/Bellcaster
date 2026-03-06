//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System;

namespace Esper.FeelSpeak.Graph.Data
{
    /// <summary>
    /// Represents a dialogue choice.
    /// </summary>
    [Serializable]
    public struct Choice
    {
        /// <summary>
        /// The name of the choice. Ideally, this should be unique.
        /// </summary>
        public string name;

        /// <summary>
        /// The choice text that the player will see.
        /// </summary>
        public string text;

        /// <summary>
        /// The index of this choice.
        /// </summary>
        public int index;

        /// <summary>
        /// Gets the choice text. This includes object tag interpolation. "???" is used if a character or emotion that is 
        /// attempted to be referenced doesn't exist.
        /// </summary>
        /// <returns>The interpolated text.</returns>
        public string GetInterpolatedText()
        {
            var activeSpeaker = FeelSpeak.FindActiveDialogueSpeaker();

            if (activeSpeaker)
            {
                var character = activeSpeaker.speaker.character;
                var emotion = activeSpeaker.currentEmotion;
                return FeelSpeakUtility.GetInterpolatedText(text, character, emotion);
            }
            else
            {
                return FeelSpeakUtility.GetInterpolatedText(text, null, null);
            }
        }
    }
}