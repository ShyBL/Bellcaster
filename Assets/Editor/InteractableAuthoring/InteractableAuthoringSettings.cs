#if UNITY_EDITOR
using UnityEngine;

/// <summary>
/// Project-wide settings for InteractableAuthoringWindow — same pattern as
/// QuestAuthoringSettings: one asset per project, found via AssetDatabase,
/// auto-created with defaults the first time the window opens and finds none.
///
/// Owns what InteractableAuthoringWindow currently hardcodes: the data-asset
/// folder/naming, and the name of the container GameObject new Interactables
/// get parented under (see AddInteractable / GetOrCreateInteractablesContainer).
/// </summary>
[CreateAssetMenu(fileName = "InteractableAuthoringSettings", menuName = "Tools/Interactable Authoring Settings")]
public class InteractableAuthoringSettings : ScriptableObject
{
    [Header("Data Assets")]
    [Tooltip("Folder new InteractableData assets are created in. Auto-created if it doesn't exist yet.")]
    public string DataFolder = "Assets/_Bell/Data/Interactables";

    [Tooltip("Placeholders: {Name}")]
    public string DataNameTemplate = "{Name}_Data";

    [Header("Scene Hierarchy")]
    [Tooltip("Name of the child GameObject under each scene root that new Interactables are parented to. Created automatically if missing.")]
    public string InteractablesContainerName = "Interactables";

    [Tooltip("Placeholders: {Name}. Used for the GameObject created by \"+ Add Interactable\".")]
    public string InteractableGameObjectNameTemplate = "{Name}";

    [Header("Prefabs")]
    [Tooltip("The default prefab used when spawning a new Interactable.")]
    public GameObject InteractablePrefab;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(DataFolder))
            Debug.LogWarning($"[InteractableAuthoringSettings] '{name}': DataFolder is empty.", this);

        if (string.IsNullOrEmpty(DataNameTemplate) || !DataNameTemplate.Contains("{Name}"))
            Debug.LogWarning($"[InteractableAuthoringSettings] '{name}': DataNameTemplate should contain " +
                             "{Name} — without it, every data asset gets the same filename and silently collides.", this);

        if (string.IsNullOrEmpty(InteractablesContainerName))
            Debug.LogWarning($"[InteractableAuthoringSettings] '{name}': InteractablesContainerName is empty — " +
                             "new Interactables would be parented directly under the scene root instead.", this);
        if (InteractablePrefab == null)
            Debug.LogWarning($"[InteractableAuthoringSettings] '{name}': InteractablePrefab is unassigned. The window will fall back to creating empty GameObjects.", this);
    }

    public static string ApplyTemplate(string template, params (string token, string value)[] substitutions)
    {
        foreach (var (token, value) in substitutions)
            template = template.Replace("{" + token + "}", value);
        return template;
    }
}
#endif