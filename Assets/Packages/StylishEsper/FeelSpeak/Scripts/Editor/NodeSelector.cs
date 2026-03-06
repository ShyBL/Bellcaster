//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class NodeSelector
    {
        public Toolbar root;
        public Button dialogueButton;
        public Button choiceButton;
        public Button delayButton;
        public Button soundButton;
        public Button actionButton;
        public Button booleanButton;
        public Button nestButton;

        public NodeSelector(Toolbar root, DialogueGraphView graphView) 
        {
            this.root = root;
            dialogueButton = root.Q<Button>("DialogueButton");
            choiceButton = root.Q<Button>("ChoiceButton");
            delayButton = root.Q<Button>("DelayButton");
            soundButton = root.Q<Button>("SoundButton");
            actionButton = root.Q<Button>("ActionButton");
            booleanButton = root.Q<Button>("BooleanButton");
            nestButton = root.Q<Button>("NestButton");

            dialogueButton.clicked += () =>
            {
                graphView.CreateNewDialogueNode();
                Close();
            };

            choiceButton.clicked += () =>
            {
                graphView.CreateNewChoiceNode();
                Close();
            };

            delayButton.clicked += () =>
            {
                graphView.CreateNewDelayNode();
                Close();
            };

            soundButton.clicked += () =>
            {
                graphView.CreateNewSoundNode();
                Close();
            };

            root.RegisterCallback<GeometryChangedEvent>(ClampOnScreen);
        }

        public void ClampOnScreen(GeometryChangedEvent evt)
        {
            var position = new Vector2(root.resolvedStyle.left, root.resolvedStyle.top);

            if (position.x + root.resolvedStyle.width > root.parent.resolvedStyle.width)
            {
                position.x = root.parent.resolvedStyle.width - root.resolvedStyle.width;
            }
            else if (position.x < 0)
            {
                position.x = 0;
            }

            if (position.y + root.resolvedStyle.height > root.parent.resolvedStyle.height)
            {
                position.y = root.parent.resolvedStyle.height - root.resolvedStyle.height;
            }
            else if (position.y < 0)
            {
                position.y = 0;
            }

            root.style.left = position.x;
            root.style.top = position.y;
        }

        public void Open(Vector2 position)
        {
            SetPosition(position);
            root.style.display = DisplayStyle.Flex;
        }

        public void Close()
        {
            root.style.display = DisplayStyle.None;
        }

        public void SetPosition(Vector2 position)
        {
            root.style.left = position.x;
            root.style.top = position.y;
        }
    }
}
#endif