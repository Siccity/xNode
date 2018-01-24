using System;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public partial class NodeEditorWindow {

        public static bool isPanning { get; private set; }
        public static Vector2[] dragOffset;

        //private bool IsDraggingNode { get { return draggedNode != null; } }
        private bool IsDraggingPort { get { return draggedOutput != null; } }
        private bool IsHoveringPort { get { return hoveredPort != null; } }
        private bool IsHoveringNode { get { return hoveredNode != null; } }
        public bool CanDragNodeHeader { get; private set; }
        public bool DidDragNodeHeader { get; private set; }
        private XNode.Node hoveredNode = null;

        //[NonSerialized] private XNode.Node draggedNode = null;
        [NonSerialized] private XNode.NodePort hoveredPort = null;
        [NonSerialized] private XNode.NodePort draggedOutput = null;
        [NonSerialized] private XNode.NodePort draggedOutputTarget = null;
        private Rect nodeRects;

        public void Controls() {
            wantsMouseMove = true;
            Event e = Event.current;
            switch (e.type) {
                case EventType.MouseMove:
                    break;
                case EventType.ScrollWheel:
                    if (e.delta.y > 0) zoom += 0.1f * zoom;
                    else zoom -= 0.1f * zoom;
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0) {
                        if (IsDraggingPort) {
                            if (IsHoveringPort && hoveredPort.IsInput) {
                                if (!draggedOutput.IsConnectedTo(hoveredPort)) {
                                    draggedOutputTarget = hoveredPort;
                                }
                            } else {
                                draggedOutputTarget = null;
                            }
                            Repaint();
                        } else if (CanDragNodeHeader) {
                            for (int i = 0; i < Selection.objects.Length; i++) {
                                if (Selection.objects[i] is XNode.Node) {
                                    XNode.Node node = Selection.objects[i] as XNode.Node;
                                    node.position = WindowToGridPosition(e.mousePosition) + dragOffset[i];
                                    if (NodeEditorPreferences.gridSnap) {
                                        node.position.x = (Mathf.Round((node.position.x + 8) / 16) * 16) - 8;
                                        node.position.y = (Mathf.Round((node.position.y + 8) / 16) * 16) - 8;
                                    }
                                }
                            }
                            DidDragNodeHeader = true;
                            Repaint();
                        }
                    } else if (e.button == 1 || e.button == 2) {
                        Vector2 tempOffset = panOffset;
                        tempOffset += e.delta * zoom;
                        // Round value to increase crispyness of UI text
                        tempOffset.x = Mathf.Round(tempOffset.x);
                        tempOffset.y = Mathf.Round(tempOffset.y);
                        panOffset = tempOffset;
                        isPanning = true;
                    }
                    break;
                case EventType.MouseDown:
                    Repaint();
                    if (e.button == 0) {

                        if (IsHoveringPort) {
                            if (hoveredPort.IsOutput) {
                                draggedOutput = hoveredPort;
                            } else {
                                hoveredPort.VerifyConnections();
                                if (hoveredPort.IsConnected) {
                                    XNode.Node node = hoveredPort.node;
                                    XNode.NodePort output = hoveredPort.Connection;
                                    hoveredPort.Disconnect(output);
                                    draggedOutput = output;
                                    draggedOutputTarget = hoveredPort;
                                    if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                                }
                            }
                        } else if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                            // If mousedown on node header, select or deselect
                            if (!Selection.Contains(hoveredNode)) SelectNode(hoveredNode, e.control || e.shift);
                            else if (e.control || e.shift) DeselectNode(hoveredNode);
                            e.Use();
                            DidDragNodeHeader = false;
                            CanDragNodeHeader = true;
                            dragOffset = new Vector2[Selection.objects.Length];
                            for (int i = 0; i < dragOffset.Length; i++) {
                                if (Selection.objects[i] is XNode.Node) {
                                    XNode.Node node = Selection.objects[i] as XNode.Node;
                                    dragOffset[i] = node.position - WindowToGridPosition(e.mousePosition);
                                }
                            }
                        }
                        // If mousedown on grid background, deselect all
                        else if (!IsHoveringNode) {
                            Selection.activeObject = null;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (e.button == 0) {
                        //Port drag release
                        if (IsDraggingPort) {
                            //If connection is valid, save it
                            if (draggedOutputTarget != null) {
                                XNode.Node node = draggedOutputTarget.node;
                                if (graph.nodes.Count != 0) draggedOutput.Connect(draggedOutputTarget);
                                if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                                EditorUtility.SetDirty(graph);
                            }
                            //Release dragged connection
                            draggedOutput = null;
                            draggedOutputTarget = null;
                            EditorUtility.SetDirty(graph);
                            Repaint();
                            AssetDatabase.SaveAssets();
                        } else if (CanDragNodeHeader) {
                            CanDragNodeHeader = false;
                            AssetDatabase.SaveAssets();
                        } else if (!IsHoveringNode) {
                            // If click outside node, release field focus
                            if (!isPanning) {
                                GUIUtility.hotControl = 0;
                                GUIUtility.keyboardControl = 0;
                            }
                            Repaint();
                            AssetDatabase.SaveAssets();
                        }

                        // If click node header, select single node.
                        if (IsHoveringNode && !DidDragNodeHeader && IsHoveringTitle(hoveredNode) && !(e.control || e.shift)) {
                            SelectNode(hoveredNode, false);
                            Repaint();
                        }
                    } else if (e.button == 1) {
                        if (!isPanning) {
                            if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                                if (!Selection.Contains(hoveredNode)) SelectNode(hoveredNode,false);
                                ShowNodeContextMenu();
                            } else if (!IsHoveringNode) {
                                ShowGraphContextMenu();
                            }
                        }
                        isPanning = false;
                    }
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.Delete) RemoveSelectedNodes();
                    else if (e.keyCode == KeyCode.D && e.control) DublicateSelectedNodes();
                    Repaint();
                    break;
            }
        }

        /// <summary> Puts all nodes in focus. If no nodes are present, resets view to  </summary>
        public void Home() {
            zoom = 2;
            panOffset = Vector2.zero;
        }

        public void CreateNode(Type type, Vector2 position) {
            XNode.Node node = graph.AddNode(type);
            node.position = position;
            Repaint();
        }

        /// <summary> Remove nodes in the graph in Selection.objects</summary>
        public void RemoveSelectedNodes() {
            foreach (UnityEngine.Object item in Selection.objects) {
                if (item is XNode.Node) {
                    XNode.Node node = item as XNode.Node;
                    graph.RemoveNode(node);
                }
            }
        }

        // <summary> Dublicate selected nodes and select the dublicates
        public void DublicateSelectedNodes() {
            UnityEngine.Object[] newNodes = new UnityEngine.Object[Selection.objects.Length];
            for (int i = 0; i < Selection.objects.Length; i++) {
                if (Selection.objects[i] is XNode.Node) {
                    XNode.Node node = Selection.objects[i] as XNode.Node;
                    if (node.graph != graph) continue; // ignore nodes selected in another graph
                    XNode.Node n = graph.CopyNode(node);
                    n.position = node.position + new Vector2(30, 30);
                    newNodes[i] = n;
                }
            }
            Selection.objects = newNodes;
        }

        /// <summary> Draw a connection as we are dragging it </summary>
        public void DrawDraggedConnection() {
            if (IsDraggingPort) {
                if (!_portConnectionPoints.ContainsKey(draggedOutput)) return;
                Vector2 from = _portConnectionPoints[draggedOutput].center;
                Vector2 to = draggedOutputTarget != null ? portConnectionPoints[draggedOutputTarget].center : WindowToGridPosition(Event.current.mousePosition);
                Color col = NodeEditorPreferences.GetTypeColor(draggedOutput.ValueType);
                col.a = 0.6f;
                DrawConnection(from, to, col);
            }
        }

        bool IsHoveringTitle(XNode.Node node) {
            Vector2 mousePos = Event.current.mousePosition;
            //Get node position
            Vector2 nodePos = GridToWindowPosition(node.position);
            float width = 200;
            if (nodeWidths.ContainsKey(node)) width = nodeWidths[node];
            Rect windowRect = new Rect(nodePos, new Vector2(width / zoom, 30 / zoom));
            return windowRect.Contains(mousePos);
        }
    }
}