using System;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> UNEC-specific version of <see cref="EditorGUILayout"/> </summary>
    public static class NodeEditorGUILayout {

        public static void PropertyField(SerializedProperty property, bool includeChildren = true) {
            if (property == null) throw new NullReferenceException();
            Node node = property.serializedObject.targetObject as Node;
            NodePort port = node.GetPort(property.name);

            // If property is not a port, display a regular property field
            if (port == null) EditorGUILayout.PropertyField(property, includeChildren, GUILayout.MinWidth(30));
            else {
                Rect rect = new Rect();

                // If property is an input, display a regular property field and put a port handle on the left side
                if (port.direction == NodePort.IO.Input) {
                    // Display a label if port is connected
                    if (port.IsConnected) EditorGUILayout.LabelField(property.displayName);
                    // Display an editable property field if port is not connected
                    else EditorGUILayout.PropertyField(property, includeChildren, GUILayout.MinWidth(30));
                    rect = GUILayoutUtility.GetLastRect();
                    rect.position = rect.position - new Vector2(16, 0);
                    // If property is an output, display a text label and put a port handle on the right side
                } else if (port.direction == NodePort.IO.Output) {
                    EditorGUILayout.LabelField(property.displayName, NodeEditorResources.styles.outputPort, GUILayout.MinWidth(30));
                    rect = GUILayoutUtility.GetLastRect();
                    rect.position = rect.position + new Vector2(rect.width, 0);
                }

                rect.size = new Vector2(16, 16);

                Color backgroundColor = new Color32(90, 97, 105, 255);
                if (NodeEditorWindow.nodeTint.ContainsKey(port.node.GetType())) backgroundColor *= NodeEditorWindow.nodeTint[port.node.GetType()];
                DrawPortHandle(rect, port.ValueType, backgroundColor);

                // Register the handle position
                Vector2 portPos = rect.center;
                if (NodeEditor.portPositions.ContainsKey(port)) NodeEditor.portPositions[port] = portPos;
                else NodeEditor.portPositions.Add(port, portPos);
            }
        }

        public static void PortField(NodePort port) {
            if (port == null) return;
            EditorGUILayout.LabelField(port.fieldName.PrettifyCamelCase(), GUILayout.MinWidth(30));

            Rect rect = GUILayoutUtility.GetLastRect();
            if (port.direction == NodePort.IO.Input) rect.position = rect.position - new Vector2(16, 0);
            else if (port.direction == NodePort.IO.Output) rect.position = rect.position + new Vector2(rect.width, 0);
            rect.size = new Vector2(16, 16);

            Color backgroundColor = new Color32(90, 97, 105, 255);
            if (NodeEditorWindow.nodeTint.ContainsKey(port.node.GetType())) backgroundColor *= NodeEditorWindow.nodeTint[port.node.GetType()];
            DrawPortHandle(rect, port.ValueType, backgroundColor);

            // Register the handle position
            Vector2 portPos = rect.center;
            if (NodeEditor.portPositions.ContainsKey(port)) NodeEditor.portPositions[port] = portPos;
            else NodeEditor.portPositions.Add(port, portPos);
        }

        private static void DrawPortHandle(Rect rect, Type type, Color backgroundColor) {
            Color col = GUI.color;
            GUI.color = backgroundColor;
            GUI.DrawTexture(rect, NodeEditorResources.dotOuter);
            GUI.color = NodeEditorPreferences.GetTypeColor(type);
            GUI.DrawTexture(rect, NodeEditorResources.dot);
            GUI.color = col;
        }
    }
}