using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary> Base class for all node graphs </summary>
[Serializable]
public class NodeGraph {
    /// <summary> All nodes in the graph. <para/>
    /// See: <see cref="AddNode{T}"/> </summary>
    [NonSerialized] public List<Node> nodes = new List<Node>();

    /// <summary> Serialized nodes. </summary>
    [SerializeField] public string[] s_nodes;

    public List<string> strings = new List<string>() { "ASDF", "3523" };
    public T AddNode<T>() where T : Node {
        T node = default(T);
        nodes.Add(node);
        return node;
    }

    public Node AddNode(Type type) {
        Node node = (Node)Activator.CreateInstance(type);
        if (node == null) {
            Debug.LogError("Node could node be instanced");
            return null;
        }
        nodes.Add(node);
        return node;
    }

    public Node AddNode(string type) {
        Node node = (Node)Activator.CreateInstance(null,type).Unwrap();
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

    public string Serialize() {
        //Unity serializer doesn't support polymorphism, so we'll have to use a hack
        s_nodes = new string[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) {
            s_nodes[i] = JsonUtility.ToJson(nodes[i]);
        }
        //s_nodes = new string[] { "<SERIALIZED_NODES>" };
        string json = JsonUtility.ToJson(this);
        //json = json.Replace("\"<SERIALIZED_NODES>\"", GetSerializedList(nodes));
        return json;
    }

    private string GetSerializedList<T>(List<T> list) {
        string[] s_list = new string[list.Count];
        for (int i = 0; i < list.Count; i++) {
            s_list[i] = JsonUtility.ToJson(list[i]);
        }
        return string.Join(",", s_list);
        
    }

    public static NodeGraph Deserialize(string json) {
        NodeGraph nodeGraph = JsonUtility.FromJson<NodeGraph>(json);
        for (int i = 0; i < nodeGraph.s_nodes.Length; i++) {
            NodeTyper tempNode = new NodeTyper();
            JsonUtility.FromJsonOverwrite(nodeGraph.s_nodes[i],tempNode);
            Node node = nodeGraph.AddNode(tempNode.nodeType);
            JsonUtility.FromJsonOverwrite(nodeGraph.s_nodes[i], node);
        }
        return nodeGraph;
    }

    private class NodeTyper {
        public string nodeType;
    }
}

