using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
	public interface INodeEditor {
		Editor editor { get; }
		void OnBodyGUI();
		void OnHeaderGUI();
		/// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
		void AddContextMenuItems(GenericMenu menu);
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class CustomNodeEditorAttribute : Attribute, XNodeEditor.Internal.INodeEditorAttrib {
		private Type inspectedType;
		/// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
		/// <param name="inspectedType">Type that this editor can edit</param>
		public CustomNodeEditorAttribute(Type inspectedType) {
			this.inspectedType = inspectedType;
		}

		public Type GetInspectedType() {
			return inspectedType;
		}
	}
}