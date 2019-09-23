using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNode.Examples.LogicToy;

namespace XNodeEditor.Examples.LogicToy {
	[CustomNodeGraphEditor(typeof(LogicGraph))]
	public class LogicGraphEditor : NodeGraphEditor {
		private class NoodleTimer {
			public NodePort output, input;
			public double lastOnTime;
		}

		/// <summary> 
		/// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
		/// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "XNode.Examples.LogicToy") {
				return base.GetNodeMenuName(type).Replace("X Node/Examples/Logic Toy/", "");
			} else return null;
		}

		public override void OnGUI() {
			// Repaint each frame
			window.Repaint();
		}

		public override Color GetNoodleColor(NodePort output, NodePort input) {
			LogicNode node = output.node as LogicNode;
			LogicNodeEditor nodeEditor = NodeEditor.GetEditor(node, window) as LogicNodeEditor;
			Color baseColor = base.GetNoodleColor(output, input);

			float t = (float) (EditorApplication.timeSinceStartup - nodeEditor.lastOnTime);
			t *= 2f;
			return Color.Lerp(Color.yellow, baseColor, t);
		}
	}
}