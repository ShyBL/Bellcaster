//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Overworld;
using TMPro;
using UnityEngine;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// A component that works as a name tag of a speaker.
    /// </summary>
    public class NameTag : Billboarded
    {
        /// <summary>
        /// The text label.
        /// </summary>
        [HideInInspector]
        public TextMeshProUGUI label;

        /// <summary>
        /// The speaker to display the name for.
        /// </summary>
        public Speaker speaker;

        private void Awake()
        {
            label = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateName();
        }

        /// <summary>
        /// Updates the name tag.
        /// </summary>
        public void UpdateName()
        {
            if (!speaker)
            {
                FeelSpeakLogger.LogWarning("Name Tag: speaker is missing or was never set!");
            }

            if (FeelSpeak.Settings.enableObjectTextColors)
            {
                label.text = $"<color=#{speaker.character.textColor.ToHexString()}>{speaker.character.characterName}</color>";
            }
            else
            {
                label.text = speaker.character.characterName;
            }
        }
    }
}