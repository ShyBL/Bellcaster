//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class DraggableEditorElement : ToolbarButton
    {
#elif UNITY_2022
    public class DraggableEditorElement : ToolbarButton
    {
        public new class UxmlFactory : UxmlFactory<DraggableEditorElement, UxmlTraits> { }
#endif
        public VisualElement content;
        public Button resizeButton;
        protected Vector2 startingPosition;
        protected Vector2 startingMousePosition;
        protected Vector2 originalSize;
        protected bool isDragging = false;
        protected bool isMinimized = false;
        protected bool isReady = false;

        public override VisualElement contentContainer 
        {
            get
            {
                if (!isReady)
                {
                    return this;
                }
                else
                {
                    return content;
                }    
            }
        }

        public DraggableEditorElement()
        {
            style.position = Position.Absolute;
            style.left = StyleKeyword.Auto;
            style.marginTop = 0;
            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.paddingTop = 4;
            style.paddingBottom = 4;
            style.paddingLeft = 4;
            style.paddingRight = 4;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopLeftRadius = 0;
            style.borderTopRightRadius = 0;
            style.borderBottomLeftRadius = 0;
            style.borderBottomRightRadius = 0;

            var top = new VisualElement();
            top.AddToClassList("minimizeBar");
            top.style.flexGrow = 0;
            Add(top);

            resizeButton = new Button();
            resizeButton.text = "-";
            resizeButton.AddToClassList("minimizeButton");
            resizeButton.clicked += ToggleResize;
            top.Add(resizeButton);

            content = new VisualElement();
            content.style.flexGrow = 1;
            Add(content);

            RegisterElement(resizeButton);
            RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            RegisterCallback<GeometryChangedEvent>(GeometryChanged);

            isReady = true;
        }

        public void RegisterElement(VisualElement element)
        {
            element.RegisterCallback<PointerUpEvent>(x => ForceStopDrag(), TrickleDown.TrickleDown);
            element.RegisterCallback<PointerDownEvent>(x => ForceStopDrag(), TrickleDown.TrickleDown);
        }

        public void GeometryChanged(GeometryChangedEvent evt)
        {
            originalSize = worldBound.size;
            UnregisterCallback<GeometryChangedEvent>(GeometryChanged);
        }

        public void ClampOnScreen(GeometryChangedEvent evt)
        {
            var position = new Vector2(resolvedStyle.left, resolvedStyle.top);

            if (position.x + resolvedStyle.width > parent.resolvedStyle.width)
            {
                position.x = parent.resolvedStyle.width - resolvedStyle.width;
            }
            else if (position.x < 0)
            {
                position.x = 0;
            }

            if (position.y + resolvedStyle.height > parent.resolvedStyle.height)
            {
                position.y = parent.resolvedStyle.height - resolvedStyle.height;
            }
            else if (position.y < 0)
            {
                position.y = 0;
            }

            style.left = position.x;
            style.top = position.y;

            UnregisterCallback<GeometryChangedEvent>(ClampOnScreen);
        }

        public void ForceStopDrag()
        {
            isDragging = false;
        }

        private void ToggleResize()
        {
            RegisterCallback<GeometryChangedEvent>(ClampOnScreen);

            if (isMinimized)
            {
                isMinimized = false;
                style.width = new StyleLength(new Length(40, LengthUnit.Percent));
                style.height = new StyleLength(new Length(80, LengthUnit.Percent));
                content.style.display = DisplayStyle.Flex;
                resizeButton.text = "-";
            }
            else
            {
                isMinimized = true;
                style.width = 50;
                style.height = 50;
                content.style.display = DisplayStyle.None;
                resizeButton.text = "+";
            }

            ForceStopDrag();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                isDragging = true;
                startingMousePosition = evt.position;
                startingPosition = new Vector2(resolvedStyle.left, resolvedStyle.top);
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (isDragging)
            {
                var mousePosition = (Vector2)evt.position;
                var diff = mousePosition - startingMousePosition;
                var position = startingPosition + diff;

                if (position.x + resolvedStyle.width > parent.resolvedStyle.width)
                {
                    position.x = parent.resolvedStyle.width - resolvedStyle.width;
                }
                else if (position.x < 0)
                {
                    position.x = 0;
                }

                if (position.y + resolvedStyle.height > parent.resolvedStyle.height)
                {
                    position.y = parent.resolvedStyle.height - resolvedStyle.height;
                }
                else if (position.y < 0)
                {
                    position.y = 0;
                }

                style.left = position.x;
                style.top = position.y;
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                isDragging = false;
            }
        }
    }
}
#endif