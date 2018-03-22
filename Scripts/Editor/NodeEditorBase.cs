using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace XNodeEditor.Internal {
	/// <summary> Handles caching of custom editor classes and their target types. Accessible with GetEditor(Type type) </summary>
	public class NodeEditorBase<T, A, K> where A : Attribute, NodeEditorBase<T, A, K>.INodeEditorAttrib where T : NodeEditorBase<T,A,K> where K : ScriptableObject {
		/// <summary> Custom editors defined with [CustomNodeEditor] </summary>
		private static Dictionary<Type, T> editors;
		public K target;
		public SerializedObject serializedObject;

		public static T GetEditor(K target) {
			if (target == null) return null;
			Type type = target.GetType();
			T editor = GetEditor(type);
			editor.target = target;
			editor.serializedObject = new SerializedObject(target);
			return editor;
		}

		private static T GetEditor(Type type) {
			if (type == null) return null;
			if (editors == null) CacheCustomEditors();
			if (editors.ContainsKey(type)) return editors[type];
			//If type isn't found, try base type
			return GetEditor(type.BaseType);
		}

		private static void CacheCustomEditors() {
			editors = new Dictionary<Type, T>();

			//Get all classes deriving from NodeEditor via reflection
			Type[] nodeEditors = XNodeEditor.NodeEditorWindow.GetDerivedTypes(typeof(T));
			for (int i = 0; i < nodeEditors.Length; i++) {
				var attribs = nodeEditors[i].GetCustomAttributes(typeof(A), false);
				if (attribs == null || attribs.Length == 0) continue;
				if (nodeEditors[i].IsAbstract) continue;
				A attrib = attribs[0] as A;
				editors.Add(attrib.GetInspectedType(), Activator.CreateInstance(nodeEditors[i]) as T);
			}
		}

		public interface INodeEditorAttrib {
			Type GetInspectedType();
		}
	}
}