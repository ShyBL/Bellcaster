//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Database;
using Esper.FeelSpeak.Settings;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class FeelSpeakSettingsEditorWindow : EditorWindow
    {
        private EnumField debugLogField;
        private EnumField databaseTypeField;
        private FloatField textSpeedField;
        private Toggle enableFastForwardField;
        private FloatField sentenceEndPauseField;
        private FloatField ellipsisSpeedMultiplierField;
        private Toggle enableObjectTextColorsField;
        private Toggle enableTextSoundsField;
        private ObjectField defaultTextSoundField;
        private FloatField textSoundMinDelayField;
        private Toggle enableVoiceLinesField;
        private ColorField activeSpeakerSpriteColorField;
        private ColorField inactiveSpeakerSpriteColorField;
        private FloatField choiceDisplayLengthField;
        private Button validateButton;
        private TextField databaseNameField;
        private Toggle defaultAutomaticNextOnCompleteField;
        private FloatField defaultAutomaticNextDelayField;
        private Toggle timeoutEnabledByDefaultField;
        private FloatField defaultTimeoutLengthField;
        private FloatField defaultDelayField;
        private Toggle defaultHideDialogueBoxField;
        private Button deleteAllGraphsButton;
        private Button deleteAllCharactersButton;
        private Button deleteAllEmotionsButton;

        private FeelSpeakSettings settings;

        [MenuItem("Window/Feel Speak/Settings")]
        public static void Open()
        {
            FeelSpeakSettingsEditorWindow wnd = GetWindow<FeelSpeakSettingsEditorWindow>();
            wnd.titleContent = new GUIContent("Feel Speak Settings");
        }

        private void CreateGUI()
        {
            minSize = new Vector2(350, 450);

            VisualElement root = rootVisualElement;
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "FeelSpeakSettingsEditorWindow");
            visualTree.CloneTree(root);

            debugLogField = root.Q<EnumField>("DebugLogField");
            databaseTypeField = root.Q<EnumField>("DatabaseTypeField");
            textSpeedField = root.Q<FloatField>("TextSpeedField");
            enableFastForwardField = root.Q<Toggle>("EnableFastForwardField");
            sentenceEndPauseField = root.Q<FloatField>("SentenceEndPauseField");
            ellipsisSpeedMultiplierField = root.Q<FloatField>("EllipsisSpeedMultiplierField");
            enableObjectTextColorsField = root.Q<Toggle>("EnableObjectTextColorsField");
            enableTextSoundsField = root.Q<Toggle>("EnableTextSoundsField");
            defaultTextSoundField = root.Q<ObjectField>("DefaultTextSoundField");
            textSoundMinDelayField = root.Q<FloatField>("TextSoundMinDelayField");
            enableVoiceLinesField = root.Q<Toggle>("EnableVoiceLinesField");
            activeSpeakerSpriteColorField = root.Q<ColorField>("ActiveSpeakerSpriteColorField");
            inactiveSpeakerSpriteColorField = root.Q<ColorField>("InactiveSpeakerSpriteColorField");
            choiceDisplayLengthField = root.Q<FloatField>("ChoiceDisplayLengthField");
            validateButton = root.Q<Button>("ValidateButton");
            databaseNameField = root.Q<TextField>("DatabaseNameField");
            defaultAutomaticNextOnCompleteField = root.Q<Toggle>("DefaultAutomaticNextOnCompleteField");
            defaultAutomaticNextDelayField = root.Q<FloatField>("DefaultAutomaticNextDelayField");
            timeoutEnabledByDefaultField = root.Q<Toggle>("TimeoutEnabledByDefaultField");
            defaultTimeoutLengthField = root.Q<FloatField>("DefaultTimeoutLengthField");
            defaultDelayField = root.Q<FloatField>("DefaultDelayField");
            defaultHideDialogueBoxField = root.Q<Toggle>("DefaultHideDialogueBoxField");
            deleteAllGraphsButton = root.Q<Button>("DeleteAllGraphsButton");
            deleteAllCharactersButton = root.Q<Button>("DeleteAllCharactersButton");
            deleteAllEmotionsButton = root.Q<Button>("DeleteAllEmotionsButton");

            validateButton.clicked += ValidateDatabase;
            deleteAllGraphsButton.clicked += DeleteAllGraphs;
            deleteAllCharactersButton.clicked += DeleteAllCharacters;
            deleteAllEmotionsButton.clicked += DeleteAllEmotions;

            settings = GetOrCreateSettings();
            var serializedObject = new SerializedObject(settings);
            debugLogField.BindProperty(serializedObject.FindProperty("debugLogMode"));
            databaseTypeField.BindProperty(serializedObject.FindProperty("databaseType"));
            textSpeedField.BindProperty(serializedObject.FindProperty("textSpeed"));
            enableFastForwardField.BindProperty(serializedObject.FindProperty("enableFastForward"));
            sentenceEndPauseField.BindProperty(serializedObject.FindProperty("sentenceEndPause"));
            ellipsisSpeedMultiplierField.BindProperty(serializedObject.FindProperty("ellipsisSpeedMultiplier"));
            enableObjectTextColorsField.BindProperty(serializedObject.FindProperty("enableObjectTextColors"));
            enableTextSoundsField.BindProperty(serializedObject.FindProperty("enableTextSounds"));
            defaultTextSoundField.BindProperty(serializedObject.FindProperty("defaultTextSound"));
            textSoundMinDelayField.BindProperty(serializedObject.FindProperty("textSoundMinDelay"));
            enableVoiceLinesField.BindProperty(serializedObject.FindProperty("enableVoiceLines"));
            activeSpeakerSpriteColorField.BindProperty(serializedObject.FindProperty("activeSpeakerSpriteColor"));
            inactiveSpeakerSpriteColorField.BindProperty(serializedObject.FindProperty("inactiveSpeakerSpriteColor"));
            choiceDisplayLengthField.BindProperty(serializedObject.FindProperty("choiceDisplayLength"));
            databaseNameField.BindProperty(serializedObject.FindProperty("databaseName"));
            defaultAutomaticNextOnCompleteField.BindProperty(serializedObject.FindProperty("defaultAutomaticNextOnComplete"));
            defaultAutomaticNextDelayField.BindProperty(serializedObject.FindProperty("defaultAutomaticNextDelay"));
            timeoutEnabledByDefaultField.BindProperty(serializedObject.FindProperty("timeoutEnabledByDefault"));
            defaultTimeoutLengthField.BindProperty(serializedObject.FindProperty("defaultTimeoutLength"));
            defaultDelayField.BindProperty(serializedObject.FindProperty("defaultDelay"));
            defaultHideDialogueBoxField.BindProperty(serializedObject.FindProperty("defaultHideDialogueBox"));

            enableTextSoundsField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue)
                {
                    defaultTextSoundField.style.display = DisplayStyle.Flex;
                    textSoundMinDelayField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    defaultTextSoundField.style.display = DisplayStyle.None;
                    textSoundMinDelayField.style.display = DisplayStyle.None;
                }
            });

            if (settings.enableTextSounds)
            {
                defaultTextSoundField.style.display = DisplayStyle.Flex;
                textSoundMinDelayField.style.display = DisplayStyle.Flex;
            }
            else
            {
                defaultTextSoundField.style.display = DisplayStyle.None;
                textSoundMinDelayField.style.display = DisplayStyle.None;
            }

            textSoundMinDelayField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0)
                {
                    textSoundMinDelayField.value = 0;
                }
            });

            textSpeedField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0.001f)
                {
                    textSpeedField.value = 0.001f;
                }
            });

            choiceDisplayLengthField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0f)
                {
                    choiceDisplayLengthField.value = 0f;
                }
            });

            sentenceEndPauseField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0f)
                {
                    sentenceEndPauseField.value = 0f;
                }
            });

            ellipsisSpeedMultiplierField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0.001f)
                {
                    ellipsisSpeedMultiplierField.value = 0.001f;
                }
            });

            databaseNameField.RegisterValueChangedCallback(x =>
            {
                if (string.IsNullOrEmpty(x.newValue))
                {
                    databaseNameField.value = "FeelSpeak";
                }
            });

            defaultTimeoutLengthField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0f)
                {
                    defaultTimeoutLengthField.value = 0f;
                }
            });

            defaultDelayField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0f)
                {
                    defaultDelayField.value = 0f;
                }
            });
        }

        private void ValidateDatabase()
        {
            var path = Path.Combine(Application.streamingAssetsPath, settings.databaseName + ".db");

            if (!File.Exists(path))
            {
                FeelSpeakDatabase.Initialize();
            }
            else
            {
                FeelSpeakDatabase.Disconnect();
                FeelSpeakDatabase.DeleteRuntimeDatabaseSQL();
                FeelSpeakDatabase.DeleteEditorDatabaseSQL();
                FeelSpeakDatabase.Initialize();
            }

            var graphs = FeelSpeak.GetAllDialogueGraphs();

            foreach (var item in graphs)
            {
                item.UpdateDatabaseRecord();
            }

            var characters = FeelSpeak.GetAllCharacters();

            foreach (var item in characters)
            {
                item.UpdateDatabaseRecord();
            }

            var emotions = FeelSpeak.GetAllEmotions();

            foreach (var item in emotions)
            {
                item.UpdateDatabaseRecord();
            }

            graphs = null;
            characters = null;
            emotions = null;

            if (!HasOpenInstances<FeelSpeakEditorWindow>() && !HasOpenInstances<DialogueGraphEditorWindow>())
            {
                FeelSpeakDatabase.Disconnect();
            }

            Resources.UnloadUnusedAssets();

            EditorUtility.DisplayDialog("Done.", "Database validated.", "Ok");
        }

        public static FeelSpeakSettings GetOrCreateSettings()
        {
            string assetDirectoryPath = Path.Combine(AssetSearch.FolderOf<TextAsset>("FeelSpeakIdentifier"), "Resources", "FeelSpeakResources", "Settings");
            string fullAssetPath = Path.Combine(assetDirectoryPath, "FeelSpeakSettings.asset");
            string fullDirectoryPath = Path.GetFullPath(assetDirectoryPath);

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            var settings = AssetSearch.Find<FeelSpeakSettings>(fullAssetPath);

            if (!settings)
            {
                settings = CreateInstance<FeelSpeakSettings>();
                settings.debugLogMode = FeelSpeakSettings.DebugLogMode.Normal;
                AssetDatabase.CreateAsset(settings, fullAssetPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        private void DeleteAllGraphs()
        {
            if (!EditorUtility.DisplayDialog($"Delete All Dialogue Graphs?", "Are you sure you want to delete all dialogue graphs? This cannot be undone.", "Delete", "Cancel"))
            {
                return;
            }

            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }

            var graphs = FeelSpeak.GetAllDialogueGraphs();

            foreach (var graph in graphs)
            {
                graph.DeleteDatabaseRecord();
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(graph));
            }

            if (HasOpenInstances<FeelSpeakEditorWindow>())
            {
                GetWindow<FeelSpeakEditorWindow>().Close();
            }

            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }

            EditorUtility.DisplayDialog("Done.", "Successfully deleted.", "Ok");
        }

        private void DeleteAllCharacters()
        {
            if (!EditorUtility.DisplayDialog($"Delete All Characters?", "Are you sure you want to delete all characters? This cannot be undone.", "Delete", "Cancel"))
            {
                return;
            }

            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }

            var characters = FeelSpeak.GetAllCharacters();

            foreach (var character in characters)
            {
                character.DeleteDatabaseRecord();
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(character));
            }

            if (HasOpenInstances<FeelSpeakEditorWindow>())
            {
                GetWindow<FeelSpeakEditorWindow>().Close();
            }

            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }

            EditorUtility.DisplayDialog("Done.", "Successfully deleted.", "Ok");
        }

        private void DeleteAllEmotions()
        {
            if (!EditorUtility.DisplayDialog($"Delete All Emotions?", "Are you sure you want to delete all emotions? This cannot be undone.", "Delete", "Cancel"))
            {
                return;
            }

            bool disconnectOnComplete = false;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
                disconnectOnComplete = true;
            }

            var emotions = FeelSpeak.GetAllEmotions();

            foreach (var emotion in emotions)
            {
                emotion.DeleteDatabaseRecord();
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(emotion));
            }

            if (HasOpenInstances<FeelSpeakEditorWindow>())
            {
                GetWindow<FeelSpeakEditorWindow>().Close();
            }

            if (disconnectOnComplete)
            {
                FeelSpeakDatabase.Disconnect();
            }

            EditorUtility.DisplayDialog("Done.", "Successfully deleted.", "Ok");
        }
    }
}
#endif