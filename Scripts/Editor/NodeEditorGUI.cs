using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UNEC {
    /// <summary> Contains GUI methods </summary>
    public static class NodeEditorGUI {

        public static NodeEditorWindow window { get { return NodeEditorWindow.window; } }

        public static void DrawGrid(Rect rect, float zoom, Vector2 panOffset) {

            rect.position = Vector2.zero;

            Vector2 center = rect.size / 2f;
            Texture2D gridTex = NodeEditorResources.gridTexture;
            Texture2D crossTex = NodeEditorResources.crossTexture;
            
            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f,0.5f), tileAmount));
        }

        public static void DrawToolbar() {
            EditorGUILayout.BeginHorizontal("Toolbar");

            if (DropdownButton("File", 50)) FileContextMenu();
            if (DropdownButton("Edit", 50)) EditContextMenu();
            if (DropdownButton("View", 50)) { }
            if (DropdownButton("Settings", 70)) { }
            if (DropdownButton("Tools", 50)) { }

            // Make the toolbar extend all throughout the window extension.
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        public static bool DropdownButton(string name, float width) {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
        }

        public static void RightClickContextMenu() {
            GenericMenu contextMenu = new GenericMenu();
            Vector2 pos = window.GetMousePositionOnGrid();
            for (int i = 0; i < NodeEditorReflection.nodeTypes.Length; i++) {
                Type type = NodeEditorReflection.nodeTypes[i];
                contextMenu.AddItem(new GUIContent(NodeEditorReflection.nodeTypes[i].ToString()), false, () => {
                    NodeEditorAction.CreateNode(type, pos);
                });
            }

            contextMenu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
        }

        public static void FileContextMenu() {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Create New"), false, null);
            contextMenu.AddItem(new GUIContent("Load"), false, null);

            contextMenu.AddSeparator("");
            contextMenu.AddItem(new GUIContent("Save"), false, null);
            contextMenu.AddItem(new GUIContent("Save As"), false, null);

            contextMenu.DropDown(new Rect(5f, 17f, 0f, 0f));
        }

        public static void EditContextMenu() {
            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Clear"), false, () => window.currentGraph.Clear());

            contextMenu.DropDown(new Rect(5f, 17f, 0f, 0f));
        }
    }
}