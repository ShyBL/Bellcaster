using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Small floating utility window with one text field and Create/Cancel —
/// Unity has no built-in single-line text input dialog
/// (EditorUtility.DisplayDialog only supports buttons). Used by
/// QuestAuthoringWindow to ask for a Quest ID or Objective short name
/// before creating the asset, so the folder/filename can be correct from
/// the moment of creation rather than needing a rename pass afterward.
/// </summary>
public class EditorInputPrompt : EditorWindow
{
    private string value;
    private string fieldLabel;
    private Action<string> onConfirm;
    private bool focused;

    public static void Show(string title, string label, string defaultValue, Action<string> onConfirm)
    {
        var window = CreateInstance<EditorInputPrompt>();
        window.titleContent = new GUIContent(title);
        window.value = defaultValue ?? string.Empty;
        window.fieldLabel = label;
        window.onConfirm = onConfirm;
        window.minSize = new Vector2(320, 80);
        window.maxSize = new Vector2(320, 80);
        window.ShowUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);

        GUI.SetNextControlName("EditorInputPromptField");
        value = EditorGUILayout.TextField(fieldLabel, value);

        if (!focused)
        {
            EditorGUI.FocusTextInControl("EditorInputPromptField");
            focused = true;
        }

        bool enterPressed = Event.current.type == EventType.KeyDown &&
                            (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Cancel"))
            Close();

        if (GUILayout.Button("Create") || enterPressed)
        {
            var confirmed = value;
            Close();
            onConfirm?.Invoke(confirmed);
        }

        EditorGUILayout.EndHorizontal();
    }
}