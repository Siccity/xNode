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
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Add Node " + type.ToString());
#endif
            Node node = ScriptableObject.CreateInstance(type) as Node;
            nodes.Add(node);
            node.graph = this;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
                node.name = UnityEditor.ObjectNames.NicifyVariableName(node.name);
            }
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual Node CopyNode(Node original) {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Copy Node");
#endif
            Node node = ScriptableObject.Instantiate(original);
            node.ClearConnections();
            nodes.Add(node);
            node.graph = this;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
                node.name = UnityEditor.ObjectNames.NicifyVariableName(node.name);
            }
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            return node;
        }

        /// <summary> Safely remove a node and all its connections </summary>
        /// <param name="node"></param>
        public void RemoveNode(Node node) {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Remove Node");
#endif
            node.ClearConnections();
            nodes.Remove(node);
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                DestroyImmediate(node, true);
            }
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary> Remove all nodes and connections from the graph </summary>
        public void Clear() {
            nodes.Clear();
        }
    }
}