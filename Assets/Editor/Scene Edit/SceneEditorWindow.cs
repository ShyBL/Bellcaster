// ==================== SceneEditorWindow.cs ====================
// Place in Assets/Editor/SceneEditorWindow.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SceneEditorWindow : EditorWindow
{
    // Window state
    private Vector2 scrollPosition;
    private List<GameObject> sceneInteractables = new List<GameObject>();
    private GameObject selectedObject;
    private InteractableData selectedData;
    
    // Scene management
    private string currentSceneName = "";
    private List<string> availableScenes = new List<string>();
    private int selectedSceneIndex = 0;
    
    // Creation settings
    private InteractableTemplateType selectedTemplate = InteractableTemplateType.SimpleExamine;
    private string newObjectName = "New Interactable";
    private Sprite objectSprite;
    private Vector3 spawnPosition = Vector3.zero;
    
    // CSV Import
    private TextAsset csvFile;
    
    // Folders
    private const string DATA_ROOT_FOLDER = "Assets/Data/Interactables";
    
    [MenuItem("Tools/Interactable Workshop")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneEditorWindow>("Interactable Workshop");
        window.minSize = new Vector2(400, 600);
    }
    
    void OnEnable()
    {
        RefreshCurrentScene();
        RefreshSceneInteractables();
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    
    void OnDisable()
    {
        EditorSceneManager.sceneOpened -= OnSceneOpened;
    }
    
    void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        RefreshCurrentScene();
        RefreshSceneInteractables();
    }
    
    void RefreshCurrentScene()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        currentSceneName = string.IsNullOrEmpty(activeScene.name) ? "Untitled" : activeScene.name;
        
        // Get all scenes from build settings
        availableScenes = EditorBuildSettings.scenes
            .Select(s => Path.GetFileNameWithoutExtension(s.path))
            .ToList();
        
        selectedSceneIndex = availableScenes.IndexOf(currentSceneName);
        if (selectedSceneIndex < 0) selectedSceneIndex = 0;
    }
    
    string GetSceneDataFolder()
    {
        return $"{DATA_ROOT_FOLDER}/{currentSceneName}";
    }
    
    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawHeader();
        EditorGUILayout.Space(10);
        
        DrawSceneSelector();
        EditorGUILayout.Space(10);
        
        DrawSceneInteractablesList();
        EditorGUILayout.Space(10);
        
        DrawCreationPanel();
        EditorGUILayout.Space(10);
        
        if (selectedObject != null)
        {
            DrawEditPanel();
        }
        
        EditorGUILayout.Space(10);
        DrawQuickActionsPanel();
        
        EditorGUILayout.EndScrollView();
    }
    
    // ==================== Header ====================
    void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.LabelField("INTERACTABLE WORKSHOP", titleStyle);
        EditorGUILayout.LabelField("Scene-Based Level Design Tool", EditorStyles.centeredGreyMiniLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== Scene Selector ====================
    void DrawSceneSelector()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("CURRENT SCENE", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Display current scene name prominently
        GUIStyle sceneStyle = new GUIStyle(EditorStyles.textField);
        sceneStyle.fontSize = 14;
        sceneStyle.fontStyle = FontStyle.Bold;
        sceneStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.LabelField(currentSceneName, sceneStyle, GUILayout.Height(25));
        
        if (GUILayout.Button("↻", GUILayout.Width(30), GUILayout.Height(25)))
        {
            RefreshCurrentScene();
            RefreshSceneInteractables();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Scene switching dropdown (if scenes are in build settings)
        if (availableScenes.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Switch to:", GUILayout.Width(70));
            
            int newIndex = EditorGUILayout.Popup(selectedSceneIndex, availableScenes.ToArray());
            
            if (newIndex != selectedSceneIndex && newIndex >= 0)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    string scenePath = EditorBuildSettings.scenes[newIndex].path;
                    EditorSceneManager.OpenScene(scenePath);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // Data folder info
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Data Folder:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField(GetSceneDataFolder(), EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== Scene Interactables List ====================
    void DrawSceneInteractablesList()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"INTERACTABLES IN {currentSceneName.ToUpper()}", EditorStyles.boldLabel);
        if (GUILayout.Button("Refresh", GUILayout.Width(70)))
        {
            RefreshSceneInteractables();
        }
        EditorGUILayout.EndHorizontal();
        
        if (sceneInteractables.Count == 0)
        {
            EditorGUILayout.HelpBox("No interactables found in this scene.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.LabelField($"Total: {sceneInteractables.Count}", EditorStyles.miniLabel);
            
            foreach (var obj in sceneInteractables)
            {
                if (obj == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                
                bool isSelected = selectedObject == obj;
                GUIStyle style = isSelected ? new GUIStyle(EditorStyles.helpBox) : EditorStyles.label;
                
                if (GUILayout.Button(obj.name, style, GUILayout.Height(25)))
                {
                    SelectObject(obj);
                }
                
                if (GUILayout.Button("Focus", GUILayout.Width(50)))
                {
                    Selection.activeGameObject = obj;
                    SceneView.FrameLastActiveSceneView();
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== Creation Panel ====================
    void DrawCreationPanel()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("CREATE NEW INTERACTABLE", EditorStyles.boldLabel);
        
        // Template selection
        selectedTemplate = (InteractableTemplateType)EditorGUILayout.EnumPopup("Template", selectedTemplate);
        EditorGUILayout.HelpBox(InteractableTemplates.GetTemplateDescription(selectedTemplate), MessageType.Info);
        
        // Basic info
        newObjectName = EditorGUILayout.TextField("Object Name", newObjectName);
        objectSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", objectSprite, typeof(Sprite), false);
        spawnPosition = EditorGUILayout.Vector3Field("Spawn Position", spawnPosition);
        
        EditorGUILayout.Space(5);
        
        // Create button
        GUI.enabled = !string.IsNullOrEmpty(newObjectName);
        if (GUILayout.Button("CREATE AND SPAWN", GUILayout.Height(35)))
        {
            CreateAndSpawnInteractable();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== Edit Panel ====================
    void DrawEditPanel()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"EDITING: {selectedObject.name}", EditorStyles.boldLabel);
        
        if (selectedData == null)
        {
            EditorGUILayout.HelpBox("No InteractableData assigned to this object.", MessageType.Warning);
            
            if (GUILayout.Button("Create New Data"))
            {
                CreateDataForExistingObject(selectedObject);
            }
        }
        else
        {
            // Edit data inline
            Editor dataEditor = Editor.CreateEditor(selectedData);
            dataEditor.OnInspectorGUI();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select in Project"))
            {
                Selection.activeObject = selectedData;
                EditorGUIUtility.PingObject(selectedData);
            }
            if (GUILayout.Button("Clone Data"))
            {
                CloneDataForObject(selectedObject);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(5);
        
        // Position controls
        spawnPosition = selectedObject.transform.position;
        spawnPosition = EditorGUILayout.Vector3Field("Position", spawnPosition);
        if (GUI.changed)
        {
            selectedObject.transform.position = spawnPosition;
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Focus in Scene"))
        {
            Selection.activeGameObject = selectedObject;
            SceneView.FrameLastActiveSceneView();
        }
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete"))
        {
            if (EditorUtility.DisplayDialog("Delete Interactable", 
                $"Are you sure you want to delete {selectedObject.name}?", 
                "Yes", "No"))
            {
                DeleteInteractable(selectedObject);
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== Quick Actions ====================
    void DrawQuickActionsPanel()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("QUICK ACTIONS", EditorStyles.boldLabel);
        
        // CSV Import Section
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("CSV Import", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Import interactables from a CSV file exported from Google Sheets.", MessageType.Info);
        
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        
        GUI.enabled = csvFile != null;
        if (GUILayout.Button("Import from CSV", GUILayout.Height(30)))
        {
            ImportFromCSV(csvFile.text);
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // Scene-specific presets
        if (currentSceneName == "Workshop")
        {
            if (GUILayout.Button("Spawn All Workshop Interactables"))
            {
                SpawnWorkshopPresets();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"No presets available for '{currentSceneName}' scene.", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button($"Clear All Interactables in {currentSceneName}"))
        {
            if (EditorUtility.DisplayDialog("Clear All", 
                $"Are you sure you want to delete ALL interactables in {currentSceneName}?", 
                "Yes", "No"))
            {
                ClearAllInteractables();
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    // ==================== CSV Import ====================
    
    void ImportFromCSV(string csvContent)
    {
        List<InteractableCSVRow> rows = InteractableCSVParser.ParseCSV(csvContent);
        
        if (rows.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "CSV Import Failed", 
                "No valid rows found in CSV file.\n\nMake sure:\n- CSV has 'object_name' header\n- At least one data row exists\n- Required columns are filled", 
                "OK"
            );
            return;
        }
        
        int successCount = 0;
        int failCount = 0;
        List<string> errors = new List<string>();
        
        foreach (var row in rows)
        {
            try
            {
                CreateInteractableFromCSV(row);
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create {row.object_name}: {e.Message}");
                errors.Add($"{row.object_name}: {e.Message}");
                failCount++;
            }
        }
        
        RefreshSceneInteractables();
        
        string message = $"Successfully imported {successCount} interactables.";
        if (failCount > 0)
        {
            message += $"\n\nFailed: {failCount}";
            if (errors.Count > 0)
            {
                message += "\n\nErrors:\n" + string.Join("\n", errors.Take(5));
                if (errors.Count > 5)
                {
                    message += $"\n... and {errors.Count - 5} more (check Console)";
                }
            }
        }
        
        EditorUtility.DisplayDialog("CSV Import Complete", message, "OK");
    }
    
    void CreateInteractableFromCSV(InteractableCSVRow row)
    {
        // Parse template enum
        if (!System.Enum.TryParse<InteractableTemplateType>(row.template, true, out var templateType))
        {
            throw new System.Exception($"Invalid template type: '{row.template}'");
        }
        
        // Create GameObject
        GameObject newObj = new GameObject(row.object_name);
        newObj.transform.position = new Vector3(row.position_x, row.position_y, row.position_z);
        
        // Add SpriteRenderer
        SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
        
        // Set sorting layer if specified
        if (!string.IsNullOrWhiteSpace(row.sorting_layer))
        {
            sr.sortingLayerName = row.sorting_layer.Trim();
        }
        
        // Set order in layer if specified
        int? orderInLayer = row.GetInt(row.order_in_layer);
        if (orderInLayer.HasValue)
        {
            sr.sortingOrder = orderInLayer.Value;
        }
        
        // Add Collider
        BoxCollider2D collider = newObj.AddComponent<BoxCollider2D>();
        
        float? colliderWidth = row.GetFloat(row.collider_width);
        float? colliderHeight = row.GetFloat(row.collider_height);
        
        if (colliderWidth.HasValue && colliderHeight.HasValue)
        {
            collider.size = new Vector2(colliderWidth.Value, colliderHeight.Value);
        }
        // If no manual size, sprite will auto-size when assigned
        
        // Add Interactable
        Interactable interactable = newObj.AddComponent<Interactable>();
        
        // Create InteractableData from template
        InteractableData data = InteractableTemplates.ApplyTemplate(templateType, row.object_name);
        
        // Apply template overrides if specified
        bool? canExamine = row.GetBool(row.can_examine);
        if (canExamine.HasValue) data.canExamine = canExamine.Value;
        
        bool? canPickup = row.GetBool(row.can_pickup);
        if (canPickup.HasValue) data.canPickUp = canPickup.Value;
        
        bool? canInteract = row.GetBool(row.can_interact);
        if (canInteract.HasValue) data.canInteract = canInteract.Value;
        
        // Override with CSV data
        if (!string.IsNullOrWhiteSpace(row.examine_text))
        {
            data.examineText = row.examine_text.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(row.pickup_destination))
        {
            if (System.Enum.TryParse<PickupDestination>(row.pickup_destination.Trim(), true, out var dest))
            {
                data.pickupDestination = dest;
            }
        }
        
        if (!string.IsNullOrWhiteSpace(row.pickup_requirement))
        {
            data.pickupRequirement = row.pickup_requirement.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(row.required_inventory_item))
        {
            data.requiredInventoryItem = row.required_inventory_item.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(row.interact_result_state))
        {
            data.interactResultState = row.interact_result_state.Trim();
        }
        
        // Find and reference GameObject if specified
        if (!string.IsNullOrWhiteSpace(row.interact_result_object))
        {
            GameObject resultObj = GameObject.Find(row.interact_result_object.Trim());
            if (resultObj != null)
            {
                data.interactResultObject = resultObj;
            }
            else
            {
                Debug.LogWarning($"GameObject '{row.interact_result_object}' not found in scene for {row.object_name}");
            }
        }
        
        // Set active state
        bool? active = row.GetBool(row.active);
        if (active.HasValue)
        {
            newObj.SetActive(active.Value);
        }
        
        // Save InteractableData asset
        string folderPath = GetSceneDataFolder();
        EnsureFolderExists(folderPath);
        
        string assetPath = $"{folderPath}/{row.object_name.Replace(" ", "").Replace("/", "")}_Data.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        
        interactable.data = data;
        
        Debug.Log($"Created: {row.object_name} ({templateType})");
    }
    
    // ==================== Core Functions ====================
    
    void RefreshSceneInteractables()
    {
        sceneInteractables = FindObjectsOfType<Interactable>()
            .Select(i => i.gameObject)
            .ToList();
    }
    
    void SelectObject(GameObject obj)
    {
        selectedObject = obj;
        var interactable = obj.GetComponent<Interactable>();
        selectedData = interactable != null ? interactable.data : null;
        spawnPosition = obj.transform.position;
    }
    
    void CreateAndSpawnInteractable()
    {
        // Create GameObject
        GameObject newObj = new GameObject(newObjectName);
        newObj.transform.position = spawnPosition;
        
        // Add SpriteRenderer
        SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
        if (objectSprite != null)
        {
            sr.sprite = objectSprite;
        }
        
        // Add Collider (auto-sized to sprite)
        BoxCollider2D collider = newObj.AddComponent<BoxCollider2D>();
        if (objectSprite != null)
        {
            collider.size = objectSprite.bounds.size;
        }
        
        // Add Interactable script
        Interactable interactable = newObj.AddComponent<Interactable>();
        
        // Create and assign data
        InteractableData data = InteractableTemplates.ApplyTemplate(selectedTemplate, newObjectName);
        
        // Save data asset in scene-specific folder
        string folderPath = GetSceneDataFolder();
        EnsureFolderExists(folderPath);
        
        string assetPath = $"{folderPath}/{newObjectName.Replace(" ", "")}_Data.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        
        interactable.data = data;
        
        // Select and focus
        Selection.activeGameObject = newObj;
        SceneView.FrameLastActiveSceneView();
        
        RefreshSceneInteractables();
        SelectObject(newObj);
        
        Debug.Log($"Created interactable in {currentSceneName}: {newObjectName} with template: {selectedTemplate}");
    }
    
    void CreateDataForExistingObject(GameObject obj)
    {
        Interactable interactable = obj.GetComponent<Interactable>();
        if (interactable == null) return;
        
        InteractableData data = InteractableTemplates.ApplyTemplate(InteractableTemplateType.Custom, obj.name);
        
        string folderPath = GetSceneDataFolder();
        EnsureFolderExists(folderPath);
        
        string assetPath = $"{folderPath}/{obj.name.Replace(" ", "")}_Data.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(data, assetPath);
        AssetDatabase.SaveAssets();
        
        interactable.data = data;
        selectedData = data;
    }
    
    void CloneDataForObject(GameObject obj)
    {
        Interactable interactable = obj.GetComponent<Interactable>();
        if (interactable == null || interactable.data == null) return;
        
        InteractableData clone = Instantiate(interactable.data);
        
        string folderPath = GetSceneDataFolder();
        string assetPath = $"{folderPath}/{obj.name.Replace(" ", "")}_Data_Clone.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(clone, assetPath);
        AssetDatabase.SaveAssets();
        
        interactable.data = clone;
        selectedData = clone;
    }
    
    void DeleteInteractable(GameObject obj)
    {
        sceneInteractables.Remove(obj);
        if (selectedObject == obj)
        {
            selectedObject = null;
            selectedData = null;
        }
        DestroyImmediate(obj);
    }
    
    void SpawnWorkshopPresets()
    {
        var presets = WorkshopPresets.GetAllPresets();
        
        foreach (var preset in presets)
        {
            // Create GameObject
            GameObject newObj = new GameObject(preset.name);
            newObj.transform.position = preset.position;
            
            // Add components
            SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
            BoxCollider2D collider = newObj.AddComponent<BoxCollider2D>();
            Interactable interactable = newObj.AddComponent<Interactable>();
            
            // Create custom data from preset
            InteractableData customData = preset.dataCreator();
            
            string folderPath = GetSceneDataFolder();
            EnsureFolderExists(folderPath);
            
            string assetPath = $"{folderPath}/{preset.name.Replace(" ", "")}_Data.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(customData, assetPath);
            AssetDatabase.SaveAssets();
            
            interactable.data = customData;
        }
        
        RefreshSceneInteractables();
        Debug.Log($"Spawned {presets.Count} Workshop interactables in {currentSceneName}!");
    }
    
    void ClearAllInteractables()
    {
        foreach (var obj in sceneInteractables.ToList())
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        
        selectedObject = null;
        selectedData = null;
        RefreshSceneInteractables();
    }
    
    void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = $"{currentPath}/{folders[i]}";
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }
}
#endif