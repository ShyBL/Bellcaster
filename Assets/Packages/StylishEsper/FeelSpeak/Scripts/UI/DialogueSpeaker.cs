//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph;
using Esper.FeelSpeak.Overworld;
using Esper.FeelSpeak.UI.UGUI;
using UnityEngine;

namespace Esper.FeelSpeak.UI
{
    /// <summary>
    /// Represents a character in dialogue.
    /// </summary>
    public abstract class DialogueSpeaker : MonoBehaviour
    {
        /// <summary>
        /// The speaker.
        /// </summary>
        [HideInInspector]
        public Speaker speaker;

        /// <summary>
        /// The current emotion of the speaker.
        /// </summary>
        [HideInInspector]
        public Emotion currentEmotion;

        /// <summary>
        /// If a character has been set.
        /// </summary>
        public bool HasCharacter { get => speaker; }

        /// <summary>
        /// Visually sets this speaker as active.
        /// </summary>
        /// <param name="color">The color of the sprite.</param>
        public abstract void SetActive(Color? color = null);

        /// <summary>
        /// Visually sets this speaker as inactive.
        /// </summary>
        /// <param name="color">The color of the sprite.</param>
        public abstract void SetInactive(Color? color = null);

        /// <summary>
        /// Sets the speaker data based on a dialogue node.
        /// </summary>
        /// <param name="dialogueNode">The dialogue node.</param>
        public abstract void SetData(DialogueNode dialogueNode);

        /// <summary>
        /// Updates the UI elements.
        /// </summary>
        public abstract void UpdateElements();

        /// <summary>
        /// If this speaker is the active one.
        /// </summary>
        /// <returns>True if this speaker is active. Otherwise, false.</returns>
        public bool IsActive()
        {
            if (!DialogueBox.Instance)
            {
                return false;
            }

            return DialogueBox.Instance.activeDialogueSpeaker == this;
        }
    }
}