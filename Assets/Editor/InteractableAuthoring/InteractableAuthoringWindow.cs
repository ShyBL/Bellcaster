#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Since every in-game "scene" lives in one single Unity Scene, Pane A
/// doesn't come from SceneManager at all — it's the distinct set of topmost
/// parent GameObjects (transform.root) found by walking up from every
/// Interactable in the hierarchy. That sidesteps needing a marker component
/// or any assumption about which top-level objects "count" as a scene:
/// managers, cameras, etc. never appear here, because nothing walks up to
/// them from an Interactable.
///
/// Pane B lists the Interactables under the selected scene root. Pane C
/// edits the selected Interactable's InteractableData (a flat ScriptableObject
/// with no nested lists) via a single InspectorElement — deliberately no
/// Transform/SpriteRenderer/Collider fields here; GameObject setup stays a
/// manual Hierarchy/Inspector step.
///
/// EditorApplication.hierarchyChanged keeps Pane A/B in sync automatically
/// if you add/remove Interactables directly in the Hierarchy while this
/// window is open, without needing a manual refresh button.
/// </summary>
public class InteractableAuthoringWindow : EditorWindow
{
    // Adjust if the UXML/USS assets are moved.
    private const string UxmlPath = "Assets/Editor/InteractableAuthoring/InteractableAuthoringWindow.uxml";
    private const string UssPath  = "Assets/Editor/InteractableAuthoring/InteractableAuthoringWindow.uss";

    // Default save location for newly created data assets.
    //  private const string DefaultFolder = "Assets/_Bell/Data/Interactables";

    private ListView sceneListView;
    private ListView interactableListView;
    private Label propertiesHeaderLabel;
    private ScrollView inspectorScrollView;
    private ToolbarSearchField searchField;

    private Button createDataButton;
    private Button selectAssetButton;
    private Button duplicateButton;
    private Button deleteButton;
    private Button addInteractableButton;
    private Button removeInteractableButton;
    private ToolbarButton settingsButton;

    // Cached once per window session, same lazy-refetch pattern as
    // QuestAuthoringWindow.settings.
    private InteractableAuthoringSettings settings;
    
    // Pane A state
    private List<GameObject> allScenes      = new List<GameObject>();
    private List<GameObject> displayedScenes = new List<GameObject>();
    private string currentSearchText = string.Empty;
    private GameObject selectedScene;

    // Pane B state
    private List<Interactable> currentInteractables = new List<Interactable>();
    private Interactable selectedInteractable;

    [MenuItem("Tools/Interactable Authoring")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<InteractableAuthoringWindow>();
        wnd.titleContent = new GUIContent("Interactable Authoring");
        wnd.minSize = new Vector2(760, 460);
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);

        if (visualTree == null)
        {
            rootVisualElement.Add(new Label(
                $"[InteractableAuthoringWindow] Could not load UXML at '{UxmlPath}'. " +
                "Update the UxmlPath constant if the file was moved."));
            return;
        }

        VisualElement root = visualTree.Instantiate();
        root.style.flexGrow = 1;
        if (styleSheet != null) root.styleSheets.Add(styleSheet);
        rootVisualElement.Add(root);

        root.Q<ToolbarButton>("toolbar-btn-save").clicked += () => AssetDatabase.SaveAssets();
        searchField = root.Q<ToolbarSearchField>("toolbar-search");
        searchField.RegisterValueChangedCallback(evt => ApplySceneFilter(evt.newValue));

        sceneListView        = root.Q<ListView>("scene-list-view");
        interactableListView = root.Q<ListView>("interactable-list-view");

        propertiesHeaderLabel = root.Q<Label>("properties-header-label");
        inspectorScrollView   = root.Q<ScrollView>("inspector-scroll-view");

        createDataButton  = root.Q<Button>("btn-create-data");
        selectAssetButton = root.Q<Button>("btn-select-asset");
        duplicateButton   = root.Q<Button>("btn-duplicate");
        deleteButton      = root.Q<Button>("btn-delete");

        addInteractableButton    = root.Q<Button>("btn-add-interactable");
        removeInteractableButton = root.Q<Button>("btn-remove-interactable");
        settingsButton            = root.Q<ToolbarButton>("toolbar-btn-settings");
        
        addInteractableButton.clicked    += AddInteractable;
        removeInteractableButton.clicked += RemoveSelectedInteractable;
        if (settingsButton != null) settingsButton.clicked += OpenSettings;
        
