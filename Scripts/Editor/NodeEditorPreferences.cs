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

        private static Dictionary<string, Color> typeColors;
        private static Dictionary<string, Color> generatedTypeColors;
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

            //Get saved type keys
            string[] typeKeys = new string[typeColors.Count];
            typeColors.Keys.CopyTo(typeKeys, 0);
            //Display saved type colors
            foreach (var key in typeKeys) {
                EditorGUILayout.BeginHorizontal();
                if (!EditorGUILayout.Toggle(new GUIContent(key, key), true)) {
                    typeColors.Remove(key);
                    SavePrefs();
                    EditorGUILayout.EndHorizontal();
                    continue;
                }
                Color col = typeColors[key];
                col = EditorGUILayout.ColorField(col);
                typeColors[key] = col;
                EditorGUILayout.EndHorizontal();
            }
            if (GUI.changed) {
                SavePrefs();
                NodeEditorWindow.RepaintAll();
            }

            //Get generated type keys
            string[] generatedTypeKeys = new string[generatedTypeColors.Count];
            generatedTypeColors.Keys.CopyTo(generatedTypeKeys, 0);
            //Display generated type colors
            foreach (var key in generatedTypeKeys) {
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle(new GUIContent(key, key), false)) {
                    typeColors.Add(key, generatedTypeColors[key]);
                    generatedTypeColors.Remove(key);
                    SavePrefs();
                    EditorGUILayout.EndHorizontal();
                    continue;
                }
                Color col = generatedTypeColors[key];
                EditorGUI.BeginDisabledGroup(true);
                col = EditorGUILayout.ColorField(col);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void LoadPrefs() {
            //Load type colors
            generatedTypeColors = new Dictionary<string, Color>();

            if (!EditorPrefs.HasKey("unec_typecolors")) EditorPrefs.SetString("unec_typecolors", "");
            string[] data = EditorPrefs.GetString("unec_typecolors").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            typeColors = new Dictionary<string, Color>();
            for (int i = 0; i < data.Length; i += 2) {
                Color col;
                if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col)) {
                    typeColors.Add(data[i], col);
                }
            }

            //Load grid colors
            if (!EditorPrefs.HasKey("unec_gridcolor0")) EditorPrefs.SetString("unec_gridcolor0", ColorUtility.ToHtmlStringRGB(new Color(0.45f, 0.45f, 0.45f)));
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("unec_gridcolor0"), out _gridLineColor);
            if (!EditorPrefs.HasKey("unec_gridcolor1")) EditorPrefs.SetString("unec_gridcolor1", ColorUtility.ToHtmlStringRGB(new Color(0.18f, 0.18f, 0.18f)));
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("unec_gridcolor1"), out _gridBgColor);

            //Load snap option
            if (EditorPrefs.HasKey("unec_gridsnap")) _gridSnap = EditorPrefs.GetBool("unec_gridsnap");

            NodeEditorWindow.RepaintAll();
            prefsLoaded = true;
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs() {
            if (EditorPrefs.HasKey("unec_typecolors")) EditorPrefs.DeleteKey("unec_typecolors");
            if (EditorPrefs.HasKey("unec_gridcolor0")) EditorPrefs.DeleteKey("unec_gridcolor0");
            if (EditorPrefs.HasKey("unec_gridcolor1")) EditorPrefs.DeleteKey("unec_gridcolor1");
            LoadPrefs();
        }

        private static void SavePrefs() {
            string s = "";
            foreach (var item in typeColors) {
                s += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
            }
            EditorPrefs.SetString("unec_typecolors", s);
            EditorPrefs.SetString("unec_gridcolor0", ColorUtility.ToHtmlStringRGB(_gridLineColor));
            EditorPrefs.SetString("unec_gridcolor1", ColorUtility.ToHtmlStringRGB(_gridBgColor));
            EditorPrefs.SetBool("unec_gridsnap", _gridSnap);
        }

        private static void VerifyLoaded() {
            if (!prefsLoaded) LoadPrefs();
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(System.Type type) {
            VerifyLoaded();
            if (type == null) return Color.gray;
            string typeName = type.PrettyName();
            if (typeColors.ContainsKey(typeName)) return typeColors[typeName];
            if (generatedTypeColors.ContainsKey(typeName)) return generatedTypeColors[typeName];
            #if UNITY_5_4_OR_NEWER
            UnityEngine.Random.InitState(typeName.GetHashCode());
            #else
            UnityEngine.Random.seed = typeName.GetHashCode();
            #endif
            generatedTypeColors.Add(typeName, new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
            return generatedTypeColors[typeName];
        }
    }
}