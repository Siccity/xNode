using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UNEC;

public class NodeEditorWindow : EditorWindow {

    private Dictionary<NodePort, Vector2> portConnectionPoints = new Dictionary<NodePort, Vector2>();
    public bool IsDraggingConnection { get { return draggedConnection.enabled; } }
    public bool IsHoveringPort { get { return hoveredPort != null; } }
    public bool IsHoveringNode { get { return hoveredNode != null; } }
    public bool HasSelectedNode { get { return selectedNode != null; } }
    public NodeGraph graph { get { return _graph != null ? _graph : _graph = new NodeGraph(); } }
    public NodeGraph _graph;
    public Node hoveredNode;
    /// <summary> Currently selected node </summary>
    public Node selectedNode { get; private set; }
    public NodePort hoveredPort;
    public NodeConnection draggedConnection;

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
        DrawDraggedConnection();
        NodeEditorGUI.DrawToolbar(this);

        GUI.matrix = m;
    }

    /// <summary> Draw a connection as we are dragging it </summary>
    private void DrawDraggedConnection() {
        if (IsDraggingConnection) {
            Node inputNode = graph.GetNode(draggedConnection.inputNodeId);
            Node outputNode = graph.GetNode(draggedConnection.outputNodeId);
            if (inputNode != null) {
                NodePort outputPort =  inputNode.GetOutput(draggedConnection.outputPortId);
                Vector2 startPoint = GridToWindowPosition( portConnectionPoints[outputPort]);
                Vector2 endPoint = Event.current.mousePosition;
                if (outputNode != null) {
                    NodePort inputPort = outputNode.GetInput(draggedConnection.inputPortId);
                    if (inputPort != null) endPoint = GridToWindowPosition(portConnectionPoints[inputPort]);
                }

                Vector2 startTangent = startPoint;
                startTangent.x = Mathf.Lerp(startPoint.x,endPoint.x,0.7f);
                Vector2 endTangent = endPoint;
                endTangent.x = Mathf.Lerp(endPoint.x, startPoint.x, 0.7f);
                Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.gray, null, 4);
                Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.white, null, 2);
                Repaint();
            }
        }
    }
    private void DrawNodes() {
        portConnectionPoints.Clear();
        Event e = Event.current;

        BeginWindows();
        NodeEditorGUI.BeginZoomed(position, zoom);
        if (e.type == EventType.repaint) {
            hoveredPort = null;
        }
        hoveredNode = null;
        foreach (KeyValuePair<int, Node> kvp in graph.nodes) {
            Node node = kvp.Value;
            int id = kvp.Key;

            //Get node position
            Vector2 nodePos = GridToWindowPositionNoClipped(node.position.position);

            Rect windowRect = new Rect(nodePos, new Vector2(200, 200));
            if (windowRect.Contains(e.mousePosition)) hoveredNode = node;

            GUIStyle style = (node == selectedNode) ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0";
            GUILayout.BeginArea(windowRect, node.ToString(), style);
            GUILayout.BeginHorizontal();
            
            //Inputs
            GUILayout.BeginVertical();
            for (int i = 0; i < node.InputCount; i++) {
                NodePort input = node.GetInput(i);
                Rect r = GUILayoutUtility.GetRect(new GUIContent(input.name), NodeEditorResources.styles.GetInputStyle(input.type));
                GUI.Label(r, input.name, NodeEditorResources.styles.GetInputStyle(input.type));
                if (e.type == EventType.repaint) {
                    if (r.Contains(e.mousePosition)) hoveredPort = input;
                }
                portConnectionPoints.Add(input, new Vector2(r.xMin, r.yMin + (r.height * 0.5f)) + node.position.position);
            }
            GUILayout.EndVertical();

            //Outputs
            GUILayout.BeginVertical();
            for (int i = 0; i < node.OutputCount; i++) {
                NodePort output = node.GetOutput(i);
                Rect r = GUILayoutUtility.GetRect(new GUIContent(output.name), NodeEditorResources.styles.GetOutputStyle(output.type));
                GUI.Label(r, output.name, NodeEditorResources.styles.GetOutputStyle(output.type));
                if (e.type == EventType.repaint) {
                    if (r.Contains(e.mousePosition)) hoveredPort = output;
                }
                portConnectionPoints.Add(output, new Vector2(r.xMax, r.yMin + (r.height * 0.5f)) + node.position.position);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Label("More stuff");
            EditorGUILayout.Toggle("aDF",false);

            GUILayout.EndArea();

            if (windowRect.position != nodePos) {
                nodePos = windowRect.position;
                node.position.position = WindowToGridPosition(nodePos);
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