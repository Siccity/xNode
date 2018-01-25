using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public static class NodeEditorPreferences {

        public static Texture2D gridTexture {
            get {
                VerifyLoaded();
                if (_gridTexture == null) _gridTexture = NodeEditorResources.GenerateGridTexture(settings.gridLineColor, settings.gridBgColor);
                return _gridTexture;
            }
        }
        private static Texture2D _gridTexture;
        public static Texture2D crossTexture {
            get {
                VerifyLoaded();
                if (_crossTexture == null) _crossTexture = NodeEditorResources.GenerateCrossTexture(settings.gridLineColor);
                return _crossTexture;
            }
        }
        private static Texture2D _crossTexture;

        /// <summary> TypeColors requested by the editor </summary>
        public static bool gridSnap { get { VerifyLoaded(); return settings.gridSnap; } }

        private static Dictionary<string, Color> typeColors = new Dictionary<string, Color>();
        private static Settings settings;

        [System.Serializable]
        private class Settings : ISerializationCallbackReceiver {
            public Color32 gridLineColor = new Color(0.45f, 0.45f, 0.45f);
            public Color32 gridBgColor = new Color(0.18f, 0.18f, 0.18f);
            public bool gridSnap = true;
            public string typeColorsData = "";
            public Dictionary<string, Color> typeColors = new Dictionary<string, Color>();

            public void OnAfterDeserialize() {
                // Deserialize typeColorsData
                typeColors = new Dictionary<string, Color>();
                string[] data = typeColorsData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < data.Length; i += 2) {
                    Color col;
                    if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col)) {
                        typeColors.Add(data[i], col);
                    }
                }
            }

            public void OnBeforeSerialize() {
                // Serialize typeColors
                typeColorsData = "";
                foreach (var item in typeColors) {
                    typeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
                }
            }
        }

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
            settings.gridSnap = EditorGUILayout.Toggle("Snap", settings.gridSnap);

            settings.gridLineColor = EditorGUILayout.ColorField("Color", settings.gridLineColor);
            settings.gridBgColor = EditorGUILayout.ColorField(" ", settings.gridBgColor);
            if (GUI.changed) {
                SavePrefs();
                _gridTexture = NodeEditorResources.GenerateGridTexture(settings.gridLineColor, settings.gridBgColor);
                _crossTexture = NodeEditorResources.GenerateCrossTexture(settings.gridLineColor);
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
                    if (settings.typeColors.ContainsKey(key)) settings.typeColors[key] = col;
                    else settings.typeColors.Add(key, col);
                    SavePrefs();
                    NodeEditorWindow.RepaintAll();
                }
            }
        }

        private static Settings LoadPrefs() {
            // Remove obsolete editorprefs
            if (EditorPrefs.HasKey("xnode_typecolors")) EditorPrefs.DeleteKey("xnode_typecolors");
            if (EditorPrefs.HasKey("xnode_gridcolor0")) EditorPrefs.DeleteKey("xnode_gridcolor0");
            if (EditorPrefs.HasKey("xnode_gridcolor1")) EditorPrefs.DeleteKey("xnode_gridcolor1");
            if (EditorPrefs.HasKey("xnode_gridsnap")) EditorPrefs.DeleteKey("xnode_gridcolor1");

            if (!EditorPrefs.HasKey("xNode.Settings")) EditorPrefs.SetString("xNode.Settings", JsonUtility.ToJson(new Settings()));
            return JsonUtility.FromJson<Settings>(EditorPrefs.GetString("xNode.Settings"));
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs() {
            if (EditorPrefs.HasKey("xNode.Settings")) EditorPrefs.DeleteKey("xNode.Settings");

            settings = LoadPrefs();
            typeColors = new Dictionary<string, Color>();
            _gridTexture = NodeEditorResources.GenerateGridTexture(settings.gridLineColor, settings.gridBgColor);
            _crossTexture = NodeEditorResources.GenerateCrossTexture(settings.gridLineColor);
            NodeEditorWindow.RepaintAll();
        }

        private static void SavePrefs() {
            EditorPrefs.SetString("xNode.Settings", JsonUtility.ToJson(settings));
        }

        private static void VerifyLoaded() {
            if (settings == null) settings = LoadPrefs();
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(System.Type type) {
            VerifyLoaded();
            if (type == null) return Color.gray;
            string typeName = type.PrettyName();
            if (!typeColors.ContainsKey(typeName)) {
                if (settings.typeColors.ContainsKey(typeName)) typeColors.Add(typeName, settings.typeColors[typeName]);
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