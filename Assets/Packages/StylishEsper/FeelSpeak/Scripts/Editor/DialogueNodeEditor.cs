//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class DialogueNodeEditor : GraphNodeEditor
    {
        private Color portColor = new Color(0.5372549f, 0.7764707f, 0.6470588f, 1);

        private VisualElement characterImage;
        private VisualElement emotionImage;

        private static Texture2D noCharacterImage;
        private static Texture2D noEmotionImage;

        public DialogueNode Value { get => value as DialogueNode; }

        public DialogueNodeEditor(DialogueNode node) : base(node)
        {
            characterImage = new VisualElement();
            characterImage.style.width = 100;
            characterImage.style.height = 100;
            mainContainer.Add(characterImage);

            emotionImage = new VisualElement();
            emotionImage.AddToClassList("emotionImage");
            emotionImage.pickingMode = PickingMode.Ignore;
            mainContainer.Add(emotionImage);
        }

        public override void Refresh()
        {
            var node = Value;

            if (node == null)
            {
                title = "Missing";
                return;
            }

            title = "Dialogue";

            if (node.character)
            {
                characterImage.style.display = DisplayStyle.Flex;

                if (!node.character.sprite)
                {
                    if (!noCharacterImage)
                    {
                        noCharacterImage = AssetSearch.FindAsset<Texture2D>("FeelSpeak", "character_icon");
                    }

                    characterImage.style.backgroundImage = noCharacterImage;
                }
                else
                {
                    characterImage.style.backgroundImage = new StyleBackground(node.character.GetSprite(node.emotion));
                }
            }
            else
            {
                characterImage.style.display = DisplayStyle.None;
            }

            if (node.emotion)
            {
                emotionImage.style.display = DisplayStyle.Flex;

                if (!node.emotion.sprite)
                {
                    if (!noCharacterImage)
                    {
                        noEmotionImage = AssetSearch.FindAsset<Texture2D>("FeelSpeak", "emotion_icon");
                    }

                    emotionImage.style.backgroundImage = noEmotionImage;
                }
                else
                {
                    emotionImage.style.backgroundImage = new StyleBackground(node.emotion.sprite);
                }
            }
            else
            {
                emotionImage.style.display = DisplayStyle.None;
            }

            UpdateState();
        }

        protected override void CreateInputPorts()
        {
            var input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portColor = portColor;
            input.portName = string.Empty;
            inputContainer.Add(input);
            inputPorts.Add(input);
        }

        protected override void CreateOutputPorts()
        {
            var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portColor = portColor;
            output.portName = string.Empty;
            outputContainer.Add(output);
            outputPorts.Add(output);
        }
    }
}
#endif