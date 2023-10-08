using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace XNodeEditor
{
    public enum NoodlePath
    {
        Curvy,
        Straight,
        Angled,
        ShaderLab
    }
    public enum NoodleStroke
    {
        Full,
        Dashed
    }

    public static class NodeEditorPreferences
    {
        /// <summary> The last editor we checked. This should be the one we modify </summary>
        private static NodeGraphEditor lastEditor;
        /// <summary> The last key we checked. This should be the one we modify </summary>
        private static string lastKey = "xNode.Settings";

        private static Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>();
        private static readonly Dictionary<string, Settings> settings = new Dictionary<string, Settings>();

        [Serializable]
        public class Settings : ISerializationCallbackReceiver
        {
            [SerializeField] private Color32 _gridLineColor = new Color(.23f, .23f, .23f);
            public Color32 gridLineColor
            {
                get => _gridLineColor;
                set
                {
                    _gridLineColor = value;
                    _gridTexture = null;
                    _crossTexture = null;
                }
            }

            [SerializeField] private Color32 _gridBgColor = new Color(.19f, .19f, .19f);
            public Color32 gridBgColor
            {
                get => _gridBgColor;
                set
                {
                    _gridBgColor = value;
                    _gridTexture = null;
                }
            }

            [Obsolete("Use maxZoom instead")]
            public float zoomOutLimit
            {
                get => maxZoom;
                set => maxZoom = value;
            }

            [FormerlySerializedAs("zoomOutLimit")]
            public float maxZoom = 5f;
            public float minZoom = 1f;
            public Color32 tintColor = new Color32(90, 97, 105, 255);
            public Color32 bgHeaderColor = new Color32(50, 51, 54, 255);
            public Color32 bgPortsColor = new Color32(65, 67, 70, 255);
            public Color32 bgBodyColor = new Color32(65, 67, 70, 255);
            public Color32 highlightColor = new Color32(255, 255, 255, 255);
            public bool gridSnap = true;
            public bool autoSave = true;
            public bool openOnCreate = true;
            public bool dragToCreate = true;
            public bool createFilter = true;
            public bool zoomToMouse = true;
            public bool portTooltips = true;
            [SerializeField] private string typeColorsData = "";
            [NonSerialized] public Dictionary<string, Color> typeColors = new Dictionary<string, Color>();
            [FormerlySerializedAs("noodleType")] public NoodlePath noodlePath = NoodlePath.Curvy;
            public float noodleThickness = 2f;

            public NoodleStroke noodleStroke = NoodleStroke.Full;

            private Texture2D _gridTexture;
            public Texture2D gridTexture
            {
                get
                {
                    if (_gridTexture == null)
                    {
                        _gridTexture = NodeEditorResources.GenerateGridTexture(gridLineColor, gridBgColor);
                    }

                    return _gridTexture;
                }
            }
            private Texture2D _crossTexture;
            public Texture2D crossTexture
            {
                get
                {
                    if (_crossTexture == null)
                    {
                        _crossTexture = NodeEditorResources.GenerateCrossTexture(gridLineColor);
                    }

                    return _crossTexture;
                }
            }

            public void OnAfterDeserialize()
            {
                // Deserialize typeColorsData
                typeColors = new Dictionary<string, Color>();
                string[] data = typeColorsData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < data.Length; i += 2)
                {
                    Color col;
                    if (ColorUtility.TryParseHtmlString("#" + data[i + 1], out col))
                    {
                        typeColors.Add(data[i], col);
                    }
                }
            }

            public void OnBeforeSerialize()
            {
                // Serialize typeColors
                typeColorsData = "";
                foreach (var item in typeColors)
                {
                    typeColorsData += item.Key + "," + ColorUtility.ToHtmlStringRGB(item.Value) + ",";
                }
            }
        }

        /// <summary> Get settings of current active editor </summary>
        public static Settings GetSettings()
        {
            if (NodeEditorWindow.current == null)
            {
                return new Settings();
            }

            if (lastEditor != NodeEditorWindow.current.graphEditor)
            {
                object[] attribs = NodeEditorWindow.current.graphEditor.GetType()
                    .GetCustomAttributes(typeof(NodeGraphEditor.CustomNodeGraphEditorAttribute), true);
                if (attribs.Length == 1)
                {
                    NodeGraphEditor.CustomNodeGraphEditorAttribute attrib =
                        attribs[0] as NodeGraphEditor.CustomNodeGraphEditorAttribute;
                    lastEditor = NodeEditorWindow.current.graphEditor;
                    lastKey = attrib.editorPrefsKey;
                }
                else
                {
                    return null;
                }
            }

            if (!settings.ContainsKey(lastKey))
            {
                VerifyLoaded();
            }

            return settings[lastKey];
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateXNodeSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/Node Editor", SettingsScope.User)
            {
                guiHandler = searchContext => { PreferencesGUI(); },
                keywords = new HashSet<string>(new[]
                    { "xNode", "node", "editor", "graph", "connections", "noodles", "ports" })
            };
            return provider;
        }
#endif

#if !UNITY_2019_1_OR_NEWER
        [PreferenceItem("Node Editor")]
#endif
        private static void PreferencesGUI()
        {
            VerifyLoaded();
            Settings settings = NodeEditorPreferences.settings[lastKey];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button(new GUIContent("Documentation", "https://github.com/Siccity/xNode/wiki"),
                    GUILayout.Width(100)))
            {
                Application.OpenURL("https://github.com/Siccity/xNode/wiki");
            }

            EditorGUILayout.Space();

            NodeSettingsGUI(lastKey, settings);
            GridSettingsGUI(lastKey, settings);
            SystemSettingsGUI(lastKey, settings);
            TypeColorsGUI(lastKey, settings);
            if (GUILayout.Button(new GUIContent("Set Default", "Reset all values to default"), GUILayout.Width(120)))
            {
                ResetPrefs();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private static void GridSettingsGUI(string key, Settings settings)
        {
            //Label
            // EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            Separator("Grid Appearance");
            settings.gridSnap = EditorGUILayout.Toggle(new GUIContent("Snap", "Hold CTRL in editor to invert"),
                settings.gridSnap);
            settings.zoomToMouse =
                EditorGUILayout.Toggle(new GUIContent("Zoom to Mouse", "Zooms towards mouse position"),
                    settings.zoomToMouse);
            EditorGUILayout.LabelField("Zoom");
            EditorGUI.indentLevel++;
            settings.maxZoom =
                EditorGUILayout.FloatField(new GUIContent("Max", "Upper limit to zoom"), settings.maxZoom);
            settings.minZoom =
                EditorGUILayout.FloatField(new GUIContent("Min", "Lower limit to zoom"), settings.minZoom);
            EditorGUI.indentLevel--;
            settings.gridLineColor = EditorGUILayout.ColorField("Line Color", settings.gridLineColor);
            settings.gridBgColor = EditorGUILayout.ColorField("Background Color", settings.gridBgColor);
            if (GUI.changed)
            {
                SavePrefs(key, settings);

                NodeEditorWindow.RepaintAll();
            }

            EditorGUILayout.Space();
        }

        private static void SystemSettingsGUI(string key, Settings settings)
        {
            //Label
            // EditorGUILayout.LabelField("System", EditorStyles.boldLabel);
            Separator("System");
            settings.autoSave =
                EditorGUILayout.Toggle(new GUIContent("Autosave", "Disable for better editor performance"),
                    settings.autoSave);
            settings.openOnCreate =
                EditorGUILayout.Toggle(
                    new GUIContent("Open Editor on Create",
                        "Disable to prevent openening the editor when creating a new graph"), settings.openOnCreate);
            if (GUI.changed)
            {
                SavePrefs(key, settings);
            }

            EditorGUILayout.Space();
        }

        private static void NodeSettingsGUI(string key, Settings settings)
        {
            //Label
            // EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);
            Separator("Node Appearance");
            settings.tintColor = EditorGUILayout.ColorField("Global Tint", settings.tintColor);
            settings.bgHeaderColor = EditorGUILayout.ColorField("Header Background", settings.bgHeaderColor);
            settings.bgPortsColor = EditorGUILayout.ColorField("Ports Background", settings.bgPortsColor);
            settings.bgBodyColor = EditorGUILayout.ColorField("Body Background", settings.bgBodyColor);
            settings.highlightColor = EditorGUILayout.ColorField("Selection", settings.highlightColor);
            EditorGUILayout.Space();
            settings.noodlePath = (NoodlePath)EditorGUILayout.EnumPopup("Noodle path", settings.noodlePath);
            settings.noodleThickness = EditorGUILayout.FloatField(
                new GUIContent("Noodle thickness", "Noodle Thickness of the node connections"),
                settings.noodleThickness);
            settings.noodleStroke = (NoodleStroke)EditorGUILayout.EnumPopup("Noodle stroke", settings.noodleStroke);
            EditorGUILayout.Space();
            settings.portTooltips = EditorGUILayout.Toggle("Port Tooltips", settings.portTooltips);
            settings.dragToCreate =
                EditorGUILayout.Toggle(
                    new GUIContent("Drag to Create",
                        "Drag a port connection anywhere on the grid to create and connect a node"),
                    settings.dragToCreate);
            settings.createFilter =
                EditorGUILayout.Toggle(
                    new GUIContent("Create Filter", "Only show nodes that are compatible with the selected port"),
                    settings.createFilter);

            //END
            if (GUI.changed)
            {
                SavePrefs(key, settings);
                NodeEditorWindow.RepaintAll();
            }

            EditorGUILayout.Space();
        }

        private static void TypeColorsGUI(string key, Settings settings)
        {
            //Label
            // EditorGUILayout.LabelField("Types", EditorStyles.boldLabel);
            Separator("Types");

            //Clone keys so we can enumerate the dictionary and make changes.
            var typeColorKeys = new List<Type>(typeColors.Keys);

            //Display type colors. Save them if they are edited by the user
            foreach (Type type in typeColorKeys)
            {
                string typeColorKey = type.PrettyName();
                Color col = typeColors[type];
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                col = EditorGUILayout.ColorField(typeColorKey, col);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    typeColors[type] = col;
                    if (settings.typeColors.ContainsKey(typeColorKey))
                    {
                        settings.typeColors[typeColorKey] = col;
                    }
                    else
                    {
                        settings.typeColors.Add(typeColorKey, col);
                    }

                    SavePrefs(key, settings);
                    NodeEditorWindow.RepaintAll();
                }
            }
        }

        private static void Separator(string label = "")
        {
            Rect labelRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
            labelRect.y += 10;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            Vector2 textSize = labelStyle.CalcSize(new GUIContent(label));
            float separatorWidth = (labelRect.width - textSize.x) / 2 - 5;

            // Needed here otherwise BeginHorizontal group has 0 height.
            GUILayout.Label("");
            Color initialColor = GUI.color;
            Color lineColor = new Color(0.5f, 0.5f, 0.5f);
            GUI.color = lineColor;
            GUI.Box(new Rect(labelRect.xMin + 5, labelRect.yMin, separatorWidth - 5, 1), string.Empty);

            GUI.color = initialColor;
            GUI.Label(new Rect(labelRect.xMin + separatorWidth + 5, labelRect.yMin - 10, textSize.x, 20), label,
                labelStyle);

            GUI.color = lineColor;
            GUI.Box(new Rect(labelRect.xMin + separatorWidth + 10 + textSize.x, labelRect.yMin, separatorWidth - 5, 1),
                string.Empty);

            GUI.color = initialColor;
            EditorGUILayout.EndHorizontal();
        }

        /// <summary> Load prefs if they exist. Create if they don't </summary>
        private static Settings LoadPrefs()
        {
            // Create settings if it doesn't exist
            if (!EditorPrefs.HasKey(lastKey))
            {
                if (lastEditor != null)
                {
                    EditorPrefs.SetString(lastKey, JsonUtility.ToJson(lastEditor.GetDefaultPreferences()));
                }
                else
                {
                    EditorPrefs.SetString(lastKey, JsonUtility.ToJson(new Settings()));
                }
            }

            return JsonUtility.FromJson<Settings>(EditorPrefs.GetString(lastKey));
        }

        /// <summary> Delete all prefs </summary>
        public static void ResetPrefs()
        {
            if (EditorPrefs.HasKey(lastKey))
            {
                EditorPrefs.DeleteKey(lastKey);
            }

            if (settings.ContainsKey(lastKey))
            {
                settings.Remove(lastKey);
            }

            typeColors = new Dictionary<Type, Color>();
            VerifyLoaded();
            NodeEditorWindow.RepaintAll();
        }

        /// <summary> Save preferences in EditorPrefs </summary>
        private static void SavePrefs(string key, Settings settings)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(settings));
        }

        /// <summary> Check if we have loaded settings for given key. If not, load them </summary>
        private static void VerifyLoaded()
        {
            if (!settings.ContainsKey(lastKey))
            {
                settings.Add(lastKey, LoadPrefs());
            }
        }

        /// <summary> Return color based on type </summary>
        public static Color GetTypeColor(Type type)
        {
            VerifyLoaded();
            if (type == null)
            {
                return Color.gray;
            }

            Color col;
            if (!typeColors.TryGetValue(type, out col))
            {
                string typeName = type.PrettyName();
                if (settings[lastKey].typeColors.ContainsKey(typeName))
                {
                    typeColors.Add(type, settings[lastKey].typeColors[typeName]);
                }
                else
                {
#if UNITY_5_4_OR_NEWER
                    Random.State oldState = Random.state;
                    Random.InitState(typeName.GetHashCode());
#else
                    int oldSeed = UnityEngine.Random.seed;
                    UnityEngine.Random.seed = typeName.GetHashCode();
#endif
                    col = new Color(Random.value, Random.value, Random.value);
                    typeColors.Add(type, col);
#if UNITY_5_4_OR_NEWER
                    Random.state = oldState;
#else
                    UnityEngine.Random.seed = oldSeed;
#endif
                }
            }

            return col;
        }
    }
}