using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class NodePort {
    public enum IO { Input, Output}

    public int ConnectionCount { get { return connections.Count; } }
    /// <summary> Return the first connection </summary>
    public NodePort Connection { get { return connections.Count > 0 ? connections[0].Port : null; } }

    public IO direction { get { return _direction; } }
    /// <summary> Is this port connected to anytihng? </summary>
    public bool IsConnected { get { return connections.Count != 0; } }
    public bool IsInput { get { return direction == IO.Input; } }
    public bool IsOutput { get { return direction == IO.Output; } }

    public Node node { get; private set; }
    public string name { get { return _name; } }
    public bool enabled { get { return _enabled; } set { _enabled = value; } }
    public string id { get { return _id; } }

    [SerializeField] private List<PortConnection> connections = new List<PortConnection>();
    [SerializeField] private string asdf;
    [SerializeField] private string _id;
    [SerializeField] public Type type;
    [SerializeField] private string _name;
    [SerializeField] private bool _enabled = true;
    [SerializeField] private IO _direction;

    public NodePort(string name, Type type, Node node, IO direction) {
        _name = name;
        this.type = type;
        this.node = node;
        _direction = direction;
        _id = node.GetInstanceID() + _name;
    }

    /// <summary> Connect this <see cref="NodePort"/> to another </summary>
    /// <param name="port">The <see cref="NodePort"/> to connect to</param>
    public void Connect(NodePort port) {
        if (connections == null) connections = new List<PortConnection>();
        if (port == null) { Debug.LogWarning("Cannot connect to null port"); return; }
        if (port == this) { Debug.LogWarning("Attempting to connect port to self."); return; }
        if (IsConnectedTo(port)) { Debug.LogWarning("Port already connected. "); return; }
        if (direction == port.direction) { Debug.LogWarning("Cannot connect two " + (direction == IO.Input ? "input" : "output") + " connections"); return; }
        connections.Add(new PortConnection(port));
        if (port.connections == null) port.connections = new List<PortConnection>();
        port.connections.Add(new PortConnection(this)); 
        node.OnCreateConnection(this, port);
        port.node.OnCreateConnection(this, port);
    }

    public NodePort GetConnection(int i) {
        return connections[i].Port;
    }

    public bool IsConnectedTo(NodePort port) {
        for (int i = 0; i < connections.Count; i++) {
            if (connections[i].Port == port) return true;
        }
        return false;
    }

    public void Disconnect(NodePort port) {
        for (int i = 0; i < connections.Count; i++) {
            if (connections[i].Port == port) {
                connections.RemoveAt(i);
            }
        }
        for (int i = 0; i < port.connections.Count; i++) {
            if (port.connections[i].Port == this) {
                port.connections.RemoveAt(i);
            }
        }
    }

    public void ClearConnections() {
        for (int i = 0; i < connections.Count; i++) {
            Disconnect(connections[i].Port);
        }
    }

    [Serializable]
    public class PortConnection {
        [SerializeField] public Node node;
        [SerializeField] public string portID;
        public NodePort Port { get { return port != null ? port : port = GetPort(); } }
        [NonSerialized] private NodePort port;

        public PortConnection(NodePort port) {
            this.port = port;
            node = port.node;
            portID = port.id;
        }

        private NodePort GetPort() {
            for (int i = 0; i < node.OutputCount; i++) {
                if (node.outputs[i].id == portID) return node.outputs[i];
            }
            for (int i = 0; i < node.InputCount; i++) {
                if (node.inputs[i].id == portID) return node.inputs[i];
            }
            return null;
        }
    }
}
