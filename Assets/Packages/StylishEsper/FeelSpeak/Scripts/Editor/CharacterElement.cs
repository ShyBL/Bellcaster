//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class CharacterElement : Button
    {
#elif UNITY_2022
    public class CharacterElement : Button
    {
        public new class UxmlFactory : UxmlFactory<CharacterElement, UxmlTraits> { }
#endif
        public Character target;
        public Label label;

        public CharacterElement()
        {
            AddToClassList("character");

            label = new Label();
            label.AddToClassList("characterLabel");
            Add(label);
        }

        public CharacterElement(Character character)
        {
            target = character;
            AddToClassList("character");

            label = new Label();
            label.AddToClassList("characterLabel");
            Add(label);
        }

        public void Refresh()
        {
            if (!target)
            {
                style.backgroundImage = StyleKeyword.Null;
                AddToClassList("characterDefault");
                label.text = "<NULL>";
            }
            else
            {
                if (target.sprite)
                {
                    RemoveFromClassList("characterDefault");
                    style.backgroundImage = new StyleBackground(target.sprite);
                }
                else
                {
                    style.backgroundImage = StyleKeyword.Null;
                    AddToClassList("characterDefault");
                }

                label.text = target.characterName;
            }
        }
    }
}
#endif