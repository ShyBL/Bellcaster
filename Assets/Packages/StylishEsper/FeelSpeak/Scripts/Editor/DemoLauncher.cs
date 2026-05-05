//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Database;
using Esper.FeelSpeak.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class DemoLauncher : EditorWindow
    {
        private Label importedLabel;

        [MenuItem("Window/Feel Speak/Demo Launcher", secondaryPriority = 2)]
        public static void Open()
        {
            DemoLauncher wnd = GetWindow<DemoLauncher>();
            wnd.titleContent = new GUIContent("Demo Launcher");
        }

        public void CreateGUI()
        {
            minSize = new Vector2(700, 550);

            VisualElement root = rootVisualElement;
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "DemoLauncher");
            visualTree.CloneTree(root);

            FeelSpeakSettingsEditorWindow.GetOrCreateSettings();

            var importButton = root.Q<Button>("ImportButton");
            var removeButton = root.Q<Button>("RemoveButton");
            importButton.clicked += ImportDemoAssets;
            removeButton.clicked += RemoveDemoAssets;

            importedLabel = root.Q<Label>("ImportedLabel");
            RefreshImportedLabel();

            var fs3d = root.Q<Button>("FS3D");
            var fs2d = root.Q<Button>("FS2D");
            fs3d.clicked += Open3DScene;
            fs2d.clicked += Open2DScene;
        }

        private void RefreshImportedLabel()
        {
            bool disconnectOnComplete = false;
            if (!FeelSpeak.IsInitialized)
            {
                disconnectOnComplete = true;
                FeelSpeakDatabase.Initialize();
            }

            bool alreadyImported = false;
            var record = FeelSpeakDatabase.GetDialogueRecord(-1);

            if (record != null)
            {
                alreadyImported = Resources.Load<DialogueGraph>($"{DialogueGraph.resourcesPath}/{record.objectName}");
            }

            if (alreadyImported)
            {
                importedLabel.text = "The demo assets have already been imported.";
                importedLabel.style.color = Color.green;
            }
            else
            {
                importedLabel.text = "The demo assets have not been imported.";
                importedLabel.style.color = Color.red;
            }

            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }
        }

        private void ImportDemoAssets()
        {
            var demoAssets = AssetSearch.FindAsset<DefaultAsset>("FeelSpeak", "DemoAssets");

            if (!demoAssets)
            {
                EditorUtility.DisplayDialog("Failed", "Demo assets package not found! Has it been removed? A reinstall may be required if this error persists.", "Ok");
                Debug.LogError("Demo assets package not found! Has it been removed? A reinstall may be required if this error persists.");
                return;
            }

            var path = AssetDatabase.GetAssetPath(demoAssets);
            AssetDatabase.importPackageCompleted += OnDemoPackageImported;
            AssetDatabase.ImportPackage(path, false);
        }

        private void OnDemoPackageImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnDemoPackageImported;
            FeelSpeakSettingsEditorWindow.ValidateDatabase();
            RefreshImportedLabel();
        }

        private void RemoveDemoAssets()
        {
            List<string> paths = new();

            var dialogues = FeelSpeak.GetAllDialogueGraphs();
            foreach (var obj in dialogues)
            {
                if (obj.id < 0)
                {
                    paths.Add(AssetDatabase.GetAssetPath(obj));
                }
            }

            var characters = FeelSpeak.GetAllCharacters();
            foreach (var obj in dialogues)
            {
                if (obj.id < 0)
                {
                    paths.Add(AssetDatabase.GetAssetPath(obj));
                }
            }

            var emotions = FeelSpeak.GetAllEmotions();
            foreach (var obj in dialogues)
            {
                if (obj.id < 0)
                {
                    paths.Add(AssetDatabase.GetAssetPath(obj));
                }
            }

            if (paths.Count > 0)
            {
                AssetDatabase.DeleteAssets(paths.ToArray(), new());
            }

            FeelSpeakSettingsEditorWindow.ValidateDatabase();
            RefreshImportedLabel();
        }

        private void Open3DScene()
        {
            var scene = AssetSearch.FindAsset<SceneAsset>("FeelSpeak", "FS3D");

            if (!scene)
            {
                EditorUtility.DisplayDialog("Failed", "3D scene not found! Has it been removed? A reinstall may be required if this error persists.", "Ok");
                Debug.LogError("3D scene not found! Has it been removed? A reinstall may be required if this error persists.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(scene);
            EditorSceneManager.OpenScene(path);
        }

        private void Open2DScene()
        {
            var scene = AssetSearch.FindAsset<SceneAsset>("FeelSpeak", "FS2D");

            if (!scene)
            {
                EditorUtility.DisplayDialog("Failed", "2D scene not found! Has it been removed? A reinstall may be required if this error persists.", "Ok");
                Debug.LogError("2D scene not found! Has it been removed? A reinstall may be required if this error persists.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(scene);
            EditorSceneManager.OpenScene(path);
        }
    }
}
#endif