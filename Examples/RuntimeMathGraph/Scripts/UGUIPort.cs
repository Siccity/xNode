using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode;

namespace XNode.Examples.RuntimeMathNodes {
	public class UGUIPort : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler {

		public string fieldName;
		[HideInInspector] public XNode.Node node;

		private NodePort port;
		private Connection tempConnection;
		private NodePort startPort;
		private UGUIPort tempHovered;
		private RuntimeMathGraph graph;
		private Vector2 startPos;
		private List<Connection> connections = new List<Connection>();

		void Start() {
			port = node.GetPort(fieldName);
			graph = GetComponentInParent<RuntimeMathGraph>();
			if (port.IsOutput && port.IsConnected) {
				for (int i = 0; i < port.ConnectionCount; i++) {
					AddConnection();
				}
			}
		}

		void Reset() {
			fieldName = name;
		}

		private void OnDestroy() {
			// Also destroy connections
			for (int i = connections.Count - 1; i >= 0; i--) {
				Destroy(connections[i].gameObject);
			}
			connections.Clear();
		}

		public void UpdateConnectionTransforms() {
			if (port.IsInput) return;

			while (connections.Count < port.ConnectionCount) AddConnection();
			while (connections.Count > port.ConnectionCount) {
				Destroy(connections[0].gameObject);
				connections.RemoveAt(0);
			}

			// Loop through connections
			for (int i = 0; i < port.ConnectionCount; i++) {
				NodePort other = port.GetConnection(i);
				UGUIMathBaseNode otherNode = graph.GetRuntimeNode(other.node);
				if (!otherNode) Debug.LogWarning(other.node.name + " node not found", this);
				Transform port2 = otherNode.GetPort(other.fieldName).transform;
				if (!port2) Debug.LogWarning(other.fieldName + " not found", this);
				connections[i].SetPosition(transform.position, port2.position);
			}
		}

		private void AddConnection() {
			Connection connection = Instantiate(graph.runtimeConnectionPrefab);
			connection.transform.SetParent(graph.scrollRect.content);
			connections.Add(connection);
		}

		public void OnBeginDrag(PointerEventData eventData) {
			if (port.IsOutput) {
				tempConnection = Instantiate(graph.runtimeConnectionPrefab);
				tempConnection.transform.SetParent(graph.scrollRect.content);
				tempConnection.SetPosition(transform.position, eventData.position);
				startPos = transform.position;
				startPort = port;
			} else {
				if (port.IsConnected) {
					NodePort output = port.Connection;
					Debug.Log("has " + port.ConnectionCount + " connections");
					Debug.Log(port.GetConnection(0));
					UGUIMathBaseNode otherNode = graph.GetRuntimeNode(output.node);
					UGUIPort otherUGUIPort = otherNode.GetPort(output.fieldName);
					Debug.Log("Disconnect");
					output.Disconnect(port);
					tempConnection = Instantiate(graph.runtimeConnectionPrefab);
					tempConnection.transform.SetParent(graph.scrollRect.content);
					tempConnection.SetPosition(otherUGUIPort.transform.position, eventData.position);
					startPos = otherUGUIPort.transform.position;
					startPort = otherUGUIPort.port;
					graph.GetRuntimeNode(node).UpdateGUI();
				}
			}
		}

		public void OnDrag(PointerEventData eventData) {
			if (tempConnection == null) return;
			UGUIPort otherPort = FindPortInStack(eventData.hovered);
			tempHovered = otherPort;
			tempConnection.SetPosition(startPos, eventData.position);
		}

		public void OnEndDrag(PointerEventData eventData) {
			if (tempConnection == null) return;
			if (tempHovered) {
				startPort.Connect(tempHovered.port);
				graph.GetRuntimeNode(tempHovered.node).UpdateGUI();
			}
			Destroy(tempConnection.gameObject);
		}

		public UGUIPort FindPortInStack(List<GameObject> stack) {
			for (int i = 0; i < stack.Count; i++) {
				UGUIPort port = stack[i].GetComponent<UGUIPort>();
				if (port) return port;
			}
			return null;
		}

		public void OnPointerEnter(PointerEventData eventData) {
			graph.tooltip.Show();
			object obj = node.GetInputValue<object>(port.fieldName, null);
			if (obj != null) graph.tooltip.label.text = obj.ToString();
			else graph.tooltip.label.text = "n/a";
		}

		public void OnPointerExit(PointerEventData eventData) {
			graph.tooltip.Hide();
		}
	}
}