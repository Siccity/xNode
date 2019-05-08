using System;
using System.Collections.Generic;

namespace XNode {
	/// <summary> Used by advanced extensions that need to alter the base classes of NodeGraphs </summary>
	public interface INodeGraph {
		void MoveNodeToTop(INode node);
		IEnumerable<INode> Nodes { get; }
		INode AddNode(Type type);
		INode CopyNode(INode original);
		void RemoveNode(INode node);
	}
}