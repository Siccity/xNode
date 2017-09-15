using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UNEC;

public class NodeEditorWindow : EditorWindow {


    public static NodeEditorWindow window { get { return focusedWindow as NodeEditorWindow; } }

    public NodeGraph currentGraph { get { return _currentGraph != null ? _currentGraph : _currentGraph = new NodeGraph(); }}
    public NodeGraph _currentGraph;
    public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
    private Vector2 _panOffset;
    public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, 1f, 5f); Repaint(); } }
    private float _zoom = 1;


    [MenuItem("Window/UNEC")]
    static void Init() {
        NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
        w.titleContent = new GUIContent("UNEC");
        w.Show();
    }

    private void OnGUI() {
        NodeEditorAction.Controls();


        BeginWindows();
        NodeEditorGUI.DrawGrid(position, zoom, panOffset);
        NodeEditorGUI.DrawToolbar();
        DrawNodes();
        EndWindows();

    }

    private void DrawNodes() {
        Matrix4x4 m = GUI.matrix;
        GUI.EndClip();
        GUIUtility.ScaleAroundPivot(Vector2.one / zoom, position.size * 0.5f);
        foreach (KeyValuePair<string, Node> kvp in currentGraph.nodes) {
            Node node = kvp.Value;

            //Vector2 p = node.position.position + (position.size *0.5f) + panOffset;
            //Get node position
            Vector2 windowPos = GridToWindowPosition(node.position.position);
            windowPos = -windowPos;

            Rect windowRect = new Rect(windowPos, new Vector2(200,200));
            windowRect = GUI.Window(0, windowRect, DraggableWindow, node.ToString());
            //node.position = windowRect.position;
        }
        GUI.matrix = m;
        Vector2 padding = new Vector2(0, 0);
        padding *= zoom;
        GUI.BeginClip(new Rect(
            -(((position.width*zoom) - position.width)*0.5f) + (padding.x),
            -(((position.height * zoom) - position.height) * 0.5f) + (padding.y),
            (position.width*zoom)- (padding.x * 2),
            (position.height * zoom) - (padding.y * 2)));
    }

    private void DraggableWindow(int windowID) {
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    public Vector2 GetMousePositionOnGrid() {
        return WindowToGridPosition(Event.current.mousePosition);
    }

    public Vector2 WindowToGridPosition(Vector2 windowPosition) {
        return windowPosition - (window.position.size * 0.5f) - (panOffset * zoom);
    }

    public Vector2 GridToWindowPosition(Vector2 gridPosition) {
        return ((window.position.size * 0.5f) - (panOffset * zoom)) + gridPosition;
    }
}