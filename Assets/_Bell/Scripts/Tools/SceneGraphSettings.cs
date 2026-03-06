using UnityEngine;

/// <summary>
/// Project-level settings for the Scene Relationship Graph editor.
/// Shared across the team via version control.
/// Create via: Assets > Create > Tools > Scene Graph Settings
/// </summary>
[CreateAssetMenu(fileName = "SceneGraphSettings", menuName = "Tools/Scene Graph Settings")]
public class SceneGraphSettings : ScriptableObject
{
    private const string AssetPath = "Assets/Editor/SceneGraphSettings.asset";

    // ── Viewport ─────────────────────────────────────────────────────────────
    [Header("Viewport")]
    public Vector3 ViewPosition = Vector3.zero;
    public Vector3 ViewScale    = Vector3.one;

    // ── Window ───────────────────────────────────────────────────────────────
    [Header("Window")]
    public Color WindowColor = new Color(0.15f, 0.15f, 0.15f, 1f);

    // ── Grid ─────────────────────────────────────────────────────────────────
    [Header("Grid")]
    public Color GridBackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1f);
    public Color GridLineColor       = new Color(0.22f, 0.22f, 0.22f, 1f);

    // ── Nodes ─────────────────────────────────────────────────────────────────
    [Header("Nodes")]
    public Vector2 NodeSize       = new Vector2(210, 180);
    public Color NodeTitleColor   = new Color(0.16f, 0.16f, 0.16f, 0.95f);
    public Color NodeBodyColor    = new Color(0.22f, 0.22f, 0.22f, 0.95f);

    // ── Edges ─────────────────────────────────────────────────────────────────
    [Header("Edges")]
    public Color EdgeColor           = new Color(0.53f, 0.81f, 0.98f, 1f);

    // ── Static loader ────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the settings asset, or creates one at the default path if none exists.
    /// </summary>
    public static SceneGraphSettings GetOrCreate()
    {
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SceneGraphSettings");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<SceneGraphSettings>(path);
        }

        // Create folder if needed
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Editor"))
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Editor");

        var settings = CreateInstance<SceneGraphSettings>();
        UnityEditor.AssetDatabase.CreateAsset(settings, AssetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        return settings;
#else
        return null;
#endif
    }
}