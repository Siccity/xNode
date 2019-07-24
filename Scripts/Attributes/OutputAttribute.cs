using System;

namespace XNode {
	/// <summary> Mark a serializable field as an output port. You can access this through <see cref="GetOutputPort(string)"/> </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class OutputAttribute : Attribute {
		public ShowBackingValue backingValue;
		public ConnectionType connectionType;
		public bool dynamicPortList;

		/// <summary> Mark a serializable field as an output port. You can access this through <see cref="GetOutputPort(string)"/> </summary>
		/// <param name="backingValue">Should we display the backing value for this port as an editor field? </param>
		/// <param name="connectionType">Should we allow multiple connections? </param>
		/// <param name="dynamicPortList">If true, will display a reorderable list of outputs instead of a single port. Will automatically add and display values for lists and arrays </param>
		public OutputAttribute(ShowBackingValue backingValue = ShowBackingValue.Never, ConnectionType connectionType = ConnectionType.Multiple, bool dynamicPortList = false) {
			this.backingValue = backingValue;
			this.connectionType = connectionType;
			this.dynamicPortList = dynamicPortList;
		}
	}
}