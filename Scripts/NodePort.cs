using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace XNode {
    [Serializable]
    public class NodePort : ISerializationCallbackReceiver {
        public enum IO { Input, Output }

        public int ConnectionCount { get { return connectionCache.Count; } }
        /// <summary> Return the first non-null connection </summary>
        public NodePort Connection {
            get {
                for (int i = 0; i < connectionCache.Count; i++) {
                    if (connectionCache[i] != null) return connectionCache[i];
                }
                return null;
            }
        }

        public IO direction { get { return _direction; } }
        public Node.ConnectionType connectionType { get { return _connectionType; } }
        public Node.TypeConstraint typeConstraint { get { return _typeConstraint; } }
        public ReadOnlyCollection<NodePort> Connections {
            get {
                VerifyConnectionCache();
                return _Connections;
            }
        }
        private ReadOnlyCollection<NodePort> _Connections;

        /// <summary> Is this port connected to anytihng? </summary>
        public bool IsConnected { get { return connectionCache.Count != 0; } }
        public bool IsInput { get { return direction == IO.Input; } }
        public bool IsOutput { get { return direction == IO.Output; } }

        public string fieldName { get { return _fieldName; } }
        public Node node { get { return _node; } }
        public bool IsDynamic { get { return _dynamic; } }
        public bool IsStatic { get { return !_dynamic; } }
        public Type ValueType {
            get {
                if (valueType == null && !string.IsNullOrEmpty(_typeQualifiedName)) valueType = Type.GetType(_typeQualifiedName, false);
                return valueType;
            }
            set {
                valueType = value;
                if (value != null) _typeQualifiedName = value.AssemblyQualifiedName;
            }
        }
        private Type valueType;

        [SerializeField] private string _fieldName;
        [SerializeField] private Node _node;
        [SerializeField] private string _typeQualifiedName;
        [SerializeField] private List<PortConnection> connections = new List<PortConnection>();
        [SerializeField] private IO _direction;
        [SerializeField] private Node.ConnectionType _connectionType;
        [SerializeField] private Node.TypeConstraint _typeConstraint;
        [SerializeField] private bool _dynamic;

        [NonSerialized] readonly private List<NodePort> connectionCache = new List<NodePort>();
        [NonSerialized] readonly private Dictionary<NodePort, List<Vector2>> rerouteCache = new Dictionary<NodePort, List<Vector2>>();
        [NonSerialized] private bool initializedCache;

        private NodePort() {
            connectionCache = new List<NodePort>();
            rerouteCache = new Dictionary<NodePort, List<Vector2>>();
        }

        /// <summary> Construct a static targetless nodeport. Used as a template. </summary>
        public NodePort(FieldInfo fieldInfo) {
            connectionCache = new List<NodePort>();
            rerouteCache = new Dictionary<NodePort, List<Vector2>>();
            _fieldName = fieldInfo.Name;
            ValueType = fieldInfo.FieldType;
            _dynamic = false;
            var attribs = fieldInfo.GetCustomAttributes(false);
            for (int i = 0; i < attribs.Length; i++) {
                if (attribs[i] is Node.InputAttribute) {
                    _direction = IO.Input;
                    _connectionType = (attribs[i] as Node.InputAttribute).connectionType;
                    _typeConstraint = (attribs[i] as Node.InputAttribute).typeConstraint;
                } else if (attribs[i] is Node.OutputAttribute) {
                    _direction = IO.Output;
                    _connectionType = (attribs[i] as Node.OutputAttribute).connectionType;
                }
            }
        }

        /// <summary> Copy a nodePort but assign it to another node. </summary>
        public NodePort(NodePort nodePort, Node node) {
            connectionCache = new List<NodePort>();
            rerouteCache = new Dictionary<NodePort, List<Vector2>>();
            _fieldName = nodePort._fieldName;
            ValueType = nodePort.valueType;
            _direction = nodePort.direction;
            _dynamic = nodePort._dynamic;
            _connectionType = nodePort._connectionType;
            _typeConstraint = nodePort._typeConstraint;
            _node = node;
        }

        /// <summary> Construct a dynamic port. Dynamic ports are not forgotten on reimport, and is ideal for runtime-created ports. </summary>
        public NodePort(string fieldName, Type type, IO direction, Node.ConnectionType connectionType, Node.TypeConstraint typeConstraint, Node node) {
            Debug.Log("Create");
            _fieldName = fieldName;
            this.ValueType = type;
            _direction = direction;
            _node = node;
            _dynamic = true;
            _connectionType = connectionType;
            _typeConstraint = typeConstraint;
        }

        private void VerifyConnectionCache() {
            if (!initializedCache) {
                connectionCache.Clear();
                rerouteCache.Clear();
                for (int i = 0; i < connections.Count; i++) {
                    NodePort port = connections[i].GetPort();
                    connectionCache.Add(port);
                    rerouteCache.Add(port, connections[i].reroutePoints);
                }
                _Connections = new ReadOnlyCollection<NodePort>(connectionCache);
                initializedCache = true;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if (initializedCache) {
                connections = new List<PortConnection>();
                for (int i = 0; i < connectionCache.Count; i++) {
                    List<Vector2> reroutes;
                    if (rerouteCache.TryGetValue(connectionCache[i], out reroutes)) {
                        connections.Add(new PortConnection(connectionCache[i], reroutes));
                    } else {
                        connections.Add(new PortConnection(connectionCache[i]));
                    }
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        /// <summary> Checks all connections for invalid references, and removes them. </summary>
        public void VerifyConnections() {
            VerifyConnectionCache();
            for (int i = connectionCache.Count - 1; i >= 0; i--) {
                if (connectionCache[i].node != null &&
                    !string.IsNullOrEmpty(connectionCache[i].fieldName) &&
                    connectionCache[i].node.GetPort(connectionCache[i].fieldName) != null)
                    continue;
                connectionCache.RemoveAt(i);
            }
        }

        /// <summary> Return the output value of this node through its parent nodes GetValue override method. </summary>
        /// <returns> <see cref="Node.GetValue(NodePort)"/> </returns>
        public object GetOutputValue() {
            if (direction == IO.Input) return null;
            return node.GetValue(this);
        }

        /// <summary> Return the output value of the first connected port. Returns null if none found or invalid.</summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public object GetInputValue() {
            NodePort connectedPort = Connection;
            if (connectedPort == null) return null;
            return connectedPort.GetOutputValue();
        }

        /// <summary> Return the output values of all connected ports. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public object[] GetInputValues() {
            VerifyConnectionCache();
            object[] objs = new object[ConnectionCount];
            for (int i = 0; i < ConnectionCount; i++) {
                NodePort connectedPort = connectionCache[i];
                if (connectedPort == null) { // if we happen to find a null port, remove it and look again
                    connectionCache.RemoveAt(i);
                    i--;
                    continue;
                }
                objs[i] = connectedPort.GetOutputValue();
            }
            return objs;
        }

        /// <summary> Return the output value of the first connected port. Returns null if none found or invalid. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public T GetInputValue<T>() {
            object obj = GetInputValue();
            return obj is T ? (T) obj : default(T);
        }

        /// <summary> Return the output values of all connected ports. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public T[] GetInputValues<T>() {
            object[] objs = GetInputValues();
            T[] ts = new T[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                if (objs[i] is T) ts[i] = (T) objs[i];
            }
            return ts;
        }

        /// <summary> Return true if port is connected and has a valid input. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public bool TryGetInputValue<T>(out T value) {
            object obj = GetInputValue();
            if (obj is T) {
                value = (T) obj;
                return true;
            } else {
                value = default(T);
                return false;
            }
        }

        /// <summary> Return the sum of all inputs. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public float GetInputSum(float fallback) {
            object[] objs = GetInputValues();
            if (objs.Length == 0) return fallback;
            float result = 0;
            for (int i = 0; i < objs.Length; i++) {
                if (objs[i] is float) result += (float) objs[i];
            }
            return result;
        }

        /// <summary> Return the sum of all inputs. </summary>
        /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
        public int GetInputSum(int fallback) {
            object[] objs = GetInputValues();
            if (objs.Length == 0) return fallback;
            int result = 0;
            for (int i = 0; i < objs.Length; i++) {
                if (objs[i] is int) result += (int) objs[i];
            }
            return result;
        }

        /// <summary> Connect this <see cref="NodePort"/> to another </summary>
        /// <param name="port">The <see cref="NodePort"/> to connect to</param>
        public void Connect(NodePort port) {
            VerifyConnectionCache();
            if (port == null) { Debug.LogWarning("Cannot connect to null port"); return; }
            if (port == this) { Debug.LogWarning("Cannot connect port to self."); return; }
            if (IsConnectedTo(port)) { Debug.LogWarning("Port already connected. "); return; }
            if (direction == port.direction) { Debug.LogWarning("Cannot connect two " + (direction == IO.Input ? "input" : "output") + " connections"); return; }
            if (port.connectionType == Node.ConnectionType.Override && port.ConnectionCount != 0) { port.ClearConnections(); }
            if (connectionType == Node.ConnectionType.Override && ConnectionCount != 0) { ClearConnections(); }
            connectionCache.Add(port);
            port.connectionCache.Add(this);
            node.OnCreateConnection(this, port);
            port.node.OnCreateConnection(this, port);
        }

        [Obsolete("Use Connections property instead")]
        public List<NodePort> GetConnections() {
            VerifyConnectionCache();
            List<NodePort> result = new List<NodePort>();
            for (int i = 0; i < connectionCache.Count; i++) {
                NodePort port = connectionCache[i];
                result.Add(port);
            }
            return result;
        }

        [Obsolete("Use Connections[i] instead")]
        public NodePort GetConnection(int i) {
            VerifyConnectionCache();
            return connectionCache[i];
        }

        [Obsolete("Use Connections.IndexOf(port) instead")]
        /// <summary> Get index of the connection connecting this and specified ports </summary>
        public int GetConnectionIndex(NodePort port) {
            VerifyConnectionCache();
            return connectionCache.IndexOf(port);
        }

        public bool IsConnectedTo(NodePort port) {
            VerifyConnectionCache();
            return connectionCache.Contains(port);
        }

        /// <summary> Returns true if this port can connect to specified port </summary>
        public bool CanConnectTo(NodePort port) {
            // Figure out which is input and which is output
            NodePort input = null, output = null;
            if (IsInput) input = this;
            else output = this;
            if (port.IsInput) input = port;
            else output = port;
            // If there isn't one of each, they can't connect
            if (input == null || output == null) return false;
            // Check type constraints
            if (input.typeConstraint == XNode.Node.TypeConstraint.Inherited && !input.ValueType.IsAssignableFrom(output.ValueType)) return false;
            if (input.typeConstraint == XNode.Node.TypeConstraint.Strict && input.ValueType != output.ValueType) return false;
            // Success
            return true;
        }

        /// <summary> Disconnect this port from another port </summary>
        public void Disconnect(NodePort port) {

            VerifyConnectionCache();
            // Remove this ports connection to the other
            for (int i = connectionCache.Count - 1; i >= 0; i--) {
                connectionCache.Remove(port);
            }
            if (port != null) {
                // Remove the other ports connection to this port
                for (int i = 0; i < port.connectionCache.Count; i++) {
                    port.connectionCache.Remove(this);
                }
            } else Debug.LogWarning("Trying to disconnect a null port");
            // Trigger OnRemoveConnection
            node.OnRemoveConnection(this);
            if (port != null) port.node.OnRemoveConnection(port);
        }

        /// <summary> Disconnect this port from another port </summary>
        public void Disconnect(int i) {
            Disconnect(connectionCache[i]);
        }

        /// <summary> Disconnect all ports from this port </summary>
        public void ClearConnections() {
            for (int i = connectionCache.Count - 1; i >= 0; i--) {
                Disconnect(connectionCache[i]);
            }
        }

        /// <summary> Get reroute points for a given connection. This is used for organization </summary>
        public List<Vector2> GetReroutePoints(NodePort port) {
            VerifyConnectionCache();
            if (!rerouteCache.ContainsKey(port)) {
                List<Vector2> reroutes = new List<Vector2>();
                rerouteCache.Add(port, reroutes);
            }
            return rerouteCache[port];
        }

        /// <summary> Get reroute points for a given connection. This is used for organization </summary>
        public List<Vector2> GetReroutePoints(int index) {
            return GetReroutePoints(connectionCache[index]);
        }

        /// <summary> Swap connections with another node </summary>
        public void SwapConnections(NodePort targetPort) {
            VerifyConnectionCache();
            int aConnectionCount = connectionCache.Count;
            int bConnectionCount = targetPort.connectionCache.Count;

            List<NodePort> portConnections = new List<NodePort>();
            List<NodePort> targetPortConnections = new List<NodePort>();

            // Cache port connections
            for (int i = 0; i < aConnectionCount; i++)
                portConnections.Add(connectionCache[i]);

            // Cache target port connections
            for (int i = 0; i < bConnectionCount; i++)
                targetPortConnections.Add(targetPort.connectionCache[i]);

            ClearConnections();
            targetPort.ClearConnections();

            // Add port connections to targetPort
            for (int i = 0; i < portConnections.Count; i++)
                targetPort.Connect(portConnections[i]);

            // Add target port connections to this one
            for (int i = 0; i < targetPortConnections.Count; i++)
                Connect(targetPortConnections[i]);

        }

        /// <summary> Copy all connections pointing to a node and add them to this one </summary>
        public void AddConnections(NodePort targetPort) {
            VerifyConnectionCache();
            int connectionCount = targetPort.ConnectionCount;
            for (int i = 0; i < connectionCount; i++) {
                NodePort otherPort = targetPort.connectionCache[i];
                Connect(otherPort);
            }
        }

        /// <summary> Move all connections pointing to this node, to another node </summary>
        public void MoveConnections(NodePort targetPort) {
            VerifyConnectionCache();
            int connectionCount = connectionCache.Count;

            // Add connections to target port
            for (int i = 0; i < connectionCount; i++) {
                NodePort otherPort = targetPort.connectionCache[i];
                Connect(otherPort);
            }
            ClearConnections();
        }

        /// <summary> Swap connected nodes from the old list with nodes from the new list </summary>
        public void Redirect(List<Node> oldNodes, List<Node> newNodes) {
            VerifyConnectionCache();
            foreach (NodePort port in connectionCache) {
                int index = oldNodes.IndexOf(port._node);
                if (index >= 0) port._node = newNodes[index];
            }
        }

        [Serializable]
        private class PortConnection {
            [SerializeField] private string fieldName;
            [SerializeField] private Node node;
            /// <summary> Extra connection path points for organization </summary>
            [SerializeField] public List<Vector2> reroutePoints = new List<Vector2>();

            public PortConnection(NodePort port) {
                node = port.node;
                fieldName = port.fieldName;
            }

            public PortConnection(NodePort port, List<Vector2> reroutePoints) {
                node = port.node;
                fieldName = port.fieldName;
                this.reroutePoints = reroutePoints;
            }

            public NodePort GetPort() {
                return node.GetPort(fieldName);
            }
        }
    }
}