        createDataButton.clicked  += CreateDataForSelected;
        selectAssetButton.clicked += SelectAssetInProject;
        duplicateButton.clicked   += DuplicateSelectedData;
        deleteButton.clicked      += DeleteSelectedData;

        SetupSceneListView();
        SetupInteractableListView();

        EditorApplication.hierarchyChanged += OnHierarchyChanged;

        RefreshScenes();
        ShowProperties();
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private void OnHierarchyChanged()
    {
        RefreshScenes();
    }

    // ── Pane A: scenes ───────────────────────────────────────────────────

    private void SetupSceneListView()
    {
        sceneListView.makeItem = () => new Label();
        sceneListView.bindItem = (element, i) =>
        {
            var scene = displayedScenes[i];
            ((Label)element).text = scene == null ? "(missing)" : scene.name;
        };
        sceneListView.itemsSource = displayedScenes;
        sceneListView.selectionChanged += OnSceneSelectionChanged;
    }

    /// <summary>
    /// Rebuilds the scene list from scratch and tries to preserve the
    /// current Pane A/B selection through the rebuild — this runs on every
    /// hierarchy change, so without this an unrelated edit elsewhere in the
    /// scene would otherwise silently reset whatever you were looking at.
    /// </summary>
    private void RefreshScenes()
    {
        var previousScene        = selectedScene;
        var previousInteractable = selectedInteractable;

        allScenes = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => t.CompareTag("GameScene"))
            .Select(t => t.gameObject)
            .OrderBy(go => go.name)
            .ToList();

        ApplySceneFilter(currentSearchText);

        if (previousScene != null && displayedScenes.Contains(previousScene))
        {
            SelectSceneInList(previousScene);

            // ADDED: Explicitly rebuild Pane B because the hierarchy change 
            // likely added or removed an interactable under this scene.
            RefreshInteractablesForSelectedScene();

            // Check if the previously selected interactable still exists
            if (previousInteractable != null && currentInteractables.Contains(previousInteractable))
            {
                SelectInteractableInList(previousInteractable);
            }
            else
            {
                // ADDED: If it doesn't exist (because you just deleted it), 
                // clear the selection in Pane B and empty Pane C.
                selectedInteractable = null;
                interactableListView.ClearSelection();
                ShowProperties();
            }

            return;
        }

