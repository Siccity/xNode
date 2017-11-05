using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XNode;

namespace XNodeEditor {
    /// <summary> Contains reflection-related info </summary>
    public partial class NodeEditorWindow {
        [NonSerialized] private static Dictionary<Type, NodeEditor> customNodeEditor;
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
    }
}