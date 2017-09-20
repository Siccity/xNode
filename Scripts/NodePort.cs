using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NodePort {
    public enum IO { Input, Output}

    public int ConnectionCount { get { return connections.Count; } }
    /// <summary> Return the first connection </summary>
    public NodePort Connection { get { return connections.Count > 0 ? connections[0] : null; } }
    /// <summary> Returns a copy of the connections list </summary>
    public List<NodePort> Connections { get { return new List<NodePort>(connections); } }

    public IO direction { get { return _direction; } }
    /// <summary> Is this port connected to anytihng? </summary>
    public bool IsConnected { get { return connections.Count != 0; } }
    public bool IsInput { get { return direction == IO.Input; } }
    public bool IsOutput { get { return direction == IO.Output; } }

    public Type type { get { return _type; } }
    public Node node { get; private set; }
    public string name { get { return _name; } set { _name = value; } }
    public bool enabled { get { return _enabled; } set { _enabled = value; } }

    private Type _type;
    private List<NodePort> connections = new List<NodePort>();

    [SerializeField] private string _name;
    [SerializeField] private bool _enabled;
    [SerializeField] private IO _direction;

    public NodePort(string name, Type type, Node node, bool enabled, IO direction) {
        _name = name;
        _enabled = enabled;
        _type = type;
        this.node = node;
        _direction = direction;
    }

    public void Connect(NodePort port) {
        if (port == this) { Debug.LogWarning("Attempting to connect port to self."); return; }
        if (connections.Contains(port)) { Debug.LogWarning("Port already connected."); return; }
        if (direction == port.direction) { Debug.LogWarning("Cannot connect two " + (direction == IO.Input ? "input" : "output") + " connections"); return; }
        connections.Add(port);
        port.connections.Add(this);
    }

    public NodePort GetConnection(int i) {
        return connections[i];
    }

    public bool IsConnectedTo(NodePort port) {
        return connections.Contains(port);
    }

    public void Disconnect(NodePort port) {
        connections.Remove(port);
        port.connections.Remove(this);
    }

    public void ClearConnections() {
        for (int i = 0; i < connections.Count; i++) {
            connections[i].connections.Remove(this);
        }
        connections.Clear();
    }
}
