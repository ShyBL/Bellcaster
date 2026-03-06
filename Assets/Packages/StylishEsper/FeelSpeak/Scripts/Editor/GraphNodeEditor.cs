//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public abstract class GraphNodeEditor : Node
    {
        public Graph.Node value;

        public List<Port> inputPorts = new();
        public List<Port> outputPorts = new();

        public VisualElement border;
        public VisualElement startIcon;
        public DialogueGraphView graphView;
        protected Label titleLabel;
        protected Color originalBorderColor;

        public int Id { get => value.id; }

        public Vector2 Position { get => value.position; }

        public GraphNodeEditor(Graph.Node value)
        {
            m_CollapseButton.style.display = DisplayStyle.None;
            border = this.Q<VisualElement>("node-border");
            border.style.overflow = Overflow.Visible;
            border.style.borderTopLeftRadius = 0f;
            border.style.borderTopRightRadius = 0f;
            border.style.borderBottomLeftRadius = 0f;
            border.style.borderBottomRightRadius = 0f;
            titleLabel = this.Q<Label>("title-label");
            titleLabel.style.textOverflow = TextOverflow.Ellipsis;
            titleLabel.style.overflow = Overflow.Hidden;
            style.minWidth = 75;
            originalBorderColor = new Color(0.09803922f, 0.09803922f, 0.09803922f, 1);
            this.value = value;
            style.left = value.position.x;
            style.top = value.position.y;

            startIcon = new VisualElement();
            startIcon.AddToClassList("start");
            startIcon.pickingMode = PickingMode.Ignore;
            mainContainer.Add(startIcon);

            inputContainer.style.minWidth = 35;
            outputContainer.style.minWidth = 35;

            CreateInputPorts();
            CreateOutputPorts();
        }

        public virtual void UpdateCompleteState()
        {
            if (value.IsComplete())
            {
                border.style.borderTopColor = originalBorderColor;
                border.style.borderBottomColor = originalBorderColor;
                border.style.borderLeftColor = originalBorderColor;
                border.style.borderRightColor = originalBorderColor;
            }
            else
            {
                border.style.borderTopColor = Color.red;
                border.style.borderBottomColor = Color.red;
                border.style.borderLeftColor = Color.red;
                border.style.borderRightColor = Color.red;
            }
        }

        public bool IsStartingNode()
        {
            return value.id == graphView.graph.startingNodeId;
        }

        public void UpdateState()
        {
            bool isStartingNode = IsStartingNode();

            if (isStartingNode)
            {
                startIcon.style.display = DisplayStyle.Flex;
            }
            else
            {
                startIcon.style.display = DisplayStyle.None;

                if (inputPorts.Count == 0)
                {
                    CreateInputPorts();
                }
            }

            UpdateCompleteState();
        }

        public abstract void Refresh();

        protected abstract void CreateInputPorts();

        protected abstract void CreateOutputPorts();

        public void SetPosition(Vector2 position)
        {
            value.position = position;
            style.left = position.x;
            style.top = position.y;
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (graphView.GetSelectedNodeCount() > 1)
            {
                graphView.nodeInspector.SetNone();
                return;
            }

            graphView.SelectNode(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
        }
    }
}
#endif