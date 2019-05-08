using System;
using System.Collections.Generic;
using UnityEngine;

namespace XNode {
    public interface INode {
        string name { get; set; }
        INodeGraph Graph { get; }
        Vector2 Position { get; set; }
        object GetValue(NodePort port);
        bool HasPort(string fieldName);
        NodePort GetPort(string fieldName);
        void UpdateStaticPorts();
        IEnumerable<NodePort> Ports { get; }
        IEnumerable<NodePort> Outputs { get; }
        IEnumerable<NodePort> Inputs { get; }
        IEnumerable<NodePort> InstancePorts { get; }
        NodePort AddInstanceOutput(Type type, XNode.Node.ConnectionType connectionType = XNode.Node.ConnectionType.Multiple, string fieldName = null);
        NodePort AddInstanceInput(Type type, XNode.Node.ConnectionType connectionType = XNode.Node.ConnectionType.Multiple, string fieldName = null);
        NodePort GetInputPort(string fieldName);
        NodePort GetOutputPort(string fieldName);
        void OnCreateConnection(NodePort from, NodePort to);
        void OnRemoveConnection(NodePort port);
        void ClearConnections();
        void RemoveInstancePort(string fieldName);
    }
}