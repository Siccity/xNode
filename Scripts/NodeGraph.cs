using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all node graphs </summary>
public class NodeGraph {

    /// <summary> All nodes in the graph. <para/>
    /// See: <see cref="AddNode{T}"/> </summary>
    public List<Node> nodes = new List<Node>();

    public T AddNode<T>() where T : Node {
        T node = default(T);
        nodes.Add(node);
        return node;
    }

    public Node AddNode(Type type)  {
        Node node = (Node)Activator.CreateInstance(type);
        if (node == null) {
            Debug.LogError("Node could node be instanced");
            return null;
        }
        nodes.Add(node);
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
}
