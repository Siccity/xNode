using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNodeEditor.Internal;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace XNodeEditor {
    public partial class NodeEditorWindow {
        public enum NodeActivity { Idle, HoldNode, DragNode, HoldGrid, DragGrid }
        public static NodeActivity currentActivity = NodeActivity.Idle;
        public static bool isPanning { get; private set; }
        public static Vector2[] dragOffset;

        public static XNode.Node[] copyBuffer = null;

        public bool IsDraggingPort { get { return draggedOutput != null; } }
        public bool IsHoveringPort { get { return hoveredPort != null; } }
        public bool IsHoveringNode { get { return hoveredNode != null; } }
        public bool IsHoveringReroute { get { return hoveredReroute.port != null; } }

        /// <summary> Return the dragged port or null if not exist </summary>
        public XNode.NodePort DraggedOutputPort { get { var result = draggedOutput; return result; } }
        /// <summary> Return the Hovered port or null if not exist </summary>
        public XNode.NodePort HoveredPort { get { var result = hoveredPort; return result; } }
        /// <summary> Return the Hovered node or null if not exist </summary>
        public XNode.Node HoveredNode { get { var result = hoveredNode; return result; } }

        private XNode.Node hoveredNode = null;
        [NonSerialized] public XNode.NodePort hoveredPort = null;
        [NonSerialized] private XNode.NodePort draggedOutput = null;
        [NonSerialized] private XNode.NodePort draggedOutputTarget = null;
        [NonSerialized] private XNode.NodePort autoConnectOutput = null;
        [NonSerialized] private List<Vector2> draggedOutputReroutes = new List<Vector2>();

        private RerouteReference hoveredReroute = new RerouteReference();
        public List<RerouteReference> selectedReroutes = new List<RerouteReference>();
        private Vector2 dragBoxStart;
        private UnityEngine.Object[] preBoxSelection;
        private RerouteReference[] preBoxSelectionReroute;
        private Rect selectionBox;
        private bool isDoubleClick = false;
        private Vector2 lastMousePosition;
        private float dragThreshold = 1f;

        public void Controls() {
            wantsMouseMove = true;
            var e = Event.current;
            switch (e.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (e.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();
                        graphEditor.OnDropObjects(DragAndDrop.objectReferences);
                    }
                    break;
                case EventType.MouseMove:
                    //Keyboard commands will not get correct mouse position from Event
                    lastMousePosition = e.mousePosition;
                    break;
                case EventType.ScrollWheel:
                    var oldZoom = zoom;
                    if (e.delta.y > 0)
                    {
                        zoom += 0.1f * zoom;
                    }
                    else
                    {
                        zoom -= 0.1f * zoom;
                    }

                    if (NodeEditorPreferences.GetSettings().zoomToMouse)
                    {
                        panOffset += (1 - oldZoom / zoom) * (WindowToGridPosition(e.mousePosition) + panOffset);
                    }

                    break;
                case EventType.MouseDrag:
                    if (e.button == 0) {
                        if (IsDraggingPort) {
                            // Set target even if we can't connect, so as to prevent auto-conn menu from opening erroneously
                            if (IsHoveringPort && hoveredPort.IsInput && !draggedOutput.IsConnectedTo(hoveredPort)) {
                                draggedOutputTarget = hoveredPort;
                            } else {
                                draggedOutputTarget = null;
                            }
                            Repaint();
                        } else if (currentActivity == NodeActivity.HoldNode) {
                            RecalculateDragOffsets(e);
                            currentActivity = NodeActivity.DragNode;
                            Repaint();
                        }
                        if (currentActivity == NodeActivity.DragNode) {
                            // Holding ctrl inverts grid snap
                            var gridSnap = NodeEditorPreferences.GetSettings().gridSnap;
                            if (e.control)
                            {
                                gridSnap = !gridSnap;
                            }

                            var mousePos = WindowToGridPosition(e.mousePosition);
                            // Move selected nodes with offset
                            for (var i = 0; i < Selection.objects.Length; i++) {
                                if (Selection.objects[i] is XNode.Node) {
                                    var node = Selection.objects[i] as XNode.Node;
                                    Undo.RecordObject(node, "Moved Node");
                                    var initial = node.position;
                                    node.position = mousePos + dragOffset[i];
                                    if (gridSnap) {
                                        node.position.x = (Mathf.Round((node.position.x + 8) / 16) * 16) - 8;
                                        node.position.y = (Mathf.Round((node.position.y + 8) / 16) * 16) - 8;
                                    }

                                    // Offset portConnectionPoints instantly if a node is dragged so they aren't delayed by a frame.
                                    var offset = node.position - initial;
                                    if (offset.sqrMagnitude > 0) {
                                        foreach (var output in node.Outputs) {
                                            Rect rect;
                                            if (portConnectionPoints.TryGetValue(output, out rect)) {
                                                rect.position += offset;
                                                portConnectionPoints[output] = rect;
                                            }
                                        }

                                        foreach (var input in node.Inputs) {
                                            Rect rect;
                                            if (portConnectionPoints.TryGetValue(input, out rect)) {
                                                rect.position += offset;
                                                portConnectionPoints[input] = rect;
                                            }
                                        }
                                    }
                                }
                            }
                            // Move selected reroutes with offset
                            for (var i = 0; i < selectedReroutes.Count; i++) {
                                var pos = mousePos + dragOffset[Selection.objects.Length + i];
                                if (gridSnap) {
                                    pos.x = (Mathf.Round(pos.x / 16) * 16);
                                    pos.y = (Mathf.Round(pos.y / 16) * 16);
                                }
                                selectedReroutes[i].SetPoint(pos);
                            }
                            Repaint();
                        } else if (currentActivity == NodeActivity.HoldGrid) {
                            currentActivity = NodeActivity.DragGrid;
                            preBoxSelection = Selection.objects;
                            preBoxSelectionReroute = selectedReroutes.ToArray();
                            dragBoxStart = WindowToGridPosition(e.mousePosition);
                            Repaint();
                        } else if (currentActivity == NodeActivity.DragGrid) {
                            var boxStartPos = GridToWindowPosition(dragBoxStart);
                            var boxSize = e.mousePosition - boxStartPos;
                            if (boxSize.x < 0) { boxStartPos.x += boxSize.x; boxSize.x = Mathf.Abs(boxSize.x); }
                            if (boxSize.y < 0) { boxStartPos.y += boxSize.y; boxSize.y = Mathf.Abs(boxSize.y); }
                            selectionBox = new Rect(boxStartPos, boxSize);
                            Repaint();
                        }
                    } else if (e.button == 1 || e.button == 2) {
                        //check drag threshold for larger screens
                        if (e.delta.magnitude > dragThreshold) {
                            panOffset += e.delta * zoom;
                            isPanning = true;
                        }
                    }
                    break;
                case EventType.MouseDown:
                    Repaint();
                    if (e.button == 0) {
                        draggedOutputReroutes.Clear();

                        if (IsHoveringPort) {
                            if (hoveredPort.IsOutput) {
                                draggedOutput = hoveredPort;
                                autoConnectOutput = hoveredPort;
                            } else {
                                hoveredPort.VerifyConnections();
                                autoConnectOutput = null;
                                if (hoveredPort.IsConnected) {
                                    var node = hoveredPort.node;
                                    var output = hoveredPort.Connection;
                                    var outputConnectionIndex = output.GetConnectionIndex(hoveredPort);
                                    draggedOutputReroutes = output.GetReroutePoints(outputConnectionIndex);
                                    hoveredPort.Disconnect(output);
                                    draggedOutput = output;
                                    draggedOutputTarget = hoveredPort;
                                    if (NodeEditor.onUpdateNode != null)
                                    {
                                        NodeEditor.onUpdateNode(node);
                                    }
                                }
                            }
                        } else if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                            // If mousedown on node header, select or deselect
                            if (!Selection.Contains(hoveredNode)) {
                                SelectNode(hoveredNode, e.control || e.shift);
                                if (!e.control && !e.shift)
                                {
                                    selectedReroutes.Clear();
                                }
                            } else if (e.control || e.shift)
                            {
                                DeselectNode(hoveredNode);
                            }

                            // Cache double click state, but only act on it in MouseUp - Except ClickCount only works in mouseDown.
                            isDoubleClick = (e.clickCount == 2);

                            e.Use();
                            currentActivity = NodeActivity.HoldNode;
                        } else if (IsHoveringReroute) {
                            // If reroute isn't selected
                            if (!selectedReroutes.Contains(hoveredReroute)) {
                                // Add it
                                if (e.control || e.shift)
                                {
                                    selectedReroutes.Add(hoveredReroute);
                                }
                                // Select it
                                else {
                                    selectedReroutes = new List<RerouteReference>() { hoveredReroute };
                                    Selection.activeObject = null;
                                }

                            }
                            // Deselect
                            else if (e.control || e.shift)
                            {
                                selectedReroutes.Remove(hoveredReroute);
                            }

                            e.Use();
                            currentActivity = NodeActivity.HoldNode;
                        }
                        // If mousedown on grid background, deselect all
                        else if (!IsHoveringNode) {
                            currentActivity = NodeActivity.HoldGrid;
                            if (!e.control && !e.shift) {
                                selectedReroutes.Clear();
                                Selection.activeObject = null;
                            }
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (e.button == 0) {
                        //Port drag release
                        if (IsDraggingPort) {
                            // If connection is valid, save it
                            if (draggedOutputTarget != null && graphEditor.CanConnect(draggedOutput, draggedOutputTarget)) {
                                var node = draggedOutputTarget.node;
                                if (graph.nodes.Count != 0)
                                {
                                    draggedOutput.Connect(draggedOutputTarget);
                                }

                                // ConnectionIndex can be -1 if the connection is removed instantly after creation
                                var connectionIndex = draggedOutput.GetConnectionIndex(draggedOutputTarget);
                                if (connectionIndex != -1) {
                                    draggedOutput.GetReroutePoints(connectionIndex).AddRange(draggedOutputReroutes);
                                    if (NodeEditor.onUpdateNode != null)
                                    {
                                        NodeEditor.onUpdateNode(node);
                                    }

                                    EditorUtility.SetDirty(graph);
                                }
                            }
                            // Open context menu for auto-connection if there is no target node
                            else if (draggedOutputTarget == null && NodeEditorPreferences.GetSettings().dragToCreate && autoConnectOutput != null) {
                                var menu = new GenericMenu();
                                graphEditor.AddContextMenuItems(menu, draggedOutput.ValueType);
                                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                            }
                            //Release dragged connection
                            draggedOutput = null;
                            draggedOutputTarget = null;
                            EditorUtility.SetDirty(graph);
                            if (NodeEditorPreferences.GetSettings().autoSave)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        } else if (currentActivity == NodeActivity.DragNode) {
                            var nodes = Selection.objects.Where(x => x is XNode.Node).Select(x => x as XNode.Node);
                            foreach (var node in nodes)
                            {
                                EditorUtility.SetDirty(node);
                            }

                            if (NodeEditorPreferences.GetSettings().autoSave)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        } else if (!IsHoveringNode) {
                            // If click outside node, release field focus
                            if (!isPanning) {
                                EditorGUI.FocusTextInControl(null);
                                EditorGUIUtility.editingTextField = false;
                            }
                            if (NodeEditorPreferences.GetSettings().autoSave)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        }

                        // If click node header, select it.
                        if (currentActivity == NodeActivity.HoldNode && !(e.control || e.shift)) {
                            selectedReroutes.Clear();
                            SelectNode(hoveredNode, false);

                            // Double click to center node
                            if (isDoubleClick) {
                                var nodeDimension = nodeSizes.ContainsKey(hoveredNode) ? nodeSizes[hoveredNode] / 2 : Vector2.zero;
                                panOffset = -hoveredNode.position - nodeDimension;
                            }
                        }

                        // If click reroute, select it.
                        if (IsHoveringReroute && !(e.control || e.shift)) {
                            selectedReroutes = new List<RerouteReference>() { hoveredReroute };
                            Selection.activeObject = null;
                        }

                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    } else if (e.button == 1 || e.button == 2) {
                        if (!isPanning) {
                            if (IsDraggingPort) {
                                draggedOutputReroutes.Add(WindowToGridPosition(e.mousePosition));
                            } else if (currentActivity == NodeActivity.DragNode && Selection.activeObject == null && selectedReroutes.Count == 1) {
                                selectedReroutes[0].InsertPoint(selectedReroutes[0].GetPoint());
                                selectedReroutes[0] = new RerouteReference(selectedReroutes[0].port, selectedReroutes[0].connectionIndex, selectedReroutes[0].pointIndex + 1);
                            } else if (IsHoveringReroute) {
                                ShowRerouteContextMenu(hoveredReroute);
                            } else if (IsHoveringPort) {
                                ShowPortContextMenu(hoveredPort);
                            } else if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                                if (!Selection.Contains(hoveredNode))
                                {
                                    SelectNode(hoveredNode, false);
                                }

                                autoConnectOutput = null;
                                var menu = new GenericMenu();
                                NodeEditor.GetEditor(hoveredNode, this).AddContextMenuItems(menu);
                                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                                e.Use(); // Fixes copy/paste context menu appearing in Unity 5.6.6f2 - doesn't occur in 2018.3.2f1 Probably needs to be used in other places.
                            } else if (!IsHoveringNode) {
                                autoConnectOutput = null;
                                var menu = new GenericMenu();
                                graphEditor.AddContextMenuItems(menu);
                                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
                            }
                        }
                        isPanning = false;
                    }
                    // Reset DoubleClick
                    isDoubleClick = false;
                    break;
                case EventType.KeyDown:
                    if (EditorGUIUtility.editingTextField || GUIUtility.keyboardControl != 0)
                    {
                        break;
                    }
                    else if (e.keyCode == KeyCode.F)
                    {
                        Home();
                    }

                    if (NodeEditorUtilities.IsMac()) {
                        if (e.keyCode == KeyCode.Return)
                        {
                            RenameSelectedNode();
                        }
                    } else {
                        if (e.keyCode == KeyCode.F2)
                        {
                            RenameSelectedNode();
                        }
                    }
                    if (e.keyCode == KeyCode.A) {
                        if (Selection.objects.Any(x => graph.nodes.Contains(x as XNode.Node))) {
                            foreach (var node in graph.nodes) {
                                DeselectNode(node);
                            }
                        } else {
                            foreach (var node in graph.nodes) {
                                SelectNode(node, true);
                            }
                        }
                        Repaint();
                    }
                    break;
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    if (e.commandName == "SoftDelete") {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            RemoveSelectedNodes();
                        }

                        e.Use();
                    } else if (NodeEditorUtilities.IsMac() && e.commandName == "Delete") {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            RemoveSelectedNodes();
                        }

                        e.Use();
                    } else if (e.commandName == "Duplicate") {
                        if (e.type == EventType.ExecuteCommand)
                        {
                            DuplicateSelectedNodes();
                        }

                        e.Use();
                    } else if (e.commandName == "Copy") {
                        if (!EditorGUIUtility.editingTextField) {
                            if (e.type == EventType.ExecuteCommand)
                            {
                                CopySelectedNodes();
                            }

                            e.Use();
                        }
                    } else if (e.commandName == "Paste") {
                        if (!EditorGUIUtility.editingTextField) {
                            if (e.type == EventType.ExecuteCommand)
                            {
                                PasteNodes(WindowToGridPosition(lastMousePosition));
                            }

                            e.Use();
                        }
                    }
                    Repaint();
                    break;
                case EventType.Ignore:
                    // If release mouse outside window
                    if (e.rawType == EventType.MouseUp && currentActivity == NodeActivity.DragGrid) {
                        Repaint();
                        currentActivity = NodeActivity.Idle;
                    }
                    break;
            }
        }

        private void RecalculateDragOffsets(Event current) {
            dragOffset = new Vector2[Selection.objects.Length + selectedReroutes.Count];
            // Selected nodes
            for (var i = 0; i < Selection.objects.Length; i++) {
                if (Selection.objects[i] is XNode.Node) {
                    var node = Selection.objects[i] as XNode.Node;
                    dragOffset[i] = node.position - WindowToGridPosition(current.mousePosition);
                }
            }

            // Selected reroutes
            for (var i = 0; i < selectedReroutes.Count; i++) {
                dragOffset[Selection.objects.Length + i] = selectedReroutes[i].GetPoint() - WindowToGridPosition(current.mousePosition);
            }
        }

        /// <summary> Puts all selected nodes in focus. If no nodes are present, resets view and zoom to to origin </summary>
        public void Home() {
            var nodes = Selection.objects.Where(o => o is XNode.Node).Cast<XNode.Node>().ToList();
            if (nodes.Count > 0) {
                var minPos = nodes.Select(x => x.position).Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
                var maxPos = nodes.Select(x => x.position + (nodeSizes.ContainsKey(x) ? nodeSizes[x] : Vector2.zero)).Aggregate((x, y) => new Vector2(Mathf.Max(x.x, y.x), Mathf.Max(x.y, y.y)));
                panOffset = -(minPos + (maxPos - minPos) / 2f);
            } else {
                zoom = 2;
                panOffset = Vector2.zero;
            }
        }

        /// <summary> Remove nodes in the graph in Selection.objects</summary>
        public void RemoveSelectedNodes() {
            // We need to delete reroutes starting at the highest point index to avoid shifting indices
            selectedReroutes = selectedReroutes.OrderByDescending(x => x.pointIndex).ToList();
            for (var i = 0; i < selectedReroutes.Count; i++) {
                selectedReroutes[i].RemovePoint();
            }
            selectedReroutes.Clear();
            foreach (var item in Selection.objects) {
                if (item is XNode.Node) {
                    var node = item as XNode.Node;
                    graphEditor.RemoveNode(node);
                }
            }
        }

        /// <summary> Initiate a rename on the currently selected node </summary>
        public void RenameSelectedNode() {
            if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
                var node = Selection.activeObject as XNode.Node;
                Vector2 size;
                if (nodeSizes.TryGetValue(node, out size)) {
                    RenamePopup.Show(Selection.activeObject, size.x);
                } else {
                    RenamePopup.Show(Selection.activeObject);
                }
            }
        }

        /// <summary> Draw this node on top of other nodes by placing it last in the graph.nodes list </summary>
        public void MoveNodeToTop(XNode.Node node) {
            int index;
            while ((index = graph.nodes.IndexOf(node)) != graph.nodes.Count - 1) {
                graph.nodes[index] = graph.nodes[index + 1];
                graph.nodes[index + 1] = node;
            }
        }

        /// <summary> Duplicate selected nodes and select the duplicates </summary>
        public void DuplicateSelectedNodes() {
            // Get selected nodes which are part of this graph
            var selectedNodes = Selection.objects.Select(x => x as XNode.Node).Where(x => x != null && x.graph == graph).ToArray();
            if (selectedNodes == null || selectedNodes.Length == 0)
            {
                return;
            }

            // Get top left node position
            var topLeftNode = selectedNodes.Select(x => x.position).Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
            InsertDuplicateNodes(selectedNodes, topLeftNode + new Vector2(30, 30));
        }

        public void CopySelectedNodes() {
            copyBuffer = Selection.objects.Select(x => x as XNode.Node).Where(x => x != null && x.graph == graph).ToArray();
        }

        public void PasteNodes(Vector2 pos) {
            InsertDuplicateNodes(copyBuffer, pos);
        }

        private void InsertDuplicateNodes(XNode.Node[] nodes, Vector2 topLeft) {
            if (nodes == null || nodes.Length == 0)
            {
                return;
            }

            // Get top-left node
            var topLeftNode = nodes.Select(x => x.position).Aggregate((x, y) => new Vector2(Mathf.Min(x.x, y.x), Mathf.Min(x.y, y.y)));
            var offset = topLeft - topLeftNode;

            var newNodes = new UnityEngine.Object[nodes.Length];
            var substitutes = new Dictionary<XNode.Node, XNode.Node>();
            for (var i = 0; i < nodes.Length; i++) {
                var srcNode = nodes[i];
                if (srcNode == null)
                {
                    continue;
                }

                // Check if user is allowed to add more of given node type
                XNode.Node.DisallowMultipleNodesAttribute disallowAttrib;
                var nodeType = srcNode.GetType();
                if (NodeEditorUtilities.GetAttrib(nodeType, out disallowAttrib)) {
                    var typeCount = graph.nodes.Count(x => x.GetType() == nodeType);
                    if (typeCount >= disallowAttrib.max)
                    {
                        continue;
                    }
                }

                var newNode = graphEditor.CopyNode(srcNode);
                substitutes.Add(srcNode, newNode);
                newNode.position = srcNode.position + offset;
                newNodes[i] = newNode;
            }

            // Walk through the selected nodes again, recreate connections, using the new nodes
            for (var i = 0; i < nodes.Length; i++) {
                var srcNode = nodes[i];
                if (srcNode == null)
                {
                    continue;
                }

                foreach (var port in srcNode.Ports) {
                    for (var c = 0; c < port.ConnectionCount; c++) {
                        var inputPort = port.direction == XNode.NodePort.IO.Input ? port : port.GetConnection(c);
                        var outputPort = port.direction == XNode.NodePort.IO.Output ? port : port.GetConnection(c);

                        XNode.Node newNodeIn, newNodeOut;
                        if (substitutes.TryGetValue(inputPort.node, out newNodeIn) && substitutes.TryGetValue(outputPort.node, out newNodeOut)) {
                            newNodeIn.UpdatePorts();
                            newNodeOut.UpdatePorts();
                            inputPort = newNodeIn.GetInputPort(inputPort.fieldName);
                            outputPort = newNodeOut.GetOutputPort(outputPort.fieldName);
                        }
                        if (!inputPort.IsConnectedTo(outputPort))
                        {
                            inputPort.Connect(outputPort);
                        }
                    }
                }
            }
            EditorUtility.SetDirty(graph);
            // Select the new nodes
            Selection.objects = newNodes;
        }

        /// <summary> Draw a connection as we are dragging it </summary>
        public void DrawDraggedConnection() {
            if (IsDraggingPort) {
                var gradient = graphEditor.GetNoodleGradient(draggedOutput, null);
                var thickness = graphEditor.GetNoodleThickness(draggedOutput, null);
                var path = graphEditor.GetNoodlePath(draggedOutput, null);
                var stroke = graphEditor.GetNoodleStroke(draggedOutput, null);

                Rect fromRect;
                if (!_portConnectionPoints.TryGetValue(draggedOutput, out fromRect))
                {
                    return;
                }

                var gridPoints = new List<Vector2>();
                gridPoints.Add(fromRect.center);
                for (var i = 0; i < draggedOutputReroutes.Count; i++) {
                    gridPoints.Add(draggedOutputReroutes[i]);
                }
                if (draggedOutputTarget != null)
                {
                    gridPoints.Add(portConnectionPoints[draggedOutputTarget].center);
                }
                else
                {
                    gridPoints.Add(WindowToGridPosition(Event.current.mousePosition));
                }

                DrawNoodle(gradient, path, stroke, thickness, gridPoints);

                var portStyle = NodeEditorWindow.current.graphEditor.GetPortStyle(draggedOutput);
                var bgcol = Color.black;
                var frcol = gradient.colorKeys[0].color;
                bgcol.a = 0.6f;
                frcol.a = 0.6f;

                // Loop through reroute points again and draw the points
                for (var i = 0; i < draggedOutputReroutes.Count; i++) {
                    // Draw reroute point at position
                    var rect = new Rect(draggedOutputReroutes[i], new Vector2(16, 16));
                    rect.position = new Vector2(rect.position.x - 8, rect.position.y - 8);
                    rect = GridToWindowRect(rect);

                    NodeEditorGUILayout.DrawPortHandle(rect, bgcol, frcol, portStyle.normal.background, portStyle.active.background);
                }
            }
        }

        private bool IsHoveringTitle(XNode.Node node) {
            var mousePos = Event.current.mousePosition;
            //Get node position
            var nodePos = GridToWindowPosition(node.position);
            float width;
            Vector2 size;
            if (nodeSizes.TryGetValue(node, out size))
            {
                width = size.x;
            }
            else
            {
                width = 200;
            }

            var windowRect = new Rect(nodePos, new Vector2(width / zoom, 30 / zoom));
            return windowRect.Contains(mousePos);
        }

        /// <summary> Attempt to connect dragged output to target node </summary>
        public void AutoConnect(XNode.Node node) {
            if (autoConnectOutput == null)
            {
                return;
            }

            // Find compatible input port
            var inputPort = node.Ports.FirstOrDefault(x => x.IsInput && graphEditor.CanConnect(autoConnectOutput, x));
            if (inputPort != null)
            {
                autoConnectOutput.Connect(inputPort);
            }

            // Save changes
            EditorUtility.SetDirty(graph);
            if (NodeEditorPreferences.GetSettings().autoSave)
            {
                AssetDatabase.SaveAssets();
            }

            autoConnectOutput = null;
        }
    }
}
