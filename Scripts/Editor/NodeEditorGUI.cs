using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary> Contains GUI methods </summary>
public partial class NodeEditorWindow {

    private void OnGUI() {
        Event e = Event.current;
        Matrix4x4 m = GUI.matrix;
        Controls();

        DrawGrid(position, zoom, panOffset);
        DrawConnections();
        DrawDraggedConnection();
        DrawNodes();
        DrawToolbar();

        GUI.matrix = m;
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

    /// <summary> Draw a bezier from startpoint to endpoint, both in grid coordinates </summary>
    public void DrawConnection(Vector2 startPoint, Vector2 endPoint, Color col) {
        startPoint = GridToWindowPosition(startPoint);
        endPoint = GridToWindowPosition(endPoint);

        Vector2 startTangent = startPoint;
        if (startPoint.x < endPoint.x) startTangent.x = Mathf.LerpUnclamped(startPoint.x, endPoint.x, 0.7f);
        else startTangent.x = Mathf.LerpUnclamped(startPoint.x, endPoint.x, -0.7f);

        Vector2 endTangent = endPoint;
        if (startPoint.x > endPoint.x) endTangent.x = Mathf.LerpUnclamped(endPoint.x, startPoint.x, -0.7f);
        else endTangent.x = Mathf.LerpUnclamped(endPoint.x, startPoint.x, 0.7f);

        Color prevCol = GUI.color;
        Color edgeCol = new Color(0.1f, 0.1f, 0.1f, col.a);
        Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, edgeCol, null, 4);
        Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, col, null, 2);
        GUI.color = prevCol;
    }

    /// <summary> Draws all connections </summary>
    public void DrawConnections() {
        foreach (Node node in graph.nodes) {
            for (int i = 0; i < node.OutputCount; i++) {
                NodePort output = node.outputs[i];

                //Needs cleanup. Null checks are ugly
                if (!portConnectionPoints.ContainsKey(output)) continue;
                Vector2 from = _portConnectionPoints[output].center;
                for (int k = 0; k < output.ConnectionCount; k++) {
                    NodePort input = output.GetConnection(k);
                    Vector2 to = _portConnectionPoints[input].center;
                    DrawConnection(from, to, NodeEditorUtilities.GetTypeColor(output.type));
                }
            }
        }
    }

    private void DrawNodes() {
        Event e = Event.current;
        if (e.type == EventType.Repaint) portConnectionPoints.Clear();

        //Selected node is hashed before and after node GUI to detect changes
        int nodeHash = 0;
        System.Reflection.MethodInfo onValidate = null;
        if (selectedNode != null) {
            onValidate = selectedNode.GetType().GetMethod("OnValidate");
            if (onValidate != null) nodeHash = selectedNode.GetHashCode();
        }

        BeginZoomed(position, zoom);
        
        foreach (Node node in graph.nodes) {
            NodeEditor nodeEditor = GetNodeEditor(node.GetType());
            nodeEditor.target = node;

            //Get node position
            Vector2 nodePos = GridToWindowPositionNoClipped(node.rect.position);

            //GUIStyle style = (node == selectedNode) ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0";
            
            GUILayout.BeginArea(new Rect(nodePos,new Vector2(nodeEditor.GetWidth(), 4000)));

            GUIStyle style = NodeEditorResources.styles.nodeBody;
            GUILayout.BeginVertical(new GUIStyle(style));

            //Draw node contents
            Dictionary<NodePort, Vector2> portHandlePoints;
            nodeEditor.OnNodeGUI(out portHandlePoints);
            if (e.type == EventType.Repaint) {
                foreach (var kvp in portHandlePoints) {
                    Vector2 portHandlePos = kvp.Value;
                    portHandlePos += node.rect.position;
                    Rect rect = new Rect(portHandlePos.x - 8, portHandlePos.y - 8, 16, 16);
                    portConnectionPoints.Add(kvp.Key, rect);
                }
            }

            GUILayout.EndVertical();

            //if (e.type == EventType.Repaint) node.rect.size = GUILayoutUtility.GetLastRect().size;
            GUILayout.EndArea();
        }
        EndZoomed(position, zoom);

        //If a change in hash is detected in the selected node, call OnValidate method. 
        //This is done through reflection because OnValidate is only relevant in editor, 
        //and thus, the code should not be included in build.
        if (selectedNode != null) {
            if (onValidate != null && nodeHash != selectedNode.GetHashCode()) onValidate.Invoke(selectedNode, null);
        }
    }
}
