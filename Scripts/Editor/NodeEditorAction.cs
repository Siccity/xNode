using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UNEC;

public static class NodeEditorAction {

    public static NodeEditorWindow window { get { return NodeEditorWindow.window; } }
    public static bool dragging;

    public static void Controls() {
        Event e = Event.current;

        switch (e.type) {
            
            case EventType.ScrollWheel:
                if (e.delta.y > 0) window.zoom += 0.1f * window.zoom;
                else window.zoom -= 0.1f * window.zoom;
                break;
            case EventType.MouseDrag:
                if (e.button == 1) {
                    window.panOffset += e.delta * window.zoom;
                    dragging = true;
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.F) Focus();
                break;
            case EventType.mouseDown:
                dragging = false;
                break;
            case EventType.MouseUp:
                if (dragging) return;
                if (e.button == 1) {
                    NodeEditorGUI.RightClickContextMenu();
                }
                break;
        }
    }

    /// <summary> Puts all nodes in focus. If no nodes are present, resets view to  </summary>
    public static void Focus() {
        window.zoom = 2;
        window.panOffset = Vector2.zero;
    }

    public static void CreateNode(Type type, Vector2 position) {
        Node node = window.currentGraph.AddNode(type);
        Debug.Log("SetNode " + position);
        node.position.position = position;
    }
}
