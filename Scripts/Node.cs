using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UNEC;

/// <summary> Base class for all nodes </summary>
public abstract class Node {
    public Rect position = new Rect(0,0,200,200);
    protected NodePort[] inputs = new NodePort[0];
    protected NodePort[] outputs = new NodePort[0];

    public int InputCount { get { return inputs.Length; } }
    public int OutputCount { get { return outputs.Length; } }

    protected Node() {
        Init();
    }

    abstract protected void Init();

    public int GetInputPortId(NodePort input) {
        for (int i = 0; i < inputs.Length; i++) {
            if (input == inputs[i]) return i;

        }
        return -1;
    }
    public int GetOutputPortId(NodePort output) {
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

    public NodePort CreateNodeInput(string name, Type type, bool enabled = true) {
        return new NodePort(name, type, this, enabled);
    }
    public NodePort CreateNodeOutput(string name, Type type, bool enabled = true) {
        return new NodePort(name, type, this, enabled);
    }
}
