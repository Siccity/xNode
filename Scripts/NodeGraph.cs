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
        return AddNode(typeof(T)) as T;
    }

    public virtual Node AddNode(Type type) {
        Node node = (Node)Activator.CreateInstance(type);
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

    /// <summary> Remove all nodes and connections from the graph </summary>
    public void Clear() {
        nodes.Clear();
    }
}

