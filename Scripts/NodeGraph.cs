using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all node graphs </summary>
public abstract class NodeGraph : ScriptableObject {
    /// <summary> All nodes in the graph. <para/>
    /// See: <see cref="AddNode{T}"/> </summary>
    [NonSerialized] public List<Node> nodes = new List<Node>();

    /// <summary> Serialized nodes. </summary>
    [SerializeField] public string[] s_nodes;

    public T AddNode<T>() where T : Node {
        T node = default(T);
        return AddNode(node) as T;
    }

    public Node AddNode(Type type) {
        Node node = (Node)Activator.CreateInstance(type);
        return AddNode(node);
    }

    public Node AddNode(string type) {
        Debug.Log(type);
        Node node = (Node)Activator.CreateInstance(null,type).Unwrap();
        return AddNode(node);
    }

    public Node AddNode(Node node) {
        if (node == null) {
            Debug.LogError("Node could node be instanced");
            return null;
        }
        nodes.Add(node);
        node.graph = this;
        return node;
    }

    /// <summary> Safely remove a node and all its connections </summary>
    /// <param name="node"></param>
    public void RemoveNode(Node node) {
        node.ClearConnections();
        nodes.Remove(node);
    }

    public void Clear() {
        nodes.Clear();
    }

    private class NodeTyper {
        public string nodeType = "Node";
    }
}

