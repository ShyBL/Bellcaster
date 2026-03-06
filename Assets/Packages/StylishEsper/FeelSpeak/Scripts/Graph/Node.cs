//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// The base graph node.
    /// </summary>
    [System.Serializable]
    public abstract class Node : GraphElement
    {
        /// <summary>
        /// The node ID.
        /// </summary>
        public int id;

        /// <summary>
        /// The position in the graph.
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// A list of all input connections.
        /// </summary>
        public List<Connection> inputConnections = new();

        /// <summary>
        /// A list of all output connections.
        /// </summary>
        public List<Connection> outputConnections = new();

        /// <summary>
        /// If this node type executes immediately during dialogue.
        /// </summary>
        public bool IsImmediateExecutionNode { get => this is SoundNode; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The node ID.</param>
        /// <param name="position">The position in the graph.</param>
        public Node(int id, Vector2 position) 
        {
            this.id = id;
            this.position = position;
        }

        /// <summary>
        /// Checks if the data is required to input in the node.
        /// </summary>
        public abstract bool IsComplete();

        /// <summary>
        /// Invokes this node's global event.
        /// </summary>
        public abstract void InvokeEvent();
    }
}