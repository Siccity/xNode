using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor.Internal {
	/// <summary> Handles caching of custom editor classes and their target types. Accessible with GetEditor(Type type) </summary>
	public class NodeEditorBase<T, A, K> where A : Attribute, NodeEditorBase<T, A, K>.INodeEditorAttrib where T : NodeEditorBase<T, A, K> where K : ScriptableObject {
		/// <summary> Custom editors defined with [CustomNodeEditor] </summary>
		private static Dictionary<Type, T> editorTemplates;
		private static Dictionary<K, T> editors = new Dictionary<K, T>();
		public K target;
		public SerializedObject serializedObject;

		public static T GetEditor(K target) {
			if (target == null) return null;
			if (!editors.ContainsKey(target)) {
				Type type = target.GetType();
				T editor = GetEditor(type);
				editors.Add(target, Activator.CreateInstance(editor.GetType()) as T);
				editors[target].target = target;
				editors[target].serializedObject = new SerializedObject(target);
			}
			return editors[target];
		}

		private static T GetEditor(Type type) {
			if (type == null) return null;
			if (editorTemplates == null) CacheCustomEditors();
			if (editorTemplates.ContainsKey(type)) return editorTemplates[type];
			//If type isn't found, try base type
			return GetEditor(type.BaseType);
		}

		private static void CacheCustomEditors() {
			editorTemplates = new Dictionary<Type, T>();

			//Get all classes deriving from NodeEditor via reflection
			Type[] nodeEditors = XNodeEditor.NodeEditorWindow.GetDerivedTypes(typeof(T));
			for (int i = 0; i < nodeEditors.Length; i++) {
				var attribs = nodeEditors[i].GetCustomAttributes(typeof(A), false);
				if (attribs == null || attribs.Length == 0) continue;
				if (nodeEditors[i].IsAbstract) continue;
				A attrib = attribs[0] as A;
				editorTemplates.Add(attrib.GetInspectedType(), Activator.CreateInstance(nodeEditors[i]) as T);
			}
		}

		public interface INodeEditorAttrib {
			Type GetInspectedType();
		}
	}
}