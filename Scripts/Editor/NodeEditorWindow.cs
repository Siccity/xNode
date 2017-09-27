using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO; 
using UnityEditor.Callbacks;
using System;

[InitializeOnLoad]
public partial class NodeEditorWindow : EditorWindow { 
    public Dictionary<NodePort, Rect> portConnectionPoints { get { return _portConnectionPoints; } }
    private Dictionary<NodePort, Rect> _portConnectionPoints = new Dictionary<NodePort, Rect>();
    public NodeGraph graph { get { return _graph != null ? _graph : _graph = CreateInstance<NodeGraph>(); } }
    public NodeGraph _graph; 
    public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
    private Vector2 _panOffset; 
    public float zoom { get { return _zoom; } set { _zoom = Mathf.Clamp(value, 1f, 5f); Repaint(); } }
    private float _zoom = 1; 

    partial void OnEnable();
    /// <summary> Create editor window </summary>
    //[MenuItem("Window/UNEC")]
    public static NodeEditorWindow Init() {
        NodeEditorWindow w = CreateInstance<NodeEditorWindow>();
        w.titleContent = new GUIContent("UNEC");
        w.wantsMouseMove = true;
        w.Show();
        return w;
    }

    public void Save() {
        if (AssetDatabase.Contains(_graph)) {
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
        }
        else SaveAs();
    }

    public void SaveAs() {
        string path = EditorUtility.SaveFilePanelInProject("Save NodeGraph", "NewNodeGraph", "asset", "");
        if (string.IsNullOrEmpty(path)) return;
        else {
            NodeGraph existingGraph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
            if (existingGraph != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(_graph, path);
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
        }
    }

    private void DraggableWindow(int windowID) {
        GUI.DragWindow();
    }

    public Vector2 WindowToGridPosition(Vector2 windowPosition) {
        return (windowPosition - (position.size * 0.5f) - (panOffset / zoom)) * zoom;
    }

    public Vector2 GridToWindowPosition(Vector2 gridPosition) {
        return (position.size * 0.5f) + (panOffset / zoom) + (gridPosition/zoom);
    }

    public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition) {
        Vector2 center = position.size * 0.5f;
        float xOffset = (center.x * zoom + (panOffset.x + gridPosition.x));
        float yOffset = (center.y * zoom + (panOffset.y + gridPosition.y));
        return new Vector2(xOffset, yOffset);
    }

    public void SelectNode(Node node) {
        selectedNode = node;
    }

    [OnOpenAsset(0)]
    public static bool OnOpen(int instanceID, int line) {
        NodeGraph nodeGraph = EditorUtility.InstanceIDToObject(instanceID) as NodeGraph;
        if (nodeGraph != null) {
            NodeEditorWindow w = Init();
            w._graph = nodeGraph;
            return true;
        }
        return false;
    }
}