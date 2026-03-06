//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System;
using UnityEngine.Events;

namespace Esper.FeelSpeak.Overworld.Data
{
    /// <summary>
    /// Represents a selectable dialogue option.
    /// </summary>
    [Serializable]
    public class DialogueOption
    {
        /// <summary>
        /// The order as it appears in the choices list.
        /// </summary>
        public int sortingOrder;

        /// <summary>
        /// A callback for when this option is selected.
        /// </summary>
        public UnityEvent onSelected = new();

        /// <summary>
        /// The text getter.
        /// </summary>
        public Func<string> textGetter;

        /// <summary>
        /// Gets the display text.
        /// </summary>
        /// <returns>The display text.</returns>
        public virtual string GetDisplayText()
        {
            return textGetter.Invoke();
        }
    }
}