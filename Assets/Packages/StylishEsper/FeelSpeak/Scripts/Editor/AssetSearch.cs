//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Esper.FeelSpeak.Editor
{
    public static class AssetSearch
    {
        public static string FindFolder(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return string.Empty;
            }

            string[] folderGuids = AssetDatabase.FindAssets($"t:folder {folderName}");

            foreach (string guid in folderGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string folderOnly = Path.GetFileName(path);

                if (folderOnly == folderName)
                {
                    return path;
                }
            }

            return null;
        }

        public static string FolderOf<T>(string assetName) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {assetName}");

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);

                if (fileName == assetName)
                {
                    string folder = Path.GetDirectoryName(assetPath).Replace('\\', '/');
                    return folder;
                }
            }

            return null;
        }

        public static T Find<T>(string startFolder, string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(Path.Combine(FindFolder(startFolder), path));
        }

        public static T Find<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static T FindAsset<T>(string pathRequirement, string assetName) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {assetName}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(pathRequirement))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }

            return null;
        }

        public static T FindFirstInScene<T>() where T : Object
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<T>(true);
#endif
        }
    }
}
#endif