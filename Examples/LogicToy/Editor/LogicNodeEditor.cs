using UnityEditor;
using UnityEngine;
using XNode.Examples.LogicToy;

namespace XNodeEditor.Examples.LogicToy {
	[CustomNodeEditor(typeof(LogicNode))]
	public class LogicNodeEditor : NodeEditor {
		private LogicNode node;
		public double lastOnTime;

		public override void OnHeaderGUI() {
			// Initialization
			if (node == null) {
				node = target as LogicNode;
			}

			base.OnHeaderGUI();
			Rect dotRect = GUILayoutUtility.GetLastRect();
			dotRect.size = new Vector2(16, 16);
			dotRect.y += 6;

			if (node.on) {
				GUI.color = Color.green;
				lastOnTime = EditorApplication.timeSinceStartup;
			} else {
				float t = (float) (EditorApplication.timeSinceStartup - lastOnTime);
				t *= 2f;
				if (t < 1) {
					GUI.color = Color.Lerp(Color.green, Color.red, t);
				} else GUI.color = Color.red;
			}
			GUI.DrawTexture(dotRect, NodeEditorResources.dot);
			GUI.color = Color.white;
		}
	}
}