using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
public class NodeEditor {
    /// <summary> Fires every whenever a node was modified through the editor </summary>
    public static Action<Node> onUpdateNode;
    public Node target;

    /// <summary> Draws the node GUI.</summary>
    /// <param name="portPositions">Port handle positions need to be returned to the NodeEditorWindow </param>
    public void OnNodeGUI(out Dictionary<NodePort, Vector2> portPositions) {
        OnHeaderGUI();
        OnBodyGUI(out portPositions);
    }

    protected virtual void OnHeaderGUI() {
        GUI.color = Color.white;
        string title = NodeEditorUtilities.PrettifyCamelCase(target.name);
        GUILayout.Label(title, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
    }

    /// <summary> Draws standard field editors for all public fields </summary>
    protected virtual void OnBodyGUI(out Dictionary<NodePort, Vector2> portPositions) {
        portPositions = new Dictionary<NodePort, Vector2>();

        EditorGUI.BeginChangeCheck();
        FieldInfo[] fields = GetInspectorFields(target);
        for (int i = 0; i < fields.Length; i++) {
            object[] fieldAttribs = fields[i].GetCustomAttributes(false);
            if (fields[i].Name == "graph" || fields[i].Name == "position") continue;
            NodeEditorGUILayout.PropertyField(target, fields[i], portPositions);
        }
        //If user changed a value, notify other scripts through onUpdateNode
        if (EditorGUI.EndChangeCheck()) {
            if (onUpdateNode != null) onUpdateNode(target);
        }
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