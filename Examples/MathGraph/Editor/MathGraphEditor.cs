using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode.Examples;

namespace XNodeEditor.Examples {
	[CustomNodeGraphEditor(typeof(MathGraph))]
	public class MathGraphEditor : NodeGraphEditor {

		/// <summary> 
		/// Overriding GetNodePath lets you control if and how nodes are categorized.
	    /// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
		/// </summary>
		public override string GetNodePath(System.Type type) {
			if (type.Namespace == "XNode.Examples.MathNodes") {
				return base.GetNodePath(type).Replace("X Node/Examples/Math Nodes/", "");
			} else return null;
		}
	}
}