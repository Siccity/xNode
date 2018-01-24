using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public static class NodeEditorPreferences {

        public static Texture2D gridTexture {
            get {
                VerifyLoaded();
                if (_gridTexture == null) _gridTexture = NodeEditorResources.GenerateGridTexture(_gridLineColor, _gridBgColor);
                return _gridTexture;
            }
        }
        private static Texture2D _gridTexture;
        public static Texture2D crossTexture {
            get {
                VerifyLoaded();
                if (_crossTexture == null) _crossTexture = NodeEditorResources.GenerateCrossTexture(_gridLineColor);
                return _crossTexture;
            }
        }
        private static Texture2D _crossTexture;

        /// <summary> Have we loaded the prefs yet </summary>
        private static bool prefsLoaded = false;

        /// <summary> TypeColors requested by the editor </summary>
        private static Dictionary<string, Color> typeColors = new Dictionary<string, Color>();
        /// <summary> TypeColors available in EditorPrefs </summary>
        private static Dictionary<string, Color> prefsTypeColors = new Dictionary<string, Color>();
        public static bool gridSnap { get { VerifyLoaded(); return _gridSnap; } }
        private static bool _gridSnap = true;
        public static Color gridLineColor { get { VerifyLoaded(); return _gridLineColor; } }
        private static Color _gridLineColor;
        public static Color gridBgColor { get { VerifyLoaded(); return _gridBgColor; } }
        private static Color _gridBgColor;
        [PreferenceItem("Node Editor")]
        private static void PreferencesGUI() {
            VerifyLoaded();

            GridSettingsGUI();
            TypeColorsGUI();
            if (GUILayout.Button(new GUIContent("Set Default", "Reset all values to default"), GUILayout.Width(120))) {
                ResetPrefs();
            }
        }

        private static void GridSettingsGUI() {
            //Label
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            _gridSnap = EditorGUILayout.Toggle("Snap", _gridSnap);

            //EditorGUIUtility.labelWidth = 30;
            _gridLineColor = EditorGUILayout.ColorField("Color", _gridLineColor);
            _gridBgColor = EditorGUILayout.ColorField(" ", _gridBgColor);
            if (GUI.changed) {
                SavePrefs();
                _gridTexture = NodeEditorResources.GenerateGridTexture(_gridLineColor, _gridBgColor);
                _crossTexture = NodeEditorResources.GenerateCrossTexture(_gridLineColor);
                NodeEditorWindow.RepaintAll();
            }
            EditorGUILayout.Space();
        }

        private static void TypeColorsGUI() {
            //Label
            EditorGUILayout.LabelField("Type colors", EditorStyles.boldLabel);

            //Display type colors. Save them if they are edited by the user
            List<string> keys = new List<string>(typeColors.Keys);
            foreach (string key in keys) {
                Color col = typeColors[key];
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                col = EditorGUILayout.ColorField(key, col);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck()) {
                    typeColors[key] = col;
                    SaveTypeColor(key, col);
                    NodeEditorWindow.RepaintAll();
                }
            }
        }

        private static void LoadPrefs() {
            prefsTypeColors = LoadTypeColors();

            //Load grid colors
            if (!EditorPrefs.HasKey("xnode_gridcolor0")) EditorPrefs.SetString("xnode_gridcolor0", ColorUtility.ToHtmlStringRGB(new Color(0.45f, 0.45f, 0.45f)));
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("xnode_gridcolor0"), out _gridLineColor);
            if (!EditorPrefs.HasKey("xnode_gridcolor1")) EditorPrefs.SetString("xnode_gridcolor1", ColorUtility.ToHtmlStringRGB(new Color(0.18f, 0.18f, 0.18f)));
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("xnode_gridcolor1"), out _gridBgColor);

            //Load snap option
            if (EditorPrefs.HasKey("xnode_gridsnap")) _gridSnap = EditorPrefs.GetBool("xnode_gridsnap");

            NodeEditorWindow.RepaintAll();
            prefsLoaded = true;
        }

        /// <summary> Get Type Colors from EditorPrefs. Colors are saved as CSV in pairs of two hexcolor/name </summary>
        public static Dictionary<string, Color> LoadTypeColors() {
            //Load type colors
            Dictionary<string, Color> result = new Dictionary<string, Color>();

            if (!EditorPrefs.HasKey("xnode_typecolors")) EditorPrefs.SetString("xnode_typecolors", "");
            string[] data = EditorPrefs.GetString("xnode_typecolors").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < data.Length; i += 2) {
                Color col;
                if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col)) {
                    result.Add(data[i], col);
                }
            }
            return result;
        }

        /// <summary> Get Type Colors from EditorPrefs. Colors are saved as CSV in pairs of two hexcolor/name ""
        public static Dictionary<string, Color> SaveTypeColor(string typeName, Color col) {
            //Load type colors
            Dictionary<string, Color> result = LoadTypeColors();
            if (result.ContainsKey(typeName)) result[typeName] = col;
            else result.Add(typeName, col);
            string s = "";
            foreach (var item in result) {
                s += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
            }
            EditorPrefs.SetString("xnode_typecolors", s);
            return result;
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs() {
            if (EditorPrefs.HasKey("xnode_typecolors")) EditorPrefs.DeleteKey("xnode_typecolors");
            if (EditorPrefs.HasKey("xnode_gridcolor0")) EditorPrefs.DeleteKey("xnode_gridcolor0");
            if (EditorPrefs.HasKey("xnode_gridcolor1")) EditorPrefs.DeleteKey("xnode_gridcolor1");
            LoadPrefs();
        }

        private static void SavePrefs() {
            EditorPrefs.SetString("xnode_gridcolor0", ColorUtility.ToHtmlStringRGB(_gridLineColor));
            EditorPrefs.SetString("xnode_gridcolor1", ColorUtility.ToHtmlStringRGB(_gridBgColor));
            EditorPrefs.SetBool("xnode_gridsnap", _gridSnap);
        }

        private static void VerifyLoaded() {
            if (!prefsLoaded) LoadPrefs();
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(System.Type type) {
            VerifyLoaded();
            if (type == null) return Color.gray;
            string typeName = type.PrettyName();
            if (!typeColors.ContainsKey(typeName)) {
                if (prefsTypeColors.ContainsKey(typeName)) typeColors.Add(typeName, prefsTypeColors[typeName]);
                else {
#if UNITY_5_4_OR_NEWER
                    UnityEngine.Random.InitState(typeName.GetHashCode());
#else
                    UnityEngine.Random.seed = typeName.GetHashCode();
#endif
                    typeColors.Add(typeName, new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
                }
            }
            return typeColors[typeName];
        }
    }
}