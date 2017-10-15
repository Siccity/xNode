using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary> Provides unec-specific field editors </summary>
public class NodeEditorGUILayout {

    private static double tempValue;

    public static object PortField(string label, object value, System.Type type, NodePort port, bool fallback, out Vector2 portPosition) {
        if (fallback) value = PropertyField(label, value, type);
        else EditorGUILayout.LabelField(label);

        Rect rect = GUILayoutUtility.GetLastRect();
        if (port.direction == NodePort.IO.Input) rect.position = rect.position - new Vector2(16, 0);
        if (port.direction == NodePort.IO.Output) rect.position = rect.position + new Vector2(rect.width, 0);
        rect.size = new Vector2(16, 16);

        Color col = GUI.color;
        GUI.color = new Color32(90, 97, 105, 255);
        GUI.DrawTexture(rect, NodeEditorResources.dotOuter);
        GUI.color = NodeEditorPreferences.GetTypeColor(port.type);
        GUI.DrawTexture(rect, NodeEditorResources.dot);
        GUI.color = col;
        portPosition = rect.center;

        return value;
    }

    public static object PropertyField(Node target, FieldInfo fieldInfo, Dictionary<NodePort, Vector2> portPositions) {
        Type fieldType = fieldInfo.FieldType;
        string fieldName = fieldInfo.Name;
        string fieldPrettyName = fieldName.PrettifyCamelCase();
        object fieldValue = fieldInfo.GetValue(target);
        object[] fieldAttribs = fieldInfo.GetCustomAttributes(false);

        HeaderAttribute headerAttrib;
        if (NodeEditorUtilities.GetAttrib(fieldAttribs, out headerAttrib)) {
            EditorGUILayout.LabelField(headerAttrib.header);
        }

        Node.OutputAttribute outputAttrib;
        Node.InputAttribute inputAttrib;

        EditorGUI.BeginChangeCheck();
        if (NodeEditorUtilities.GetAttrib(fieldAttribs, out inputAttrib)) {
            NodePort port = target.GetPortByFieldName(fieldName);
            Vector2 portPos;
            fieldValue = PortField(fieldPrettyName, fieldValue, fieldType, port, inputAttrib.fallback, out portPos);
            portPositions.Add(port, portPos);
        } else if (NodeEditorUtilities.GetAttrib(fieldAttribs, out outputAttrib)) {
            NodePort port = target.GetPortByFieldName(fieldName);
            Vector2 portPos;
            fieldValue = PortField(fieldPrettyName, fieldValue, fieldType, port, outputAttrib.fallback, out portPos);
            portPositions.Add(port, portPos);
        } else {
            fieldValue = PropertyField(fieldPrettyName, fieldValue, fieldType);
        }

        if (EditorGUI.EndChangeCheck()) {
            fieldInfo.SetValue(target, fieldValue);
            EditorUtility.SetDirty(target);
        }
        return fieldValue;
    }

