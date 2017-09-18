using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NodePort {
    public enum IO { None, Input, Output}

    public IO direction {
        get {
            for (int i = 0; i < node.InputCount; i++) {
                if (node.GetInput(i) == this) return IO.Input;
            }
            for (int i = 0; i < node.OutputCount; i++) {
                if (node.GetOutput(i) == this) return IO.Output;
            }
            return IO.None;
        }
    }
    public Node node { get; private set; }
    public string name { get { return _name; } set { _name = value; } }
    [SerializeField]
    private string _name;
    public Type type { get; private set; }
    [SerializeField]
    private string _type;
    public bool enabled { get { return _enabled; } set { _enabled = value; } }
    [SerializeField]
    private bool _enabled;

    public NodePort(string name, Type type, Node node, bool enabled) {
        _name = name;
        _enabled = enabled;
        this.type = type;
        _type = type.FullName;
        this.node = node;
    }
    
    public NodePort GetConnection() {
        return null;
    }
    public NodePort[] GetConnections() {
        return null;
    }
}
