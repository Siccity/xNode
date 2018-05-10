using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes {
	public class RuntimeMathGraph : MonoBehaviour {
		[Header("Graph")]
		public MathGraph graph;
		[Header("Prefabs")]
		public XNode.Examples.RuntimeMathNodes.MathNode runtimeMathNodePrefab;
		public XNode.Examples.RuntimeMathNodes.Vector runtimeVectorPrefab;
		public XNode.Examples.RuntimeMathNodes.DisplayValue runtimeDisplayValuePrefab;
		public XNode.Examples.RuntimeMathNodes.Connection runtimeConnectionPrefab;

		public ScrollRect scrollRect { get; private set; }
		private List<RuntimeMathNodes> nodes;

		private void Awake() {
			scrollRect = GetComponentInChildren<ScrollRect>();
		}

		private void Start() {
			SpawnGraph();
		}

		public void SpawnGraph() {
			if (nodes != null) nodes.Clear();
			else nodes = new List<RuntimeMathNodes>();

			for (int i = 0; i < graph.nodes.Count; i++) {
				Node node = graph.nodes[i];

				RuntimeMathNodes runtimeNode = null;
				if (node is XNode.Examples.MathNodes.MathNode) {
					runtimeNode = Instantiate(runtimeMathNodePrefab);
				} else if (node is XNode.Examples.MathNodes.Vector) {
					runtimeNode = Instantiate(runtimeVectorPrefab);
				} else if (node is XNode.Examples.MathNodes.DisplayValue) {
					runtimeNode = Instantiate(runtimeDisplayValuePrefab);
				}
				runtimeNode.transform.SetParent(scrollRect.content);
				runtimeNode.node = node;
				runtimeNode.graph = this;
				nodes.Add(runtimeNode);
			}
		}

		public RuntimeMathNodes GetRuntimeNode(Node node) {
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i].node == node) {
					return nodes[i];
				} else {
				}
			}
			return null;
		}
	}
}