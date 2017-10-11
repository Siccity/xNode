using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all node graphs </summary>
[Serializable]
public abstract class NodeGraph : ScriptableObject, ISerializationCallbackReceiver {

    /// <summary> All nodes in the graph. <para/>
    /// See: <see cref="AddNode{T}"/> </summary>
    [SerializeField] public List<Node> nodes = new List<Node>();

    public T AddNode<T>() where T : Node {
        return AddNode(typeof(T)) as T;
    }

    public virtual Node AddNode(Type type) {
        if (!NodeDataCache.Initialized) NodeDataCache.Initialize();
        Node node = ScriptableObject.CreateInstance(type) as Node;
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
        nodes.Add(node);
        node.graph = this;
        return node;
    }

    /// <summary> Safely remove a node and all its connections </summary>
    /// <param name="node"></param>
    public void RemoveNode(Node node) {
        node.ClearConnections();
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            DestroyImmediate(node, true);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
        nodes.Remove(node);
    }

    /// <summary> Remove all nodes and connections from the graph </summary>
    public void Clear() {
        nodes.Clear();
    }

    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].graph = this;
        }
        VerifyConnections();
    }

    /// <summary> Checks all connections for invalid references, and removes them. </summary>
    public void VerifyConnections() {
        for (int i = 0; i < nodes.Count; i++) {
            nodes[i].VerifyConnections();
        }
    }
}

