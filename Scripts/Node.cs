using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XNode {
    /// <summary>
    /// Base class for all nodes
    /// </summary>
    /// <example>
    /// Classes extending this class will be considered as valid nodes by xNode.
    /// <code>
    /// [System.Serializable]
    /// public class Adder : Node {
    ///     [Input] public float a;
    ///     [Input] public float b;
    ///     [Output] public float result;
    ///
    ///     // GetValue should be overridden to return a value for any specified output port
    ///     public override object GetValue(NodePort port) {
    ///         return a + b;
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public abstract class Node : ScriptableObject, XNode.INode {
        /// <summary> Iterate over all ports on this node. </summary>
        public IEnumerable<NodePort> Ports { get { foreach (NodePort port in ports.Values) yield return port; } }
        /// <summary> Iterate over all outputs on this node. </summary>
        public IEnumerable<NodePort> Outputs { get { foreach (NodePort port in Ports) { if (port.IsOutput) yield return port; } } }
        /// <summary> Iterate over all inputs on this node. </summary>
        public IEnumerable<NodePort> Inputs { get { foreach (NodePort port in Ports) { if (port.IsInput) yield return port; } } }
        /// <summary> Iterate over all dynamic ports on this node. </summary>
        public IEnumerable<NodePort> DynamicPorts { get { foreach (NodePort port in Ports) { if (port.IsDynamic) yield return port; } } }
        /// <summary> Iterate over all dynamic outputs on this node. </summary>
        public IEnumerable<NodePort> DynamicOutputs { get { foreach (NodePort port in Ports) { if (port.IsDynamic && port.IsOutput) yield return port; } } }
        /// <summary> Iterate over all dynamic inputs on this node. </summary>
        public IEnumerable<NodePort> DynamicInputs { get { foreach (NodePort port in Ports) { if (port.IsDynamic && port.IsInput) yield return port; } } }
        /// <summary> Parent <see cref="NodeGraph"/> </summary>
        [SerializeField] public NodeGraph graph;
        /// <summary> Position on the <see cref="NodeGraph"/> </summary>
        [SerializeField] public Vector2 position;
        /// <summary> It is recommended not to modify these at hand. Instead, see <see cref="InputAttribute"/> and <see cref="OutputAttribute"/> </summary>
        [SerializeField] private NodePortDictionary ports = new NodePortDictionary();

        /// <summary> Used during node instantiation to fix null/misconfigured graph during OnEnable/Init. Set it before instantiating a node. Will automatically be unset during OnEnable </summary>
        public static NodeGraph graphHotfix;

        private void Awake() {
            if (graphHotfix != null) graph = graphHotfix;
            graphHotfix = null;
        }

        /// <summary> Update static ports to reflect class fields. This happens automatically on enable. </summary>
        public void UpdateStaticPorts() {
            NodeDataCache.UpdatePorts(this, ports);
        }

        /// <summary> Checks all connections for invalid references, and removes them. </summary>
        public void VerifyConnections() {
            foreach (NodePort port in Ports) port.VerifyConnections();
        }

#region Dynamic Ports
        /// <summary> Convenience function. </summary>
        /// <seealso cref="AddInstancePort"/>
        /// <seealso cref="AddInstanceOutput"/>
        public NodePort AddDynamicInput(Type type, ConnectionType connectionType = ConnectionType.Multiple, TypeConstraint typeConstraint = TypeConstraint.None, string fieldName = null) {
            return (NodePort) ((INode) this).AddDynamicPort(type, IO.Input, connectionType, typeConstraint, fieldName);
        }

        /// <summary> Convenience function. </summary>
        /// <seealso cref="AddInstancePort"/>
        /// <seealso cref="AddInstanceInput"/>
        public NodePort AddDynamicOutput(Type type, ConnectionType connectionType = ConnectionType.Multiple, TypeConstraint typeConstraint = TypeConstraint.None, string fieldName = null) {
            return (NodePort) ((INode) this).AddDynamicPort(type, IO.Output, connectionType, typeConstraint, fieldName);
        }

        /// <summary> Remove an dynamic port from the node </summary>
        public void RemoveDynamicPort(string fieldName) {
            NodePort dynamicPort = GetPort(fieldName);
            if (dynamicPort == null) throw new ArgumentException("port " + fieldName + " doesn't exist");
            RemoveDynamicPort(GetPort(fieldName));
        }

        /// <summary> Remove an dynamic port from the node </summary>
        public void RemoveDynamicPort(NodePort port) {
            if (port == null) throw new ArgumentNullException("port");
            else if (port.IsStatic) throw new ArgumentException("cannot remove static port");
            port.ClearConnections();
            ports.Remove(port.fieldName);
        }

        /// <summary> Removes all dynamic ports from the node </summary>
        [ContextMenu("Clear Dynamic Ports")]
        public void ClearDynamicPorts() {
            List<NodePort> dynamicPorts = new List<NodePort>(DynamicPorts);
            foreach (NodePort port in dynamicPorts) {
                RemoveDynamicPort(port);
            }
        }
#endregion

#region Ports
        /// <summary> Returns output port which matches fieldName </summary>
        public NodePort GetOutputPort(string fieldName) {
            NodePort port = GetPort(fieldName);
            if (port == null || port.direction != IO.Output) return null;
            else return port;
        }

        /// <summary> Returns input port which matches fieldName </summary>
        public NodePort GetInputPort(string fieldName) {
            NodePort port = GetPort(fieldName);
            if (port == null || port.direction != IO.Input) return null;
            else return port;
        }

        /// <summary> Returns port which matches fieldName </summary>
        public NodePort GetPort(string fieldName) {
            NodePort port;
            if (ports.TryGetValue(fieldName, out port)) return port;
            else return null;
        }

        public bool HasPort(string fieldName) {
            return ports.ContainsKey(fieldName);
        }
#endregion

#region Inputs/Outputs
        /// <summary> Return input value for a specified port. Returns fallback value if no ports are connected </summary>
        /// <param name="fieldName">Field name of requested input port</param>
        /// <param name="fallback">If no ports are connected, this value will be returned</param>
        public T GetInputValue<T>(string fieldName, T fallback = default(T)) {
            NodePort port = GetPort(fieldName);
            if (port != null && port.IsConnected) return port.GetInputValue<T>();
            else return fallback;
        }

        /// <summary> Return all input values for a specified port. Returns fallback value if no ports are connected </summary>
        /// <param name="fieldName">Field name of requested input port</param>
        /// <param name="fallback">If no ports are connected, this value will be returned</param>
        public T[] GetInputValues<T>(string fieldName, params T[] fallback) {
            NodePort port = GetPort(fieldName);
            if (port != null && port.IsConnected) return port.GetInputValues<T>();
            else return fallback;
        }

        /// <summary> Returns a value based on requested port output. Should be overridden in all derived nodes with outputs. </summary>
        /// <param name="port">The requested port.</param>
        public virtual object GetValue(NodePort port) {
            Debug.LogWarning("No GetValue(NodePort port) override defined for " + GetType());
            return null;
        }
#endregion

        /// <summary> Called after a connection between two <see cref="NodePort"/>s is created </summary>
        /// <param name="from">Output</param> <param name="to">Input</param>
        public virtual void OnCreateConnection(NodePort from, NodePort to) { }

        /// <summary> Called after a connection is removed from this port </summary>
        /// <param name="port">Output or Input</param>
        public virtual void OnRemoveConnection(NodePort port) { }

        /// <summary> Disconnect everything from this node </summary>
        public void ClearConnections() {
            foreach (NodePort port in Ports) port.ClearConnections();
        }

        [Serializable] private class NodePortDictionary : Dictionary<string, NodePort>, ISerializationCallbackReceiver {
            [SerializeField] private List<string> keys = new List<string>();
            [SerializeField] private List<NodePort> values = new List<NodePort>();

            public void OnBeforeSerialize() {
                keys.Clear();
                values.Clear();
                foreach (KeyValuePair<string, NodePort> pair in this) {
                    keys.Add(pair.Key);
                    values.Add(pair.Value);
                }
            }

            public void OnAfterDeserialize() {
                this.Clear();

                if (keys.Count != values.Count)
                    throw new System.Exception("there are " + keys.Count + " keys and " + values.Count + " values after deserialization. Make sure that both key and value types are serializable.");

                for (int i = 0; i < keys.Count; i++)
                    this.Add(keys[i], values[i]);
            }
        }

#region Interface implementation
        string INode.Name { get { return name; } set { name = value; } }
        INodeGraph INode.Graph { get { return graph; } }
        Vector2 INode.Position { get { return position; } set { position = value; } }
        Object INode.Object { get { return this; } }

        /// <summary> Add a dynamic, serialized port to this node. </summary>
        /// <seealso cref="AddDynamicInput"/>
        /// <seealso cref="AddDynamicOutput"/>
        INodePort INode.AddDynamicPort(Type type, IO direction, ConnectionType connectionType, TypeConstraint typeConstraint, string fieldName) {
            if (fieldName == null) {
                fieldName = "dynamicInput_0";
                int i = 0;
                while (HasPort(fieldName)) fieldName = "dynamicInput_" + (++i);
            } else if (HasPort(fieldName)) {
                Debug.LogWarning("Port '" + fieldName + "' already exists in " + name, this);
                return ports[fieldName];
            }
            NodePort port = new NodePort(fieldName, type, direction, connectionType, typeConstraint, this);
            ports.Add(fieldName, port);
            return port;
        }
#endregion
    }
}