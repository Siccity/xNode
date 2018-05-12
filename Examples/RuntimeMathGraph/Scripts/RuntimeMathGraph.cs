using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes {
	public class RuntimeMathGraph : MonoBehaviour, IPointerClickHandler {
		[Header("Graph")]
		public MathGraph graph;
		[Header("Prefabs")]
		public XNode.Examples.RuntimeMathNodes.UGUIMathNode runtimeMathNodePrefab;
		public XNode.Examples.RuntimeMathNodes.UGUIVector runtimeVectorPrefab;
		public XNode.Examples.RuntimeMathNodes.UGUIDisplayValue runtimeDisplayValuePrefab;
		public XNode.Examples.RuntimeMathNodes.Connection runtimeConnectionPrefab;
		[Header("References")]
		public UGUIContextMenu graphContextMenu;
		public UGUIContextMenu nodeContextMenu;
		public UGUITooltip tooltip;

		public ScrollRect scrollRect { get; private set; }
		private List<UGUIMathBaseNode> nodes;

		private void Awake() {
			// Create a clone so we don't modify the original asset
			graph = graph.Copy() as MathGraph;
			scrollRect = GetComponentInChildren<ScrollRect>();
			graphContextMenu.onClickSpawn -= SpawnNode;
			graphContextMenu.onClickSpawn += SpawnNode;
		}

		private void Start() {
			SpawnGraph();
		}

		public void Refresh() {
			Clear();
			SpawnGraph();
		}

		public void Clear() {
			for (int i = nodes.Count - 1; i >= 0; i--) {
				Destroy(nodes[i].gameObject);
			}
			nodes.Clear();
		}

		public void SpawnGraph() {
			if (nodes != null) nodes.Clear();
			else nodes = new List<UGUIMathBaseNode>();

			for (int i = 0; i < graph.nodes.Count; i++) {
				Node node = graph.nodes[i];

				UGUIMathBaseNode runtimeNode = null;
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

		public UGUIMathBaseNode GetRuntimeNode(Node node) {
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i].node == node) {
					return nodes[i];
				} else { }
			}
			return null;
		}

		public void SpawnNode(Type type, Vector2 position) {
			Node node = graph.AddNode(type);
			node.name = type.Name;
			node.position = position;
			Refresh();
		}

		public void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Right)
				return;

			graphContextMenu.OpenAt(eventData.position);
		}
	}
}