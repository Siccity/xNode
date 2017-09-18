using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all node graphs </summary>
public class NodeGraph {

    public Dictionary<int, Node> nodes = new Dictionary<int, Node>();

    public T AddNode<T>() where T : Node {
        T node = default(T);
        nodes.Add(GetUniqueID(), node);
        return node;
    }

    public Node AddNode(Type type)  {
        Node node = (Node)Activator.CreateInstance(type);
        if (node == null) return null;
        nodes.Add(GetUniqueID(), node);
        return node;
    }

    public void RemoveNode(Node node) {
        int id = GetNodeID(node);
        if (id != -1) nodes.Remove(id);
        else Debug.LogWarning("Node " + node.ToString() + " is not part of NodeGraph");
    }

    public void RemoveNode(int id) {
        nodes.Remove(id);
    }

    public int GetNodeID(Node node) {
        foreach(var kvp in nodes) {
            if (kvp.Value == node) return kvp.Key;
        }
        return -1;
    }

    public Node GetNode(int id) {
        if (nodes.ContainsKey(id)) return nodes[id];
        return null;
    }

    private int GetUniqueID() {
        int id = 0;
        while (nodes.ContainsKey(id)) id++;
        return id;
    }

    public void Clear() {
        nodes.Clear();
    }
}
