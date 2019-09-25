using System;
using UnityEngine;

namespace XNode.Examples.LogicToy {
	/// <summary> Base node for the LogicToy system </summary>
	public abstract class LogicNode : Node {
		public Action onStateChange;
		public abstract bool led { get; }

		public void SendSignal(NodePort output) {
			// Loop through port connections
			int connectionCount = output.ConnectionCount;
			for (int i = 0; i < connectionCount; i++) {
				NodePort connectedPort = output.GetConnection(i);

				// Get connected ports logic node
				LogicNode connectedNode = connectedPort.node as LogicNode;

				// Trigger it
				if (connectedNode != null) connectedNode.OnInputChanged();
			}
			if (onStateChange != null) onStateChange();
		}

		protected abstract void OnInputChanged();

		public override void OnCreateConnection(NodePort from, NodePort to) {
			OnInputChanged();
		}
	}
}