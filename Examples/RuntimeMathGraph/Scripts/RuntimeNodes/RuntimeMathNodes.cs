using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes {
	public class RuntimeMathNodes : MonoBehaviour, IDragHandler {
		[HideInInspector] public Node node;
		[HideInInspector] public RuntimeMathGraph graph;
		public Text header;
		public List<Transform> ports;

		private List<Connection> connections = new List<Connection>();

		private void Start() {
			header.text = node.name;
			SetPosition(node.position);
			foreach (NodePort port in node.Outputs) {
				if (port.IsConnected) {
					for (int i = 0; i < port.ConnectionCount; i++) {
						Connection connection = Instantiate(graph.runtimeConnectionPrefab);
						connection.transform.SetParent(graph.scrollRect.content);
						connections.Add(connection);
					}
				}
			}
		}

		void LateUpdate() {
			UpdateConnectionTransforms();
		}

		public void UpdateConnectionTransforms() {
			int c = 0;
			foreach (NodePort port in node.Outputs) {
				Transform port1 = GetPort(port.fieldName);
				if (!port1) Debug.LogWarning(port.fieldName + " not found", this);
				for (int i = 0; i < port.ConnectionCount; i++) {
					NodePort other = port.GetConnection(i);
					Connection connection = connections[c++];
					RuntimeMathNodes otherNode = graph.GetRuntimeNode(other.node);
					if (!otherNode) Debug.LogWarning(other.node.name + " node not found", this);
					Transform port2 = otherNode.GetPort(other.fieldName);
					if (!port2) Debug.LogWarning(other.fieldName + " not found", this);
					connection.SetPosition(port1.position, port2.position);
				}
			}
		}

		public Transform GetPort(string name) {
			for (int i = 0; i < ports.Count; i++) {
				if (ports[i].name == name) return ports[i];
			}
			return null;
		}

		public void SetPosition(Vector2 pos) {
			pos.y = -pos.y;
			transform.localPosition = pos;
		}

		public void SetName(string name) {
			header.text = name;
		}

		public void OnDrag(PointerEventData eventData) {

		}
	}
}