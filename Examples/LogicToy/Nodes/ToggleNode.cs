using UnityEngine;

namespace XNode.Examples.LogicToy {
	[NodeWidth(140)]
	public class ToggleNode : LogicNode {
		private bool _on;
		[Input] public LogicNode input;
		[Output] public LogicNode output;
		public override bool on { get { return _on; } }

		/// <summary> This node has no inputs, so this does nothing </summary>
		protected override void OnTrigger() {
			_on = !_on;
			if (on) SendPulse(GetPort("output"));
		}
	}
}