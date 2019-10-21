using System.Linq;
using UnityEngine;

namespace XNode.Examples.LogicToy {
	[NodeWidth(140), NodeTint(100, 70, 70)]
	public class AndNode : LogicNode {
		[Input, HideInInspector] public bool input;
		[Output, HideInInspector] public bool output;
		public override bool led { get { return output; } }

		protected override void OnInputChanged() {
			bool newInput = GetPort("input").GetInputValues<bool>().All(x => x);

			if (input != newInput) {
				input = newInput;
				output = newInput;
				SendSignal(GetPort("output"));
			}
		}

		public override object GetValue(NodePort port) {
			return output;
		}
	}
}