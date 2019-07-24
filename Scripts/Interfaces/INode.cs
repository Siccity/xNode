using System;
using System.Collections.Generic;
using UnityEngine;

namespace XNode {
    public interface INode {
        string Name { get; set; }
        INodeGraph Graph { get; }
        Vector2 Position { get; set; }
        UnityEngine.Object Object { get; }
        object GetValue(INodePort port);
        bool HasPort(string fieldName);
        INodePort GetPort(string fieldName);
        void UpdateStaticPorts();
        IEnumerable<INodePort> Ports { get; }
        IEnumerable<INodePort> Outputs { get; }
        IEnumerable<INodePort> Inputs { get; }
        IEnumerable<INodePort> DynamicPorts { get; }
        INodePort AddDynamicPort(Type type, XNode.IO direction, XNode.ConnectionType connectionType, XNode.TypeConstraint typeConstraint, string fieldName);
        void RemoveDynamicPort(string fieldName);
        INodePort GetInputPort(string fieldName);
        INodePort GetOutputPort(string fieldName);
        void OnCreateConnection(INodePort from, INodePort to);
        void OnRemoveConnection(INodePort port);
        void ClearConnections();
    }
}