using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneGraphEditor : EditorWindow
{
    private SceneGraphData _dataContainer;

    [MenuItem("Tools/Scene Relationship Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<SceneGraphEditor>();
        window.titleContent = new GUIContent("Scene Graph");
    }

    private void OnEnable()
    {
        LoadData();
        ConstructGraph();
    }

    private void LoadData()
    {
        // Look for the data file in the project
        string[] guids = AssetDatabase.FindAssets("t:SceneGraphData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _dataContainer = AssetDatabase.LoadAssetAtPath<SceneGraphData>(path);
        }
        else
        {
            // Create it if it doesn't exist
            _dataContainer = ScriptableObject.CreateInstance<SceneGraphData>();
            AssetDatabase.CreateAsset(_dataContainer, "Assets/SceneGraphSaveData.asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void ConstructGraph()
    {
        rootVisualElement.Clear(); // Clear old graph on refresh
        var graphView = new SceneGraphView(_dataContainer) { name = "Scene Graph View" };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }
}