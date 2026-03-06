//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Database;
using Esper.FeelSpeak.Graph;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class FeelSpeakObjectFinder : EditorWindow
    {
        private ToolbarSearchField searchField;
        private VisualElement dialogueContent;
        private VisualElement characterContent;
        private VisualElement emotionContent;
        private Action<DialogueGraph> onGraphSelected;
        private Action<Character> onCharacterSelected;
        private Action<Emotion> onEmotionSelected;
        private Button noneButton;
        private Label searchLabel;

        private bool acceptsNull = true;

        public void CreateGUI()
        {
            minSize = new Vector2(310, 440);
            maxSize = new Vector2(310, 440);

            VisualElement root = rootVisualElement;
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "FeelSpeakObjectFinder");
            visualTree.CloneTree(root);

            searchField = root.Q<ToolbarSearchField>("SearchField");
            dialogueContent = root.Q<VisualElement>("DialogueContent");
            characterContent = root.Q<VisualElement>("CharacterContent");
            emotionContent = root.Q<VisualElement>("EmotionContent");
            noneButton = root.Q<Button>("NoneButton");
            searchLabel = root.Q<Label>("SearchLabel");

            dialogueContent.Clear();
            characterContent.Clear();
            emotionContent.Clear();

            noneButton.clicked += () =>
            {
                if (onGraphSelected != null)
                {
                    onGraphSelected(null);
                }
                else if (onCharacterSelected != null)
                {
                    onCharacterSelected(null);
                }
                else if (onEmotionSelected != null)
                {
                    onEmotionSelected(null);
                }

                Close();
            };

            searchField.RegisterValueChangedCallback(x => ApplyFilter(x.newValue));

            FeelSpeakDatabase.Initialize();

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private void OnBeforeAssemblyReload()
        {
            Close();
        }

        private void OnDestroy()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        }

        public void LookForDialogueGraph(Action<DialogueGraph> onGraphSelected, bool acceptsNull)
        {
            titleContent = new GUIContent("Graph Finder");
            this.onGraphSelected = onGraphSelected;
            onCharacterSelected = null;
            onEmotionSelected = null;
            searchField.Focus();
            this.acceptsNull = acceptsNull;
            UpdateElements();
        }

        public void LookForCharacter(Action<Character> onCharacterSelected, bool acceptsNull)
        {
            titleContent = new GUIContent("Character Finder");
            this.onCharacterSelected = onCharacterSelected;
            onGraphSelected = null;
            onEmotionSelected = null;
            searchField.Focus();
            this.acceptsNull = acceptsNull;
            UpdateElements();
        }

        public void LookForEmotion(Action<Emotion> onEmotionSelected, bool acceptsNull)
        {
            titleContent = new GUIContent("Emotion Finder");
            this.onEmotionSelected = onEmotionSelected;
            onGraphSelected = null;
            onCharacterSelected = null;
            searchField.Focus();
            this.acceptsNull = acceptsNull;
            UpdateElements();
        }

        private void UpdateElements()
        {
            if (acceptsNull)
            {
                searchLabel.text = "Search for an object or click the button below to set to none.";
                noneButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                searchLabel.text = "Search for an object.";
                noneButton.style.display = DisplayStyle.None;
            }

            dialogueContent.style.display = DisplayStyle.None;
            characterContent.style.display = DisplayStyle.None;
            emotionContent.style.display = DisplayStyle.None;

            if (onGraphSelected != null)
            {
                dialogueContent.style.display = DisplayStyle.Flex;
            }
            else if (onCharacterSelected != null)
            {
                characterContent.style.display = DisplayStyle.Flex;
            }
            else if (onEmotionSelected != null)
            {
                emotionContent.style.display = DisplayStyle.Flex;
            }
        }

        private void ApplyFilter(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                searchLabel.style.display = DisplayStyle.Flex;
                dialogueContent.Clear();
                characterContent.Clear();
                emotionContent.Clear();
                return;
            }

            searchLabel.style.display = DisplayStyle.None;

            if (!FeelSpeakDatabase.IsConnected)
            {
                FeelSpeakDatabase.Initialize();
            }

            if (onGraphSelected != null)
            {
                dialogueContent.Clear();

                var records = FeelSpeakDatabase.GetDialogueRecords(pattern, false);

                foreach (var record in records)
                {
                    var obj = FeelSpeak.GetDialogueGraph(record);
                    var element = new DialogueElement(obj);
                    element.Refresh();

                    element.clicked += () =>
                    {
                        onGraphSelected(obj);
                        Close();
                    };

                    dialogueContent.Add(element);
                }
            }
            else if (onCharacterSelected != null)
            {
                characterContent.Clear();

                var records = FeelSpeakDatabase.GetCharacterRecords(pattern, false);

                foreach (var record in records)
                {
                    var obj = FeelSpeak.GetCharacter(record);
                    var element = new CharacterElement(obj);
                    element.Refresh();

                    element.clicked += () =>
                    {
                        onCharacterSelected(obj);
                        Close();
                    };

                    characterContent.Add(element);
                }
            }
            else if (onEmotionSelected != null)
            {
                emotionContent.Clear();

                var records = FeelSpeakDatabase.GetEmotionRecords(pattern, false);

                foreach (var record in records)
                {
                    var obj = FeelSpeak.GetEmotion(record);
                    var element = new EmotionElement(obj);
                    element.Refresh();

                    element.clicked += () =>
                    {
                        onEmotionSelected(obj);
                        Close();
                    };

                    emotionContent.Add(element);
                }
            }
        }
    }
}
#endif