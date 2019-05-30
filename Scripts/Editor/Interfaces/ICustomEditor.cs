using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
	/// <summary> 
	/// Workaround since c# doesn't support nested interfaces. 
	/// 
	/// Used with INodeEditor and INodeGraphEditor. 
	/// </summary>
	public interface ICustomEditor<T> {
		T Target { get; }
		SerializedObject SerializedObject { get; }
	}
}