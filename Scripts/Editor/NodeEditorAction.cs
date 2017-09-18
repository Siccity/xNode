using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UNEC;

public static class NodeEditorAction {

    public static bool dragging;
    public static Vector2 dragOffset;

    public static void Controls(NodeEditorWindow window) {
        Event e = Event.current;
        switch (e.type) {
            
            case EventType.ScrollWheel:
                if (e.delta.y > 0) window.zoom += 0.1f * window.zoom;
                else window.zoom -= 0.1f * window.zoom;
                break;
            case EventType.MouseDrag:
                if (e.button == 0) {
                    if (window.activeNode != null) {
                        if (window.hoveredPort != null || window.tempConnection != null) {
                            if (window.tempConnection == null) {
                                if (window.hoveredPort.direction == NodePort.IO.Output) {
                                    dragging = true;
                                    int inputNodeId = window.graph.GetNodeId(window.activeNode);
                                    int outputPortId = window.activeNode.GetOutputPortId(window.hoveredPort);
                                    window.tempConnection = new NodeConnection(inputNodeId, outputPortId, -1,-1);
                                }
                                else {
                                    Debug.Log("input");
                                    /*int outputNodeId = window.graph.GetNodeId(window.activeNode);
                                    int outputPortId = window.activeNode.GetInputPortId(window.hoveredPort);
                                    window.tempConnection = new NodeConnection(-1,-1,outputNodeId,outputPortId);*/
                                }
                            }
                        }
                        else {
                            window.activeNode.position.position = window.WindowToGridPosition(e.mousePosition) + dragOffset;
                            window.Repaint();
                        }
                    }
                } 
                else if (e.button == 1) {
                    window.panOffset += e.delta * window.zoom;
                    dragging = true;
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.F) Focus(window);
                break;
            case EventType.MouseDown:
                dragging = false;
                window.Repaint();
                window.SelectNode(window.hoveredNode);
                if (window.hoveredNode != null) {
                    dragOffset = window.hoveredNode.position.position - window.WindowToGridPosition(e.mousePosition);
                }
                window.Repaint();
                break;
            case EventType.MouseUp:
                window.tempConnection = null;
                if (dragging) return;

                if (e.button == 1) {
                    NodeEditorGUI.RightClickContextMenu(window);
                }
                break;
            case EventType.repaint:

                break;
        }
    }

    /// <summary> Puts all nodes in focus. If no nodes are present, resets view to  </summary>
    public static void Focus(this NodeEditorWindow window) {
        window.zoom = 2;
        window.panOffset = Vector2.zero;
    }

    public static void CreateNode(this NodeEditorWindow window, Type type, Vector2 position) {
        Node node = window.graph.AddNode(type);
        node.position.position = position;
    }
}
