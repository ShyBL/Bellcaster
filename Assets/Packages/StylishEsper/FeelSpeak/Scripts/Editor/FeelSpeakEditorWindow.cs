//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Database;
using Esper.FeelSpeak.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class FeelSpeakEditorWindow : EditorWindow
    {
        private Button dialogueButton;
        private Button characterButton;
        private Button emotionButton;
        private Button createButton;
        private Button deleteButton;
        private Button previousPageButton;
        private Button nextPageButton;
        private Label pagesLabel;
        private Label amountLabel;
        private DropdownField sortField;
        private ToolbarSearchField searchField;
        private VisualElement dialogueTab;
        private VisualElement characterTab;
        private VisualElement emotionTab;
        private VisualElement dialogueInspector;
        private VisualElement characterInspector;
        private VisualElement emotionInspector;

        private IntegerField dialogueIDField;
        private TextField dialogueNameField;
        private Button openInDialogueEditorButton;

        private IntegerField characterIDField;
        private ObjectField characterSpriteField;
        private TextField characterNameField;
        private ObjectField characterTextSoundField;
        private ColorField characterTextColorField;
        private List<CharacterEmotionField> characterEmotionFields = new();
        private Label characterEmotionSettingsLabel;

        private IntegerField emotionIDField;
        private ObjectField emotionSpriteField;
        private TextField emotionNameField;
        private TextField emotionAnimationNameField;
        private ObjectField emotionSoundField;
        private FloatField emotionTextSpeedMultiplierField;
        private ColorField emotionTextColorField;

        private Dictionary<DialogueGraph, DialogueElement> loadedGraphElements = new();
        private Dictionary<Character, CharacterElement> loadedCharacterElements = new();
        private Dictionary<Emotion, EmotionElement> loadedEmotionElements = new();

        private DialogueGraph selectedGraph;
        private Character selectedCharacter;
        private Emotion selectedEmotion;

        private int amountPerPage = 50;
        private int currentPage = 1;
        private int maxPages = 1;
        private int currentPageFiltered = 1;
        private int maxPagesFiltered = 1;
        private int totalCount;
        private bool filterOn;

        [MenuItem("Window/Feel Speak/Object Manager")]
        public static void Open()
        {
            FeelSpeakEditorWindow wnd = GetWindow<FeelSpeakEditorWindow>();
            wnd.titleContent = new GUIContent("Feel Speak");
        }

        private void CreateGUI()
        {
            minSize = new Vector2(700, 500);

            VisualElement root = rootVisualElement;
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "FeelSpeakEditorWindow");
            visualTree.CloneTree(root);

            FeelSpeakSettingsEditorWindow.GetOrCreateSettings();
            FeelSpeakDatabase.Initialize();

            dialogueButton = root.Q<Button>("DialogueButton");
            characterButton = root.Q<Button>("CharacterButton");
            emotionButton = root.Q<Button>("EmotionButton");
            createButton = root.Q<Button>("CreateButton");
            deleteButton = root.Q<Button>("DeleteButton");
            previousPageButton = root.Q<Button>("PreviousPageButton");
            nextPageButton = root.Q<Button>("NextPageButton");
            pagesLabel = root.Q<Label>("PagesLabel");
            amountLabel = root.Q<Label>("AmountLabel");
            sortField = root.Q<DropdownField>("SortField");
            searchField = root.Q<ToolbarSearchField>("SearchField");
            dialogueTab = root.Q<VisualElement>("DialogueTab");
            characterTab = root.Q<VisualElement>("CharacterTab");
            emotionTab = root.Q<VisualElement>("EmotionTab");
            dialogueInspector = root.Q<VisualElement>("DialogueInspector");
            characterInspector = root.Q<VisualElement>("CharacterInspector");
            emotionInspector = root.Q<VisualElement>("EmotionInspector");
            openInDialogueEditorButton = root.Q<Button>("OpenInDialogueEditorButton");

            dialogueIDField = root.Q<IntegerField>("DialogueIDField");
            dialogueNameField = root.Q<TextField>("DialogueNameField");
            openInDialogueEditorButton.BringToFront();

            characterIDField = root.Q<IntegerField>("CharacterIDField");
            characterSpriteField = root.Q<ObjectField>("CharacterSpriteField");
            characterNameField = root.Q<TextField>("CharacterNameField");
            characterTextSoundField = root.Q<ObjectField>("CharacterTextSoundField");
            characterTextColorField = root.Q<ColorField>("CharacterTextColorField");
            characterEmotionSettingsLabel = root.Q<Label>("CharacterEmotionSettingsLabel");

            emotionIDField = root.Q<IntegerField>("EmotionIDField");
            emotionSpriteField = root.Q<ObjectField>("EmotionSpriteField");
            emotionNameField = root.Q<TextField>("EmotionNameField");
            emotionAnimationNameField = root.Q<TextField>("EmotionAnimationNameField");
            emotionSoundField = root.Q<ObjectField>("EmotionSoundField");
            emotionTextSpeedMultiplierField = root.Q<FloatField>("EmotionTextSpeedMultiplierField");
            emotionTextColorField = root.Q<ColorField>("EmotionTextColorField");

            emotionTextSpeedMultiplierField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue < 0.001f)
                {
                    emotionTextSpeedMultiplierField.value = 0.001f;
                }
            });

            dialogueIDField.SetEnabled(false);
            characterIDField.SetEnabled(false);
            emotionIDField.SetEnabled(false);

            previousPageButton.clicked += PreviousPage;
            nextPageButton.clicked += NextPage;

            dialogueButton.clicked += () =>
            {
                EnableTab(dialogueTab, dialogueInspector);
                SelectTabButton(dialogueButton);
                ResetFilters();
            };

            characterButton.clicked += () =>
            {
                EnableTab(characterTab, characterInspector);
                SelectTabButton(characterButton);
                ResetFilters();
            };

            emotionButton.clicked += () =>
            {
                EnableTab(emotionTab, emotionInspector);
                SelectTabButton(emotionButton);
                ResetFilters();
            };

            createButton.clicked += CreateObject;
            deleteButton.clicked += DeleteObject;

            searchField.RegisterValueChangedCallback(x =>
            {
                ApplyFilter();
            });

            dialogueNameField.RegisterValueChangedCallback(x =>
            {
                if (!selectedGraph || !loadedGraphElements.ContainsKey(selectedGraph))
                {
                    return;
                }

                loadedGraphElements[selectedGraph].Refresh();
            });

            dialogueNameField.RegisterCallback<FocusOutEvent>(x =>
            {
                if (!selectedGraph)
                {
                    return;
                }

                selectedGraph.UpdateAssetName();
            });

            characterNameField.RegisterValueChangedCallback(x =>
            {
                if (!selectedCharacter || !loadedCharacterElements.ContainsKey(selectedCharacter))
                {
                    return;
                }

                loadedCharacterElements[selectedCharacter].Refresh();
            });

            characterNameField.RegisterCallback<FocusOutEvent>(x =>
            {
                if (!selectedCharacter)
                {
                    return;
                }

                selectedCharacter.UpdateAssetName();
            });

            characterSpriteField.RegisterValueChangedCallback(x =>
            {
                if (!selectedCharacter || !loadedCharacterElements.ContainsKey(selectedCharacter))
                {
                    return;
                }

                loadedCharacterElements[selectedCharacter].Refresh();
            });

            emotionNameField.RegisterValueChangedCallback(x =>
            {
                if (!selectedEmotion || !loadedEmotionElements.ContainsKey(selectedEmotion))
                {
                    return;
                }

                loadedEmotionElements[selectedEmotion].Refresh();
            });

            emotionNameField.RegisterCallback<FocusOutEvent>(x =>
            {
                if (!selectedEmotion)
                {
                    return;
                }

                selectedEmotion.UpdateAssetName();
            });

            emotionSpriteField.RegisterValueChangedCallback(x =>
            {
                if (!selectedEmotion || !loadedEmotionElements.ContainsKey(selectedEmotion))
                {
                    return;
                }

                loadedEmotionElements[selectedEmotion].Refresh();
            });

            sortField.RegisterValueChangedCallback(x =>
            {
                ReloadCurrentPage();
            });

            openInDialogueEditorButton.clicked += OpenDialogue;

            EnableTab(dialogueTab, dialogueInspector);
            ResetFilters();
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying && !HasOpenInstances<DialogueGraphEditorWindow>())
            {
                FeelSpeakDatabase.Disconnect();
            }
        }

        public void OpenDialogue()
        {
            if (!selectedGraph)
            {
                return;
            }

            DialogueGraphEditorWindow.Open(selectedGraph);
        }

        public void ResetFilters()
        {
            searchField.SetValueWithoutNotify(string.Empty);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            filterOn = !string.IsNullOrEmpty(searchField.value);
            RecalculatePages();
            ReloadCurrentPage();
        }

        private void SelectTabButton(Button button)
        {
            dialogueButton.SetEnabled(true);
            characterButton.SetEnabled(true);
            emotionButton.SetEnabled(true);

            button.SetEnabled(false);
        }

        private void ShowDialogues()
        {
            EnableTab(dialogueTab, dialogueInspector);
            SelectTabButton(dialogueButton);
            ResetFilters();
        }

        private void SelectGraph(DialogueGraph graph)
        {
            if (selectedGraph && loadedGraphElements.ContainsKey(selectedGraph))
            {
                loadedGraphElements[selectedGraph].SetEnabled(true);
            }

            selectedGraph = graph;

            if (selectedGraph)
            {
                if (selectedGraph && loadedGraphElements.ContainsKey(selectedGraph))
                {
                    loadedGraphElements[selectedGraph].SetEnabled(false);
                }

                dialogueInspector.style.display = DisplayStyle.Flex;

                var serializedObject = new SerializedObject(selectedGraph);
                dialogueIDField.BindProperty(serializedObject.FindProperty("id"));
                dialogueNameField.BindProperty(serializedObject.FindProperty("graphName"));

                if (HasOpenInstances<DialogueGraphEditorWindow>())
                {
                    DialogueGraphEditorWindow.Open(selectedGraph);
                }
            }
            else
            {
                dialogueInspector.style.display = DisplayStyle.None;
            }
        }

        private void SelectCharacter(Character character)
        {
            if (selectedCharacter && loadedCharacterElements.ContainsKey(selectedCharacter))
            {
                loadedCharacterElements[selectedCharacter].SetEnabled(true);
            }

            selectedCharacter = character;

            if (selectedCharacter)
            {
                if (selectedCharacter && loadedCharacterElements.ContainsKey(selectedCharacter))
                {
                    loadedCharacterElements[selectedCharacter].SetEnabled(false);
                }

                characterInspector.style.display = DisplayStyle.Flex;

                var serializedObject = new SerializedObject(selectedCharacter);
                characterIDField.BindProperty(serializedObject.FindProperty("id"));
                characterSpriteField.BindProperty(serializedObject.FindProperty("sprite"));
                characterNameField.BindProperty(serializedObject.FindProperty("characterName"));
                characterTextSoundField.BindProperty(serializedObject.FindProperty("textSound"));
                characterTextColorField.BindProperty(serializedObject.FindProperty("textColor"));

                foreach (var item in characterEmotionFields)
                {
                    characterInspector.Remove(item);
                }

                characterEmotionFields.Clear();

                characterEmotionSettingsLabel.BringToFront();

                var characterEmotionsProperty = serializedObject.FindProperty("characterEmotions");

                for (int i = 0; i < selectedCharacter.characterEmotions.Count; i++)
                {
                    int index = i;
                    var field = new CharacterEmotionField();
                    field.BindProperty(selectedCharacter.characterEmotions[index].emotion.emotionName, characterEmotionsProperty.GetArrayElementAtIndex(index));
                    characterInspector.Add(field);
                    characterEmotionFields.Add(field);
                }

                characterTextColorField.style.display = FeelSpeak.Settings.enableObjectTextColors ? DisplayStyle.Flex : DisplayStyle.None;
                characterTextSoundField.style.display = FeelSpeak.Settings.enableTextSounds ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                characterInspector.style.display = DisplayStyle.None;
            }
        }

        private void SelectEmotion(Emotion emotion)
        {
            if (selectedEmotion && loadedEmotionElements.ContainsKey(selectedEmotion))
            {
                loadedEmotionElements[selectedEmotion].SetEnabled(true);
            }

            selectedEmotion = emotion;

            if (selectedEmotion)
            {
                if (selectedEmotion && loadedEmotionElements.ContainsKey(selectedEmotion))
                {
                    loadedEmotionElements[selectedEmotion].SetEnabled(false);
                }

                emotionInspector.style.display = DisplayStyle.Flex;

                var serializedObject = new SerializedObject(selectedEmotion);
                emotionIDField.BindProperty(serializedObject.FindProperty("id"));
                emotionNameField.BindProperty(serializedObject.FindProperty("emotionName"));
                emotionSpriteField.BindProperty(serializedObject.FindProperty("sprite"));
                emotionAnimationNameField.BindProperty(serializedObject.FindProperty("animationName"));
                emotionSoundField.BindProperty(serializedObject.FindProperty("sound"));
                emotionTextSpeedMultiplierField.BindProperty(serializedObject.FindProperty("textSpeedMultiplier"));
                emotionTextColorField.BindProperty(serializedObject.FindProperty("textColor"));

                emotionSoundField.style.display = FeelSpeak.Settings.enableTextSounds ? DisplayStyle.Flex : DisplayStyle.None;
                emotionTextColorField.style.display = FeelSpeak.Settings.enableObjectTextColors ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else
            {
                emotionInspector.style.display = DisplayStyle.None;
            }
        }

        private void RebuildCharacterEmotions()
        {
            var characters = FeelSpeak.GetAllCharacters();
            var emotions = FeelSpeak.GetAllEmotions().ToList();

            foreach (var character in characters)
            {
                character.RebuildCharacterEmotionsList(emotions);
                character.Save();
            }
        }

        private void CreateObject()
        {
             if (dialogueTab.style.display == DisplayStyle.Flex)
            {
                var graph = DialogueGraph.Create();
                RecalculatePages();
                currentPage = maxPages;
                RefreshPagesLabel();
                ReloadCurrentPage();
                SelectGraph(graph);
                UpdateAmountLabel();
            }
            else if (characterTab.style.display == DisplayStyle.Flex)
            {
                var character = Character.Create();
                RecalculatePages();
                currentPage = maxPages;
                RefreshPagesLabel();
                ReloadCurrentPage();
                SelectCharacter(character);
                UpdateAmountLabel();
            }
            else
            {
                var emotion = Emotion.Create();
                RebuildCharacterEmotions();
                RecalculatePages();
                currentPage = maxPages;
                RefreshPagesLabel();
                ReloadCurrentPage();
                SelectEmotion(emotion);
                UpdateAmountLabel();
            }
        }

        private void DeleteObject()
        {
            if (dialogueTab.style.display == DisplayStyle.Flex)
            {
                if (!selectedGraph)
                {
                    return;
                }

                if (EditorUtility.DisplayDialog("Are you sure?", $"Are you sure you'd like to delete the '{selectedGraph.graphName}' dialogue graph?", "Yes", "Cancel"))
                {
                    selectedGraph.DeleteDatabaseRecord();
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selectedGraph));
                    RecalculatePages();
                    SelectGraph(null);
                    ReloadCurrentPage();
                    UpdateAmountLabel();
                }
            }
            else if (characterTab.style.display == DisplayStyle.Flex)
            {
                if (!selectedCharacter)
                {
                    return;
                }

                if (EditorUtility.DisplayDialog("Are you sure?", $"Are you sure you'd like to delete the '{selectedCharacter.characterName}' character?", "Yes", "Cancel"))
                {
                    selectedCharacter.DeleteDatabaseRecord();
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selectedCharacter));
                    RecalculatePages();
                    SelectCharacter(null);
                    ReloadCurrentPage();
                    UpdateAmountLabel();
                }
            }
            else
            {
                if (!selectedEmotion)
                {
                    return;
                }

                if (EditorUtility.DisplayDialog("Are you sure?", $"Are you sure you'd like to delete the '{selectedEmotion.emotionName}' emotion?", "Yes", "Cancel"))
                {
                    selectedEmotion.DeleteDatabaseRecord();
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selectedEmotion));
                    RebuildCharacterEmotions();
                    RecalculatePages();
                    SelectEmotion(null);
                    ReloadCurrentPage();
                    UpdateAmountLabel();
                }
            }
        }

        private void EnableTab(VisualElement tab, VisualElement inspector)
        {
            SelectGraph(null);
            SelectCharacter(null);
            SelectEmotion(null);

            dialogueTab.style.display = DisplayStyle.None;
            characterTab.style.display = DisplayStyle.None;
            emotionTab.style.display = DisplayStyle.None;
            dialogueInspector.style.display = DisplayStyle.None;
            characterInspector.style.display = DisplayStyle.None;
            emotionInspector.style.display = DisplayStyle.None;

            tab.style.display = DisplayStyle.Flex;
        }

        private void ReloadCurrentPage()
        {
            if (dialogueTab.style.display == DisplayStyle.Flex)
            {
                dialogueTab.Clear();
                loadedGraphElements.Clear();
                List<DialogueRecord> records;

                if (!filterOn)
                {
                    var min = (currentPage - 1) * amountPerPage;
                    var max = currentPage * amountPerPage;
                    records = FeelSpeakDatabase.GetDialogueRecords(min, max, "%", sortField.index == 1);
                }
                else
                {
                    var min = (currentPageFiltered - 1) * amountPerPage;
                    var max = currentPageFiltered * amountPerPage;
                    records = FeelSpeakDatabase.GetDialogueRecords(min, max, searchField.value, sortField.index == 1);
                }

                foreach (var record in records)
                {
                    var obj = FeelSpeak.GetDialogueGraph(record);

                    try
                    {
                        var element = new DialogueElement(obj);
                        element.clicked += () => SelectGraph(obj);
                        element.Refresh();
                        dialogueTab.Add(element);
                        loadedGraphElements.Add(obj, element);
                    }
                    catch
                    {
                        obj.graphName = obj.SanitizeName(obj.graphName);
                        obj.UpdateAssetName();
                        Debug.LogError("A dialogue graph failed to load. Please try again later.");
                    }
                }
            }
            else if (characterTab.style.display == DisplayStyle.Flex)
            {
                characterTab.Clear();
                loadedCharacterElements.Clear();
                List<CharacterRecord> records;

                if (!filterOn)
                {
                    var min = (currentPage - 1) * amountPerPage;
                    var max = currentPage * amountPerPage;
                    records = FeelSpeakDatabase.GetCharacterRecords(min, max, "%", sortField.index == 1);
                }
                else
                {
                    var min = (currentPageFiltered - 1) * amountPerPage;
                    var max = currentPageFiltered * amountPerPage;
                    records = FeelSpeakDatabase.GetCharacterRecords(min, max, searchField.value, sortField.index == 1);
                }

                foreach (var record in records)
                {
                    var obj = FeelSpeak.GetCharacter(record);

                    try
                    {
                        var element = new CharacterElement(obj);
                        element.clicked += () => SelectCharacter(obj);
                        element.Refresh();
                        characterTab.Add(element);
                        loadedCharacterElements.Add(obj, element);
                    }
                    catch
                    {
                        obj.characterName = obj.SanitizeName(obj.characterName);
                        obj.UpdateAssetName();
                        Debug.LogError("A character failed to load. Please try again later.");
                    }
                }
            }
            else
            {
                emotionTab.Clear();
                loadedEmotionElements.Clear();
                List<EmotionRecord> records;

                if (!filterOn)
                {
                    var min = (currentPage - 1) * amountPerPage;
                    var max = currentPage * amountPerPage;
                    records = FeelSpeakDatabase.GetEmotionRecords(min, max, "%", sortField.index == 1);
                }
                else
                {
                    var min = (currentPageFiltered - 1) * amountPerPage;
                    var max = currentPageFiltered * amountPerPage;
                    records = FeelSpeakDatabase.GetEmotionRecords(min, max, searchField.value, sortField.index == 1);
                }

                foreach (var record in records)
                {
                    var obj = FeelSpeak.GetEmotion(record);

                    try
                    {
                        var element = new EmotionElement(obj);
                        element.clicked += () => SelectEmotion(obj);
                        element.Refresh();
                        emotionTab.Add(element);
                        loadedEmotionElements.Add(obj, element);
                    }
                    catch
                    {
                        obj.emotionName = obj.SanitizeName(obj.emotionName);
                        obj.UpdateAssetName();
                        Debug.LogError("An emotion failed to load. Please try again later.");
                    }
                }
            }

            RefreshPagesLabel();
            UpdateAmountLabel();
        }

        private void RecalculatePages()
        {
            var totalCount = GetObjectTotalCount();

            if (totalCount == 0)
            {
                currentPage = 1;
                maxPages = 1;
                currentPageFiltered = 1;
                maxPagesFiltered = 1;
            }
            else
            {
                if (!filterOn)
                {
                    maxPages = Mathf.CeilToInt(totalCount / (float)amountPerPage);

                    if (maxPages == 0)
                    {
                        maxPages = 1;
                    }

                    if (currentPage == 0)
                    {
                        currentPage = 1;
                    }

                    if (currentPage > maxPages)
                    {
                        currentPage = maxPages;
                    }
                }
                else
                {
                    maxPagesFiltered = Mathf.CeilToInt(GetObjectFilteredCount() / (float)amountPerPage);

                    if (maxPagesFiltered == 0)
                    {
                        maxPagesFiltered = 1;
                    }

                    if (currentPageFiltered == 0)
                    {
                        currentPageFiltered = 1;
                    }

                    if (currentPageFiltered > maxPagesFiltered)
                    {
                        currentPageFiltered = maxPagesFiltered;
                    }
                }
            }

            RefreshPagesLabel();
        }

        private void RefreshPagesLabel()
        {
            if (!filterOn)
            {
                pagesLabel.text = $"{currentPage}/{maxPages}";
            }
            else
            {
                pagesLabel.text = $"{currentPageFiltered}/{maxPagesFiltered}";
            }
        }

        private void PreviousPage()
        {
            if (!filterOn)
            {
                if (currentPage == 1)
                {
                    return;
                }

                currentPage--;
            }
            else
            {
                if (currentPageFiltered == 1)
                {
                    return;
                }

                currentPageFiltered--;
            }

            RefreshPagesLabel();
            ReloadCurrentPage();
            UpdateAmountLabel();
        }

        private void NextPage()
        {
            if (!filterOn)
            {
                if (currentPage == maxPages)
                {
                    return;
                }

                currentPage++;
            }
            else
            {
                if (currentPageFiltered == maxPagesFiltered)
                {
                    return;
                }

                currentPageFiltered++;
            }

            RefreshPagesLabel();
            ReloadCurrentPage();
            UpdateAmountLabel();
        }

        private void UpdateAmountLabel()
        {
            int displayMin = 0;
            int displayMax = 0;

            if (!filterOn)
            {
                totalCount = GetObjectTotalCount();

                if (totalCount == 0)
                {
                    amountLabel.text = "0-0 of 0";
                    return;
                }

                displayMin = amountPerPage * (currentPage - 1) + 1;
                displayMax = Math.Min(totalCount, amountPerPage * currentPage);
            }
            else
            {
                totalCount = GetObjectFilteredCount();

                if (totalCount == 0)
                {
                    amountLabel.text = "0-0 of 0";
                    return;
                }

                displayMin = amountPerPage * (currentPageFiltered - 1) + 1;
                displayMax = Math.Min(totalCount, amountPerPage * currentPageFiltered);
            }

            amountLabel.text = $"{displayMin}-{displayMax} of {totalCount}";
        }

        private int GetObjectTotalCount()
        {
            if (dialogueTab.style.display == DisplayStyle.Flex)
            {
                return FeelSpeakDatabase.GetDialogueCount();
            }
            else if (characterTab.style.display == DisplayStyle.Flex)
            {
                return FeelSpeakDatabase.GetCharacterCount();
            }
            else
            {
                return FeelSpeakDatabase.GetEmotionCount();
            }
        }

        private int GetObjectFilteredCount()
        {
            if (dialogueTab.style.display == DisplayStyle.Flex)
            {
                return FeelSpeakDatabase.GetFilteredDialogueCount(searchField.value);
            }
            else if (characterTab.style.display == DisplayStyle.Flex)
            {
                return FeelSpeakDatabase.GetFilteredCharacterCount(searchField.value);
            }
            else
            {
                return FeelSpeakDatabase.GetFilteredEmotionCount(searchField.value);
            }
        }
    }
}
#endif