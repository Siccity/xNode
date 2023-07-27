﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XNode {
    /// <summary> Precaches reflection data in editor so we won't have to do it runtime </summary>
    public static class NodeDataCache {
        private static PortDataCache portDataCache;
        private static Dictionary<System.Type, Dictionary<string, string>> formerlySerializedAsCache;
        private static Dictionary<System.Type, string> typeQualifiedNameCache;
        private static bool Initialized { get { return portDataCache != null; } }

        public static string GetTypeQualifiedName(System.Type type) {
            if(typeQualifiedNameCache == null)
            {
                typeQualifiedNameCache = new Dictionary<System.Type, string>();
            }

            string name;
            if (!typeQualifiedNameCache.TryGetValue(type, out name)) {
                name = type.AssemblyQualifiedName;
                typeQualifiedNameCache.Add(type, name);
            }
            return name;
        }

        /// <summary> Update static ports and dynamic ports managed by DynamicPortLists to reflect class fields. </summary>
        public static void UpdatePorts(Node node, Dictionary<string, NodePort> ports) {
            if (!Initialized)
            {
                BuildCache();
            }

            Dictionary<string, List<NodePort>> removedPorts = new Dictionary<string, List<NodePort>>();
            System.Type nodeType = node.GetType();

            Dictionary<string, string> formerlySerializedAs = null;
            if (formerlySerializedAsCache != null)
            {
                formerlySerializedAsCache.TryGetValue(nodeType, out formerlySerializedAs);
            }

            List<NodePort> dynamicListPorts = new List<NodePort>();

            Dictionary<string, NodePort> staticPorts;
            if (!portDataCache.TryGetValue(nodeType, out staticPorts)) {
                 staticPorts = new Dictionary<string, NodePort>();
            }            

            // Cleanup port dict - Remove nonexisting static ports - update static port types
            // AND update dynamic ports (albeit only those in lists) too, in order to enforce proper serialisation.
            // Loop through current node ports
            foreach (NodePort port in ports.Values.ToArray()) {
                // If port still exists, check it it has been changed
                NodePort staticPort;
                if (staticPorts.TryGetValue(port.fieldName, out staticPort)) {
                    // If port exists but with wrong settings, remove it. Re-add it later.
                    if (port.IsDynamic || port.direction != staticPort.direction || port.connectionType != staticPort.connectionType || port.typeConstraint != staticPort.typeConstraint) {
                        // If port is not dynamic and direction hasn't changed, add it to the list so we can try reconnecting the ports connections.
                        if (!port.IsDynamic && port.direction == staticPort.direction)
                        {
                            removedPorts.Add(port.fieldName, port.GetConnections());
                        }

                        port.ClearConnections();
                        ports.Remove(port.fieldName);
                    } else
                    {
                        port.ValueType = staticPort.ValueType;
                    }
                }
                // If port doesn't exist anymore, remove it
                else if (port.IsStatic) {
                    //See if the field is tagged with FormerlySerializedAs, if so add the port with its new field name to removedPorts
                    // so it can be reconnected in missing ports stage.
                    string newName = null;
                    if (formerlySerializedAs != null && formerlySerializedAs.TryGetValue(port.fieldName, out newName))
                    {
                        removedPorts.Add(newName, port.GetConnections());
                    }

                    port.ClearConnections();
                    ports.Remove(port.fieldName);
                }
                // If the port is dynamic and is managed by a dynamic port list, flag it for reference updates
                else if (IsDynamicListPort(port)) {
                    dynamicListPorts.Add(port);
                }
            }
            // Add missing ports
            foreach (NodePort staticPort in staticPorts.Values) {
                if (!ports.ContainsKey(staticPort.fieldName)) {
                    NodePort port = new NodePort(staticPort, node);
                    //If we just removed the port, try re-adding the connections
                    List<NodePort> reconnectConnections;
                    if (removedPorts.TryGetValue(staticPort.fieldName, out reconnectConnections)) {
                        for (var i = 0; i < reconnectConnections.Count; i++) {
                            NodePort connection = reconnectConnections[i];
                            if (connection == null)
                            {
                                continue;
                            }

                            // CAVEAT: Ports connected under special conditions defined in graphEditor.CanConnect overrides will not auto-connect.
                            // To fix this, this code would need to be moved to an editor script and call graphEditor.CanConnect instead of port.CanConnectTo.
                            // This is only a problem in the rare edge case where user is using non-standard CanConnect overrides and changes port type of an already connected port
                            if (port.CanConnectTo(connection))
                            {
                                port.Connect(connection);
                            }
                        }
                    }
                    ports.Add(staticPort.fieldName, port);
                }
            }

            // Finally, make sure dynamic list port settings correspond to the settings of their "backing port"
            foreach (NodePort listPort in dynamicListPorts) {
                // At this point we know that ports here are dynamic list ports
                // which have passed name/"backing port" checks, ergo we can proceed more safely.
                var backingPortName = listPort.fieldName.Substring(0, listPort.fieldName.IndexOf(' '));
                NodePort backingPort = staticPorts[backingPortName];

                // Update port constraints. Creating a new port instead will break the editor, mandating the need for setters.
                listPort.ValueType = GetBackingValueType(backingPort.ValueType);
                listPort.direction = backingPort.direction;
                listPort.connectionType = backingPort.connectionType;
                listPort.typeConstraint = backingPort.typeConstraint;
            }
        }

        /// <summary>
        /// Extracts the underlying types from arrays and lists, the only collections for dynamic port lists
        /// currently supported. If the given type is not applicable (i.e. if the dynamic list port was not
        /// defined as an array or a list), returns the given type itself.
        /// </summary>
        private static System.Type GetBackingValueType(System.Type portValType) {
            if (portValType.HasElementType) {
                return portValType.GetElementType();
            }
            if (portValType.IsGenericType && portValType.GetGenericTypeDefinition() == typeof(List<>)) {
                return portValType.GetGenericArguments()[0];
            }
            return portValType;
        }

        /// <summary>Returns true if the given port is in a dynamic port list.</summary>
        private static bool IsDynamicListPort(NodePort port) {
            // Ports flagged as "dynamicPortList = true" end up having a "backing port" and a name with an index, but we have
            // no guarantee that a dynamic port called "output 0" is an element in a list backed by a static "output" port.
            // Thus, we need to check for attributes... (but at least we don't need to look at all fields this time)
            var fieldNameParts = port.fieldName.Split(' ');
            if (fieldNameParts.Length != 2)
            {
                return false;
            }

            FieldInfo backingPortInfo = port.node.GetType().GetField(fieldNameParts[0]);
            if (backingPortInfo == null)
            {
                return false;
            }

            var attribs = backingPortInfo.GetCustomAttributes(true);
            return attribs.Any(x => {
                Node.InputAttribute inputAttribute = x as Node.InputAttribute;
                Node.OutputAttribute outputAttribute = x as Node.OutputAttribute;
                return inputAttribute != null && inputAttribute.dynamicPortList ||
                       outputAttribute != null && outputAttribute.dynamicPortList;
            });
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
                var assemblyName = assembly.GetName().Name;
                var index = assemblyName.IndexOf('.');
                if (index != -1)
                {
                    assemblyName = assemblyName.Substring(0, index);
                }

                switch (assemblyName) {
                    // The following assemblies, and sub-assemblies (eg. UnityEngine.UI) are skipped
                    case "UnityEditor":
                    case "UnityEngine":
                    case "Unity":
                    case "System":
                    case "mscorlib":
                    case "Microsoft":
                        continue;
                    default:
                        nodeTypes.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                        break;
                }
            }

            for (var i = 0; i < nodeTypes.Count; i++) {
                CachePorts(nodeTypes[i]);
            }
        }

        public static List<FieldInfo> GetNodeFields(System.Type nodeType) {
            List<System.Reflection.FieldInfo> fieldInfo = new List<System.Reflection.FieldInfo>(nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

            // GetFields doesnt return inherited private fields, so walk through base types and pick those up
            System.Type tempType = nodeType;
            while ((tempType = tempType.BaseType) != typeof(XNode.Node)) {
                FieldInfo[] parentFields = tempType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                for (var i = 0; i < parentFields.Length; i++) {
                    // Ensure that we do not already have a member with this type and name
                    FieldInfo parentField = parentFields[i];
                    if (fieldInfo.TrueForAll(x => x.Name != parentField.Name)) {
                        fieldInfo.Add(parentField);
                    }
                }
            }
            return fieldInfo;
        }

        private static void CachePorts(System.Type nodeType) {
            List<System.Reflection.FieldInfo> fieldInfo = GetNodeFields(nodeType);

            for (var i = 0; i < fieldInfo.Count; i++) {

                //Get InputAttribute and OutputAttribute
                var attribs = fieldInfo[i].GetCustomAttributes(true);
                Node.InputAttribute inputAttrib = attribs.FirstOrDefault(x => x is Node.InputAttribute) as Node.InputAttribute;
                Node.OutputAttribute outputAttrib = attribs.FirstOrDefault(x => x is Node.OutputAttribute) as Node.OutputAttribute;
                UnityEngine.Serialization.FormerlySerializedAsAttribute formerlySerializedAsAttribute = attribs.FirstOrDefault(x => x is UnityEngine.Serialization.FormerlySerializedAsAttribute) as UnityEngine.Serialization.FormerlySerializedAsAttribute;

                if (inputAttrib == null && outputAttrib == null)
                {
                    continue;
                }

                if (inputAttrib != null && outputAttrib != null)
                {
                    Debug.LogError("Field " + fieldInfo[i].Name + " of type " + nodeType.FullName + " cannot be both input and output.");
                }
                else {
                    if (!portDataCache.ContainsKey(nodeType))
                    {
                        portDataCache.Add(nodeType, new Dictionary<string, NodePort>());
                    }

                    NodePort port = new NodePort(fieldInfo[i]);
                     portDataCache[nodeType].Add(port.fieldName, port);
                }

                if (formerlySerializedAsAttribute != null) {
                    if (formerlySerializedAsCache == null)
                    {
                        formerlySerializedAsCache = new Dictionary<System.Type, Dictionary<string, string>>();
                    }

                    if (!formerlySerializedAsCache.ContainsKey(nodeType))
                    {
                        formerlySerializedAsCache.Add(nodeType, new Dictionary<string, string>());
                    }

                    if (formerlySerializedAsCache[nodeType].ContainsKey(formerlySerializedAsAttribute.oldName))
                    {
                        Debug.LogError("Another FormerlySerializedAs with value '" + formerlySerializedAsAttribute.oldName + "' already exist on this node.");
                    }
                    else
                    {
                        formerlySerializedAsCache[nodeType].Add(formerlySerializedAsAttribute.oldName, fieldInfo[i].Name);
                    }
                }
            }
        }

        [System.Serializable]
        private class PortDataCache : Dictionary<System.Type, Dictionary<string, NodePort>> { }
    }
}
