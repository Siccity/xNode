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

        [PreferenceItem("Node Editor")]
        private static void PreferencesGUI() {
            if (!prefsLoaded) LoadPrefs();
            EditorGUILayout.LabelField("Type colors", EditorStyles.boldLabel);

            string[] typeKeys = new string[typeColors.Count];
            typeColors.Keys.CopyTo(typeKeys, 0);

            foreach (var key in typeKeys) {
                EditorGUILayout.BeginHorizontal();
                Color col = typeColors[key];
                col = EditorGUILayout.ColorField(key, col);
                typeColors[key] = col;
                if (!GUILayout.Toggle(true, "")) {
                    typeColors.Remove(key);
                    SavePrefs();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUI.changed) {
                SavePrefs();
            }

            string[] generatedTypeKeys = new string[generatedTypeColors.Count];
            generatedTypeColors.Keys.CopyTo(generatedTypeKeys, 0);
            foreach (var key in generatedTypeKeys) {
                EditorGUILayout.BeginHorizontal();
                Color col = generatedTypeColors[key];
                EditorGUI.BeginDisabledGroup(true);
                col = EditorGUILayout.ColorField(key, col);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Toggle(false, "")) {
                    typeColors.Add(key, generatedTypeColors[key]);
                    generatedTypeColors.Remove(key);
                    SavePrefs();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void LoadPrefs() {
            generatedTypeColors = new Dictionary<string, Color>();
            typeColors = GetTypeColors();
            prefsLoaded = true;
        }

        private static void SavePrefs() {
            if (!prefsLoaded) return;
            string s = "";
            foreach (var item in typeColors) {
                s += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
            }
            EditorPrefs.SetString("unec_typecolors", s);
        }

        public static void SetDefaultTypeColors() {
            EditorPrefs.SetString("unec_typecolors", "int,2568CA,string,CE743A,bool,00FF00");
        }

        public static Dictionary<string, Color> GetTypeColors() {
            if (prefsLoaded) return typeColors;
            if (!EditorPrefs.HasKey("unec_typecolors")) SetDefaultTypeColors();
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