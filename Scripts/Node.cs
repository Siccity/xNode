using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> Base class for all nodes </summary>
[Serializable]
public abstract class Node : ScriptableObject {

    /// <summary> Name of the node </summary>
    [SerializeField] public NodeGraph graph;
    [SerializeField] public Vector2 position;
    /// <summary> Input <see cref="NodePort"/>s. It is recommended not to modify these at hand. Instead, see <see cref="InputAttribute"/> </summary>
    [SerializeField] public List<NodePort> inputs = new List<NodePort>();
    /// <summary> Output <see cref="NodePort"/>s. It is recommended not to modify these at hand. Instead, see <see cref="InputAttribute"/> </summary>
    [SerializeField] public List<NodePort> outputs = new List<NodePort>();

    public int InputCount { get { return inputs.Count; } }
    public int OutputCount { get { return outputs.Count; } }

    protected void OnEnable() {
        GetPorts();
        Init();
    }

    /// <summary> Checks all connections for invalid references, and removes them. </summary>
    public void VerifyConnections() {
        for (int i = 0; i < InputCount; i++) {
            inputs[i].VerifyConnections();
        }
        for (int i = 0; i < OutputCount; i++) {
            outputs[i].VerifyConnections();
        }
    }

    /// <summary> Returns input or output port which matches fieldName </summary>
    public NodePort GetPortByFieldName(string fieldName) {
        NodePort port = GetOutputByFieldName(fieldName);
        if (port != null) return port;
        else return GetInputByFieldName(fieldName);
    }

    /// <summary> Returns output port which matches fieldName. Returns null if none found. </summary>
    public NodePort GetOutputByFieldName(string fieldName) {
        for (int i = 0; i < OutputCount; i++) {
            if (outputs[i].fieldName == fieldName) return outputs[i];
        }
        return null;
    }

    /// <summary> Returns input port which matches. Returns null if none found. </summary>
    public NodePort GetInputByFieldName(string fieldName) {
        for (int i = 0; i < InputCount; i++) {
            if (inputs[i].fieldName == fieldName) return inputs[i];
        }
        return null;
    }

    public virtual object GetValue(NodePort port) {
        Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
        return null;
    }

    /// <summary> Initialize node. Called on creation. </summary>
    protected virtual void Init() { name = GetType().Name; }

    /// <summary> Called whenever a connection is being made between two <see cref="NodePort"/>s</summary>
    /// <param name="from">Output</param> <param name="to">Input</param>
    public virtual void OnCreateConnection(NodePort from, NodePort to) { }

    public int GetInputId(NodePort input) {
        for (int i = 0; i < inputs.Count; i++) {
            if (input == inputs[i]) return i;

        }
        return -1;
    }
    public int GetOutputId(NodePort output) {
        for (int i = 0; i < outputs.Count; i++) {
            if (output == outputs[i]) return i;

        }
        return -1;
    }

    public void ClearConnections() {
        for (int i = 0; i < inputs.Count; i++) {
            inputs[i].ClearConnections();
        }
        for (int i = 0; i < outputs.Count; i++) {
            outputs[i].ClearConnections();
        }
    }

    public override int GetHashCode() {
        return JsonUtility.ToJson(this).GetHashCode();
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InputAttribute : Attribute {
        public bool fallback;
        public InputAttribute(bool fallback) { this.fallback = fallback; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OutputAttribute : Attribute {
        public bool fallback;
        public OutputAttribute(bool fallback) { this.fallback = fallback; }
    }

    private void GetPorts() {
        NodeDataCache.UpdatePorts(this, inputs, outputs);
    }
}