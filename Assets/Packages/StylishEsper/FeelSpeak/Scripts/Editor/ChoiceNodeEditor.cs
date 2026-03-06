//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Esper.FeelSpeak.Editor
{
    public class ChoiceNodeEditor : GraphNodeEditor
    {
        private Color portColor = new Color(0.5647059f, 0.5686275f, 0.854902f, 1);

        public ChoiceNode Value { get => value as ChoiceNode; }

        public ChoiceNodeEditor(ChoiceNode node) : base(node)
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

            title = "Choice";

            UpdateState();
            CreateOutputPorts();
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
            int ports = outputPorts.Count == 0 ? 0 : outputPorts.Count;

            for (int i = ports; i < Value.choices.Count; i++)
            {
                var choice = Value.choices[i];
                var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                output.portColor = portColor;
                output.portName = choice.name;
                outputContainer.Add(output);
                outputPorts.Add(output);
            }
        }

        public void UpdateChoiceName(int index)
        {
            var port = outputPorts[index];
            port.portName = Value.choices[index].name;
        }

        public void RemoveChoice(int index)
        {
            var port = outputPorts[index];

            var connections = new List<Edge>(port.connections);

            foreach (var edge in connections)
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
            }

            graphView.DeleteElements(connections);

            outputContainer.RemoveAt(index);
            outputPorts.RemoveAt(index);
            Value.RemoveChoice(index);
        }

        public void MoveChoiceUp(int index)
        {
            if (index <= 0 || index >= outputPorts.Count)
            {
                return;
            }

            int up = index - 1;

            if (outputPorts[up].connections.Count() > 0)
            {
                var edge = outputPorts[up].connections.First();
                int outputNodeId = (edge.output.node as GraphNodeEditor).Id;
                int inputNodeId = (edge.input.node as GraphNodeEditor).Id;
                int outputPortIndex = up;
                int inputPortIndex = 0;
                var connection = graphView.GetConnection(outputNodeId, inputNodeId, outputPortIndex, inputPortIndex);

                if (connection != null)
                {
                    connection.outputPortIndex = index;
                }
            }

            if (outputPorts[index].connections.Count() > 0)
            {
                var edge = outputPorts[index].connections.First();
                int outputNodeId = (edge.output.node as GraphNodeEditor).Id;
                int inputNodeId = (edge.input.node as GraphNodeEditor).Id;
                int outputPortIndex = index;
                int inputPortIndex = 0;
                var connection = graphView.GetConnection(outputNodeId, inputNodeId, outputPortIndex, inputPortIndex);

                if (connection != null)
                {
                    connection.outputPortIndex = up;
                }
            }

            var tempPort = outputPorts[up];
            outputPorts[up] = outputPorts[index];
            outputPorts[index] = tempPort;

            var upperElement = outputContainer[up];
            outputContainer.RemoveAt(up);
            outputContainer.Insert(index, upperElement);
            Value.MoveChoiceUp(index);
        }

        public void MoveChoiceDown(int index)
        {
            if (index < 0 || index >= outputPorts.Count - 1)
            {
                return;
            }

            int down = index + 1;

            if (outputPorts[down].connections.Count() > 0)
            {
                var edge = outputPorts[down].connections.First();
                int outputNodeId = (edge.output.node as GraphNodeEditor).Id;
                int inputNodeId = (edge.input.node as GraphNodeEditor).Id;
                int outputPortIndex = down;
                int inputPortIndex = 0;
                var connection = graphView.GetConnection(outputNodeId, inputNodeId, outputPortIndex, inputPortIndex);

                if (connection != null)
                {
                    connection.outputPortIndex = index;
                }
            }

            if (outputPorts[index].connections.Count() > 0)
            {
                var edge = outputPorts[index].connections.First();
                int outputNodeId = (edge.output.node as GraphNodeEditor).Id;
                int inputNodeId = (edge.input.node as GraphNodeEditor).Id;
                int outputPortIndex = index;
                int inputPortIndex = 0;
                var connection = graphView.GetConnection(outputNodeId, inputNodeId, outputPortIndex, inputPortIndex);

                if (connection != null)
                {
                    connection.outputPortIndex = down;
                }
            }

            var tempPort = outputPorts[down];
            outputPorts[down] = outputPorts[index];
            outputPorts[index] = tempPort;

            var lowerElement = outputContainer[down];
            outputContainer.RemoveAt(down);
            outputContainer.Insert(index, lowerElement);
            Value.MoveChoiceDown(index);
        }
    }
}
#endif