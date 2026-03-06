//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class DelayNodeEditor : GraphNodeEditor
    {
        private Color portColor = new Color(0.7764707f, 0.4117647f, 0.2784314f, 1);

        private VisualElement delayIcon;
        private Label delayLabel;

        public DelayNode Value { get => value as DelayNode; }

        public DelayNodeEditor(DelayNode node) : base(node)
        {
            delayIcon = new VisualElement();
            delayIcon.AddToClassList("delayIcon");
            delayIcon.pickingMode = PickingMode.Ignore;
            mainContainer.Add(delayIcon);

            delayLabel = new Label("0");
            delayLabel.AddToClassList("delayLabel");
            delayLabel.style.fontSize = 7;
            delayIcon.Add(delayLabel);
        }

        public override void Refresh()
        {
            var node = Value;

            if (node == null)
            {
                title = "Missing";
                return;
            }

            title = "Delay";

            if (node.delay < 99.99f)
            {
                delayLabel.text = node.delay.ToString("0.##");
            }
            else if (node.delay > 99.99 && node.delay < 9999)
            {
                delayLabel.text = node.delay.ToString("F0");
            }
            else
            {
                delayLabel.text = node.delay.ToString("0.##");
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