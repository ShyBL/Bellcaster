//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class EmotionElement : Button
    {
#elif UNITY_2022
    public class EmotionElement : Button
    {
        public new class UxmlFactory : UxmlFactory<EmotionElement, UxmlTraits> { }
#endif
        public Emotion target;
        public Label label;

        public EmotionElement() 
        {
            AddToClassList("emotion");

            label = new Label();
            label.AddToClassList("emotionLabel");
            Add(label);
        }

        public EmotionElement(Emotion emotion)
        {
            target = emotion;
            AddToClassList("emotion");

            label = new Label();
            label.AddToClassList("emotionLabel");
            Add(label);
        }

        public void Refresh()
        {
            if (!target)
            {
                style.backgroundImage = StyleKeyword.Null;
                AddToClassList("emotionDefault");
                label.text = "<NULL>";
            }
            else
            {
                if (target.sprite)
                {
                    RemoveFromClassList("emotionDefault");
                    style.backgroundImage = new StyleBackground(target.sprite);
                }
                else
                {
                    style.backgroundImage = StyleKeyword.Null;
                    AddToClassList("emotionDefault");
                }

                label.text = target.emotionName;
            }
        }
    }
}
#endif