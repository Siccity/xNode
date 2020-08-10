using System;
using System.Collections.Generic;
using UnityEngine;

namespace XNode
{
    [Serializable]
    public class PortConnection {
        [SerializeField] public string fieldName;
        [SerializeField] public string connectionLabel;
        [SerializeField] public Node node;
        public NodePort Port { get { return port != null ? port : port = GetPort(); } }

        [NonSerialized] protected NodePort port;
        /// <summary> Extra connection path points for organization </summary>
        [SerializeField] public List<Vector2> reroutePoints = new List<Vector2>();

        public PortConnection(NodePort port, string connectionLabel = null) {
            this.port = port;
            node = port.node;
            fieldName = port.fieldName;
            this.connectionLabel = connectionLabel;
        }

        /// <summary> Returns the port that this <see cref="PortConnection"/> points to </summary>
        private NodePort GetPort() {
            if (node == null || string.IsNullOrEmpty(fieldName)) return null;
            return node.GetPort(fieldName);
        }
    }
}