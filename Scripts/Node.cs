using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all nodes </summary>
[Serializable]
public abstract class Node {

    /// <summary> Name of the node </summary>
    public string name = "";
    [NonSerialized] public NodeGraph graph;
    public string NodeType { get { return nodeType; } }
    [SerializeField] private string nodeType;

    [SerializeField] public Rect position = new Rect(0,0,200,200);
    [SerializeField] protected NodePort[] inputs = new NodePort[0];
    [SerializeField] protected NodePort[] outputs = new NodePort[0];

    public int InputCount { get { return inputs.Length; } }
    public int OutputCount { get { return outputs.Length; } }

    protected Node() {
        nodeType = GetType().ToString();
        Init();
    }

    abstract protected void Init();

    public int GetInputId(NodePort input) {
        for (int i = 0; i < inputs.Length; i++) {
            if (input == inputs[i]) return i;

        }
        return -1;
    }
    public int GetOutputId(NodePort output) {
        for (int i = 0; i < outputs.Length; i++) {
            if (output == outputs[i]) return i;

        }
        return -1;
    }

    public NodePort GetInput(int portId) {
        return inputs[portId];
    }

    public NodePort GetOutput(int portId) {
        return outputs[portId];
    }

    public NodePort CreateNodeInput(string name, Type type) {
        return new NodePort(name, type, this, NodePort.IO.Input);
    }
    public NodePort CreateNodeOutput(string name, Type type) {
        return new NodePort(name, type, this, NodePort.IO.Output);
    }

    public void FinalizeDeserialization() {
        for (int i = 0; i < outputs.Length; i++) {
            outputs[i].FinalizeDeserialization();
        }
    }


    public void ClearConnections() {
        for (int i = 0; i < inputs.Length; i++) {
            inputs[i].ClearConnections();
        }
        for (int i = 0; i < outputs.Length; i++) {
            outputs[i].ClearConnections();
        }
    }
}
