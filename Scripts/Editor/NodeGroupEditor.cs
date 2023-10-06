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
        private bool _isDragging;
        private Vector2 _size;
        private float _currentHeight;

        public override void OnCreate()
        {
            _currentHeight = group.height;
        }

        public override void OnHeaderGUI()
        {
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        }

        public override void OnBodyGUI()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (_isDragging)
                    {
                        group.width = Mathf.Max(200, (int)e.mousePosition.x + 16);
                        group.height = Mathf.Max(100, (int)e.mousePosition.y - 34);
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

                    if (NodeEditorWindow.current.nodeSizes.TryGetValue(target, out _size))
                    {
                        // Mouse position checking is in node local space
                        Rect lowerRight = new Rect(_size.x - 34, _size.y - 34, 30, 30);
                        if (lowerRight.Contains(e.mousePosition))
                        {
                            _isDragging = true;
                        }
                    }

                    break;
                case EventType.MouseUp:
                    _isDragging = false;
                    // Select nodes inside the group
                    if (Selection.Contains(target))
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
                        Rect lowerRight = new Rect(target.position, new Vector2(30, 30));
                        lowerRight.y += _size.y - 34;
                        lowerRight.x += _size.x - 34;
                        lowerRight = NodeEditorWindow.current.GridToWindowRect(lowerRight);
                        NodeEditorWindow.current.onLateGUI += () => AddMouseRect(lowerRight);
                    }

                    break;
            }

            GUILayout.Space(_currentHeight);
        }

        public override void OnRenameActive()
        {
            _currentHeight += 30 - NodeEditorResources.styles.nodeHeaderRename.fixedHeight -
                             NodeEditorResources.styles.nodeHeaderRename.margin.top +
                             NodeEditorResources.styles.nodeHeaderRename.margin.bottom / 2;
        }


        public override void OnRename()
        {
            _currentHeight = group.height;
        }

        public override int GetWidth()
        {
            return group.width;
        }

        public override Color GetTint()
        {
            return group.color;
        }

        public static void AddMouseRect(Rect rect)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeUpLeft);
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