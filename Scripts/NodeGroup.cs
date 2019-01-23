using System;
using UnityEngine;

namespace XNode
{
    [Serializable]
    public class NodeGroup : ScriptableObject
    {
        [SerializeField] public NodeGraph graph;
        [SerializeField] public Vector2 position;
        [SerializeField] public Vector2 size = new Vector2(200, 300);
    }
}