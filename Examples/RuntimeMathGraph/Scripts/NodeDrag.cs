using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XNode.Examples.RuntimeMathNodes {
	public class NodeDrag : MonoBehaviour, IPointerDownHandler, IDragHandler {
		private Vector3 offset;
		private RuntimeMathNodes node;

		private void Awake() {
			node = GetComponentInParent<RuntimeMathNodes>();
		}

		public void OnDrag(PointerEventData eventData) {
			node.transform.localPosition = node.graph.scrollRect.content.InverseTransformPoint(eventData.position) - offset;
		}

		public void OnPointerDown(PointerEventData eventData) {
			Vector2 pointer = node.graph.scrollRect.content.InverseTransformPoint(eventData.position);
			Vector2 pos = node.transform.localPosition;
			offset = pointer - pos;
		}
	}
}