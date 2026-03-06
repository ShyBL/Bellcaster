//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Overworld;
using Esper.FeelSpeak.UI.UGUI;
using UnityEditor;
using UnityEngine;

namespace Esper.FeelSpeak.Editor
{
    public static class FeelSpeakEditorMenuItems
    {
        [MenuItem("GameObject/Feel Speak/Initializer", priority = 0)]
        private static void AddInitializer()
        {
            var obj = AssetSearch.FindFirstInScene<FeelSpeakInitializer>();

            if (!obj)
            {
                obj = ObjectFactory.CreateGameObject("FeelSpeakInitializer", typeof(FeelSpeakInitializer)).GetComponent<FeelSpeakInitializer>();

                if (Selection.activeGameObject)
                {
                    obj.transform.SetParent(Selection.activeGameObject.transform, false);
                }

                Undo.RegisterCreatedObjectUndo(obj, "Undo Add Feel Speak Initializer");
            }

            Selection.activeGameObject = obj.gameObject;
        }

        [MenuItem("GameObject/Feel Speak/UI/Dialogue Box", priority = 1)]
        private static void AddDialogueBox()
        {
            var existing = AssetSearch.FindFirstInScene<DialogueBoxUGUI>();

            if (existing)
            {
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var obj = Object.Instantiate(AssetSearch.FindAsset<GameObject>("FeelSpeak", "DialogueBox").GetComponent<DialogueBoxUGUI>(), Selection.activeTransform);
            obj.name = "DialogueBox";

            var canvas = obj.GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Add Dialogue Box");
            Selection.activeGameObject = obj.gameObject;
        }

        [MenuItem("GameObject/Feel Speak/UI/Choices List", priority = 2)]
        private static void AddChoicesList()
        {
            var existing = AssetSearch.FindFirstInScene<ChoicesListUGUI>();

            if (existing)
            {
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var obj = Object.Instantiate(AssetSearch.FindAsset<GameObject>("FeelSpeak", "ChoicesList").GetComponent<ChoicesListUGUI>(), Selection.activeTransform);
            obj.name = "ChoicesList";

            var canvas = obj.GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Add Choices List");
            Selection.activeGameObject = obj.gameObject;
        }

        [MenuItem("GameObject/Feel Speak/UI/Choice Timer", priority = 3)]
        private static void AddChoiceTimer()
        {
            var existing = AssetSearch.FindFirstInScene<ChoiceTimerUGUI>();

            if (existing)
            {
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var obj = Object.Instantiate(AssetSearch.FindAsset<GameObject>("FeelSpeak", "ChoiceTimer").GetComponent<ChoiceTimerUGUI>(), Selection.activeTransform);
            obj.name = "ChoiceTimer";

            var canvas = obj.GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Add Choice Timer");
            Selection.activeGameObject = obj.gameObject;
        }

        [MenuItem("GameObject/Feel Speak/UI/Name Tag", priority = 4)]
        private static void AddNameTag()
        {
            var obj = Object.Instantiate(AssetSearch.FindAsset<GameObject>("FeelSpeak", "NameTag").GetComponent<NameTag>(), Selection.activeTransform);
            obj.name = "NameTag";

            var canvas = obj.GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            var speaker = obj.GetComponentInParent<Speaker>();

            if (speaker)
            {
                obj.speaker = speaker;
            }

            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Add Name Tag");
            Selection.activeGameObject = obj.gameObject;
        }
    }
}
#endif