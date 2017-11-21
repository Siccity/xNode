using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public static class NodeEditorPreferences {

        /// <summary> Have we loaded the prefs yet </summary>
        private static bool prefsLoaded = false;

        private static Dictionary<string, Color> typeColors;
        private static Dictionary<string, Color> generatedTypeColors;
        private static bool gridSnap;
        public static Color gridLineColor { get { VerifyLoaded(); return _gridLineColor; } }
        private static Color _gridLineColor;
        public static Color gridBgColor { get { VerifyLoaded(); return _gridBgColor; } }
        private static Color _gridBgColor;
        [PreferenceItem("Node Editor")]
        private static void PreferencesGUI() {
            if (!prefsLoaded) LoadPrefs();
            GridSettingsGUI();
            TypeColorsGUI();
            if (GUILayout.Button("Set Default", GUILayout.Width(120))) {
                ResetPrefs();
            }
        }

        private static void GridSettingsGUI() {
            //Label
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            gridSnap = EditorGUILayout.Toggle("Snap", gridSnap);

            //EditorGUIUtility.labelWidth = 30;
            _gridLineColor = EditorGUILayout.ColorField("Color", _gridLineColor);
            _gridBgColor = EditorGUILayout.ColorField(" ", _gridBgColor);
            if (GUI.changed) {
                SavePrefs();
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
                if (!EditorGUILayout.Toggle(key, true)) {
                    typeColors.Remove(key);
                    SavePrefs();
                }
                Color col = typeColors[key];
                col = EditorGUILayout.ColorField(col);
                typeColors[key] = col;
                EditorGUILayout.EndHorizontal();
            }
            if (GUI.changed) {
                SavePrefs();
            }

            //Get generated type keys
            string[] generatedTypeKeys = new string[generatedTypeColors.Count];
            generatedTypeColors.Keys.CopyTo(generatedTypeKeys, 0);
            //Display generated type colors
            foreach (var key in generatedTypeKeys) {
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle(key, false)) {
                    typeColors.Add(key, generatedTypeColors[key]);
                    generatedTypeColors.Remove(key);
                    SavePrefs();
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
            typeColors = GetTypeColors();

            //Load grid colors
            if (EditorPrefs.HasKey("unec_gridcolor0")) {
                Color color;
                if (ColorUtility.TryParseHtmlString(EditorPrefs.GetString("unec_gridcolor0"), out color)) {
                    _gridLineColor = color;
                }
            }
            if (EditorPrefs.HasKey("unec_gridcolor1")) {
                Color color;
                if (ColorUtility.TryParseHtmlString(EditorPrefs.GetString("unec_gridcolor1"), out color)) {
                    _gridBgColor = color;
                }
            }

            //Load snap option
            if (EditorPrefs.HasKey("unec_gridsnap")) gridSnap = EditorPrefs.GetBool("unec_gridsnap");

            prefsLoaded = true;
        }

        private static void ResetPrefs() {
            EditorPrefs.SetString("unec_typecolors", "int,2568CA,string,CE743A,bool,00FF00");
            EditorPrefs.SetString("unec_gridcolor0", ColorUtility.ToHtmlStringRGB(new Color(0.45f, 0.45f, 0.45f)));
            EditorPrefs.SetString("unec_gridcolor1", ColorUtility.ToHtmlStringRGB(new Color(0.18f, 0.18f, 0.18f)));
            LoadPrefs();
        }

        private static void SavePrefs() {
            if (!prefsLoaded) return;
            string s = "";
            foreach (var item in typeColors) {
                s += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
            }
            EditorPrefs.SetString("unec_typecolors", s);
            EditorPrefs.SetString("unec_gridcolor0", ColorUtility.ToHtmlStringRGB(_gridLineColor));
            EditorPrefs.SetString("unec_gridcolor1", ColorUtility.ToHtmlStringRGB(_gridBgColor));
            EditorPrefs.SetBool("unec_gridsnap", gridSnap);
        }

        private static void VerifyLoaded() {
            if (!prefsLoaded) LoadPrefs();
        }

        public static Dictionary<string, Color> GetTypeColors() {
            if (prefsLoaded) return typeColors;
            string[] data = EditorPrefs.GetString("unec_typecolors").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, Color> dict = new Dictionary<string, Color>();
            for (int i = 0; i < data.Length; i += 2) {
                Color col;
                if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col)) {
                    dict.Add(data[i], col);
                }
            }
            return dict;
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(System.Type type) {
            if (!prefsLoaded) LoadPrefs();
            if (type == null) return Color.gray;
            if (typeColors.ContainsKey(type.Name)) return typeColors[type.Name];
            if (generatedTypeColors.ContainsKey(type.Name)) return generatedTypeColors[type.Name];
            UnityEngine.Random.InitState(type.Name.GetHashCode());
            generatedTypeColors.Add(type.Name, new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
            return generatedTypeColors[type.Name];
        }
    }
}