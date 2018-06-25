using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    /// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>

    [CustomNodeEditor(typeof(XNode.Node))]
    public class NodeEditor : XNodeEditor.Internal.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, XNode.Node> {

        /// <summary> Fires every whenever a node was modified through the editor </summary>
        public static Action<XNode.Node> onUpdateNode;
        public static Dictionary<XNode.NodePort, Vector2> portPositions;
        public static int renaming;

        /// <summary> Draws the node GUI.</summary>
        /// <param name="portPositions">Port handle positions need to be returned to the NodeEditorWindow </param>
        public void OnNodeGUI() {
            OnHeaderGUI();
            OnBodyGUI();
        }

        public virtual void OnHeaderGUI() {
            string title = target.name;
            if (renaming != 0 && Selection.Contains(target)) {
                int controlID = EditorGUIUtility.GetControlID(FocusType.Keyboard) + 1;
                if (renaming == 1) {
                    EditorGUIUtility.keyboardControl = controlID;
                    EditorGUIUtility.editingTextField = true;
                    renaming = 2;
                }
                target.name = EditorGUILayout.TextField(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
                if (!EditorGUIUtility.editingTextField) {
                    Rename(target.name);
                    renaming = 0;
                }
            } else {
                GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
            }
        }

        /// <summary> Draws standard field editors for all public fields </summary>
        public virtual void OnBodyGUI() {
            string[] excludes = { "m_Script", "graph", "position", "ports" };
            portPositions = new Dictionary<XNode.NodePort, Vector2>();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            EditorGUIUtility.labelWidth = 84;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                NodeEditorGUILayout.PropertyField(iterator, true);
            }
        }

        public virtual int GetWidth() {
            Type type = target.GetType();
            if (NodeEditorWindow.nodeWidth.ContainsKey(type)) return NodeEditorWindow.nodeWidth[type];
            else return 208;
        }

        public virtual Color GetTint() {
            Type type = target.GetType();
            if (NodeEditorWindow.nodeTint.ContainsKey(type)) return NodeEditorWindow.nodeTint[type];
            else return Color.white;
        }

        public void InitiateRename() {
            renaming = 1;
        }

        public void Rename(string newName) {
            target.name = newName;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeEditorAttribute : Attribute,
            XNodeEditor.Internal.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, XNode.Node>.INodeEditorAttrib {
            private Type inspectedType;
            /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="contextMenuName">Path to the node</param>
            public CustomNodeEditorAttribute(Type inspectedType) {
                this.inspectedType = inspectedType;
            }

            public Type GetInspectedType() {
                return inspectedType;
            }
        }
    }
}