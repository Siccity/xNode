using UnityEditor;
using UnityEngine;
using XNode.Examples.LogicToy;

namespace XNodeEditor.Examples.LogicToy {
	[CustomNodeEditor(typeof(PulseNode))]
	public class PulseNodeEditor : LogicNodeEditor {
		private PulseNode node;

		public override void OnBodyGUI() {
			// Initialization
			if (node == null) {
				node = target as PulseNode;
				lastOnTime = EditorApplication.timeSinceStartup;
			}

			// Timer
			if (EditorApplication.timeSinceStartup - lastOnTime > node.interval) {
				lastOnTime = EditorApplication.timeSinceStartup;
				node.FirePulse();
			}

			// Basic GUI
			base.OnBodyGUI();
		}
	}
}