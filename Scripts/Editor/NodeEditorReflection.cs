using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> Contains reflection-related info </summary>
    public partial class NodeEditorWindow {
        /// <summary> Custom node editors defined with [CustomNodeInspector] </summary>
        [NonSerialized] private static Dictionary<Type, NodeEditor> customNodeEditor;
        /// <summary> Custom node tint colors defined with [NodeColor(r, g, b)] </summary>
        public static Dictionary<Type, Color> nodeTint { get { return _nodeTint != null ? _nodeTint : _nodeTint = GetNodeTint(); } }
        [NonSerialized] private static Dictionary<Type, Color> _nodeTint;
        /// <summary> All available node types </summary>
        public static Type[] nodeTypes { get { return _nodeTypes != null ? _nodeTypes : _nodeTypes = GetNodeTypes(); } }
        [NonSerialized] private static Type[] _nodeTypes = null;

        public static NodeEditor GetNodeEditor(Type node) {
            if (customNodeEditor == null) CacheCustomNodeEditors();
            if (customNodeEditor.ContainsKey(node)) return customNodeEditor[node];
            return customNodeEditor[typeof(Node)];
        }

        public static Type[] GetNodeTypes() {
            //Get all classes deriving from Node via reflection
            return GetDerivedTypes(typeof(Node));
        }

        public static void CacheCustomNodeEditors() {
            customNodeEditor = new Dictionary<Type, NodeEditor>();
            customNodeEditor.Add(typeof(Node), new NodeEditor());
            //Get all classes deriving from NodeEditor via reflection
            Type[] nodeEditors = GetDerivedTypes(typeof(NodeEditor));
            for (int i = 0; i < nodeEditors.Length; i++) {
                var attribs = nodeEditors[i].GetCustomAttributes(typeof(CustomNodeEditorAttribute), false);
                if (attribs == null || attribs.Length == 0) continue;
                if (nodeEditors[i].IsAbstract) continue;
                CustomNodeEditorAttribute attrib = attribs[0] as CustomNodeEditorAttribute;
                customNodeEditor.Add(attrib.inspectedType, Activator.CreateInstance(nodeEditors[i]) as NodeEditor);
            }
        }

        public static Dictionary<Type, Color> GetNodeTint() {
            Dictionary<Type, Color> tints = new Dictionary<Type, Color>();
            for (int i = 0; i < nodeTypes.Length; i++) {
                var attribs = nodeTypes[i].GetCustomAttributes(typeof(Node.NodeTint), true);
                if (attribs == null || attribs.Length == 0) continue;
                Node.NodeTint attrib = attribs[0] as Node.NodeTint;
                tints.Add(nodeTypes[i], attrib.color);
            }
            return tints;
        }

        public static Type[] GetDerivedTypes(Type baseType) {
            //Get all classes deriving from baseType via reflection
            Assembly assembly = Assembly.GetAssembly(baseType);
            return assembly.GetTypes().Where(t =>
                !t.IsAbstract &&
                baseType.IsAssignableFrom(t)
            ).ToArray();
        }

        public static object ObjectFromType(Type type) {
            return Activator.CreateInstance(type);
        }

        public static object ObjectFromFieldName(object obj, string fieldName) {
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName);
            return fieldInfo.GetValue(obj);
        }
    }
}