using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XNodeEditor {
    [InitializeOnLoad, CustomNodeGraphWindow(typeof(XNode.NodeGraph))]
    public partial class NodeGraphWindow : EditorWindow {

        /// <summary> Stores node positions for all nodePorts. </summary>
        public Dictionary<XNode.NodePort, Rect> portConnectionPoints { get { return _portConnectionPoints; } }
        private Dictionary<XNode.NodePort, Rect> _portConnectionPoints = new Dictionary<XNode.NodePort, Rect>();
        [SerializeField] private NodePortReference[] _references = new NodePortReference[0];
        [SerializeField] private Rect[] _rects = new Rect[0];

        private Func<bool> isDocked {
            get {
                if (_isDocked == null) _isDocked = this.GetIsDockedDelegate();
                return _isDocked;
            }
        }
        private Func<bool> _isDocked;

        [System.Serializable] private class NodePortReference {
            [SerializeField] private XNode.Node _node;
            [SerializeField] private string _name;

            public NodePortReference(XNode.NodePort nodePort) {
                _node = nodePort.node;
                _name = nodePort.fieldName;
            }

            public XNode.NodePort GetNodePort() {
                if (_node == null) {
                    return null;
                }
                return _node.GetPort(_name);
            }
        }

        public static NodeGraphWindow current;

        public XNode.NodeGraph target;

        /// <summary> Are we currently renaming a node? </summary>
        protected bool isRenaming;

        /// <summary> Called when opened by NodeEditorWindow </summary>
        public virtual void OnOpen() { }

        public virtual void OnOverlayGUI() { }

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
            XNode.Node.CreateNodeMenuAttribute attrib;
            if (NodeEditorUtilities.GetAttrib(type, out attrib)) // Return custom path
                return attrib.menuName;
            else // Return generated path
                return ObjectNames.NicifyVariableName(type.ToString().Replace('.', '/'));
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(GenericMenu menu) {
            Vector2 pos = WindowToGridPosition(Event.current.mousePosition);
            for (int i = 0; i < NodeEditorReflection.nodeTypes.Length; i++) {
                Type type = NodeEditorReflection.nodeTypes[i];

                //Get node context menu path
                string path = GetNodeMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;

                menu.AddItem(new GUIContent(path), false, () => {
                    CreateNode(type, pos);
                });
            }
            menu.AddSeparator("");
            if (NodeGraphWindow.copyBuffer != null && NodeGraphWindow.copyBuffer.Length > 0) menu.AddItem(new GUIContent("Paste"), false, () => PasteNodes(pos));
            else menu.AddDisabledItem(new GUIContent("Paste"));
            menu.AddItem(new GUIContent("Preferences"), false, () => NodeEditorReflection.OpenPreferences());
            NodeEditorReflection.AddCustomContextMenuItems(menu, target);
        }

        public virtual Color GetPortColor(XNode.NodePort port) {
            return GetTypeColor(port.ValueType);
        }

        public virtual Color GetTypeColor(Type type) {
            return NodeEditorPreferences.GetTypeColor(type);
        }

        public virtual string GetPortTooltip(XNode.NodePort port) {
            Type portType = port.ValueType;
            string tooltip = "";
            tooltip = portType.PrettyName();
            if (port.IsOutput) {
                object obj = port.node.GetValue(port);
                tooltip += " = " + (obj != null ? obj.ToString() : "null");
            }
            return tooltip;
        }

        /// <summary> Deal with objects dropped into the graph through DragAndDrop </summary>
        public virtual void OnDropObjects(UnityEngine.Object[] objects) {
            Debug.Log("No OnDropItems override defined for " + GetType());
        }

        /// <summary> Create a node and save it in the graph asset </summary>
        public virtual void CreateNode(Type type, Vector2 position) {
            XNode.Node node = target.AddNode(type);
            node.position = position;
            if (node.name == null || node.name.Trim() == "") node.name = NodeEditorUtilities.NodeDefaultName(type);
            AssetDatabase.AddObjectToAsset(node, target);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            NodeGraphWindow.RepaintAll();
        }

        /// <summary> Creates a copy of the original node in the graph </summary>
        public XNode.Node CopyNode(XNode.Node original) {
            XNode.Node node = target.CopyNode(original);
            node.name = original.name;
            AssetDatabase.AddObjectToAsset(node, target);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary> Safely remove a node and all its connections. </summary>
        public virtual void RemoveNode(XNode.Node node) {
            target.RemoveNode(node);
            UnityEngine.Object.DestroyImmediate(node, true);
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeGraphWindowAttribute : Attribute, XNodeEditor.Internal.INodeEditorAttrib {
            private Type inspectedType;
            public string editorPrefsKey;
            /// <summary> Tells a NodeGraphEditor which Graph type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            /// <param name="editorPrefsKey">Define unique key for unique layout settings instance</param>
            public CustomNodeGraphWindowAttribute(Type inspectedType, string editorPrefsKey = "xNode.Settings") {
                this.inspectedType = inspectedType;
                this.editorPrefsKey = editorPrefsKey;
            }

            public Type GetInspectedType() {
                return inspectedType;
            }
        }

        private void OnDisable() {
            // Cache portConnectionPoints before serialization starts
            int count = portConnectionPoints.Count;
            _references = new NodePortReference[count];
            _rects = new Rect[count];
            int index = 0;
            foreach (var portConnectionPoint in portConnectionPoints) {
                _references[index] = new NodePortReference(portConnectionPoint.Key);
                _rects[index] = portConnectionPoint.Value;
                index++;
            }
        }

        private void OnEnable() {
            // Reload portConnectionPoints if there are any
            int length = _references.Length;
            if (length == _rects.Length) {
                for (int i = 0; i < length; i++) {
                    XNode.NodePort nodePort = _references[i].GetNodePort();
                    if (nodePort != null)
                        _portConnectionPoints.Add(nodePort, _rects[i]);
                }
            }
        }

        public Dictionary<XNode.Node, Vector2> nodeSizes { get { return _nodeSizes; } }
        private Dictionary<XNode.Node, Vector2> _nodeSizes = new Dictionary<XNode.Node, Vector2>();
        public XNode.NodeGraph graph;
        public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
        private Vector2 _panOffset;
        public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, NodeEditorPreferences.GetSettings().minZoom, NodeEditorPreferences.GetSettings().maxZoom); Repaint(); } }
        private float _zoom = 1;

        void OnFocus() {
            current = this;
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        }

        [InitializeOnLoadMethod]
        private static void OnLoad() {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        /// <summary> Handle Selection Change events</summary>
        private static void OnSelectionChanged() {
            XNode.NodeGraph nodeGraph = Selection.activeObject as XNode.NodeGraph;
            if (nodeGraph && !AssetDatabase.Contains(nodeGraph)) {
                Open(nodeGraph);
            }
        }

        /// <summary> Create editor window </summary>
        public static NodeGraphWindow Init() {
            NodeGraphWindow w = CreateInstance<NodeGraphWindow>();
            w.titleContent = new GUIContent("xNode");
            w.wantsMouseMove = true;
            w.Show();
            return w;
        }

        public void Save() {
            if (AssetDatabase.Contains(graph)) {
                EditorUtility.SetDirty(graph);
                if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            } else SaveAs();
        }

        public void SaveAs() {
            string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");
            if (string.IsNullOrEmpty(path)) return;
            else {
                XNode.NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<XNode.NodeGraph>(path);
                if (existingGraph != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(graph, path);
                EditorUtility.SetDirty(graph);
                if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
            }
        }

        private void DraggableWindow(int windowID) {
            GUI.DragWindow();
        }

        public Vector2 WindowToGridPosition(Vector2 windowPosition) {
            return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
        }

        public Vector2 GridToWindowPosition(Vector2 gridPosition) {
            return (position.size * 0.5f) + (panOffset / zoom) + (gridPosition / zoom);
        }

        public Rect GridToWindowRectNoClipped(Rect gridRect) {
            gridRect.position = GridToWindowPositionNoClipped(gridRect.position);
            return gridRect;
        }

        public Rect GridToWindowRect(Rect gridRect) {
            gridRect.position = GridToWindowPosition(gridRect.position);
            gridRect.size /= zoom;
            return gridRect;
        }

        public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition) {
            Vector2 center = position.size * 0.5f;
            // UI Sharpness complete fix - Round final offset not panOffset
            float xOffset = Mathf.Round(center.x * zoom + (panOffset.x + gridPosition.x));
            float yOffset = Mathf.Round(center.y * zoom + (panOffset.y + gridPosition.y));
            return new Vector2(xOffset, yOffset);
        }

        public void SelectNode(XNode.Node node, bool add) {
            if (add) {
                List<Object> selection = new List<Object>(Selection.objects);
                selection.Add(node);
                Selection.objects = selection.ToArray();
            } else Selection.objects = new Object[] { node };
        }

        public void DeselectNode(XNode.Node node) {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(node);
            Selection.objects = selection.ToArray();
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            XNode.NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as XNode.NodeGraph;
            if (nodeGraph != null) {
                Open(nodeGraph);
                return true;
            }
            return false;
        }

        /// <summary>Open the provided graph in the NodeEditor</summary>
        public static void Open(XNode.NodeGraph graph) {
            if (!graph) return;

            NodeGraphWindow w = GetWindow(typeof(NodeGraphWindow), false, "xNode", true) as NodeGraphWindow;
            w.wantsMouseMove = true;
            w.graph = graph;
        }

        /// <summary> Repaint all open NodeEditorWindows. </summary>
        public static void RepaintAll() {
            NodeGraphWindow[] windows = Resources.FindObjectsOfTypeAll<NodeGraphWindow>();
            for (int i = 0; i < windows.Length; i++) {
                windows[i].Repaint();
            }
        }
    }
}