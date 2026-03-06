//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class DialogueGraphView : GraphView
    {
#elif UNITY_2022
    public class DialogueGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<DialogueGraphView, UxmlTraits> { }
#endif
        public DialogueGraph graph;
        public UnityEvent onGraphChanged = new();
        public NodeInspector nodeInspector;
        public NodeSelector nodeSelector;
        public Dictionary<int, GraphNodeEditor> loadedNodeElements = new();
        private GraphNodeEditor nodeToReplace;

        private DialogueGraph temp;
        private List<Graph.GraphElement> copiedElements = new();

        private Vector2 mouseDownPosition;
        private Vector2 mouseUpPosition;
        private Vector2 mousePosition;
        private Vector2 unscaledMouseDownPosition;
        private Vector2 unscaledMouseUpPosition;
        private Vector2 unscaledMousePosition;

        public bool hasUnsavedChanges;
        private bool graphChangeLock;
        private bool isReplacingNode;

        public DialogueGraphView()
        {
            Insert(0, new GridBackground());

            var contentZoomer = new ContentZoomer();
            contentZoomer.minScale = 0.05f;
            contentZoomer.maxScale = 2f;

            this.AddManipulator(contentZoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            RegisterCallback<MouseDownEvent>(evt =>
            {
                unscaledMouseDownPosition = evt.localMousePosition;
                var view = viewTransform as VisualElement;
                mouseDownPosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
            });

            RegisterCallback<MouseUpEvent>(evt =>
            {
                unscaledMouseUpPosition = evt.localMousePosition;
                var view = viewTransform as VisualElement;
                mouseUpPosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
            });

            RegisterCallback<MouseMoveEvent>(evt =>
            {
                unscaledMousePosition = evt.localMousePosition;
                var view = viewTransform as VisualElement;
                mousePosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
            });

            RegisterCallback<PointerDownEvent>(evt =>
            {
                nodeSelector.Close();
            });

            var styleSheet = AssetSearch.FindAsset<StyleSheet>("FeelSpeak", "DialogueGraphEditorWindow");
            styleSheets.Add(styleSheet);

            graphViewChanged += OnGraphChanged;
        }

        public void Initialize(Toolbar nodeSelectorRoot)
        {
            nodeSelector = new NodeSelector(nodeSelectorRoot, this);

            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                nodeInspector.ClampOnScreen(evt);
            });

            CloseNodeSelector();
        }

        public int GetSelectedNodeCount()
        {
            return selection
                .OfType<UnityEditor.Experimental.GraphView.Node>()
                .Count();
        }

        private GraphViewChange OnGraphChanged(GraphViewChange graphViewChange)
        {
            if (!graph || graphChangeLock)
            {
                return graphViewChange;
            }

            Undo.RegisterCompleteObjectUndo(graph, "Graph Change");

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var item in graphViewChange.elementsToRemove)
                {
                    if (item is Edge)
                    {
                        var edge = item as Edge;
                        var outputNode = (GraphNodeEditor)edge.output.node;
                        var inputNode = (GraphNodeEditor)edge.input.node;
                        var outputPortIndex = 0;
                        var inputPortIndex = 0;

                        for (int i = 0; i < edge.output.node.outputContainer.childCount; i++)
                        {
                            if (edge.output == (Port)edge.output.node.outputContainer.ElementAt(i))
                            {
                                outputPortIndex = i;
                                break;
                            }
                        }

                        for (int i = 0; i < edge.input.node.inputContainer.childCount; i++)
                        {
                            if (edge.input == (Port)edge.input.node.inputContainer.ElementAt(i))
                            {
                                inputPortIndex = i;
                                break;
                            }
                        }

                        var connectionToRemove = new Connection(outputNode.Id, inputNode.Id, outputPortIndex, inputPortIndex);

                        foreach (var connection in graph.connections)
                        {
                            if (connection.Matches(connectionToRemove))
                            {
                                graph.connections.Remove(connection);
                                break;
                            }
                        }
                    }
                    else if (item is DialogueNodeEditor)
                    {
                        var node = item as DialogueNodeEditor;
                        graph.dialogueNodes.Remove(node.Value);
                        loadedNodeElements.Remove(node.Id);

                        if (nodeInspector.GetSelectedNodeId() == node.Id)
                        {
                            nodeInspector.SetNone();
                        }

                        if (node.Id == graph.startingNodeId)
                        {
                            SetStartingNode(null);
                        }
                    }
                    else if (item is ChoiceNodeEditor)
                    {
                        var node = item as ChoiceNodeEditor;
                        graph.choiceNodes.Remove(node.Value);
                        loadedNodeElements.Remove(node.Id);

                        if (nodeInspector.GetSelectedNodeId() == node.Id)
                        {
                            nodeInspector.SetNone();
                        }

                        if (node.Id == graph.startingNodeId)
                        {
                            SetStartingNode(null);
                        }
                    }
                    else if (item is DelayNodeEditor)
                    {
                        var node = item as DelayNodeEditor;
                        graph.delayNodes.Remove(node.Value);
                        loadedNodeElements.Remove(node.Id);

                        if (nodeInspector.GetSelectedNodeId() == node.Id)
                        {
                            nodeInspector.SetNone();
                        }

                        if (node.Id == graph.startingNodeId)
                        {
                            SetStartingNode(null);
                        }
                    }
                    else if (item is SoundNodeEditor)
                    {
                        var node = item as SoundNodeEditor;
                        graph.soundNodes.Remove(node.Value);
                        loadedNodeElements.Remove(node.Id);

                        if (nodeInspector.GetSelectedNodeId() == node.Id)
                        {
                            nodeInspector.SetNone();
                        }

                        if (node.Id == graph.startingNodeId)
                        {
                            SetStartingNode(null);
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var connection = GetConnectionData(edge);
                    bool matchFound = false;

                    foreach (var existingConnection in graph.connections)
                    {
                        if (existingConnection.Matches(connection))
                        {
                            schedule.Execute(x =>
                            {
                                edge.input.Disconnect(edge);
                                edge.output.Disconnect(edge);
                                RemoveElement(edge);
                            });

                            matchFound = true;
                            break;
                        }
                    }

                    if (!matchFound)
                    {
                        graph.connections.Add(connection);
                    }
                }
            }

            if (graphViewChange.movedElements != null)
            {
                foreach (var item in graphViewChange.movedElements)
                {
                    if (item is GraphNodeEditor)
                    {
                        var node = item as GraphNodeEditor;
                        node.value.position = new Vector2(item.resolvedStyle.left, item.resolvedStyle.top);
                    }
                }
            }

            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
            return graphViewChange;
        }

        public Connection GetConnectionData(Edge edge)
        {
            var outputNode = (GraphNodeEditor)edge.output.node;
            var inputNode = (GraphNodeEditor)edge.input.node;
            var outputPortIndex = 0;
            var inputPortIndex = 0;

            for (int i = 0; i < edge.output.node.outputContainer.childCount; i++)
            {
                if (edge.output == (Port)edge.output.node.outputContainer.ElementAt(i))
                {
                    outputPortIndex = i;
                    break;
                }
            }

            for (int i = 0; i < edge.input.node.inputContainer.childCount; i++)
            {
                if (edge.input == (Port)edge.input.node.inputContainer.ElementAt(i))
                {
                    inputPortIndex = i;
                    break;
                }
            }

            var connection = new Connection(outputNode.value.id, inputNode.value.id, outputPortIndex, inputPortIndex);
            connection.graphId = graph.id;
            return connection;
        }

        public Connection GetConnection(int outputNodeId, int inputNodeId, int outputPortIndex, int inputPortIndex)
        {
            foreach (var connection in graph.connections)
            {
                if (connection.Matches(outputNodeId, inputNodeId, outputPortIndex, inputPortIndex))
                {
                    return connection;
                }
            }

            return null;
        }

        public List<Graph.GraphElement> GetSelection()
        {
            var selection = new List<Graph.GraphElement>();

            foreach (var item in this.selection)
            {
                if (item is GraphNodeEditor)
                {
                    var nodeElement = item as GraphNodeEditor;
                    selection.Add(nodeElement.value);
                }
                else if (item is Edge)
                {
                    var edge = item as Edge;
                    selection.Add(GetConnectionData(edge));
                }
            }

            return selection;
        }

        public void Refresh()
        {
            var selectedNodeId = nodeInspector.GetSelectedNodeId();
            DisplayGraph(graph);

            if (selectedNodeId != -1)
            {
                if (!loadedNodeElements.ContainsKey(selectedNodeId))
                {
                    return;
                }

                var selectedNode = loadedNodeElements[selectedNodeId];

                if (selectedNode.value is DialogueNode dialogueNode)
                {
                    nodeInspector.SetTarget(dialogueNode);
                }
                else if (selectedNode.value is ChoiceNode choiceNode)
                {
                    nodeInspector.SetTarget(choiceNode);
                }
                else if (selectedNode.value is DelayNode delayNode)
                {
                    nodeInspector.SetTarget(delayNode);
                }
                else if (selectedNode.value is SoundNode soundNode)
                {
                    nodeInspector.SetTarget(soundNode);
                }
            }
        }

        public void RefreshNode(Graph.Node node)
        {
            loadedNodeElements[node.id].Refresh();
        }

        public T GetNodeElement<T>(Graph.Node node) where T : GraphNodeEditor
        {
            return (T)loadedNodeElements[node.id];
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = ports.Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
            return compatiblePorts;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!graph)
            {
                return;
            }

            if (selection.Count > 0)
            {
                evt.menu.AppendAction("Cut", x => CutSelection());
                evt.menu.AppendAction("Copy", x => CopySelection());
                evt.menu.AppendAction("Delete", x => DeleteSelection());
            }

            if (copiedElements.Count > 0)
            {
                evt.menu.AppendAction("Paste", x => PasteSelection());
            }

            evt.menu.AppendSeparator();

            if (selection.Count == 1 && selection[0] is GraphNodeEditor node)
            {
                nodeToReplace = node;
                evt.menu.AppendAction("Replace Node", x => OpenNodeSelector(true));
            }
            else
            {
                nodeToReplace = null;
            }

            evt.menu.AppendAction("Add Node", x => OpenNodeSelector(false));
        }

        public void OpenNodeSelector(bool replace)
        {
            isReplacingNode = replace;
            var position = unscaledMouseUpPosition - new Vector2(0, 20);
            nodeSelector.Open(position);
        }

        public void CloseNodeSelector()
        {
            nodeSelector.Close();
        }

        public void SetStartingNode(Graph.Node node)
        {
            var temp = graph.startingNodeId;

            if (node != null)
            {
                graph.startingNodeId = node.id;
                loadedNodeElements[node.id].Refresh();
            }
            else
            {
                graph.startingNodeId = -1;
            }

            if (temp != -1 && loadedNodeElements.ContainsKey(temp))
            {
                loadedNodeElements[temp].Refresh();
            }
        }

        public void DisplayGraph(DialogueGraph graph)
        {
            graphChangeLock = true;

            this.graph = graph;

            DeleteElements(graphElements);

            if (graph)
            {
                temp = graph.CreateCopy();
                loadedNodeElements.Clear();

                foreach (var node in graph.dialogueNodes)
                {
                    CreateDialogueNodeElement(node);
                }

                foreach (var node in graph.choiceNodes)
                {
                    CreateChoiceNodeElement(node);
                }

                foreach (var node in graph.delayNodes)
                {
                    CreateDelayNodeElement(node);
                }

                foreach (var node in graph.soundNodes)
                {
                    CreateSoundNodeElement(node);
                }

                List<Connection> brokenConnections = new();

                foreach (var connection in graph.connections)
                {
                    try
                    {
                        var outputNode = loadedNodeElements[connection.outputNodeId];
                        var inputNode = loadedNodeElements[connection.inputNodeId];

                        if (outputNode.outputPorts.Count < connection.outputPortIndex + 1 || inputNode.inputPorts.Count < connection.inputPortIndex + 1)
                        {
                            brokenConnections.Add(connection);
                        }
                        else
                        {
                            var edge = outputNode.outputPorts[connection.outputPortIndex].ConnectTo(inputNode.inputPorts[connection.inputPortIndex]);
                            AddElement(edge);
                        }
                    }
                    catch
                    {
                        brokenConnections.Add(connection);
                    }
                }

                foreach (var connection in brokenConnections)
                {
                    graph.connections.Remove(connection);
                }
            }

            hasUnsavedChanges = false;
            graphChangeLock = false;

            nodeInspector.SetNone();
        }

        public void SelectNode(GraphNodeEditor graphNode)
        {
            if (graphNode is DialogueNodeEditor)
            {
                var node = (DialogueNodeEditor)graphNode;
                nodeInspector.SetTarget(node.Value);
            }
            else if (graphNode is ChoiceNodeEditor)
            {
                var node = (ChoiceNodeEditor)graphNode;
                nodeInspector.SetTarget(node.Value);
            }
            else if (graphNode is DelayNodeEditor)
            {
                var node = (DelayNodeEditor)graphNode;
                nodeInspector.SetTarget(node.Value);
            }
            else if (graphNode is SoundNodeEditor)
            {
                var node = (SoundNodeEditor)graphNode;
                nodeInspector.SetTarget(node.Value);
            }
        }

        public void CreateNewDialogueNode()
        {
            Undo.RegisterCompleteObjectUndo(graph, "Graph Change");

            var node = new DialogueNode(graph.GetAvailableNodeID(), mouseUpPosition);
            node.graphId = graph.id;
            graph.dialogueNodes.Add(node);

            if (graph.startingNodeId == -1)
            {
                graph.startingNodeId = node.id;
            }

            if (isReplacingNode)
            {
                node.id = nodeToReplace.Id;
                node.position = nodeToReplace.Position;
                graph.RemoveNode(nodeToReplace.value);
                Refresh();
            }
            else
            {
                CreateDialogueNodeElement(node);
            }

            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
        }

        public void CreateDialogueNodeElement(DialogueNode node)
        {
            var nodeElement = new DialogueNodeEditor(node);
            nodeElement.graphView = this;
            nodeElement.Refresh();

            nodeElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    var view = viewTransform as VisualElement;
                    mouseUpPosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
                    unscaledMouseUpPosition = evt.mousePosition;
                }
            });

            loadedNodeElements.Add(nodeElement.Id, nodeElement);
            AddElement(nodeElement);
        }

        public void CreateNewChoiceNode()
        {
            Undo.RegisterCompleteObjectUndo(graph, "Graph Change");

            var node = new ChoiceNode(graph.GetAvailableNodeID(), mouseUpPosition);
            node.graphId = graph.id;
            graph.choiceNodes.Add(node);

            if (graph.startingNodeId == -1)
            {
                graph.startingNodeId = node.id;
            }

            if (isReplacingNode)
            {
                node.id = nodeToReplace.Id;
                node.position = nodeToReplace.Position;
                graph.RemoveNode(nodeToReplace.value);
                Refresh();
            }
            else
            {
                CreateChoiceNodeElement(node);
            }

            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
        }

        public void CreateChoiceNodeElement(ChoiceNode node)
        {
            var nodeElement = new ChoiceNodeEditor(node);
            nodeElement.graphView = this;
            nodeElement.Refresh();

            nodeElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    var view = viewTransform as VisualElement;
                    mouseUpPosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
                    unscaledMouseUpPosition = evt.mousePosition;
                }
            });

            loadedNodeElements.Add(nodeElement.Id, nodeElement);
            AddElement(nodeElement);
        }

        public void CreateNewDelayNode()
        {
            Undo.RegisterCompleteObjectUndo(graph, "Graph Change");

            var node = new DelayNode(graph.GetAvailableNodeID(), mouseUpPosition);
            node.graphId = graph.id;
            graph.delayNodes.Add(node);

            if (graph.startingNodeId == -1)
            {
                graph.startingNodeId = node.id;
            }

            if (isReplacingNode)
            {
                node.id = nodeToReplace.Id;
                node.position = nodeToReplace.Position;
                graph.RemoveNode(nodeToReplace.value);
                Refresh();
            }
            else
            {
                CreateDelayNodeElement(node);
            }

            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
        }

        public void CreateDelayNodeElement(DelayNode node)
        {
            var nodeElement = new DelayNodeEditor(node);
            nodeElement.graphView = this;
            nodeElement.Refresh();

            nodeElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    var view = viewTransform as VisualElement;
                    mouseUpPosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
                    unscaledMouseUpPosition = evt.mousePosition;
                }
            });

            loadedNodeElements.Add(nodeElement.Id, nodeElement);
            AddElement(nodeElement);
        }

        public void CreateNewSoundNode()
        {
            Undo.RegisterCompleteObjectUndo(graph, "Graph Change");

            var node = new SoundNode(graph.GetAvailableNodeID(), mouseUpPosition);
            node.graphId = graph.id;
            graph.soundNodes.Add(node);

            if (graph.startingNodeId == -1)
            {
                graph.startingNodeId = node.id;
            }

            if (isReplacingNode)
            {
                node.id = nodeToReplace.Id;
                node.position = nodeToReplace.Position;
                graph.RemoveNode(nodeToReplace.value);
                Refresh();
            }
            else
            {
                CreateSoundNodeElement(node);
            }

            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
        }

        public void CreateSoundNodeElement(SoundNode node)
        {
            var nodeElement = new SoundNodeEditor(node);
            nodeElement.graphView = this;
            nodeElement.Refresh();

            nodeElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button == 1)
                {
                    var view = viewTransform as VisualElement;
                    mouseUpPosition = (evt.localMousePosition - (Vector2)view.resolvedStyle.translate) / scale;
                    unscaledMouseUpPosition = evt.mousePosition;
                }
            });

            loadedNodeElements.Add(nodeElement.Id, nodeElement);
            AddElement(nodeElement);
        }
        public void Save()
        {
            if (!graph)
            {
                return;
            }

            graph.Save();
            temp = graph.CreateCopy();
            hasUnsavedChanges = false;
            onGraphChanged.Invoke();
        }

        public void Revert()
        {
            if (!graph)
            {
                return;
            }

            graph.CopyData(temp);
            hasUnsavedChanges = false;
            onGraphChanged.Invoke();
        }

        public void Changed()
        {
            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
        }

        public void CutSelection()
        {
            CopySelection();
            DeleteSelection();
        }

        public void CopySelection()
        {
            copiedElements.Clear();
            var selection = GetSelection();
            selection = selection.OrderByDescending(x => x is Graph.Node).ToList();

            foreach (var element in selection)
            {
                if (element is DialogueNode)
                {
                    var node = element as DialogueNode;
                    copiedElements.Add(node.CreateCopy());
                }
                else if (element is ChoiceNode)
                {
                    var node = element as ChoiceNode;
                    copiedElements.Add(node.CreateCopy());
                }
                else if (element is DelayNode)
                {
                    var node = element as DelayNode;
                    copiedElements.Add(node.CreateCopy());
                }
                else if (element is SoundNode)
                {
                    var node = element as SoundNode;
                    copiedElements.Add(node.CreateCopy());
                }
                else if (element is Connection)
                {
                    var connection = element as Connection;
                    copiedElements.Add(connection.CreateCopy());
                }
            }
        }

        public void PasteSelection()
        {
            Undo.RegisterCompleteObjectUndo(graph, "Graph Change");

            Dictionary<int, int> newIds = new();

            var distancesFromFirst = new List<Vector2>() { Vector2.zero };
            Graph.Node first = null;

            for (int i = 0; i < copiedElements.Count; i++)
            {
                if (copiedElements[i] is Graph.Node)
                {
                    var node = copiedElements[i] as Graph.Node;

                    if (first == null)
                    {
                        first = node;
                    }
                    else
                    {
                        distancesFromFirst.Add(node.position - first.position);
                    }
                }
            }

            if (first == null)
            {
                copiedElements.Clear();
                return;
            }

            int count = 0;
            bool firstNode = true;
            foreach (var element in copiedElements)
            {
                if (element is DialogueNode)
                {
                    var node = (element as DialogueNode).CreateCopy();
                    int prevID = node.id;
                    int newID = graph.GetAvailableNodeID();
                    newIds.Add(prevID, newID);
                    node.id = newID;

                    if (firstNode)
                    {
                        node.position = mousePosition;
                        firstNode = false;
                    }
                    else
                    {
                        node.position = mousePosition + distancesFromFirst[count];
                    }

                    graph.dialogueNodes.Add(node);
                    CreateDialogueNodeElement(node);

                    count++;
                }
                if (element is ChoiceNode)
                {
                    var node = (element as ChoiceNode).CreateCopy();
                    int prevID = node.id;
                    int newID = graph.GetAvailableNodeID();
                    newIds.Add(prevID, newID);
                    node.id = newID;

                    if (firstNode)
                    {
                        node.position = mousePosition;
                        firstNode = false;
                    }
                    else
                    {
                        node.position = mousePosition + distancesFromFirst[count];
                    }

                    graph.choiceNodes.Add(node);
                    CreateChoiceNodeElement(node);

                    count++;
                }
                if (element is DelayNode)
                {
                    var node = (element as DelayNode).CreateCopy();
                    int prevID = node.id;
                    int newID = graph.GetAvailableNodeID();
                    newIds.Add(prevID, newID);
                    node.id = newID;

                    if (firstNode)
                    {
                        node.position = mousePosition;
                        firstNode = false;
                    }
                    else
                    {
                        node.position = mousePosition + distancesFromFirst[count];
                    }

                    graph.delayNodes.Add(node);
                    CreateDelayNodeElement(node);

                    count++;
                }
                if (element is SoundNode)
                {
                    var node = (element as SoundNode).CreateCopy();
                    int prevID = node.id;
                    int newID = graph.GetAvailableNodeID();
                    newIds.Add(prevID, newID);
                    node.id = newID;

                    if (firstNode)
                    {
                        node.position = mousePosition;
                        firstNode = false;
                    }
                    else
                    {
                        node.position = mousePosition + distancesFromFirst[count];
                    }

                    graph.soundNodes.Add(node);
                    CreateSoundNodeElement(node);

                    count++;
                }
                else if (element is Connection)
                {
                    var connection = (element as Connection).CreateCopy();

                    if (newIds.ContainsKey(connection.outputNodeId))
                    {
                        connection.outputNodeId = newIds[connection.outputNodeId];
                    }

                    if (newIds.ContainsKey(connection.inputNodeId))
                    {
                        connection.inputNodeId = newIds[connection.inputNodeId];
                    }

                    graph.connections.Add(connection);
                    var outputNode = loadedNodeElements[connection.outputNodeId];
                    var inputNode = loadedNodeElements[connection.inputNodeId];
                    var edge = outputNode.outputPorts[connection.outputPortIndex].ConnectTo(inputNode.inputPorts[connection.inputPortIndex]);
                    AddElement(edge);
                }
            }

            hasUnsavedChanges = true;
            onGraphChanged.Invoke();
        }

        public override EventPropagation DeleteSelection()
        {
            return base.DeleteSelection();
        }
    }
}
#endif