using UnityEditor;
using UnityEngine;
using XNode;
using XNode.Examples.LogicToy;

namespace XNodeEditor.Examples.LogicToy {
	[CustomNodeEditor(typeof(LogicNode))]
	public class LogicNodeEditor : NodeEditor {
		private LogicNode node;
		private LogicGraphEditor graphEditor;

		public override void OnHeaderGUI() {
			// Initialization
			if (node == null) {
				node = target as LogicNode;
				graphEditor = NodeGraphEditor.GetEditor(target.graph, window) as LogicGraphEditor;
			}

			base.OnHeaderGUI();
			Rect dotRect = GUILayoutUtility.GetLastRect();
			dotRect.size = new Vector2(16, 16);
			dotRect.y += 6;

			GUI.color = graphEditor.GetLerpColor(Color.red, Color.green, node, node.led);
			GUI.DrawTexture(dotRect, NodeEditorResources.dot);
			GUI.color = Color.white;
		}
	}
}