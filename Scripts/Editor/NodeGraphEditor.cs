using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> Base class to derive custom Node Graph editors from. Use this to override how graphs are drawn in the editor. </summary>
    [CustomNodeGraphEditor(typeof(NodeGraph))]
    public class NodeGraphEditor : XNodeInternal.NodeEditorBase<NodeGraphEditor, NodeGraphEditor.CustomNodeGraphEditorAttribute> {
        /// <summary> Custom node editors defined with [CustomNodeGraphEditor] </summary>
        [NonSerialized] private static Dictionary<Type, NodeGraphEditor> editors;

        public NodeGraph target;
        public SerializedObject serializedObject;

        public virtual Texture2D GetGridTexture() {
            return NodeEditorPreferences.gridTexture;
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeGraphEditorAttribute : Attribute,
        INodeEditorAttrib {
            private Type inspectedType;
            /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="contextMenuName">Path to the node</param>
            public CustomNodeGraphEditorAttribute(Type inspectedType) {
                this.inspectedType = inspectedType;
            }

            public Type GetInspectedType() {
                return inspectedType;
            }
        }
    }
}