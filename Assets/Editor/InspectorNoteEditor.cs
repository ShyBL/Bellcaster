using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(InspectorNote))]
public class InspectorNoteEditor : Editor
{
    const string AssetPath = "Assets/Editor/InspectorNoteEditor.uxml";

    public override VisualElement CreateInspectorGUI()
    {
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetPath);

        var doc = uxml.Instantiate();
        doc.dataSource = target;

        var showEditorProp = serializedObject.FindProperty("_showEditor");
        var noteEditor = doc.Q("note-editor");
        var doneButton = doc.Q<Button>("done-button");

        if (!showEditorProp.boolValue)
            noteEditor.style.display = DisplayStyle.None;

        noteEditor.TrackPropertyValue(showEditorProp, prop => {
            noteEditor.style.display =
              prop.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
        });

        doneButton.clicked += () => {
            showEditorProp.boolValue = false;
            serializedObject.ApplyModifiedProperties();
        };

        return doc;
    }
}