        // The previously selected scene no longer exists (or nothing was selected)
        selectedScene = null;
        selectedInteractable = null;
        sceneListView.ClearSelection();
        RefreshInteractablesForSelectedScene();
        ShowProperties();
    }

    private void ApplySceneFilter(string searchText)
    {
        currentSearchText = searchText ?? string.Empty;

        displayedScenes = string.IsNullOrEmpty(currentSearchText)
            ? new List<GameObject>(allScenes)
            : allScenes
                .Where(go => go != null && go.name.IndexOf(currentSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

        sceneListView.itemsSource = displayedScenes;
        sceneListView.Rebuild();
    }

    private void OnSceneSelectionChanged(IEnumerable<object> selection)
    {
        selectedScene = selection.FirstOrDefault() as GameObject;

        // Always reset the dependent selection explicitly on every change,
        // whether or not a scene ended up selected.
        selectedInteractable = null;

        RefreshInteractablesForSelectedScene();
        interactableListView.ClearSelection();
        ShowProperties();
    }

    private void SelectSceneInList(GameObject scene)
    {
        int index = displayedScenes.IndexOf(scene);
        if (index >= 0)
            sceneListView.SetSelection(index);
    }

    // ── Pane B: interactables in the selected scene ──────────────────────

    private void SetupInteractableListView()
    {
        interactableListView.makeItem = () => new Label();
        interactableListView.bindItem = (element, i) =>
        {
            var interactable = currentInteractables[i];
            ((Label)element).text = interactable == null ? "(missing)" : DisplayNameFor(interactable);
        };
        interactableListView.itemsSource = currentInteractables;
        interactableListView.selectionChanged += OnInteractableSelectionChanged;
        
        interactableListView.reorderable = true;
        interactableListView.reorderMode = ListViewReorderMode.Animated;
        interactableListView.itemIndexChanged += OnInteractableReordered;
    }

    private void OnInteractableReordered(int oldIndex, int newIndex)
    {
        // ListView has already moved the item to newIndex in currentInteractables
        // by this point. Sibling index in the Hierarchy is the only persistent
        // "order" an Interactable GameObject has, so that's what gets written —
        // hierarchyChanged then fires and RefreshScenes() reads it back out,
        // same round-trip QuestAuthoringWindow does via SerializedObject.
        var moved = currentInteractables[newIndex];
        if (moved == null) return;

        Undo.SetTransformParent(moved.transform, moved.transform.parent, "Reorder Interactable");
        moved.transform.SetSiblingIndex(newIndex);
    }
    
    private void RefreshInteractablesForSelectedScene()
    {
        currentInteractables = selectedScene != null
            ? FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(i => i.transform.IsChildOf(selectedScene.transform))
                .OrderBy(i => i.transform.GetSiblingIndex())
                .ToList()
            : new List<Interactable>();

        interactableListView.itemsSource = currentInteractables;
        interactableListView.Rebuild();
    }

    private void OnInteractableSelectionChanged(IEnumerable<object> selection)
    {
        selectedInteractable = selection.FirstOrDefault() as Interactable;
        ShowProperties();
    }

    private void SelectInteractableInList(Interactable interactable)
    {
        int index = currentInteractables.IndexOf(interactable);
        if (index >= 0)
            interactableListView.SetSelection(index);
    }

    private static string DisplayNameFor(Interactable interactable)
    {
        if (interactable.data != null && !string.IsNullOrEmpty(interactable.data.objectName))
            return interactable.data.objectName;
        return interactable.gameObject.name;
    }

    private void AddInteractable()
    {
        if (selectedScene == null)
        {
            Debug.LogWarning("[InteractableAuthoringWindow] Select a scene before adding an interactable.");
            return;
        }

        var targetScene = selectedScene;

        EditorInputPrompt.Show("New Interactable", "Name", "", rawName =>
        {
            if (string.IsNullOrEmpty(rawName) || targetScene == null) return;

            var s = GetSettings();
            var container = GetOrCreateInteractablesContainer(targetScene);

            var goName = InteractableAuthoringSettings.ApplyTemplate(
                s.InteractableGameObjectNameTemplate, ("Name", rawName));

            GameObject go;

            // Check if the prefab is assigned in settings
            if (s.InteractablePrefab != null)
            {
                // Instantiate as a linked prefab instance
                go = (GameObject)PrefabUtility.InstantiatePrefab(s.InteractablePrefab);
                go.name = goName;
                Undo.RegisterCreatedObjectUndo(go, "Create Interactable");
                Undo.SetTransformParent(go.transform, container.transform, "Create Interactable");
            }
            else
            {
                // Fallback to empty GameObject if setting is missing
                go = new GameObject(goName);
                Undo.RegisterCreatedObjectUndo(go, "Create Interactable");
                Undo.SetTransformParent(go.transform, container.transform, "Create Interactable");
            }

            go.transform.SetAsLastSibling();

            // Check if the prefab already has the component, add it if it doesn't
            var interactable = go.GetComponent<Interactable>();
            if (interactable == null)
            {
                interactable = Undo.AddComponent<Interactable>(go);
            }

            CreateDataFor(interactable, targetScene);
            RefreshInteractablesForSelectedScene();

            int index = currentInteractables.IndexOf(interactable);
            if (index >= 0)
                interactableListView.SetSelection(index);
        });
    }
    
    // private void AddInteractable()
    // {
    //     if (selectedScene == null)
    //     {
    //         Debug.LogWarning("[InteractableAuthoringWindow] Select a scene before adding an interactable.");
    //         return;
    //     }
    //
    //     var targetScene = selectedScene;
    //
    //     EditorInputPrompt.Show("New Interactable", "Name", "", rawName =>
    //     {
    //         if (string.IsNullOrEmpty(rawName) || targetScene == null) return;
    //
    //         var s = GetSettings();
    //         var container = GetOrCreateInteractablesContainer(targetScene);
    //
    //         var goName = InteractableAuthoringSettings.ApplyTemplate(
    //             s.InteractableGameObjectNameTemplate, ("Name", rawName));
    //
    //         var go = new GameObject(goName);
    //         Undo.RegisterCreatedObjectUndo(go, "Create Interactable");
    //         Undo.SetTransformParent(go.transform, container.transform, "Create Interactable");
    //         go.transform.SetAsLastSibling();
    //
    //         var interactable = Undo.AddComponent<Interactable>(go);
    //
    //         // FIX: Automatically create and assign the data to the correct folder
    //         CreateDataFor(interactable, targetScene);
    //
    //         // FIX: Explicitly refresh Pane B so the new item appears instantly
    //         RefreshInteractablesForSelectedScene();
    //
    //         int index = currentInteractables.IndexOf(interactable);
    //         if (index >= 0)
    //             interactableListView.SetSelection(index);
    //     });
    // }

    /// <summary>
    /// Finds the existing "Interactables" child under the scene root, or
    /// creates it — this is what makes (a)'s sibling-index reordering
    /// meaningful and guarantees new Interactables always land in the same
    /// place, regardless of what else lives under the scene root.
    /// </summary>
    private GameObject GetOrCreateInteractablesContainer(GameObject sceneRoot)
    {
        var s = GetSettings();

        var existing = sceneRoot.transform
            .Cast<Transform>()
            .FirstOrDefault(t => t.name == s.InteractablesContainerName);

        if (existing != null) return existing.gameObject;

        var container = new GameObject(s.InteractablesContainerName);
        Undo.RegisterCreatedObjectUndo(container, "Create Interactables Container");
        Undo.SetTransformParent(container.transform, sceneRoot.transform, "Create Interactables Container");
        return container;
    }

    private void RemoveSelectedInteractable()
    {
        if (selectedInteractable == null)
        {
            Debug.LogWarning("[InteractableAuthoringWindow] No interactable selected to remove.");
            return;
        }

        string displayName = DisplayNameFor(selectedInteractable);
        //bool hasData = selectedInteractable.data != null;
        var dataToDelete = selectedInteractable.data;

        bool confirmed = EditorUtility.DisplayDialog(
            "Delete Interactable?",
            $"Delete '{displayName}' from the scene?" +
            // (hasData ? " Its InteractableData asset is NOT deleted — it's left unassigned on disk." : ""),
            (dataToDelete != null ? " This also deletes its InteractableData asset — cannot be undone." : ""),
            "Delete", "Cancel");

        if (!confirmed) return;

        var go = selectedInteractable.gameObject;
        selectedInteractable = null;
        interactableListView.ClearSelection();

        // Same cascade order as RemoveSelectedQuest: delete the owned asset
        // first, while we still have a live reference to it, before destroying
        // the GameObject that reference came from.
        if (dataToDelete != null)
        {
            var path = AssetDatabase.GetAssetPath(dataToDelete);
            if (!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(path);
        }
    
        Undo.DestroyObjectImmediate(go);
        ShowProperties(); // hierarchyChanged handles RefreshScenes() from here
    }

    // ── Pane C: properties ────────────────────────────────────────────────

    private void ShowProperties()
    {
        inspectorScrollView.Clear();

        if (selectedInteractable == null)
        {
            propertiesHeaderLabel.text = "Properties";
            SetActionButtonsVisible(false, false, false, false);
            return;
        }

        propertiesHeaderLabel.text = DisplayNameFor(selectedInteractable);

        bool hasData = selectedInteractable.data != null;
        SetActionButtonsVisible(
            createData: !hasData,
            selectAsset: hasData,
            duplicate: hasData,
            delete: hasData);

        if (!hasData)
        {
            inspectorScrollView.Add(new HelpBox(
                "This GameObject has no InteractableData assigned yet. Use \"Create Data\" to make one.",
                HelpBoxMessageType.Info));
            return;
        }

        // A single flat object with no nested arrays — one InspectorElement
        // on the whole SerializedObject shows every field automatically,
        // so this can't drift out of sync with whatever fields
        // InteractableData actually has.
        var serializedObject = new SerializedObject(selectedInteractable.data);
        var inspector = new InspectorElement(serializedObject);
        inspectorScrollView.Add(inspector);
    }

    private void SetActionButtonsVisible(bool createData, bool selectAsset, bool duplicate, bool delete)
    {
        createDataButton.style.display  = createData  ? DisplayStyle.Flex : DisplayStyle.None;
        selectAssetButton.style.display = selectAsset  ? DisplayStyle.Flex : DisplayStyle.None;
        duplicateButton.style.display   = duplicate    ? DisplayStyle.Flex : DisplayStyle.None;
        deleteButton.style.display      = delete       ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ── Create / Duplicate / Delete data ─────────────────────────────────

    private void CreateDataFor(Interactable interactable, GameObject sceneRoot)
    {
        var s = GetSettings();
        var data = ScriptableObject.CreateInstance<InteractableData>();
    
        // Set the data name to the exact GameObject name
        data.objectName = interactable.gameObject.name;

        // Apply template to get "{GameObjectName}_Data"
        var fileName = InteractableAuthoringSettings.ApplyTemplate(
            s.DataNameTemplate, ("Name", data.objectName));
    
        // Append the scene name to the base folder path
        string targetFolder = $"{s.DataFolder}/{sceneRoot.name}";
        EnsureFolder(targetFolder);

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{fileName}.asset");

        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();

        Undo.RecordObject(interactable, "Assign Interactable Data");
        interactable.data = data;
        EditorUtility.SetDirty(interactable);
    }
    
    private void CreateDataForSelected()
    {
        if (selectedInteractable == null || selectedScene == null)
        {
            Debug.LogWarning("[InteractableAuthoringWindow] Select an interactable and a scene first.");
            return;
        }

        CreateDataFor(selectedInteractable, selectedScene);

        interactableListView.Rebuild();
        ShowProperties();
    }

    private void SelectAssetInProject()
    {
        if (selectedInteractable?.data == null) return;
        Selection.activeObject = selectedInteractable.data;
        EditorGUIUtility.PingObject(selectedInteractable.data);
    }

    private void DuplicateSelectedData()
    {
        if (selectedInteractable?.data == null)
        {
            Debug.LogWarning("[InteractableAuthoringWindow] No data asset to duplicate.");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(selectedInteractable.data);
        string newPath = AssetDatabase.GenerateUniqueAssetPath(sourcePath);

        if (!AssetDatabase.CopyAsset(sourcePath, newPath))
        {
            Debug.LogWarning($"[InteractableAuthoringWindow] Failed to duplicate asset at '{sourcePath}'.");
            return;
        }

        AssetDatabase.SaveAssets();
        var clone = AssetDatabase.LoadAssetAtPath<InteractableData>(newPath);

        // The clone isn't assigned to any GameObject, so it won't show up in
        // Pane B until you assign it manually — ping it so it's easy to find.
        Selection.activeObject = clone;
        EditorGUIUtility.PingObject(clone);
    }

    /// <summary>
    /// Clears the GameObject's data reference BEFORE deleting the asset, so
    /// the Interactable component never ends up with a missing reference.
    /// </summary>
    private void DeleteSelectedData()
    {
        if (selectedInteractable?.data == null)
        {
            Debug.LogWarning("[InteractableAuthoringWindow] No data asset to delete.");
            return;
        }

        string displayName = DisplayNameFor(selectedInteractable);
        var dataToDelete = selectedInteractable.data;

        bool confirmed = EditorUtility.DisplayDialog(
            "Delete Interactable Data?",
            $"Delete the InteractableData asset for '{displayName}'? This cannot be undone. " +
            "The GameObject itself stays in the scene, but its data reference will be cleared.",
            "Delete", "Cancel");

        if (!confirmed) return;

        Undo.RecordObject(selectedInteractable, "Clear Interactable Data");
        selectedInteractable.data = null;
        EditorUtility.SetDirty(selectedInteractable);

        string path = AssetDatabase.GetAssetPath(dataToDelete);
        AssetDatabase.DeleteAsset(path);

        interactableListView.Rebuild();
        ShowProperties();
    }
    
    // ── Settings ─────────────────────────────────────────────────────────

    private InteractableAuthoringSettings GetSettings()
    {
        if (settings != null) return settings;

        var guids = AssetDatabase.FindAssets("t:InteractableAuthoringSettings");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            settings = AssetDatabase.LoadAssetAtPath<InteractableAuthoringSettings>(path);
            if (settings != null) return settings;
        }

        EnsureFolder("Assets/Editor");
        var instance = ScriptableObject.CreateInstance<InteractableAuthoringSettings>();
        AssetDatabase.CreateAsset(instance, "Assets/Editor/InteractableAuthoringSettings.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        settings = instance;
        return settings;
    }

    private void OpenSettings()
    {
        var s = GetSettings();
        Selection.activeObject = s;
        EditorGUIUtility.PingObject(s);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        var name   = Path.GetFileName(folder);

        if (string.IsNullOrEmpty(parent)) return;
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif