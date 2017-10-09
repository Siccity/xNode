using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

/// <summary> Precaches reflection data in editor so we won't have to do it runtime </summary>
public sealed class NodeDataCache : ScriptableObject {
    public static NodeDataCache instance { get { return _instance; } }
    private static NodeDataCache _instance;

    [SerializeField]
    private PortDataCache portDataCache = new PortDataCache();

    [RuntimeInitializeOnLoadMethod]
    private static void InitializeInstance() {
        Debug.Log("INIT");
        NodeDataCache[] ndc = Resources.FindObjectsOfTypeAll<NodeDataCache>();
        if (ndc == null || ndc.Length == 0) {
            Debug.LogWarning("No NodeDataCache found. Creating.");
            _instance = ScriptableObject.CreateInstance<NodeDataCache>();
            _instance.BuildCache();
        }
        else if (ndc.Length > 1) {
            Debug.LogWarning("Multiple NodeDataCaches found.");
        }
        _instance = ndc[0];
    }

    /// <summary> Return port data from cache </summary>
    public static void GetPorts(Node node, out List<NodePort> inputs, out List<NodePort> outputs) {
        if (_instance == null) InitializeInstance();

        System.Type nodeType = node.GetType();
        inputs = new List<NodePort>();
        outputs = new List<NodePort>();
        if (!_instance.portDataCache.ContainsKey(nodeType)) return;
        for (int i = 0; i < _instance.portDataCache[nodeType].Count; i++) {
            if (_instance.portDataCache[nodeType][i].direction == NodePort.IO.Input) inputs.Add(new NodePort(_instance.portDataCache[nodeType][i], node));
            else outputs.Add(new NodePort(_instance.portDataCache[nodeType][i], node));
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    private static void Init() {
        instance.BuildCache();
    }

    private void BuildCache() {
        System.Type baseType = typeof(Node);
        Assembly assembly = Assembly.GetAssembly(baseType);
        System.Type[] nodeTypes = assembly.GetTypes().Where(t =>
            !t.IsAbstract &&
            baseType.IsAssignableFrom(t)
            ).ToArray();
        portDataCache.Clear();

        for (int i = 0; i < nodeTypes.Length; i++) {
            CachePorts(nodeTypes[i]);
        }
    }

    private void CachePorts(System.Type nodeType) {
        List<NodePort> inputPorts = new List<NodePort>();
        List<NodePort> outputPorts = new List<NodePort>();

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
