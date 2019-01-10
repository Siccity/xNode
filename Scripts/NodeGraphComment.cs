using System;
using UnityEngine;

namespace XNode
{
    [Serializable]
    public class NodeGraphComment : ScriptableObject
    {
        [SerializeField] public NodeGraph graph;
        [SerializeField] public Vector2 position;
        [SerializeField] public Vector2 size = new Vector2(200, 300);
        [SerializeField] public string comment;
    }
}