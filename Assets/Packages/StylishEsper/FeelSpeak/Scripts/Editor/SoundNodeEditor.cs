//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Esper.FeelSpeak.Editor
{
    public class SoundNodeEditor : GraphNodeEditor
    {
        private Color portColor = new Color(0.7764707f, 0.2705882f, 0.3882353f, 1f);

        public SoundNode Value { get => value as SoundNode; }

        public SoundNodeEditor(SoundNode node) : base(node)
        {

        }

        public override void Refresh()
        {
            var node = Value;

            if (node == null)
            {
                title = "Missing";
                return;
            }

            title = "Sound";

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