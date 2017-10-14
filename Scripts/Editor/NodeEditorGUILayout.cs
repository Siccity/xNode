using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary> Provides unec-specific field editors </summary>
public class NodeEditorGUILayout {

    private static double tempValue;

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
        Rect rect = EditorGUILayout.GetControlRect();
        rect.width *= 0.5f;
        EditorGUI.LabelField(rect, label);
        rect.x += rect.width;
        return EditorGUI.EnumPopup(rect, value);
    }
}