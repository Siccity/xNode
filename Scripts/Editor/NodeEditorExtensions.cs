using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using XNode;

namespace XNodeEditor.Internal {
	/// <summary> Handles caching of custom editor classes and their target types. Accessible with GetEditor(Type type) </summary>
	public static class NodeEditorExtensions {
		/// <summary> Custom editors defined with [CustomNodeEditor] </summary>
		private static Dictionary<Type, Type> nodeEditorTypes;
		/// <summary> Custom editors defined with [CustomGraphEditor] </summary>
		private static Dictionary<Type, Type> graphEditorTypes;
		private static Dictionary<Object, INodeEditor> nodeEditors = new Dictionary<Object, INodeEditor>();
		private static Dictionary<Object, INodeGraphEditor> graphEditors = new Dictionary<Object, INodeGraphEditor>();

		public static INodeGraphEditor GetGraphEditor(this INodeGraph target, NodeEditorWindow window) {
			INodeGraphEditor graphEditor = GetEditor(target.Object, graphEditors);
			if (graphEditor.window != window) graphEditor.window = window;
			return graphEditor;
		}

		public static INodeEditor GetNodeEditor(this INode target) {
			INodeEditor nodeEditor = GetEditor(target.Object, nodeEditors);
			return nodeEditor;
		}

		private static T GetEditor<T>(UnityEngine.Object target, Dictionary<Object, T> editors) where T : class {
			if (target == null) return null;
			T tEditor;
			if (!editors.TryGetValue(target, out tEditor)) {
				Type editorType = GetEditorType(target.GetType());
				tEditor = Editor.CreateEditor(target, editorType) as T;
				editors.Add(target, tEditor);
			}
			Editor editor = tEditor as Editor;
			if (editor.target == null) editor.Initialize(new UnityEngine.Object[] { target });
			return tEditor;
		}

		private static Type GetEditorType(Type type) {
			if (type == null) return null;
			if (graphEditorTypes == null) graphEditorTypes = CacheCustomEditors<CustomNodeGraphEditorAttribute>(typeof(INodeGraphEditor));
			if (nodeEditorTypes == null) nodeEditorTypes = CacheCustomEditors<CustomNodeEditorAttribute>(typeof(INodeEditor));
			Type result;
			if (graphEditorTypes.TryGetValue(type, out result)) return result;
			//If type isn't found, try base type
			return GetEditorType(type.BaseType);
		}

		private static Dictionary<Type, Type> CacheCustomEditors<A>(Type editorInterface) where A : Attribute, INodeEditorAttrib {
			Dictionary<Type, Type> dict = new Dictionary<Type, Type>();

			//Get all classes deriving from editorInterface via reflection
			Type[] editors = XNodeEditor.NodeEditorWindow.GetDerivedTypes(editorInterface);
			for (int i = 0; i < editors.Length; i++) {
				if (editors[i].IsAbstract) continue;
				object[] attribs = editors[i].GetCustomAttributes(typeof(A), false);
				if (attribs == null || attribs.Length == 0) continue;
				A attrib = attribs[0] as A;
				dict.Add(attrib.GetInspectedType(), editors[i]);
			}
			return dict;
		}
	}

	public interface INodeEditorAttrib {
		Type GetInspectedType();
	}
}