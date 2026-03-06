//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System;

namespace Esper.FeelSpeak.Overworld.Data
{
    /// <summary>
    /// Represents a custom option that the player can select when interacting with the speaker. This is generally meant to
    /// help trigger dialogue, but can technically be used for anything.
    /// </summary>
    [Serializable]
    public class CustomDialogueOption : DialogueOption
    {
        /// <summary>
        /// The display text.
        /// </summary>
        public string text;

        /// <summary>
        /// The locale table name.
        /// </summary>
        public string tableName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">The option text.</param>
        /// <param name="onSelected">The action to execute when the option is selected.</param>
        public CustomDialogueOption(string text, Action onSelected)
        {
            this.text = text;
            this.onSelected.AddListener(onSelected.Invoke);
        }
    }
}