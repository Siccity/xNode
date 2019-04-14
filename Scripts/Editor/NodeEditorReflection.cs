using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    /// <summary> Contains reflection-related info </summary>
    public partial class NodeEditorWindow {
        /// <summary> Custom node tint colors defined with [NodeColor(r, g, b)] </summary>
        public static Dictionary<Type, Color> nodeTint { get { return _nodeTint != null ? _nodeTint : _nodeTint = GetNodeTint(); } }

        [NonSerialized] private static Dictionary<Type, Color> _nodeTint;
        /// <summary> Custom node widths defined with [NodeWidth(width)] </summary>
        public static Dictionary<Type, int> nodeWidth { get { return _nodeWidth != null ? _nodeWidth : _nodeWidth = GetNodeWidth(); } }

        [NonSerialized] private static Dictionary<Type, int> _nodeWidth;
        /// <summary> All available node types </summary>
        public static Type[] nodeTypes { get { return _nodeTypes != null ? _nodeTypes : _nodeTypes = GetNodeTypes(); } }

        [NonSerialized] private static Type[] _nodeTypes = null;

        private Func<bool> isDocked {
            get {
                if (_isDocked == null) {
                    BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                    MethodInfo isDockedMethod = typeof(NodeEditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
                    _isDocked = (Func<bool>) Delegate.CreateDelegate(typeof(Func<bool>), this, isDockedMethod);
                }
                return _isDocked;
            }
        }
        private Func<bool> _isDocked;

        public static Type[] GetNodeTypes() {
            //Get all classes deriving from Node via reflection
            return GetDerivedTypes(typeof(XNode.Node));
        }

        public static Dictionary<Type, Color> GetNodeTint() {
            Dictionary<Type, Color> tints = new Dictionary<Type, Color>();
            for (int i = 0; i < nodeTypes.Length; i++) {
                var attribs = nodeTypes[i].GetCustomAttributes(typeof(XNode.Node.NodeTintAttribute), true);
                if (attribs == null || attribs.Length == 0) continue;
                XNode.Node.NodeTintAttribute attrib = attribs[0] as XNode.Node.NodeTintAttribute;
                tints.Add(nodeTypes[i], attrib.color);
            }
            return tints;
        }

        public static Dictionary<Type, int> GetNodeWidth() {
            Dictionary<Type, int> widths = new Dictionary<Type, int>();
            for (int i = 0; i < nodeTypes.Length; i++) {
                var attribs = nodeTypes[i].GetCustomAttributes(typeof(XNode.Node.NodeWidthAttribute), true);
                if (attribs == null || attribs.Length == 0) continue;
                XNode.Node.NodeWidthAttribute attrib = attribs[0] as XNode.Node.NodeWidthAttribute;
                widths.Add(nodeTypes[i], attrib.width);
            }
            return widths;
        }

        /// <summary> Get FieldInfo of a field, including those that are private and/or inherited </summary>
        public static FieldInfo GetFieldInfo(Type type, string fieldName) {
            // If we can't find field in the first run, it's probably a private field in a base class.
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // Search base classes for private fields only. Public fields are found above
            while (field == null && (type = type.BaseType) != typeof(XNode.Node)) field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field;
        }

        /// <summary> Get all classes deriving from baseType via reflection </summary>
        public static Type[] GetDerivedTypes(Type baseType) {
            List<System.Type> types = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                try {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                } catch(ReflectionTypeLoadException) {}
            }
            return types.ToArray();
        }

        public static void AddCustomContextMenuItems(GenericMenu contextMenu, object obj) {
            KeyValuePair<ContextMenu, System.Reflection.MethodInfo>[] items = GetContextMenuMethods(obj);
            if (items.Length != 0) {
                contextMenu.AddSeparator("");
                for (int i = 0; i < items.Length; i++) {
                    KeyValuePair<ContextMenu, System.Reflection.MethodInfo> kvp = items[i];
                    contextMenu.AddItem(new GUIContent(kvp.Key.menuItem), false, () => kvp.Value.Invoke(obj, null));
                }
            }
        }

        public static KeyValuePair<ContextMenu, MethodInfo>[] GetContextMenuMethods(object obj) {
            Type type = obj.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            List<KeyValuePair<ContextMenu, MethodInfo>> kvp = new List<KeyValuePair<ContextMenu, MethodInfo>>();
            for (int i = 0; i < methods.Length; i++) {
                ContextMenu[] attribs = methods[i].GetCustomAttributes(typeof(ContextMenu), true).Select(x => x as ContextMenu).ToArray();
                if (attribs == null || attribs.Length == 0) continue;
                if (methods[i].GetParameters().Length != 0) {
                    Debug.LogWarning("Method " + methods[i].DeclaringType.Name + "." + methods[i].Name + " has parameters and cannot be used for context menu commands.");
                    continue;
                }
                if (methods[i].IsStatic) {
                    Debug.LogWarning("Method " + methods[i].DeclaringType.Name + "." + methods[i].Name + " is static and cannot be used for context menu commands.");
                    continue;
                }

                for (int k = 0; k < attribs.Length; k++) {
                    kvp.Add(new KeyValuePair<ContextMenu, MethodInfo>(attribs[k], methods[i]));
                }
            }
#if UNITY_5_5_OR_NEWER
            //Sort menu items
            kvp.Sort((x, y) => x.Key.priority.CompareTo(y.Key.priority));
#endif
            return kvp.ToArray();
        }

        /// <summary> Very crude. Uses a lot of reflection. </summary>
        public static void OpenPreferences() {
            try {
#if UNITY_2018_3_OR_NEWER
                SettingsService.OpenUserPreferences("Preferences/Node Editor");
#else
                //Open preferences window
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
                Type type = assembly.GetType("UnityEditor.PreferencesWindow");
                type.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

                //Get the window
                EditorWindow window = EditorWindow.GetWindow(type);

                //Make sure custom sections are added (because waiting for it to happen automatically is too slow)
                FieldInfo refreshField = type.GetField("m_RefreshCustomPreferences", BindingFlags.NonPublic | BindingFlags.Instance);
                if ((bool) refreshField.GetValue(window)) {
                    type.GetMethod("AddCustomSections", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                    refreshField.SetValue(window, false);
                }

                //Get sections
                FieldInfo sectionsField = type.GetField("m_Sections", BindingFlags.Instance | BindingFlags.NonPublic);
                IList sections = sectionsField.GetValue(window) as IList;

                //Iterate through sections and check contents
                Type sectionType = sectionsField.FieldType.GetGenericArguments() [0];
                FieldInfo sectionContentField = sectionType.GetField("content", BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < sections.Count; i++) {
                    GUIContent sectionContent = sectionContentField.GetValue(sections[i]) as GUIContent;
                    if (sectionContent.text == "Node Editor") {
                        //Found contents - Set index
                        FieldInfo sectionIndexField = type.GetField("m_SelectedSectionIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                        sectionIndexField.SetValue(window, i);
                        return;
                    }
                }
#endif
            } catch (Exception e) {
                Debug.LogError(e);
                Debug.LogWarning("Unity has changed around internally. Can't open properties through reflection. Please contact xNode developer and supply unity version number.");
            }
        }
    }
}
