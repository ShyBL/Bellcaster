using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using System.IO;

[InitializeOnLoad]
public static class SceneThumbnailRecorder
{
    private const string THUMBNAIL_FOLDER = "Assets/Editor/SceneThumbnails";

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
        Camera sceneCam = FindSceneCamera(scene);
        if (sceneCam == null) return;

        if (!Directory.Exists(THUMBNAIL_FOLDER))
            Directory.CreateDirectory(THUMBNAIL_FOLDER);

        // 1. SUPERSAMPLING (Capture at 1024 for a 512 display)
        int width = 1024; 
        int height = 576; 

        // 2. COLOR SPACE: Use sRGB = true if project is Linear
        RenderTextureDescriptor desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 24);
        desc.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
        desc.msaaSamples = 8; // Anti-aliasing

        RenderTexture rt = RenderTexture.GetTemporary(desc);
        sceneCam.targetTexture = rt;

        // 3. RENDER (Standard Public API)
        // In SRP (URP/HDRP), this call automatically triggers the pipeline's 
        // internal rendering logic including Post-Processing.
        sceneCam.Render();

        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false, QualitySettings.activeColorSpace == ColorSpace.Linear);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        // Clean up
        sceneCam.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string guid = AssetDatabase.AssetPathToGUID(scene.path);
        string fileName = $"{THUMBNAIL_FOLDER}/{guid}.png";
    
        File.WriteAllBytes(fileName, bytes);
    
        // 4. FORCE HIGH QUALITY IMPORT SETTINGS
        AssetDatabase.ImportAsset(fileName);
        SetHighQualityImportSettings(fileName);
    }

    private static void SetHighQualityImportSettings(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.GUI; // Optimized for UI
        importer.mipmapEnabled = false;                 // Crisper at small sizes
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed; // FIXES PIXELATION
        importer.sRGBTexture = QualitySettings.activeColorSpace == ColorSpace.Linear;

        importer.SaveAndReimport();
    }

    public static Texture2D GetThumbnail(string guid)
    {
        string path = $"{THUMBNAIL_FOLDER}/{guid}.png";
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Camera FindSceneCamera(UnityEngine.SceneManagement.Scene scene)
    {
        Camera best = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(false))
            {
                if (!cam.enabled) continue;
                if (cam.CompareTag("MainCamera")) return cam;
                if (best == null || cam.depth < best.depth) best = cam;
            }
        }
        return best;
    }
}