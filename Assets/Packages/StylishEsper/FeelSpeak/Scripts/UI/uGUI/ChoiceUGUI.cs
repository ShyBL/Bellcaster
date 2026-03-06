//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Esper.FeelSpeak.UI.UGUI
{
    /// <summary>
    /// A UI object that represents a selectable choice.
    /// </summary>
    public class ChoiceUGUI : MonoBehaviour
    {
        /// <summary>
        /// The button component.
        /// </summary>
        [HideInInspector]
        public Button button;

        /// <summary>
        /// The label that displays the choice text.
        /// </summary>
        [HideInInspector]
        public TextMeshProUGUI label;

        /// <summary>
        /// The choice data.
        /// </summary>
        [NonSerialized]
        public Choice? choice;

        /// <summary>
        /// Sets the choice.
        /// </summary>
        /// <param name="choice">The choice.</param>
        public void SetChoice(Choice choice)
        {
            this.choice = choice;

            if (!button)
            {
                button = GetComponent<Button>();
                button.onClick.AddListener(() => ChoicesList.Instance?.OnOptionSelected(choice));
            }

            if (!label)
            {
                label = GetComponentInChildren<TextMeshProUGUI>();
            }

            Refresh();
        }

        /// <summary>
        /// Updates the UI objects.
        /// </summary>
        public void Refresh()
        {
            if (choice.HasValue)
            {
                label.text = choice.Value.GetInterpolatedText();
            }
        }
    }
}