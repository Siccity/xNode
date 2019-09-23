namespace XNode.Examples.LogicToy {
	/// <summary> Base node for the LogicToy system </summary>
	public abstract class LogicNode : Node {
		public abstract bool on { get; }

		protected abstract void OnTrigger();

		public void SendPulse(NodePort port) {
			// Loop through port connections
			int connectionCount = port.ConnectionCount;
			for (int i = 0; i < connectionCount; i++) {
				NodePort connectedPort = port.GetConnection(i);

				// Get connected ports logic node
				LogicNode logicNode = connectedPort.node as LogicNode;

				// Trigger it
				if (logicNode != null) logicNode.OnTrigger();
			}
		}
	}
}