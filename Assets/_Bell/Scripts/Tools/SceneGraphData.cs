using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneGraphData", menuName = "Tools/Scene Graph Data")]
public class SceneGraphData : ScriptableObject
{
    public List<NodeData> Nodes = new List<NodeData>();
    public List<EdgeData> Edges = new List<EdgeData>();
}

[System.Serializable]
public class NodeData
{
    public string GUID;
    public string SceneName;
    public Vector2 Position;
}

[System.Serializable]
public class EdgeData
{
    public string BaseNodeGUID;
    public string TargetNodeGUID;
    public string RelationText;
}