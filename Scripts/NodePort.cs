using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class NodePort :ISerializationCallbackReceiver{
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

    [NonSerialized] private List<NodePort> connections = new List<NodePort>();

    [SerializeField] private Type _type;
    [SerializeField] private string _name;
    [SerializeField] private bool _enabled = true;
    [SerializeField] private IO _direction;

    [SerializeField] private PortID[] connectionIDs;

    public NodePort(string name, Type type, Node node, IO direction) {
        _name = name;
        _type = type;
        this.node = node;
        _direction = direction;
    }

    public void Connect(NodePort port) {
        if (connections == null) connections = new List<NodePort>();
        if (port == null) { Debug.LogWarning("Cannot connect to null port"); return; }
        if (port == this) { Debug.LogWarning("Attempting to connect port to self."); return; }
        if (connections.Contains(port)) { Debug.LogWarning("Port already connected. "); return; }
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

    public void OnBeforeSerialize() {
        if (direction == IO.Output) {
            connectionIDs = new PortID[connections.Count];
            for (int i = 0; i < connections.Count; i++) {
                connectionIDs[i] = new PortID();
                connectionIDs[i].nodeID = node.graph.nodes.IndexOf(connections[i].node);
                connectionIDs[i].portID = connections[i].node.GetInputId(connections[i]);
            }
        }
    }

    public void OnAfterDeserialize() {

    }

    public void FinalizeDeserialization() {
        //Reconnect
        if (direction == IO.Output) {
            connections = new List<NodePort>();
            for (int i = 0; i < connectionIDs.Length; i++) {
                Node node = this.node.graph.nodes[connectionIDs[i].nodeID];
                NodePort port = node.GetInput(connectionIDs[i].portID);
                Connect(port);
            }
        }
    }


    [Serializable]
    private class PortID {
        public int nodeID;
        public int portID;
    }
}
