using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class NodeEditorAction {

    public static void Controls(NodeEditorWindow window) {
        Event e = Event.current;

        switch (e.type) {
            case EventType.ScrollWheel:
                if (e.delta.y > 0) window.zoom += 0.1f * window.zoom;
                else window.zoom -= 0.1f * window.zoom;
                break;
            case EventType.MouseDrag:
                if (e.button == 1) window.panOffset += e.delta*window.zoom;
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.F) {
                    window.zoom = 2;
                    window.panOffset = Vector2.zero;
                }
                break;
        }
    }
}
