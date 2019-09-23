namespace XNode.Examples.LogicToy {
	[NodeWidth(140)]
	public class PulseNode : LogicNode {
		public float interval = 1f;
		[Output] public LogicNode output;
		public override bool on { get { return false; } }

		/// <summary> Called from editor </summary>
		public void FirePulse() {
			SendPulse(GetPort("output"));
		}

		/// <summary> This node has no inputs, so this does nothing </summary>
		protected override void OnTrigger() { }
	}
}