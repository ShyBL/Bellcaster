//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using UnityEngine;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// A node that plays a sound.
    /// </summary>
    [System.Serializable]
    public class SoundNode : Node
    {
        /// <summary>
        /// The sound to play.
        /// </summary>
        public AudioClip sound;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <param name="position">The position in the graph.</param>
        public SoundNode(int id, Vector2 position) : base(id, position)
        {

        }

        public override bool IsComplete()
        {
            return true;
        }

        /// <summary>
        /// Creates a copy of this node.
        /// </summary>
        /// <returns>The copy.</returns>
        public SoundNode CreateCopy()
        {
            var copy = new SoundNode(id, position);
            copy.graphId = graphId;
            copy.sound = sound;
            return copy;
        }

        public override void InvokeEvent()
        {
            Dialogue.onSoundNodeTriggered.Invoke(this);
        }
    }
}