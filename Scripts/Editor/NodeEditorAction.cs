using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class NodeEditorWindow {

    public static bool isPanning { get; private set; }
    public static Vector2 dragOffset;

    private bool IsDraggingNode { get { return draggedNode != null; } }
    private bool IsDraggingPort { get { return draggedOutput != null; } }
    private bool IsHoveringPort { get { return hoveredPort != null; } }
    private bool IsHoveringNode { get { return hoveredNode != null; } }
    private bool HasSelectedNode { get { return selectedNode != null; } }

    private Node hoveredNode = null;

    [NonSerialized] private Node selectedNode = null;
    [NonSerialized] private Node draggedNode = null;
    [NonSerialized] private NodePort hoveredPort = null;
    [NonSerialized] private NodePort draggedOutput = null;
    [NonSerialized] private NodePort draggedOutputTarget = null;

    private Rect nodeRects;

    public void Controls() {
        wantsMouseMove = true;

        Event e = Event.current;
        switch (e.type) {
            case EventType.MouseMove:
                UpdateHovered();
                break;
            case EventType.ScrollWheel:
                if (e.delta.y > 0) zoom += 0.1f * zoom;
                else zoom -= 0.1f * zoom;
                break;
            case EventType.MouseDrag:
                UpdateHovered();
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
                    } else if (IsDraggingNode) {
                        draggedNode.rect.position = WindowToGridPosition(e.mousePosition) + dragOffset;
                        Repaint();
                    }
                } else if (e.button == 1) {
                    panOffset += e.delta * zoom;
                    isPanning = true;
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.F) Home();
                break;
            case EventType.MouseDown:
                UpdateHovered();
                Repaint();
                SelectNode(hoveredNode);
                if (IsHoveringPort) {
                    if (hoveredPort.IsOutput) {
                        draggedOutput = hoveredPort;
                    } else {
                        if (hoveredPort.IsConnected) {
                            NodePort output = hoveredPort.Connection;
                            hoveredPort.Disconnect(output);
                            draggedOutput = output;
                            draggedOutputTarget = hoveredPort;
                        }
                    }
                } else if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                    draggedNode = hoveredNode;
                    dragOffset = hoveredNode.rect.position - WindowToGridPosition(e.mousePosition);
                }
                break;
            case EventType.MouseUp:
                if (e.button == 0) {
                    //Port drag release
                    if (IsDraggingPort) {
                        //If connection is valid, save it
                        if (draggedOutputTarget != null) {
                            if (graph.nodes.Count != 0) draggedOutput.Connect(draggedOutputTarget);
                            EditorUtility.SetDirty(graph);
                        }
                        //Release dragged connection
                        draggedOutput = null;
                        draggedOutputTarget = null;
                        EditorUtility.SetDirty(graph);
                        Repaint();
                    } else if (IsDraggingNode) {
                        draggedNode = null;
                    }
                } else if (e.button == 1) {
                    if (!isPanning) ShowContextMenu();
                    isPanning = false;
                }
                UpdateHovered();
                AssetDatabase.SaveAssets();
                break;
        }
    }

    /// <summary> Puts all nodes in focus. If no nodes are present, resets view to  </summary>
    public void Home() {
        zoom = 2;
        panOffset = Vector2.zero;
    }

    public void CreateNode(Type type, Vector2 position) {
        Node node = graph.AddNode(type);
        node.rect.position = position;
        Repaint();
    }

    /// <summary> Draw a connection as we are dragging it </summary>
    public void DrawDraggedConnection() {
        if (IsDraggingPort) {
            if (!_portConnectionPoints.ContainsKey(draggedOutput)) return;
            Vector2 from = _portConnectionPoints[draggedOutput].center;
            Vector2 to = draggedOutputTarget != null ? portConnectionPoints[draggedOutputTarget].center : WindowToGridPosition(Event.current.mousePosition);
            Color col = NodeEditorPreferences.GetTypeColor(draggedOutput.type);
            col.a = 0.6f;
            DrawConnection(from, to, col);
        }
    }

    /// <summary> Updates <see cref="hoveredNode"/> and <see cref="hoveredPort"/> </summary>
    void UpdateHovered() {
        Vector2 mousePos = Event.current.mousePosition;
        Node newHoverNode = null;
        foreach (Node node in graph.nodes) {
            //Get node position
            Vector2 nodePos = GridToWindowPosition(node.rect.position);
            Rect windowRect = new Rect(nodePos, new Vector2(node.rect.size.x / zoom, node.rect.size.y / zoom));
            if (windowRect.Contains(mousePos)) {
                newHoverNode = node;
            }
        }
        if (newHoverNode != hoveredNode) {
            hoveredNode = newHoverNode;
            Repaint();
        }
        //If we are hovering a node, check if we are also hovering a port
        NodePort newHoverPort = null;
        //Check all input ports
        for (int k = 0; k < graph.nodes.Count; k++) {

            for (int i = 0; i < graph.nodes[k].InputCount; i++) {
                NodePort port = graph.nodes[k].inputs[i];
                //Check if port rect is available
                if (!portConnectionPoints.ContainsKey(port)) continue;
                Rect r = GridToWindowRect(portConnectionPoints[port]);
                if (r.Contains(mousePos)) newHoverPort = port;
            }
            //Check all output ports
            for (int i = 0; i < graph.nodes[k].OutputCount; i++) {
                NodePort port = graph.nodes[k].outputs[i];
                //Check if port rect is available
                if (!portConnectionPoints.ContainsKey(port)) continue;
                Rect r = GridToWindowRect(portConnectionPoints[port]);
                if (r.Contains(mousePos)) newHoverPort = port;
            }
        }
        if (newHoverPort != hoveredPort) {
            hoveredPort = newHoverPort;
        }
    }

    bool IsHoveringTitle(Node node) {
        Vector2 mousePos = Event.current.mousePosition;
        //Get node position
        Vector2 nodePos = GridToWindowPosition(node.rect.position);
        Rect windowRect = new Rect(nodePos, new Vector2(node.rect.size.x / zoom, 30 / zoom));
        return windowRect.Contains(mousePos);
    }
}