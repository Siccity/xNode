using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode.Examples.StateGraph;

namespace XNodeEditor.Examples {
	[CustomNodeEditor(typeof(StateNode))]
	public class StateNodeEditor : NodeEditor {

		public override void OnHeaderGUI() {
			GUI.color = Color.white;
			StateNode node = target as StateNode;
			StateGraph graph = node.graph as StateGraph;
			if (graph.current == node) GUI.color = Color.blue;
			string title = target.name;
			GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
			GUI.color = Color.white;
		}

		public override void OnBodyGUI() {
			base.OnBodyGUI();
			StateNode node = target as StateNode;
			StateGraph graph = node.graph as StateGraph;
			if (GUILayout.Button("MoveNext Node")) node.MoveNext();
			if (GUILayout.Button("Continue Graph")) graph.Continue();
			if (GUILayout.Button("Set as current")) graph.current = node;
		}
	}
}