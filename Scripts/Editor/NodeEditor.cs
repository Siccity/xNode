using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
public class NodeEditor {

    public Node target;

    /// <summary> Draws the node GUI.</summary>
    /// <param name="portPositions">Port handle positions need to be returned to the NodeEditorWindow </param>
    public virtual void OnNodeGUI(out Dictionary<NodePort, Vector2> portPositions) {
        DrawDefaultHeaderGUI();
        DrawDefaultNodeBodyGUI(out portPositions);
    }

    protected void DrawDefaultHeaderGUI() {
        GUI.color = Color.white;
        GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }

    /// <summary> Draws standard field editors for all public fields </summary>
    protected void DrawDefaultNodeBodyGUI(out Dictionary<NodePort, Vector2> portPositions) {
        portPositions = new Dictionary<NodePort, Vector2>();

        FieldInfo[] fields = GetInspectorFields(target);
        for (int i = 0; i < fields.Length; i++) {
            object[] fieldAttribs = fields[i].GetCustomAttributes(false);
            if (fields[i].Name == "graph" || fields[i].Name == "rect") continue;
            NodeEditorGUILayout.PropertyField(target, fields[i], portPositions);
        }
    }

    /// <summary> Draw node port GUI using automatic layouting. Returns port handle position. </summary>
    protected Vector2 DrawNodePortGUI(NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputPort : NodeEditorResources.styles.outputPort;
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(port.fieldName.PrettifyCamelCase()), style);
        return DrawNodePortGUI(rect, port);
    }

    /// <summary> Draw node port GUI in rect. Returns port handle position. </summary>
    protected Vector2 DrawNodePortGUI(Rect rect, NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputPort : NodeEditorResources.styles.outputPort;
        GUI.Label(rect, new GUIContent(port.fieldName.PrettifyCamelCase()), style);

        Vector2 handlePoint = rect.center;

        switch (port.direction) {
            case NodePort.IO.Input:
                handlePoint.x = rect.xMin;
                break;
            case NodePort.IO.Output:
                handlePoint.x = rect.xMax;
                break;
        }

        Color col = GUI.color;
        Rect handleRect = new Rect(handlePoint.x - 8, handlePoint.y - 8, 16, 16);
        GUI.color = new Color32(90, 97, 105, 255);
        GUI.DrawTexture(handleRect, NodeEditorResources.dotOuter);
        GUI.color = NodeEditorPreferences.GetTypeColor(port.type);
        GUI.DrawTexture(handleRect, NodeEditorResources.dot);
        GUI.color = col;
        return handlePoint;
    }

    private static FieldInfo[] GetInspectorFields(Node node) {
        return node.GetType().GetFields().Where(f => f.IsPublic || f.GetCustomAttributes(typeof(SerializeField), false) != null).ToArray();
    }

    public virtual int GetWidth() {
        return 200;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class CustomNodeEditorAttribute : Attribute {
    public Type inspectedType { get { return _inspectedType; } }
    private Type _inspectedType;
    public string contextMenuName { get { return _contextMenuName; } }
    private string _contextMenuName;
    /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
    /// <param name="inspectedType">Type that this editor can edit</param>
    /// <param name="contextMenuName">Path to the node</param>
    public CustomNodeEditorAttribute(Type inspectedType, string contextMenuName) {
        _inspectedType = inspectedType;
        _contextMenuName = contextMenuName;
    }
}