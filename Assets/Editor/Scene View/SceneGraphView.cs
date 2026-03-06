using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SceneGraphView : GraphView
{
    private SceneGraphData _container;

    public SceneGraphView(SceneGraphData container)
    {
        _container = container;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        graphViewChanged = OnGraphViewChanged;

        PopulateGraph();
    }

    private void PopulateGraph()
    {
        // 1. Get only scenes included in Build Settings
        var buildScenes = EditorBuildSettings.scenes;
        
        foreach (var scene in buildScenes)
        {
            // Get the GUID for the scene asset
            string guid = scene.guid.ToString();
            
            // Get the path and name
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue; // Skip if scene was deleted but still in build settings
            
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            // Find existing saved position or default to center
            var savedNode = _container.Nodes.FirstOrDefault(n => n.GUID == guid);
            Vector2 pos = (savedNode != null) ? savedNode.Position : new Vector2(100, 100);

            CreateSceneNode(guid, name, pos);
        }

        // Optional: Clean up data for scenes no longer in the build
        var buildGuids = buildScenes.Select(s => s.guid.ToString()).ToList();
        _container.Nodes.RemoveAll(n => !buildGuids.Contains(n.GUID));
    }

    private void CreateSceneNode(string guid, string sceneName, Vector2 position)
    {
        // We use the GUID as the viewDataKey so we can identify the node during moves
        var node = new Node { title = sceneName, viewDataKey = guid };

        // Input Port
        var inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "In";
        node.inputContainer.Add(inputPort);

        // Output Port
        var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        outputPort.portName = "Out";
        node.outputContainer.Add(outputPort);

        node.SetPosition(new Rect(position, new Vector2(200, 150)));
        AddElement(node);
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.movedElements != null)
        {
            foreach (var element in change.movedElements)
            {
                if (element is Node node)
                {
                    string guid = node.viewDataKey;
                    var nodeData = _container.Nodes.FirstOrDefault(n => n.GUID == guid);
                    
                    if (nodeData == null)
                    {
                        nodeData = new NodeData { GUID = guid, SceneName = node.title };
                        _container.Nodes.Add(nodeData);
                    }
                    
                    nodeData.Position = node.GetPosition().position;
                }
            }
            EditorUtility.SetDirty(_container);
            AssetDatabase.SaveAssets(); // Ensure it writes to disk immediately
        }
        return change;
    }

    // Overriding this allows us to use the standard "Connect" behavior
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => 
            endPort.direction != startPort.direction && 
            endPort.node != startPort.node).ToList();
    }
}