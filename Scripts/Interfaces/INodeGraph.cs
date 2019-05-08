using System;

namespace XNode {
	/// <summary> Used by advanced extensions that need to alter the base classes of NodeGraphs </summary>
	public interface INodeGraph {
		int NodesCount { get; }
		void MoveNodeToTop(INode node);
		INode[] GetNodes();
		INode AddNode(Type type);
		INode CopyNode(INode original);
		void RemoveNode(INode node);
	}
}