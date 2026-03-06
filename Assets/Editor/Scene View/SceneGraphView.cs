using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SceneGraphView : GraphView
{
    private SceneGraphData     _container;
    private SceneGraphSettings _settings;
    private SceneGraphEditor   _editor;
    private ColorableGrid      _grid;

    // Keep references to all live nodes so we can re-tint them when settings change
    private readonly List<Node> _sceneNodes = new List<Node>();

    public SceneGraphView(SceneGraphData container, SceneGraphSettings settings, SceneGraphEditor editor)
    {
        _container = container;
        _settings  = settings;
        _editor    = editor;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // Custom painted grid — respects our color settings at runtime
        _grid = new ColorableGrid();
        Insert(0, _grid);
        _grid.StretchToParentSize();

        graphViewChanged = OnGraphViewChanged;

        // Apply colors before populating so nodes get the right tint on creation
        ApplySettings(_settings);
        PopulateGraph();

        // Restore saved viewport (must happen after layout, hence delayCall)
        EditorApplication.delayCall += RestoreViewport;
    }

    // ── Population ────────────────────────────────────────────────────────────

    private void PopulateGraph()
    {
        // 1. Nodes
        foreach (var scene in EditorBuildSettings.scenes)
        {
            string guid = scene.guid.ToString();
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;

            string name      = System.IO.Path.GetFileNameWithoutExtension(path);
            var    savedNode = _container.Nodes.FirstOrDefault(n => n.GUID == guid);
            Vector2 pos      = savedNode != null ? savedNode.Position : new Vector2(100, 100);

            CreateSceneNode(guid, name, pos);
        }

        // 2. Restore saved edges
        foreach (var edgeData in _container.Edges)
        {
            var outputNode = GetNodeByGUID(edgeData.BaseNodeGUID);
            var inputNode  = GetNodeByGUID(edgeData.TargetNodeGUID);
            if (outputNode == null || inputNode == null) continue;

            var outputPort = outputNode.outputContainer.Q<Port>();
            var inputPort  = inputNode.inputContainer.Q<Port>();
            if (outputPort == null || inputPort == null) continue;

            AddElement(outputPort.ConnectTo(inputPort));
        }
    }

    private void CreateSceneNode(string guid, string sceneName, Vector2 position)
    {
        var node = new Node { title = sceneName, viewDataKey = guid };

        // Ports
        var inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "In";
        node.inputContainer.Add(inputPort);

        var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
        outputPort.portName = "Out";
        node.outputContainer.Add(outputPort);

        // Thumbnail
        var imageSection = new VisualElement();
        imageSection.style.paddingTop = imageSection.style.paddingBottom =
            imageSection.style.paddingLeft = imageSection.style.paddingRight = 5;

        var thumbnail = new Image { scaleMode = ScaleMode.ScaleAndCrop };
    
        // FIX 1: Calculate explicit pixel dimensions instead of relying on 100% Flexbox width
        float thumbWidth = _settings.NodeSize.x - 10f; // Node width minus 5px padding on each side
        float thumbHeight = thumbWidth * (9f / 16f);
        thumbnail.style.width  = thumbWidth;
        thumbnail.style.height = thumbHeight;

        Texture2D tex = SceneThumbnailRecorder.GetThumbnail(guid);
        if (tex != null)
        {
            thumbnail.image = tex;
        }
        else
        {
            var placeholder = new Label("No Preview Available");
            placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholder.style.flexGrow = 1;
            placeholder.style.color    = Color.gray;
            thumbnail.Add(placeholder);
            thumbnail.style.backgroundColor = Color.black;
        }

        imageSection.Add(thumbnail);
        node.mainContainer.Add(imageSection);
        node.extensionContainer.style.display = DisplayStyle.None;

        // FIX 2: Explicitly set the node's style width so the background stretches
        node.style.width = _settings.NodeSize.x;

        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(position, _settings.NodeSize));

        AddElement(node);
        _sceneNodes.Add(node);

        // Apply current node colors
        ApplyNodeColors(node);
    }

    // ── Settings / styling ────────────────────────────────────────────────────

    /// <summary>Called from the editor settings panel on any color change.</summary>
    public void ApplySettings(SceneGraphSettings s)
    {
        _settings = s;

        // Drive our custom painted grid
        if (_grid != null)
        {
            _grid.BackgroundColor = s.GridBackgroundColor;
            _grid.LineColor       = s.GridLineColor;
            _grid.MarkDirtyRepaint();
        }

        // Node colors
        foreach (var node in _sceneNodes)
            ApplyNodeColors(node);

        // Edge colors
        foreach (var edge in edges.ToList())
            ApplyEdgeColor(edge);
    }

    private void ApplyNodeColors(Node node)
    {
        if (_settings == null) return;

        // Title bar
        var titleBar = node.Q("title");
        if (titleBar != null)
            titleBar.style.backgroundColor = _settings.NodeTitleColor;

        // FIX 3: Target 'node-border' instead of 'mainContainer'
        // The node-border is the actual master background of a GraphView Node.
        var nodeBorder = node.Q("node-border");
        if (nodeBorder != null)
        {
            nodeBorder.style.backgroundColor = _settings.NodeBodyColor;
        }
    
        // Clear the mainContainer color just in case it's layering weirdly
        node.mainContainer.style.backgroundColor = new StyleColor(StyleKeyword.Null);
    }

    private void ApplyEdgeColor(Edge edge)
    {
        if (_settings == null) return;
        edge.edgeControl.inputColor  = _settings.EdgeColor;
        edge.edgeControl.outputColor = _settings.EdgeColor;
    }

    // ── Viewport persistence ──────────────────────────────────────────────────

    public void SaveViewport(SceneGraphSettings s)
    {
        s.ViewPosition = viewTransform.position;
        s.ViewScale    = viewTransform.scale;
    }

    private void RestoreViewport()
    {
        if (_settings == null) return;
        UpdateViewTransform(_settings.ViewPosition, _settings.ViewScale);
    }

    // ── Edge serialization ────────────────────────────────────────────────────

    public void SerializeEdges()
    {
        _container.Edges.Clear();

        foreach (var edge in edges.ToList())
        {
            var outputNode = edge.output?.node as Node;
            var inputNode  = edge.input?.node  as Node;
            if (outputNode == null || inputNode == null) continue;

            _container.Edges.Add(new EdgeData
            {
                BaseNodeGUID   = outputNode.viewDataKey,
                TargetNodeGUID = inputNode.viewDataKey
            });
        }
    }

    // ── Graph change callback ─────────────────────────────────────────────────

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        bool hasChanged = false;

        if (change.movedElements != null)
        {
            foreach (var element in change.movedElements)
            {
                if (element is not Node node) continue;
                var nodeData = _container.Nodes.FirstOrDefault(n => n.GUID == node.viewDataKey);
                if (nodeData == null)
                {
                    nodeData = new NodeData { GUID = node.viewDataKey, SceneName = node.title };
                    _container.Nodes.Add(nodeData);
                }
                nodeData.Position = node.GetPosition().position;
                hasChanged = true;
            }
        }

        // Color newly created edges immediately
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
                ApplyEdgeColor(edge);
        }

        if (hasChanged
            || (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
            || (change.edgesToCreate    != null && change.edgesToCreate.Count    > 0))
        {
            EditorApplication.delayCall += () => {
                if (_container != null) _editor.RequestSave();
            };
        }

        return change;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Node GetNodeByGUID(string guid) =>
        nodes.ToList().OfType<Node>().FirstOrDefault(n => n.viewDataKey == guid);

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) =>
        ports.ToList().Where(p => p.direction != startPort.direction && p.node != startPort.node).ToList();
}

