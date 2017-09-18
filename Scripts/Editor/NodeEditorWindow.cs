using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UNEC;

public class NodeEditorWindow : EditorWindow {

    public NodeGraph graph { get { return _graph != null ? _graph : _graph = new NodeGraph(); } }
    public NodeGraph _graph;
    public Node hoveredNode;
    public Node activeNode { get; private set; }

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
        Matrix4x4 m = GUI.matrix;
        NodeEditorAction.Controls(this);


        NodeEditorGUI.DrawGrid(position, zoom, panOffset);
        DrawNodes();
        NodeEditorGUI.DrawToolbar(this);

        GUI.matrix = m;
    }

    private void DrawNodes() {
        BeginWindows();
        NodeEditorGUI.BeginZoomed(position, zoom);
        Event e = Event.current;
        hoveredNode = null;
        foreach (KeyValuePair<int, Node> kvp in graph.nodes) {
            Node node = kvp.Value;
            int id = kvp.Key;

            //Get node position
            Vector2 windowPos = GridToWindowPositionNoClipped(node.position.position);

            Rect windowRect = new Rect(windowPos, new Vector2(200, 200));
            if (windowRect.Contains(e.mousePosition)) hoveredNode = node;

            GUIStyle style = (node == activeNode) ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0";
            GUI.Box(windowRect, node.ToString(), style);


            if (windowRect.position != windowPos) {
                windowPos = windowRect.position;
                node.position.position = WindowToGridPosition(windowPos);
            //Vector2 newPos = windowRect =
            }

        }
        NodeEditorGUI.EndZoomed(position, zoom);
        EndWindows();
    }

    private void DraggableWindow(int windowID) {
        GUI.DragWindow();

        if (GUILayout.Button("ASDF")) Debug.Log("ASDF");
        if (GUI.Button(new Rect(20,20,200,200),"ASDF")) Debug.Log("ASDF");
    }

    public Vector2 WindowToGridPosition(Vector2 windowPosition) {
        return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
    }

    public Vector2 GridToWindowPosition(Vector2 gridPosition) {
        return (position.size * 0.5f) + (panOffset / zoom) + gridPosition;
    }

    public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition) {
        Vector2 center = position.size * 0.5f;
        float xOffset = (center.x * zoom + (panOffset.x + gridPosition.x));
        float yOffset = (center.y * zoom + (panOffset.y + gridPosition.y));
        return new Vector2(xOffset, yOffset);
    }

    public void SelectNode(Node node) {
        activeNode = node;
    }
}