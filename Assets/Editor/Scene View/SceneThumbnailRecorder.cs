using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class SceneThumbnailRecorder
{
    private const string THUMBNAIL_FOLDER = "Assets/Editor/SceneThumbnails";

    static SceneThumbnailRecorder()
    {
        // Capture whenever a scene is saved
        EditorSceneManager.sceneSaved += OnSceneSaved;
    }

    private static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
    {
        CaptureThumbnail(scene);
    }

    public static void CaptureThumbnail(UnityEngine.SceneManagement.Scene scene)
    {
        if (SceneView.lastActiveSceneView == null) return;

        // Ensure directory exists
        if (!Directory.Exists(THUMBNAIL_FOLDER))
            Directory.CreateDirectory(THUMBNAIL_FOLDER);

        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        int width = 256;
        int height = 144; // 16:9 aspect ratio

        RenderTexture rt = new RenderTexture(width, height, 24);
        sceneCam.targetTexture = rt;
        sceneCam.Render();

        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        sceneCam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string guid = AssetDatabase.AssetPathToGUID(scene.path);
        string fileName = $"{THUMBNAIL_FOLDER}/{guid}.png";
        
        File.WriteAllBytes(fileName, bytes);
        AssetDatabase.ImportAsset(fileName);
    }

    public static Texture2D GetThumbnail(string guid)
    {
        string path = $"{THUMBNAIL_FOLDER}/{guid}.png";
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}