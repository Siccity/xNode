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
		private static Dictionary<Type, Type> editorTypes;
		private static Dictionary<K, T> editors = new Dictionary<K, T>();
		public K target;
		public SerializedObject serializedObject;

		public static T GetEditor(K target) {
			if (target == null) return null;
			if (!editors.ContainsKey(target)) {
				Type type = target.GetType();
				Type editorType = GetEditorType(type);
				editors.Add(target, Activator.CreateInstance(editorType) as T);
				editors[target].target = target;
				editors[target].serializedObject = new SerializedObject(target);
			}
			T editor = editors[target];
			if (editor.target == null) editor.target = target;
			if (editor.serializedObject == null) editor.serializedObject = new SerializedObject(target);
			return editor;
		}

		private static Type GetEditorType(Type type) {
			if (type == null) return null;
			if (editorTypes == null) CacheCustomEditors();
			if (editorTypes.ContainsKey(type)) return editorTypes[type];
			//If type isn't found, try base type
			return GetEditorType(type.BaseType);
		}

		private static void CacheCustomEditors() {
			editorTypes = new Dictionary<Type, Type>();

			//Get all classes deriving from NodeEditor via reflection
			Type[] nodeEditors = XNodeEditor.NodeEditorWindow.GetDerivedTypes(typeof(T));
			for (int i = 0; i < nodeEditors.Length; i++) {
				if (nodeEditors[i].IsAbstract) continue;
				var attribs = nodeEditors[i].GetCustomAttributes(typeof(A), false);
				if (attribs == null || attribs.Length == 0) continue;
				A attrib = attribs[0] as A;
				editorTypes.Add(attrib.GetInspectedType(), nodeEditors[i]);
			}
		}

		public interface INodeEditorAttrib {
			Type GetInspectedType();
		}
	}
}