    public static object PropertyField(string label, object value, System.Type type) {
        if (type == typeof(int)) return IntField(label, (int) value);
        else if (type == typeof(float)) return FloatField(label, (float) value);
        else if (type == typeof(double)) return DoubleField(label, (double) value);
        else if (type == typeof(long)) return LongField(label, (long) value);
        else if (type == typeof(bool)) return Toggle(label, (bool) value);
        else if (type == typeof(Enum)) return EnumField(label, (Enum) value);
        else if (type == typeof(string)) return TextField(label, (string) value);
        else if (type == typeof(Rect)) return RectField(label, (Rect) value);
        else if (type == typeof(Vector2)) return Vector2Field(label, (Vector2) value);
        else if (type == typeof(Vector3)) return Vector3Field(label, (Vector3) value);
        else if (type == typeof(Vector4)) return Vector4Field(label, (Vector4) value);
        else if (type == typeof(Color)) return ColorField(label, (Color) value);
        else if (type == typeof(AnimationCurve)) return CurveField(label, (AnimationCurve) value);
        else if (type.IsSubclassOf(typeof(UnityEngine.Object)) || type == typeof(UnityEngine.Object)) return ObjectField(label, (UnityEngine.Object) value);
        else return value;
    }
    public static Rect GetRect(string label) {
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;
        EditorGUI.LabelField(rect, label);
        rect.x += rect.width;
        return rect;
    }
    public static UnityEngine.Object ObjectField(string label, UnityEngine.Object value) {
        return EditorGUI.ObjectField(GetRect(label), value, value.GetType(), true);
    }
    public static AnimationCurve CurveField(string label, AnimationCurve value) {
        if (value == null) value = new AnimationCurve();
        return EditorGUI.CurveField(GetRect(label), value);
    }
    public static Color ColorField(string label, Color value) {
        return EditorGUI.ColorField(GetRect(label), value);
    }
    public static Vector4 Vector4Field(string label, Vector4 value) {
        return EditorGUILayout.Vector4Field(label, value);
    }
    public static Vector3 Vector3Field(string label, Vector3 value) {
        return EditorGUILayout.Vector3Field(label, value);
    }
    public static Vector2 Vector2Field(string label, Vector2 value) {
        return EditorGUILayout.Vector2Field(label, value);
    }
    public static Rect RectField(string label, Rect value) {
        return EditorGUILayout.RectField(label, value);
    }
    public static string TextField(string label, string value) {
        return EditorGUI.TextField(GetRect(label), value);
    }
    public static bool Toggle(string label, bool value) {
        return EditorGUI.Toggle(GetRect(label), value);
    }
    public static int IntField(string label, int value) {
        GUIUtility.GetControlID(FocusType.Passive);
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;
        if (NodeEditorWindow.current != null) {
            double v = (double) value;
            DragNumber(rect, ref v);
            value = (int) v;
            if (GUI.changed) NodeEditorWindow.current.Repaint();
        }
        EditorGUI.LabelField(rect, label);
        rect.x += rect.width;
        if (!GUI.changed) value = EditorGUI.IntField(rect, value);
        else {
            EditorGUI.IntField(rect, value);
        }
        return value;
    }
    public static float FloatField(string label, float value) {
        GUIUtility.GetControlID(FocusType.Passive);
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;
        if (NodeEditorWindow.current != null) {
            double v = (double) value;
            DragNumber(rect, ref v);
            value = (float) v;
            if (GUI.changed) NodeEditorWindow.current.Repaint();
        }
        EditorGUI.LabelField(rect, label);
        rect.x += rect.width;
        if (!GUI.changed) value = EditorGUI.FloatField(rect, value);
        else {
            EditorGUI.FloatField(rect, value);
        }
        return value;
    }
    public static double DoubleField(string label, double value) {
        GUIUtility.GetControlID(FocusType.Passive);
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;
        if (NodeEditorWindow.current != null) {
            double v = (double) value;
            DragNumber(rect, ref v);
            value = (double) v;
            if (GUI.changed) NodeEditorWindow.current.Repaint();
        }
        EditorGUI.LabelField(rect, label);
        rect.x += rect.width;
        if (!GUI.changed) value = EditorGUI.DoubleField(rect, value);
        else {
            EditorGUI.DoubleField(rect, value);
        }
        return value;
    }
    public static long LongField(string label, long value) {
        GUIUtility.GetControlID(FocusType.Passive);
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;
        if (NodeEditorWindow.current != null) {
            double v = (double) value;
            DragNumber(rect, ref v);
            value = (long) v;
            if (GUI.changed) NodeEditorWindow.current.Repaint();
        }
        EditorGUI.LabelField(rect, label);
        rect.x += rect.width;
        if (!GUI.changed) value = EditorGUI.LongField(rect, value);
        else {
            EditorGUI.LongField(rect, value);
        }
        return value;
    }
    public static void DragNumber(Rect rect, ref double value) {
        double sensitivity = Math.Max(0.09432981473891d, Math.Pow(Math.Abs(value), 0.5d) * 0.03d);

        int id = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;
        switch (e.type) {
            case EventType.MouseDown:
                if (rect.Contains(e.mousePosition) && e.button == 0) {
                    tempValue = value;
                    GUIUtility.hotControl = id;
                    e.Use();
                    GUIUtility.keyboardControl = id;
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                tempValue = 0;
                if (GUIUtility.hotControl == id) {
                    GUIUtility.hotControl = 0;
                    e.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);
                }
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == id) {
                    tempValue += HandleUtility.niceMouseDelta * sensitivity;
                    value = tempValue;
                    GUI.changed = true;
                }
                break;
            case EventType.Repaint:
                if (NodeEditorWindow.current != null && Mathf.Approximately(NodeEditorWindow.current.zoom, 1)) {
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);
                }
                break;
        }
    }
    public static Enum EnumField(string label, Enum value) {
        return EditorGUI.EnumPopup(GetRect(label), value);
    }
}