using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.Examples.StateGraph;
using XNodeEditor;

namespace XNodeEditor.Examples {
	[CustomNodeGraphEditor(typeof(StateGraph))]
	public class StateGraphEditor : NodeGraphEditor {

		/// <summary> 
		/// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
	    /// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodeMenuName(System.Type type) {
			if (type.Namespace == "XNode.Examples.StateGraph") {
				return base.GetNodeMenuName(type).Replace("X Node/Examples/State Graph/", "");
			} else return null;
		}
	}
}