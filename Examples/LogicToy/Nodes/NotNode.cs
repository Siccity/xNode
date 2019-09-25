using System.Linq;
using UnityEngine;

namespace XNode.Examples.LogicToy {
	[NodeWidth(140)]
	public class NotNode : LogicNode {
		[Input] public bool input;
		[Output] public bool output = true;
		public override bool led { get { return output; } }

		protected override void OnInputChanged() {
			bool newInput = GetPort("input").GetInputValues<bool>().Any(x => x);

			if (input != newInput) {
				input = newInput;
				output = !newInput;
				SendSignal(GetPort("output"));
			}
		}

		public override object GetValue(NodePort port) {
			return output;
		}
	}
}