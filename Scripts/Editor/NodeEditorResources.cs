using UnityEngine;
using UnityEditor;

namespace UNEC {
    public static class NodeEditorResources {

        public static Texture2D gridTexture { get { return _gridTexture != null ? _gridTexture : _gridTexture = GenerateGridTexture(); } }
        private static Texture2D _gridTexture;
        public static Texture2D crossTexture { get { return _crossTexture != null ? _crossTexture : _crossTexture = GenerateCrossTexture(); } }
        private static Texture2D _crossTexture;


        private static Color backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        private static Color veinColor = new Color(0.2f, 0.2f, 0.2f);
        private static Color arteryColor = new Color(0.28f, 0.28f, 0.28f);
        private static Color crossColor = new Color(0.4f, 0.4f, 0.4f);

        public static Texture2D GenerateGridTexture() {
            Texture2D tex = new Texture2D(64,64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++) {
                for (int x = 0; x < 64; x++) {
                    Color col = backgroundColor;
                    if (y % 16 == 0 || x % 16 == 0) col = veinColor;
                    if (y == 63 || x == 63) col = arteryColor;
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

        public static Texture2D GenerateCrossTexture() {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++) {
                for (int x = 0; x < 64; x++) {
                    Color col = crossColor;
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