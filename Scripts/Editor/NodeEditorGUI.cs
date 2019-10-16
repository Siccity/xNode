using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNodeEditor.Internal;

namespace XNodeEditor {
    /// <summary> Contains GUI methods </summary>
    public partial class NodeEditorWindow {
        public NodeGraphEditor graphEditor;
        private List<UnityEngine.Object> selectionCache;
        private List<XNode.Node> culledNodes;
        /// <summary> 19 if docked, 22 if not </summary>
        private int topPadding { get { return isDocked() ? 19 : 22; } }
        /// <summary> Executed after all other window GUI. Useful if Zoom is ruining your day. Automatically resets after being run.</summary>
        public event Action onLateGUI;

        private void OnGUI() {
            Event e = Event.current;
            Matrix4x4 m = GUI.matrix;
            if (graph == null) return;
            ValidateGraphEditor();
            Controls();

            DrawGrid(position, zoom, panOffset);
            DrawConnections();
            DrawDraggedConnection();
            DrawNodes();
            DrawSelectionBox();
            DrawTooltip();
            graphEditor.OnGUI();

            // Run and reset onLateGUI
            if (onLateGUI != null) {
                onLateGUI();
                onLateGUI = null;
            }

            GUI.matrix = m;
        }

        public static void BeginZoomed(Rect rect, float zoom, float topPadding) {
            GUI.EndClip();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            Vector4 padding = new Vector4(0, topPadding, 0, 0);
            padding *= zoom;
            GUI.BeginClip(new Rect(-((rect.width * zoom) - rect.width) * 0.5f, -(((rect.height * zoom) - rect.height) * 0.5f) + (topPadding * zoom),
                rect.width * zoom,
                rect.height * zoom));
        }

