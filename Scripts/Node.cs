using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all nodes </summary>
[Serializable]
public abstract class Node {

    /// <summary> Name of the node </summary>
    [SerializeField] public string name = "";
    [SerializeField] public NodeGraph graph;

    [SerializeField] private NodePort[] inputs = new NodePort[0];
    [SerializeField] private NodePort[] outputs = new NodePort[0];
    [SerializeField] public Rect rect = new Rect(0,0,200,200);

    public int InputCount { get { return inputs.Length; } }
    public int OutputCount { get { return outputs.Length; } }

    /// <summary> Constructor </summary>
    protected Node() {
        CachePorts(); //Cache the ports at creation time so we don't have to use reflection at runtime
        Init();
    }

    /// <summary> Initialize node. Called on creation. </summary>
    protected virtual void Init() {

    }

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

    public void ClearConnections() {
        for (int i = 0; i < inputs.Length; i++) {
            inputs[i].ClearConnections();
        }
        for (int i = 0; i < outputs.Length; i++) {
            outputs[i].ClearConnections();
        }
    }

    public override int GetHashCode() {
        return JsonUtility.ToJson(this).GetHashCode();
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InputAttribute : Attribute {
        public InputAttribute() {
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class OutputAttribute : Attribute {
        public OutputAttribute() {
        }
    }

    /// <summary> Use reflection to find all fields with <see cref="InputAttribute"/> or <see cref="OutputAttribute"/>, and write to <see cref="inputs"/> and <see cref="outputs"/> </summary>
    private void CachePorts() {
        List<NodePort> inputPorts = new List<NodePort>();
        List<NodePort> outputPorts = new List<NodePort>();

        System.Reflection.FieldInfo[] fieldInfo = GetType().GetFields();
        for (int i = 0; i < fieldInfo.Length; i++) {
            //Get InputAttribute and OutputAttribute
            object[] attribs = fieldInfo[i].GetCustomAttributes(false);
            InputAttribute inputAttrib =  null;
            OutputAttribute outputAttrib = null;
            for (int k = 0; k < attribs.Length; k++) {
                if (attribs[k] is InputAttribute) inputAttrib = attribs[k] as InputAttribute;
                else if (attribs[k] is OutputAttribute) outputAttrib = attribs[k] as OutputAttribute;
            }
            if (inputAttrib != null && outputAttrib != null) Debug.LogError("Field " + fieldInfo + " cannot be both input and output.");
            else if (inputAttrib != null) inputPorts.Add(new NodePort(fieldInfo[i].Name, fieldInfo[i].FieldType, this, NodePort.IO.Input));
            else if (outputAttrib != null) outputPorts.Add(new NodePort(fieldInfo[i].Name, fieldInfo[i].FieldType, this, NodePort.IO.Output));
        }

        inputs = inputPorts.ToArray();
        outputs = outputPorts.ToArray();
    }
}
