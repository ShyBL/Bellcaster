//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Database;
using UnityEngine.Events;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using Esper.FeelSpeak.Editor;
using System.IO;
#endif
using System.Collections.Generic;

namespace Esper.FeelSpeak.Graph
{
    /// <summary>
    /// A graph that contains nodes and connections.
    /// </summary>
    public class DialogueGraph : FeelSpeakObject
    {
        /// <summary>
        /// The searchable name of the graph.
        /// </summary>
        public string graphName;

        /// <summary>
        /// The ID of the starting node. This is equal to -1 if a starting node has not been set.
        /// </summary>
        public int startingNodeId = -1;

        /// <summary>
        /// A list of dialogue nodes.
        /// </summary>
        public List<DialogueNode> dialogueNodes = new();

        /// <summary>
        /// A list of choice nodes.
        /// </summary>
        public List<ChoiceNode> choiceNodes = new();

        /// <summary>
        /// A list of delay nodes.
        /// </summary>
        public List<DelayNode> delayNodes = new();

        /// <summary>
        /// A list of sound nodes.
        /// </summary>
        public List<SoundNode> soundNodes = new();

        /// <summary>
        /// A list of all connections.
        /// </summary>
        public List<Connection> connections = new();

        /// <summary>
        /// A callback for when any dialogue graph begins. All events are cleared after invoke. This accepts 1 argument: 
        /// the dialogue graph (DialogueGraph).
        /// </summary>
        public UnityEvent callOnceOnDialogueStarted = new();

        /// <summary>
        /// A callback for when any dialogue graph ends. All events are cleared after invoke. This accepts 1 argument: 
        /// the dialogue graph (DialogueGraph).
        /// </summary>
        public UnityEvent callOnceOnDialogueEnded = new();

        /// <summary>
        /// The database record.
        /// </summary>
        public DialogueRecord DatabaseRecord
        {
            get
            {
                return new DialogueRecord
                {
                    id = id,
                    objectName = name,
                    graphName = graphName
                };
            }
        }

        /// <summary>
        /// The path to all generated objects of this type relative to the resources folder.
        /// </summary>
        public static string resourcesPath = "FeelSpeakResources/Dialogues";

#if UNITY_EDITOR
        /// <summary>
        /// The directory of all generated objects of this type. Works in the editor only.
        /// </summary>
        public static string DirectoryPath { get => Path.Combine(AssetSearch.FolderOf<TextAsset>("FeelSpeakIdentifier"), "Resources", "FeelSpeakResources", "Dialogues"); }

        /// <summary>
        /// Deletes the record of this object from the database.
        /// </summary>
        public void DeleteDatabaseRecord()
        {
            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }

            if (FeelSpeakDatabase.HasDialogueRecord(id))
            {
                FeelSpeakDatabase.DeleteDialogueRecord(DatabaseRecord);
            }

            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }
        }
#endif

        /// <summary>
        /// Updates the record of this object in the database.
        /// </summary>
        public void UpdateDatabaseRecord()
        {
#if UNITY_EDITOR
            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }
#endif

            if (FeelSpeakDatabase.HasDialogueRecord(id))
            {
                FeelSpeakDatabase.UpdateDialogueRecord(DatabaseRecord);
            }
            else
            {
                FeelSpeakDatabase.InsertDialogueRecord(DatabaseRecord);
            }

#if UNITY_EDITOR
            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }
