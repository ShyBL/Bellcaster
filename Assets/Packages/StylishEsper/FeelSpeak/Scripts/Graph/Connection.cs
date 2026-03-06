//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// A connection between two nodes.
    /// </summary>
    [System.Serializable]
    public class Connection : GraphElement
    {
        /// <summary>
        /// The output node ID.
        /// </summary>
        public int outputNodeId;

        /// <summary>
        /// The input node ID.
        /// </summary>
        public int inputNodeId;

        /// <summary>
        /// The output port index.
        /// </summary>
        public int outputPortIndex;

        /// <summary>
        /// The input port index.
        /// </summary>
        public int inputPortIndex;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputNodeId">The output node ID.</param>
        /// <param name="inputNodeId">The input node ID.</param>
        /// <param name="outputPortIndex">The output port index.</param>
        /// <param name="inputPortIndex">The input port index.</param>
        public Connection(int outputNodeId, int inputNodeId, int outputPortIndex, int inputPortIndex)
        {
            this.outputNodeId = outputNodeId;
            this.inputNodeId = inputNodeId;
            this.outputPortIndex = outputPortIndex;
            this.inputPortIndex = inputPortIndex;
        }

        /// <summary>
        /// If the data matches this node's conection data.
        /// </summary>
        /// <param name="outputNodeId">The output node ID.</param>
        /// <param name="inputNodeId">The input node ID.</param>
        /// <param name="outputPortIndex">The output port index.</param>
        /// <param name="inputPortIndex">The input port index.</param>
        /// <returns>True if all data matches. Otherwise, false.</returns>
        public bool Matches(int outputNodeId, int inputNodeId, int outputPortIndex, int inputPortIndex)
        {
            return this.outputNodeId == outputNodeId && this.inputNodeId == inputNodeId && this.outputPortIndex == outputPortIndex && this.inputPortIndex == inputPortIndex;
        }

        /// <summary>
        /// If another connection matches this connection.
        /// </summary>
        /// <param name="connection">The other connection.</param>
        /// <returns>True if all data matches. Otherwise, false.</returns>
        public bool Matches(Connection connection)
        {
            return outputNodeId == connection.outputNodeId && inputNodeId == connection.inputNodeId && outputPortIndex == connection.outputPortIndex && inputPortIndex == connection.inputPortIndex;
        }

        /// <summary>
        /// Creates a copy of this connection.
        /// </summary>
        /// <returns>The copy.</returns>
        public Connection CreateCopy()
        {
            var copy = new Connection(outputNodeId, inputNodeId, outputPortIndex, inputPortIndex);
            copy.graphId = graphId;
            return copy;
        }
    }
}