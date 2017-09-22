using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;

/// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
public class NodeEditor {

    public Node target;

    public virtual void OnNodeGUI() {
        DrawDefaultNodeGUI();
    }

    /// <summary> Draws standard field editors for all public fields </summary>
    protected void DrawDefaultNodeGUI() {
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
                fieldValue = EditorGUILayout.ObjectField(fieldName, (UnityEngine.Object)fieldValue, fieldType, true);
            }

            if (EditorGUI.EndChangeCheck()) {
                fields[i].SetValue(target, fieldValue);
            }
        }
        EditorGUILayout.Space();
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
