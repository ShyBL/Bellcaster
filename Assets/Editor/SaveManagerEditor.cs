#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw default inspector fields (SerializedFields, etc.)
        DrawDefaultInspector();

        SaveManager manager = (SaveManager)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

        // 2. Button: Delete Save File
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f); // Red tint for destructive actions
        if (GUILayout.Button("Delete & Reset Save Data", GUILayout.Height(30)))
        {
            manager.ResetSaveData();
        }

        // 3. Button: Reset & Reload State (Only during Play Mode)
        GUI.backgroundColor = Color.white;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Reset Save & Reload Boot State", GUILayout.Height(25)))
            {
                manager.ResetSaveData();
                manager.LoadAndInitialize();
            }
        }
    }
}
#endif