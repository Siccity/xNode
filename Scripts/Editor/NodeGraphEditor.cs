using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> Base class to derive custom NodeGraph editors from. Use this to override how graphs are drawn in the editor. </summary>
    [CustomNodeGraphEditor(typeof(XNode.NodeGraph))]
    public class NodeGraphEditor : Editor, INodeGraphEditor {

        public NodeEditorWindow window;

        [Obsolete("Use window.position instead")]
        public Rect position { get { return window.position; } set { window.position = value; } }

#region Interface implementation
        NodeEditorWindow INodeGraphEditor.window { get { return window; } set { window = value; } }
        Editor INodeGraphEditor.editor { get { return this; } }
#endregion

        /// <summary> Are we currently renaming a node? </summary>
        protected bool isRenaming;

        public virtual void OnGUI() { }

        /// <summary> Called when opened by NodeEditorWindow </summary>
        public virtual void OnOpen() { }

        public virtual Texture2D GetGridTexture() {
            return NodeEditorPreferences.GetSettings().gridTexture;
        }

        public virtual Texture2D GetSecondaryGridTexture() {
            return NodeEditorPreferences.GetSettings().crossTexture;
        }

        /// <summary> Return default settings for this graph type. This is the settings the user will load if no previous settings have been saved. </summary>
        public virtual NodeEditorPreferences.Settings GetDefaultPreferences() {
            return new NodeEditorPreferences.Settings();
        }

        /// <summary> Returns context node menu path. Null or empty strings for hidden nodes. </summary>
        public virtual string GetNodeMenuName(Type type) {
            //Check if type has the CreateNodeMenuAttribute
            XNode.CreateNodeMenuAttribute attrib;
            if (NodeEditorUtilities.GetAttrib(type, out attrib)) // Return custom path
                return attrib.menuName;
            else // Return generated path
                return ObjectNames.NicifyVariableName(type.ToString().Replace('.', '/'));
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(GenericMenu menu) {
            Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
            for (int i = 0; i < NodeEditorWindow.nodeTypes.Length; i++) {
                Type type = NodeEditorWindow.nodeTypes[i];

                //Get node context menu path
                string path = GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;

                menu.AddItem(new GUIContent(path), false, () => {
                    CreateNode(type, pos);
                });
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Preferences"), false, () => NodeEditorWindow.OpenPreferences());
            NodeEditorWindow.AddCustomContextMenuItems(menu, target);
        }

        public virtual Color GetPortColor(XNode.NodePort port) {
            return GetTypeColor(port.ValueType);
        }

        public virtual Color GetTypeColor(Type type) {
            return NodeEditorPreferences.GetTypeColor(type);
        }

        /// <summary> Create a node and save it in the graph asset </summary>
        public virtual XNode.INode CreateNode(Type type, Vector2 position) {
            XNode.INode node = ((INodeGraph) target).AddNode(type);
            node.Position = position;
            if (string.IsNullOrEmpty(node.Name)) {
                string typeName = type.Name;
                if (typeName.EndsWith("Node")) typeName = typeName.Substring(0, typeName.LastIndexOf("Node"));
                node.Name = UnityEditor.ObjectNames.NicifyVariableName(typeName);
            }
            if (node is ScriptableObject) AssetDatabase.AddObjectToAsset(node as UnityEngine.Object, target as UnityEngine.Object);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            NodeEditorWindow.RepaintAll();
            return node;
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public virtual XNode.INode CopyNode(XNode.INode original) {
            XNode.INode node = ((INodeGraph) target).CopyNode(original);
            node.Name = original.Name;
            if (node is ScriptableObject) AssetDatabase.AddObjectToAsset(node as UnityEngine.Object, target as UnityEngine.Object);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary> Safely remove a node and all its connections. </summary>
        public virtual void RemoveNode(XNode.INode node) {
            ((INodeGraph) target).RemoveNode(node);
            UnityEngine.Object.DestroyImmediate(node as UnityEngine.Object, true);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
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
}