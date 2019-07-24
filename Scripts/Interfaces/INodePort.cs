using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode {
	public enum IO { Input, Output }

	public interface INodePort {
		string fieldName { get; }
		INode node { get; }
		List<INodePort> GetConnections();
		IO direction { get; }
		ConnectionType connectionType { get; }
		TypeConstraint typeConstraint { get; }
		bool dynamic { get; }
	}
}