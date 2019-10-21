using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XNode.Examples.RuntimeMathNodes {
	public class UGUITooltip : MonoBehaviour {
		public CanvasGroup group;
		public Text label;
		private bool show;
		private RuntimeMathGraph graph;

		private void Awake() {
			graph = GetComponentInParent<RuntimeMathGraph>();
		}

		private void Start() {
			Hide();
		}

		private void Update() {
			if (show) UpdatePosition();
		}

		public void Show() {
			show = true;
			group.alpha = 1;
			UpdatePosition();
			transform.SetAsLastSibling();
		}

		public void Hide() {
			show = false;
			group.alpha = 0;
		}

		private void UpdatePosition() {
			Vector2 pos;
			RectTransform rect = graph.scrollRect.content.transform as RectTransform;
			Camera cam = graph.gameObject.GetComponentInParent<Canvas>().worldCamera;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, cam, out pos);
			transform.localPosition = pos;
		}
	}
}