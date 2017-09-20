using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public partial class  NodeEditorWindow : EditorWindow {

    public Dictionary<NodePort, Vector2> portConnectionPoints { get { return _portConnectionPoints; } }
    private Dictionary<NodePort, Vector2> _portConnectionPoints = new Dictionary<NodePort, Vector2>();
    private Dictionary<NodePort, Rect> portRects = new Dictionary<NodePort, Rect>();
    public NodeGraph graph { get { return _graph != null ? _graph : _graph = new NodeGraph(); } }
    public NodeGraph _graph;
    public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
    private Vector2 _panOffset;
    public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, 1f, 5f); Repaint(); } }
    private float _zoom = 1; 

    partial void OnEnable();

    [MenuItem("Window/UNEC")]
    static void Init() {
        NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
        w.titleContent = new GUIContent("UNEC");
        w.wantsMouseMove = true;
        w.Show();
    }

    public void DrawConnections() {
        foreach(Node node in graph.nodes) {
            for (int i = 0; i < node.OutputCount; i++) {
                NodePort output = node.GetOutput(i);
                Vector2 from = _portConnectionPoints[output];
                for (int k = 0; k < output.ConnectionCount; k++) {
                    NodePort input = output.GetConnection(k);
                    Vector2 to = _portConnectionPoints[input];
                    DrawConnection(from, to);
                }
            }
        }
    }

    private void DrawNodes() {
        portConnectionPoints.Clear();
        Event e = Event.current;

        BeginWindows();
        BeginZoomed(position, zoom);
        if (e.type == EventType.Repaint) portRects.Clear();
        foreach (Node node in graph.nodes) {
            //Get node position
            Vector2 nodePos = GridToWindowPositionNoClipped(node.position.position);

            Rect windowRect = new Rect(nodePos, node.position.size);

            GUIStyle style = (node == selectedNode) ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0";
            GUILayout.BeginArea(windowRect, node.ToString(), style);
            GUILayout.BeginHorizontal();
            
            //Inputs
            GUILayout.BeginVertical();
            for (int i = 0; i < node.InputCount; i++) {
                NodePort input = node.GetInput(i);
                Rect r = GUILayoutUtility.GetRect(new GUIContent(input.name), styles.GetInputStyle(input.type));
                GUI.Label(r, input.name, styles.GetInputStyle(input.type));
                if (e.type == EventType.Repaint) portRects.Add(input, r);
                portConnectionPoints.Add(input, new Vector2(r.xMin, r.yMin + (r.height * 0.5f)) + node.position.position);
            }
            GUILayout.EndVertical();

            //Outputs
            GUILayout.BeginVertical();
            for (int i = 0; i < node.OutputCount; i++) {
                NodePort output = node.GetOutput(i);
                Rect r = GUILayoutUtility.GetRect(new GUIContent(output.name), styles.GetOutputStyle(output.type));
                GUI.Label(r, output.name, styles.GetOutputStyle(output.type));
                if (e.type == EventType.Repaint) portRects.Add(output, r);
                portConnectionPoints.Add(output, new Vector2(r.xMax, r.yMin + (r.height * 0.5f)) + node.position.position);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            // GUI

            GUILayout.EndArea();

            if (windowRect.position != nodePos) {
                nodePos = windowRect.position;
                node.position.position = WindowToGridPosition(nodePos);
            //Vector2 newPos = windowRect =
            }

        }
        EndZoomed(position, zoom);
        EndWindows();
    }

    private void DraggableWindow(int windowID) {
        GUI.DragWindow();
    }

    public Vector2 WindowToGridPosition(Vector2 windowPosition) {
        return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
    }

    public Vector2 GridToWindowPosition(Vector2 gridPosition) {
        //Vector2 center = position.size * 0.5f;
        return (position.size * 0.5f) + (panOffset / zoom) + (gridPosition/zoom);
    }

    public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition) {
        Vector2 center = position.size * 0.5f;
        float xOffset = (center.x * zoom + (panOffset.x + gridPosition.x));
        float yOffset = (center.y * zoom + (panOffset.y + gridPosition.y));
        return new Vector2(xOffset, yOffset);
    }

    public void SelectNode(Node node) {
        selectedNode = node;
    }

}