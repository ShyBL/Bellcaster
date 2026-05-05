#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor
{
    private Editor _dataEditor;

    public override void OnInspectorGUI()
    {
        // Draw the default Interactable fields (just the 'data' slot)
        DrawDefaultInspector();

        var interactable = (Interactable)target;

        if (interactable.data == null)
        {
            EditorGUILayout.HelpBox("No InteractableData assigned.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Interactable Data", EditorStyles.boldLabel);

        // Create or reuse an editor for the data asset
        CreateCachedEditor(interactable.data, null, ref _dataEditor);
        _dataEditor.OnInspectorGUI();
    }

    private void OnDisable()
    {
        if (_dataEditor != null)
        {
            DestroyImmediate(_dataEditor);
            _dataEditor = null;
        }
    }
}
#endif