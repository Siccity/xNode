using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UNEC {
    /// <summary> Contains GUI methods </summary>
    public static class NodeEditorGUI {

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
    }
}