using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

/// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
public class NodeEditor {

    public Dictionary<NodePort, Rect> portRects = new Dictionary<NodePort, Rect>();
    public Node target;

    public virtual void OnNodeGUI() {
        portRects.Clear();
        DrawDefaultNodePortsGUI();
        DrawDefaultNodeBodyGUI();
    }

    /// <summary> Draws standard editors for all fields marked with <see cref="Node.InputAttribute"/> or <see cref="Node.OutputAttribute"/> </summary>
    protected void DrawDefaultNodePortsGUI() {

        Event e = Event.current;

        GUILayout.BeginHorizontal();

        //Inputs
        GUILayout.BeginVertical();
        for (int i = 0; i < target.InputCount; i++) {
            DrawNodePortGUI(target.inputs[i]);
        }
        GUILayout.EndVertical();

        //Outputs
        GUILayout.BeginVertical();
        for (int i = 0; i < target.OutputCount; i++) {
            DrawNodePortGUI(target.outputs[i]);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    /// <summary> Draws standard field editors for all public fields </summary>
    protected void DrawDefaultNodeBodyGUI() {
        FieldInfo[] fields = GetInspectorFields(target);
        for (int i = 0; i < fields.Length; i++) {
            object[] fieldAttribs = fields[i].GetCustomAttributes(false);
            if (NodeEditorUtilities.HasAttrib<Node.InputAttribute>(fieldAttribs) || NodeEditorUtilities.HasAttrib<Node.OutputAttribute>(fieldAttribs)) continue;
            DrawFieldInfoDrawerGUI(fields[i]);
        }
        EditorGUILayout.Space();
    }

    protected void DrawNodePortGUI(NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputStyle : NodeEditorResources.styles.outputStyle;
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(port.name.PrettifyCamelCase()), style);
        DrawNodePortGUI(rect, port);
    }

    protected void DrawNodePortGUI(Rect rect, NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputStyle : NodeEditorResources.styles.outputStyle;
        GUI.Label(rect, new GUIContent(port.name.PrettifyCamelCase()), style);
        Rect handleRect = new Rect(0, 0, 16, 16);
        switch (port.direction) {
            case NodePort.IO.Input:
                handleRect.position = new Vector2(rect.xMin - 8, rect.position.y + (rect.height * 0.5f) - 8);
                break;
            case NodePort.IO.Output:
                handleRect.position = new Vector2(rect.xMax - 8, rect.position.y + (rect.height * 0.5f) - 8);
                break;
        }
        portRects.Add(port, handleRect);
        Color col = GUI.color;
        GUI.color = NodeEditorUtilities.GetTypeColor(port.type);
        GUI.DrawTexture(handleRect, NodeEditorResources.dot);
        GUI.color = new Color(0.29f,0.31f,0.32f);
        GUI.DrawTexture(handleRect, NodeEditorResources.dotOuter);
        GUI.color = col;
    }

    private static FieldInfo[] GetInspectorFields(Node node) {
        return node.GetType().GetFields().Where(f => f.IsPublic || f.GetCustomAttributes(typeof(SerializeField),false) != null).ToArray();
    }

    private void DrawFieldInfoDrawerGUI(FieldInfo fieldInfo) {
        Type fieldType = fieldInfo.FieldType;
        string fieldName = fieldInfo.Name;
        string fieldPrettyName = fieldName.PrettifyCamelCase();
        object fieldValue = fieldInfo.GetValue(target);
        object[] fieldAttribs = fieldInfo.GetCustomAttributes(false);

        HeaderAttribute headerAttrib;
        if (NodeEditorUtilities.GetAttrib(fieldAttribs, out headerAttrib)) {
            EditorGUILayout.LabelField(headerAttrib.header);
        }

        EditorGUI.BeginChangeCheck();
        if (fieldType == typeof(int)) {
            fieldValue = EditorGUILayout.IntField(fieldPrettyName, (int)fieldValue);
        }
        else if (fieldType == typeof(bool)) {
            fieldValue = EditorGUILayout.Toggle(fieldPrettyName, (bool)fieldValue);
        }
        else if (fieldType.IsEnum) {
            fieldValue = EditorGUILayout.EnumPopup(fieldPrettyName, (Enum)fieldValue);
        }
        else if (fieldType == typeof(string)) {

            if (fieldName == "name") return; //Ignore 'name'
            TextAreaAttribute textAreaAttrib;
            if (NodeEditorUtilities.GetAttrib(fieldAttribs, out textAreaAttrib)) {
                fieldValue = EditorGUILayout.TextArea(fieldValue != null ? (string)fieldValue : "");
            }
            else
                fieldValue = EditorGUILayout.TextField(fieldPrettyName, fieldValue != null ? (string)fieldValue : "");
        }
        else if (fieldType == typeof(Rect)) {
            if (fieldName == "position") return; //Ignore 'position'
            fieldValue = EditorGUILayout.RectField(fieldPrettyName, (Rect)fieldValue);
        }
        else if (fieldType == typeof(float)) {
            fieldValue = EditorGUILayout.FloatField(fieldPrettyName, (float)fieldValue);
        }
        else if (fieldType == typeof(Vector2)) {
            fieldValue = EditorGUILayout.Vector2Field(fieldPrettyName, (Vector2)fieldValue);
        }
        else if (fieldType == typeof(Vector3)) {
            fieldValue = EditorGUILayout.Vector3Field(new GUIContent(fieldPrettyName), (Vector3)fieldValue);
        }
        else if (fieldType == typeof(Vector4)) {
            fieldValue = EditorGUILayout.Vector4Field(fieldPrettyName, (Vector4)fieldValue);
        }
        else if (fieldType == typeof(Color)) {
            fieldValue = EditorGUILayout.ColorField(fieldPrettyName, (Color)fieldValue);
        }
        else if (fieldType == typeof(AnimationCurve)) {
            AnimationCurve curve = fieldValue != null ? (AnimationCurve)fieldValue : new AnimationCurve();
            curve = EditorGUILayout.CurveField(fieldPrettyName, curve);
            if (fieldValue != curve) fieldInfo.SetValue(target, curve);
        }
        else if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)) || fieldType == typeof(UnityEngine.Object)) {
            if (fieldName == "graph") return; //Ignore 'graph'
            fieldValue = EditorGUILayout.ObjectField(fieldPrettyName, (UnityEngine.Object)fieldValue, fieldType, true);
        }

        if (EditorGUI.EndChangeCheck()) {
            fieldInfo.SetValue(target, fieldValue);
        }
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
