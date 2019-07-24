using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using XNodeEditor.Internal;

namespace XNodeEditor {
    [InitializeOnLoad]
    public partial class NodeEditorWindow : EditorWindow {
        public static NodeEditorWindow current;

        /// <summary> Stores node.Positions for all nodePorts. </summary>
        public Dictionary<XNode.NodePort, Rect> portConnectionPoints { get { return _portConnectionPoints; } }
        private Dictionary<XNode.NodePort, Rect> _portConnectionPoints = new Dictionary<XNode.NodePort, Rect>();
        [SerializeField] private NodePortReference[] _references = new NodePortReference[0];
        [SerializeField] private Rect[] _rects = new Rect[0];

        [System.Serializable] private class NodePortReference {
            [SerializeField] private UnityEngine.Object _node;
            [SerializeField] private string _name;

            public NodePortReference(XNode.NodePort nodePort) {
                _node = nodePort.node;
                _name = nodePort.fieldName;
            }

            public XNode.NodePort GetNodePort() {
                if (_node == null) {
                    return null;
                }
                return (_node as XNode.INode).GetPort(_name);
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

        public Dictionary<XNode.INode, Vector2> nodeSizes { get { return _nodeSizes; } }
        private Dictionary<XNode.INode, Vector2> _nodeSizes = new Dictionary<XNode.INode, Vector2>();
        public XNode.INodeGraph graph;
        public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
        private Vector2 _panOffset;
        public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, NodeEditorPreferences.GetSettings().minZoom, NodeEditorPreferences.GetSettings().maxZoom); Repaint(); } }
        private float _zoom = 1;

        void OnFocus() {
            current = this;
            ValidateGraphEditor();
            if (graphEditor != null && NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        }

        [InitializeOnLoadMethod]
        private static void OnLoad() {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        /// <summary> Handle Selection Change events</summary>
        private static void OnSelectionChanged() {
            XNode.INodeGraph nodeGraph = Selection.activeObject as XNode.INodeGraph;
            // Open if double clicked non-asset graph (eg a runtime cloned graph)
            if (nodeGraph != null && !AssetDatabase.Contains(nodeGraph.Object)) {
                Open(nodeGraph);
            }
        }

        /// <summary> Make sure the graph editor is assigned and to the right object </summary>
        private void ValidateGraphEditor() {
            INodeGraphEditor graphEditor = graph.GetGraphEditor(this);
            if (this.graphEditor != graphEditor) {
                this.graphEditor = graphEditor;
                graphEditor.OnOpen();
            }
        }

        /// <summary> Create editor window </summary>
        public static NodeEditorWindow Init() {
            NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
            w.titleContent = new GUIContent("xNode");
            w.wantsMouseMove = true;
            w.Show();
            return w;
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

        public void SelectNode(XNode.INode node, bool add) {
            if (add) {
                List<Object> selection = new List<Object>(Selection.objects);
                selection.Add(node.Object);
                Selection.objects = selection.ToArray();
            } else Selection.objects = new Object[] { node.Object };
        }

        public void DeselectNode(XNode.INode node) {
            List<Object> selection = new List<Object>(Selection.objects);
            selection.Remove(node.Object);
            Selection.objects = selection.ToArray();
        }

        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line) {
            XNode.INodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as XNode.INodeGraph;
            if (nodeGraph != null) {
                Open(nodeGraph);
                return true;
            }
            return false;
        }

        /// <summary>Open the provided graph in the NodeEditor</summary>
        public static void Open(XNode.INodeGraph graph) {
            if (!graph) return;

            NodeEditorWindow w = GetWindow(typeof(NodeEditorWindow), false, "xNode", true) as NodeEditorWindow;
            w.wantsMouseMove = true;
            w.graph = graph;
        }

        /// <summary> Repaint all open NodeEditorWindows. </summary>
        public static void RepaintAll() {
            NodeEditorWindow[] windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
            for (int i = 0; i < windows.Length; i++) {
                windows[i].Repaint();
            }
        }
    }
}
