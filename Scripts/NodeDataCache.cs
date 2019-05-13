using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XNode {
    /// <summary> Precaches reflection data in editor so we won't have to do it runtime </summary>
    public static class NodeDataCache {
        private static PortDataCache portDataCache;
        private static bool Initialized { get { return portDataCache != null; } }

        /// <summary> Update static ports to reflect class fields. </summary>
        public static void UpdatePorts(Node node, Dictionary<string, NodePort> ports) {
            if (!Initialized) BuildCache();

            Dictionary<string, NodePort> staticPorts = new Dictionary<string, NodePort>();
            System.Type nodeType = node.GetType();

            List<NodePort> typePortCache;
            if (portDataCache.TryGetValue(nodeType, out typePortCache)) {
                for (int i = 0; i < typePortCache.Count; i++) {
                    staticPorts.Add(typePortCache[i].fieldName, portDataCache[nodeType][i]);
                }
            }

            // Cleanup port dict - Remove nonexisting static ports - update static port types
            // Loop through current node ports
            foreach (NodePort port in ports.Values.ToList()) {
                // If port still exists, check it it has been changed
                NodePort staticPort;
                if (staticPorts.TryGetValue(port.fieldName, out staticPort)) {
                    // If port exists but with wrong settings, remove it. Re-add it later.
                    if (port.connectionType != staticPort.connectionType || port.IsDynamic || port.direction != staticPort.direction || port.typeConstraint != staticPort.typeConstraint) ports.Remove(port.fieldName);
                    else port.ValueType = staticPort.ValueType;
                }
                // If port doesn't exist anymore, remove it
                else if (port.IsStatic) ports.Remove(port.fieldName);
            }
            // Add missing ports
            foreach (NodePort staticPort in staticPorts.Values) {
                if (!ports.ContainsKey(staticPort.fieldName)) {
                    ports.Add(staticPort.fieldName, new NodePort(staticPort, node));
                }
            }
        }

        private static void BuildCache() {
            portDataCache = new PortDataCache();
            System.Type baseType = typeof(Node);
            List<System.Type> nodeTypes = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            Assembly selfAssembly = Assembly.GetAssembly(baseType);
            if (selfAssembly.FullName.StartsWith("Assembly-CSharp") && !selfAssembly.FullName.Contains("-firstpass")) {
                // If xNode is not used as a DLL, check only CSharp (fast)
                nodeTypes.AddRange(selfAssembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)));
            } else {
                // Else, check all relevant DDLs (slower)
                // ignore all unity related assemblies
                foreach (Assembly assembly in assemblies) {
                    if (assembly.FullName.StartsWith("Unity")) continue;
                    // unity created assemblies always have version 0.0.0
                    if (!assembly.FullName.Contains("Version=0.0.0")) continue;
                    nodeTypes.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                }
            }
            for (int i = 0; i < nodeTypes.Count; i++) {
                CachePorts(nodeTypes[i]);
            }
        }

        public static List<FieldInfo> GetNodeFields(System.Type nodeType) {
            List<System.Reflection.FieldInfo> fieldInfo = new List<System.Reflection.FieldInfo>(nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

            // GetFields doesnt return inherited private fields, so walk through base types and pick those up
            System.Type tempType = nodeType;
            while ((tempType = tempType.BaseType) != typeof(XNode.Node)) {
                fieldInfo.AddRange(tempType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
            }
            return fieldInfo;
        }

        private static void CachePorts(System.Type nodeType) {
            List<System.Reflection.FieldInfo> fieldInfo = GetNodeFields(nodeType);

            for (int i = 0; i < fieldInfo.Count; i++) {

                //Get InputAttribute and OutputAttribute
                object[] attribs = fieldInfo[i].GetCustomAttributes(true);
                Node.InputAttribute inputAttrib = attribs.FirstOrDefault(x => x is Node.InputAttribute) as Node.InputAttribute;
                Node.OutputAttribute outputAttrib = attribs.FirstOrDefault(x => x is Node.OutputAttribute) as Node.OutputAttribute;

                if (inputAttrib == null && outputAttrib == null) continue;

                if (inputAttrib != null && outputAttrib != null) Debug.LogError("Field " + fieldInfo[i].Name + " of type " + nodeType.FullName + " cannot be both input and output.");
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
}