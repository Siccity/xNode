using UnityEngine;

namespace XNode.Examples.LogicToy {
	[NodeWidth(140)]
	public class PulseNode : LogicNode, ITimerTick {
		public float interval = 1f;
		[Output] public bool output;
		public override bool led { get { return output; } }

		private float timer;

		public void Tick(float deltaTime) {
			timer += deltaTime;
			if (!output && timer > interval) {
				timer -= interval;
				output = true;
				SendSignal(GetPort("output"));
			} else if (output) {
				output = false;
				SendSignal(GetPort("output"));
			}
		}

		/// <summary> This node can not receive signals, so this is not used </summary>
		protected override void OnInputChanged() { }

		public override object GetValue(NodePort port) {
			return output;
		}
	}
}