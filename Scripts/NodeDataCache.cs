using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEditor;

/// <summary> Precaches reflection data in editor so we won't have to do it runtime </summary>
public static class NodeDataCache {
    private static PortDataCache portDataCache;
    private static bool Initialized { get { return portDataCache != null; } }

    /// <summary> Checks for invalid and removes them. 
    /// Checks for missing ports and adds them. 
    /// Checks for invalid connections and removes them. </summary>
    public static void UpdatePorts(Node node, List<NodePort> inputs, List<NodePort> outputs) {
        if (!Initialized) BuildCache();

        List<NodePort> inputPorts = new List<NodePort>();
        List<NodePort> outputPorts = new List<NodePort>();

        System.Type nodeType = node.GetType();
        inputPorts = new List<NodePort>();
        outputPorts = new List<NodePort>();
        if (!portDataCache.ContainsKey(nodeType)) return;
        for (int i = 0; i < portDataCache[nodeType].Count; i++) {
            if (portDataCache[nodeType][i].direction == NodePort.IO.Input) inputPorts.Add(new NodePort(portDataCache[nodeType][i], node));
            else outputPorts.Add(new NodePort(portDataCache[nodeType][i], node));
        }

        for (int i = inputs.Count-1; i >= 0; i--) {
            int index = inputPorts.FindIndex(x => inputs[i].fieldName == x.fieldName);
            //If input nodeport does not exist, remove it
            if (index == -1) inputs.RemoveAt(i);
            //If input nodeport does exist, update it
            else inputs[i].type = inputPorts[index].type;
        }
        for (int i = outputs.Count - 1; i >= 0; i--) {
            int index = outputPorts.FindIndex(x => outputs[i].fieldName == x.fieldName);
            //If output nodeport does not exist, remove it
            if (index == -1) outputs.RemoveAt(i);
            //If output nodeport does exist, update it
            else outputs[i].type = outputPorts[index].type;
        }
        //Add
        for (int i = 0; i < inputPorts.Count; i++) {
            //If inputports contains a new port, add it
            if (!inputs.Any(x => x.fieldName == inputPorts[i].fieldName)) inputs.Add(inputPorts[i]);
        }
        for (int i = 0; i < outputPorts.Count; i++) {
            //If inputports contains a new port, add it
            if (!outputs.Any(x => x.fieldName == outputPorts[i].fieldName)) outputs.Add(outputPorts[i]);
        }
    }

    private static void BuildCache() {
        portDataCache = new PortDataCache();
        System.Type baseType = typeof(Node);
        Assembly assembly = Assembly.GetAssembly(baseType);
        System.Type[] nodeTypes = assembly.GetTypes().Where(t =>
            !t.IsAbstract &&
            baseType.IsAssignableFrom(t)
            ).ToArray();

        for (int i = 0; i < nodeTypes.Length; i++) {
            CachePorts(nodeTypes[i]);
        }
    }

    private static void CachePorts(System.Type nodeType) {
        System.Reflection.FieldInfo[] fieldInfo = nodeType.GetFields();
        for (int i = 0; i < fieldInfo.Length; i++) {

            //Get InputAttribute and OutputAttribute
            object[] attribs = fieldInfo[i].GetCustomAttributes(false);
            Node.InputAttribute inputAttrib = attribs.FirstOrDefault(x => x is Node.InputAttribute) as Node.InputAttribute;
            Node.OutputAttribute outputAttrib = attribs.FirstOrDefault(x => x is Node.OutputAttribute) as Node.OutputAttribute;

            if (inputAttrib == null && outputAttrib == null) continue;

            if (inputAttrib != null && outputAttrib != null) Debug.LogError("Field " + fieldInfo + " cannot be both input and output.");
            else {
                if (!portDataCache.ContainsKey(nodeType)) portDataCache.Add(nodeType, new List<NodePort>());
                portDataCache[nodeType].Add(new NodePort(fieldInfo[i]));
            }
        }
    }

    [System.Serializable]
    private class PortDataCache : Dictionary<System.Type, List<NodePort>>, ISerializationCallbackReceiver {
        [SerializeField] private List<System.Type> keys = new List<System.Type>();
        [SerializeField] private List<List<NodePort>> values = new List<List<NodePort>>();

        // save the dictionary to lists
        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();
            foreach (var pair in this) {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize() {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
}
