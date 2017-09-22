using UnityEngine;
using UnityEditor;
using System;

/// <summary> Contains GUI methods </summary>
public partial class NodeEditorWindow {

    private void OnGUI() {
        Event e = Event.current;
        Matrix4x4 m = GUI.matrix;
        Controls();

        DrawGrid(position, zoom, panOffset);
        DrawNodes();
        DrawConnections();
        DrawDraggedConnection();
        DrawToolbar();

        GUI.matrix = m;
    }

    public static void DrawConnection(Vector2 from, Vector2 to, Color col) {
        Handles.DrawBezier(from, to, from, to, col, new Texture2D(2, 2), 2);
    }

    public static void BeginZoomed(Rect rect, float zoom) {
        GUI.EndClip();
            
        GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
        Vector4 padding = new Vector4(0, 22, 0, 0);
        padding *= zoom;
        GUI.BeginClip(new Rect(
            -((rect.width * zoom) - rect.width) * 0.5f,
            -(((rect.height * zoom) - rect.height) * 0.5f) + (22 * zoom),
            rect.width * zoom,
            rect.height * zoom));
    }

    public static void EndZoomed(Rect rect, float zoom) {
        GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
        Vector3 offset = new Vector3(
            (((rect.width * zoom) - rect.width) * 0.5f),
            (((rect.height * zoom) - rect.height) * 0.5f) + (-22 * zoom)+22,
            0);
        GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
    }

    public static void DrawGrid(Rect rect, float zoom, Vector2 panOffset) {

        rect.position = Vector2.zero;

        Vector2 center = rect.size / 2f;
        Texture2D gridTex = NodeEditorResources.gridTexture;
        Texture2D crossTex = NodeEditorResources.crossTexture;
            
        // Offset from origin in tile units
        float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
        float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

        Vector2 tileOffset = new Vector2(xOffset, yOffset);

        // Amount of tiles
        float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
        float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

        Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

        // Draw tiled background
        GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
        GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f,0.5f), tileAmount));
    }

    public static bool DropdownButton(string name, float width) {
        return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
    }

    /// <summary> Show right-click context menu </summary>
    public void ShowContextMenu() {
        GenericMenu contextMenu = new GenericMenu();
        Vector2 pos = WindowToGridPosition(Event.current.mousePosition);

        if (hoveredNode != null) {
            Node node = hoveredNode;
            contextMenu.AddItem(new GUIContent("Remove"), false, () => graph.RemoveNode(node));
        }
        else {
            for (int i = 0; i < nodeTypes.Length; i++) {
                Type type = nodeTypes[i];
                Type editorType = GetNodeEditor(type).GetType();

                string name = nodeTypes[i].ToString().Replace('.', '/');
                CustomNodeEditorAttribute attrib;
                if (NodeEditorUtilities.GetAttrib(editorType, out attrib)) {
                    name = attrib.contextMenuName;
                }
                contextMenu.AddItem(new GUIContent(name), false, () => {
                    CreateNode(type, pos);
                });
            }
        }
        contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
    }

    public void DrawConnection(Vector2 startPoint, Vector2 endPoint) {
        startPoint = GridToWindowPosition(startPoint);
        endPoint = GridToWindowPosition(endPoint);

        Vector2 startTangent = startPoint;
        if (startPoint.x < endPoint.x) startTangent.x = Mathf.LerpUnclamped(startPoint.x, endPoint.x, 0.7f);
        else startTangent.x = Mathf.LerpUnclamped(startPoint.x, endPoint.x, -0.7f);

        Vector2 endTangent = endPoint;
        if (startPoint.x > endPoint.x) endTangent.x = Mathf.LerpUnclamped(endPoint.x, startPoint.x, -0.7f);
        else endTangent.x = Mathf.LerpUnclamped(endPoint.x, startPoint.x, 0.7f);

        Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.gray, null, 4);
        Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.white, null, 2);
    }

    public void DrawConnections() {
        foreach (Node node in graph.nodes) {
            for (int i = 0; i < node.OutputCount; i++) {
                NodePort output = node.GetOutput(i);

                //Needs cleanup. Null checks are ugly
                if (!portConnectionPoints.ContainsKey(output)) continue;
                Vector2 from = _portConnectionPoints[output].center + node.position.position;
                for (int k = 0; k < output.ConnectionCount; k++) {
                    NodePort input = output.GetConnection(k);
                    Vector2 to = input.node.position.position + _portConnectionPoints[input].center;
                    DrawConnection(from, to);
                }
            }
        }
    }

    private void DrawNodes() {
        Event e = Event.current;
        if (e.type == EventType.Repaint) portConnectionPoints.Clear();

        BeginWindows();
        BeginZoomed(position, zoom);
        //if (e.type == EventType.Repaint) portRects.Clear();
        foreach (Node node in graph.nodes) {

            //Get node position
            Vector2 nodePos = GridToWindowPositionNoClipped(node.position.position);

            GUIStyle style = (node == selectedNode) ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0";
            GUILayout.BeginArea(new Rect(nodePos,new Vector2(240,4000)));
            string nodeName = string.IsNullOrEmpty(node.name) ? node.ToString() : node.name;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(nodeName, style, GUILayout.Width(200));

            NodeEditor nodeEditor = GetNodeEditor(node.GetType());

            nodeEditor.target = node;

            //Node is hashed before and after node GUI to detect changes
            int nodeHash = 0;
            var onValidate = node.GetType().GetMethod("OnValidate");
            if (onValidate != null) nodeHash = node.GetHashCode();

            nodeEditor.OnNodeGUI();
            if (e.type == EventType.Repaint) {
                foreach (var kvp in nodeEditor.portRects) {
                    portConnectionPoints.Add(kvp.Key, kvp.Value);
                }
            }

            //If a change in hash is detected, call OnValidate method. This is done through reflection because OnValidate is only relevant in editor, and thus, the code should not be included in build.
            if (onValidate != null && nodeHash != node.GetHashCode()) onValidate.Invoke(node, null);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            if (e.type == EventType.Repaint) node.position.size = GUILayoutUtility.GetLastRect().size;
            GUILayout.EndArea();
        }
        EndZoomed(position, zoom);
        EndWindows();
    }
}
