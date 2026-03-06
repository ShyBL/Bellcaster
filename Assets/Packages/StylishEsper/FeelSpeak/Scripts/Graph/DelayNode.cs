//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// A node that delays dialogue continuation.
    /// </summary>
    [System.Serializable]
    public class DelayNode : Node
    {
        /// <summary>
        /// The delay.
        /// </summary>
        public float delay;

        /// <summary>
        /// If the dialogue box UI should be hidden while this delay is active.
        /// </summary>
        public bool hideDialogueBox;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <param name="position">The position in the graph.</param>
        public DelayNode(int id, Vector2 position) : base(id, position)
        {
            delay = FeelSpeak.Settings.defaultDelay;
            hideDialogueBox = FeelSpeak.Settings.defaultHideDialogueBox;
        }

        public override bool IsComplete()
        {
            return true;
        }

        /// <summary>
        /// Creates a copy of this node.
        /// </summary>
        /// <returns>The copy.</returns>
        public DelayNode CreateCopy()
        {
            var copy = new DelayNode(id, position);
            copy.graphId = graphId;
            copy.delay = delay;
            copy.hideDialogueBox = hideDialogueBox;
            return copy;
        }

        public override void InvokeEvent()
        {
            Dialogue.onDelayNodeTriggered.Invoke(this);
        }
    }
}