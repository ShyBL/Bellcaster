//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

#if UNITY_EDITOR
using Esper.FeelSpeak.Database;
using Esper.FeelSpeak.Graph;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Esper.FeelSpeak.Editor
{
    public class DialogueGraphEditorWindow : EditorWindow
    {
        public DialogueGraphView graphView;
        private Button saveButton;
        private NodeInspector nodeInspector;

        public static void Open(DialogueGraph graph)
        {
            DialogueGraphEditorWindow wnd = GetWindow<DialogueGraphEditorWindow>();
            wnd.titleContent = new GUIContent("Dialogue Editor");

            if (graph)
            {
                if (wnd.graphView.hasUnsavedChanges)
                {
                    wnd.AskToSave();
                }

                wnd.graphView.DisplayGraph(graph);
            }
        }

        private void CreateGUI()
        {
            minSize = new Vector2(700, 500);

            VisualElement root = rootVisualElement;
            var visualTree = AssetSearch.FindAsset<VisualTreeAsset>("FeelSpeak", "DialogueGraphEditorWindow");
            visualTree.CloneTree(root);

            if (!FeelSpeak.Settings)
            {
                FeelSpeakSettingsEditorWindow.GetOrCreateSettings();
            }

            graphView = root.Q<DialogueGraphView>();
            saveButton = root.Q<Button>("SaveButton");

            saveButton.clicked += graphView.Save;
            saveButton.style.display = DisplayStyle.None;

            graphView.Initialize(root.Q<Toolbar>("NodeSelector"));
            graphView.onGraphChanged.AddListener(OnGraphChanged);

            nodeInspector = root.Q<NodeInspector>();
            nodeInspector.Initialize(graphView);
            graphView.nodeInspector = nodeInspector;

            FeelSpeakDatabase.Initialize();
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying && !HasOpenInstances<FeelSpeakEditorWindow>())
            {
                FeelSpeakDatabase.Disconnect();
            }

            AskToSave();
        }

        [Shortcut("Dialogue Editor/Save", typeof(DialogueGraphEditorWindow), KeyCode.S, ShortcutModifiers.Action)]
        private static void SaveShortcut()
        {
            if (HasOpenInstances<DialogueGraphEditorWindow>())
            {
                var window = GetWindow<DialogueGraphEditorWindow>();

                if (window.graphView == null)
                {
                    return;
                }

                window.graphView.Save();
            }
        }

        [Shortcut("Dialogue Editor/Delete", typeof(DialogueGraphEditorWindow), KeyCode.Delete)]
        private static void DeleteShortcut()
        {
            if (HasOpenInstances<DialogueGraphEditorWindow>())
            {
                var window = GetWindow<DialogueGraphEditorWindow>();

                if (window.graphView == null)
                {
                    return;
                }

                window.graphView.DeleteSelection();
            }
        }

        [Shortcut("Dialogue Editor/Copy", typeof(DialogueGraphEditorWindow), KeyCode.C, ShortcutModifiers.Action)]
        private static void CopyShortcut()
        {
            if (HasOpenInstances<DialogueGraphEditorWindow>())
            {
                var window = GetWindow<DialogueGraphEditorWindow>();

                if (window.graphView == null)
                {
                    return;
                }

                window.graphView.CopySelection();
            }
        }

        [Shortcut("Dialogue Editor/Paste", typeof(DialogueGraphEditorWindow), KeyCode.V, ShortcutModifiers.Action)]
        private static void PasteShortcut()
        {
            if (HasOpenInstances<DialogueGraphEditorWindow>())
            {
                var window = GetWindow<DialogueGraphEditorWindow>();

                if (window.graphView == null)
                {
                    return;
                }

                window.graphView.PasteSelection();
            }
        }

        [Shortcut("Dialogue Editor/Cut", typeof(DialogueGraphEditorWindow), KeyCode.X, ShortcutModifiers.Action)]
        private static void CutShortcut()
        {
            if (HasOpenInstances<DialogueGraphEditorWindow>())
            {
                var window = GetWindow<DialogueGraphEditorWindow>();

                if (window.graphView == null)
                {
                    return;
                }

                window.graphView.CutSelection();
            }
        }

        private void OnGraphChanged()
        {
            if (graphView == null)
            {
                return;
            }

            if (graphView.hasUnsavedChanges)
            {
                saveButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                saveButton.style.display = DisplayStyle.None;
            }
        }

        private void AskToSave()
        {
            if (graphView == null || !graphView.hasUnsavedChanges || !graphView.graph)
            {
                return;
            }

            if (EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes. If the graph is not saved, all changes will be reverted. Would you like to save the graph?", "Save", "Revert"))
            {
                graphView.Save();
            }
            else
            {
                graphView.Revert();
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            if (graphView == null)
            {
                return;
            }

            graphView.Refresh();
            graphView.hasUnsavedChanges = true;
            graphView.onGraphChanged.Invoke();
        }
    }
}
#endif