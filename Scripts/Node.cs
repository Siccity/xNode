using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary> Base class for all nodes </summary>
[Serializable]
public abstract class Node : ScriptableObject {

    /// <summary> Name of the node </summary>
    [NonSerialized] public NodeGraph graph;
    [SerializeField] public Rect rect = new Rect(0,0,200,200);
    /// <summary> Input <see cref="NodePort"/>s. It is recommended not to modify these at hand. Instead, see <see cref="InputAttribute"/> </summary>
    [SerializeField] public List<NodePort> inputs = new List<NodePort>();
    /// <summary> Output <see cref="NodePort"/>s. It is recommended not to modify these at hand. Instead, see <see cref="InputAttribute"/> </summary>
    [SerializeField] public NodePort[] outputs = new NodePort[0];

    public int InputCount { get { return inputs.Count; } }
    public int OutputCount { get { return outputs.Length; } }

    protected Node() {
        CachePorts(); //Cache the ports at creation time so we don't have to use reflection at runtime
    }

    protected void OnEnable() {
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

    /// <summary> Returns output port which matches fieldName </summary>
    public NodePort GetOutputByFieldName(string fieldName) {
        for (int i = 0; i < OutputCount; i++) {
            if (outputs[i].fieldName == fieldName) return outputs[i];
        }
        return null;
    }

    /// <summary> Returns input port which matches fieldName </summary>
    public NodePort GetInputByFieldName(string fieldName) {
        for (int i = 0; i < InputCount; i++) {
            if (inputs[i].fieldName == fieldName) return inputs[i];
        }
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
        for (int i = 0; i < outputs.Length; i++) {
            if (output == outputs[i]) return i;

        }
        return -1;
    }

    public void ClearConnections() {
        for (int i = 0; i < inputs.Count; i++) {
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
            else if (inputAttrib != null) inputPorts.Add(new NodePort(fieldInfo[i], this));
            else if (outputAttrib != null) outputPorts.Add(new NodePort(fieldInfo[i], this));
        }

        inputs = inputPorts;
        outputs = outputPorts.ToArray();
    }
}
