using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

namespace XNode {
    /// <summary> Base class for all node graphs </summary>
    [Serializable]
    public abstract class NodeGraph : ScriptableObject {

        /// <summary> All nodes in the graph. <para/>
        /// See: <see cref="AddNode{T}"/> </summary>
        [SerializeField] public List<Node> nodes = new List<Node>();
        
        /// <summary> All variables in the graph. </summary>
        [SerializeField] public List<Variable> variables = new List<Variable>();

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
                node.name = UnityEditor.ObjectNames.NicifyVariableName(node.name);
            }
#endif
            nodes.Add(node);
            node.graph = this;
            node.Init();
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
                node.name = UnityEditor.ObjectNames.NicifyVariableName(node.name);
            }
#endif
            nodes.Add(node);
            node.graph = this;
            node.Init();
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

        /// <summary> Remove all nodes, connections and variables from the graph </summary>
        public void Clear() {
            nodes.Clear();
            variables.Clear();
        }

        /// <summary> Create a new deep copy of this graph </summary>
        public XNode.NodeGraph Copy() {
            // Instantiate a new nodegraph instance
            NodeGraph graph = Instantiate(this);
            // Instantiate all nodes inside the graph
            for (int i = 0; i < nodes.Count; i++) {
                Node node = Instantiate(nodes[i]) as Node;
                node.graph = graph;
                graph.nodes[i] = node;
                node.Init();
            }

            // Redirect all connections
            for (int i = 0; i < graph.nodes.Count; i++) {
                foreach (NodePort port in graph.nodes[i].Ports) {
                    port.Redirect(nodes, graph.nodes);
                }
            }

            return graph;
        }

        /// <summary> Add a variable to the graph </summary>
        /// <param name="newVariable">the new variable data</param>  
        /// <returns>the actual id used, avoiding duplicates</returns>     
        public string AddVariable(Variable newVariable)
        {
            newVariable.id = GetSafeId(newVariable.id);
            newVariable.typeString = GetSafeType(newVariable.typeString);
            variables.Add(newVariable);
            return newVariable.id;
        }

        /// <summary> Remove a variable from the graph </summary>
        public void RemoveVariable(string id)
        {
            variables.Remove(GetVariable(id));
        }

        /// <summary> Get a variable from the graph </summary>
        public Variable GetVariable(string id)
        {
            return variables.Find((Variable v) => v.id == id);
        }

        /// <summary> Get a duplication safe id </summary>
        public string GetSafeId(string id)
        {
            id = id.ToLower();
            id.Trim();
            var rx = new Regex(@"\s");
            rx.Replace(id, new MatchEvaluator((Match m) => "_"));
            
            var existingVariable = variables.Find((Variable v) => v.id == id);
            if (existingVariable == null)
                return id;

            int index = 1;
            
            while (variables.Find((Variable v) => v.id == id + "_" + index.ToString()) != null)
                index++;
            return id + "_" + index.ToString();
        }

        /// <summary> Get a variable safe type </summary>
        public static string GetSafeType(string type)
        {
			const string UE = "UnityEngine."; 
			if (type.Contains(UE))
			{
				type = type.Substring(UE.Length);
				type = type.Substring(0,1).ToLower() + type.Substring(1);
			}

            return type;
        }
    }
}