/// <summary>
/// A fully custom-painted grid that replaces GridBackground so we can
/// control both background and line colors at runtime without USS fights.
/// </summary>
public class ColorableGrid : VisualElement
{
    public Color BackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);
    public Color LineColor       = new Color(0.22f, 0.22f, 0.22f, 1f);

    private const float SmallStep = 20f;
    private const float LargeStep = 100f;

    public ColorableGrid()
    {
        generateVisualContent += OnGenerateVisualContent;
        // Prevent grid from blocking mouse events on the graph
        pickingMode = PickingMode.Ignore;
    }

    private void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        var r       = contentRect;

        // Background fill
        painter.fillColor = BackgroundColor;
        painter.BeginPath();
        painter.MoveTo(new Vector2(r.xMin, r.yMin));
        painter.LineTo(new Vector2(r.xMax, r.yMin));
        painter.LineTo(new Vector2(r.xMax, r.yMax));
        painter.LineTo(new Vector2(r.xMin, r.yMax));
        painter.ClosePath();
        painter.Fill();

        // Small grid lines (thin, more transparent)
        DrawLines(painter, r, SmallStep, new Color(LineColor.r, LineColor.g, LineColor.b, LineColor.a * 0.5f), 0.5f);

        // Large grid lines (slightly stronger)
        DrawLines(painter, r, LargeStep, LineColor, 1f);
    }

    private static void DrawLines(Painter2D painter, Rect r, float step, Color color, float width)
    {
        painter.strokeColor = color;
        painter.lineWidth   = width;

        // Vertical lines
        for (float x = r.xMin - (r.xMin % step); x <= r.xMax; x += step)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, r.yMin));
            painter.LineTo(new Vector2(x, r.yMax));
            painter.Stroke();
        }

        // Horizontal lines
        for (float y = r.yMin - (r.yMin % step); y <= r.yMax; y += step)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(r.xMin, y));
            painter.LineTo(new Vector2(r.xMax, y));
            painter.Stroke();
        }
    }
}