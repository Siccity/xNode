using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
	public interface INodeGraphEditor {
		INodeGraph target { get; }
		void OnGUI();
		void OnOpen();
		Texture2D GetGridTexture();
		Texture2D GetSecondaryGridTexture();
		/// <summary> Return default settings for this graph type. This is the settings the user will load if no previous settings have been saved. </summary>
		NodeEditorPreferences.Settings GetDefaultPreferences();
		/// <summary> Returns context node menu path. Null or empty strings for hidden nodes. </summary>
		string GetNodeMenuName(Type type);
		/// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
		void AddContextMenuItems(GenericMenu menu);
		Color GetPortColor(XNode.NodePort port);
		Color GetTypeColor(Type type);
		/// <summary> Create a node and save it in the graph asset </summary>
		XNode.INode CreateNode(Type type, Vector2 position);
		/// <summary> Creates a copy of the original node in the graph </summary>
		XNode.INode CopyNode(XNode.INode original);
		/// <summary> Safely remove a node and all its connections. </summary>
		void RemoveNode(XNode.INode node);
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class CustomNodeGraphEditorAttribute : Attribute,
		XNodeEditor.Internal.INodeEditorAttrib {
			private Type inspectedType;
			public string editorPrefsKey;
			/// <summary> Tells a NodeGraphEditor which Graph type it is an editor for </summary>
			/// <param name="inspectedType">Type that this editor can edit</param>
			/// <param name="editorPrefsKey">Define unique key for unique layout settings instance</param>
			public CustomNodeGraphEditorAttribute(Type inspectedType, string editorPrefsKey = "xNode.Settings") {
				this.inspectedType = inspectedType;
				this.editorPrefsKey = editorPrefsKey;
			}

			public Type GetInspectedType() {
				return inspectedType;
			}
		}
}