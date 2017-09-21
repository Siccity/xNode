using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

[CreateAssetMenu(fileName = "NewNodeGraph", menuName = "Node Graph")]
public class NodeGraphAsset : ScriptableObject {
    public string json { get { return _json; } set { _json = value; _nodeGraph = null; } }
    [SerializeField] private string _json;

    public NodeGraph nodeGraph { get { return _nodeGraph != null ? _nodeGraph : _nodeGraph = NodeGraph.Deserialize(json); } }
    [NonSerialized] private NodeGraph _nodeGraph = null;
}
