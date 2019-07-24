namespace XNode {
	/// <summary> Used by <see cref="InputAttribute"/> and <see cref="OutputAttribute"/> to determine when to display the field value associated with a <see cref="NodePort"/> </summary>
	public enum ShowBackingValue {
		/// <summary> Never show the backing value </summary>
		Never,
		/// <summary> Show the backing value only when the port does not have any active connections </summary>
		Unconnected,
		/// <summary> Always show the backing value </summary>
		Always
	}

	public enum ConnectionType {
		/// <summary> Allow multiple connections</summary>
		Multiple,
		/// <summary> always override the current connection </summary>
		Override
	}

	/// <summary> Tells which types of input to allow </summary>
	public enum TypeConstraint {
		/// <summary> Allow all types of input</summary>
		None,
		/// <summary> Allow similar and inherited types </summary>
		Inherited,
		/// <summary> Allow only similar types </summary>
		Strict
	}
}