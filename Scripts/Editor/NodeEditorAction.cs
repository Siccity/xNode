using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/// <summary> User input related UNEC functionality </summary>
public partial class NodeEditorWindow {

    public static bool isPanning { get; private set; }
    public static Vector2 dragOffset;

    private bool IsDraggingNode { get { return draggedNode != null; } }
    private bool IsDraggingPort { get { return draggedOutput != null; } }
    private bool IsHoveringPort { get { return hoveredPort != null; } }
    private bool IsHoveringNode { get { return hoveredNode != null; } }
    private bool HasSelectedNode { get { return selectedNode != null; } }

    private Node hoveredNode;
    private Node selectedNode;
    private Node draggedNode;
    private NodePort hoveredPort;
    private NodePort draggedOutput;
    private NodePort draggedOutputTarget;

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
                        }
                        else {
                            draggedOutputTarget = null;
                        }
                        Repaint();
                    }
                    else if (IsDraggingNode) {
                        draggedNode.position.position = WindowToGridPosition(e.mousePosition) + dragOffset;
                        Repaint();
                    }
                }
                else if (e.button == 1) {
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
                    }
                    else {
                        if (hoveredPort.IsConnected) {
                            NodePort output = hoveredPort.Connection;
                            hoveredPort.Disconnect(output);
                            draggedOutput = output;
                            draggedOutputTarget = hoveredPort;
                        }
                    }
                }
                else if (IsHoveringNode) {
                    draggedNode = hoveredNode;
                    dragOffset = hoveredNode.position.position - WindowToGridPosition(e.mousePosition);
                }
                break;
            case EventType.MouseUp:
                if (e.button == 0) {
                    //Port drag release
                    if (IsDraggingPort) {
                        //If connection is valid, save it
                        if (draggedOutputTarget != null) {
                            if (graph.nodes.Count != 0) draggedOutput.Connect(draggedOutputTarget);
                        }
                        //Release dragged connection
                        draggedOutput = null;
                        draggedOutputTarget = null;
                        Repaint();
                    }
                    else if (IsDraggingNode) {
                        draggedNode = null;
                    }
                }
                else if (e.button == 1) {
                    if (!isPanning) RightClickContextMenu();
                    isPanning = false;
                }
                UpdateHovered();
                break;
            case EventType.repaint:

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
        node.position.position = position;
    }

    /// <summary> Draw a connection as we are dragging it </summary>
    public void DrawDraggedConnection() {
        if (IsDraggingPort) {
            if (!_portConnectionPoints.ContainsKey(draggedOutput)) return;
            Vector2 from = _portConnectionPoints[draggedOutput];
            Vector2 to = draggedOutputTarget != null ? portConnectionPoints[draggedOutputTarget] : WindowToGridPosition(Event.current.mousePosition);
            DrawConnection(from, to);
        }
    }

    void UpdateHovered() {
        Vector2 mousePos = Event.current.mousePosition;
        Node newHoverNode = null;
        foreach (Node node in graph.nodes) {
            //Get node position
            Vector2 nodePos = GridToWindowPosition(node.position.position);
            Rect windowRect = new Rect(nodePos, new Vector2(node.position.size.x / zoom, node.position.size.y / zoom));
            if (windowRect.Contains(mousePos)) {
                newHoverNode = node;
            }
        }
        if (newHoverNode != hoveredNode) {
            hoveredNode = newHoverNode;
            Repaint();
        }
        if (IsHoveringNode) {
            NodePort newHoverPort = null;
            for (int i = 0; i < hoveredNode.InputCount; i++) {
                NodePort port = hoveredNode.GetInput(i);
                if (!portRects.ContainsKey(port)) continue;
                Rect r = portRects[port];
                r.position = GridToWindowPosition(r.position + hoveredNode.position.position);
                r.size /= zoom;
                if (r.Contains(mousePos)) newHoverPort = port;
            }
            for (int i = 0; i < hoveredNode.OutputCount; i++) {
                NodePort port = hoveredNode.GetOutput(i);
                if (!portRects.ContainsKey(port)) continue;
                Rect r = portRects[port];
                r.position = GridToWindowPosition(r.position + hoveredNode.position.position);
                r.size /= zoom;
                if (r.Contains(mousePos)) newHoverPort = port;
            }
            if (newHoverPort != hoveredPort) {
                hoveredPort = newHoverPort;
                Repaint();
            }
        }
        else hoveredPort = null;
    }
}
