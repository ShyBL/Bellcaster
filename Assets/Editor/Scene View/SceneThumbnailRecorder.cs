using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class SceneThumbnailRecorder
{
    private const string THUMBNAIL_FOLDER = "Assets/Editor/SceneThumbnails";
    private const int WIDTH  = 256;
    private const int HEIGHT = 144; // 16:9

    static SceneThumbnailRecorder()
    {
        EditorSceneManager.sceneSaved += OnSceneSaved;
    }

    private static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
    {
        CaptureThumbnail(scene);
    }

    public static void CaptureThumbnail(UnityEngine.SceneManagement.Scene scene)
    {
        Camera cam = FindSceneCamera(scene);

        if (cam == null)
        {
            Debug.LogWarning($"[SceneThumbnailRecorder] No camera found in '{scene.name}'. Thumbnail not captured.");
            return;
        }

        if (!Directory.Exists(THUMBNAIL_FOLDER))
            Directory.CreateDirectory(THUMBNAIL_FOLDER);

        // Render from the game camera into a RenderTexture
        RenderTexture rt = new RenderTexture(WIDTH, HEIGHT, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing  = 1;

        RenderTexture prevTarget = cam.targetTexture;
        Rect          prevRect   = cam.rect;

        cam.targetTexture = rt;
        cam.rect          = new Rect(0, 0, 1, 1); // full render, ignore any viewport splits
        cam.Render();

        // Read pixels out
        RenderTexture.active = rt;
        Texture2D thumbnail  = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        thumbnail.ReadPixels(new Rect(0, 0, WIDTH, HEIGHT), 0, 0);
        thumbnail.Apply();

        // Restore camera state
        cam.targetTexture = prevTarget;
        cam.rect          = prevRect;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        // Write to disk
        byte[] bytes   = thumbnail.EncodeToPNG();
        string guid    = AssetDatabase.AssetPathToGUID(scene.path);
        string filePath = $"{THUMBNAIL_FOLDER}/{guid}.png";

        File.WriteAllBytes(filePath, bytes);
        AssetDatabase.ImportAsset(filePath);

        Object.DestroyImmediate(thumbnail);
    }

    /// <summary>
    /// Finds the best camera to use for the thumbnail in this order:
    /// 1. Camera tagged "MainCamera" that belongs to this scene
    /// 2. Any enabled camera in the scene (lowest depth wins = background camera)
    /// </summary>
    private static Camera FindSceneCamera(UnityEngine.SceneManagement.Scene scene)
    {
        // Camera.main only works if the scene is active, so we search manually
        Camera best = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(includeInactive: false))
            {
                if (!cam.enabled) continue;

                // Prefer MainCamera tag
                if (cam.CompareTag("MainCamera"))
                    return cam;

                // Otherwise take the one with the lowest depth (renders first / background)
                if (best == null || cam.depth < best.depth)
                    best = cam;
            }
        }

        return best;
    }

    public static Texture2D GetThumbnail(string guid)
    {
        string path = $"{THUMBNAIL_FOLDER}/{guid}.png";
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}