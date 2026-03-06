using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class SceneGraphEditor : EditorWindow
{
    private SceneGraphData     _dataContainer;
    private SceneGraphSettings _settings;
    private SceneGraphView     _graphView;
    private bool               _autoSave;
    private bool               _settingsPanelOpen;

    // Refs held so we can update colors live from the settings panel
    private VisualElement _settingsPanel;

    private const string AutoSavePrefKey = "SceneGraphEditor_AutoSave";

    [MenuItem("Tools/Scene Relationship Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<SceneGraphEditor>();
        window.titleContent = new GUIContent("Scene Graph");
    }

    private void OnEnable()
    {
        _autoSave = EditorPrefs.GetBool(AutoSavePrefKey, true);
        LoadData();
        ConstructGraph();
    }

    private void OnDisable()
    {
        // Persist viewport transform when window closes
        if (_graphView != null && _settings != null)
        {
            _graphView.SaveViewport(_settings);
            SaveSettings();
        }
    }

    // ── Data loading ─────────────────────────────────────────────────────────

    private void LoadData()
    {
        // Graph node/edge data
        string[] dataGuids = AssetDatabase.FindAssets("t:SceneGraphData");
        if (dataGuids.Length > 0)
        {
            _dataContainer = AssetDatabase.LoadAssetAtPath<SceneGraphData>(
                AssetDatabase.GUIDToAssetPath(dataGuids[0]));
        }
        else
        {
            _dataContainer = ScriptableObject.CreateInstance<SceneGraphData>();
            AssetDatabase.CreateAsset(_dataContainer, "Assets/SceneGraphSaveData.asset");
            AssetDatabase.SaveAssets();
        }

        // Visual settings
        _settings = SceneGraphSettings.GetOrCreate();
    }

    // ── UI construction ───────────────────────────────────────────────────────

    private void ConstructGraph()
    {
        rootVisualElement.Clear();
        rootVisualElement.style.flexDirection = FlexDirection.Column;
        rootVisualElement.style.flexGrow      = 1;

        rootVisualElement.Add(BuildToolbar());

        // ── Graph view ───────────────────────────────────────────────────────
        _graphView = new SceneGraphView(_dataContainer, _settings, this);
        _graphView.style.flexGrow = 1;
        rootVisualElement.Add(_graphView);

        // ── Settings overlay (hidden by default) ─────────────────────────────
        _settingsPanel = BuildSettingsPanel();
        rootVisualElement.Add(_settingsPanel);

        ApplyWindowColor();
    }

    // ── Toolbar ───────────────────────────────────────────────────────────────

    private Toolbar BuildToolbar()
    {
        var toolbar = new Toolbar();

        var spacer = new VisualElement();
        spacer.style.flexGrow = 1;
        toolbar.Add(spacer);

        // Settings toggle button
        var settingsBtn = new ToolbarButton(() => ToggleSettingsPanel()) { text = "⚙ Settings" };
        toolbar.Add(settingsBtn);

        // Auto-save checkbox
        var autoSaveToggle = new ToolbarToggle { text = "Auto Save", value = _autoSave };
        autoSaveToggle.RegisterValueChangedCallback(evt => {
            _autoSave = evt.newValue;
            EditorPrefs.SetBool(AutoSavePrefKey, _autoSave);
        });
        toolbar.Add(autoSaveToggle);

        toolbar.Add(new ToolbarButton(() => SaveData())       { text = "Save"    });
        var refreshBtn = new ToolbarButton(() => {
            // First, regenerate all images
            SceneThumbnailRecorder.RefreshAllThumbnails();
            // Then, rebuild the graph UI to show the new images
            ConstructGraph(); 
        }) { text = "Refresh & Recapture" };

        toolbar.Add(refreshBtn);
        // toolbar.Add(new ToolbarButton(() => ConstructGraph()) { text = "Refresh" });

        return toolbar;
    }

    // ── Settings panel ────────────────────────────────────────────────────────

    // Draft colors — staged here until Apply is pressed
    private Color   _draftWindow;
    private Color   _draftGridBg, _draftGridLine;
    private Vector2 _draftNodeSize;
    private Color   _draftNodeTitle, _draftNodeBody;
    private Color   _draftEdge;

    // ColorField refs so Reset can update their displayed values
    private ColorField   _fieldWindow;
    private ColorField   _fieldGridBg, _fieldGridLine;
    private Vector2Field _fieldNodeSize;
    private ColorField   _fieldNodeTitle, _fieldNodeBody;
    private ColorField   _fieldEdge;

    private VisualElement BuildSettingsPanel()
    {
        // Seed draft from current saved settings
        _draftWindow    = _settings.WindowColor;
        _draftGridBg    = _settings.GridBackgroundColor;
        _draftGridLine  = _settings.GridLineColor;
        _draftNodeSize  = _settings.NodeSize;
        _draftNodeTitle = _settings.NodeTitleColor;
        _draftNodeBody  = _settings.NodeBodyColor;
        _draftEdge      = _settings.EdgeColor;

        var panel = new VisualElement();
        panel.style.position        = Position.Absolute;
        panel.style.top             = 22;
        panel.style.right           = 0;
        panel.style.width           = 260;
        panel.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.97f);
        panel.style.borderTopLeftRadius    = 6;
        panel.style.borderBottomLeftRadius = 6;
        panel.style.paddingTop    = 12;
        panel.style.paddingBottom = 12;
        panel.style.paddingLeft   = 14;
        panel.style.paddingRight  = 14;
        panel.style.display = DisplayStyle.None;

        // ── Header ────────────────────────────────────────────────────────
        var header = new Label("Graph Settings");
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.fontSize    = 13;
        header.style.marginBottom = 10;
        header.style.color       = Color.white;
        panel.Add(header);

        // ── Color fields — write into drafts only ─────────────────────────
        panel.Add(MakeSectionLabel("Window"));
        panel.Add(MakeColorRow("Background", _draftWindow, c => _draftWindow = c, out _fieldWindow));

        panel.Add(MakeSectionLabel("Grid"));
        panel.Add(MakeColorRow("Background", _draftGridBg,   c => _draftGridBg   = c, out _fieldGridBg));
        panel.Add(MakeColorRow("Lines",      _draftGridLine, c => _draftGridLine  = c, out _fieldGridLine));

        panel.Add(MakeSectionLabel("Nodes"));
        panel.Add(MakeVector2Row("Size", _draftNodeSize, v => _draftNodeSize = v, out _fieldNodeSize));
        panel.Add(MakeColorRow("Title bar",  _draftNodeTitle, c => _draftNodeTitle = c, out _fieldNodeTitle));
        panel.Add(MakeColorRow("Body",       _draftNodeBody,  c => _draftNodeBody  = c, out _fieldNodeBody));

        panel.Add(MakeSectionLabel("Edges"));
        panel.Add(MakeColorRow("Color",      _draftEdge,      c => _draftEdge      = c, out _fieldEdge));

        // ── Action buttons row ────────────────────────────────────────────
        var btnRow = new VisualElement();
        btnRow.style.flexDirection  = FlexDirection.Row;
        btnRow.style.marginTop      = 14;
        panel.Add(btnRow);

        // Apply
        var applyBtn = new Button(() => {
            bool sizeChanged              = _draftNodeSize != _settings.NodeSize;
            _settings.WindowColor         = _draftWindow;
            _settings.GridBackgroundColor = _draftGridBg;
            _settings.GridLineColor       = _draftGridLine;
            _settings.NodeSize            = _draftNodeSize;
            _settings.NodeTitleColor      = _draftNodeTitle;
            _settings.NodeBodyColor       = _draftNodeBody;
            _settings.EdgeColor           = _draftEdge;
            SaveSettings();
            if (sizeChanged)
                ConstructGraph(); // Nodes must be recreated to pick up new size
            else
            {
                _graphView.ApplySettings(_settings);
                ApplyWindowColor();
            }
        }) { text = "Apply" };
        applyBtn.style.flexGrow    = 1;
        applyBtn.style.marginRight = 4;
        btnRow.Add(applyBtn);

        // Reset to Defaults — updates pickers directly, no panel rebuild needed
        var resetBtn = new Button(() => {
            var defaults = ScriptableObject.CreateInstance<SceneGraphSettings>();

            // Write into settings
            _settings.WindowColor         = defaults.WindowColor;
            _settings.GridBackgroundColor = defaults.GridBackgroundColor;
            _settings.GridLineColor       = defaults.GridLineColor;
            _settings.NodeSize            = defaults.NodeSize;
            _settings.NodeTitleColor      = defaults.NodeTitleColor;
            _settings.NodeBodyColor       = defaults.NodeBodyColor;
            _settings.EdgeColor           = defaults.EdgeColor;
            Object.DestroyImmediate(defaults);

            // Sync drafts
            _draftWindow    = _settings.WindowColor;
            _draftGridBg    = _settings.GridBackgroundColor;
            _draftGridLine  = _settings.GridLineColor;
            _draftNodeSize  = _settings.NodeSize;
            _draftNodeTitle = _settings.NodeTitleColor;
            _draftNodeBody  = _settings.NodeBodyColor;
            _draftEdge      = _settings.EdgeColor;

            // Update widgets in-place
            _fieldWindow.value    = _draftWindow;
            _fieldGridBg.value    = _draftGridBg;
            _fieldGridLine.value  = _draftGridLine;
            _fieldNodeSize.value  = _draftNodeSize;
            _fieldNodeTitle.value = _draftNodeTitle;
            _fieldNodeBody.value  = _draftNodeBody;
            _fieldEdge.value      = _draftEdge;

            _graphView.ApplySettings(_settings);
            ApplyWindowColor();
            SaveSettings();
        }) { text = "Reset" };
        resetBtn.style.flexGrow = 1;
        btnRow.Add(resetBtn);

        return panel;
    }

    private void ToggleSettingsPanel()
    {
        _settingsPanelOpen = !_settingsPanelOpen;
        // Re-seed drafts and pickers from current settings each time panel opens
        if (_settingsPanelOpen)
        {
            _draftWindow    = _settings.WindowColor;
            _draftGridBg    = _settings.GridBackgroundColor;
            _draftGridLine  = _settings.GridLineColor;
            _draftNodeSize  = _settings.NodeSize;
            _draftNodeTitle = _settings.NodeTitleColor;
            _draftNodeBody  = _settings.NodeBodyColor;
            _draftEdge      = _settings.EdgeColor;

            if (_fieldWindow != null)    _fieldWindow.value    = _draftWindow;
            if (_fieldGridBg != null)    _fieldGridBg.value    = _draftGridBg;
            if (_fieldGridLine != null)  _fieldGridLine.value  = _draftGridLine;
            if (_fieldNodeSize != null)  _fieldNodeSize.value  = _draftNodeSize;
            if (_fieldNodeTitle != null) _fieldNodeTitle.value = _draftNodeTitle;
            if (_fieldNodeBody != null)  _fieldNodeBody.value  = _draftNodeBody;
            if (_fieldEdge != null)      _fieldEdge.value      = _draftEdge;
        }
        _settingsPanel.style.display = _settingsPanelOpen ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    private static Label MakeSectionLabel(string text)
    {
        var label = new Label(text);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.color        = new Color(0.7f, 0.7f, 0.7f);
        label.style.fontSize     = 11;
        label.style.marginTop    = 8;
        label.style.marginBottom = 4;
        return label;
    }

    private static VisualElement MakeColorRow(string labelText, Color initialColor,
                                               System.Action<Color> onChanged, out ColorField field)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems    = Align.Center;
        row.style.marginBottom  = 4;

        var label = new Label(labelText);
        label.style.flexGrow = 1;
        label.style.color    = Color.white;
        label.style.fontSize = 11;
        row.Add(label);

        field = new ColorField { value = initialColor, showAlpha = true };
        field.style.width = 120;
        field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
        row.Add(field);

        return row;
    }

    private static VisualElement MakeVector2Row(string labelText, Vector2 initialValue,
                                                 System.Action<Vector2> onChanged, out Vector2Field field)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems    = Align.Center;
        row.style.marginBottom  = 4;

        var label = new Label(labelText);
        label.style.flexGrow = 1;
        label.style.color    = Color.white;
        label.style.fontSize = 11;
        row.Add(label);

        field = new Vector2Field { value = initialValue };
        field.style.width = 120;
        field.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
        row.Add(field);

        return row;
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    public void RequestSave()
    {
        if (_autoSave) SaveData();
    }

    public void SaveData()
    {
        _graphView.SerializeEdges();
        _graphView.SaveViewport(_settings);

        EditorUtility.SetDirty(_dataContainer);
        SaveSettings();

        AssetDatabase.SaveAssets();
        Debug.Log("Scene Graph Saved.");
    }

    private void SaveSettings()
    {
        EditorUtility.SetDirty(_settings);
        AssetDatabase.SaveAssets();
    }

    private void ApplyWindowColor()
    {
        // rootVisualElement sits above all USS sheets so setting it here wins
        rootVisualElement.style.backgroundColor = _settings.WindowColor;
    }
}