#endif
        }

        /// <summary>
        /// Adds a node.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddNode(Node node)
        {
            if (node is DialogueNode dialogueNode)
            {
                dialogueNodes.Add(dialogueNode);
            }
            else if (node is ChoiceNode choiceNode)
            {
                choiceNodes.Add(choiceNode);
            }
            else if (node is DelayNode delayNode)
            {
                delayNodes.Add(delayNode);
            }
            else if (node is SoundNode soundNode)
            {
                soundNodes.Add(soundNode);
            }
        }

        /// <summary>
        /// Removes a node.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public void RemoveNode(Node node)
        {
            if (node is DialogueNode dialogueNode)
            {
                dialogueNodes.Remove(dialogueNode);
            }
            else if (node is ChoiceNode choiceNode)
            {
                choiceNodes.Remove(choiceNode);
            }
            else if (node is DelayNode delayNode)
            {
                delayNodes.Remove(delayNode);
            }
            else if (node is SoundNode soundNode)
            {
                soundNodes.Remove(soundNode);
            }
        }

        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        /// <returns>The copy.</returns>
        public DialogueGraph CreateCopy()
        {
            List<DialogueNode> dialogueNodes = new();
            List<ChoiceNode> choiceNodes = new();
            List<DelayNode> delayNodes = new();
            List<SoundNode> soundNodes = new();
            List<Connection> connections = new();

            if (this.dialogueNodes != null)
            {
                foreach (var node in this.dialogueNodes)
                {
                    var nodeCopy = node.CreateCopy();
                    dialogueNodes.Add(nodeCopy);
                }
            }

            if (this.choiceNodes != null)
            {
                foreach (var node in this.choiceNodes)
                {
                    var nodeCopy = node.CreateCopy();
                    choiceNodes.Add(nodeCopy);
                }
            }

            if (this.delayNodes != null)
            {
                foreach (var node in this.delayNodes)
                {
                    var nodeCopy = node.CreateCopy();
                    delayNodes.Add(nodeCopy);
                }
            }

            if (this.soundNodes != null)
            {
                foreach (var node in this.soundNodes)
                {
                    var nodeCopy = node.CreateCopy();
                    soundNodes.Add(nodeCopy);
                }
            }

            if (this.connections != null)
            {
                foreach (var connection in this.connections)
                {
                    var connectionCopy = connection.CreateCopy();
                    connections.Add(connectionCopy);
                }
            }

            DialogueGraph copy = CreateInstance<DialogueGraph>();
            copy.id = id;
            copy.startingNodeId = startingNodeId;
            copy.graphName = graphName;
            copy.dialogueNodes = dialogueNodes;
            copy.choiceNodes = choiceNodes;
            copy.delayNodes = delayNodes;
            copy.soundNodes = soundNodes;
            copy.connections = connections;

            return copy;
        }

        /// <summary>
        /// Copies all data of another dialogue, excluding ID.
        /// </summary>
        /// <param name="other">The dialogue graph to copy.</param>
        public void CopyData(DialogueGraph other)
        {
            startingNodeId = other.startingNodeId;
            graphName = other.graphName;
            dialogueNodes = other.dialogueNodes;
            choiceNodes = other.choiceNodes;
            delayNodes = other.delayNodes;
            soundNodes = other.soundNodes;
            connections = other.connections;
        }

        /// <summary>
        /// Creates a new instance of a DialogueGraph (editor only).
        /// </summary>
        /// <returns>The created instance.</returns>
        public static DialogueGraph Create()
        {
#if UNITY_EDITOR
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
#endif
            
            var obj = CreateInstance<DialogueGraph>();
            var id = obj.GetID<DialogueGraph>();
            obj.id = id;
            var name = "New Dialogue";
            obj.graphName = name;

#if UNITY_EDITOR
            var path = Path.Combine(DirectoryPath, $"{id}_{name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(obj, path);
            obj.Save();
#endif

            return obj;
        }

        /// <summary>
        /// Updates the name of the asset (editor only).
        /// </summary>
        public void UpdateAssetName()
        {
#if UNITY_EDITOR
            graphName = SanitizeName(graphName);

            string name = $"{id}_{graphName}";
            UnityEditor.EditorApplication.delayCall += () =>
            {
                UnityEditor.AssetDatabase.RenameAsset(GetFullPath(this), name);

                UnityEditor.EditorApplication.delayCall += () =>
                {
                    this.name = name;
                    Save();
                };
            };
#endif
        }

        public override void Save()
        {
#if UNITY_EDITOR
            base.Save();
            UpdateDatabaseRecord();
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gets the full path of a graph (editor only).
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <returns>The full path to the graph.</returns>
        public static string GetFullPath(DialogueGraph graph)
        {
            return Path.Combine(DirectoryPath, $"{graph.name}.asset");
        }
#endif

        protected override int GetID<T>(string pathInResources = null)
        {
            return base.GetID<T>(resourcesPath);
        }

        /// <summary>
        /// Returns an unused node ID.
        /// </summary>
        /// <returns>An unused node ID.</returns>
        public virtual int GetAvailableNodeID()
        {
            List<int> ids = new();

            foreach (var node in dialogueNodes)
            {
                ids.Add(node.id);
            }

            foreach (var node in choiceNodes)
            {
                ids.Add(node.id);
            }

            foreach (var node in delayNodes)
            {
                ids.Add(node.id);
            }

            foreach (var node in soundNodes)
            {
                ids.Add(node.id);
            }

            for (int i = 0; i < int.MaxValue; i++)
            {
                if (!ids.Contains(i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Triggers this dialogue.
        /// </summary>
        public void Trigger()
        {
            FeelSpeak.TriggerDialogue(this);
        }

        /// <summary>
        /// Applies all connections to nodes.
        /// </summary>
        public void ValidateConnectionsInNodes()
        {
            foreach (var node in dialogueNodes)
            {
                PopulateConnections(node);
            }

            foreach (var node in choiceNodes)
            {
                PopulateConnections(node);
            }

            foreach (var node in delayNodes)
            {
                PopulateConnections(node);
            }

            foreach (var node in soundNodes)
            {
                PopulateConnections(node);
            }
        }

        /// <summary>
        /// Repopulates the connection lists of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void PopulateConnections(Node node)
        {
            node.inputConnections.Clear();
            node.outputConnections.Clear();

            foreach (var connection in connections)
            {
                if (connection.inputNodeId == node.id)
                {
                    node.inputConnections.Add(connection);
                }
                else if (connection.outputNodeId == node.id)
                {
                    node.outputConnections.Add(connection);
                }
            }

            node.inputConnections = node.inputConnections.OrderBy(x => x.inputPortIndex).ToList();
            node.outputConnections = node.outputConnections.OrderBy(x => x.outputPortIndex).ToList();
        }
    }
}