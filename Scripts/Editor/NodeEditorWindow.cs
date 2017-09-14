using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UNEC;

public class NodeEditorWindow : EditorWindow {

    public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
    private Vector2 _panOffset;
    public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, 1f, 5f); Repaint(); } }
    private float _zoom = 1;

    [MenuItem("Window/UNEC")]
    static void Init() {
        NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
        w.Show();
    }

    private void OnGUI() {

        NodeEditorAction.Controls(this);

        BeginWindows();
        NodeEditorGUI.DrawGrid(position, zoom, panOffset);
        EndWindows();
    }
}
