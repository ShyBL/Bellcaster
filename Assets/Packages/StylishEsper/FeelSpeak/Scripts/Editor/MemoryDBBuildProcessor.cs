//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Database;
using Esper.MemoryDB;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Esper.FeelSpeak.Editor
{
    public class MemoryDBBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.WebGL && FeelSpeak.Settings.databaseType == Settings.FeelSpeakSettings.DatabaseType.SQLite)
            {
                if (EditorUtility.DisplayDialog("Feel Speak Warning", "You're building for WebGL, but the database is set to SQLite. " +
                    "WebGL does not support SQLite. Switch to MDB?", "Yes", "No"))
                {
                    FeelSpeak.Settings.databaseType = Settings.FeelSpeakSettings.DatabaseType.MDB;
                    EditorUtility.SetDirty(FeelSpeak.Settings);
                }
            }
            else if (FeelSpeak.Settings.databaseType != Settings.FeelSpeakSettings.DatabaseType.MDB)
            {
                return;
            }

            Debug.Log("Feel Speak: preparing MemoryDB before build...");

            var existing = Resources.LoadAll<MemoryDatabase>("FeelSpeak");

            foreach (var item in existing)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(item));
            }

            var path = Path.Combine(AssetSearch.FolderOf<TextAsset>("FeelSpeakIdentifier"), "Resources", "FeelSpeakResources", $"{FeelSpeak.Settings.databaseName}.asset");
            var db = MemoryDatabase.CreateIfNotExists(path);

            FeelSpeakDatabase.PopulateMemoryDatabase(db);
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();

            Debug.Log("MemoryDB updated successfully before build.");
        }
    }
}
#endif