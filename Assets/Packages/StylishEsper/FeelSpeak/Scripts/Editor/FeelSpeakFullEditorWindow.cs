//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class FeelSpeakFullEditorWindow : EditorWindow
    {
        private Button getFullButton;

        [MenuItem("Window/Feel Speak/Get Full Version", secondaryPriority = 2)]
        public static void Open()
        {
            FeelSpeakFullEditorWindow wnd = GetWindow<FeelSpeakFullEditorWindow>();
            wnd.titleContent = new GUIContent("Full Version Benefits");
        }

        private void CreateGUI()
        {
            minSize = new Vector2(400, 500);

            VisualElement root = rootVisualElement;
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "FeelSpeakFullEditorWindow");
            visualTree.CloneTree(root);

            getFullButton = root.Q<Button>("GetFullButton");
            getFullButton.clicked += () => Application.OpenURL("https://u3d.as/3zcG");
        }
    }
}
#endif