#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InteractableData))]
public class InteractableDataEditor : Editor
{
    private SerializedProperty _canExamine;
    private SerializedProperty _canPickUp;
    private SerializedProperty _canInteract;
    private SerializedProperty _canNavigate;

    private void OnEnable()
    {
        _canExamine  = serializedObject.FindProperty("canExamine");
        _canPickUp   = serializedObject.FindProperty("canPickUp");
        _canInteract = serializedObject.FindProperty("canInteract");
        _canNavigate = serializedObject.FindProperty("canNavigate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject,
            "m_Script", "canExamine", "canPickUp", "canInteract", "canNavigate");

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Interaction Type (pick one)", EditorStyles.boldLabel);

        DrawRadioFlag(_canExamine,  "Examine");
        DrawRadioFlag(_canPickUp,   "Pick Up");
        DrawRadioFlag(_canInteract, "Interact");
        DrawRadioFlag(_canNavigate, "Navigate");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRadioFlag(SerializedProperty flag, string label)
    {
        bool newValue = EditorGUILayout.ToggleLeft(label, flag.boolValue);
        if (newValue == flag.boolValue) return;

        flag.boolValue = newValue;
        if (!newValue) return;

        foreach (var other in new[] { _canExamine, _canPickUp, _canInteract, _canNavigate })
        {
            if (other != flag) other.boolValue = false;
        }
    }
}
#endif