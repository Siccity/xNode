using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    /// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
    [CustomNodeEditor(typeof(XNode.Node))]
    public class NodeEditor : Editor, INodeEditor {
        public new XNode.Node target { get { return _target != null? _target : _target = base.target as XNode.Node; } }
        private XNode.Node _target;

        /// <summary> Fires whenever a node was modified through the editor </summary>
        public readonly static Dictionary<XNode.NodePort, Vector2> portPositions = new Dictionary<XNode.NodePort, Vector2>();

#region Interface implementation
        Editor INodeEditor.editor { get { return this; } }
#endregion

        public new virtual void OnHeaderGUI() {
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        }

        /// <summary> Draws standard field editors for all public fields </summary>
        public virtual void OnBodyGUI() {
            // Unity specifically requires this to save/update any serial object.
            // serializedObject.Update(); must go at the start of an inspector gui, and
            // serializedObject.ApplyModifiedProperties(); goes at the end.
            serializedObject.Update();
            string[] excludes = { "m_Script", "graph", "position", "ports" };

            // Iterate through serialized properties and draw them like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            EditorGUIUtility.labelWidth = 84;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                NodeEditorGUILayout.PropertyField(iterator, true);
            }

            // Iterate through dynamic ports and draw them in the order in which they are serialized
            foreach (XNode.NodePort dynamicPort in target.DynamicPorts) {
                if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
                NodeEditorGUILayout.PortField(dynamicPort);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public virtual int GetWidth() {
            Type type = target.GetType();
            int width;
            if (NodeEditorWindow.nodeWidth.TryGetValue(type, out width)) return width;
            else return 208;
        }

        public virtual Color GetTint() {
            Type type = target.GetType();
            Color color;
            if (NodeEditorWindow.nodeTint.TryGetValue(type, out color)) return color;
            else return Color.white;
        }

        public virtual GUIStyle GetBodyStyle() {
            return NodeEditorResources.styles.nodeBody;
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(GenericMenu menu) {
            // Actions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.INode) {
                XNode.INode node = Selection.activeObject as XNode.INode;
                menu.AddItem(new GUIContent("Move To Top"), false, () => node.Graph.MoveNodeToTop(node));
                menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);
            }

            // Add actions to any number of selected nodes
            menu.AddItem(new GUIContent("Duplicate"), false, NodeEditorWindow.current.DuplicateSelectedNodes);
            menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);

            // Custom sctions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.INode) {
                XNode.INode node = Selection.activeObject as XNode.INode;
                NodeEditorWindow.AddCustomContextMenuItems(menu, node);
            }
        }

        /// <summary> Rename the node asset. This will trigger a reimport of the node. </summary>
        public void Rename(string newName) {
            if (newName == null || newName.Trim() == "") newName = UnityEditor.ObjectNames.NicifyVariableName(target.GetType().Name);
            target.name = newName;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
        }
    }
}