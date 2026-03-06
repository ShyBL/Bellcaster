//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// A node that contains choice data.
    /// </summary>
    [System.Serializable]
    public class ChoiceNode: Node
    {
        /// <summary>
        /// If there should be limited time to make a choice.
        /// </summary>
        public bool useTimeout;

        /// <summary>
        /// The length of time the player has to make a choice.
        /// </summary>
        public float timeout;

        /// <summary>
        /// If a random option should be selected when time runs out.
        /// </summary>
        public bool timeoutChoiceIsRandom;

        /// <summary>
        /// The index of the option selected automatically when the time runs out.
        /// </summary>
        public int timeoutChoiceIndex;

        /// <summary>
        /// A list of choices.
        /// </summary>
        public List<Choice> choices = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <param name="position">The position in the graph.</param>
        public ChoiceNode(int id, Vector2 position) : base(id, position)
        {
            useTimeout = FeelSpeak.Settings.timeoutEnabledByDefault;
            timeout = FeelSpeak.Settings.defaultTimeoutLength;
        }

        /// <summary>
        /// Adds a choice.
        /// </summary>
        public void AddChoice()
        {
            var choice = new Choice()
            {
                index = choices.Count
            };

            choices.Add(choice);
        }

        /// <summary>
        /// Updates the choice indices.
        /// </summary>
        protected void UpdateIndices()
        {
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                choice.index = i;
                choices[i] = choice;
            }
        }

        /// <summary>
        /// Removes a choice at an index.
        /// </summary>
        /// <param name="index">The index of the choice.</param>
        public void RemoveChoice(int index)
        {
            if (index < 0 || index > choices.Count)
            {
                return;
            }

            choices.RemoveAt(index);

            if (index < outputConnections.Count)
            {
                outputConnections.RemoveAt(index);
            }

            UpdateIndices();
        }

        /// <summary>
        /// Moves a choice up.
        /// </summary>
        /// <param name="index">The index of the choice.</param>
        public void MoveChoiceUp(int index)
        {
            if (index <= 0 || index >= choices.Count)
            {
                return;
            }

            int up = index - 1;

            (choices[index], choices[up]) = (choices[up], choices[index]);

            if (index < outputConnections.Count && up < outputConnections.Count)
            {
                (outputConnections[index], outputConnections[up]) = (outputConnections[up], outputConnections[index]);
                (outputConnections[index].outputPortIndex, outputConnections[up].outputPortIndex) = (outputConnections[up].outputPortIndex, outputConnections[index].outputPortIndex);
            }

            UpdateIndices();
        }

        /// <summary>
        /// Moves a choice down.
        /// </summary>
        /// <param name="index">The index of the choice.</param>
        public void MoveChoiceDown(int index)
        {
            if (index < 0 || index >= choices.Count - 1)
            {
                return;
            }

            int down = index + 1;

            (choices[index], choices[down]) = (choices[down], choices[index]);

            if (index < outputConnections.Count && down < outputConnections.Count)
            {
                (outputConnections[index], outputConnections[down]) = (outputConnections[down], outputConnections[index]);
                (outputConnections[index].outputPortIndex, outputConnections[down].outputPortIndex) = (outputConnections[down].outputPortIndex, outputConnections[index].outputPortIndex);
            }

            UpdateIndices();
        }

        /// <summary>
        /// Gets the timeout choice index based on the node properties.
        /// </summary>
        /// <returns>The timeout choice or null if this node doesn't use timeout.</returns>
        public Choice? GetTimeoutChoice()
        {
            if (!useTimeout)
            {
                return null;
            }

            if (timeoutChoiceIsRandom)
            {
                return choices[Random.Range(0, choices.Count)];
            }
            else
            {
                return choices[timeoutChoiceIndex];
            }
        }

        public override bool IsComplete()
        {
            if (!FeelSpeak.Settings || choices == null || choices.Count == 0)
            {
                return true;
            }

            bool choicesComplete = true;

            foreach (var choice in choices)
            {
                if (string.IsNullOrEmpty(choice.text))
                {
                    choicesComplete = false;
                    break;
                }
            }

            return choicesComplete;
        }

        /// <summary>
        /// Gets the choice text of a choice with the specific name.
        /// </summary>
        /// <param name="name">The name of the choice.</param>
        /// <returns>The choice text.</returns>
        public string GetText(string name)
        {
            Choice? choice = null;

            foreach (var item in choices)
            {
                if (item.name == name)
                {
                    choice = item;
                }
            }

            if (!choice.HasValue)
            {
                FeelSpeakLogger.LogWarning($"Feel Speak: choice with the name {name} is not a part of this node!");
                return string.Empty;
            }

            return choice.Value.GetInterpolatedText();
        }

        /// <summary>
        /// Creates a copy of this node.
        /// </summary>
        /// <returns>The copy.</returns>
        public ChoiceNode CreateCopy()
        {
            var copy = new ChoiceNode(id, position);
            copy.graphId = graphId;
            copy.useTimeout = useTimeout;
            copy.timeout = timeout;
            copy.timeoutChoiceIsRandom = timeoutChoiceIsRandom;
            copy.timeoutChoiceIndex = timeoutChoiceIndex;

            List<Choice> choices = new();

            if (this.choices != null)
            {
                foreach (var choice in this.choices)
                {
                    var choiceCopy = choice;
                    choices.Add(choiceCopy);
                }
            }

            copy.choices = choices;
            return copy;
        }

        public override void InvokeEvent()
        {
            Dialogue.onChoiceNodeTriggered.Invoke(this);
        }
    }
}