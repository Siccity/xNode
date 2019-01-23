using System;
using System.Collections.Generic;
using UnityEngine;

namespace XNode {
    /// <summary> Base class for all node graphs </summary>
    [Serializable]
    public abstract class NodeGraph : ScriptableObject {

        /// <summary> All nodes in the graph. <para/>
        /// See: <see cref="AddNode{T}"/> </summary>
        [SerializeField] public List<Node> nodes = new List<Node>();
        /// <summary> All groups in the graph. <para/>
        /// See: <see cref="AddGroup"/> </summary>
        [SerializeField] public List<NodeGroup> groups = new List<NodeGroup>();

        /// <summary> Add a node to the graph by type </summary>
        public T AddNode<T>() where T : Node {
            return AddNode(typeof(T)) as T;
        }

        /// <summary> Add a node to the graph by type </summary>
        public virtual Node AddNode(Type type) {
            Node.graphHotfix = this;
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.graph = this;
            nodes.Add(node);
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual Node CopyNode(Node original) {
            Node.graphHotfix = this;
            Node node = ScriptableObject.Instantiate(original);
            node.graph = this;
            node.ClearConnections();
            nodes.Add(node);
            return node;
        }

        /// <summary> Safely remove a node and all its connections </summary>
        /// <param name="node"> The node to remove </param>
        public void RemoveNode(Node node) {
            node.ClearConnections();
            nodes.Remove(node);
            if (Application.isPlaying) Destroy(node);
        }

        /// <summary> Remove all nodes and connections from the graph </summary>
        public void Clear() {
            if (Application.isPlaying) {
                for (int i = 0; i < nodes.Count; i++) {
                    Destroy(nodes[i]);
                }
            }
            nodes.Clear();
        }

        /// <summary> Add a group to the graph</summary>
        public NodeGroup AddGroup() {
            NodeGroup group = ScriptableObject.CreateInstance<NodeGroup>();
            group.graph = this;
            groups.Add(group);
            return group;
        }

        /// <summary> Creates a copy of the group node in the graph </summary>
        public virtual NodeGroup CopyGroup(NodeGroup original) {
            NodeGroup group = ScriptableObject.Instantiate(original);
            group.graph = this;
            groups.Add(group);
            return group;
        }

        /// <summary> Safely remove a group </summary>
        public void RemoveGroup(NodeGroup group) {
            groups.Remove(group);
            if (Application.isPlaying) Destroy(group);
        }

        /// <summary> Create a new deep copy of this graph </summary>
        public XNode.NodeGraph Copy() {
            // Instantiate a new nodegraph instance
            NodeGraph graph = Instantiate(this);
            // Instantiate all nodes inside the graph
            for (int i = 0; i < nodes.Count; i++) {
                if (nodes[i] == null) continue;
                Node.graphHotfix = graph;
                Node node = Instantiate(nodes[i]) as Node;
                node.graph = graph;
                graph.nodes[i] = node;
            }

            // Instantiate all groups inside the graph
            for (int i = 0; i < groups.Count; i++) {
                if (groups[i] == null) continue;
                NodeGroup group = Instantiate(groups[i]) as NodeGroup;
                group.graph = graph;
                graph.groups[i] = group;
            }

            // Redirect all connections
            for (int i = 0; i < graph.nodes.Count; i++) {
                if (graph.nodes[i] == null) continue;
                foreach (NodePort port in graph.nodes[i].Ports) {
                    port.Redirect(nodes, graph.nodes);
                }
            }

            return graph;
        }

        private void OnDestroy() {
            // Remove all nodes prior to graph destruction
            Clear();
        }
    }
}