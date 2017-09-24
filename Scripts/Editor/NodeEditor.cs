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
        DrawNodePortsGUI();
        DrawDefaultNodeBody();
    }

    /// <summary> Draws standard field editors for all public fields </summary>
    protected void DrawDefaultNodeBody() {
        FieldInfo[] fields = GetInspectorFields(target);
        for (int i = 0; i < fields.Length; i++) {
            Type fieldType = fields[i].FieldType;
            string fieldName = fields[i].Name;
            object fieldValue = fields[i].GetValue(target);
            object[] fieldAttribs = fields[i].GetCustomAttributes(false);

            HeaderAttribute headerAttrib;
            if (NodeEditorUtilities.GetAttrib(fieldAttribs, out headerAttrib)) {
                EditorGUILayout.LabelField(headerAttrib.header);
            }

            //Skip if field has input or output attribute
            if (NodeEditorUtilities.HasAttrib<Node.InputAttribute>(fieldAttribs) || NodeEditorUtilities.HasAttrib<Node.OutputAttribute>(fieldAttribs)) continue;

            EditorGUI.BeginChangeCheck();
            if (fieldType == typeof(int)) {
                fieldValue = EditorGUILayout.IntField(fieldName, (int)fieldValue);
            }
            else if (fieldType == typeof(bool)) {
                fieldValue = EditorGUILayout.Toggle(fieldName, (bool)fieldValue);
            }
            else if (fieldType.IsEnum) {
                fieldValue = EditorGUILayout.EnumPopup(fieldName, (Enum)fieldValue);
            }
            else if (fieldType == typeof(string)) {
                
                if (fieldName == "name") continue; //Ignore 'name'
                TextAreaAttribute textAreaAttrib;
                if (NodeEditorUtilities.GetAttrib(fieldAttribs, out textAreaAttrib)) {
                    fieldValue = EditorGUILayout.TextArea(fieldValue != null ? (string)fieldValue : "");
                }
                else
                    fieldValue = EditorGUILayout.TextField(fieldName, fieldValue != null ? (string)fieldValue : "");
            }
            else if (fieldType == typeof(Rect)) {
                if (fieldName == "position") continue; //Ignore 'position'
                fieldValue = EditorGUILayout.RectField(fieldName, (Rect)fieldValue);
            }
            else if (fieldType == typeof(float)) {
                fieldValue = EditorGUILayout.FloatField(fieldName, (float)fieldValue);
            }
            else if (fieldType == typeof(Vector2)) {
                fieldValue = EditorGUILayout.Vector2Field(fieldName, (Vector2)fieldValue);
            }
            else if (fieldType == typeof(Vector3)) {
                fieldValue = EditorGUILayout.Vector3Field(new GUIContent(fieldName), (Vector3)fieldValue);
            }
            else if (fieldType == typeof(Vector4)) {
                fieldValue = EditorGUILayout.Vector4Field(fieldName, (Vector4)fieldValue);
            }
            else if (fieldType == typeof(Color)) {
                fieldValue = EditorGUILayout.ColorField(fieldName, (Color)fieldValue);
            }
            else if (fieldType == typeof(AnimationCurve)) {
                AnimationCurve curve = fieldValue != null ? (AnimationCurve)fieldValue : new AnimationCurve();
                curve = EditorGUILayout.CurveField(fieldName, curve);
                if (fieldValue != curve) fields[i].SetValue(target, curve);
            }
            else if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)) || fieldType == typeof(UnityEngine.Object)) {
                if (fieldName == "graph") continue; //Ignore 'graph'
                fieldValue = EditorGUILayout.ObjectField(fieldName, (UnityEngine.Object)fieldValue, fieldType, true);
            }

            if (EditorGUI.EndChangeCheck()) {
                fields[i].SetValue(target, fieldValue);
            }
        }
        EditorGUILayout.Space();
    }

    protected void DrawNodePortsGUI() {

        Event e = Event.current;

        GUILayout.BeginHorizontal();

        //Inputs
        GUILayout.BeginVertical();
        for (int i = 0; i < target.InputCount; i++) {
            DrawNodePortGUI(target.GetInput(i));
            //NodePort input = target.GetInput(i);
            //Rect r = GUILayoutUtility.GetRect(new GUIContent(input.name), NodeEditorResources.styles.GetInputStyle(input.type));
            //GUI.Label(r, input.name, NodeEditorResources.styles.GetInputStyle(input.type));
            //if (e.type == EventType.Repaint) portRects.Add(input, r);
            //portConnectionPoints.Add(input, new Vector2(r.xMin, r.yMin + (r.height * 0.5f)) + target.position.position);
        }
        GUILayout.EndVertical();

        //Outputs
        GUILayout.BeginVertical();
        for (int i = 0; i < target.OutputCount; i++) {
            DrawNodePortGUI(target.GetOutput(i));
            //NodePort output = target.GetOutput(i);
            //Rect r = GUILayoutUtility.GetRect(new GUIContent(output.name), NodeEditorResources.styles.GetOutputStyle(output.type));
            //GUI.Label(r, output.name, NodeEditorResources.styles.GetOutputStyle(output.type));
            //if (e.type == EventType.Repaint) portRects.Add(output, r);
            //portConnectionPoints.Add(output, new Vector2(r.xMax, r.yMin + (r.height * 0.5f)) + target.position.position);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    protected void DrawNodePortGUI(NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputStyle : NodeEditorResources.styles.outputStyle;
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(port.name), style);
        DrawNodePortGUI(rect, port);
    }

    protected void DrawNodePortGUI(Rect rect, NodePort port) {
        GUIStyle style = port.direction == NodePort.IO.Input ? NodeEditorResources.styles.inputStyle : NodeEditorResources.styles.outputStyle;
        GUI.Label(rect, new GUIContent(port.name), style);
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
        GUI.color = Color.black;
        GUI.DrawTexture(handleRect, NodeEditorResources.dotOuter);
        GUI.color = col;
    }

    private static FieldInfo[] GetInspectorFields(Node node) {
        return node.GetType().GetFields().Where(f => f.IsPublic || f.GetCustomAttributes(typeof(SerializeField),false) != null).ToArray();
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
