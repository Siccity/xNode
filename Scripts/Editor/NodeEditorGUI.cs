using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        DrawTooltip();

        GUI.matrix = m;
    }

    public static void BeginZoomed(Rect rect, float zoom) {
        GUI.EndClip();

        GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
        Vector4 padding = new Vector4(0, 22, 0, 0);
        padding *= zoom;
        GUI.BeginClip(new Rect(-((rect.width * zoom) - rect.width) * 0.5f, -(((rect.height * zoom) - rect.height) * 0.5f) + (22 * zoom),
            rect.width * zoom,
            rect.height * zoom));
    }

    public static void EndZoomed(Rect rect, float zoom) {
        GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
        Vector3 offset = new Vector3(
            (((rect.width * zoom) - rect.width) * 0.5f),
            (((rect.height * zoom) - rect.height) * 0.5f) + (-22 * zoom) + 22,
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
        GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
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
        } else {
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

        Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, col, null, 4);
    }

    /// <summary> Draws all connections </summary>
    public void DrawConnections() {
        foreach (Node node in graph.nodes) {
            //If a null node is found, return. This can happen if the nodes associated script is deleted. It is currently not possible in Unity to delete a null asset.
            if (node == null) continue;

            foreach(NodePort output in node.Outputs) {
                //Needs cleanup. Null checks are ugly
                if (!portConnectionPoints.ContainsKey(output)) continue;
                Vector2 from = _portConnectionPoints[output].center;
                for (int k = 0; k < output.ConnectionCount; k++) {

                    NodePort input = output.GetConnection(k);
                    if (input == null) continue; //If a script has been updated and the port doesn't exist, it is removed and null is returned. If this happens, return.
                    if (!input.IsConnectedTo(output)) input.Connect(output);
                    if (!_portConnectionPoints.ContainsKey(input)) continue;
                    Vector2 to = _portConnectionPoints[input].center;
                    DrawConnection(from, to, NodeEditorPreferences.GetTypeColor(output.type));
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

        Vector2 mousePos = Event.current.mousePosition;

        if (e.type != EventType.Layout) {
            hoveredNode = null;
            hoveredPort = null;
        }

        for (int n = 0; n < graph.nodes.Count; n++) {
            while (graph.nodes[n] == null) graph.nodes.RemoveAt(n);
            if (n >= graph.nodes.Count) return;
            Node node = graph.nodes[n];

            NodeEditor nodeEditor = GetNodeEditor(node.GetType());
            nodeEditor.target = node;
            nodeEditor.serializedObject = new SerializedObject(node);
            NodeEditor.portPositions = new Dictionary<NodePort, Vector2>();

            //Get node position
            Vector2 nodePos = GridToWindowPositionNoClipped(node.position);

            GUILayout.BeginArea(new Rect(nodePos, new Vector2(nodeEditor.GetWidth(), 4000)));

            GUIStyle style = NodeEditorResources.styles.nodeBody;
            GUILayout.BeginVertical(new GUIStyle(style));
            EditorGUI.BeginChangeCheck();

            //Draw node contents
            nodeEditor.OnNodeGUI();

            //Apply
            nodeEditor.serializedObject.ApplyModifiedProperties();

            //If user changed a value, notify other scripts through onUpdateNode
            if (EditorGUI.EndChangeCheck()) {
                if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
            }

            if (e.type == EventType.Repaint) {
                foreach (var kvp in NodeEditor.portPositions) {
                    Vector2 portHandlePos = kvp.Value;
                    portHandlePos += node.position;
                    Rect rect = new Rect(portHandlePos.x - 8, portHandlePos.y - 8, 16, 16);
                    portConnectionPoints.Add(kvp.Key, rect);
                }
            }

            GUILayout.EndVertical();

            if (e.type != EventType.Layout) {
                //Check if we are hovering this node
                Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                Rect windowRect = new Rect(nodePos, nodeSize);
                if (windowRect.Contains(mousePos)) hoveredNode = node;

                //Check if we are hovering any of this nodes ports
                //Check input ports
                foreach(NodePort input in node.Inputs) {
                    //Check if port rect is available
                    if (!portConnectionPoints.ContainsKey(input)) continue;
                    Rect r = GridToWindowRect(portConnectionPoints[input]);
                    if (r.Contains(mousePos)) hoveredPort = input;
                }
                //Check all output ports
                foreach(NodePort output in node.Outputs) {
                    //Check if port rect is available
                    if (!portConnectionPoints.ContainsKey(output)) continue;
                    Rect r = GridToWindowRect(portConnectionPoints[output]);
                    if (r.Contains(mousePos)) hoveredPort = output;
                }
            }

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

    private void DrawTooltip() {
        if (hoveredPort != null) {
            Type type = hoveredPort.type;
            GUIContent content = new GUIContent();
            content.text = TypeToString(type);
            Vector2 size = NodeEditorResources.styles.tooltip.CalcSize(content);
            Rect rect = new Rect(Event.current.mousePosition - (size), size);
            EditorGUI.LabelField(rect, content, NodeEditorResources.styles.tooltip);
            Repaint();
        }
    }

    private string TypeToString(Type type) {
        if (type == typeof(float)) return "float";
        else if (type == typeof(int)) return "int";
        else if (type == typeof(long)) return "long";
        else if (type == typeof(double)) return "double";
        else if (type == typeof(string)) return "string";
        else if (type == typeof(bool)) return "bool";
        else if (type.IsGenericType) {
            string s = "";
            Type genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(List<>)) s = "List";
            else s = type.GetGenericTypeDefinition().ToString();

            Type[] types = type.GetGenericArguments();
            string[] stypes = new string[types.Length];
            for (int i = 0; i < types.Length; i++) {
                stypes[i] = TypeToString(types[i]);
            }
            return s + "<" + string.Join(", ", stypes) + ">";
        } else if (type.IsArray) {
            string rank = "";
            for (int i = 1; i < type.GetArrayRank(); i++) {
                rank += ",";
            }
            Type elementType = type.GetElementType();
            if (!elementType.IsArray) return TypeToString(elementType) + "[" + rank + "]";
            else {
                string s = TypeToString(elementType);
                int i = s.IndexOf('[');
                return s.Substring(0,i) + "["+rank+"]" + s.Substring(i);
            }
        } else return hoveredPort.type.ToString();
    }
}