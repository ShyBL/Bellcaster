//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Graph.Data;
using Esper.FeelSpeak.UI.UGUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// The runtime version of a DialogueGraph.
    /// </summary>
    public class Dialogue
    {
        /// <summary>
        /// The dialogue graph that this has been loaded. Use Prepare to load a graph.
        /// </summary>
        public DialogueGraph dialogueGraph;

        /// <summary>
        /// A list of all nodes. This is populated when Prepare is called.
        /// </summary>
        public Dictionary<int, Node> nodes = new();

        /// <summary>
        /// The node that the dialogue always starts with.
        /// </summary>
        public Node startingNode;

        /// <summary>
        /// The current node.
        /// </summary>
        public Node currentNode;

        /// <summary>
        /// The previous node.
        /// </summary>
        public Node previousNode;

        /// <summary>
        /// If the dialogue has started (if Next was called at least once).
        /// </summary>
        public bool started;

        /// <summary>
        /// A callback for when any dialogue node in any dialogue is triggered. This accepts 1 argument: the dialogue
        /// node (DialogueNode).
        /// </summary>
        public static UnityEvent<DialogueNode> onDialogueNodeTriggered = new();

        /// <summary>
        /// A callback for when any choice node in any dialogue is triggered. This accepts 1 argument: the choice
        /// node (ChoiceNode).
        /// </summary>
        public static UnityEvent<ChoiceNode> onChoiceNodeTriggered = new();

        /// <summary>
        /// A callback for when any delay node in any dialogue is triggered. This accepts 1 argument: the delay
        /// node (DelayNode).
        /// </summary>
        public static UnityEvent<DelayNode> onDelayNodeTriggered = new();

        /// <summary>
        /// A callback for when any sound node in any dialogue is triggered. This accepts 1 argument: the sound
        /// node (SoundNode).
        /// </summary>
        public static UnityEvent<SoundNode> onSoundNodeTriggered = new();

        /// <summary>
        /// A callback for when any choice is made. This accepts 1 argument: the choice made (Choice).
        /// </summary>
        public static UnityEvent<Choice> onChoiceMade = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dialogueGraph">The dialogue graph.</param>
        public Dialogue(DialogueGraph dialogueGraph)
        {
            this.dialogueGraph = dialogueGraph;
        }

        /// <summary>
        /// Prepares the dialogue. Preparation is required before the dialogue can be used.
        /// </summary>
        /// <returns>True is the result if the preparation was successful. Otherwise, false.</returns>
        public bool Prepare()
        {
            if (!dialogueGraph)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: attempted to prepare dialogue while the dialogue graph refrence is null.");
                return false;
            }

            dialogueGraph.ValidateConnectionsInNodes();
            nodes.Clear();

            foreach (var node in dialogueGraph.dialogueNodes)
            {
                nodes.Add(node.id, node);
            }

            foreach (var node in dialogueGraph.choiceNodes)
            {
                nodes.Add(node.id, node);
            }

            foreach (var node in dialogueGraph.delayNodes)
            {
                nodes.Add(node.id, node);
            }

            foreach (var node in dialogueGraph.soundNodes)
            {
                nodes.Add(node.id, node);
            }

            if (nodes.Count == 0)
            {
                FeelSpeakLogger.LogWarning("Feel Speak: attempted to prepare dialogue while the dialogue graph has no nodes.");
                return false;
            }

            startingNode = nodes[dialogueGraph.startingNodeId];
            started = false;

            return true;
        }

        /// <summary>
        /// Asynchronously prepares the dialogue. Preparation is required before the dialogue can be used.
        /// </summary>
        /// <returns>True is the result if the preparation was successful. Otherwise, false.</returns>
        public Task<bool> PrepareAsync()
        {
            return Task.Run(() => Prepare());
        }

        /// <summary>
        /// Gets the next node based on the properties and connections of the current node. This does not update the 
        /// currentNode field.
        /// </summary>
        /// <param name="choiceIndex">The index of the choice made. This is only relevant if the current node is a choice
        /// node.</param>
        /// <returns>The next node.</returns>
        public Node ReadNext(int? choiceIndex)
        {
            Node currentNode = this.currentNode;
            return Next(choiceIndex, ref currentNode, true);
        }

        /// <summary>
        /// Gets the next node based on the properties and connections of the current node. This updates the currentNode field.
        /// </summary>
        /// <param name="choiceIndex">The index of the choice made. This is only relevant if the current node is a choice
        /// node.</param>
        /// <returns>The next node.</returns>
        public Node Next(int? choiceIndex)
        {
            previousNode = currentNode;
            return Next(choiceIndex, ref currentNode, false);
        }

        /// <summary>
        /// Gets the next node based on the properties and connections of the current node. This updates the currentNode field.
        /// </summary>
        /// <param name="choiceIndex">The index of the choice made. This is only relevant if the current node is a choice
        /// node.</param>
        /// <param name="currentNode">The node to start from.</param>
        /// <param name="readOnly">If read only mode should be used.</param>
        /// <returns>The next node.</returns>
        protected Node Next(int? choiceIndex, ref Node currentNode, bool readOnly)
        {
            if (!started)
            {
                if (!readOnly)
                {
                    started = true;
                }

                currentNode = startingNode;

                if (currentNode != null && !currentNode.IsImmediateExecutionNode)
                {
                    currentNode.InvokeEvent();
                    return currentNode;
                }
            }

            Node next;
            var visitedImmediateExecutionNodes = new HashSet<Node>();

            do
            {
                next = null;

                if (currentNode.IsImmediateExecutionNode && !visitedImmediateExecutionNodes.Add(currentNode))
                {
                    if (DialogueBox.Instance)
                    {
                        DialogueBox.Instance.EndDialogue();
                    }

                    FeelSpeakLogger.LogError($"Feel Speak: Detected an infinite loop caused by immediate execution nodes. Forcefully exited dialogue. Dialogue graph: {dialogueGraph.graphName}.");
                    break;
                }

                if (currentNode is DialogueNode dialogueNode)
                {
                    if (dialogueNode.outputConnections.Count > 0)
                    {
                        next = nodes[dialogueNode.outputConnections[0].inputNodeId];
                    }
                }
                else if (currentNode is ChoiceNode choiceNode)
                {
                    if (!choiceIndex.HasValue)
                    {
                        FeelSpeakLogger.LogWarning("Feel Speak: making a choice is required. Choice index cannot be null.");
                        return null;
                    }

                    int connectionIndex = -1;

                    for (int i = 0; i < choiceNode.outputConnections.Count; i++)
                    {
                        var connection = choiceNode.outputConnections[i];

                        if (choiceIndex == connection.outputPortIndex)
                        {
                            connectionIndex = i;
                            break;
                        }
                    }

                    if (connectionIndex == -1)
                    {
                        return null;
                    }

                    if (!readOnly)
                    {
                        onChoiceMade.Invoke(choiceNode.choices[choiceIndex.Value]);
                    }

                    next = nodes[choiceNode.outputConnections[connectionIndex].inputNodeId];
                }
                else if (currentNode is DelayNode delayNode)
                {
                    if (delayNode.outputConnections.Count > 0)
                    {
                        next = nodes[delayNode.outputConnections[0].inputNodeId];
                    }
                }
                else if (currentNode is SoundNode soundNode)
                {
                    if (!readOnly)
                    {
                        FeelSpeak.PlaySound(soundNode.sound);
                    }

                    if (soundNode.outputConnections.Count > 0)
                    {
                        next = nodes[soundNode.outputConnections[0].inputNodeId];
                    }
                }

                if (next == null)
                {
                    break;
                }

                currentNode = next;
                currentNode.InvokeEvent();

            } while (!readOnly && next.IsImmediateExecutionNode);

            return next;
        }
    }
}