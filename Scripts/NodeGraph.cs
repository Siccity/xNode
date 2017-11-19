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

        /// <summary> Add a node to the graph by type </summary>
        public T AddNode<T>() where T : Node {
            return AddNode(typeof(T)) as T;
        }

        /// <summary> Add a node to the graph by type </summary>
        public virtual Node AddNode(Type type) {
            Node node = ScriptableObject.CreateInstance(type) as Node;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
            nodes.Add(node);
            node.graph = this;
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual Node CopyNode(Node original) {
            Node node = ScriptableObject.Instantiate(original);
            node.ClearConnections();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
            nodes.Add(node);
            node.graph = this;
            return node;
        }

        /// <summary> Safely remove a node and all its connections </summary>
        /// <param name="node"></param>
        public void RemoveNode(Node node) {
            node.ClearConnections();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                DestroyImmediate(node, true);
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
            nodes.Remove(node);
        }

        /// <summary> Remove all nodes and connections from the graph </summary>
        public void Clear() {
            nodes.Clear();
        }
    }
}