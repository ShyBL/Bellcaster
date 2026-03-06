//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Esper.FeelSpeak.Graph.Data
{
    /// <summary>
    /// Processed dialogue info for pre-processed custom text tags.
    /// </summary>
    public class DialogueInfo
    {
        /// <summary>
        /// All text data.
        /// </summary>
        public List<CharacterData> data = new();

        /// <summary>
        /// The processed text.
        /// </summary>
        public string text;

        /// <summary>
        /// Custom text tags.
        /// </summary>
        private static readonly HashSet<string> customTags = new HashSet<string> { "speed", "pause", "instant" };

        /// <summary>
        /// Tag regex.
        /// </summary>
        private static readonly Regex regex = new Regex(@"\<(?<tag>\w+)(?:=(?<param>[^\>]+))?\>|\</(?<closingTag>\w+)\>", RegexOptions.Compiled);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="input">The text to process.</param>
        public DialogueInfo(string input)
        {
            ProcessText(input);
        }

        /// <summary>
        /// Removes custom tags.
        /// </summary>
        /// <param name="input">The original text.</param>
        /// <returns>The text with custom tags removed.</returns>
        public static string RemoveCustomTags(string input)
        {
            Regex tagRegex = new Regex(@"<\/?(?<tag>\w+)(=[^>]+)?>", RegexOptions.Compiled);

            return tagRegex.Replace(input, match =>
            {
                string tag = match.Groups["tag"].Value.ToLower();

                if (customTags.Contains(tag))
                {
                    return string.Empty;
                }

                return match.Value;
            });
        }

        /// <summary>
        /// Processes the text.
        /// </summary>
        /// <param name="input">The text to process.</param>
        public void ProcessText(string input)
        {
            float currentSpeed = 1f;
            bool instantEnabled = false;
            int visibleIndex = 0;
            var matches = regex.Matches(input);
            float pauseAfterEnder = 0f;

            int matchIndex = 0;
            int i = 0;

            while (i < input.Length)
            {
                // Check if we're at a tag position
                if (matchIndex < matches.Count && matches[matchIndex].Index == i)
                {
                    var match = matches[matchIndex];

                    // Check for opening tag
                    if (match.Groups["tag"].Success)
                    {
                        string tag = match.Groups["tag"].Value.ToLower();
                        string parameter = match.Groups["param"].Success ? match.Groups["param"].Value : null;

                        if (!customTags.Contains(tag))
                        {
                            i += match.Length;
                            visibleIndex += match.Length;
                            matchIndex++;
                            continue;
                        }

                        if (tag == "pause")
                        {
                            data.Add(new CharacterData
                            {
                                sourceIndex = i,
                                visibleIndex = visibleIndex,
                                speed = currentSpeed,
                                pause = float.TryParse(parameter, out var pauseValue) ? pauseValue : 0f,
                                instant = instantEnabled,
                            });
                        }
                        else if (tag == "speed")
                        {
                            currentSpeed = float.TryParse(parameter, out var speedValue) ? speedValue : 1f;
                        }
                        else if (tag == "instant")
                        {
                            instantEnabled = true; 
                        }

                        i += match.Length;
                        matchIndex++;
                        continue;
                    }
                    // Check for closing tag
                    else if (match.Groups["closingTag"].Success)
                    {
                        string closingTag = match.Groups["closingTag"].Value.ToLower();

                        if (closingTag == "speed")
                        {
                            currentSpeed = 1f;
                        }
                        else if (closingTag == "instant")
                        {
                            instantEnabled = false;    
                        }

                        i += match.Length;
                        matchIndex++;
                        continue;
                    }
                }

                // Check for multi-character ellipsis "..."
                if (i <= input.Length - 3 && input.Substring(i, 3) == "...")
                {
                    for (int j = 0; j < 3; j++)
                    {
                        data.Add(new CharacterData
                        {
                            character = '.',
                            sourceIndex = i,
                            visibleIndex = visibleIndex,
                            speed = FeelSpeak.Settings.ellipsisSpeedMultiplier,
                            pause = 0f,
                            instant = instantEnabled
                        });

                        i++;
                        visibleIndex++;
                    }
                    continue;
                }

                // Check for single-character ellipsis '…'
                if (input[i] == '\u2026')
                {
                    data.Add(new CharacterData
                    {
                        character = input[i],
                        sourceIndex = i,
                        visibleIndex = visibleIndex,
                        speed = FeelSpeak.Settings.ellipsisSpeedMultiplier,
                        pause = 0f,
                        instant = instantEnabled
                    });

                    i++;
                    visibleIndex++;
                    continue;
                }

                // Regular visible character
                data.Add(new CharacterData
                {
                    character = input[i],
                    sourceIndex = i,
                    visibleIndex = visibleIndex,
                    speed = currentSpeed,
                    pause = pauseAfterEnder,
                    instant = instantEnabled
                });

                // Check if the character is a sentence ender
                bool isSentenceEnder = input[i] == '.' || input[i] == '!' || input[i] == '?';

                if (isSentenceEnder)
                {
                    pauseAfterEnder = FeelSpeak.Settings.sentenceEndPause;
                }
                else
                {
                    pauseAfterEnder = 0f;
                }

                i++;
                visibleIndex++;
            }

            text = RemoveCustomTags(input);
        }

        /// <summary>
        /// Data of a single text character.
        /// </summary>
        public class CharacterData
        {
            /// <summary>
            /// The text character.
            /// </summary>
            public char character;

            /// <summary>
            /// The source index.
            /// </summary>
            public int sourceIndex;

            /// <summary>
            /// The visible index after processing.
            /// </summary>
            public int visibleIndex;

            /// <summary>
            /// The speed at this index.
            /// </summary>
            public float speed;

            /// <summary>
            /// The pause length at this index.
            /// </summary>
            public float pause;

            /// <summary>
            /// If this character should be displayed instantly.
            /// </summary>
            public bool instant;
        }
    }
}