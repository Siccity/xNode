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
            Dictionary<string, List<NodePort>> removedPorts = new Dictionary<string, List<NodePort>>();
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
                    if (port.IsDynamic || port.direction != staticPort.direction || port.connectionType != staticPort.connectionType || port.typeConstraint != staticPort.typeConstraint) {
                        // If port is not dynamic and direction hasn't changed, add it to the list so we can try reconnecting the ports connections.
                        if (!port.IsDynamic && port.direction == staticPort.direction) removedPorts.Add(port.fieldName, port.GetConnections());
                        port.ClearConnections();
                        ports.Remove(port.fieldName);
                    } else port.ValueType = staticPort.ValueType;
                }
                // If port doesn't exist anymore, remove it
                else if (port.IsStatic) {
                    port.ClearConnections();
                    ports.Remove(port.fieldName);
                }
            }
            // Add missing ports
            foreach (NodePort staticPort in staticPorts.Values) {
                if (!ports.ContainsKey(staticPort.fieldName)) {
                    NodePort port = new NodePort(staticPort, node);
                    //If we just removed the port, try re-adding the connections
                    List<NodePort> reconnectConnections;
                    if (removedPorts.TryGetValue(staticPort.fieldName, out reconnectConnections)) {
                        for (int i = 0; i < reconnectConnections.Count; i++) {
                            NodePort connection = reconnectConnections[i];
                            if (connection == null) continue;
                            if (port.CanConnectTo(connection)) port.Connect(connection);
                        }
                    }
                    ports.Add(staticPort.fieldName, port);
                }
            }
        }

        /// <summary> Cache node types </summary>
        private static void BuildCache() {
            portDataCache = new PortDataCache();
            System.Type baseType = typeof(Node);
            List<System.Type> nodeTypes = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            // Loop through assemblies and add node types to list
            foreach (Assembly assembly in assemblies) {
                // Skip certain dlls to improve performance
                string assemblyName = assembly.GetName().Name;
                int index = assemblyName.IndexOf('.');
                if (index != -1) assemblyName = assemblyName.Substring(0, index);
                switch (assemblyName) {
                    // The following assemblies, and sub-assemblies (eg. UnityEngine.UI) are skipped
                    case "UnityEditor":
                    case "UnityEngine":
                    case "System":
                    case "mscorlib":
                        continue;
                    default:
                        nodeTypes.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                        break;
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
