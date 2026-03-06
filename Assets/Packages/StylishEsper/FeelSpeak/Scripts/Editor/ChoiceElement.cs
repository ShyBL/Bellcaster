//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class ChoiceElement : VisualElement
    {
#endif
#if UNITY_2022
    public class ChoiceElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ChoiceElement, UxmlTraits> { }
#endif
        public TextField nameField;
        public TextField textField;
        public Label indexLabel;
        public Button removeButton;
        public Button arrowUpButton;
        public Button arrowDownButton;
        public int index;

        public ChoiceElement()
        {
            AddToClassList("choiceField");

            var actionsSection = new VisualElement();
            actionsSection.style.flexGrow = 0;
            Add(actionsSection);

            indexLabel = new Label();
            indexLabel.AddToClassList("choiceIndexLabel");
            actionsSection.Add(indexLabel);

            arrowUpButton = new Button();
            arrowUpButton.AddToClassList("arrowUpButton");
            actionsSection.Add(arrowUpButton);

            var buttonSpacer = new VisualElement();
            buttonSpacer.style.flexGrow = 0;
            buttonSpacer.style.height = 6;
            actionsSection.Add(buttonSpacer);

            arrowDownButton = new Button();
            arrowDownButton.AddToClassList("arrowDownButton");
            actionsSection.Add(arrowDownButton);

            var buttonSpacer2 = new VisualElement();
            buttonSpacer2.style.flexGrow = 0;
            buttonSpacer2.style.height = 6;
            actionsSection.Add(buttonSpacer2);

            removeButton = new Button();
            removeButton.AddToClassList("removeButton");
            actionsSection.Add(removeButton);

            var removeButtonIcon = new VisualElement();
            removeButtonIcon.AddToClassList("removeButtonIcon");
            removeButton.Add(removeButtonIcon);

            var choiceSection = new VisualElement();
            choiceSection.AddToClassList("choiceSection");
            Add(choiceSection);

            nameField = new TextField("Name");
            nameField.multiline = true;

#if UNITY_6000
            nameField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#endif

            nameField.AddToClassList("field");
            choiceSection.Add(nameField);

            textField = new TextField("Text");
            textField.multiline = true;

#if UNITY_6000
            textField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#endif

            textField.AddToClassList("field");
            choiceSection.Add(textField);

            Refresh();
        }

        public void BindProperty(SerializedProperty serializedProperty, int index)
        {
            this.index = index;
            nameField.BindProperty(serializedProperty.FindPropertyRelative("name"));
            textField.BindProperty(serializedProperty.FindPropertyRelative("text"));
            Refresh();
        }

        public void Refresh()
        {
            indexLabel.text = index.ToString();
        }
    }
}
#endif