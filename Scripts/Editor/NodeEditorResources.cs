using UnityEditor;
using UnityEngine;

namespace XNodeEditor
{
    public static class NodeEditorResources
    {
        // Textures
        public static Texture2D dot => _dot != null ? _dot : _dot = Resources.Load<Texture2D>("xnode_dot");
        private static Texture2D _dot;
        public static Texture2D dotOuter =>
            _dotOuter != null ? _dotOuter : _dotOuter = Resources.Load<Texture2D>("xnode_dot_outer");
        private static Texture2D _dotOuter;

        public static Texture2D nodeHeader =>
            _nodeHeader != null ? _nodeHeader : _nodeHeader = Resources.Load<Texture2D>("xnode_node_header");
        private static Texture2D _nodeHeader;
        public static Texture2D nodePorts =>
            _nodePorts != null ? _nodePorts : _nodePorts = Resources.Load<Texture2D>("xnode_node_ports");
        private static Texture2D _nodePorts;
        public static Texture2D nodeBody =>
            _nodeBody != null ? _nodeBody : _nodeBody = Resources.Load<Texture2D>("xnode_node_body");
        private static Texture2D _nodeBody;

        public static Texture2D nodeHighlight => _nodeHighlight != null
            ? _nodeHighlight
            : _nodeHighlight = Resources.Load<Texture2D>("xnode_node_highlight");
        private static Texture2D _nodeHighlight;

        // Styles
        public static Styles styles => _styles != null ? _styles : _styles = new Styles();
        public static Styles _styles;
        public static GUIStyle OutputPort => new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight };
        public class Styles
        {
            public GUIStyle inputPort,
                outputPort,
                nodeHeaderLabel,
                nodeHeaderLabelRename,
                nodeHeader,
                nodePorts,
                nodeBody,
                nodePadding,
                tooltip,
                nodeHighlight;

            public Styles()
            {
                GUIStyle baseStyle = new GUIStyle("Label");
                baseStyle.fixedHeight = 18;

                inputPort = new GUIStyle(baseStyle);
                inputPort.alignment = TextAnchor.UpperLeft;
                inputPort.padding.left = 0;
                inputPort.active.background = dot;
                inputPort.normal.background = dotOuter;

                outputPort = new GUIStyle(baseStyle);
                outputPort.alignment = TextAnchor.UpperRight;
                outputPort.padding.right = 0;
                outputPort.active.background = dot;
                outputPort.normal.background = dotOuter;

                nodeHeaderLabel = new GUIStyle();
                nodeHeaderLabel.alignment = TextAnchor.MiddleCenter;
                nodeHeaderLabel.fontStyle = FontStyle.Bold;
                nodeHeaderLabel.normal.textColor = Color.white;

                nodeHeaderLabelRename = new GUIStyle(GUI.skin.textField);
                nodeHeaderLabelRename.alignment = TextAnchor.MiddleCenter;
                nodeHeaderLabelRename.fontStyle = FontStyle.Bold;
                nodeHeaderLabelRename.normal.textColor = Color.white;
                nodeHeaderLabelRename.fixedHeight = 18;
                nodeHeaderLabelRename.margin = new RectOffset(5, 5, 10, 8);

                nodePadding = new GUIStyle();
                nodePadding.padding = new RectOffset(16, 16, 3, 16);

                nodeHeader = new GUIStyle();
                nodeHeader.normal.background = NodeEditorResources.nodeHeader;
                nodeHeader.border = new RectOffset(32, 32, 16, 0);
                // nodeHeader.fixedHeight = 27;
                nodeHeader.padding = new RectOffset(0, 0, 1, 0);
                // nodeHeader.padding = new RectOffset(16, 16, 3, 16);

                nodePorts = new GUIStyle();
                nodePorts.normal.background = NodeEditorResources.nodePorts;
                nodePorts.border = new RectOffset(32, 32, 32, 32);
                // nodePorts.padding = new RectOffset(16, 16, 3, 16);

                nodeBody = new GUIStyle();
                nodeBody.normal.background = NodeEditorResources.nodeBody;
                nodeBody.border = new RectOffset(32, 32, 32, 32);
                // nodeBody.padding = new RectOffset(16, 16, 3, 16);

                nodeHighlight = new GUIStyle();
                nodeHighlight.normal.background = NodeEditorResources.nodeHighlight;
                nodeHighlight.border = new RectOffset(32, 32, 32, 32);

                tooltip = new GUIStyle("helpBox");
                tooltip.alignment = TextAnchor.MiddleCenter;
            }
        }

        public static Texture2D GenerateSolidColorTexture(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static Texture2D GenerateGridTexture(Color line, Color bg)
        {
            Texture2D tex = new Texture2D(64, 64);
            var cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = bg;
                    if (y % 16 == 0 || x % 16 == 0)
                    {
                        col = Color.Lerp(line, bg, 0.65f);
                    }

                    if (y == 63 || x == 63)
                    {
                        col = Color.Lerp(line, bg, 0.35f);
                    }

                    cols[y * 64 + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateCrossTexture(Color line)
        {
            Texture2D tex = new Texture2D(64, 64);
            var cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Color col = line;
                    if (y != 31 && x != 31)
                    {
                        col.a = 0;
                    }

                    cols[y * 64 + x] = col;
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