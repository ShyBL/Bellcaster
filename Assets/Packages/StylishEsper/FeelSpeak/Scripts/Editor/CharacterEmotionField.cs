//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class CharacterEmotionField : VisualElement
    {
        public Label title;
        public VisualElement content;
        public ObjectField spriteField;
        public TextField animationNameOverrideField;

        public CharacterEmotionField()
        {
            title = new Label();
            title.AddToClassList("characterEmotionTitle");
            Add(title);

            content = new VisualElement();
            content.AddToClassList("characterEmotionContent");
            Add(content);

            spriteField = new ObjectField("Sprite");
            spriteField.objectType = typeof(Sprite);
            spriteField.AddToClassList("field");
            content.Add(spriteField);

            animationNameOverrideField = new TextField("Animation Name Override");
            animationNameOverrideField.AddToClassList("field");
            content.Add(animationNameOverrideField);
        }

        public void BindProperty(string label, SerializedProperty serializedProperty)
        {
            title.text = label;
            title.style.display = string.IsNullOrEmpty(label) ? DisplayStyle.None : DisplayStyle.Flex;
            spriteField.BindProperty(serializedProperty.FindPropertyRelative("sprite"));
            animationNameOverrideField.BindProperty(serializedProperty.FindPropertyRelative("animationNameOverride"));
        }
    }
}
#endif