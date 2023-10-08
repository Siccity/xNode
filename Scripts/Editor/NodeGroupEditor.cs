using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor.Internal;

namespace XNodeEditor.NodeGroups
{
    [CustomNodeEditor(typeof(NodeGroup))]
    public class NodeGroupEditor : NodeEditor
    {
        private NodeGroup group => _group != null ? _group : _group = target as NodeGroup;
        private NodeGroup _group;
        public static Texture2D corner =>
            _corner != null ? _corner : _corner = Resources.Load<Texture2D>("xnode_corner");
        private static Texture2D _corner;
        private bool _isResizing;
        private Vector2 _size;
        private float _currentHeight;
        private Vector2 _draggingOffset;

        private List<Vector2> _childNodesDragOffsets;

        private const int mouseRectPadding = 4;
        private const int mouseRectMargin = 30;

        private GUIStyle headerStyle;

        public override void OnCreate()
        {
            _currentHeight = group.height;
            headerStyle = new GUIStyle(NodeEditorResources.styles.nodeHeaderLabel);
            headerStyle.fontSize = 18;
        }

        public override void OnHeaderGUI()
        {
            GUILayout.Label(target.name, headerStyle, GUILayout.Height(30));
        }

        public override void OnBodyGUI()
        {
            NodeEditorWindow.current.wantsMouseMove = true;
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (_isResizing)
                    {
                        group.width = Mathf.Max(200, (int)e.mousePosition.x + (int)_draggingOffset.x + 16);
                        group.height = Mathf.Max(100, (int)e.mousePosition.y + (int)_draggingOffset.y - 34);
                        _currentHeight = group.height;
                        NodeEditorWindow.current.Repaint();
                    }

                    break;
                case EventType.MouseDown:
                    // Ignore everything except left clicks
                    if (e.button != 0)
                    {
                        return;
                    }

                    _childNodesDragOffsets = new List<Vector2>(group.GetNodes().Count);
                    foreach (Node node in group.GetNodes())
                    {
                        _childNodesDragOffsets.Add(node.position -
                                                   NodeEditorWindow.current.WindowToGridPosition(
                                                       e.mousePosition));
                    }

                    if (NodeEditorWindow.current.nodeSizes.TryGetValue(target, out _size))
                    {
                        // Mouse position checking is in node local space
                        Rect lowerRight = new Rect(_size.x - (mouseRectMargin + mouseRectPadding),
                            _size.y - (mouseRectMargin + mouseRectPadding), mouseRectMargin, mouseRectMargin);
                        if (lowerRight.Contains(e.mousePosition))
                        {
                            _isResizing = true;
                            _draggingOffset = _size - e.mousePosition - new Vector2(GetBodyStyle().padding.right,
                                GetBodyStyle().padding.bottom);
                        }
                    }

                    break;
                case EventType.MouseUp:
                    _isResizing = false;
                    // Select nodes inside the group
                    if (false && Selection.Contains(target))
                    {
                        var selection = Selection.objects.ToList();
                        // Select Nodes
                        selection.AddRange(group.GetNodes());
                        // Select Reroutes
                        foreach (Node node in target.graph.nodes)
                        {
                            if (node != null)
                            {
                                foreach (NodePort port in node.Ports)
                                {
                                    for (int i = 0; i < port.ConnectionCount; i++)
                                    {
                                        var reroutes = port.GetReroutePoints(i);
                                        for (int k = 0; k < reroutes.Count; k++)
                                        {
                                            Vector2 p = reroutes[k];
                                            if (p.x < group.position.x)
                                            {
                                                continue;
                                            }

                                            if (p.y < group.position.y)
                                            {
                                                continue;
                                            }

                                            if (p.x > group.position.x + group.width)
                                            {
                                                continue;
                                            }

                                            if (p.y > group.position.y + group.height + 30)
                                            {
                                                continue;
                                            }

                                            if (NodeEditorWindow.current.selectedReroutes.Any(x =>
                                                    x.port == port && x.connectionIndex == i && x.pointIndex == k))
                                            {
                                                continue;
                                            }

                                            NodeEditorWindow.current.selectedReroutes.Add(
                                                new RerouteReference(port, i, k)
                                            );
                                        }
                                    }
                                }
                            }
                        }

                        Selection.objects = selection.Distinct().ToArray();
                    }

                    break;
                case EventType.Repaint:
                    // Move to bottom
                    if (target.graph.nodes.IndexOf(target) != 0)
                    {
                        target.graph.nodes.Remove(target);
                        target.graph.nodes.Insert(0, target);
                    }

                    // Add scale cursors
                    if (NodeEditorWindow.current.nodeSizes.TryGetValue(target, out _size))
                    {
                        Rect lowerRight = new Rect(target.position, new Vector2(mouseRectMargin, mouseRectMargin));
                        lowerRight.y += _size.y - (mouseRectMargin + mouseRectPadding);
                        lowerRight.x += _size.x - (mouseRectMargin + mouseRectPadding);
                        lowerRight = NodeEditorWindow.current.GridToWindowRect(lowerRight);
                        NodeEditorWindow.current.onLateGUI += () => AddMouseRect(lowerRight, MouseCursor.ResizeUpLeft);
                    }

                    break;
            }

            GUILayout.Space(_currentHeight);
            GUI.DrawTexture(new Rect(group.width - 34, group.height + 16, 24, 24), corner);
        }

        public override void OnRenameActive()
        {
            _currentHeight += 30 - NodeEditorResources.styles.nodeHeaderLabelRename.fixedHeight -
                              NodeEditorResources.styles.nodeHeaderLabelRename.margin.top +
                              NodeEditorResources.styles.nodeHeaderLabelRename.margin.bottom / 2;
        }


        public override void OnRenameDeactive()
        {
            _currentHeight = group.height;
        }

        public override int GetWidth()
        {
            return group.width;
        }

        public override GUIStyle GetHeaderLabelStyle()
        {
            return headerStyle;
        }

        public static void AddMouseRect(Rect rect, MouseCursor mouseCursor)
        {
            EditorGUIUtility.AddCursorRect(rect, mouseCursor);
        }

        public override void AddContextMenuItems(GenericMenu menu)
        {
            bool canRemove = true;

            menu.AddItem(new GUIContent("Rename Group"), false, RenameNodeGroup);

            // Add actions to any number of selected nodes
            menu.AddItem(new GUIContent("Copy"), false, NodeEditorWindow.current.CopySelectedNodes);
            menu.AddItem(new GUIContent("Duplicate"), false, NodeEditorWindow.current.DuplicateSelectedNodes);

            if (canRemove)
            {
                menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);
            }
            else
            {
                menu.AddItem(new GUIContent("Remove"), false, null);
            }
        }

        public void RenameNodeGroup()
        {
            var nodeGroups = Selection.objects.ToList().Where(x => x is NodeGroup).ToList();
            if (nodeGroups.Count == 1)
            {
                NodeGroup group = nodeGroups[0] as NodeGroup;
                Vector2 size;
                if (NodeEditorWindow.current.nodeSizes.TryGetValue(group, out size))
                {
                    RenamePopup.Show(group, size.x);
                }
                else
                {
                    RenamePopup.Show(group);
                }
            }
        }
    }
}