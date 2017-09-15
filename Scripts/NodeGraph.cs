using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all node graphs </summary>
public class NodeGraph {

    public Dictionary<string, Node> nodes = new Dictionary<string, Node>();

    public T AddNode<T>() where T : Node {
        T node = default(T);
        nodes.Add(GetGUID(), node);
        return node;
    }

    public Node AddNode(Type type)  {
        Node node = (Node)Activator.CreateInstance(type);
        if (node == null) return null;
        nodes.Add(GetGUID(), node);
        return node;
    }

    private string GetGUID() {
        string guid = Guid.NewGuid().ToString();
        while(nodes.ContainsKey(guid)) guid = GetGUID();
        return guid;
    }

    public void Clear() {
        nodes.Clear();
    }
}
