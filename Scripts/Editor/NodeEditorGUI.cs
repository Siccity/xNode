using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

/// <summary> Contains GUI methods </summary>
public partial class NodeEditorWindow {

    private void OnGUI() {
        Event e = Event.current;
        Matrix4x4 m = GUI.matrix;
        Controls();

        if (e.type != EventType.MouseMove && e.type != EventType.MouseDrag) {
            DrawGrid(position, zoom, panOffset);
            DrawNodes();
            DrawConnections();
            DrawDraggedConnection();
            DrawToolbar();
        }

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
        Texture2D gridTex = gridTexture;
        Texture2D crossTex = crossTexture;
            
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

    public void RightClickContextMenu() {
        GenericMenu contextMenu = new GenericMenu();

        if (hoveredNode != null) {
            Node node = hoveredNode;
            contextMenu.AddItem(new GUIContent("Remove"), false, () => graph.RemoveNode(node));
        }
        else {
            Vector2 pos = WindowToGridPosition(Event.current.mousePosition);
            for (int i = 0; i < nodeTypes.Length; i++) {
                Type type = nodeTypes[i];
                contextMenu.AddItem(new GUIContent(nodeTypes[i].ToString()), false, () => {
                    CreateNode(type, pos);
                });
            }
        }
        contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
    }

    public  void DrawConnection(Vector2 startPoint, Vector2 endPoint) {
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
}
