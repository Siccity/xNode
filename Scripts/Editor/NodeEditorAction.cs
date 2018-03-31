using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public partial class NodeEditorWindow {
        public enum NodeActivity { Idle, HoldNode, DragNode, HoldGrid, DragGrid }
        public static NodeActivity currentActivity = NodeActivity.Idle;
        public static bool isPanning { get; private set; }
        public static Vector2[] dragOffset;

        private bool IsDraggingPort { get { return draggedOutput != null; } }
        private bool IsHoveringPort { get { return hoveredPort != null; } }
        private bool IsHoveringNode { get { return hoveredNode != null; } }
        private bool IsHoveringReroute { get { return hoveredReroute >= 0; } }
        private XNode.Node hoveredNode = null;
        [NonSerialized] private XNode.NodePort hoveredPort = null;
        [NonSerialized] private XNode.NodePort draggedOutput = null;
        [NonSerialized] private XNode.NodePort draggedOutputTarget = null;
        private int hoveredReroute = -1;
        private List<int> selectedReroutes = new List<int>();
        private Rect nodeRects;
        private Vector2 dragBoxStart;
        private UnityEngine.Object[] preBoxSelection;
        private int[] preBoxSelectionReroute;

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
                        } else if (currentActivity == NodeActivity.HoldNode) {
                            RecalculateDragOffsets(e);
                            currentActivity = NodeActivity.DragNode;
                            Repaint();
                        }
                        if (currentActivity == NodeActivity.DragNode) {
                            // Holding ctrl inverts grid snap
                            bool gridSnap = NodeEditorPreferences.GetSettings().gridSnap;
                            if (e.control) gridSnap = !gridSnap;

                            Vector2 mousePos = WindowToGridPosition(e.mousePosition);
                            // Move selected nodes with offset
                            for (int i = 0; i < Selection.objects.Length; i++) {
                                if (Selection.objects[i] is XNode.Node) {
                                    XNode.Node node = Selection.objects[i] as XNode.Node;
                                    node.position = mousePos + dragOffset[i];
                                    if (gridSnap) {
                                        node.position.x = (Mathf.Round((node.position.x + 8) / 16) * 16) - 8;
                                        node.position.y = (Mathf.Round((node.position.y + 8) / 16) * 16) - 8;
                                    }
                                }
                            }
                            // Move selected reroutes with offset
                            for (int i = 0; i < selectedReroutes.Count; i++) {
                                Vector2 pos = mousePos + dragOffset[Selection.objects.Length + i];
                                pos.x -= 8;
                                pos.y -= 8;
                                if (gridSnap) {
                                    pos.x = (Mathf.Round((pos.x + 8) / 16) * 16);
                                    pos.y = (Mathf.Round((pos.y + 8) / 16) * 16);
                                }
                                SetReroute(selectedReroutes[i], pos);
                            }
                            Repaint();
                        } else if (currentActivity == NodeActivity.HoldGrid) {
                            currentActivity = NodeActivity.DragGrid;
                            preBoxSelection = Selection.objects;
                            preBoxSelectionReroute = selectedReroutes.ToArray();
                            dragBoxStart = WindowToGridPosition(e.mousePosition);
                            Repaint();
                        } else if (currentActivity == NodeActivity.DragGrid) {
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
                            if (!Selection.Contains(hoveredNode)) {
                                SelectNode(hoveredNode, e.control || e.shift);
                                if (!e.control && !e.shift) selectedReroutes.Clear();
                            } else if (e.control || e.shift) DeselectNode(hoveredNode);
                            e.Use();
                            currentActivity = NodeActivity.HoldNode;
                        } else if (IsHoveringReroute) {
                            // If reroute isn't selected
                            if (!selectedReroutes.Contains(hoveredReroute)) {
                                // Add it
                                if (e.control || e.shift) selectedReroutes.Add(hoveredReroute);
                                // Select it
                                else {
                                    selectedReroutes = new List<int>() { hoveredReroute };
                                    Selection.activeObject = null;
                                }

                            }
                            // Deselect
                            else if (e.control || e.shift) selectedReroutes.RemoveAt(hoveredReroute);
                            e.Use();
                            currentActivity = NodeActivity.HoldNode;
                        }
                        // If mousedown on grid background, deselect all
                        else if (!IsHoveringNode) {
                            currentActivity = NodeActivity.HoldGrid;
                            if (!e.control && !e.shift) {
                                selectedReroutes.Clear();
                                Selection.activeObject = null;
                            }
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
                            AssetDatabase.SaveAssets();
                        } else if (currentActivity == NodeActivity.DragNode) {
                            AssetDatabase.SaveAssets();
                        } else if (!IsHoveringNode) {
                            // If click outside node, release field focus
                            if (!isPanning) {
                                // I've got no idea which of these do what, so we'll just reset all of it.
                                GUIUtility.hotControl = 0;
                                GUIUtility.keyboardControl = 0;
                                EditorGUIUtility.editingTextField = false;
                                EditorGUIUtility.keyboardControl = 0;
                                EditorGUIUtility.hotControl = 0;
                            }
                            AssetDatabase.SaveAssets();
                        }

                        // If click node header, select it.
                        if (currentActivity == NodeActivity.HoldNode && !(e.control || e.shift)) {
                            selectedReroutes.Clear();
                            SelectNode(hoveredNode, false);
                        }

                        // If click reroute, select it.
                        if (IsHoveringReroute && !(e.control || e.shift)) {
                            selectedReroutes = new List<int>() { hoveredReroute };
                            Selection.activeObject = null;
                        }

                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    } else if (e.button == 1) {
                        if (!isPanning) {
                            if (IsHoveringReroute) {
                                ShowRerouteContextMenu(hoveredReroute);
                            } else if (IsHoveringPort) {
                                ShowPortContextMenu(hoveredPort);
                            } else if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                                if (!Selection.Contains(hoveredNode)) SelectNode(hoveredNode, false);
                                ShowNodeContextMenu();
                            } else if (!IsHoveringNode) {
                                ShowGraphContextMenu();
                            }
                        }
                        isPanning = false;
                    }
                    break;
                case EventType.KeyDown:
                    if (EditorGUIUtility.editingTextField) break;
                    else if (e.keyCode == KeyCode.F) Home();
                    break;
                case EventType.ValidateCommand:
                    if (e.commandName == "SoftDelete") RemoveSelectedNodes();
                    else if (e.commandName == "Duplicate") DublicateSelectedNodes();
                    Repaint();
                    break;
                case EventType.Ignore:
                    // If release mouse outside window
                    if (e.rawType == EventType.MouseUp && currentActivity == NodeActivity.DragGrid) {
                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    }
                    break;
            }
        }

        private void RecalculateDragOffsets(Event current) {
            dragOffset = new Vector2[Selection.objects.Length + selectedReroutes.Count];
            // Selected nodes
            for (int i = 0; i < Selection.objects.Length; i++) {
                if (Selection.objects[i] is XNode.Node) {
                    XNode.Node node = Selection.objects[i] as XNode.Node;
                    dragOffset[i] = node.position - WindowToGridPosition(current.mousePosition);
                }
            }

            // Selected reroutes
            for (int i = 0; i < selectedReroutes.Count; i++) {
                dragOffset[Selection.objects.Length + i] = GetReroutePos(selectedReroutes[i]) - WindowToGridPosition(current.mousePosition);
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

        /// <summary> Dublicate selected nodes and select the dublicates </summary>
        public void DublicateSelectedNodes() {
            UnityEngine.Object[] newNodes = new UnityEngine.Object[Selection.objects.Length];
            Dictionary<XNode.Node, XNode.Node> substitutes = new Dictionary<XNode.Node, XNode.Node>();
            for (int i = 0; i < Selection.objects.Length; i++) {
                if (Selection.objects[i] is XNode.Node) {
                    XNode.Node srcNode = Selection.objects[i] as XNode.Node;
                    if (srcNode.graph != graph) continue; // ignore nodes selected in another graph
                    XNode.Node newNode = graph.CopyNode(srcNode);
                    substitutes.Add(srcNode, newNode);
                    newNode.position = srcNode.position + new Vector2(30, 30);
                    newNodes[i] = newNode;
                }
            }

            // Walk through the selected nodes again, recreate connections, using the new nodes
            for (int i = 0; i < Selection.objects.Length; i++) {
                if (Selection.objects[i] is XNode.Node) {
                    XNode.Node srcNode = Selection.objects[i] as XNode.Node;
                    if (srcNode.graph != graph) continue; // ignore nodes selected in another graph
                    foreach (XNode.NodePort port in srcNode.Ports) {
                        for (int c = 0; c < port.ConnectionCount; c++) {
                            XNode.NodePort inputPort = port.direction == XNode.NodePort.IO.Input ? port : port.GetConnection(c);
                            XNode.NodePort outputPort = port.direction == XNode.NodePort.IO.Output ? port : port.GetConnection(c);

                            if (substitutes.ContainsKey(inputPort.node) && substitutes.ContainsKey(outputPort.node)) {
                                XNode.Node newNodeIn = substitutes[inputPort.node];
                                XNode.Node newNodeOut = substitutes[outputPort.node];
                                newNodeIn.UpdateStaticPorts();
                                newNodeOut.UpdateStaticPorts();
                                inputPort = newNodeIn.GetInputPort(inputPort.fieldName);
                                outputPort = newNodeOut.GetOutputPort(outputPort.fieldName);
                            }
                            if (!inputPort.IsConnectedTo(outputPort)) inputPort.Connect(outputPort);
                        }
                    }
                }
            }
            Selection.objects = newNodes;
        }

        /// <summary> Add a reroute node to a graph and return index </summary>
        public int AddReroute(Vector2 position) {
            SerializedProperty reroutes = graphEditor.serializedObject.FindProperty("reroutes");
            reroutes.arraySize++;
            reroutes.GetArrayElementAtIndex(reroutes.arraySize - 1).vector2Value = position;
            graphEditor.serializedObject.ApplyModifiedProperties();
            return reroutes.arraySize - 1;
        }

        /// <summary> Set the position of a reroute node </summary>
        public void SetReroute(int index, Vector2 position) {
            SerializedProperty reroutes = graphEditor.serializedObject.FindProperty("reroutes");
            reroutes.GetArrayElementAtIndex(index).vector2Value = position;
            graphEditor.serializedObject.ApplyModifiedProperties();
        }

        /// <summary> Get the position of a reroute node </summary>
        public Vector2 GetReroutePos(int index) {
            SerializedProperty reroutes = graphEditor.serializedObject.FindProperty("reroutes");
            return reroutes.GetArrayElementAtIndex(index).vector2Value;
        }

        /// <summary> Remove a reroute node </summary>
        public void RemoveReroute(int index) {
            SerializedProperty reroutes = graphEditor.serializedObject.FindProperty("reroutes");
            reroutes.DeleteArrayElementAtIndex(index);
            graphEditor.serializedObject.ApplyModifiedProperties();
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