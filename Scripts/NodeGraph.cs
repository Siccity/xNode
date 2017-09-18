using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UNEC;

/// <summary> Base class for all node graphs </summary>
public class NodeGraph {

    public Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    private List<NodeConnection> connections = new List<NodeConnection>();

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
        int id = GetNodeId(node);
        if (id != -1) nodes.Remove(id);
        else Debug.LogWarning("Node " + node.ToString() + " is not part of NodeGraph");
    }

    public void RemoveNode(int nodeId) {
        nodes.Remove(nodeId);
    }

    public int GetNodeId(Node node) {
        foreach(var kvp in nodes) {
            if (kvp.Value == node) return kvp.Key;
        }
        return -1;
    }

    public Node GetNode(int nodeId) {
        if (nodes.ContainsKey(nodeId)) return nodes[nodeId];
        return null;
    }

    public void AddConnection(NodePort input, NodePort output) {
        int inputNodeId = GetNodeId(input.node);
        int outputPortId = input.node.GetInputPortId(output);

        int outputNodeId = GetNodeId(output.node);
        int inputPortId = output.node.GetInputPortId(input);

        NodeConnection connection = new NodeConnection(inputNodeId, inputPortId, outputNodeId, outputPortId);
    }

    private int GetUniqueID() {
        int id = 0;
        while (nodes.ContainsKey(id)) id++;
        return id;
    }

    public void Clear() {
        nodes.Clear();
        connections.Clear();
    }
}
