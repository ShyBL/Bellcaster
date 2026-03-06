//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Esper.FeelSpeak
{
    /// <summary>
    /// Feel Speak's utility functions.
    /// </summary>
    public static class FeelSpeakUtility
    {
        /// <summary>
        /// Interpolation mataching regex.
        /// </summary>
        public static readonly Regex interpolationRegex = new Regex(@"<(\w+)(?:=([^>]+))?>", RegexOptions.Compiled);

        /// <summary>
        /// Gets a CharacterEmotion from a list of CharacterEmotions.
        /// </summary>
        /// <param name="characterEmotions">The list of CharacterEmotions.</param>
        /// <param name="id">The ID of the emotion.</param>
        /// <returns>The CharacterEmotion with the ID or null if it doesn't exist.</returns>
        public static Character.CharacterEmotion? Get(this List<Character.CharacterEmotion> characterEmotions, int id)
        {
            foreach (var characterEmotion in characterEmotions)
            {
                if (characterEmotion.emotion && characterEmotion.emotion.id == id)
                {
                    return characterEmotion;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a color to a hex string.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The color as a hex string.</returns>
        public static string ToHexString(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        /// <summary>
        /// Gets the interpolated version of a given string. "???" is used if a character or emotion that is attempted to be 
        /// referenced doesn't exist.
        /// </summary>
        /// <param name="original">The original text.</param>
        /// <param name="character">The active character reference.</param>
        /// <param name="emotion">The active emotion reference.</param>
        /// <returns>The interpolated text.</returns>
        public static string GetInterpolatedText(string original, Character character, Emotion emotion)
        {
            string defaultText = "???";

            original = interpolationRegex.Replace(original, match =>
            {
                string tag = match.Groups[1].Value;
                string value = match.Groups[2].Success ? match.Groups[2].Value : null;

                switch (tag)
                {
                    case "character":
                        // Find specific character and return name
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (int.TryParse(value, out int id))
                            {
                                character = FeelSpeak.GetCharacter(id);
                            }
                            else
                            {
                                character = FeelSpeak.GetCharacter(value);
                            }
                        }

                        if (FeelSpeak.Settings.enableObjectTextColors)
                        {
                            return character ? $"<color=#{character.textColor.ToHexString()}>{character.characterName}</color>" : defaultText;
                        }
                        else
                        {
                            return character ? $"{character.characterName}" : defaultText;
                        }

                    case "emotion":
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (int.TryParse(value, out int emotionId))
                            {
                                emotion = FeelSpeak.GetEmotion(emotionId);
                            }
                            else
                            {
                                emotion = FeelSpeak.GetEmotion(value);
                            }
                        }

                        if (FeelSpeak.Settings.enableObjectTextColors)
                        {
                            return emotion ? $"<color=#{emotion.textColor.ToHexString()}>{emotion.emotionName.ToLower()}</color>" : defaultText;
                        }
                        else
                        {
                            return emotion ? $"{emotion.emotionName.ToLower()}" : defaultText;
                        }

                    default:
                        // Leave unknown tags unchanged
                        return match.Value;
                }
            });

            return original;
        }

        /// <summary>
        /// Checks if the GameObject is a prefab. Only works in the Unity editor.
        /// </summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <returns>True if the GameObject is a prefab. Otherwise, false.</returns>
        public static bool IsPrefab(this GameObject gameObject)
        {
#if UNITY_EDITOR
            return UnityEditor.PrefabUtility.GetPrefabAssetType(gameObject) != UnityEditor.PrefabAssetType.NotAPrefab
                   && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(gameObject) == UnityEditor.PrefabInstanceStatus.NotAPrefab;
#else
            return false;
#endif
        }
    }
}