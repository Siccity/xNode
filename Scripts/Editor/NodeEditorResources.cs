using UnityEngine;
using UnityEditor;
using System;

public partial class NodeEditorWindow {

    public static Texture2D gridTexture { get { return _gridTexture != null ? _gridTexture : _gridTexture = GenerateGridTexture(); } }
    private static Texture2D _gridTexture;
    public static Texture2D crossTexture { get { return _crossTexture != null ? _crossTexture : _crossTexture = GenerateCrossTexture(); } }
    private static Texture2D _crossTexture;


    private static Color backgroundColor = new Color(0.18f, 0.18f, 0.18f);
    private static Color veinColor = new Color(0.25f, 0.25f, 0.25f);
    private static Color arteryColor = new Color(0.34f, 0.34f, 0.34f);
    private static Color crossColor = new Color(0.45f, 0.45f, 0.45f);

    public static Styles styles { get { return _styles != null ? _styles : _styles = new Styles(); } }
    public static Styles _styles = null;

    public class Styles {
        GUIStyle inputInt, inputString, inputFloat, inputObject, inputTexture, inputColor;
        GUIStyle outputInt, outputString, outputFloat, outputObject, outputTexture, outputColor;

        public Styles() {
            
            inputObject = new GUIStyle((GUIStyle)"flow shader in 0");
            inputString = new GUIStyle((GUIStyle)"flow shader in 1");
            inputInt = new GUIStyle((GUIStyle)"flow shader in 2");
            inputFloat = new GUIStyle((GUIStyle)"flow shader in 3");
            inputColor = new GUIStyle((GUIStyle)"flow shader in 4");
            inputTexture = new GUIStyle((GUIStyle)"flow shader in 5");
            outputObject = new GUIStyle((GUIStyle)"flow shader out 0");
            outputString = new GUIStyle((GUIStyle)"flow shader out 1");
            outputInt = new GUIStyle((GUIStyle)"flow shader out 2");
            outputFloat = new GUIStyle((GUIStyle)"flow shader out 3");
            outputColor = new GUIStyle((GUIStyle)"flow shader out 4");
            outputTexture = new GUIStyle((GUIStyle)"flow shader out 5");

            foreach (GUIStyle style in new GUIStyle[] { inputInt, inputString, inputFloat, inputObject, inputTexture, inputColor, outputInt, outputString, outputFloat, outputObject, outputTexture, outputColor }) {
                style.normal.textColor = Color.black;
                style.fixedHeight = 18;
                style.alignment = TextAnchor.MiddleLeft;
                style.onHover.textColor = Color.red;
            }
        }

        public GUIStyle GetInputStyle(Type type) {

            if (type == typeof(int)) return inputInt;
            else if (type == typeof(string)) return inputString;
            else if (type == typeof(Texture2D)) return inputTexture;
            else if (type == typeof(float)) return inputFloat;
            else if (type == typeof(Color)) return inputColor;
            else return inputObject;
        }

        public GUIStyle GetOutputStyle(Type type) {
            if (type == typeof(int)) return outputInt;
            else if (type == typeof(string)) return outputString;
            else if (type == typeof(Texture2D)) return outputTexture;
            else if (type == typeof(float)) return outputFloat;
            else if (type == typeof(Color)) return outputColor;
            else return outputObject;
        }
    }

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
