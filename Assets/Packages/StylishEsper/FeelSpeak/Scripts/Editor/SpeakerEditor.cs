//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Overworld;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    [CustomEditor(typeof(Speaker))]
    public class SpeakerEditor : UnityEditor.Editor
    {
        private CharacterElement speakerElement;
        private DialogueElement dialogueElement;
        private ObjectField voiceAudioSourceField;
        private ObjectField inRangeIndicatorField;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "SpeakerEditor");
            visualTree.CloneTree(inspector);

            var target = this.target as Speaker;

            speakerElement = inspector.Q<CharacterElement>();
            speakerElement.target = target.character;
            speakerElement.Refresh();

            speakerElement.clicked += () =>
            {
                EditorWindow.GetWindow<FeelSpeakObjectFinder>().LookForCharacter(character =>
                {
                    target.character = character;
                    speakerElement.target = character;
                    EditorUtility.SetDirty(target);
                    speakerElement.Refresh();
                }, true);
            };

            dialogueElement = inspector.Q<DialogueElement>();
            dialogueElement.target = target.dialogue;
            dialogueElement.Refresh();

            dialogueElement.clicked += () =>
            {
                EditorWindow.GetWindow<FeelSpeakObjectFinder>().LookForDialogueGraph(dialogue =>
                {
                    target.dialogue = dialogue;
                    dialogueElement.target = dialogue;
                    EditorUtility.SetDirty(target);
                    dialogueElement.Refresh();
                }, true);
            };

            voiceAudioSourceField = inspector.Q<ObjectField>("VoiceAudioSourceField");
            voiceAudioSourceField.BindProperty(serializedObject.FindProperty("voiceAudioSource"));

            inRangeIndicatorField = inspector.Q<ObjectField>("InRangeIndicatorField");
            inRangeIndicatorField.BindProperty(serializedObject.FindProperty("inRangeIndicator"));

            return inspector;
        }
    }
}
#endif