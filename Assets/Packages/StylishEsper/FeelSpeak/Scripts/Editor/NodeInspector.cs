//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class NodeInspector : DraggableEditorElement
    {
#elif UNITY_2022
    public class NodeInspector : DraggableEditorElement
    {
        public new class UxmlFactory : UxmlFactory<NodeInspector, UxmlTraits> { }
#endif

        public VisualElement dialogueTab;
        public VisualElement dialogueContent;
        public VisualElement choiceTab;
        public VisualElement choiceContent;
        public VisualElement delayTab;
        public VisualElement soundTab;
        public VisualElement delayContent;
        public VisualElement soundContent;
        public VisualElement characterImage;
        public Label characterNameLabel;
        public Button characterButton;
        public VisualElement emotionImage;
        public Label emotionNameLabel;
        public Button emotionButton;
        public Toggle suppressEmotionAnimationField;
        public Toggle suppressEmotionSoundField;
        public Toggle automaticNextOnCompleteField;
        public FloatField automaticNextDelayField;
        public TextField dialogueField;
        public ObjectField voiceLineField;
        public Toggle useTimeoutField;
        public Label automaticChoiceLabel;
        public FloatField timeoutField;
        public Toggle timeoutChoiceIsRandomField;
        public IntegerField timeoutChoiceIndexField;
        public Button addChoiceButton;
        public FloatField delayField;
        public Toggle hideDialogueBoxField;
        public ObjectField soundField;
        public DialogueGraphView graphView;

        public Button setAsStartingNodeButton;

        public DialogueNode dialogueNode;
        public ChoiceNode choiceNode;
        public DelayNode delayNode;
        public SoundNode soundNode;

        private bool enableValueChangedCallbacks;

        public void Initialize(DialogueGraphView graphView)
        {
            this.graphView = graphView;
            dialogueTab = this.Q<VisualElement>("DialogueTab");
            dialogueContent = this.Q<VisualElement>("DialogueContent");
            choiceTab = this.Q<VisualElement>("ChoiceTab");
            choiceContent = this.Q<VisualElement>("ChoiceContent");
            delayTab = this.Q<VisualElement>("DelayTab");
            soundTab = this.Q<VisualElement>("SoundTab");
            delayContent = this.Q<VisualElement>("DelayContent");
            soundContent = this.Q<VisualElement>("soundContent");
            characterImage = this.Q<VisualElement>("CharacterImage");
            characterNameLabel = this.Q<Label>("CharacterNameLabel");
            emotionImage = this.Q<VisualElement>("EmotionImage");
            emotionNameLabel = this.Q<Label>("EmotionNameLabel");

            suppressEmotionAnimationField = this.Q<Toggle>("SuppressEmotionAnimationField");
            RegisterElement(suppressEmotionAnimationField);

            suppressEmotionSoundField = this.Q<Toggle>("SuppressEmotionSoundField");
            RegisterElement(suppressEmotionSoundField);

            automaticNextOnCompleteField = this.Q<Toggle>("AutomaticNextOnCompleteField");
            RegisterElement(automaticNextOnCompleteField);

            automaticNextDelayField = this.Q<FloatField>("AutomaticNextDelayField");
            RegisterElement(automaticNextDelayField);
            RegisterElement(automaticNextDelayField.labelElement);

            dialogueField = this.Q<TextField>("DialogueField");
            RegisterElement(dialogueField);

#if UNITY_6000
            dialogueField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#endif

            voiceLineField = this.Q<ObjectField>("VoiceLineField");
            RegisterElement(voiceLineField);

            characterButton = this.Q<Button>("CharacterButton");
            RegisterElement(characterButton);

            emotionButton = this.Q<Button>("EmotionButton");
            RegisterElement(emotionButton);

            setAsStartingNodeButton = this.Q<Button>("SetAsStartingNodeButton");
            RegisterElement(setAsStartingNodeButton);

            useTimeoutField = this.Q<Toggle>("UseTimeoutField");
            RegisterElement(useTimeoutField);

            automaticChoiceLabel = this.Q<Label>("AutomaticChoiceLabel");
            timeoutField = this.Q<FloatField>("TimeoutField");
            RegisterElement(timeoutField);
            RegisterElement(timeoutField.labelElement);

            timeoutChoiceIsRandomField = this.Q<Toggle>("TimeoutChoiceIsRandomField");
            RegisterElement(timeoutChoiceIsRandomField);

            timeoutChoiceIndexField = this.Q<IntegerField>("TimeoutChoiceIndexField");
            RegisterElement(timeoutChoiceIndexField);
            RegisterElement(timeoutChoiceIndexField.labelElement);

            addChoiceButton = this.Q<Button>("AddChoiceButton");
            RegisterElement(addChoiceButton);

            delayField = this.Q<FloatField>("DelayField");
            RegisterElement(delayField);
            RegisterElement(delayField.labelElement);

            hideDialogueBoxField = this.Q<Toggle>("HideDialogueBoxField");
            RegisterElement(hideDialogueBoxField);

            soundField = this.Q<ObjectField>("SoundField");
            RegisterElement(soundField);

            var scrollbars = this.Query<Scroller>().ToList();

            foreach (var element in scrollbars)
            {
                RegisterElement(element);
            }

            characterButton.clicked += () =>
            {
                if (!graphView.graph || dialogueNode == null)
                {
                    return;
                }

                EditorWindow.GetWindow<FeelSpeakObjectFinder>().LookForCharacter(x =>
                {
                    dialogueNode.character = x;
                    Refresh();
                    graphView.RefreshNode(dialogueNode);
                    graphView.Changed();
                }, true);
            };

            emotionButton.clicked += () =>
            {
                if (!graphView.graph || dialogueNode == null)
                {
                    return;
                }

                EditorWindow.GetWindow<FeelSpeakObjectFinder>().LookForEmotion(x =>
                {
                    dialogueNode.emotion = x;
                    Refresh();
                    graphView.RefreshNode(dialogueNode);
                    graphView.Changed();
                }, true);
            };

            setAsStartingNodeButton.clicked += () =>
            {
                if (!graphView.graph)
                {
                    return;
                }

                Node node = null;

                if (dialogueNode != null)
                {
                    node = dialogueNode;
                }
                else if (choiceNode != null)
                {
                    node = choiceNode;
                }
                else if (delayNode != null)
                {
                    node = delayNode;
                }
                else if (soundNode != null)
                {
                    node = soundNode;
                }

                graphView.SetStartingNode(node);
                Refresh();
                graphView.Changed();
            };

            dialogueField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || dialogueNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.RefreshNode(dialogueNode);
                graphView.Changed();
            });

            voiceLineField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || dialogueNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.RefreshNode(dialogueNode);
                graphView.Changed();
            });

            useTimeoutField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || choiceNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.Changed();
                Refresh();
            });

            timeoutField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || choiceNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                if (x.newValue < 0)
                {
                    timeoutField.value = 0;
                }

                graphView.Changed();
            });

            timeoutChoiceIsRandomField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || choiceNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.Changed();
                Refresh();
            });

            timeoutChoiceIndexField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || choiceNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.Changed();
            });

            addChoiceButton.clicked += () =>
            {
                if (!graphView.graph || choiceNode == null)
                {
                    return;
                }

                choiceNode.AddChoice();
                graphView.RefreshNode(choiceNode);
                graphView.Changed();
                Refresh();
            };

            delayField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || delayNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                if (x.newValue < 0)
                {
                    delayField.value = 0;
                }

                graphView.RefreshNode(delayNode);
                graphView.Changed();
            });

            automaticNextDelayField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || dialogueNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                if (x.newValue < 0)
                {
                    automaticNextDelayField.value = 0;
                }

                graphView.Changed();
            });

            suppressEmotionAnimationField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || dialogueNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.Changed();
            });

            suppressEmotionSoundField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || dialogueNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.Changed();
            });

            automaticNextOnCompleteField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || dialogueNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                graphView.Changed();
                Refresh();
            });

            timeoutChoiceIndexField.RegisterValueChangedCallback(x =>
            {
                if (!graphView.graph || choiceNode == null || !enableValueChangedCallbacks)
                {
                    return;
                }

                if (x.newValue < 0)
                {
                    timeoutChoiceIndexField.value = 0;
                }
                else if (choiceNode.choices.Count > 0 && x.newValue > choiceNode.choices.Count - 1)
                {
                    timeoutChoiceIndexField.value = choiceNode.choices.Count - 1;
                }
            });

            SetNone();
        }

        public int GetSelectedNodeId()
        {
            if (dialogueNode != null)
            {
                return dialogueNode.id;
            }
            else if (choiceNode != null)
            {
                return choiceNode.id;
            }
            else if (delayNode != null)
            {
                return delayNode.id;
            }
            else if (soundNode != null)
            {
                return soundNode.id;
            }

            return -1;
        }

        private void ValueChangePrevention()
        {
            enableValueChangedCallbacks = false;

            schedule.Execute(() =>
            {
                enableValueChangedCallbacks = true;
            });
        }

        public void SetNone()
        {
            dialogueNode = null;
            choiceNode = null;
            delayNode = null;
            soundNode = null;

            Refresh();
        }

        public void SetTarget(DialogueNode dialogueNode)
        {
            this.dialogueNode = dialogueNode;
            choiceNode = null;
            delayNode = null;
            soundNode = null;

            var nodeIndex = -1;

            for (int i = 0; i < graphView.graph.dialogueNodes.Count; i++)
            {
                var node = graphView.graph.dialogueNodes[i];

                if (node == dialogueNode)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex == -1)
            {
                FeelSpeakLogger.LogError("Dialogue Editor: node index not found!");
                return;
            }

            var serializedObject = new SerializedObject(graphView.graph);
            var nodeProperty = serializedObject.FindProperty("dialogueNodes").GetArrayElementAtIndex(nodeIndex);

            suppressEmotionAnimationField.BindProperty(nodeProperty.FindPropertyRelative("suppressEmotionAnimation"));
            suppressEmotionSoundField.BindProperty(nodeProperty.FindPropertyRelative("suppressEmotionSound"));
            automaticNextOnCompleteField.BindProperty(nodeProperty.FindPropertyRelative("automaticNextOnComplete"));
            automaticNextDelayField.BindProperty(nodeProperty.FindPropertyRelative("automaticNextDelay"));
            dialogueField.BindProperty(nodeProperty.FindPropertyRelative("dialogue"));
            voiceLineField.BindProperty(nodeProperty.FindPropertyRelative("voiceLine"));
            Refresh();
        }

        public void SetTarget(ChoiceNode choiceNode)
        {
            this.choiceNode = choiceNode;
            dialogueNode = null;
            delayNode = null;
            soundNode = null;

            var nodeIndex = -1;

            for (int i = 0; i < graphView.graph.choiceNodes.Count; i++)
            {
                var node = graphView.graph.choiceNodes[i];

                if (node == choiceNode)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex == -1)
            {
                FeelSpeakLogger.LogError("Dialogue Editor: node index not found!");
                return;
            }

            var serializedObject = new SerializedObject(graphView.graph);
            var nodeProperty = serializedObject.FindProperty("choiceNodes").GetArrayElementAtIndex(nodeIndex);

            useTimeoutField.SetValueWithoutNotify(choiceNode.useTimeout);
            timeoutChoiceIsRandomField.SetValueWithoutNotify(choiceNode.timeoutChoiceIsRandom);
            useTimeoutField.BindProperty(nodeProperty.FindPropertyRelative("useTimeout"));
            timeoutField.BindProperty(nodeProperty.FindPropertyRelative("timeout"));
            timeoutChoiceIsRandomField.BindProperty(nodeProperty.FindPropertyRelative("timeoutChoiceIsRandom"));
            timeoutChoiceIndexField.BindProperty(nodeProperty.FindPropertyRelative("timeoutChoiceIndex"));

            Refresh();
        }

        public void SetTarget(DelayNode delayNode)
        {
            this.delayNode = delayNode;
            choiceNode = null;
            dialogueNode = null;
            soundNode = null;

            var nodeIndex = -1;

            for (int i = 0; i < graphView.graph.delayNodes.Count; i++)
            {
                var node = graphView.graph.delayNodes[i];

                if (node == delayNode)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex == -1)
            {
                FeelSpeakLogger.LogError("Dialogue Editor: node index not found!");
                return;
            }

            var serializedObject = new SerializedObject(graphView.graph);
            var nodeProperty = serializedObject.FindProperty("delayNodes").GetArrayElementAtIndex(nodeIndex);
            delayField.BindProperty(nodeProperty.FindPropertyRelative("delay"));
            hideDialogueBoxField.BindProperty(nodeProperty.FindPropertyRelative("hideDialogueBox"));

            Refresh();
        }

        public void SetTarget(SoundNode soundNode)
        {
            this.soundNode = soundNode;
            choiceNode = null;
            dialogueNode = null;
            delayNode = null;

            var nodeIndex = -1;

            for (int i = 0; i < graphView.graph.soundNodes.Count; i++)
            {
                var node = graphView.graph.soundNodes[i];

                if (node == soundNode)
                {
                    nodeIndex = i;
                    break;
                }
            }

            if (nodeIndex == -1)
            {
                FeelSpeakLogger.LogError("Dialogue Editor: node index not found!");
                return;
            }

            var serializedObject = new SerializedObject(graphView.graph);
            var nodeProperty = serializedObject.FindProperty("soundNodes").GetArrayElementAtIndex(nodeIndex);
            soundField.BindProperty(nodeProperty.FindPropertyRelative("sound"));

            Refresh();
        }
        public void Refresh()
        {
            if (!graphView.graph)
            {
                return;
            }

            ValueChangePrevention();

            dialogueTab.style.display = DisplayStyle.None;
            choiceTab.style.display = DisplayStyle.None;
            delayTab.style.display = DisplayStyle.None;
            soundTab.style.display = DisplayStyle.None;
            setAsStartingNodeButton.style.display = DisplayStyle.None;

            if (dialogueNode != null)
            {
                var defaultCharacterImage = AssetSearch.FindAsset<Texture2D>("FeelSpeak", "character_icon");
                var defaultEmotionImage = AssetSearch.FindAsset<Texture2D>("FeelSpeak", "emotion_icon");

                if (dialogueNode.character)
                {
                    if (dialogueNode.character.sprite)
                    {
                        characterImage.style.backgroundImage = new StyleBackground(dialogueNode.character.sprite);
                    }
                    else
                    {
                        characterImage.style.backgroundImage = defaultCharacterImage;
                    }

                    characterNameLabel.text = dialogueNode.character.characterName;
                }
                else
                {
                    characterImage.style.backgroundImage = defaultCharacterImage;
                    characterNameLabel.text = "No Character Set";
                }

                if (dialogueNode.emotion)
                {
                    suppressEmotionAnimationField.style.display = DisplayStyle.Flex;
                    suppressEmotionSoundField.style.display = DisplayStyle.Flex;

                    if (dialogueNode.emotion.sprite)
                    {
                        emotionImage.style.backgroundImage = new StyleBackground(dialogueNode.emotion.sprite);
                    }
                    else
                    {
                        emotionImage.style.backgroundImage = defaultEmotionImage;
                    }

                    emotionNameLabel.text = dialogueNode.emotion.emotionName;
                }
                else
                {
                    suppressEmotionSoundField.style.display = DisplayStyle.None;
                    suppressEmotionAnimationField.style.display = DisplayStyle.None;
                    emotionImage.style.backgroundImage = defaultEmotionImage;
                    emotionNameLabel.text = "No Emotion Set";
                }

                voiceLineField.style.display = FeelSpeak.Settings.enableVoiceLines ? DisplayStyle.Flex : DisplayStyle.None;
                automaticNextDelayField.style.display = dialogueNode.automaticNextOnComplete ? DisplayStyle.Flex : DisplayStyle.None;
                dialogueTab.style.display = DisplayStyle.Flex;
                setAsStartingNodeButton.style.display = graphView.graph.startingNodeId == dialogueNode.id ? DisplayStyle.None : DisplayStyle.Flex;
            }
            else if (choiceNode != null)
            {
                if (!choiceNode.useTimeout)
                {
                    timeoutField.style.display = DisplayStyle.None;
                    automaticChoiceLabel.style.display = DisplayStyle.None;
                    timeoutChoiceIsRandomField.style.display = DisplayStyle.None;
                    timeoutChoiceIndexField.style.display = DisplayStyle.None;
                }
                else
                {
                    timeoutField.style.display = DisplayStyle.Flex;
                    automaticChoiceLabel.style.display = DisplayStyle.Flex;
                    timeoutChoiceIsRandomField.style.display = DisplayStyle.Flex;

                    if (!choiceNode.timeoutChoiceIsRandom)
                    {
                        timeoutChoiceIndexField.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        timeoutChoiceIndexField.style.display = DisplayStyle.None;
                    }
                }

                choiceContent.Clear();

                var nodeIndex = -1;

                for (int i = 0; i < graphView.graph.choiceNodes.Count; i++)
                {
                    var node = graphView.graph.choiceNodes[i];

                    if (node == choiceNode)
                    {
                        nodeIndex = i;
                        break;
                    }
                }

                if (nodeIndex == -1)
                {
                    FeelSpeakLogger.LogError("Dialogue Editor: node index not found!");
                    return;
                }

                var serializedObject = new SerializedObject(graphView.graph);
                var nodeProperty = serializedObject.FindProperty("choiceNodes").GetArrayElementAtIndex(nodeIndex);
                var choicesProperty = nodeProperty.FindPropertyRelative("choices");

                for (int i = 0; i < choiceNode.choices.Count; i++)
                {
                    var choiceElement = new ChoiceElement();
                    int index = i;
                    choiceElement.removeButton.clicked += () => RemoveChoice(index);
                    choiceElement.arrowUpButton.clicked += () => MoveChoiceUp(index);
                    choiceElement.arrowDownButton.clicked += () => MoveChoiceDown(index);
                    choiceElement.arrowUpButton.SetEnabled(i != 0);
                    choiceElement.arrowDownButton.SetEnabled(i != choiceNode.choices.Count - 1);
                    var choiceProperty = choicesProperty.GetArrayElementAtIndex(i);
                    choiceElement.BindProperty(choiceProperty, index);

                    choiceElement.nameField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        if (!graphView.graph || choiceNode == null)
                        {
                            return;
                        }

                        graphView.RefreshNode(choiceNode);
                        UpdateChoiceName(index);
                        graphView.Changed();
                    });

                    choiceElement.textField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        if (!graphView.graph || choiceNode == null)
                        {
                            return;
                        }

                        graphView.RefreshNode(choiceNode);
                        graphView.Changed();
                    });

                    RegisterElement(choiceElement);
                    choiceContent.Add(choiceElement);
                }

                choiceTab.style.display = DisplayStyle.Flex;
                setAsStartingNodeButton.style.display = graphView.graph.startingNodeId == choiceNode.id ? DisplayStyle.None : DisplayStyle.Flex;
            }
            else if (delayNode != null)
            {
                delayTab.style.display = DisplayStyle.Flex;
                setAsStartingNodeButton.style.display = graphView.graph.startingNodeId == delayNode.id ? DisplayStyle.None : DisplayStyle.Flex;
            }
            else if (soundNode != null)
            {
                soundTab.style.display = DisplayStyle.Flex;
                setAsStartingNodeButton.style.display = graphView.graph.startingNodeId == soundNode.id ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private void UpdateChoiceName(int index)
        {
            if (!graphView.graph || choiceNode == null)
            {
                return;
            }

            graphView.GetNodeElement<ChoiceNodeEditor>(choiceNode).UpdateChoiceName(index);
        }

        private void RemoveChoice(int index)
        {
            if (!graphView.graph || choiceNode == null)
            {
                return;
            }

            graphView.GetNodeElement<ChoiceNodeEditor>(choiceNode).RemoveChoice(index);
            graphView.Changed();
            Refresh();
        }

        private void MoveChoiceUp(int index)
        {
            if (!graphView.graph || choiceNode == null)
            {
                return;
            }

            graphView.GetNodeElement<ChoiceNodeEditor>(choiceNode).MoveChoiceUp(index);
            graphView.Changed();
            Refresh();
        }

        private void MoveChoiceDown(int index)
        {
            if (!graphView.graph || choiceNode == null)
            {
                return;
            }

            graphView.GetNodeElement<ChoiceNodeEditor>(choiceNode).MoveChoiceDown(index);
            graphView.Changed();
            Refresh();
        }
    }
}
#endif