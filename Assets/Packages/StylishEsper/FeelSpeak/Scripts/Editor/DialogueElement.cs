//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Graph;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
#if !UNITY_2022
    [UxmlElement]
    public partial class DialogueElement : Button
    {
#elif UNITY_2022
    public class DialogueElement : Button
    {
        public new class UxmlFactory : UxmlFactory<DialogueElement, UxmlTraits> { }
#endif
        public DialogueGraph target;

        public DialogueElement() 
        {
            AddToClassList("dialogue");
        }

        public DialogueElement(DialogueGraph dialogueGraph)
        {
            target = dialogueGraph;
            AddToClassList("dialogue");
        }

        public void Refresh()
        {
            if (!target)
            {
                text = "<NULL>";
            }
            else
            {
                text = target.graphName;
            }
        }
    }
}
#endif