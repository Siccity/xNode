using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public static class NodeEditorResources {
        // Textures
        public static Texture2D dot { get { return _dot != null ? _dot : _dot = NodeEditorPreferences.GetSettings().theme.xNodeDot; }}
        public static Texture2D _dot;
        public static Texture2D dotOuter { get { return _dotOuter != null ? _dotOuter : _dotOuter = NodeEditorPreferences.GetSettings().theme.xNodeDotOuter; }}
        public static Texture2D _dotOuter;
        public static Texture2D nodeBody { get { return _nodeBody != null ? _nodeBody : _nodeBody = NodeEditorPreferences.GetSettings().theme.xNodeNode; }}
        public static Texture2D _nodeBody;
        public static Texture2D nodeHighlight { get { return _nodeHighlight != null ? _nodeHighlight : _nodeHighlight = NodeEditorPreferences.GetSettings().theme.xNodeNodeHighlight; }}
        public static Texture2D _nodeHighlight;

        // Styles
        public static Styles styles { get { return _styles = new Styles(); } }
        public static Styles _styles = null;
        public static GUIStyle OutputPort { get { return new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight }; } }
        public class Styles {
            public GUIStyle inputPort, outputPort, nodeHeader, nodeBody, tooltip, nodeHighlight;

            public Styles() {
                GUIStyle baseStyle = new GUIStyle("Label");
                baseStyle.fixedHeight = 18;

                inputPort = new GUIStyle(baseStyle);
                inputPort.alignment = TextAnchor.UpperLeft;
                inputPort.padding.left = 0;
                if(!NodeEditorPreferences.GetSettings().theme.makeTheDotOuterInfrontOfFill)
                    {
                        inputPort.active.background = dot;
                        inputPort.normal.background = dotOuter;
                    }
                else
                    {
                        inputPort.normal.background = dot;
                        inputPort.active.background = dotOuter;
                    }

                outputPort = new GUIStyle(baseStyle);
                outputPort.alignment = TextAnchor.UpperRight;
                outputPort.padding.right = 0;
                if(!NodeEditorPreferences.GetSettings().theme.makeTheDotOuterInfrontOfFill)
                {
                    outputPort.active.background = dot;
                    outputPort.normal.background = dotOuter;
                }
                else
                {
                    outputPort.normal.background = dot;
                    outputPort.active.background = dotOuter;
                }

                nodeHeader = new GUIStyle();
                nodeHeader.alignment = TextAnchor.MiddleCenter;
                nodeHeader.fontStyle = NodeEditorPreferences.GetSettings().theme.headerFontStyle;
                nodeHeader.normal.textColor = NodeEditorPreferences.GetSettings().theme.headerColor;
                nodeHeader.font = NodeEditorPreferences.GetSettings().theme.headerFont;
                nodeHeader.fontSize = NodeEditorPreferences.GetSettings().theme.headerFontSize;

                nodeBody = new GUIStyle();
                nodeBody.normal.background = NodeEditorResources.nodeBody;
                nodeBody.border = new RectOffset(32, 32, 32, 32);
                nodeBody.padding = NodeEditorPreferences.GetSettings().theme.padding;

                nodeHighlight = new GUIStyle();
                nodeHighlight.normal.background = NodeEditorResources.nodeHighlight;
                nodeHighlight.border = new RectOffset(32, 32, 32, 32);

                tooltip = new GUIStyle("helpBox");
                tooltip.alignment = TextAnchor.MiddleCenter;
            }
        }

        public static Texture2D GenerateGridTexture(Color line, Color bg) {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++) {
                for (int x = 0; x < 64; x++) {
                    Color col = bg;
                    if (y % 16 == 0 || x % 16 == 0) col = Color.Lerp(line, bg, 0.65f);
                    if (y == 63 || x == 63) col = Color.Lerp(line, bg, 0.35f);
                    cols[(y * 64) + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateCrossTexture(Color line) {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++) {
                for (int x = 0; x < 64; x++) {
                    Color col = line;
                    if (y != 31 && x != 31) col.a = 0;
                    cols[(y * 64) + x] = col;
                }
            }
            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }
    }
}