        public static void EndZoomed(Rect rect, float zoom, float topPadding) {
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-topPadding * zoom) + topPadding,
                0);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
        }

        public void DrawGrid(Rect rect, float zoom, Vector2 panOffset) {

            rect.position = Vector2.zero;

            Vector2 center = rect.size / 2f;
            Texture2D gridTex = graphEditor.GetGridTexture();
            Texture2D crossTex = graphEditor.GetSecondaryGridTexture();

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

        public void DrawSelectionBox() {
            if (currentActivity == NodeActivity.DragGrid) {
                Vector2 curPos = WindowToGridPosition(Event.current.mousePosition);
                Vector2 size = curPos - dragBoxStart;
                Rect r = new Rect(dragBoxStart, size);
                r.position = GridToWindowPosition(r.position);
                r.size /= zoom;
                Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));
            }
        }

        public static bool DropdownButton(string name, float width) {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
        }

        /// <summary> Show right-click context menu for hovered reroute </summary>
        void ShowRerouteContextMenu(RerouteReference reroute) {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Remove"), false, () => reroute.RemovePoint());
            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        }

        /// <summary> Show right-click context menu for hovered port </summary>
        void ShowPortContextMenu(XNode.NodePort hoveredPort) {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Clear Connections"), false, () => hoveredPort.ClearConnections());
            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        }

        /// <summary> Draw a bezier from output to input in grid coordinates </summary>
        public void DrawNoodle(Gradient gradient, NoodlePath path, NoodleStroke stroke, float thickness, List<Vector2> gridPoints) {
            Vector2[] windowPoints = gridPoints.Select(x => GridToWindowPosition(x)).ToArray();
            Handles.color = gradient.Evaluate(0f);
            int length = gridPoints.Count;
            switch (path) {
                case NoodlePath.Curvy:
                    Vector2 outputTangent = Vector2.right;
                    for (int i = 0; i < length - 1; i++) {
                        Vector2 inputTangent = Vector2.left;
                        // Cached most variables that repeat themselves here to avoid so many indexer calls :p
                        Vector2 point_a = windowPoints[i];
                        Vector2 point_b = windowPoints[i + 1];
                        float dist_ab = Vector2.Distance(point_a, point_b);
                        if (i == 0) outputTangent = Vector2.right * dist_ab * 0.01f * zoom;
                        if (i < length - 2) {
                            Vector2 point_c = windowPoints[i + 2];
                            Vector2 ab = (point_b - point_a).normalized;
                            Vector2 cb = (point_b - point_c).normalized;
                            Vector2 ac = (point_c - point_a).normalized;
                            Vector2 p = (ab + cb) * 0.5f;
                            float tangentLength = (dist_ab + Vector2.Distance(point_b, point_c)) * 0.005f * zoom;
                            float side = ((ac.x * (point_b.y - point_a.y)) - (ac.y * (point_b.x - point_a.x)));

                            p = new Vector2(-p.y, p.x) * Mathf.Sign(side) * tangentLength;
                            inputTangent = p;
                        } else {
                            inputTangent = Vector2.left * dist_ab * 0.01f * zoom;
                        }

                        // Calculates the tangents for the bezier's curves.
                        Vector2 tangent_a = point_a + outputTangent * 50 / zoom;
                        Vector2 tangent_b = point_b + inputTangent * 50 / zoom;
                        // Hover effect.
                        int division = Mathf.RoundToInt(.2f * dist_ab) + 3;
                        Vector3[] points = Handles.MakeBezierPoints(point_a, point_b, tangent_a, tangent_b, division);
                        int draw = 0;
                        // Coloring and bezier drawing.
                        for (int j = 0; j < points.Length - 1; j++) {
                            if (stroke == NoodleStroke.Dashed) {
                                draw++;
                                if (draw >= 2) draw = -2;
                                if (draw < 0) continue;
                            }

                            if (i == gridPoints.Count - 2) Handles.color = gradient.Evaluate((j + 1f) / points.Length);
                            Handles.DrawAAPolyLine(thickness, points[j], points[j + 1]);
                        }
                        outputTangent = -inputTangent;
                    }
                    break;
                case NoodlePath.Straight:
                    for (int i = 0; i < length - 1; i++) {
                        Vector2 point_a = windowPoints[i];
                        Vector2 point_b = windowPoints[i + 1];
                        // Draws the line with the coloring.
                        Vector2 prev_point = point_a;
                        // Approximately one segment per 5 pixels
                        int segments = (int) Vector2.Distance(point_a, point_b) / 5;

                        int draw = 0;
                        for (int j = 0; j <= segments; j++) {
                            draw++;
                            float t = j / (float) segments;
                            Vector2 lerp = Vector2.Lerp(point_a, point_b, t);
                            if (draw > 0) {
                                if (i == gridPoints.Count - 2) Handles.color = gradient.Evaluate(t);
                                Handles.DrawAAPolyLine(thickness, prev_point, lerp);
                            }
                            prev_point = lerp;
                            if (stroke == NoodleStroke.Dashed && draw >= 2) draw = -2;
                        }
                    }
                    break;
                case NoodlePath.Angled:
                    for (int i = 0; i < length - 1; i++) {
                        if (i == length - 1) continue; // Skip last index
                        if (windowPoints[i].x <= windowPoints[i + 1].x - (50 / zoom)) {
                            float midpoint = (windowPoints[i].x + windowPoints[i + 1].x) * 0.5f;
                            Vector2 start_1 = windowPoints[i];
                            Vector2 end_1 = windowPoints[i + 1];
                            start_1.x = midpoint;
                            end_1.x = midpoint;
                            if (i == gridPoints.Count - 2) {
                                Handles.DrawAAPolyLine(thickness, windowPoints[i], start_1);
                                Handles.color = gradient.Evaluate(0.5f);
                                Handles.DrawAAPolyLine(thickness, start_1, end_1);
                                Handles.color = gradient.Evaluate(1f);
                                Handles.DrawAAPolyLine(thickness, end_1, windowPoints[i + 1]);
                            } else {
                                Handles.DrawAAPolyLine(thickness, windowPoints[i], start_1);
                                Handles.DrawAAPolyLine(thickness, start_1, end_1);
                                Handles.DrawAAPolyLine(thickness, end_1, windowPoints[i + 1]);
                            }
                        } else {
                            float midpoint = (windowPoints[i].y + windowPoints[i + 1].y) * 0.5f;
                            Vector2 start_1 = windowPoints[i];
                            Vector2 end_1 = windowPoints[i + 1];
                            start_1.x += 25 / zoom;
                            end_1.x -= 25 / zoom;
                            Vector2 start_2 = start_1;
                            Vector2 end_2 = end_1;
                            start_2.y = midpoint;
                            end_2.y = midpoint;
                            if (i == gridPoints.Count - 2) {
                                Handles.DrawAAPolyLine(thickness, windowPoints[i], start_1);
                                Handles.color = gradient.Evaluate(0.25f);
                                Handles.DrawAAPolyLine(thickness, start_1, start_2);
                                Handles.color = gradient.Evaluate(0.5f);
                                Handles.DrawAAPolyLine(thickness, start_2, end_2);
                                Handles.color = gradient.Evaluate(0.75f);
                                Handles.DrawAAPolyLine(thickness, end_2, end_1);
                                Handles.color = gradient.Evaluate(1f);
                                Handles.DrawAAPolyLine(thickness, end_1, windowPoints[i + 1]);
                            } else {
                                Handles.DrawAAPolyLine(thickness, windowPoints[i], start_1);
                                Handles.DrawAAPolyLine(thickness, start_1, start_2);
                                Handles.DrawAAPolyLine(thickness, start_2, end_2);
                                Handles.DrawAAPolyLine(thickness, end_2, end_1);
                                Handles.DrawAAPolyLine(thickness, end_1, windowPoints[i + 1]);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary> Draws all connections </summary>
        public void DrawConnections() {
            Vector2 mousePos = Event.current.mousePosition;
            List<RerouteReference> selection = preBoxSelectionReroute != null ? new List<RerouteReference>(preBoxSelectionReroute) : new List<RerouteReference>();
            hoveredReroute = new RerouteReference();

            Color col = GUI.color;
            foreach (XNode.Node node in graph.nodes) {
                //If a null node is found, return. This can happen if the nodes associated script is deleted. It is currently not possible in Unity to delete a null asset.
                if (node == null) continue;

                // Draw full connections and output > reroute
                foreach (XNode.NodePort output in node.Outputs) {
                    //Needs cleanup. Null checks are ugly
                    Rect fromRect;
                    if (!_portConnectionPoints.TryGetValue(output, out fromRect)) continue;

                    Color portColor = graphEditor.GetPortColor(output);
                    for (int k = 0; k < output.ConnectionCount; k++) {
                        XNode.NodePort input = output.GetConnection(k);

                        Gradient noodleGradient = graphEditor.GetNoodleGradient(output, input);
                        float noodleThickness = graphEditor.GetNoodleThickness(output, input);
                        NoodlePath noodlePath = graphEditor.GetNoodlePath(output, input);
                        NoodleStroke noodleStroke = graphEditor.GetNoodleStroke(output, input);

                        // Error handling
                        if (input == null) continue; //If a script has been updated and the port doesn't exist, it is removed and null is returned. If this happens, return.
                        if (!input.IsConnectedTo(output)) input.Connect(output);
                        Rect toRect;
                        if (!_portConnectionPoints.TryGetValue(input, out toRect)) continue;

                        List<Vector2> reroutePoints = output.GetReroutePoints(k);

                        List<Vector2> gridPoints = new List<Vector2>();
                        gridPoints.Add(fromRect.center);
                        gridPoints.AddRange(reroutePoints);
                        gridPoints.Add(toRect.center);
                        DrawNoodle(noodleGradient, noodlePath, noodleStroke, noodleThickness, gridPoints);

                        // Loop through reroute points again and draw the points
                        for (int i = 0; i < reroutePoints.Count; i++) {
                            RerouteReference rerouteRef = new RerouteReference(output, k, i);
                            // Draw reroute point at position
                            Rect rect = new Rect(reroutePoints[i], new Vector2(12, 12));
                            rect.position = new Vector2(rect.position.x - 6, rect.position.y - 6);
                            rect = GridToWindowRect(rect);

                            // Draw selected reroute points with an outline
                            if (selectedReroutes.Contains(rerouteRef)) {
                                GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
                                GUI.DrawTexture(rect, NodeEditorResources.dotOuter);
                            }

                            GUI.color = portColor;
                            GUI.DrawTexture(rect, NodeEditorResources.dot);
                            if (rect.Overlaps(selectionBox)) selection.Add(rerouteRef);
                            if (rect.Contains(mousePos)) hoveredReroute = rerouteRef;

                        }
                    }
                }
            }
            GUI.color = col;
            if (Event.current.type != EventType.Layout && currentActivity == NodeActivity.DragGrid) selectedReroutes = selection;
        }

        private void DrawNodes() {
            Event e = Event.current;
            if (e.type == EventType.Layout) {
                selectionCache = new List<UnityEngine.Object>(Selection.objects);
            }

            System.Reflection.MethodInfo onValidate = null;
            if (Selection.activeObject != null && Selection.activeObject is XNode.Node) {
                onValidate = Selection.activeObject.GetType().GetMethod("OnValidate");
                if (onValidate != null) EditorGUI.BeginChangeCheck();
            }

            BeginZoomed(position, zoom, topPadding);

            Vector2 mousePos = Event.current.mousePosition;

            if (e.type != EventType.Layout) {
                hoveredNode = null;
                hoveredPort = null;
            }

            List<UnityEngine.Object> preSelection = preBoxSelection != null ? new List<UnityEngine.Object>(preBoxSelection) : new List<UnityEngine.Object>();

            // Selection box stuff
            Vector2 boxStartPos = GridToWindowPositionNoClipped(dragBoxStart);
            Vector2 boxSize = mousePos - boxStartPos;
            if (boxSize.x < 0) { boxStartPos.x += boxSize.x; boxSize.x = Mathf.Abs(boxSize.x); }
            if (boxSize.y < 0) { boxStartPos.y += boxSize.y; boxSize.y = Mathf.Abs(boxSize.y); }
            Rect selectionBox = new Rect(boxStartPos, boxSize);

            //Save guiColor so we can revert it
            Color guiColor = GUI.color;

            if (e.type == EventType.Layout) culledNodes = new List<XNode.Node>();
            for (int n = 0; n < graph.nodes.Count; n++) {
                // Skip null nodes. The user could be in the process of renaming scripts, so removing them at this point is not advisable.
                if (graph.nodes[n] == null) continue;
                if (n >= graph.nodes.Count) return;
                XNode.Node node = graph.nodes[n];

                // Culling
                if (e.type == EventType.Layout) {
                    // Cull unselected nodes outside view
                    if (!Selection.Contains(node) && ShouldBeCulled(node)) {
                        culledNodes.Add(node);
                        continue;
                    }
                } else if (culledNodes.Contains(node)) continue;

                if (e.type == EventType.Repaint) {
                    _portConnectionPoints = _portConnectionPoints.Where(x => x.Key.node != node).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                NodeEditor nodeEditor = NodeEditor.GetEditor(node, this);

                NodeEditor.portPositions.Clear();

                // Set default label width. This is potentially overridden in OnBodyGUI
                EditorGUIUtility.labelWidth = 84;

                //Get node position
                Vector2 nodePos = GridToWindowPositionNoClipped(node.position);

                GUILayout.BeginArea(new Rect(nodePos, new Vector2(nodeEditor.GetWidth(), 4000)));

                bool selected = selectionCache.Contains(graph.nodes[n]);

                if (selected) {
                    GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                    GUIStyle highlightStyle = new GUIStyle(NodeEditorResources.styles.nodeHighlight);
                    highlightStyle.padding = style.padding;
                    style.padding = new RectOffset();
                    GUI.color = nodeEditor.GetTint();
                    GUILayout.BeginVertical(style);
                    GUI.color = NodeEditorPreferences.GetSettings().highlightColor;
                    GUILayout.BeginVertical(new GUIStyle(highlightStyle));
                } else {
                    GUIStyle style = new GUIStyle(nodeEditor.GetBodyStyle());
                    GUI.color = nodeEditor.GetTint();
                    GUILayout.BeginVertical(style);
                }

                GUI.color = guiColor;
                EditorGUI.BeginChangeCheck();

                //Draw node contents
                nodeEditor.OnHeaderGUI();
                nodeEditor.OnBodyGUI();

                //If user changed a value, notify other scripts through onUpdateNode
                if (EditorGUI.EndChangeCheck()) {
                    if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                    EditorUtility.SetDirty(node);
                    nodeEditor.serializedObject.ApplyModifiedProperties();
                }

                GUILayout.EndVertical();

                //Cache data about the node for next frame
                if (e.type == EventType.Repaint) {
                    Vector2 size = GUILayoutUtility.GetLastRect().size;
                    if (nodeSizes.ContainsKey(node)) nodeSizes[node] = size;
                    else nodeSizes.Add(node, size);

                    foreach (var kvp in NodeEditor.portPositions) {
                        Vector2 portHandlePos = kvp.Value;
                        portHandlePos += node.position;
                        Rect rect = new Rect(portHandlePos.x - 8, portHandlePos.y - 8, 16, 16);
                        portConnectionPoints[kvp.Key] = rect;
                    }
                }

                if (selected) GUILayout.EndVertical();

                if (e.type != EventType.Layout) {
                    //Check if we are hovering this node
                    Vector2 nodeSize = GUILayoutUtility.GetLastRect().size;
                    Rect windowRect = new Rect(nodePos, nodeSize);
                    if (windowRect.Contains(mousePos)) hoveredNode = node;

                    //If dragging a selection box, add nodes inside to selection
                    if (currentActivity == NodeActivity.DragGrid) {
                        if (windowRect.Overlaps(selectionBox)) preSelection.Add(node);
                    }

                    //Check if we are hovering any of this nodes ports
                    //Check input ports
                    foreach (XNode.NodePort input in node.Inputs) {
                        //Check if port rect is available
                        if (!portConnectionPoints.ContainsKey(input)) continue;
                        Rect r = GridToWindowRectNoClipped(portConnectionPoints[input]);
                        if (r.Contains(mousePos)) hoveredPort = input;
                    }
                    //Check all output ports
                    foreach (XNode.NodePort output in node.Outputs) {
                        //Check if port rect is available
                        if (!portConnectionPoints.ContainsKey(output)) continue;
                        Rect r = GridToWindowRectNoClipped(portConnectionPoints[output]);
                        if (r.Contains(mousePos)) hoveredPort = output;
                    }
                }

                GUILayout.EndArea();
            }

            if (e.type != EventType.Layout && currentActivity == NodeActivity.DragGrid) Selection.objects = preSelection.ToArray();
            EndZoomed(position, zoom, topPadding);

            //If a change in is detected in the selected node, call OnValidate method. 
            //This is done through reflection because OnValidate is only relevant in editor, 
            //and thus, the code should not be included in build.
            if (onValidate != null && EditorGUI.EndChangeCheck()) onValidate.Invoke(Selection.activeObject, null);
        }

        private bool ShouldBeCulled(XNode.Node node) {

            Vector2 nodePos = GridToWindowPositionNoClipped(node.position);
            if (nodePos.x / _zoom > position.width) return true; // Right
            else if (nodePos.y / _zoom > position.height) return true; // Bottom
            else if (nodeSizes.ContainsKey(node)) {
                Vector2 size = nodeSizes[node];
                if (nodePos.x + size.x < 0) return true; // Left
                else if (nodePos.y + size.y < 0) return true; // Top
            }
            return false;
        }

        private void DrawTooltip() {
            if (hoveredPort != null && NodeEditorPreferences.GetSettings().portTooltips && graphEditor != null) {
                string tooltip = graphEditor.GetPortTooltip(hoveredPort);
                if (string.IsNullOrEmpty(tooltip)) return;
                GUIContent content = new GUIContent(tooltip);
                Vector2 size = NodeEditorResources.styles.tooltip.CalcSize(content);
                size.x += 8;
                Rect rect = new Rect(Event.current.mousePosition - (size), size);
                EditorGUI.LabelField(rect, content, NodeEditorResources.styles.tooltip);
                Repaint();
            }
        }
    }
}